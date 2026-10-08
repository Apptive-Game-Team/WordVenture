using Cards;
using System.Collections;
using Combat.Stage;
using Core;
using Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Story;
using Tutorial;

namespace Scenes
{
    public class GameClearController : MonoBehaviour
    {
        [FormerlySerializedAs("wordSO")] [SerializeField] WordScriptableObject wordSo;
        [SerializeField] GameObject spellCard;

        [SerializeField] GameObject magicCard;

        [FormerlySerializedAs("TEXT")] [SerializeField] GameObject text;

        bool flag = false;

        private string sceneName;
        bool leaving;
        bool talking;

        private void Start()
        {
            sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "GameClearScene")
            {
                text.SetActive(false);
                // 지역 대화를 먼저 보여 주고, 그다음 입력으로 새 카드를 보여 준다.
                talking = true;
                StartCoroutine(ShowClearDialogue());
            }
        }

        void Update()
        {
            // 튜토리얼 대사를 넘기는 키가 클리어 화면 진행으로도 먹히면 안 된다.
            if (InteractionLock.IsLocked || talking || leaving)
            {
                return;
            }

            if (sceneName == "GameClearScene")
            {
                if (Input.anyKeyDown)
                {
                    if (!flag)
                    {
                        text.SetActive(true);
                        ShowGettedCard();
                        flag = true;
                    }
                    else
                    {
                        leaving = true;
                        SceneManager.LoadScene("MapScene");
                    }

                }
            } else
            {
                if (Input.anyKeyDown)
                {

                    SceneManager.LoadScene("MapScene");

                }
            }


        }

        IEnumerator ShowClearDialogue()
        {
            int stageID = StageDataSingleton.Instance.stagePosition;
            StageDialogueChapter chapter = StageDialogueView.FindUnseen(stageID,
                StageDialogueMoment.Clear, 0, out StageDialogueData data);
            // 첫 클리어에서는 할아버지의 작별 인사와 새 카드 안내가 모두 끝난 뒤에 대화한다.
            // 튜토리얼 대사 사이에도 입력 잠금이 잠깐 풀리므로 잠금 대신 튜토리얼 종료를 기다린다.
            while (TutorialController.Instance != null && !SaveLoadController.IsTutorialEnded) yield return null;
            // 마왕 처치 후에는 엔딩으로 넘어가므로 대화창을 닫지 않고 그대로 덮어 둔다.
            if (chapter != null) yield return StageDialogueView.Play(data, chapter, stageID != 4);
            if (stageID == 4)
            {
                leaving = true;
                SceneManager.LoadScene("EndingScene");
                yield break;
            }
            talking = false;
        }

        void ShowGettedCard()
        {
            if (MapMove.StagePosition - 1 == StageDataSingleton.Instance.stagePosition)
            {
                switch (StageDataSingleton.Instance.stagePosition)
                {
                    case 0: // 2
                        GameObject card1 = Instantiate(spellCard, new Vector3(0, 0, 0), Quaternion.identity);
                        card1.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[2].name);
                        card1.GetComponent<Order>().SetOrder(0);
                        break;
                    case 1: // rock 5
                        GameObject card4 = Instantiate(magicCard, new Vector3(0, 0, 0), Quaternion.identity);
                        card4.GetComponent<Order>().SetOrder(0);
                        card4.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[5].name);
                        break;
                    case 2: // 4 7
                        GameObject card5 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                        card5.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[4].name);
                        card5.GetComponent<Order>().SetOrder(0);
                        GameObject card6 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                        card6.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[7].name);
                        card6.GetComponent<Order>().SetOrder(0);
                        break;

                    case 3: // 1 6
                        GameObject card7 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                        card7.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[1].name);
                        card7.GetComponent<Order>().SetOrder(0);
                        GameObject card8 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                        card8.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[6].name);
                        card8.GetComponent<Order>().SetOrder(0);
                        break;
                }
            }

        }
    }

}
