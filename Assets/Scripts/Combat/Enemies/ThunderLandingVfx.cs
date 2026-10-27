using UnityEngine;

namespace Combat.Enemies
{
    // 후방 도약의 착지점에 먼지와 전기 불티를 짧게 남긴다. 피해 판정은 없다.
    public sealed class ThunderLandingVfx : MonoBehaviour
    {
        const float Duration = 0.35f;
        const int Count = 16;
        static Sprite pixel;
        readonly SpriteRenderer[] pieces = new SpriteRenderer[Count];
        float elapsed;
        float width;

        public static void Spawn(SpriteRenderer body)
        {
            if (body == null || body.sprite == null) return;
            Bounds visible = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
            Vector3 floor = visible.center;
            floor.y = body.flipY ? -visible.max.y : visible.min.y;
            if (body.flipX) floor.x = -floor.x;
            var go = new GameObject("ThunderLandingVfx");
            go.transform.position = body.transform.TransformPoint(floor);
            var effect = go.AddComponent<ThunderLandingVfx>();
            effect.width = visible.size.x * Mathf.Abs(body.transform.lossyScale.x);
            if (pixel == null)
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            }
            for (int i = 0; i < Count; i++)
            {
                var child = new GameObject(i < 8 ? "Dust" : "ElectricSpark");
                child.transform.SetParent(go.transform, false);
                var renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = pixel;
                renderer.sortingLayerID = body.sortingLayerID;
                renderer.sortingOrder = body.sortingOrder + 12;
                effect.pieces[i] = renderer;
            }
            effect.Draw(0);
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= Duration) { Destroy(gameObject); return; }
            Draw(elapsed / Duration);
        }

        void Draw(float progress)
        {
            for (int i = 0; i < Count; i++)
            {
                bool dust = i < 8;
                int index = i % 8;
                float side = index % 2 == 0 ? -1 : 1;
                float spread = (0.15f + index * 0.045f + progress * 0.5f) * width;
                float y = dust ? Mathf.Sin(progress * Mathf.PI) * width * 0.11f
                    : (0.12f + index * 0.035f) * width * (1 - progress);
                pieces[i].transform.localPosition = new Vector3(side * spread, y, 0);
                pieces[i].transform.localRotation = Quaternion.Euler(0, 0, dust ? 0 : (index % 3 - 1) * 45);
                float size = width * (dust ? 0.08f : 0.035f) * (1 - progress * 0.5f);
                pieces[i].transform.localScale = new Vector3(size, dust ? size : size * 3, 1);
                pieces[i].color = dust ? new Color(0.7f, 0.62f, 0.48f, 1 - progress)
                    : new Color(0.75f, 0.9f, 1f, 1 - progress);
            }
        }
    }
}
