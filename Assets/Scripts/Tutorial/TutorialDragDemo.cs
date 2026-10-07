using Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    // 실제 대상은 건드리지 않고 손으로 드래그와 클릭을 시연한다.
    public class TutorialDragDemo : MonoBehaviour
    {
        TutorialGuidance guidance;
        Image hand, ghost;
        TMP_Text cardName;
        Transform source, destination;
        bool demonstrateClick;
        float cycleStart;

        public void Initialize(TutorialGuidance guide, Sprite handSprite, TMP_FontAsset font)
        {
            guidance = guide;
            ghost = Image("DemoCard");
            var label = new GameObject("CardName", typeof(RectTransform)).GetComponent<RectTransform>();
            label.SetParent(ghost.transform, false);
            label.anchorMin = new Vector2(0.2f, 0.7f);
            label.anchorMax = new Vector2(0.8f, 0.95f);
            label.offsetMin = label.offsetMax = Vector2.zero;
            cardName = label.gameObject.AddComponent<TextMeshProUGUI>();
            cardName.font = font;
            cardName.alignment = TextAlignmentOptions.Center;
            cardName.enableAutoSizing = true;
            cardName.fontSizeMin = 8;
            cardName.fontSizeMax = 36;
            cardName.raycastTarget = false;
            hand = Image("DemoHand");
            hand.sprite = handSprite;
            // 아트의 검지 끝(위쪽 왼편)이 접촉점이다. 손바닥 중심으로 짚지 않는다.
            hand.rectTransform.pivot = new Vector2(0.26f, 0.875f);
            SetImagesVisible(false);
        }

        Image Image(string name)
        {
            var rect = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)).GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            Image image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        public void SetTargets(Transform from, Transform to, bool click = false)
        {
            if (source != from || destination != to || demonstrateClick != click) cycleStart = Time.unscaledTime;
            source = from;
            destination = to;
            demonstrateClick = click;
            if (to == null) SetImagesVisible(false);
        }

        void OnEnable() { cycleStart = Time.unscaledTime; }

        void LateUpdate()
        {
            if (guidance == null || hand == null) return;
            if ((CardManager.Inst != null && CardManager.Inst.IsDraggingCard)
                || !guidance.TryRect(destination, out Rect to))
            {
                cycleStart = Time.unscaledTime;
                SetImagesVisible(false);
                return;
            }
            // 출발 카드가 없는 버튼·적 안내는 검지 끝으로 짧게 누르는 시연이다.
            if (source == null && demonstrateClick)
            {
                ShowClick(to.center);
                return;
            }
            if (!guidance.TryRect(source, out Rect from))
            {
                SetImagesVisible(false);
                return;
            }
            float elapsed = Mathf.Repeat(Time.unscaledTime - cycleStart, 3.6f);
            if (elapsed >= 2.45f)
            {
                SetImagesVisible(false);
                return;
            }
            SetImagesVisible(true);
            float travel = Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - 0.45f) / 1.5f));
            Vector2 point = Vector2.Lerp(from.center, to.center, travel);
            float release = Mathf.Clamp01((elapsed - 1.95f) / 0.5f);
            float fade = Mathf.Min(Mathf.Clamp01(elapsed / 0.2f), 1 - release);
            float scale = guidance.StrokeWidth / 3;
            hand.rectTransform.sizeDelta = Vector2.one * (72 * scale);
            hand.rectTransform.anchoredPosition = point + Vector2.up * (release * 16 * scale);
            float press = elapsed < 0.45f ? 1 - 0.08f * Mathf.Sin(elapsed / 0.45f * Mathf.PI) : 0.96f + release * 0.04f;
            hand.rectTransform.localScale = Vector3.one * press;
            hand.color = new Color(1, 1, 1, fade);

            SpriteRenderer original = source.GetComponent<SpriteRenderer>();
            ghost.sprite = original != null ? original.sprite : null;
            if (guidance.TryRect(source, out Rect spriteRect, true))
                ghost.rectTransform.sizeDelta = spriteRect.size - Vector2.one * guidance.StrokeWidth * 5;
            ghost.rectTransform.anchoredPosition = point;
            float ghostAlpha = elapsed < 0.3f ? 0 : 0.55f * fade;
            ghost.color = new Color(1, 1, 1, ghostAlpha);
            TMP_Text originalName = source.GetComponentInChildren<TMP_Text>();
            cardName.text = originalName != null ? originalName.text : "";
            cardName.color = new Color(0.2f, 0.15f, 0.2f, ghostAlpha);
        }

        void ShowClick(Vector2 contact)
        {
            hand.gameObject.SetActive(true);
            ghost.gameObject.SetActive(false);
            float elapsed = Mathf.Repeat(Time.unscaledTime - cycleStart, 1.8f);
            float press = elapsed < 0.6f ? Mathf.Sin(elapsed / 0.6f * Mathf.PI) : 0;
            float scale = guidance.StrokeWidth / 3;
            hand.rectTransform.sizeDelta = Vector2.one * (72 * scale);
            // 누를 때 검지 끝이 대상에 닿고, 돌아올 때 오른쪽 아래로 조금 떨어진다.
            hand.rectTransform.anchoredPosition = contact + new Vector2(1, -1).normalized * (8 * scale * (1 - press));
            hand.rectTransform.localScale = Vector3.one * (1 - 0.1f * press);
            hand.color = Color.white;
        }

        void SetImagesVisible(bool visible)
        {
            if (hand != null) hand.gameObject.SetActive(visible);
            if (ghost != null) ghost.gameObject.SetActive(visible);
        }
    }
}
