using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Map_scene
{
    public class MapMove : MonoBehaviour
    {
        [SerializeField] GameObject character;
        [SerializeField] GameObject village;
        [SerializeField] GameObject battle1;
        [SerializeField] GameObject battle2;
        [SerializeField] GameObject battle3;
        [SerializeField] GameObject boss;
        [SerializeField] Text Stage;
        int position = 0;
        int StagePosition = 0;

        void Update()
        {
            CharacterMove();
            ShowStage();
            ShowBattle();
            Clear();
        }

        void CharacterMove()
        {
            if (position == 0)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && StagePosition >= 1)
                {
                    character.transform.DOMove(battle1.transform.position,1);
                    position++;
                }
            }

            else if(position == 1)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && StagePosition >= 2)
                {
                    character.transform.DOMove(battle2.transform.position,1);
                    position++;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    character.transform.DOMove(village.transform.position,1);
                    position--;
                }
            }

            else if(position == 2)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && StagePosition >= 3)
                {
                    character.transform.DOMove(battle3.transform.position,1);
                    position++;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    character.transform.DOMove(battle1.transform.position,1);
                    position--;
                }
            }

            else if(position == 3)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && StagePosition >= 4)
                {
                    character.transform.DOMove(boss.transform.position,1);
                    position++;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    character.transform.DOMove(battle2.transform.position,1);
                    position--;
                }
            }

            else if(position == 4)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    character.transform.DOMove(battle3.transform.position,1);
                    position--;
                }
            }
        }

        void ShowStage()
        {
            Stage.text = "Stage : " + position;
        }

        void ShowBattle()
        {
            switch(StagePosition)
            {
                case 1:
                    battle1.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    break;
                case 2:
                    battle2.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    Color color1 = battle1.GetComponent<SpriteRenderer>().color;
                    color1.a = 0.3f;
                    battle1.GetComponent<SpriteRenderer>().color = color1;
                    break;
                case 3:
                    battle3.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    Color color2 = battle2.GetComponent<SpriteRenderer>().color;
                    color2.a = 0.3f;
                    battle2.GetComponent<SpriteRenderer>().color = color2;
                    break;
                case 4:
                    boss.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    Color color3 = battle3.GetComponent<SpriteRenderer>().color;
                    color3.a = 0.3f;
                    battle3.GetComponent<SpriteRenderer>().color = color3;
                    break;
                case 5:
                    Color color4 = boss.GetComponent<SpriteRenderer>().color;
                    color4.a = 0.3f;
                    boss.GetComponent<SpriteRenderer>().color = color4;
                    break;
                default:
                    break;
            }
        }

        void Clear()
        {
            if(Input.GetKeyDown(KeyCode.Space) && StagePosition <= 5)
            {
                StagePosition++;
            }
        }

    }
}

