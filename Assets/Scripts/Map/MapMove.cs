using Combat.Stage;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private void Start()
        {
            InitShowBattles();
        }

        void Update()
        {
            CharacterMove();
            ShowStage();
            ShowBattle(StagePosition);
            //Clear();
        }

        void CharacterMove()
        {
            // 튜토리얼 대사를 넘기는 키가 스테이지 이동·입장으로도 먹히면 안 된다.
            if (InteractionLock.IsLocked)
            {
                return;
            }

            if (position == 0)
            {
                if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow)) && StagePosition >= 1)
                {
                    character.transform.DOMove(battle1.transform.position, 1);
                    position++;
                }
            }
            else if (position == 1)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && StagePosition >= 2)
                {
                    character.transform.DOMove(battle2.transform.position, 1);
                    position++;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    character.transform.DOMove(village.transform.position, 1);
                    position--;
                }
            }
            else if (position == 2)
            {
                if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow)) && StagePosition >= 3)
                {
                    character.transform.DOMove(battle3.transform.position, 1);
                    position++;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    character.transform.DOMove(battle1.transform.position, 1);
                    position--;
                }
            }
            else if (position == 3)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && StagePosition >= 4)
                {
                    character.transform.DOMove(boss.transform.position, 1);
                    position++;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    character.transform.DOMove(battle2.transform.position, 1);
                    position--;
                }
            }
            else if (position == 4 || position == 5)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    character.transform.DOMove(battle3.transform.position, 1);
                    position--;
                }
            }

            // 스테이지 선택 시 씬 로드
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SelectStage(position);
            }
        }

        void ShowStage()
        {
            stage.text = "Stage : " + StagePosition;
        }

        public void SelectStage(int stagePosition)
        {
            StageDataSingleton.Instance.stagePosition = stagePosition;
            SceneManager.LoadScene("TurnBattleScene");
        }

        void InitShowBattles()
        {
            for (int i = 0; i < StagePosition; i++)
            {
                ShowBattle(i);
            }
            WordPosition(StagePosition);
        }

        void ShowBattle(int stagePosition)
        {
            switch (stagePosition)
            {
                case 1:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = stage1;
                    break;
                case 2:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = stage2;
                    break;
                case 3:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = stage3;
                    break;
                case 4:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = stage4;
                    break;
                case 5:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = stage4;
                    break;
                default:
                    break;
            }
        }

        void WordPosition(int stagePosition)
        {
            position = stagePosition;
            switch (stagePosition)
            {
                case 0:
                    break;
                case 1:
                    character.transform.position = battle1.transform.position;
                    break;
                case 2:
                    character.transform.position = battle2.transform.position;
                    break;
                case 3:
                    character.transform.position = battle3.transform.position;
                    break;
                case 4:
                    character.transform.position = boss.transform.position;
                    break;
                case 5:
                    character.transform.position = boss.transform.position;
                    break;
                default:
                    break;
            }
        }

        void Clear()
        {
            if (Input.GetKeyDown(KeyCode.C) && StagePosition <= 4)
            {
                StagePosition++;
            }
        }
    }
}
