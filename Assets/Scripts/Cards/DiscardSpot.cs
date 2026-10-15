using Core;
using Story;
using TMPro;
using UnityEngine;

namespace Cards
{
    // 손패 옆에 놓이는 버리기 자리. 카드를 끌어다 놓으면 CardManager가 그 카드를 태워 버린다.
    // 씬을 고치지 않고 코드로 만든다. 놓였는지는 콜라이더가 아니라 화면 영역으로 판단한다.
    public sealed class DiscardSpot : MonoBehaviour
    {
        const float ReferenceScreenHeight = 1080f;
        // 그림 한 칸은 화면 4픽셀이다.
        const float ScreenPixelsPerArtPixel = 4f;
        const int ArtWidth = 44;
        const int ArtHeight = 52;
        const int SortingOrder = 0;

        static readonly Color ActiveTint = Color.white;
        static readonly Color Hovered = new Color(1.25f, 1.1f, 0.9f, 1f);
        static readonly Color Disabled = new Color(0.45f, 0.45f, 0.45f, 0.8f);

        static Sprite panelSprite;

        SpriteRenderer panel;
        TMP_Text[] labels;
        Bounds bounds;

        static string T(string korean) => Localization.Translate(korean);

        public static DiscardSpot Create(Vector3 position, int sortingLayerId)
        {
            var spot = new GameObject("DiscardSpot").AddComponent<DiscardSpot>();
            spot.Build(position, sortingLayerId);
            return spot;
        }

        void Build(Vector3 position, int sortingLayerId)
        {
            Camera camera = Camera.main;
            // 실제 창 높이가 아니라 1080 기준 화면으로 계산한다. 창 크기와 상관없이 그림 한 칸이 가상 화면(480x270)의 한 칸이 된다.
            float worldPerScreenPixel = camera != null ? camera.orthographicSize * 2f / ReferenceScreenHeight : 0.01f;
            float pixelUnit = worldPerScreenPixel * ScreenPixelsPerArtPixel;

            transform.position = position;
            panel = gameObject.AddComponent<SpriteRenderer>();
            panel.sprite = PanelSprite();
            panel.sortingLayerID = sortingLayerId;
            panel.sortingOrder = SortingOrder;
            transform.localScale = Vector3.one * pixelUnit;
            bounds = new Bounds(position, new Vector3(ArtWidth * pixelUnit, ArtHeight * pixelUnit, 100f));

            labels = new[]
            {
                CreateLabel("Title", T("버리기"), new Vector2(0f, 6f), 20, new Color(1f, 0.93f, 0.75f)),
                CreateLabel("Hint", T("턴마다 1장"), new Vector2(0f, -8f), 14, new Color(0.85f, 0.75f, 0.6f))
            };
            SetState(true, false);
        }

        TMP_Text CreateLabel(string objectName, string text, Vector2 artOffset, float artSize, Color color)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(artOffset.x, artOffset.y, 0f);
            TextMeshPro label = go.AddComponent<TextMeshPro>();
            label.font = Resources.Load<DialogueWindowStyle>("Story/DialogueWindowStyle").font;
            label.text = text;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 10f;
            // 부모 스케일이 그림 한 칸 단위라 로컬 1 unit이 그림 한 칸이다. TMP 월드 글자는 크기 10이 약 1 unit이므로
            // 글자 높이를 그림 칸 수로 맞추려면 10을 곱한다. 폭에 넘치면 자동 크기가 줄인다.
            label.fontSizeMax = artSize * 8f;
            label.rectTransform.sizeDelta = new Vector2(ArtWidth - 8f, artSize + 4f);
            label.sortingLayerID = panel.sortingLayerID;
            label.sortingOrder = SortingOrder + 1;
            return label;
        }

        public bool Contains(Vector3 worldPosition)
        {
            worldPosition.z = bounds.center.z;
            return bounds.Contains(worldPosition);
        }

        public void SetState(bool canDiscard, bool hovered)
        {
            panel.color = !canDiscard ? Disabled : hovered ? Hovered : ActiveTint;
            foreach (TMP_Text label in labels)
            {
                Color color = label.color;
                color.a = canDiscard ? 1f : 0.45f;
                label.color = color;
            }
        }

        public Vector3 Center => bounds.center;

        // 어두운 나무 판. 바깥 테두리, 나무 바탕, 안쪽 어두운 홈, 모서리 못으로 이루어진다.
        static Sprite PanelSprite()
        {
            if (panelSprite != null) return panelSprite;
            Color32 outline = new Color32(0x2a, 0x1a, 0x12, 255);
            Color32 wood = new Color32(0x6b, 0x45, 0x2a, 255);
            Color32 woodLight = new Color32(0x86, 0x5a, 0x37, 255);
            Color32 groove = new Color32(0x3a, 0x24, 0x18, 255);
            Color32 nail = new Color32(0xc9, 0xa8, 0x6a, 255);
            var texture = new Texture2D(ArtWidth, ArtHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[ArtWidth * ArtHeight];
            for (int y = 0; y < ArtHeight; y++)
            {
                for (int x = 0; x < ArtWidth; x++)
                {
                    int edge = Mathf.Min(Mathf.Min(x, ArtWidth - 1 - x), Mathf.Min(y, ArtHeight - 1 - y));
                    bool corner = (x < 2 || x >= ArtWidth - 2) && (y < 2 || y >= ArtHeight - 2);
                    Color32 color;
                    if (corner) color = new Color32(0, 0, 0, 0);
                    else if (edge < 2) color = outline;
                    else if (edge < 4) color = (y > ArtHeight / 2) ? woodLight : wood;
                    else if (edge < 6) color = groove;
                    else color = (y % 6 == 0) ? wood : groove;
                    pixels[y * ArtWidth + x] = color;
                }
            }
            foreach (int nx in new[] { 3, ArtWidth - 4 })
                foreach (int ny in new[] { 3, ArtHeight - 4 })
                    pixels[ny * ArtWidth + nx] = nail;
            texture.SetPixels32(pixels);
            texture.Apply();
            panelSprite = Sprite.Create(texture, new Rect(0, 0, ArtWidth, ArtHeight), new Vector2(0.5f, 0.5f), 1f);
            return panelSprite;
        }
    }
}
