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

        // "아무 키나 입력하세요" 안내. 연결하지 않은 씬에서는 쓰지 않는다.
        [SerializeField] GameObject anyKeyPrompt;

        const int ActOneFinalStageID = 4;
        const int ActTwoFinalStageID = 9;

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
            // 튜토리얼 대사가 떠 있는 동안에는 대사 쪽 안내와 겹치지 않게 숨긴다.
            if (anyKeyPrompt != null)
            {
                anyKeyPrompt.SetActive(!InteractionLock.IsLocked);
            }

            // 튜토리얼 대사를 넘기는 키가 클리어 화면 진행으로도 먹히면 안 된다.
            if (InteractionLock.IsLocked || talking || leaving)
            {
                return;
            }

            if (sceneName == "GameClearScene")
            {
                if (Input.anyKeyDown)
                {
                    // 새 카드를 주지 않는 스테이지(2부 등)에서는 "New Card" 안내 없이 바로 맵으로 간다.
                    if (!flag && !HasCardReward())
                    {
                        leaving = true;
                        SceneManager.LoadScene("MapScene");
                    }
                    else if (!flag)
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
            // 장의 마지막 지역(1부 마왕, 2부 세계수)을 끝내면 엔딩으로 넘어가므로 대화창을 닫지 않고 그대로 덮어 둔다.
            bool endsChapter = stageID == ActOneFinalStageID || stageID == ActTwoFinalStageID;
            if (chapter != null) yield return StageDialogueView.Play(data, chapter, !endsChapter);
            if (endsChapter)
            {
                leaving = true;
                SceneManager.LoadScene("EndingScene");
                yield break;
            }
            talking = false;
        }

        // 새 카드는 1부 스테이지 0~3을 처음 클리어했을 때만 준다. ShowGettedCard의 switch와 같은 조건이다.
        bool HasCardReward()
        {
            int stageID = StageDataSingleton.Instance.stagePosition;
            return MapMove.StagePosition - 1 == stageID && stageID >= 0 && stageID <= 3;
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
