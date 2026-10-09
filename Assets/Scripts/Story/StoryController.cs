using System.Collections;
using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Story
{
    public class StoryController : MonoBehaviour
    {

        public ChatWindowController chatWindowController;
        [SerializeField] ChatWindowScriptContainer scriptContainer;
        // 엔딩 씬에서 2부(세계수의 심장)까지 끝냈을 때 scriptContainer 대신 보여 줄 대사. 다른 씬은 비워 둔다.
        [SerializeField] ChatWindowScriptContainer actTwoEndingScript;

        [SerializeField] List<GameObject> backgorunds = new List<GameObject>();

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip badMood;

        // 세계수의 심장(9)을 끝내면 StagePosition이 10이 된다.
        const int ActTwoCompletedStagePosition = 10;

        void Start()
        {
            if (actTwoEndingScript != null && MapMove.StagePosition >= ActTwoCompletedStagePosition)
                scriptContainer = actTwoEndingScript;
            InitBackground();
            StartCoroutine(StoryTelling());

        }

        private void InitBackground()
        {

            backgorunds[0].SetActive(true);
            for (int i = 1; i < backgorunds.Count; i++)
            {
                backgorunds[i].SetActive(false);
            }
        }

        private void SwitchBackground(int id)
        {
            for (int i = 0; i < backgorunds.Count; i++)
            {
                if (i == id)
                {
                    backgorunds[i].SetActive(true);
                }
                else
                {
                    backgorunds[i].SetActive(false);
                }

            }
            if (id == 1 && badMood != null && audioSource != null)
            {
                audioSource.clip = badMood;
                audioSource.Play();
            }
        }


        IEnumerator StoryTelling()
        {
            for (int i = 0; i < scriptContainer.GetScriptNum(); i++)
            {
                SwitchBackground(scriptContainer.GetScriptData(i).background);
                chatWindowController.SetAnyKeyPromptVisible(false);
                chatWindowController.UpdateChatStream(scriptContainer.GetScriptData(i).name, scriptContainer.GetScriptData(i).text);

                // 타이핑 연출이 끝날 때까지 기다리되, 키를 누르면 즉시 끝낸다.
                // 재생 시간을 따로 계산해 기다리면 실제 코루틴보다 항상 조금 짧게 끝나서
                // 이전 대사의 스트리밍이 살아 있는 채로 다음 대사가 시작된다.
                yield return new WaitUntil(() => !chatWindowController.IsStreaming || IsAdvanceKeyDown());
                if (chatWindowController.IsStreaming)
                {
                    chatWindowController.CompleteStream();
                    yield return null;
                }

                yield return new WaitUntil(IsAdvanceKeyDown);
                chatWindowController.SetAnyKeyPromptVisible(false);

                // 입력이 발생한 프레임을 소비한다. WaitUntil은 yield된 프레임에 바로 평가되므로,
                // 이게 없으면 다음 대사를 시작한 직후의 대기가 같은 프레임의 anyKeyDown으로
                // 곧바로 성립해서 한 번의 입력이 "다음 대사 진행 + 타이핑 즉시 완료"까지 해버린다.
                yield return null;
            }

            LoadMapScene();
        }

        /// <summary>
        /// 아무 키 입력에는 마우스 버튼도 포함한다.
        /// </summary>
        protected static bool IsAdvanceKeyDown()
        {
            return Input.anyKeyDown;
        }

        private void LoadMapScene()
        {
            // 1부 엔딩(5)과 2부 에필로그(10) 뒤에는 타이틀로 돌아간다.
            if (MapMove.StagePosition == 5 || MapMove.StagePosition >= ActTwoCompletedStagePosition)
                SceneManager.LoadScene("TitleScene");
            else
                SceneManager.LoadScene("MapScene");
        }
    }
}
