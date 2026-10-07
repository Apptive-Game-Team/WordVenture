using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenes
{
    public class TitleCredits : MonoBehaviour
    {
        TMP_FontAsset font;
        RectTransform panel;
        GameObject overlay;
        Button openButton, closeButton;
        CanvasGroup menu;
        GameObject previousSelection;
        bool previousInteractable, previousBlocksRaycasts;

        public void Initialize(TMP_FontAsset creditsFont)
        {
            if (overlay != null) return;
            font = creditsFont;
            var entry = Rect("CreditsButton", transform, new Vector2(250, 120));
            entry.anchorMin = entry.anchorMax = new Vector2(1, 0);
            entry.pivot = new Vector2(1, 0);
            entry.anchoredPosition = new Vector2(-48, 48);
            openButton = Button(entry, "크레딧", 34, Show);

            var shade = Rect("CreditsOverlay", transform, Vector2.zero);
            Stretch(shade);
            shade.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            overlay = shade.gameObject;
            // 별도 CanvasGroup으로 키보드 탐색과 포인터 입력을 함께 차단한다.
            menu = GetComponent<CanvasGroup>();
            if (menu == null) menu = gameObject.AddComponent<CanvasGroup>();
            var modalGroup = overlay.AddComponent<CanvasGroup>();
            modalGroup.ignoreParentGroups = true;

            panel = Rect("CreditsPanel", shade, new Vector2(760, 780));
            panel.gameObject.AddComponent<CreditsWoodPanel>();
            Label("Title", panel, "크레딧", 44, 320, 80);
            Label("Subtitle", panel, "WORD VENTURE / 제작진", 24, 255, 50);
            Contributor("문성필", "개발자", "Monolong", 170);
            Contributor("김현진", "개발자", "Gimlocal", 95);
            Contributor("정윤성", "개발자", "dev-yunseong", 20);
            Contributor("정진욱", "디자이너 / 개발자", "Jinwook700", -55);
            Contributor("황인섭", "개발자", "hwanginseop", -130);
            Label("ArtDisclosure", panel, "이 게임에는 AI로 생성된 아트가 사용되었습니다.", 24, -230, 80);
            var close = Rect("CloseCredits", panel, new Vector2(230, 64));
            close.anchoredPosition = new Vector2(0, -325);
            closeButton = Button(close, "닫기", 28, Hide);
            overlay.SetActive(false);
        }

        void Contributor(string name, string role, string account, float y)
        {
            var row = Rect(name, panel, new Vector2(650, 64));
            row.anchoredPosition = new Vector2(0, y);
            Button(row, name + "  <size=22>" + role + "</size>", 30,
                () => Application.OpenURL("https://github.com/" + account), false);
        }

        void Show()
        {
            if (overlay.activeSelf) return;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            previousInteractable = menu.interactable;
            previousBlocksRaycasts = menu.blocksRaycasts;
            menu.interactable = menu.blocksRaycasts = false;
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
            closeButton.Select();
        }

        void Hide()
        {
            if (!overlay.activeSelf) return;
            overlay.SetActive(false);
            menu.interactable = previousInteractable;
            menu.blocksRaycasts = previousBlocksRaycasts;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection != null ? previousSelection : openButton.gameObject);
        }

        void Update()
        {
            if (overlay != null && overlay.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Hide();
        }

        void LateUpdate()
        {
            if (panel == null) return;
            var bounds = ((RectTransform)transform).rect;
            float scale = Mathf.Min(1, Mathf.Min(bounds.width / 820f, bounds.height / 840f));
            panel.localScale = Vector3.one * scale;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = parent.gameObject.layer;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            return rect;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        TMP_Text Label(string name, Transform parent, string text, float size, float y, float height)
        {
            var rect = Rect(name, parent, new Vector2(700, height));
            rect.anchoredPosition = new Vector2(0, y);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = new Color32(94, 70, 16, 255);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        Button Button(RectTransform rect, string text, float size, UnityEngine.Events.UnityAction action, bool framed = true)
        {
            Graphic background;
            if (framed) background = rect.gameObject.AddComponent<CreditsWoodPanel>();
            else
            {
                var image = rect.gameObject.AddComponent<Image>();
                image.color = new Color(1, 1, 1, 0);
                background = image;
            }
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            var colors = button.colors;
            colors.highlightedColor = colors.selectedColor = new Color(1, 0.94f, 0.72f);
            colors.pressedColor = new Color(0.78f, 0.67f, 0.44f);
            button.colors = colors;
            button.onClick.AddListener(action);
            var label = Label("Label", rect, text, size, 0, 64);
            Stretch(label.rectTransform);
            return button;
        }
    }
}
