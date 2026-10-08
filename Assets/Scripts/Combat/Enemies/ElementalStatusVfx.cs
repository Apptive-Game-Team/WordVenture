using System.Collections.Generic;
using UnityEngine;

namespace Combat.Enemies
{
    // 같은 위치에 그리되 적 본체보다 앞에 렌더링한다. 머리 위 상태 아이콘이 아니다.
    public sealed class ElementalStatusVfx : MonoBehaviour
    {
        const int Columns = 4;
        const int Rows = 5;
        const float FrameSeconds = 0.15f;
        const float WidthScale = 1.35f;
        const float HeightScale = 1.4f;
        static Sprite[,] frames;
        static Vector2[] visibleSizes;
        static readonly Dictionary<Sprite, Bounds> bodyBounds = new Dictionary<Sprite, Bounds>();
        readonly SpriteRenderer[] overlays = new SpriteRenderer[Rows];
        Enemy enemy;
        SpriteRenderer body;

        public void Initialize(Enemy owner, SpriteRenderer renderer)
        {
            enemy = owner;
            body = renderer;
            if (body == null) { enabled = false; return; }
            if (frames == null)
            {
                Texture2D atlas = Resources.Load<Texture2D>("Combat/ElementalStatusOverlay");
                if (atlas == null) { enabled = false; return; }
                frames = new Sprite[Rows, Columns];
                visibleSizes = new Vector2[Rows];
                float width = atlas.width / (float)Columns;
                float height = atlas.height / (float)Rows;
                Color32[] pixels = atlas.GetPixels32();
                for (int row = 0; row < Rows; row++)
                {
                    // 행별 투명 여백을 제외해 공통 피벗을 정하고 프레임 간 흔들림을 막는다.
                    int minX = (int)width, minY = (int)height, maxX = 0, maxY = 0;
                    for (int col = 0; col < Columns; col++)
                        for (int y = 0; y < (int)height; y++)
                            for (int x = 0; x < (int)width; x++)
                            {
                                int px = (int)(col * width) + x;
                                int py = (int)((Rows - row - 1) * height) + y;
                                if (pixels[py * atlas.width + px].a < 128) continue;
                                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                            }
                    Vector2 pivot = new Vector2((minX + maxX + 1) / (2f * width), (minY + maxY + 1) / (2f * height));
                    visibleSizes[row] = new Vector2(Mathf.Max(1, maxX - minX + 1) / 100f, Mathf.Max(1, maxY - minY + 1) / 100f);
                    for (int col = 0; col < Columns; col++)
                        frames[row, col] = Sprite.Create(atlas,
                            new Rect(col * width, (Rows - row - 1) * height, width, height),
                            pivot, 100f, 0, SpriteMeshType.FullRect);
                }
            }
            for (int row = 0; row < Rows; row++)
            {
                var child = new GameObject("StatusOverlay" + row);
                child.transform.SetParent(transform, false);
                overlays[row] = child.AddComponent<SpriteRenderer>();
                overlays[row].sprite = frames[row, 0];
                overlays[row].color = new Color(1, 1, 1, row == 2 ? 0.55f : 0.75f);
                overlays[row].enabled = false;
            }
        }

        public static Bounds GetVisibleBodyBounds(Sprite sprite)
        {
            if (bodyBounds.TryGetValue(sprite, out Bounds cached)) return cached;
            var points = new List<Vector2>();
            bool found = false;
            Bounds bounds = sprite.bounds;
            for (int shape = 0; shape < sprite.GetPhysicsShapeCount(); shape++)
            {
                sprite.GetPhysicsShape(shape, points);
                foreach (Vector2 point in points)
                {
                    if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                    else bounds.Encapsulate(point);
                }
            }
            bodyBounds[sprite] = bounds;
            return bounds;
        }

        void LateUpdate()
        {
            RenderFrame((int)(Time.time / FrameSeconds) % Columns);
        }

        void RenderFrame(int frame)
        {
            if (body == null || body.sprite == null || frames == null || enemy == null) return;
            var status = enemy.Status;
            for (int row = 0; row < Rows; row++)
            {
                SpriteRenderer overlay = overlays[row];
                if (overlay == null) continue;
                bool visible = row == 0 ? status.BurnTurns > 0 : row == 1 ? status.Chill > 0
                    : row == 2 ? status.Frozen : row == 3 ? status.ShockTurns > 0 : status.FractureTurns > 0;
                overlay.enabled = visible && enemy.IsAlive && body.enabled;
                if (!overlay.enabled) continue;
                overlay.sprite = frames[row, frame];
                overlay.sortingLayerID = body.sortingLayerID;
                overlay.sortingOrder = body.sortingOrder + 1 + row;
                // 기존 스프라이트의 피벗/크기와 프레임 변화에 맞춰 중심과 크기를 따라간다.
                Bounds visibleBody = GetVisibleBodyBounds(body.sprite);
                Vector3 center = visibleBody.center;
                if (body.flipX) center.x = -center.x;
                if (body.flipY) center.y = -center.y;
                // 크기가 커져도 아래 경계가 발밑에 머물도록 중심을 함께 올린다.
                center.y += visibleBody.size.y * (HeightScale - 1f) * 0.5f;
                overlay.transform.localPosition = center;
                Vector3 size = visibleBody.size;
                Vector2 effectSize = visibleSizes[row];
                overlay.transform.localScale = new Vector3(size.x * WidthScale / effectSize.x, size.y * HeightScale / effectSize.y, 1);
                overlay.flipX = body.flipX;
                overlay.flipY = body.flipY;
            }
        }
    }
}
