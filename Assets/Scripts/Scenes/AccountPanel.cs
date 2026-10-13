using Account;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenes
{
    /// <summary>
    /// 타이틀의 계정 창. 크레딧 창처럼 메뉴를 막고 열리며, 로그인 여부에 따라 로그인 화면이나 회원 화면을 보여 준다.
    /// 로그인은 선택 사항이라 이 창을 열지 않아도 게임은 그대로 돌아간다.
    /// </summary>
    public class AccountPanel : MonoBehaviour
    {
        [SerializeField] RectTransform panel;
        [SerializeField] GameObject overlay;
        [SerializeField] Button openButton;
        [SerializeField] CanvasGroup menu;
        // 요청을 보내는 동안 창 안의 버튼과 입력칸을 막는다.
        [SerializeField] CanvasGroup content;
        [SerializeField] LoginForm loginForm;
        [SerializeField] MemberView memberView;
        [SerializeField] FeedbackPanel feedbackPanel;
        [SerializeField] TMP_Text status;

        static bool restoreTried;
        static AccountPanel current;
        int hiddenFrame = -1;

        // 설정 메뉴는 Esc 키로 열리므로, 계정 창이 열려 있거나 이번 프레임에 Esc 키로 닫혔으면 메뉴를 열지 않는다.
        public static bool HoldsEscape => current != null && (current.overlay.activeSelf || current.hiddenFrame == Time.frameCount);
        GameObject previousSelection;
        bool previousInteractable, previousBlocksRaycasts;

        void Awake()
        {
            current = this;
            AccountSession.Changed += ShowHomeIfOpen;
            AccountRequests.BusyChanged += ShowBusy;
        }

        void OnDestroy()
        {
            if (current == this) current = null;
            AccountSession.Changed -= ShowHomeIfOpen;
            AccountRequests.BusyChanged -= ShowBusy;
        }

        void Start()
        {
            ShowBusy();
            // 저장된 refresh token 이 있으면 게임을 켠 뒤 처음 타이틀을 열 때 한 번만 조용히 로그인을 복원한다.
            if (restoreTried || AccountSession.IsLoggedIn) return;
            restoreTried = true;
            AccountRequests.Enqueue(AccountService.RestoreSession(_ => { }));
        }

        public void Show()
        {
            if (overlay.activeSelf) return;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            previousInteractable = menu.interactable;
            previousBlocksRaycasts = menu.blocksRaycasts;
            menu.interactable = menu.blocksRaycasts = false;
            overlay.SetActive(true);
            ShowStatus("");
            ShowHome(true);
        }

        public void Hide()
        {
            if (!overlay.activeSelf) return;
            overlay.SetActive(false);
            hiddenFrame = Time.frameCount;
            menu.interactable = previousInteractable;
            menu.blocksRaycasts = previousBlocksRaycasts;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection != null ? previousSelection : openButton.gameObject);
        }

        // refresh 가 true 면 회원 화면이 서버 진행도를 다시 받아 온다.
        public void ShowHome(bool refresh)
        {
            bool loggedIn = AccountSession.IsLoggedIn;
            feedbackPanel.gameObject.SetActive(false);
            loginForm.gameObject.SetActive(!loggedIn);
            memberView.gameObject.SetActive(loggedIn);
            if (loggedIn) memberView.Open(refresh);
            else loginForm.Open();
        }

        public void ShowFeedback()
        {
            loginForm.gameObject.SetActive(false);
            memberView.gameObject.SetActive(false);
            feedbackPanel.gameObject.SetActive(true);
            feedbackPanel.Open();
        }

        // AccountResult.Message 처럼 한국어 원문을 받아 지금 언어로 보여 준다.
        public void ShowStatus(string message)
        {
            status.text = Localization.Translate(message);
        }

        void ShowHomeIfOpen()
        {
            if (overlay.activeSelf) ShowHome(true);
        }

        void ShowBusy()
        {
            content.interactable = !AccountRequests.IsBusy;
        }

        void Update()
        {
            if (overlay.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Hide();
        }

        void LateUpdate()
        {
            var bounds = ((RectTransform)transform).rect;
            float scale = Mathf.Min(1, Mathf.Min(bounds.width / 820f, bounds.height / 840f));
            panel.localScale = Vector3.one * scale;
        }
    }
}
