using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Map_scene
{
    public class MapScene : MonoBehaviour
    {
        [SerializeField] GameObject character;
        [SerializeField] GameObject Background;
        [SerializeField] GameObject village;
        [SerializeField] GameObject battle1;
        [SerializeField] GameObject battle2;
        [SerializeField] GameObject battle3;
        [SerializeField] GameObject boss;
        [SerializeField] TextMeshProUGUI Stage;
        [SerializeField] Sprite Stage1;
        [SerializeField] Sprite Stage2;
        [SerializeField] Sprite Stage3;
        [SerializeField] Sprite Stage4;
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
            Stage.text = "Stage : " + StagePosition;
        }

        public void SelectStage(int stagePosition)
        {
            StageDataSingleton.Instance.StagePosition = stagePosition;
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

        void ShowBattle(int StagePosition)
        {
            switch (StagePosition)
            {
                case 1:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = Stage1;
                    break;
                case 2:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = Stage2;
                    break;
                case 3:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = Stage3;
                    break;
                case 4:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = Stage4;
                    break;
                case 5:
                    GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite = Stage4;
                    break;
                default:
                    break;
            }
        }

        void WordPosition(int StagePosition)
        {
            position = StagePosition;
            switch (StagePosition)
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
