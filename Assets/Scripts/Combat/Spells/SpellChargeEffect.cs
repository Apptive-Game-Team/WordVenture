using Cards;
using Combat.Enemies;
using UnityEngine;

namespace Combat.Spells
{
    // 조합한 주문이 날아가기 전까지 워드의 지팡이 구슬에 원소 색 빛이 모이는 연출.
    // 씬에 따로 배치하지 않고 CombineZone이 처음 주문을 조합할 때 만든다.
    public class SpellChargeEffect : MonoBehaviour
    {
        // 워드 스프라이트(512px, 100 PPU, 중앙 피벗)에서 지팡이 구슬 중심의 로컬 좌표.
        static readonly Vector3 StaffOrbLocalPosition = new Vector3(1.79f, 1.41f, 0f);

        const int MoteCount = 12;
        const float MoteLifetime = 0.7f;
        const float MoteStartRadius = 0.9f;
        const float MoteSize = 0.12f;
        const float CoreSize = 0.55f;
        const float CoreGrowTime = 0.5f;
        const float ReleaseTime = 0.25f;
        const int SortingOrder = 5;

        static Sprite glowSprite;

        SpriteRenderer core;
        readonly SpriteRenderer[] motes = new SpriteRenderer[MoteCount];
        readonly float[] moteAngles = new float[MoteCount];
        readonly float[] moteAges = new float[MoteCount];

        Color color;
        float chargeTime;
        float releaseTime;
        bool isCharging;
        bool isReleasing;

        public static SpellChargeEffect Create()
        {
            var effect = new GameObject("SpellChargeEffect").AddComponent<SpellChargeEffect>();
            effect.Build();
            effect.gameObject.SetActive(false);
            return effect;
        }

        public void Begin(MagicType element)
        {
            color = GetElementColor(element);
            chargeTime = 0f;
            isCharging = true;
            isReleasing = false;

            for (int i = 0; i < MoteCount; i++)
            {
                moteAngles[i] = Random.Range(0f, 360f);
                // 처음부터 고르게 퍼져 있도록 나이를 엇갈려 둔다.
                moteAges[i] = MoteLifetime * i / MoteCount;
            }

            FollowStaff();
            gameObject.SetActive(true);
        }

        // 주문이 실제로 발사되는 순간 모인 빛을 한 번 터뜨리고 사라진다.
        public void Release()
        {
            if (!isCharging) return;
            isCharging = false;
            isReleasing = true;
            releaseTime = 0f;
            foreach (SpriteRenderer mote in motes)
            {
                mote.enabled = false;
            }
        }

        void Build()
        {
            if (glowSprite == null)
            {
                glowSprite = CreateGlowSprite();
            }

            core = CreateSprite("Core");
            for (int i = 0; i < MoteCount; i++)
            {
                motes[i] = CreateSprite("Mote");
            }
        }

        SpriteRenderer CreateSprite(string objectName)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            var spriteRenderer = child.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = glowSprite;
            spriteRenderer.sortingOrder = SortingOrder;
            return spriteRenderer;
        }

        void Update()
        {
            FollowStaff();

            if (isCharging)
            {
                UpdateCharging();
            }
            else if (isReleasing)
            {
                UpdateReleasing();
            }
        }

        void FollowStaff()
        {
            Player player = Player.PlayerInt();
            if (player != null)
            {
                transform.position = player.transform.TransformPoint(StaffOrbLocalPosition);
            }
        }

        void UpdateCharging()
        {
            chargeTime += Time.deltaTime;

            float grow = Mathf.Clamp01(chargeTime / CoreGrowTime);
            float pulse = 1f + 0.12f * Mathf.Sin(chargeTime * 8f);
            core.transform.localScale = Vector3.one * (CoreSize * grow * pulse);
            core.color = WithAlpha(color, 0.85f * grow);

            for (int i = 0; i < MoteCount; i++)
            {
                moteAges[i] += Time.deltaTime;
                if (moteAges[i] >= MoteLifetime)
                {
                    moteAges[i] -= MoteLifetime;
                    moteAngles[i] = Random.Range(0f, 360f);
                }

                // 바깥에서 구슬 쪽으로 빨려 들어가며 살짝 돈다.
                float progress = moteAges[i] / MoteLifetime;
                float radius = MoteStartRadius * (1f - progress * progress);
                float angle = (moteAges[i] * 120f + moteAngles[i]) * Mathf.Deg2Rad;

                SpriteRenderer mote = motes[i];
                mote.enabled = true;
                mote.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                mote.transform.localScale = Vector3.one * (MoteSize * (1f - 0.5f * progress));
                mote.color = WithAlpha(color, Mathf.Sin(progress * Mathf.PI));
            }
        }

        void UpdateReleasing()
        {
            releaseTime += Time.deltaTime;
            float progress = Mathf.Clamp01(releaseTime / ReleaseTime);
            core.transform.localScale = Vector3.one * (CoreSize * (1f + 1.5f * progress));
            core.color = WithAlpha(Color.Lerp(color, Color.white, 0.5f), 1f - progress);

            if (progress >= 1f)
            {
                isReleasing = false;
                gameObject.SetActive(false);
            }
        }

        static Color GetElementColor(MagicType element)
        {
            switch (element)
            {
                case MagicType.Fire: return new Color(1f, 0.45f, 0.15f);
                case MagicType.Ice: return new Color(0.55f, 0.9f, 1f);
                case MagicType.Rock: return new Color(0.75f, 0.55f, 0.3f);
                case MagicType.Lightning: return new Color(1f, 0.92f, 0.3f);
                case MagicType.Holy: return new Color(1f, 0.97f, 0.75f);
                case MagicType.Undead: return new Color(0.65f, 0.35f, 0.9f);
                default: return Color.white;
            }
        }

        static Color WithAlpha(Color source, float alpha)
        {
            source.a = alpha;
            return source;
        }

        // 가장자리가 부드럽게 흐려지는 원. 픽셀 아트 원소 빛에 쓸 별도 이미지가 없어 코드로 만든다.
        static Sprite CreateGlowSprite()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[size * size];
            float center = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = new Vector2(x - center, y - center).magnitude / center;
                    float alpha = Mathf.Clamp01(1f - distance);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255 * alpha * alpha));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            // pixelsPerUnit를 size로 두면 스프라이트 지름이 1 유닛이 되어 localScale이 곧 지름이다.
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
