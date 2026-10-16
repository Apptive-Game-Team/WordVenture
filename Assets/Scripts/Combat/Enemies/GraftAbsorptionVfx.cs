using UnityEngine;

namespace Combat.Enemies
{
    // 흡수 판정과 분리한 그림 사본. 대상은 풀로 돌아가도 몸이 보스에게 끌려가는 동작은 끝까지 보인다.
    public sealed class GraftAbsorptionVfx : MonoBehaviour
    {
        public const float Duration = 1.05f;
        static Sprite pixel;
        Enemy owner;
        SpriteRenderer absorbed, glow;
        readonly SpriteRenderer[] motes = new SpriteRenderer[24];
        Vector3 origin, sourceScale, sourceCenter;
        Color sourceColor;
        float elapsed;
        public Transform AbsorbedBody => absorbed != null ? absorbed.transform : null;
        public float Progress => Mathf.Clamp01(elapsed / Duration);

        public static GraftAbsorptionVfx Begin(Enemy owner, Component target)
        {
            var source = target.GetComponent<SpriteRenderer>();
            if (source == null || source.sprite == null) return null;
            var root = new GameObject("GraftAbsorptionVfx");
            var effect = root.AddComponent<GraftAbsorptionVfx>();
            effect.owner = owner;
            effect.origin = EnemyMagicVisual.Center(target);
            effect.sourceScale = target.transform.lossyScale;
            effect.sourceColor = source.color;
            effect.sourceCenter = ElementalStatusVfx.GetVisibleBodyBounds(source.sprite).center;
            if (source.flipX) effect.sourceCenter.x *= -1;
            if (source.flipY) effect.sourceCenter.y *= -1;
            effect.absorbed = effect.Part("AbsorbedSlime", source.sprite, 61);
            effect.absorbed.flipX = source.flipX;
            effect.absorbed.flipY = source.flipY;
            effect.glow = effect.Part("AbsorptionGlow", owner.GetComponent<SpriteRenderer>().sprite, 62);
            if (pixel == null)
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply(false, true);
                pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 27);
            }
            for (int i = 0; i < effect.motes.Length; i++) effect.motes[i] = effect.Part("InwardMote", pixel, 60);
            effect.Render(0);
            return effect;
        }
        SpriteRenderer Part(string name, Sprite sprite, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }
        void Update()
        {
            if (owner == null || !owner.IsAlive) { Destroy(gameObject); return; }
            elapsed += Time.deltaTime;
            Render(Progress);
            if (elapsed >= Duration) Destroy(gameObject);
        }
        void Render(float progress)
        {
            Vector3 destination = EnemyMagicVisual.Center(owner);
            float travel = Mathf.Clamp01((progress - .1f) / .75f);
            float acceleration = travel * travel;
            Vector3 point = Vector3.Lerp(origin, destination, acceleration) + Vector3.up * (Mathf.Sin(travel * Mathf.PI) * .6f);
            float shrink = Mathf.Lerp(1, .08f, acceleration);
            absorbed.transform.localScale = sourceScale * shrink;
            absorbed.transform.position = point - Vector3.Scale(sourceCenter, sourceScale) * shrink;
            absorbed.color = new Color(sourceColor.r, sourceColor.g, sourceColor.b, Mathf.Clamp01((1 - travel) * 5));
            // 작은 빛은 대상에서 보스로 흐르고, 안쪽 고리는 점점 좁아진다.
            for (int i = 0; i < motes.Length; i++)
            {
                SpriteRenderer mote = motes[i];
                if (i < 12)
                {
                    float phase = Mathf.Repeat(progress * 2 + i / 12f, 1);
                    Vector3 route = Vector3.Lerp(origin, destination, phase * phase);
                    route.y += Mathf.Sin(phase * Mathf.PI) * .45f;
                    mote.transform.position = route;
                    mote.transform.localScale = Vector3.one * (i % 3 == 0 ? 3 : 2);
                }
                else
                {
                    float angle = (i * 30 + progress * 540) * Mathf.Deg2Rad;
                    float radius = Mathf.Lerp(1.1f, .22f, progress);
                    mote.transform.position = destination + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * .65f, 0) * radius;
                    mote.transform.localScale = new Vector3(3, 2, 1);
                }
                mote.color = new Color(.75f, 1f, .3f, Mathf.Sin(progress * Mathf.PI));
            }
            var ownerBody = owner.GetComponent<SpriteRenderer>();
            glow.sprite = ownerBody.sprite;
            glow.flipX = ownerBody.flipX;
            glow.flipY = ownerBody.flipY;
            glow.transform.position = ownerBody.transform.position;
            glow.transform.localScale = ownerBody.transform.lossyScale;
            float pulse = Mathf.Sin(Mathf.Clamp01((progress - .72f) / .28f) * Mathf.PI);
            glow.color = new Color(.75f, 1f, .3f, pulse * .55f);
        }
    }
}
