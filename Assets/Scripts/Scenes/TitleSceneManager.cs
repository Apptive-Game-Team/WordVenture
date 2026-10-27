using Core;
using Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes
{

    public class TitleSceneManager : MonoBehaviour
    {
        SaveLoadController saveLoadController;
        [SerializeField] GameObject continueButton;
        [SerializeField] TitleStageView stageView;
        GameObject newGameConfirmation;

        private void Start()
        {
            saveLoadController = GameObject.Find("SaveLoadController").GetComponent<SaveLoadController>();
            ShowSavedProgress();
        }

        // 서버에서 진행도를 불러온 뒤에도 다시 불러 이어하기 버튼과 배경을 맞춘다.
        public void ShowSavedProgress()
        {
            int stagePosition = saveLoadController.LoadPlayData();
            continueButton.SetActive(stagePosition != -1);
            stageView.Show(stagePosition);
        }


        public void LoadStoryScene()
        {
            if (saveLoadController.LoadPlayData() == -1)
            {
                MapMove.StagePosition = 0;
                saveLoadController.SavePlayData();
                SceneManager.LoadScene("StoryScene");

            } else
            {
                SceneManager.LoadScene("MapScene");
            }


        }

        // 시작하기 버튼. 저장된 진행도가 있으면 지우기 전에 확인 창을 띄운다.
        public void RequestNewGame()
        {
            if (saveLoadController.LoadPlayData() == -1)
            {
                InitPlayerData();
                return;
            }
            if (newGameConfirmation == null) newGameConfirmation = CreateNewGameConfirmation();
            newGameConfirmation.SetActive(true);
        }

        // 버튼은 이어하기 버튼을 복제해 타이틀 메뉴와 같은 스프라이트와 폰트를 쓴다.
        GameObject CreateNewGameConfirmation()
        {
            Transform canvas = continueButton.GetComponentInParent<Canvas>().transform;
            RectTransform dim = Rect("NewGameConfirmation", canvas);
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.sizeDelta = Vector2.zero;
            dim.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

            RectTransform panel = Rect("ConfirmationPanel", dim);
            panel.sizeDelta = new Vector2(820, 400);
            panel.gameObject.AddComponent<Image>().color = UiPalette.Panel;
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = UiPalette.Border;
            outline.effectDistance = new Vector2(4, -4);

            TMP_Text sample = continueButton.GetComponentInChildren<TMP_Text>(true);
            RectTransform question = Rect("Question", panel);
            question.anchorMin = new Vector2(0, 0.42f);
            question.anchorMax = Vector2.one;
            question.offsetMin = new Vector2(40, 0);
            question.offsetMax = new Vector2(-40, -30);
            TMP_Text questionText = question.gameObject.AddComponent<TextMeshProUGUI>();
            questionText.font = sample.font;
            questionText.fontSize = 36;
            questionText.color = UiPalette.Text;
            questionText.alignment = TextAlignmentOptions.Center;
            questionText.enableWordWrapping = true;
            questionText.raycastTarget = false;
            questionText.text = Localization.Translate("저장된 진행도가 모두 사라져요.\n처음부터 다시 시작할까요?");

            ConfirmationButton("CancelNewGame", panel, "취소", -160, () => dim.gameObject.SetActive(false));
            ConfirmationButton("ConfirmNewGame", panel, "시작하기", 160, InitPlayerData);
            return dim.gameObject;
        }

        void ConfirmationButton(string name, RectTransform panel, string text, float x, UnityEngine.Events.UnityAction onClick)
        {
            GameObject copy = Instantiate(continueButton, panel);
            copy.name = name;
            copy.SetActive(true);
            var rect = (RectTransform)copy.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(x, 100);
            Button button = copy.GetComponent<Button>();
            // 복제본에 남은 이어하기 연결을 지운다. RemoveAllListeners 는 씬에 직렬화된 연결을 지우지 못한다.
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(onClick);
            copy.GetComponentInChildren<TMP_Text>(true).text = Localization.Translate(text);
        }

        static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public void InitPlayerData()
        {
            SaveLoadController.Instance.InitPlayData();
            LoadStoryScene();
        }


        public void QuitGame()
        {
            SaveLoadController.Instance.SavePlayData();
            SaveLoadController.Instance.QuitGame();
        }
    }

}
