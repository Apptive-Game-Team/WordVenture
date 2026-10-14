using System.Collections;
using UnityEngine;

namespace Combat.Enemies
{
    // 원소 반응, 방패 보호, 회복을 적 몸 위에 한 번 재생한다. 머리 위 글자 대신 이 효과로 보여 준다.
    // 그림은 docs/design/combat-effects/build_reaction_bursts.py가 만든 Resources/Combat/ReactionBurst.png다.
    public sealed class ReactionBurstVfx : MonoBehaviour
    {
        // 행 순서는 그림의 행 순서와 같다.
        public enum Burst { Overload, Shatter, Paralysis, LavaCrack, Guard, Heal }

        const int Columns = 6;
        const int Rows = 6;
        const float FrameSeconds = 0.07f;
        // 그림 한 칸은 48x40칸이고, 몸은 가운데 28칸 폭이며 바닥선은 아래에서 6칸 위에 있다.
        const float CellWidth = 48f;
        const float CellHeight = 40f;
        const float BodyWidth = 28f;
        const float FloorFromBottom = 6f;
        const int SortingOffset = 10;

        static Sprite[,] frames;
        SpriteRenderer body;
        SpriteRenderer burst;
        Coroutine playing;

        public Burst? LastBurst { get; private set; }

        public void Initialize(SpriteRenderer renderer)
        {
            body = renderer;
            if (body == null) { enabled = false; return; }
            if (frames == null)
            {
                Texture2D atlas = Resources.Load<Texture2D>("Combat/ReactionBurst");
                if (atlas == null) { enabled = false; return; }
                frames = new Sprite[Rows, Columns];
                float width = atlas.width / (float)Columns;
                float height = atlas.height / (float)Rows;
                var pivot = new Vector2(0.5f, FloorFromBottom / CellHeight);
                for (int row = 0; row < Rows; row++)
                    for (int column = 0; column < Columns; column++)
                        // 칸 폭이 1 unit이 되도록 pixelsPerUnit을 칸 폭으로 둔다.
                        frames[row, column] = Sprite.Create(atlas,
                            new Rect(column * width, (Rows - row - 1) * height, width, height),
                            pivot, width, 0, SpriteMeshType.FullRect);
            }
            var child = new GameObject("ReactionBurst");
            child.transform.SetParent(transform, false);
            burst = child.AddComponent<SpriteRenderer>();
            burst.enabled = false;
        }

        // ElementalStatus.HitResult.Reaction 글자를 효과로 바꾼다. 반응이 없으면 false다.
        public static bool TryGetBurst(string reaction, out Burst result)
        {
            switch (reaction)
            {
                case ElementalStatus.OverloadReaction: result = Burst.Overload; return true;
                case ElementalStatus.ShatterReaction: result = Burst.Shatter; return true;
                case ElementalStatus.ParalysisReaction: result = Burst.Paralysis; return true;
                case ElementalStatus.LavaCrackReaction: result = Burst.LavaCrack; return true;
                default: result = default; return false;
            }
        }

        public void Play(Burst kind)
        {
            LastBurst = kind;
            if (!enabled || burst == null || !gameObject.activeInHierarchy) return;
            if (playing != null) StopCoroutine(playing);
            playing = StartCoroutine(Playing(kind));
        }

        IEnumerator Playing(Burst kind)
        {
            // 신경 마비는 머리 위를 도는 별이 한 바퀴 더 돌도록 두 번 재생한다.
            int loops = kind == Burst.Paralysis ? 2 : 1;
            burst.enabled = true;
            for (int loop = 0; loop < loops; loop++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    burst.sprite = frames[(int)kind, column];
                    FitToBody();
                    yield return new WaitForSeconds(FrameSeconds);
                }
            }
            burst.enabled = false;
            playing = null;
        }

        void FitToBody()
        {
            if (body.sprite == null) return;
            Bounds visibleBody = ElementalStatusVfx.GetVisibleBodyBounds(body.sprite);
            float x = body.flipX ? -visibleBody.center.x : visibleBody.center.x;
            burst.transform.localPosition = new Vector3(x, visibleBody.min.y, 0f);
            float size = visibleBody.size.x * CellWidth / BodyWidth;
            burst.transform.localScale = new Vector3(size, size, 1f);
            burst.flipX = body.flipX;
            burst.sortingLayerID = body.sortingLayerID;
            burst.sortingOrder = body.sortingOrder + SortingOffset;
        }

        void OnDisable()
        {
            // 쓰러져 풀로 돌아간 적이 재생 중이던 효과를 들고 다시 나오지 않게 한다.
            if (burst != null) burst.enabled = false;
            playing = null;
        }
    }
}
