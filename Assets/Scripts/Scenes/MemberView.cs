using Account;
using Combat.Stage;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes
{
    /// <summary>
    /// 계정 창의 회원 화면. 이 기기와 서버의 진행도를 나란히 보여 주고 올리기와 불러오기를 맡는다.
    /// 불러오기는 이 기기의 진행도를 덮어쓰므로 한 번 더 묻는다.
    /// 전투나 대화 도중에 덮어쓰면 진행 중인 씬과 저장된 stage 가 맞지 않으므로 불러오기는 타이틀에서만 보인다.
    /// </summary>
    public class MemberView : MonoBehaviour
    {
        [SerializeField] AccountPanel account;
        // TitleScene 의 TitleStageView 와 같은 목록이다. PrefabWiringTests 가 두 목록이 같은지 검사한다.
        [SerializeField] StageData[] stages;
        [SerializeField] GameObject download;
        [SerializeField] TMP_Text memberName;
        [SerializeField] TMP_Text progress;
        [SerializeField] GameObject actions;
        [SerializeField] GameObject confirm;
        [SerializeField] TMP_Text confirmText;
        [SerializeField] Button firstAction;
        [SerializeField] Button cancelConfirm;

        // 서버 진행도를 아직 모르면 null 이다. 서버에 저장이 없으면 serverChecked 만 true 가 된다.
        CloudSave serverSave;
        bool serverChecked;

        public void Open(bool refresh)
        {
            memberName.text = string.Format(Localization.Translate("{0} 님"), AccountSession.MemberName);
            download.SetActive(SceneManager.GetActiveScene().name == SettingsMenu.TitleSceneName);
            ShowConfirm(false);
            if (refresh)
            {
                serverSave = null;
                serverChecked = false;
                AccountRequests.Enqueue(AccountService.DownloadSave(Downloaded));
            }
            ShowProgress();
        }

        public void Upload()
        {
            if (AccountRequests.IsBusy) return;
            account.ShowStatus("서버에 저장하는 중...");
            AccountRequests.Enqueue(AccountService.UploadSave(result =>
            {
                if (this == null) return;
                account.ShowStatus(result.Message);
                if (!result.Success) return;
                serverSave = CloudSave.FromLocal();
                serverChecked = true;
                ShowProgress();
            }));
        }

        // 확인을 묻기 전에 서버 진행도를 다시 받아서 지금 서버에 있는 값으로 덮어쓰게 한다.
        public void AskDownload()
        {
            if (AccountRequests.IsBusy) return;
            account.ShowStatus("서버 진행도를 확인하는 중...");
            AccountRequests.Enqueue(AccountService.DownloadSave((result, save) =>
            {
                if (this == null) return;
                Downloaded(result, save);
                if (!result.Success || save == null)
                {
                    account.ShowStatus(result.Message);
                    return;
                }
                account.ShowStatus("");
                confirmText.text = string.Format(Localization.Translate("이 기기의 진행도({0})를 서버 진행도({1})로 덮어씁니다."),
                    Describe(CloudSave.FromLocal().StagePosition), Describe(save.StagePosition));
                ShowConfirm(true);
            }));
        }

        public void ConfirmDownload()
        {
            if (serverSave == null) return;
            serverSave.ApplyToLocal();
            TitleSceneManager title = FindObjectOfType<TitleSceneManager>();
            if (title != null) title.ShowSavedProgress();
            ShowConfirm(false);
            ShowProgress();
            account.ShowStatus("서버 진행도를 불러왔습니다.");
        }

        public void CancelDownload()
        {
            ShowConfirm(false);
        }

        public void Logout()
        {
            if (AccountRequests.IsBusy) return;
            AccountRequests.Enqueue(AccountService.Logout(result =>
            {
                if (this != null) account.ShowStatus(result.Message);
            }));
        }

        void Downloaded(AccountResult result, CloudSave save)
        {
            if (this == null) return;
            serverChecked = result.Success;
            serverSave = save;
            ShowProgress();
        }

        void ShowProgress()
        {
            string server = serverSave != null ? Describe(serverSave.StagePosition)
                : Localization.Translate(serverChecked ? "저장 없음" : "확인 중");
            progress.text = string.Format(Localization.Translate("이 기기: {0}"), Describe(CloudSave.FromLocal().StagePosition))
                + "\n" + string.Format(Localization.Translate("서버: {0}"), server);
        }

        string Describe(int stagePosition) => TitleStageView.Describe(stages, stagePosition);

        void ShowConfirm(bool asking)
        {
            confirm.SetActive(asking);
            actions.SetActive(!asking);
            (asking ? cancelConfirm : firstAction).Select();
        }
    }
}
