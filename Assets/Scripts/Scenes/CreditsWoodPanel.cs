using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    // 타이틀 나무판의 색과 각진 테두리를 공유하는 크기 조절 가능한 UI.
    [RequireComponent(typeof(CanvasRenderer))]
    public class CreditsWoodPanel : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = rectTransform.rect;
            Quad(mesh, bounds, 0, 0, 1, 1, new Color32(81, 60, 13, 255));
            Quad(mesh, bounds, 4, 4, bounds.width - 8, bounds.height - 8, new Color32(105, 81, 19, 255), true);
            Quad(mesh, bounds, 8, 12, bounds.width - 20, bounds.height - 20, new Color32(153, 122, 43, 255), true);
            Quad(mesh, bounds, 16, 20, bounds.width - 36, bounds.height - 36, new Color32(190, 156, 70, 255), true);
            Quad(mesh, bounds, 24, 28, bounds.width - 52, bounds.height - 52, new Color32(203, 173, 85, 255), true);
            // 픽셀 단위의 단차와 나뭇결. 문구가 놓이는 중앙은 비워 둔다.
            Quad(mesh, bounds, 28, bounds.height - 24, bounds.width - 60, 4, new Color32(216, 185, 95, 255), true);
            Quad(mesh, bounds, 24, 20, bounds.width - 52, 4, new Color32(128, 98, 28, 255), true);
            for (int i = 0; i < 5; i++)
            {
                float x = 32 + (bounds.width - 96) * i / 5;
                Quad(mesh, bounds, x, bounds.height - 16, 12 + i % 3 * 8, 3, new Color32(105, 81, 19, 255), true);
            }
        }

        void Quad(VertexHelper mesh, Rect bounds, float x, float y, float width, float height, Color32 shade, bool pixels = false)
        {
            if (!pixels) { width *= bounds.width; height *= bounds.height; }
            var tint = (Color)shade * color;
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(bounds.xMin + x, bounds.yMin + y), tint, Vector2.zero);
            mesh.AddVert(new Vector3(bounds.xMin + x, bounds.yMin + y + height), tint, Vector2.zero);
            mesh.AddVert(new Vector3(bounds.xMin + x + width, bounds.yMin + y + height), tint, Vector2.zero);
            mesh.AddVert(new Vector3(bounds.xMin + x + width, bounds.yMin + y), tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
