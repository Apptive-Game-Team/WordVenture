using Cards;
using System;
using System.Collections;
using Combat.Stage;
using Core;
using Map;
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
        const int SpawnRewardStageID = 5;
        const int ActTwoFinalStageID = 9;

        bool flag = false;

        private string sceneName;
        bool leaving;
        bool talking;
        StageResultView resultView;

        private void Start()
        {
            sceneName = SceneManager.GetActiveScene().name;
            PlayResultMusic(StageDataSingleton.Instance != null ? StageDataSingleton.Instance.stagePosition : 0);
            if (sceneName == "GameClearScene")
            {
                text.SetActive(false);
                // 지역 대화를 먼저 보여 주고, 그다음 입력으로 새 카드를 보여 준다.
                talking = true;
                StartCoroutine(ShowClearDialogue());
            }
        }

        void PlayResultMusic(int stageID)
        {
            // 읽음 여부와 무관하게 승리/패배 씬 전체에서 지역 곡을 재생한다.
            // 대화창이 닫혀도 결과와 새 카드 보상 화면까지 이 소스가 남는다.
            var data = Resources.Load<StageDialogueData>("Story/ActOneDialogues");
            StageDialogueChapter chapter = data != null ? data.FindChapter(stageID, StageDialogueMoment.Clear, 0) : null;
            if (chapter == null || chapter.music == null) return;
            var music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.clip = chapter.music;
            music.loop = true;
            music.spatialBlend = 0f;
            music.volume = 0.1f;
            music.Play();
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
                    // 결과 화면이 나오는 중이면 첫 키는 연출을 건너뛰고, 다 나온 뒤의 키는 결과 화면을 닫고 아래 진행으로 넘어간다.
                    if (resultView != null)
                    {
                        if (!resultView.IsRevealed)
                        {
                            resultView.Skip();
                            return;
                        }
                        Destroy(resultView.gameObject);
                        resultView = null;
                    }
                    // 새 카드를 주지 않는 스테이지(2부 스테이지 5 제외)에서는 "New Card" 안내 없이 바로 맵으로 간다.
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
            // 다시 깬 마지막 지역은 엔딩 없이 맵으로 돌아간다.
            bool endsChapter = IsFirstClear && (stageID == ActOneFinalStageID || stageID == ActTwoFinalStageID);
            // 엔딩으로 이어지는 대화창은 닫지 않으므로, 마지막 지역은 대화 앞에 결과 화면을 보여 준다.
            if (endsChapter)
            {
                if (HasResult) yield return GameTips.ShowOnce(GameTip.Result);
                yield return ShowResultUntilKey();
            }
            if (chapter != null) yield return StageDialogueView.Play(data, chapter, !endsChapter);
            if (endsChapter)
            {
                leaving = true;
                SceneManager.LoadScene("EndingScene");
                yield break;
            }
            // 결과 화면이 나오기 직전에 처음 한 번만 점수와 보상을 설명한다. talking 이 켜져 있어 Update 가 키를 받지 않는다.
            if (HasResult) yield return GameTips.ShowOnce(GameTip.Result);
            talking = false;
            // 새 카드 단계 앞에 결과 화면을 보여 준다. 키 입력은 Update 가 처리한다.
            resultView = StageResultView.Show(StageDataSingleton.Instance);
        }

        // talking 동안에는 Update 가 키를 받지 않으므로 여기서 직접 받는다. 첫 키는 연출을 건너뛰고 다음 키에 닫는다.
        IEnumerator ShowResultUntilKey()
        {
            StageResultView view = StageResultView.Show(StageDataSingleton.Instance);
            if (view == null) yield break;
            while (true)
            {
                yield return null;
                if (InteractionLock.IsLocked || !Input.anyKeyDown) continue;
                if (!view.IsRevealed)
                {
                    view.Skip();
                    continue;
                }
                break;
            }
            Destroy(view.gameObject);
            // 결과 화면을 닫은 키가 바로 대사를 넘기지 않게 한 프레임 기다린다.
            yield return null;
        }

        static bool HasResult => StageDataSingleton.Instance != null && StageDataSingleton.Instance.hasResult;

        bool IsFirstClear => StageDataSingleton.Instance.isFirstClear;

        // 새 카드는 1부 스테이지 0~3과 2부 스테이지 5를 처음 클리어했을 때 준다. ShowGettedCard의 switch와 같은 조건이다.
        bool HasCardReward()
        {
            int stageID = StageDataSingleton.Instance.stagePosition;
            return IsFirstClear && stageID >= 0 && (stageID <= 3 || stageID == SpawnRewardStageID);
        }

        void ShowGettedCard()
        {
            if (IsFirstClear)
            {
                switch (StageDataSingleton.Instance.stagePosition)
                {
                    case 0: // 2
                        ShowCard(spellCard, wordSo.words[2], new Vector3(0, 0, 0));
                        break;
                    case 1: // rock 5
                        ShowCard(magicCard, wordSo.words[5], new Vector3(0, 0, 0));
                        break;
                    case 2: // 4 7
                        ShowCard(magicCard, wordSo.words[4], new Vector3(-2, 0, 0));
                        ShowCard(magicCard, wordSo.words[7], new Vector3(2, 0, 0));
                        break;

                    case 3: // 1 6
                        ShowCard(magicCard, wordSo.words[1], new Vector3(-2, 0, 0));
                        ShowCard(magicCard, wordSo.words[6], new Vector3(2, 0, 0));
                        break;
                    case SpawnRewardStageID:
                        Word spawnWord = Array.Find(wordSo.words, word => word.magicType == MagicType.Spawn);
                        ShowCard(spellCard, spawnWord, new Vector3(0, 0, 0));
                        break;
                }
            }

        }

        static void ShowCard(GameObject prefab, Word word, Vector3 position)
        {
            GameObject card = Instantiate(prefab, position, Quaternion.identity);
            card.GetComponent<Card>().Setup(word);
            card.GetComponent<Order>().SetOrder(0);
        }
    }

}
