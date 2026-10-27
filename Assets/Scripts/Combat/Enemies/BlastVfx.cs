using UnityEngine;

namespace Combat.Enemies
{
    // 피해 판정과 분리된 폭발. 몸 중심의 불꽃과 방사형 불티는 적이 꺼진 뒤에도 끝까지 남는다.
    public sealed class BlastVfx : MonoBehaviour
    {
        const float FrameSeconds = 0.1f;
        const int SortingOffset = 10;
        const int SparkCount = 16;
        static Sprite pixel;
        Sprite[] frames;
        SpriteRenderer render;
        SpriteRenderer flash;
        readonly SpriteRenderer[] sparks = new SpriteRenderer[SparkCount];
        readonly SpriteRenderer[] dust = new SpriteRenderer[10];
        Vector3 floor;
        float elapsed;
        float artWidth;
        public float Diameter { get; private set; }

        // diameter는 효과의 가로 길이이고 피해 반경을 바꾸지 않는다.
        public static BlastVfx Spawn(Vector3 position, Sprite[] frames, float diameter, float frameWidth, SpriteRenderer body)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null) return null;
            var go = new GameObject("BlastVfx");
            go.transform.position = position;
            float visibleWidth = 0;
            foreach (Sprite frame in frames) if (frame != null)
                visibleWidth = Mathf.Max(visibleWidth, EnemyMagicVisual.Catalog.VisibleBounds(frame).size.x);
            float scale = diameter / Mathf.Max(0.01f, visibleWidth > 0 ? visibleWidth : frameWidth);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var vfx = go.AddComponent<BlastVfx>();
            vfx.Diameter = diameter;
            vfx.artWidth = visibleWidth > 0 ? visibleWidth : frameWidth;
            vfx.frames = frames;
            // 폭발 원점은 몸 중심에 고정한다. 원본 프레임의 피벗/여백 때문에 발밑으로 내려가지 않는다.
            vfx.render = vfx.CreateLayer("Fire", body, frames[0], 0);
            vfx.render.transform.localPosition = -EnemyMagicVisual.Catalog.VisibleBounds(frames[0]).center;
            vfx.flash = vfx.CreateLayer("Flash", body, Pixel, 2);
            for (int i = 0; i < SparkCount; i++)
                vfx.sparks[i] = vfx.CreateLayer("Spark", body, Pixel, 1);
            if (body != null && body.sprite != null)
            {
                Bounds visible = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
                Vector3 localFloor = visible.center;
                localFloor.y = body.flipY ? -visible.max.y : visible.min.y;
                if (body.flipX) localFloor.x = -localFloor.x;
                vfx.floor = go.transform.InverseTransformPoint(body.transform.TransformPoint(localFloor));
            }
            for (int i = 0; i < vfx.dust.Length; i++)
                vfx.dust[i] = vfx.CreateLayer("GroundDust", body, Pixel, -1);
            vfx.RenderBurst(0f);
            return vfx;
        }

        static Sprite Pixel
        {
            get
            {
                if (pixel != null) return pixel;
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
                return pixel;
            }
        }

        SpriteRenderer CreateLayer(string name, SpriteRenderer body, Sprite sprite, int offset)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var layer = child.AddComponent<SpriteRenderer>();
            layer.sprite = sprite;
            layer.sortingLayerID = body != null ? body.sortingLayerID : 0;
            layer.sortingOrder = (body != null ? body.sortingOrder : 0) + SortingOffset + offset;
            return layer;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            int index = (int)(elapsed / FrameSeconds);
            if (index >= frames.Length) { Destroy(gameObject); return; }
            render.sprite = frames[index];
            render.transform.localPosition = -EnemyMagicVisual.Catalog.VisibleBounds(frames[index]).center;
            RenderBurst(elapsed / (frames.Length * FrameSeconds));
        }

        void RenderBurst(float progress)
        {
            float expansion = 1f - Mathf.Pow(1f - progress, 3);
            flash.transform.localScale = Vector3.one * artWidth * Mathf.Lerp(0.15f, 0.45f, Mathf.Clamp01(progress * 6));
            flash.transform.localRotation = Quaternion.Euler(0, 0, 45);
            flash.color = new Color(1f, 1f, 0.8f, Mathf.Clamp01(1f - progress * 5));
            for (int i = 0; i < dust.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / dust.Length;
                dust[i].transform.localPosition = floor + new Vector3(Mathf.Cos(angle) * expansion * 0.55f * artWidth,
                    Mathf.Sin(angle) * expansion * 0.06f * artWidth, 0);
                float size = Mathf.Lerp(0.045f, 0.08f, progress) * artWidth;
                dust[i].transform.localScale = new Vector3(size * 1.5f, size * 0.45f, 1);
                dust[i].color = new Color(0.8f, 0.6f, 0.3f, (1f - progress) * 0.8f);
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / sparks.Length;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                SpriteRenderer spark = sparks[i];
                spark.transform.localPosition = direction * artWidth * Mathf.Lerp(0.07f, 0.5f, expansion);
                spark.transform.localRotation = Quaternion.Euler(0, 0, i * 360f / sparks.Length);
                float length = Mathf.Lerp(0.08f, 0.025f, progress) * artWidth;
                spark.transform.localScale = new Vector3(length, 0.024f * artWidth, 1);
                spark.color = Color.Lerp(new Color(1f, 0.95f, 0.5f), new Color(1f, 0.3f, 0.05f, 0), progress);
            }
        }
    }
}
