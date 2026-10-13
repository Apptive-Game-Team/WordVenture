using System.Linq;
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
    /// 화면 모드, 언어, 크레딧, 타이틀로 돌아가기를 한곳에 모은다.
    ///
    /// 씬마다 배치하지 않고 첫 씬을 연 뒤 Resources 의 prefab 을 한 번 만들어 씬이 바뀌어도 남긴다.
    /// 다른 스크립트가 같은 프레임에 Esc 키를 아무 키 입력으로 읽기 전에 메뉴가 먼저 열리도록 가장 먼저 실행한다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SettingsMenu : MonoBehaviour
    {
        public const string PrefabPath = "UI/SettingsMenu";
        const string TitleSceneName = "TitleScene";
        const string MapSceneName = "MapScene";
        const int WindowWidth = 1280;
        const int WindowHeight = 720;
        // 맵 배경 그림의 왼쪽 위에는 로고와 Stage 글자가 있어서 메뉴 버튼을 그 아래 바다 위로 내린다.
        static readonly Vector2 ButtonPosition = new Vector2(24, -24);
        static readonly Vector2 MapButtonPosition = new Vector2(24, -170);

        [SerializeField] GameObject menu;
        [SerializeField] Button openButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button screenModeButton;
        [SerializeField] Button languageButton;
        [SerializeField] Button creditsButton;
        [SerializeField] Button titleButton;
        [SerializeField] TMP_Text heading;
        [SerializeField] TMP_Text languageNotice;
        [SerializeField] Credits credits;

        float pausedTimeScale = 1;

        public static SettingsMenu Instance { get; private set; }
        public bool IsOpen => menu.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            var prefab = Resources.Load<SettingsMenu>(PrefabPath);
            if (prefab != null && Instance == null) Instantiate(prefab);
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            menu.SetActive(false);
            credits.gameObject.SetActive(false);
            openButton.onClick.AddListener(Open);
            resumeButton.onClick.AddListener(Close);
            screenModeButton.onClick.AddListener(ToggleScreenMode);
            languageButton.onClick.AddListener(ToggleLanguage);
            creditsButton.onClick.AddListener(credits.Show);
            titleButton.onClick.AddListener(GoToTitle);
            SceneManager.sceneLoaded += OnSceneLoaded;
            PlaceOpenButton();
            EnsureEventSystem();
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (IsOpen) Close();
            Instance = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11)) ToggleScreenMode();
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (credits.IsOpen) credits.Hide();
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
            menu.SetActive(false);
            Time.timeScale = pausedTimeScale;
            InteractionLock.IsPaused = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        // Automatic navigation 은 메뉴 아래에 있는 씬 버튼까지 찾아가므로, 보이는 메뉴 버튼끼리만 위아래로 잇는다.
        void LinkNavigation()
        {
            Button[] buttons = new[] { resumeButton, screenModeButton, languageButton, creditsButton, titleButton }
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

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            Close();
            PlaceOpenButton();
            EnsureEventSystem();
        }

        void PlaceOpenButton()
        {
            ((RectTransform)openButton.transform).anchoredPosition =
                SceneManager.GetActiveScene().name == MapSceneName ? MapButtonPosition : ButtonPosition;
        }

        // 메뉴 버튼은 씬의 EventSystem 으로 클릭을 받는다. EventSystem 이 없는 씬(GameOverScene)에는 그 씬에 하나 만든다.
        static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
