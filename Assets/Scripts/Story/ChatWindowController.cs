using System.Collections;
using TMPro;
using UnityEngine;

namespace Story
{

    public class ChatWindowController : MonoBehaviour
    {
        private const float TextStreamInterval = 0.03f;

        [SerializeField] GameObject anyKeyPrompt;

        private TMP_Text chatName;
        private TMP_Text chatText;

        private Coroutine streamingCoroutine;
        private string streamingText;

        public bool IsStreaming { get { return streamingCoroutine != null; } }

        private void Awake()
        {
            InitTexts();
        }

        public void UpdateChatStream(string name, string text)
        {
            // 이전 대사의 스트리밍이 남아 있으면 정리한다. 두 코루틴이 같은 TMP_Text에
            // 서로 다른 substring을 쓰면 대사를 연타로 넘길 때 텍스트가 깜박인다.
            if (streamingCoroutine != null)
            {
                StopCoroutine(streamingCoroutine);
            }

            chatName.SetText(name);
            streamingText = text + " ";
            streamingCoroutine = StartCoroutine(UpdateStreamingChat());
        }

        /// <summary>
        /// 진행 중인 타이핑 연출을 즉시 끝내고 전체 대사를 표시한다.
        /// </summary>
        public void CompleteStream()
        {
            if (streamingCoroutine == null)
            {
                return;
            }

            StopCoroutine(streamingCoroutine);
            streamingCoroutine = null;
            chatText.SetText(streamingText);
            OnStreamComplete();
        }

        IEnumerator UpdateStreamingChat()
        {
            for (int i = 0; i < streamingText.Length; i++)
            {
                yield return new WaitForSeconds(TextStreamInterval);
                chatText.SetText(streamingText.Substring(0, i));
            }

            chatText.SetText(streamingText);
            streamingCoroutine = null;
            OnStreamComplete();
        }

        /// <summary>
        /// "아무 키나 누르세요" 안내를 켜고 끈다. 안내 오브젝트가 연결되지 않은
        /// 대화창도 있으므로 null이면 조용히 넘어간다.
        /// </summary>
        public void SetAnyKeyPromptVisible(bool visible)
        {
            if (anyKeyPrompt == null)
            {
                return;
            }

            anyKeyPrompt.SetActive(visible);
        }

        protected virtual void OnStreamComplete()
        {
            SetAnyKeyPromptVisible(true);
        }

        /// <summary>
        /// 자식 TMP_Text를 이름으로 찾는다. 인덱스로 찾으면 대화창에 텍스트가
        /// 하나만 추가돼도 참조가 어긋난다.
        /// </summary>
        protected virtual void InitTexts()
        {
            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == "ChatName")
                {
                    chatName = text;
                }
                else if (text.name == "ChatText")
                {
                    chatText = text;
                }
            }
        }

    }

}
