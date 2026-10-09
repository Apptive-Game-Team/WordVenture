using System.Collections;
using Core;
using TMPro;
using UnityEngine;

namespace Story
{

    public class ChatWindowController : MonoBehaviour
    {
        private const float TextStreamInterval = 0.03f;

        // 글자마다 새로 만들면 대사 한 줄에 길이만큼 할당이 쌓인다.
        private static readonly WaitForSeconds TextStreamDelay = new WaitForSeconds(TextStreamInterval);

        [SerializeField] GameObject anyKeyPrompt;

        private TMP_Text chatName;
        private TMP_Text chatText;

        private Coroutine streamingCoroutine;
        private string streamingText;
        public DialogueWindowPresentation Presentation { get; private set; }

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

            chatName.SetText(Localization.Translate(name));
            Presentation.SetSpeaker(name);
            chatText.SetText(string.Empty);
            Presentation.PromptLabel.SetText(Localization.Translate("클릭 / 아무 키 · 대사 펼치기"));
            SetAnyKeyPromptVisible(true);
            streamingText = Localization.Translate(text) + " ";
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
                yield return TextStreamDelay;
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
            Presentation.PromptLabel.SetText(Localization.Translate("클릭 / 아무 키 · 다음"));
            SetAnyKeyPromptVisible(true);
        }

        public void SetPortraits(Sprite left, Sprite right, bool leftSpeaking)
        {
            Presentation.SetPortraits(left, right, leftSpeaking);
        }

        /// <summary>
        /// 기존 씬의 직렬화 연결은 유지하면서 공통 대화창을 구성한다.
        /// </summary>
        protected virtual void InitTexts()
        {
            // 기존 씬/프리팹의 컨트롤러 연결은 유지하고 표시 자식만 공통 창으로 교체한다.
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            DialogueWindowPresentation.Stretch((RectTransform)transform);
            DialogueWindowPresentation.ConfigureCanvas(GetComponentInParent<Canvas>());
            Presentation = DialogueWindowPresentation.Create(transform);
            chatName = Presentation.NameLabel;
            chatText = Presentation.TextLabel;
            anyKeyPrompt = Presentation.PromptLabel.gameObject;
        }

        void OnDisable()
        {
            if (streamingCoroutine != null) StopCoroutine(streamingCoroutine);
            streamingCoroutine = null;
        }

    }

}
