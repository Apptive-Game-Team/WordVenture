using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Cards
{
    // 손패가 가득 찬 채 뽑은 카드를 잠깐 보여 준 뒤 태워 없앤다. 호출한 쪽은 기다리지 않는다.
    // 불타는 선이 카드 위쪽에서 아래쪽으로 내려가며, 선 위쪽은 SpriteMask로 잘려 사라진다.
    // 카드는 손패 목록에 들어가지 않고, 시작하는 순간 콜라이더와 태그를 지워 잡거나 조합창에 놓을 수 없다.
    public sealed class CardBurnVfx : MonoBehaviour
    {
        const float ReferenceScreenHeight = 1080f;
        const float FlySeconds = 0.3f;
        const float BurnSeconds = 1.1f;
        // 그림 한 칸은 화면 4픽셀이다.
        const float ScreenPixelsPerArtPixel = 4f;
        const float ParticlesPerSecond = 90f;
        const int SortingBoost = 1000;
        const int ParticleSortingOrder = 3000;
        const int MaxParticles = 60;
        // 불꽃 띠: 선 아래 그을린 3칸 + 선 위 불꽃 4칸.
        const int EdgeRowsBelow = 3;
        const int EdgeRowsAbove = 4;
        const float EdgeFlickerSeconds = 0.06f;

        static readonly Color Ember = new Color(1f, 0.55f, 0.1f, 1f);
        static readonly Color Char = new Color(0.12f, 0.08f, 0.07f, 1f);

        static Sprite flameSprite;
        static Sprite sparkSprite;
        static Sprite maskSprite;

        struct Particle
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public Vector2 velocity;
            public float age;
            public float life;
            public Color color;
        }

        readonly List<Particle> particles = new List<Particle>();
        readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        readonly List<Color> spriteColors = new List<Color>();
        readonly List<TMP_Text> texts = new List<TMP_Text>();
        readonly List<Color> textColors = new List<Color>();
        Tween tween;
        bool burning;
        float emitCarry;
        float pixelUnit;
        // 카드 로컬 좌표 기준 몸통 범위. 선과 마스크는 카드의 자식이라 회전을 따라간다.
        Rect localRect;
        float localPixel;
        float lineTop;
        float lineBottom;
        int lineSteps;
        int sortingMin;
        int sortingMax;
        SpriteMask mask;
        SpriteRenderer edge;
        Texture2D edgeTexture;
        Sprite edgeSprite;
        Color32[] edgePixels;
        int[] edgeHeights;
        float edgeTimer;
        float lineY;
        bool lineReady;
        readonly List<float> textTop = new List<float>();
        readonly List<float> textBottom = new List<float>();

        // burnPosition까지 날아가 불타고 사라진다. delay는 같은 턴에 겹쳐 타는 카드를 어긋나게 한다.
        public static void Play(Card card, Vector3 burnPosition, float delay)
        {
            CardBurnVfx vfx = card.gameObject.AddComponent<CardBurnVfx>();
            vfx.Begin(card, burnPosition, delay);
        }

        void Begin(Card card, Vector3 burnPosition, float delay)
        {
            // 상호작용을 먼저 끊는다. 날아가는 동안에도 카드는 잡히면 안 된다.
            foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
                collider.enabled = false;
            gameObject.tag = "Untagged";
            card.enabled = false;

            Camera camera = Camera.main;
            // 실제 창 높이가 아니라 1080 기준 화면으로 계산한다. 창 크기와 상관없이 그림 한 칸이 가상 화면(480x270)의 한 칸이 된다.
            float worldPerScreenPixel = camera != null ? camera.orthographicSize * 2f / ReferenceScreenHeight : 0.01f;
            pixelUnit = worldPerScreenPixel * ScreenPixelsPerArtPixel;

            // 뽑자마자 타는 카드는 손패에 들어가지 않아 Order 가 순서를 정한 적이 없다. 그대로 두면 글자와 그림이
            // 같은 순서라 글자가 그림 뒤에 그려질 수 있으므로 먼저 손패와 같은 순서를 매긴다.
            Order order = GetComponent<Order>();
            if (order != null) order.SetOrder(0);

            // 손패 위로 올려 그린다. 상대 순서는 유지해 글자가 그림 위에 남는다.
            sortingMin = int.MaxValue;
            sortingMax = int.MinValue;
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.sortingOrder += SortingBoost;
                var sprite = renderer as SpriteRenderer;
                if (sprite != null)
                {
                    sprites.Add(sprite);
                    spriteColors.Add(sprite.color);
                    sortingMin = Mathf.Min(sortingMin, sprite.sortingOrder);
                    sortingMax = Mathf.Max(sortingMax, sprite.sortingOrder);
                }
            }
            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>())
            {
                texts.Add(text);
                textColors.Add(text.color);
            }

            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.AppendInterval(delay);
            sequence.Append(transform.DOMove(burnPosition, FlySeconds).SetEase(Ease.OutCubic));
            sequence.Join(transform.DOScale(Vector3.one * 0.5f, FlySeconds));
            sequence.AppendCallback(StartBurn);
            sequence.Append(DOTween.To(() => 0f, ApplyBurn, 1f, BurnSeconds).SetEase(Ease.Linear));
            sequence.OnComplete(Finish);
            tween = sequence;
        }

        // 날아오기가 끝나 크기가 정해진 뒤에 마스크와 불꽃 띠를 만든다.
        void StartBurn()
        {
            burning = true;
            var body = GetComponent<SpriteRenderer>();
            if (sprites.Count == 0 || body == null || body.sprite == null) return;

            // 카드 크기가 바뀐 만큼 로컬 단위를 보정해 그림 한 칸이 화면 4픽셀로 남게 한다.
            float scale = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
            localPixel = pixelUnit / scale;
            Bounds bounds = body.sprite.bounds;
            localRect = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
            lineTop = localRect.yMax + localPixel * EdgeRowsAbove;
            lineBottom = localRect.yMin - localPixel;
            lineSteps = Mathf.Max(1, Mathf.CeilToInt((lineTop - lineBottom) / localPixel));
            lineY = lineTop;

            // 마스크 범위를 이 카드의 정렬 순서로 좁혀 다른 카드와 배경이 잘리지 않게 한다.
            var maskObject = new GameObject("BurnMask");
            maskObject.transform.SetParent(transform, false);
            mask = maskObject.AddComponent<SpriteMask>();
            mask.sprite = MaskSprite();
            mask.isCustomRangeActive = true;
            mask.frontSortingLayerID = body.sortingLayerID;
            mask.backSortingLayerID = body.sortingLayerID;
            mask.frontSortingOrder = sortingMax;
            mask.backSortingOrder = sortingMin - 1;
            foreach (SpriteRenderer sprite in sprites)
                sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            BuildEdge(body);
            for (int i = 0; i < texts.Count; i++)
            {
                Bounds textBounds = texts[i].GetComponent<Renderer>().bounds;
                float top = transform.InverseTransformPoint(new Vector3(textBounds.center.x, textBounds.max.y, textBounds.center.z)).y;
                float bottom = transform.InverseTransformPoint(new Vector3(textBounds.center.x, textBounds.min.y, textBounds.center.z)).y;
                textTop.Add(Mathf.Max(top, bottom));
                textBottom.Add(Mathf.Min(top, bottom));
            }
            lineReady = true;
            SetLine(0f);
        }

        void BuildEdge(SpriteRenderer body)
        {
            int width = Mathf.Max(1, Mathf.CeilToInt(localRect.width / localPixel));
            int height = EdgeRowsBelow + EdgeRowsAbove;
            edgeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            edgePixels = new Color32[width * height];
            edgeHeights = new int[width];
            // 선이 띠의 아래에서 EdgeRowsBelow칸 위에 오도록 피벗을 잡는다.
            edgeSprite = Sprite.Create(edgeTexture, new Rect(0, 0, width, height),
                new Vector2(0.5f, EdgeRowsBelow / (float)height), 1f);
            var edgeObject = new GameObject("BurnEdge");
            edgeObject.transform.SetParent(transform, false);
            edgeObject.transform.localScale = new Vector3(localPixel, localPixel, 1f);
            edge = edgeObject.AddComponent<SpriteRenderer>();
            edge.sprite = edgeSprite;
            edge.sortingLayerID = body.sortingLayerID;
            edge.sortingOrder = sortingMax + 1;
            RefreshEdge();
        }

        // 불꽃 윗면이 들쭉날쭉하게 일렁이도록 열마다 높이를 조금씩 바꾼다.
        void RefreshEdge()
        {
            int width = edgeTexture.width;
            int height = edgeTexture.height;
            for (int x = 0; x < width; x++)
            {
                int previous = x > 0 ? edgeHeights[x - 1] : EdgeRowsAbove - 2;
                edgeHeights[x] = Mathf.Clamp(previous + Random.Range(-1, 2), 0, EdgeRowsAbove - 1);
                if (Random.value < 0.15f) edgeHeights[x] = Random.Range(0, EdgeRowsAbove);
                for (int y = 0; y < height; y++)
                    edgePixels[y * width + x] = EdgePixel(y, edgeHeights[x]);
            }
            edgeTexture.SetPixels32(edgePixels);
            edgeTexture.Apply();
        }

        // y는 아래에서 센 행. 0~2는 그을린 종이, 3은 선 바로 위 불씨, 4 이상은 위로 갈수록 붉어지는 불꽃.
        static Color32 EdgePixel(int y, int flameHeight)
        {
            switch (y)
            {
                case 0: return new Color32(30, 20, 18, 90);
                case 1: return new Color32(70, 28, 10, 170);
                case 2: return new Color32(255, 140, 30, 255);
                case 3: return new Color32(255, 230, 110, 255);
            }
            int flameRow = y - EdgeRowsBelow - 1;
            if (flameRow >= flameHeight) return new Color32(0, 0, 0, 0);
            return flameRow == 0
                ? new Color32(255, 190, 60, 255)
                : new Color32(255, 100, 25, 255);
        }

        void ApplyBurn(float t)
        {
            if (!lineReady) return;
            SetLine(t);
            // 종이가 달아오르는 정도만 색으로 남긴다. 사라지는 것은 마스크가 맡는다.
            float heat = Mathf.Clamp01(t / 0.5f) * 0.35f;
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] == null) continue;
                sprites[i].color = Color.Lerp(spriteColors[i], Ember, heat);
            }
            // TextMeshPro 글자는 SpriteMask로 잘리지 않아, 선이 글자를 지나는 동안 투명해지게 한다.
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i] == null) continue;
                float height = Mathf.Max(textTop[i] - textBottom[i], localPixel * 2f);
                float visible = Mathf.Clamp01((lineY - textBottom[i]) / height);
                Color color = Color.Lerp(Char, textColors[i], visible);
                color.a = textColors[i].a * visible;
                texts[i].color = color;
            }
        }

        // 선은 그림 한 칸 단위로만 움직여 잘린 가장자리가 계단처럼 내려간다.
        void SetLine(float t)
        {
            int step = Mathf.Min(lineSteps, Mathf.FloorToInt(t * lineSteps));
            lineY = lineTop - step * localPixel;
            float bottom = localRect.yMin - localRect.height;
            mask.transform.localPosition = new Vector3(localRect.center.x, bottom, 0f);
            mask.transform.localScale = new Vector3(localRect.width * 1.5f, Mathf.Max(0f, lineY - bottom), 1f);
            edge.transform.localPosition = new Vector3(localRect.center.x, lineY, 0f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (burning && lineReady) Emit(dt);
            if (edge != null)
            {
                edgeTimer += dt;
                if (edgeTimer >= EdgeFlickerSeconds)
                {
                    edgeTimer = 0f;
                    RefreshEdge();
                }
            }
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle p = particles[i];
                p.age += dt;
                if (p.age >= p.life)
                {
                    Destroy(p.transform.gameObject);
                    particles.RemoveAt(i);
                    continue;
                }
                float k = p.age / p.life;
                // 위로 올라가며 주황에서 어두운 붉은색으로 식고 줄어든다.
                p.transform.position += (Vector3)(p.velocity * dt);
                Color color = Color.Lerp(p.color, new Color(0.5f, 0.1f, 0.05f, 1f), k);
                color.a = 1f - k * k;
                p.renderer.color = color;
                particles[i] = p;
            }
        }

        void Emit(float dt)
        {
            emitCarry += ParticlesPerSecond * dt;
            while (emitCarry >= 1f && particles.Count < MaxParticles)
            {
                emitCarry -= 1f;
                bool flame = Random.value < 0.45f;
                // 선 위의 임의 위치에서 올라온다. 선이 카드 위쪽 바깥에 있을 때는 몸통 안쪽으로 맞춘다.
                float emitY = Mathf.Min(lineY, localRect.yMax);
                Vector3 origin = transform.TransformPoint(new Vector3(
                    Random.Range(localRect.xMin, localRect.xMax), emitY, 0f));
                var go = new GameObject(flame ? "Flame" : "Ember");
                go.transform.position = origin;
                // 그림 한 칸이 화면 4픽셀이 되도록 스프라이트 한 칸을 pixelUnit으로 맞춘다.
                go.transform.localScale = Vector3.one * pixelUnit;
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = flame ? FlameSprite() : SparkSprite();
                renderer.sortingOrder = ParticleSortingOrder;
                Color color = flame
                    ? Color.Lerp(new Color(1f, 0.8f, 0.2f), Ember, Random.value)
                    : Color.Lerp(Ember, new Color(1f, 0.3f, 0.1f), Random.value);
                renderer.color = color;
                float speed = flame ? Random.Range(1.2f, 2.2f) : Random.Range(0.6f, 2.8f);
                particles.Add(new Particle
                {
                    transform = go.transform,
                    renderer = renderer,
                    velocity = new Vector2(Random.Range(-0.5f, 0.5f), speed),
                    life = flame ? Random.Range(0.3f, 0.5f) : Random.Range(0.4f, 0.8f),
                    color = color
                });
            }
        }

        void Finish()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            tween?.Kill();
            foreach (Particle p in particles)
                if (p.transform != null) Destroy(p.transform.gameObject);
            particles.Clear();
            if (edgeSprite != null) Destroy(edgeSprite);
            if (edgeTexture != null) Destroy(edgeTexture);
        }

        // 5x7 불꽃. 아래가 넓고 위가 뾰족하다.
        static Sprite FlameSprite()
        {
            if (flameSprite != null) return flameSprite;
            string[] rows =
            {
                "..#..",
                "..#..",
                ".###.",
                ".###.",
                "#####",
                "#####",
                ".###.",
            };
            flameSprite = CreateSprite(rows);
            return flameSprite;
        }

        // 3x3 불똥.
        static Sprite SparkSprite()
        {
            if (sparkSprite != null) return sparkSprite;
            string[] rows =
            {
                ".#.",
                "###",
                ".#.",
            };
            sparkSprite = CreateSprite(rows);
            return sparkSprite;
        }

        // 카드 몸통 크기로 늘려 쓰는 1x1 마스크. 피벗이 아래 가운데라 y 배율이 곧 보이는 높이다.
        static Sprite MaskSprite()
        {
            if (maskSprite != null) return maskSprite;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            maskSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0f), 1f);
            return maskSprite;
        }

        // 위 행부터 적은 문자 그림을 한 칸이 1 unit인 스프라이트로 만든다.
        static Sprite CreateSprite(string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                string row = rows[height - 1 - y];
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = row[x] == '#' ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
