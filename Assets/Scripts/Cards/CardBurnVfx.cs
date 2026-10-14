using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Cards
{
    // 손패가 가득 찬 채 뽑은 카드를 잠깐 보여 준 뒤 태워 없앤다. 호출한 쪽은 기다리지 않는다.
    // 카드는 손패 목록에 들어가지 않고, 시작하는 순간 콜라이더와 태그를 지워 잡거나 조합창에 놓을 수 없다.
    public sealed class CardBurnVfx : MonoBehaviour
    {
        const float ReferenceScreenHeight = 1080f;
        const float FlySeconds = 0.3f;
        const float BurnSeconds = 0.6f;
        // 그림 한 칸은 화면 4픽셀이다.
        const float ScreenPixelsPerArtPixel = 4f;
        const float ParticlesPerSecond = 70f;
        const int SortingBoost = 1000;
        const int ParticleSortingOrder = 3000;
        const int MaxParticles = 60;

        static readonly Color Ember = new Color(1f, 0.55f, 0.1f, 1f);
        static readonly Color Char = new Color(0.12f, 0.08f, 0.07f, 1f);

        static Sprite flameSprite;
        static Sprite sparkSprite;

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
        Vector2 cardHalfSize;

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

            // 손패 위로 올려 그린다. 상대 순서는 유지해 글자가 그림 위에 남는다.
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.sortingOrder += SortingBoost;
                var sprite = renderer as SpriteRenderer;
                if (sprite != null)
                {
                    sprites.Add(sprite);
                    spriteColors.Add(sprite.color);
                }
            }
            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>())
            {
                texts.Add(text);
                textColors.Add(text.color);
            }

            var body = GetComponent<SpriteRenderer>();
            cardHalfSize = body != null && body.sprite != null
                ? (Vector2)body.bounds.extents
                : new Vector2(0.5f, 0.9f);

            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.AppendInterval(delay);
            sequence.Append(transform.DOMove(burnPosition, FlySeconds).SetEase(Ease.OutCubic));
            sequence.Join(transform.DOScale(Vector3.one * 0.5f, FlySeconds));
            sequence.AppendCallback(() => burning = true);
            sequence.Append(DOTween.To(() => 0f, ApplyBurn, 1f, BurnSeconds).SetEase(Ease.Linear));
            sequence.OnComplete(Finish);
            tween = sequence;
        }

        void ApplyBurn(float t)
        {
            // 앞 40%는 주황으로 달아오르고, 이후 검게 그을리며 뒤 절반에 걸쳐 사라진다.
            float heat = Mathf.Clamp01(t / 0.4f);
            float charred = Mathf.Clamp01((t - 0.3f) / 0.5f);
            float fade = 1f - Mathf.Clamp01((t - 0.5f) / 0.5f);
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] == null) continue;
                sprites[i].color = Tint(spriteColors[i], heat, charred, fade);
            }
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i] == null) continue;
                texts[i].color = Tint(textColors[i], heat, charred, fade);
            }
        }

        static Color Tint(Color original, float heat, float charred, float fade)
        {
            Color color = Color.Lerp(original, Ember, heat * 0.8f);
            color = Color.Lerp(color, Char, charred);
            color.a = original.a * fade;
            return color;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (burning) Emit(dt);
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
                Vector3 origin = transform.position + new Vector3(
                    Random.Range(-cardHalfSize.x, cardHalfSize.x) * 0.5f,
                    Random.Range(-cardHalfSize.y, cardHalfSize.y) * 0.5f, 0f);
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
