using Account;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenes
{
    /// <summary>
    /// 설정 메뉴에서 여는 계정 창. 크레딧 창처럼 메뉴를 막고 열리며, 로그인 여부에 따라 로그인 화면이나 회원 화면을 보여 준다.
    /// Esc 키는 설정 메뉴가 받아서 이 창을 닫는다.
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
        GameObject previousSelection;
        bool previousInteractable, previousBlocksRaycasts;

        public bool IsOpen => overlay.activeSelf;

        void Awake()
        {
            AccountSession.Changed += ShowHomeIfOpen;
            AccountRequests.BusyChanged += ShowBusy;
        }

        void OnDestroy()
        {
            AccountSession.Changed -= ShowHomeIfOpen;
            AccountRequests.BusyChanged -= ShowBusy;
        }

        void Start()
        {
            ShowBusy();
            // 저장된 refresh token 이 있으면 게임을 켠 뒤 처음 연 씬에서 한 번만 조용히 로그인을 복원한다.
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

        void LateUpdate()
        {
            var bounds = ((RectTransform)transform).rect;
            float scale = Mathf.Min(1, Mathf.Min(bounds.width / 820f, bounds.height / 840f));
            panel.localScale = Vector3.one * scale;
        }
    }
}
