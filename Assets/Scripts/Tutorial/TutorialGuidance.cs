using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    // SpriteRenderer/Collider2D와 UI RectTransform을 같은 캔버스 좌표로 투영한다.
    [RequireComponent(typeof(CanvasRenderer))]
    public class TutorialGuidance : MaskableGraphic
    {
        Transform source, destination;
        readonly Vector3[] corners = new Vector3[4];
        public float StrokeWidth { get; set; } = 3;

        public void SetTargets(Transform from, Transform to)
        {
            source = from;
            destination = to;
            SetVerticesDirty();
        }

        void LateUpdate() { SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            bool hasSource = TryRect(source, out Rect from);
            bool hasDestination = TryRect(destination, out Rect to);
            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.unscaledTime * 3);
            if (hasSource) Corners(mesh, from, new Color(1, 0.94f, 0.76f, 0.9f), false);
            if (hasDestination) Corners(mesh, to, new Color(1, 0.8f, 0.35f, pulse), true);
        }

        public bool TryRect(Transform target, out Rect result, bool spriteBounds = false)
        {
            result = default;
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            Camera targetCamera = Camera.main;
            if (target is RectTransform ui)
            {
                ui.GetWorldCorners(corners);
                Canvas targetCanvas = ui.GetComponentInParent<Canvas>();
                if (targetCanvas != null)
                    targetCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera;
            }
            else
            {
                if (targetCamera == null) return false;
                SpriteRenderer sprite = target.GetComponent<SpriteRenderer>();
                Collider2D collider = target.GetComponent<Collider2D>();
                // 카드 스프라이트의 투명 여백 대신 실제 조작 영역을 감싼다.
                Bounds bounds = spriteBounds && sprite != null ? sprite.bounds
                    : collider != null ? collider.bounds : sprite != null ? sprite.bounds : new Bounds(target.position, Vector3.one);
                corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
                corners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.center.z);
                corners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.center.z);
                corners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z);
            }
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            foreach (Vector3 corner in corners)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(targetCamera, corner);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screen, canvasCamera, out Vector2 local);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            float padding = StrokeWidth * 2.5f;
            result = Rect.MinMaxRect(min.x - padding, min.y - padding, max.x + padding, max.y + padding);
            return true;
        }

        void Corners(VertexHelper mesh, Rect rect, Color tint, bool destinationFocus)
        {
            float width = StrokeWidth * (destinationFocus ? 1.4f : 1);
            float arm = Mathf.Min(StrokeWidth * 6, Mathf.Min(rect.width, rect.height) * 0.25f);
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                {
                    Vector2 corner = new Vector2(x == 0 ? rect.xMin : rect.xMax, y == 0 ? rect.yMin : rect.yMax);
                    Vector2 horizontal = new Vector2(x == 0 ? arm : -arm, 0);
                    Vector2 vertical = new Vector2(0, y == 0 ? arm : -arm);
                    Color shadow = new Color(0.14f, 0.12f, 0.19f, tint.a * 0.85f);
                    Line(mesh, corner, corner + horizontal, width + StrokeWidth * 1.5f, shadow);
                    Line(mesh, corner, corner + vertical, width + StrokeWidth * 1.5f, shadow);
                    Line(mesh, corner, corner + horizontal, width, tint);
                    Line(mesh, corner, corner + vertical, width, tint);
                }
        }

        static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
        {
            Vector2 normal = new Vector2(-(b - a).y, (b - a).x).normalized * width * 0.5f;
            int i = mesh.currentVertCount;
            mesh.AddVert(a - normal, tint, Vector2.zero);
            mesh.AddVert(a + normal, tint, Vector2.zero);
            mesh.AddVert(b + normal, tint, Vector2.zero);
            mesh.AddVert(b - normal, tint, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }

    }
}

