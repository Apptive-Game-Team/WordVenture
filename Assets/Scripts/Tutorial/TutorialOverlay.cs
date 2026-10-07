using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    // 기존 튜토리얼 캔버스 위에 UI를 구성한다. 폰트는 프리팹에 명시적으로 연결한다.
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] TMP_FontAsset font;
        [SerializeField] Sprite dragHand;
        RectTransform root;
        RectTransform skipButton;
        RectTransform confirmation;
        RectTransform panel;
        RectTransform cancelButton;
        RectTransform confirmButton;
        RectTransform hintPanel;
        TMP_Text hint;
        TutorialGuidance guidance;
        TutorialDragDemo dragDemo;
        TMP_Text skipText, questionText, cancelText, confirmText;
        bool modal;

        public void Initialize(TutorialController controller)
        {
            if (root != null) return;
            root = Rect("TutorialOverlay", transform);
            Stretch(root);
            // ChatWindow와 InputBlocker보다 뒤에 그려져 버튼이 차단되지 않는다.
            root.SetAsLastSibling();

            RectTransform guideRect = Rect("ActionGuidance", root);
            Stretch(guideRect);
            guidance = guideRect.gameObject.AddComponent<TutorialGuidance>();
            guidance.raycastTarget = false;
            RectTransform demoRect = Rect("DragDemo", root);
            Stretch(demoRect);
            dragDemo = demoRect.gameObject.AddComponent<TutorialDragDemo>();
            dragDemo.Initialize(guidance, dragHand, font);
            hintPanel = Background("ActionHint", root, new Color(0.04f, 0.08f, 0.14f, 0.94f), false);
            hintPanel.anchorMin = hintPanel.anchorMax = new Vector2(0.5f, 1);
            hintPanel.pivot = new Vector2(0.5f, 1);
            hint = Label("HintText", hintPanel, "");
            hint.enableWordWrapping = true;

            skipButton = Button("SkipTutorial", root, "튜토리얼 건너뛰기", controller.RequestSkip, out skipText);
            skipButton.anchorMin = skipButton.anchorMax = Vector2.one;
            skipButton.pivot = Vector2.one;

            confirmation = Background("SkipConfirmation", root, new Color(0, 0, 0, 0.75f), true);
            Stretch(confirmation);
            panel = Background("ConfirmationPanel", confirmation, new Color(0.06f, 0.1f, 0.17f, 1), true);
            questionText = Label("Question", panel, "튜토리얼을 건너뛸까요?");
            questionText.rectTransform.anchorMin = new Vector2(0, 0.4f);
            questionText.rectTransform.anchorMax = Vector2.one;
            cancelButton = Button("KeepLearning", panel, "계속 배우기", controller.CancelSkip, out cancelText);
            confirmButton = Button("ConfirmSkip", panel, "건너뛰기", controller.ConfirmSkip, out confirmText);
            cancelButton.anchorMin = cancelButton.anchorMax = new Vector2(0.27f, 0.23f);
            confirmButton.anchorMin = confirmButton.anchorMax = new Vector2(0.73f, 0.23f);
            confirmButton.GetComponent<Image>().color = new Color(0.38f, 0.23f, 0.08f, 1);
            confirmation.gameObject.SetActive(false);
            ClearGuidance();
            Layout();
        }

        void LateUpdate()
        {
            if (root != null && root.gameObject.activeSelf) Layout();
        }

        void Layout()
        {
            // Map 씬은 캔버스를 1920x1080 기준으로 덮어쓴다. 화면 배율을 캔버스
            // 배율로 나눠야 두 번 축소되지 않고 다른 씬에서도 같은 크기로 보인다.
            float canvasScale = GetComponent<Canvas>().scaleFactor;
            float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), 0.4f, 2f)
                / Mathf.Max(canvasScale, 0.001f);
            skipButton.sizeDelta = new Vector2(250, 48) * scale;
            skipButton.anchoredPosition = new Vector2(-20, -20) * scale;
            hintPanel.sizeDelta = new Vector2(Mathf.Min(620 * scale, Screen.width / canvasScale - 32 * scale), 64 * scale);
            hintPanel.anchoredPosition = new Vector2(0, -84 * scale);
            panel.sizeDelta = new Vector2(520, 230) * scale;
            cancelButton.sizeDelta = confirmButton.sizeDelta = new Vector2(210, 52) * scale;
            skipText.fontSize = 20 * scale;
            hint.fontSize = 22 * scale;
            questionText.fontSize = 28 * scale;
            cancelText.fontSize = confirmText.fontSize = 22 * scale;
            guidance.StrokeWidth = 3 * scale;
        }

        public void ShowGuidance(Transform source, Transform destination, string message, bool demonstrateClick = false)
        {
            if (guidance == null) return;
            guidance.SetTargets(source, destination);
            dragDemo.SetTargets(source, destination, demonstrateClick);
            hint.text = message;
            guidance.gameObject.SetActive(!modal);
            dragDemo.gameObject.SetActive(!modal);
            hintPanel.gameObject.SetActive(!modal);
        }

        public void ClearGuidance()
        {
            if (guidance == null) return;
            guidance.SetTargets(null, null);
            dragDemo.SetTargets(null, null);
            guidance.gameObject.SetActive(false);
            dragDemo.gameObject.SetActive(false);
            hintPanel.gameObject.SetActive(false);
        }

        public void ShowConfirmation(bool visible)
        {
            modal = visible;
            confirmation.gameObject.SetActive(visible);
            skipButton.gameObject.SetActive(!visible);
            if (visible)
            {
                guidance.gameObject.SetActive(false);
                dragDemo.gameObject.SetActive(false);
                hintPanel.gameObject.SetActive(false);
            }
        }

        public bool IsSkipPointer(Vector2 screenPoint)
        {
            Canvas canvas = GetComponent<Canvas>();
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            return skipButton != null && skipButton.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(skipButton, screenPoint, camera);
        }

        public void Hide()
        {
            ClearGuidance();
            if (root != null) root.gameObject.SetActive(false);
        }

        static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static RectTransform Background(string name, Transform parent, Color color, bool blocksInput)
        {
            RectTransform rect = Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = blocksInput;
            return rect;
        }

        TMP_Text Label(string name, RectTransform parent, string text)
        {
            RectTransform rect = Rect(name, parent);
            Stretch(rect);
            rect.offsetMin = new Vector2(12, 6);
            rect.offsetMax = new Vector2(-12, -6);
            TMP_Text label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.color = new Color(1, 0.94f, 0.8f, 1);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        RectTransform Button(string name, Transform parent, string text, UnityEngine.Events.UnityAction onClick, out TMP_Text label)
        {
            RectTransform rect = Background(name, parent, new Color(0.12f, 0.2f, 0.28f, 1), true);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(onClick);
            label = Label("Label", rect, text);
            return rect;
        }
    }
}
