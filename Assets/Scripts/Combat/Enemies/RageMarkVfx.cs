using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    // 적 머리 위 체력 숫자 위에 빨간 위쪽 화살표와 "x단계"를 도트로 보여 준다. 0단계에서는 숨긴다.
    // 그림 한 칸은 화면 4 픽셀이고 point filtering 이다. 적 배율이 커져도 아이콘 크기는 그대로 두려고 부모 배율을 상쇄한다.
    public sealed class RageMarkVfx : MonoBehaviour
    {
        const float ScreenPixelsPerUnit = 108f;
        const float ScreenPixelsPerArtPixel = 4f;
        const float ArtPixelWorldSize = ScreenPixelsPerArtPixel / ScreenPixelsPerUnit;
        // 체력 숫자 위쪽 여백(적 로컬 단위)이다.
        const float HpGap = 0.12f;
        const int SortingOffset = 22;
        const int MaxShownLevel = 99;
        const float PopStepSeconds = 0.07f;
        // 단계가 오를 때 아이콘이 튀어 오르는 높이다. 단위는 그림 칸이다.
        static readonly int[] PopLift = { 3, 5, 3, 1 };

        static readonly Color Fill = new Color32(226, 52, 44, 255);
        static readonly Color Shade = new Color32(150, 24, 30, 255);
        static readonly Color Digit = new Color32(255, 240, 200, 255);
        static readonly Color Outline = new Color32(40, 12, 16, 255);
        static readonly Color PopTint = new Color32(255, 255, 140, 255);

        // 위 화살표는 7x7이다. 'o' 는 기본색, 's' 는 그림자색이다.
        static readonly string[] ArrowRows =
        {
            "...o...",
            "..ooo..",
            ".ooooo.",
            "ooooooo",
            "..oos..",
            "..oos..",
            "..oos..",
        };
        static readonly string[] CrossRows = { "o.o", ".o.", "o.o" };
        static readonly string[][] DigitRows =
        {
            new[] { "ooo", "o.o", "o.o", "o.o", "ooo" },
            new[] { ".o.", "oo.", ".o.", ".o.", "ooo" },
            new[] { "ooo", "..o", "ooo", "o..", "ooo" },
            new[] { "ooo", "..o", "ooo", "..o", "ooo" },
            new[] { "o.o", "o.o", "ooo", "..o", "..o" },
            new[] { "ooo", "o..", "ooo", "..o", "ooo" },
            new[] { "ooo", "o..", "ooo", "o.o", "ooo" },
            new[] { "ooo", "..o", "..o", "..o", "..o" },
            new[] { "ooo", "o.o", "ooo", "o.o", "ooo" },
            new[] { "ooo", "o.o", "ooo", "..o", "ooo" },
        };

        static readonly Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();

        SpriteRenderer body;
        SpriteRenderer mark;
        TMP_Text hpText;
        float popElapsed = -1f;

        public int ShownLevel { get; private set; }

        public void Initialize(SpriteRenderer renderer)
        {
            body = renderer;
            if (body == null) { enabled = false; return; }
            hpText = GetComponentInChildren<TMP_Text>();
            var child = new GameObject("RageMark");
            child.transform.SetParent(transform, false);
            mark = child.AddComponent<SpriteRenderer>();
            mark.enabled = false;
        }

        public void Show(int level, bool pop)
        {
            ShownLevel = level;
            if (mark == null) return;
            if (level <= 0)
            {
                mark.enabled = false;
                popElapsed = -1f;
                return;
            }
            mark.sprite = GetSprite(Mathf.Min(level, MaxShownLevel));
            mark.enabled = true;
            popElapsed = pop ? 0f : -1f;
            Place();
        }

        void LateUpdate()
        {
            if (mark == null || !mark.enabled) return;
            Place();
        }

        void Place()
        {
            if (body.sprite == null) return;
            Bounds visibleBody = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
            float x = body.flipX ? -visibleBody.center.x : visibleBody.center.x;
            float y = visibleBody.max.y;
            if (hpText != null)
            {
                Vector3 hpTop = transform.InverseTransformPoint(hpText.transform.position);
                float hpHalf = hpText.textBounds.extents.y * Mathf.Abs(hpText.transform.lossyScale.y)
                    / Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
                y = Mathf.Max(y, hpTop.y + hpHalf);
            }

            Vector3 parentScale = transform.lossyScale;
            float inverseX = Inverse(parentScale.x);
            float inverseY = Inverse(parentScale.y);
            float lift = 0f;
            Color tint = Color.white;
            if (popElapsed >= 0f)
            {
                int step = (int)(popElapsed / PopStepSeconds);
                if (step >= PopLift.Length) popElapsed = -1f;
                else
                {
                    lift = PopLift[step] * ArtPixelWorldSize;
                    if (step < 2) tint = PopTint;
                    popElapsed += Time.unscaledDeltaTime;
                }
            }
            // 위로 올리는 양은 월드 단위라서 부모 y 배율로 나눈 만큼 로컬 위치에 더한다.
            mark.transform.localPosition = new Vector3(x, y + HpGap + Mathf.Abs(inverseY) * lift, 0f);
            // 부모 배율이 음수여도 아이콘이 뒤집히지 않게 하고, 그림 한 칸이 화면 4 픽셀이 되게 한다.
            mark.transform.localScale = new Vector3(inverseX, inverseY, 1f);
            mark.color = tint;
            mark.sortingLayerID = body.sortingLayerID;
            mark.sortingOrder = body.sortingOrder + SortingOffset;
        }

        void OnDisable()
        {
            popElapsed = -1f;
        }

        static float Inverse(float scale)
        {
            return Mathf.Abs(scale) < 0.0001f ? 1f : 1f / scale;
        }

        public static Sprite GetSprite(int level)
        {
            if (sprites.TryGetValue(level, out Sprite cached) && cached != null) return cached;
            Sprite made = Build(level);
            sprites[level] = made;
            return made;
        }

        static Sprite Build(int level)
        {
            string digits = level.ToString();
            // 화살표 | 빈칸 | x | 빈칸 | 숫자들(사이 빈칸 1). 바깥 테두리 때문에 사방에 1칸씩 더한다.
            int contentWidth = 7 + 1 + 3 + 1 + digits.Length * 3 + (digits.Length - 1);
            int width = contentWidth + 2;
            int height = 9;
            var cells = new Color?[width, height];
            Stamp(cells, ArrowRows, 1, 1, Fill, Shade);
            int cursor = 1 + 7 + 1;
            // 글자 줄은 화살표 높이 7칸의 가운데에 놓는다.
            Stamp(cells, CrossRows, cursor, 1 + 2, Digit, Digit);
            cursor += 3 + 1;
            foreach (char c in digits)
            {
                Stamp(cells, DigitRows[c - '0'], cursor, 1 + 1, Digit, Digit);
                cursor += 3 + 1;
            }
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = new Color32(0, 0, 0, 0);
                    if (cells[x, y].HasValue) pixel = cells[x, y].Value;
                    else if (TouchesFilled(cells, x, y, width, height)) pixel = Outline;
                    pixels[y * width + x] = pixel;
                }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "RageMark" + level,
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            float pixelsPerUnit = ScreenPixelsPerUnit / ScreenPixelsPerArtPixel;
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        // rows 는 위에서 아래로 적혀 있고 텍스처는 아래에서 위로 채우므로 뒤집어서 찍는다. 's' 는 그림자색이다.
        static void Stamp(Color?[,] cells, string[] rows, int left, int bottom, Color main, Color shade)
        {
            for (int row = 0; row < rows.Length; row++)
                for (int column = 0; column < rows[row].Length; column++)
                {
                    char c = rows[row][column];
                    if (c == 'o') cells[left + column, bottom + rows.Length - 1 - row] = main;
                    else if (c == 's') cells[left + column, bottom + rows.Length - 1 - row] = shade;
                }
        }

        static bool TouchesFilled(Color?[,] cells, int x, int y, int width, int height)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    if (cells[nx, ny].HasValue) return true;
                }
            return false;
        }
    }
}
