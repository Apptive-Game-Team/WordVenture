using System.Collections;
using TMPro;
using UnityEngine;

namespace Combat.Enemies
{
    // 주문이 적 속성에 강했는지 약했는지를 적 머리 위에 작은 표시로 한 번 보여 준다. 글자는 쓰지 않는다.
    // 그림은 docs/design/combat-effects/build_affinity_marks.py가 만든 Resources/Combat/AffinityMark.png다.
    public sealed class AffinityMarkVfx : MonoBehaviour
    {
        // 행 순서는 그림의 행 순서와 같다.
        public enum Mark { Weak, Resisted }

        const int Columns = 6;
        const int Rows = 2;
        const float FrameSeconds = 0.07f;
        // 마지막 두 프레임은 흐려지는 그림이라, 앞 네 프레임 뒤에 잠깐 더 머물러 0.5초쯤 보이게 한다.
        const float HoldSeconds = 0.12f;
        // 그림 한 칸은 16x16칸이고 표시는 가운데 12칸 폭 안에 그려지며 아래 가장자리는 아래에서 2칸 위에 있다.
        const float CellSize = 16f;
        const float ArtWidth = 12f;
        const float FloorFromBottom = 2f;
        // 표시 폭을 보이는 몸 폭의 절반으로 맞춘다.
        const float WidthRatio = 0.5f;
        // 체력 숫자 위쪽 여백과 재생 동안 떠오르는 높이다. 둘 다 적 로컬 단위다.
        const float HpGap = 0.05f;
        const float DriftHeight = 0.15f;
        const int SortingOffset = 20;

        static Sprite[,] frames;
        SpriteRenderer body;
        SpriteRenderer mark;
        TMP_Text hpText;
        Coroutine playing;

        public Mark? LastMark { get; private set; }

        public void Initialize(SpriteRenderer renderer)
        {
            body = renderer;
            if (body == null) { enabled = false; return; }
            if (frames == null)
            {
                Texture2D atlas = Resources.Load<Texture2D>("Combat/AffinityMark");
                if (atlas == null) { enabled = false; return; }
                var loaded = new Sprite[Rows, Columns];
                float width = atlas.width / (float)Columns;
                float height = atlas.height / (float)Rows;
                var pivot = new Vector2(0.5f, FloorFromBottom / CellSize);
                for (int row = 0; row < Rows; row++)
                    for (int column = 0; column < Columns; column++)
                        // 칸 폭이 1 unit이 되도록 pixelsPerUnit을 칸 폭으로 둔다.
                        loaded[row, column] = Sprite.Create(atlas,
                            new Rect(column * width, (Rows - row - 1) * height, width, height),
                            pivot, width, 0, SpriteMeshType.FullRect);
                frames = loaded;
            }
            hpText = GetComponentInChildren<TMP_Text>();
            var child = new GameObject("AffinityMark");
            child.transform.SetParent(transform, false);
            mark = child.AddComponent<SpriteRenderer>();
            mark.enabled = false;
        }

        public void Play(Mark kind)
        {
            LastMark = kind;
            if (!enabled || mark == null || !gameObject.activeInHierarchy) return;
            if (playing != null) StopCoroutine(playing);
            playing = StartCoroutine(Playing(kind));
        }

        IEnumerator Playing(Mark kind)
        {
            mark.enabled = true;
            for (int column = 0; column < Columns; column++)
            {
                mark.sprite = frames[(int)kind, column];
                Place(kind, column / (float)(Columns - 1));
                // 표시가 가장 또렷한 가운데 두 프레임을 조금 더 붙든다.
                yield return new WaitForSeconds(column == 2 || column == 3 ? FrameSeconds + HoldSeconds : FrameSeconds);
            }
            mark.enabled = false;
            playing = null;
        }

        // progress는 0에서 1까지 재생이 얼마나 지났는지다. 약점은 위로 떠오르고 저항은 제자리에 둔다.
        void Place(Mark kind, float progress)
        {
            if (body.sprite == null) return;
            Bounds visibleBody = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
            float x = body.flipX ? -visibleBody.center.x : visibleBody.center.x;
            float y = visibleBody.max.y;
            // 체력 숫자가 몸보다 높이 있으면 숫자 위에 둔다.
            if (hpText != null)
            {
                Vector3 hpTop = transform.InverseTransformPoint(hpText.transform.position);
                float hpHalf = hpText.textBounds.extents.y * Mathf.Abs(hpText.transform.lossyScale.y)
                    / Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
                y = Mathf.Max(y, hpTop.y + hpHalf + HpGap);
            }
            if (kind == Mark.Weak) y += DriftHeight * progress;
            mark.transform.localPosition = new Vector3(x, y, 0f);
            float size = visibleBody.size.x * WidthRatio * CellSize / ArtWidth;
            // 적이 반대로 돌아 부모 x 배율이 음수가 되어도 표시는 뒤집히지 않게 한다.
            float sign = transform.lossyScale.x < 0f ? -1f : 1f;
            mark.transform.localScale = new Vector3(size * sign, size, 1f);
            mark.sortingLayerID = body.sortingLayerID;
            mark.sortingOrder = body.sortingOrder + SortingOffset;
        }

        void OnDisable()
        {
            // 쓰러져 풀로 돌아간 적이 재생 중이던 표시를 들고 다시 나오지 않게 한다.
            if (mark != null) mark.enabled = false;
            playing = null;
        }
    }
}
