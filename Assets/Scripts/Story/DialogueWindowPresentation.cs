using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Story
{
    // 스토리, 지역 대화, 튜토리얼의 표시를 한 곳에서 구성한다.
    public sealed class DialogueWindowPresentation : MonoBehaviour
    {
        public TMP_Text NameLabel { get; private set; }
        public TMP_Text TextLabel { get; private set; }
        public TMP_Text PromptLabel { get; private set; }
        public Image LeftPortrait { get; private set; }
        public Image RightPortrait { get; private set; }
        DialogueWindowStyle style;

        public static DialogueWindowPresentation Create(Transform parent)
        {
            var root = new GameObject("DialogueWindow", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform);
            var view = root.AddComponent<DialogueWindowPresentation>();
            view.style = Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle");
            view.Build();
            return view;
        }

        public static void ConfigureCanvas(Canvas canvas)
        {
            if (canvas == null) return;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        void Build()
        {
            LeftPortrait = Picture("WordPortrait", transform, new Vector2(0.05f, 0.25f), new Vector2(0.46f, 0.88f), Color.white);
            RightPortrait = Picture("OtherPortrait", transform, new Vector2(0.54f, 0.25f), new Vector2(0.95f, 0.88f), Color.white);
            LeftPortrait.preserveAspect = RightPortrait.preserveAspect = true;
            Image panel = Picture("DialoguePanel", transform, new Vector2(0.04f, 0.035f), new Vector2(0.96f, 0.29f), style.panelColor);
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = style.borderColor;
            outline.effectDistance = new Vector2(3, -3);
            NameLabel = Label("ChatName", panel.transform, new Vector2(0.035f, 0.74f), new Vector2(0.965f, 0.97f), style.nameSize, style.nameColor);
            TextLabel = Label("ChatText", panel.transform, new Vector2(0.035f, 0.24f), new Vector2(0.965f, 0.74f), style.textSize, style.textColor);
            PromptLabel = Label("AdvancePrompt", panel.transform, new Vector2(0.035f, 0.03f), new Vector2(0.965f, 0.2f), style.promptSize, style.promptColor);
            PromptLabel.alignment = TextAlignmentOptions.Right;
            SetPortraits(null, null, true);
        }

        public void SetPortraits(Sprite left, Sprite right, bool leftSpeaking)
        {
            LeftPortrait.sprite = left;
            RightPortrait.sprite = right;
            LeftPortrait.gameObject.SetActive(left != null);
            RightPortrait.gameObject.SetActive(right != null);
            LeftPortrait.color = leftSpeaking ? Color.white : style.listenerColor;
            RightPortrait.color = leftSpeaking ? style.listenerColor : Color.white;
        }

        public void SetSpeaker(string name)
        {
            bool narration = string.IsNullOrEmpty(name) || name == "나레이터";
            NameLabel.gameObject.SetActive(!narration);
            TextLabel.rectTransform.anchorMax = new Vector2(0.965f, narration ? 0.9f : 0.74f);
            if (narration) SetPortraits(null, null, true);
            else if (name == "워드" && LeftPortrait.sprite == null && RightPortrait.sprite == null)
                SetPortraits(style.wordPortrait, null, true);
        }

        public static Image Picture(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        TMP_Text Label(string name, Transform parent, Vector2 min, Vector2 max, int size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = obj.AddComponent<TextMeshProUGUI>();
            label.font = style.font;
            label.fontSize = size;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }
    }
}
