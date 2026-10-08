using System.Collections;
using Combat.Stage;
using Core;
using DG.Tweening;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Map
{
    public class MapMove : MonoBehaviour
    {
        [SerializeField] GameObject character;
        [FormerlySerializedAs("Background")] [SerializeField] GameObject background;
        [SerializeField] GameObject village;
        [SerializeField] GameObject battle1;
        [SerializeField] GameObject battle2;
        [SerializeField] GameObject battle3;
        [SerializeField] GameObject boss;
        [FormerlySerializedAs("Stage")] [SerializeField] TextMeshProUGUI stage;
        [FormerlySerializedAs("Stage1")] [SerializeField] Sprite stage1;
        [FormerlySerializedAs("Stage2")] [SerializeField] Sprite stage2;
        [FormerlySerializedAs("Stage3")] [SerializeField] Sprite stage3;
        [FormerlySerializedAs("Stage4")] [SerializeField] Sprite stage4;
        int position = 0;
        public static int StagePosition;

        [SerializeField] float stageClickRadius = 0.6f;
        GameObject[] stageLocations;
        Camera mapCamera;
        Tween movement;
        bool enteringStage;

        SpriteRenderer backgroundRenderer;

        // 마지막으로 화면에 반영한 StagePosition. 아직 아무것도 그리지 않은 상태를
        // 뜻하는 값으로 시작해야 첫 프레임에 한 번은 반드시 갱신된다.
        int renderedStagePosition = -1;

        private void Awake()
        {
            backgroundRenderer = background.GetComponent<SpriteRenderer>();
            stageLocations = new[] { village, battle1, battle2, battle3, boss };
            mapCamera = Camera.main;
        }

        private void Start()
        {
            InitShowBattles();
        }

        void Update()
        {
            CharacterMove();
#if UNITY_EDITOR
            UnlockNextStageForTest();
#endif
            RefreshStageVisuals();
        }

        /// <summary>
        /// 스테이지 표시를 StagePosition이 바뀐 프레임에만 다시 그린다.
        /// 매 프레임 갱신하면 배경 스프라이트를 같은 값으로 덮어쓰고
        /// 스테이지 문자열을 새로 만드는 비용만 반복된다.
        /// </summary>
        void RefreshStageVisuals()
        {
            if (renderedStagePosition == StagePosition)
            {
                return;
            }

            renderedStagePosition = StagePosition;
            ShowStage();
            ShowBattle(StagePosition);
        }

        void CharacterMove()
        {
            if (InteractionLock.IsLocked || enteringStage)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0) &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                HandleStageClick();
                return;
            }

            if (movement != null && movement.IsActive() && movement.IsPlaying())
            {
                return;
            }

            bool right = Input.GetKeyDown(KeyCode.RightArrow);
            bool left = Input.GetKeyDown(KeyCode.LeftArrow);
            bool up = Input.GetKeyDown(KeyCode.UpArrow);
            bool down = Input.GetKeyDown(KeyCode.DownArrow);

            if (right || (up && (position == 0 || position == 2)))
            {
                MoveToStage(position + 1);
            }
            else if (left || (down && (position == 1 || position == 3)))
            {
                MoveToStage(position - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                SelectStage(position);
            }
        }

        void HandleStageClick()
        {
            if (mapCamera == null)
            {
                return;
            }

            // 맵의 지점은 별도 스프라이트 없이 배경 위의 위치로 정의되어 있다.
            Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            Plane mapPlane = new Plane(Vector3.forward, village.transform.position);
            if (!mapPlane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector3 point = ray.GetPoint(distance);
            int clickedStage = -1;
            float closestDistance = stageClickRadius * stageClickRadius;
            for (int i = 0; i < stageLocations.Length; i++)
            {
                float squaredDistance = ((Vector2)(point - stageLocations[i].transform.position)).sqrMagnitude;
                if (squaredDistance <= closestDistance)
                {
                    clickedStage = i;
                    closestDistance = squaredDistance;
                }
            }

            if (!IsStageUnlocked(clickedStage))
            {
                return;
            }

            if (clickedStage == position)
            {
                SelectStage(clickedStage);
            }
            else
            {
                MoveToStage(clickedStage);
            }
        }

        bool IsStageUnlocked(int target)
        {
            return target >= 0 && target < stageLocations.Length && target <= StagePosition;
        }

        void MoveToStage(int target)
        {
            if (InteractionLock.IsLocked || enteringStage || !IsStageUnlocked(target) || target == position)
            {
                return;
            }

            movement?.Kill();
            position = target;
            movement = character.transform.DOMove(stageLocations[target].transform.position, 1);
        }

        void OnDestroy()
        {
            movement?.Kill();
        }
        void ShowStage()
        {
            stage.text = "Stage : " + StagePosition;
        }

        public void SelectStage(int stagePosition)
        {
            if (InteractionLock.IsLocked || enteringStage || !IsStageUnlocked(stagePosition) ||
                stagePosition != position ||
                (movement != null && movement.IsActive() && movement.IsPlaying()))
            {
                return;
            }

            enteringStage = true;
            StageDataSingleton.Instance.stagePosition = stagePosition;
            StageDialogueChapter chapter = StageDialogueView.FindUnseen(stagePosition,
                StageDialogueMoment.Enter, 0, out StageDialogueData data);
            if (chapter != null) StartCoroutine(EnterAfterDialogue(data, chapter));
            else SceneManager.LoadScene("TurnBattleScene");
        }

        IEnumerator EnterAfterDialogue(StageDialogueData data, StageDialogueChapter chapter)
        {
            yield return StageDialogueView.Play(data, chapter, false);
            SceneManager.LoadScene("TurnBattleScene");
        }

        void InitShowBattles()
        {
            // 배경 스프라이트는 RefreshStageVisuals가 첫 프레임에 StagePosition 기준으로
            // 맞춘다. 여기서 0..StagePosition을 훑어도 마지막 값만 살아남고 바로 덮어써진다.
            WordPosition(StagePosition);
        }

        void ShowBattle(int stagePosition)
        {
            switch (stagePosition)
            {
                case 1:
                    backgroundRenderer.sprite = stage1;
                    break;
                case 2:
                    backgroundRenderer.sprite = stage2;
                    break;
                case 3:
                    backgroundRenderer.sprite = stage3;
                    break;
                case 4:
                    backgroundRenderer.sprite = stage4;
                    break;
                case 5:
                    backgroundRenderer.sprite = stage4;
                    break;
                default:
                    break;
            }
        }

        void WordPosition(int stagePosition)
        {
            position = Mathf.Clamp(stagePosition, 0, stageLocations.Length - 1);
            character.transform.position = stageLocations[position].transform.position;
        }
#if UNITY_EDITOR
        // 테스트용: 에디터에서 C 키로 다음 스테이지를 연다. 마왕 성(4)까지만 연다.
        void UnlockNextStageForTest()
        {
            if (InteractionLock.IsLocked || enteringStage || !Input.GetKeyDown(KeyCode.C)) return;
            if (StagePosition < stageLocations.Length - 1) StagePosition++;
        }
#endif
    }
}
