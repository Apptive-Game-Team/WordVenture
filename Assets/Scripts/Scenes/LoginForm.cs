using Account;
using Core;
using TMPro;
using UnityEngine;

namespace Scenes
{
    // 계정 창의 로그인 화면. 회원가입으로 바꾸면 닉네임 입력칸이 더 나온다.
    public class LoginForm : MonoBehaviour
    {
        [SerializeField] AccountPanel account;
        [SerializeField] TMP_InputField email;
        [SerializeField] TMP_InputField password;
        [SerializeField] TMP_InputField nickname;
        [SerializeField] TMP_Text submitLabel;
        [SerializeField] TMP_Text modeLabel;

        bool signingUp;

        public void Open()
        {
            SetSigningUp(false);
            email.Select();
        }

        public void ToggleSignUp()
        {
            SetSigningUp(!signingUp);
        }

        public void Submit()
        {
            if (AccountRequests.IsBusy) return;
            account.ShowStatus(signingUp ? "가입하는 중..." : "로그인하는 중...");
            AccountRequests.Enqueue(signingUp
                ? AccountService.SignUp(email.text.Trim(), nickname.text.Trim(), password.text, Done)
                : AccountService.Login(email.text.Trim(), password.text, Done));
        }

        void Done(AccountResult result)
        {
            if (this == null) return;
            if (result.Success) password.text = "";
            account.ShowStatus(result.Message);
        }

        void SetSigningUp(bool value)
        {
            signingUp = value;
            nickname.gameObject.SetActive(value);
            submitLabel.text = Localization.Translate(value ? "가입하기" : "로그인");
            modeLabel.text = Localization.Translate(value ? "로그인으로" : "회원가입");
        }
    }
}
