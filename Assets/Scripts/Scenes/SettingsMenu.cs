using System.Linq;
using Combat.UI;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes
{
    /// <summary>
    /// 어느 씬에서나 Esc 키나 왼쪽 위 메뉴 버튼으로 여는 설정 메뉴.
    /// 화면 모드, 언어, 크레딧, 계정, 타이틀로 돌아가기를 한곳에 모은다.
    ///
    /// 빌드 설정의 씬마다 prefab 을 배치한다. runtime 에 생성하면 메뉴 버튼이 생기지 않을 때가 있어서,
    /// 씬 파일만 보고 메뉴가 있는지 알 수 있게 했다.
    /// 다른 스크립트가 같은 프레임에 Esc 키를 아무 키 입력으로 읽기 전에 메뉴가 먼저 열리도록 가장 먼저 실행한다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SettingsMenu : MonoBehaviour
    {
        public const string TitleSceneName = "TitleScene";
        const string MapSceneName = "MapScene";
        const int WindowWidth = 1280;
        const int WindowHeight = 720;

        [SerializeField] GameObject menu;
        [SerializeField] Button openButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button screenModeButton;
        [SerializeField] Button languageButton;
        [SerializeField] Button creditsButton;
        [SerializeField] Button accountButton;
        [SerializeField] Button titleButton;
        [SerializeField] TMP_Text heading;
        [SerializeField] TMP_Text languageNotice;
        [SerializeField] Credits credits;
        [SerializeField] AccountPanel account;

        float pausedTimeScale = 1;

        public static SettingsMenu Instance { get; private set; }
        public bool IsOpen => menu.activeSelf;

        void Awake()
        {
            Instance = this;
            menu.SetActive(false);
            credits.gameObject.SetActive(false);
            openButton.onClick.AddListener(Open);
            resumeButton.onClick.AddListener(Close);
            screenModeButton.onClick.AddListener(ToggleScreenMode);
            languageButton.onClick.AddListener(ToggleLanguage);
            creditsButton.onClick.AddListener(credits.Show);
            accountButton.onClick.AddListener(account.Show);
            titleButton.onClick.AddListener(GoToTitle);
        }

        // 씬을 바꾸면 다음 씬의 메뉴가 Awake 에서 Instance 를 먼저 가져갈 수 있다.
        // 그래도 열린 채로 사라지는 메뉴는 멈춘 시간과 입력을 되돌려 놓는다.
        void OnDestroy()
        {
            // 씬을 내릴 때는 자식인 menu 가 먼저 지워져 있을 수 있다.
            if (menu != null && IsOpen) Close();
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11)) ToggleScreenMode();
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (credits.IsOpen) credits.Hide();
            else if (account.IsOpen) account.Hide();
            // 맵의 슬라임 도감은 Esc 키로 닫으므로, 도감이 열려 있으면 메뉴를 열지 않는다.
            else if (!IsOpen && SlimeEncyclopediaView.Current != null) return;
            else if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            pausedTimeScale = Time.timeScale;
            Time.timeScale = 0;
            InteractionLock.IsPaused = true;
            languageNotice.gameObject.SetActive(false);
            titleButton.gameObject.SetActive(SceneManager.GetActiveScene().name != TitleSceneName);
            LinkNavigation();
            ShowLabels();
            menu.SetActive(true);
            resumeButton.Select();
        }

        public void Close()
        {
            if (!IsOpen) return;
            credits.Hide();
            account.Hide();
            menu.SetActive(false);
            Time.timeScale = pausedTimeScale;
            InteractionLock.IsPaused = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        // Automatic navigation 은 메뉴 아래에 있는 씬 버튼까지 찾아가므로, 보이는 메뉴 버튼끼리만 위아래로 잇는다.
        void LinkNavigation()
        {
            Button[] buttons = new[] { resumeButton, screenModeButton, languageButton, creditsButton, accountButton, titleButton }
                .Where(button => button.gameObject.activeSelf).ToArray();
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length],
                    selectOnDown = buttons[(i + 1) % buttons.Length],
                };
        }

        void ShowLabels()
        {
            heading.text = Localization.Translate("설정");
            Label(resumeButton).text = Localization.Translate("계속하기");
            Label(screenModeButton).text = Localization.Translate(Screen.fullScreen ? "창 모드 (F11)" : "전체 화면 (F11)");
            // 언어 버튼에는 지금 언어가 아니라 바꿀 언어를 그 언어로 적는다.
            Label(languageButton).text = Localization.Current == Language.Korean ? "English" : "한국어";
            Label(creditsButton).text = Localization.Translate("크레딧");
            Label(accountButton).text = Localization.Translate("계정");
            Label(titleButton).text = Localization.Translate("타이틀로");
            languageNotice.text = Localization.Translate("다음 화면부터 적용됩니다");
        }

        static TMP_Text Label(Button button) => button.GetComponentInChildren<TMP_Text>(true);

        void ToggleScreenMode()
        {
#if UNITY_WEBGL
            Screen.fullScreen = !Screen.fullScreen;
#else
            // 화면 모드와 창 크기는 Unity 가 PlayerPrefs 에 저장해서 다음 실행에도 그대로 연다.
            if (Screen.fullScreen)
            {
                Resolution display = Screen.currentResolution;
                float fit = Mathf.Min(1, display.width * 0.8f / WindowWidth, display.height * 0.8f / WindowHeight);
                Screen.SetResolution((int)(WindowWidth * fit), (int)(WindowHeight * fit), FullScreenMode.Windowed);
            }
            else
            {
                Resolution display = Screen.currentResolution;
                Screen.SetResolution(display.width, display.height, FullScreenMode.FullScreenWindow);
            }
#endif
            // Screen.fullScreen 은 다음 프레임에 바뀌므로 버튼에는 바뀔 모드를 바로 적는다.
            if (IsOpen)
                Label(screenModeButton).text = Localization.Translate(Screen.fullScreen ? "전체 화면 (F11)" : "창 모드 (F11)");
        }

        void ToggleLanguage()
        {
            Localization.SetLanguage(Localization.Current == Language.Korean ? Language.English : Language.Korean);
            string scene = SceneManager.GetActiveScene().name;
            // 씬에 직렬화된 문구는 씬을 열 때 바뀐다. 타이틀과 맵은 다시 열어도 잃는 진행이 없으므로 바로 다시 열고,
            // 전투와 대화는 진행을 잃지 않도록 다음 씬부터 적용한다.
            if (scene == TitleSceneName || scene == MapSceneName)
            {
                Close();
                SceneManager.LoadScene(scene);
                return;
            }
            ShowLabels();
            languageNotice.gameObject.SetActive(true);
        }

        void GoToTitle()
        {
            if (SaveLoadController.Instance != null) SaveLoadController.Instance.SavePlayData();
            Close();
            SceneManager.LoadScene(TitleSceneName);
        }
    }
}
