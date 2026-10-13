using Account;
using TMPro;
using UnityEngine;

namespace Scenes
{
    // 계정 창의 피드백 화면. 로그인한 회원만 열 수 있다.
    public class FeedbackPanel : MonoBehaviour
    {
        [SerializeField] AccountPanel account;
        [SerializeField] TMP_InputField text;

        void Awake()
        {
            text.characterLimit = AccountSession.FeedbackMaxLength;
        }

        public void Open()
        {
            text.Select();
        }

        public void Send()
        {
            if (AccountRequests.IsBusy) return;
            if (string.IsNullOrWhiteSpace(text.text))
            {
                account.ShowStatus("보낼 내용을 적어 주세요.");
                return;
            }
            account.ShowStatus("보내는 중...");
            AccountRequests.Enqueue(AccountService.SendFeedback(text.text, result =>
            {
                if (this == null) return;
                if (result.Success) text.text = "";
                account.ShowStatus(result.Message);
            }));
        }

        // 계정 창을 닫지 않고 회원 화면으로 돌아간다.
        public void Close()
        {
            account.ShowStatus("");
            account.ShowHome(false);
        }
    }
}
