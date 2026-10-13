using UnityEngine;

namespace Combat.Enemies
{
    // 폭발 슬라임이 터진 자리에 폭발 그림을 한 번 재생한다. 슬라임은 쓰러지면 0.25초 뒤 꺼지므로
    // 슬라임의 자식이 아닌 따로 선 오브젝트로 만들고, 재생이 끝나면 스스로 지운다. 피해는 주지 않는다.
    public sealed class BlastVfx : MonoBehaviour
    {
        const float FrameSeconds = 1f / 12f;
        const int SortingOffset = 10;

        Sprite[] frames;
        SpriteRenderer render;
        float elapsed;

        // diameter는 폭발 그림이 차지할 가로 길이(unit)다. body의 정렬 층 위에 그린다.
        public static BlastVfx Spawn(Vector3 position, Sprite[] frames, float diameter, SpriteRenderer body)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null) return null;
            var go = new GameObject("BlastVfx");
            go.transform.position = position;
            // 그림 한 장의 가로가 1 unit이 아니어도 diameter에 맞춘다.
            float scale = diameter / frames[0].bounds.size.x;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var vfx = go.AddComponent<BlastVfx>();
            vfx.frames = frames;
            vfx.render = go.AddComponent<SpriteRenderer>();
            vfx.render.sprite = frames[0];
            if (body != null)
            {
                vfx.render.sortingLayerID = body.sortingLayerID;
                vfx.render.sortingOrder = body.sortingOrder + SortingOffset;
            }
            return vfx;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            int index = (int)(elapsed / FrameSeconds);
            if (index >= frames.Length) { Destroy(gameObject); return; }
            render.sprite = frames[index];
        }
    }
}
