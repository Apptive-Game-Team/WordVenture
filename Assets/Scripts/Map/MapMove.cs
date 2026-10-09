using System.Collections;
using Combat.Stage;
using Core;
using DG.Tweening;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
        // 1부를 끝내면(StagePosition 5 이상) 이 장의 배경과 지점으로 바꿔 보여 준다.
        [SerializeField] MapChapter actTwoMap;
        // 1부를 끝낸 뒤에만 보이는, 1부 맵과 2부 맵을 오가는 버튼. Tab 키로도 바꾼다.
        [SerializeField] Button chapterSwitchButton;
        [SerializeField] TextMeshProUGUI chapterSwitchLabel;
        const int ActOneLastStageID = 4;
        // 2부 맵에서 1부 맵으로 돌아올 때 지점을 되돌려 놓을 씬 원래 위치.
        Vector3[] actOneStageLocations;
        // 지금 맵에 보이는 장. 1부면 null이다.
        MapChapter chapter;
        // 지점 번호 position에 더하면 스테이지 번호가 된다. 1부는 0, 2부는 5다.
        int firstStageID;
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
            actOneStageLocations = System.Array.ConvertAll(stageLocations, location => location.transform.position);
            if (CanSwitchChapter && !SaveLoadController.ShowsActOneMap) UseChapter(actTwoMap);
        }

        bool CanSwitchChapter => actTwoMap != null && StagePosition >= actTwoMap.firstStageID;

        // 지점 오브젝트는 스프라이트 없이 위치만 나타내므로, 장의 좌표로 옮겨서 그대로 쓴다.
        // mapChapter가 null이면 1부 맵으로 돌아간다.
        void UseChapter(MapChapter mapChapter)
        {
            chapter = mapChapter;
            if (mapChapter == null)
            {
                firstStageID = 0;
                for (int i = 0; i < stageLocations.Length; i++)
                    stageLocations[i].transform.position = actOneStageLocations[i];
                return;
            }

            firstStageID = mapChapter.firstStageID;
            for (int i = 0; i < stageLocations.Length && i < mapChapter.stagePoints.Length; i++)
            {
                Vector3 point = stageLocations[i].transform.position;
                point.x = mapChapter.stagePoints[i].x;
                point.y = mapChapter.stagePoints[i].y;
                stageLocations[i].transform.position = point;
            }
        }

        int LastPlayableStageID => chapter != null ? chapter.lastPlayableStageID : ActOneLastStageID;

        private void Start()
        {
            InitShowBattles();
            InitChapterSwitchButton();
        }

        void InitChapterSwitchButton()
        {
            if (chapterSwitchButton == null) return;
            chapterSwitchButton.gameObject.SetActive(CanSwitchChapter);
            chapterSwitchButton.onClick.AddListener(SwitchChapter);
            ShowChapterSwitchLabel();
        }

        void ShowChapterSwitchLabel()
        {
            if (chapterSwitchLabel == null) return;
            chapterSwitchLabel.text = chapter != null ? "1부 맵으로 (Tab)" : "2부 맵으로 (Tab)";
        }

        void SwitchChapter()
        {
            if (!CanSwitchChapter || InteractionLock.IsLocked || enteringStage) return;

            movement?.Kill();
            UseChapter(chapter != null ? null : actTwoMap);
            SaveLoadController.ShowsActOneMap = chapter == null;
            WordPosition(StagePosition);
            // 배경과 스테이지 문구는 지금 장을 기준으로 그리므로 다음 프레임에 다시 그린다.
            renderedStagePosition = -1;
            ShowChapterSwitchLabel();
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

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchChapter();
                return;
            }

            // 1부 길은 위아래로 꺾이므로 지점마다 위아래 키의 방향이 다르다. 2부 길은 순서대로 이어 간다.
            bool next = chapter != null ? right || up : right || (up && (position == 0 || position == 2));
            bool previous = chapter != null ? left || down : left || (down && (position == 1 || position == 3));
            if (next)
            {
                MoveToStage(position + 1);
            }
            else if (previous)
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

        // target은 지금 장 안의 지점 번호다. 전투 데이터가 아직 없는 지점은 보이기만 하고 들어갈 수 없다.
        bool IsStageUnlocked(int target)
        {
            int stageID = firstStageID + target;
            return target >= 0 && target < stageLocations.Length && stageID <= StagePosition
                && stageID <= LastPlayableStageID;
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
            stage.text = StagePosition > LastPlayableStageID && StagePosition <= LastStageIDOnMap
                ? "Stage : " + StagePosition + " (준비 중)"
                : "Stage : " + StagePosition;
        }

        int LastStageIDOnMap => firstStageID + stageLocations.Length - 1;

        // locationIndex는 지금 장 안의 지점 번호다.
        public void SelectStage(int locationIndex)
        {
            if (InteractionLock.IsLocked || enteringStage || !IsStageUnlocked(locationIndex) ||
                locationIndex != position ||
                (movement != null && movement.IsActive() && movement.IsPlaying()))
            {
                return;
            }

            enteringStage = true;
            int stageID = firstStageID + locationIndex;
            StageDataSingleton.Instance.stagePosition = stageID;
            StageDialogueChapter dialogue = StageDialogueView.FindUnseen(stageID,
                StageDialogueMoment.Enter, 0, out StageDialogueData data);
            if (dialogue != null) StartCoroutine(EnterAfterDialogue(data, dialogue));
            else SceneManager.LoadScene("TurnBattleScene");
        }

        IEnumerator EnterAfterDialogue(StageDialogueData data, StageDialogueChapter dialogue)
        {
            yield return StageDialogueView.Play(data, dialogue, false);
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
            if (chapter != null)
            {
                backgroundRenderer.sprite = chapter.background;
                return;
            }

            // 2부를 진행 중에 1부 맵을 열면 1부를 끝낸 배경(stage4)을 보여 준다.
            switch (Mathf.Min(stagePosition, ActOneLastStageID + 1))
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
            position = Mathf.Clamp(Mathf.Min(stagePosition, LastPlayableStageID) - firstStageID, 0, stageLocations.Length - 1);
            character.transform.position = stageLocations[position].transform.position;
        }
#if UNITY_EDITOR
        // 테스트용: 에디터에서 C 키로 다음 스테이지를 연다. 지금 장의 마지막 지점까지만 연다.
        void UnlockNextStageForTest()
        {
            if (InteractionLock.IsLocked || enteringStage || !Input.GetKeyDown(KeyCode.C)) return;
            if (StagePosition < LastStageIDOnMap) StagePosition++;
        }
#endif
    }
}
