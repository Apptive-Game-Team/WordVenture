using System;
using Cards;
using UnityEngine;

namespace Combat.Allies
{
    // 아군 슬라임의 공격과 회복이 목표까지 날아가는 구슬. 도착하면 onArrive를 부르고 사라진다.
    // 구슬 모양은 코드로 만든 원 하나를 속성 색으로 칠해서 쓴다.
    public class AllyProjectile : MonoBehaviour
    {
        const float Speed = 16f;
        const float ArriveDistance = 0.1f;
        // 목표가 사라지거나 길이 막혀도 턴이 멈추지 않도록 이 시간이 지나면 도착한 것으로 친다.
        const float MaxSeconds = 2f;
        const float BallScale = 0.35f;
        const int TextureSize = 32;

        static Sprite ballSprite;
        static int activeCount;
        public static int ActiveCount => activeCount;

        Transform target;
        Vector3 lastTargetPosition;
        Action onArrive;
        float elapsed;

        public static AllyProjectile Fire(Vector3 from, Transform target, MagicType element, Action onArrive)
        {
            var ball = new GameObject("AllyProjectile_" + element);
            ball.transform.position = from;
            ball.transform.localScale = Vector3.one * BallScale;
            SpriteRenderer renderer = ball.AddComponent<SpriteRenderer>();
            renderer.sprite = BallSprite();
            renderer.color = ColorOf(element);
            renderer.sortingOrder = 50;
            AllyProjectile projectile = ball.AddComponent<AllyProjectile>();
            projectile.target = target;
            projectile.lastTargetPosition = target != null ? target.position : from;
            projectile.onArrive = onArrive;
            activeCount++;
            return projectile;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            if (target != null) lastTargetPosition = target.position;
            Vector3 destination = new Vector3(lastTargetPosition.x, lastTargetPosition.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, destination, Speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, destination) <= ArriveDistance || elapsed >= MaxSeconds)
                Arrive();
        }

        void Arrive()
        {
            Action callback = onArrive;
            onArrive = null;
            Destroy(gameObject);
            callback?.Invoke();
        }

        void OnDestroy()
        {
            activeCount = Mathf.Max(0, activeCount - 1);
        }

        static Sprite BallSprite()
        {
            if (ballSprite != null) return ballSprite;
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            float center = (TextureSize - 1) / 2f;
            for (int y = 0; y < TextureSize; y++)
            for (int x = 0; x < TextureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                // 가운데는 흰색, 가장자리로 갈수록 투명해지는 부드러운 원.
                float alpha = Mathf.Clamp01((1f - distance) * 3f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            texture.Apply();
            ballSprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), TextureSize);
            return ballSprite;
        }

        static Color ColorOf(MagicType element)
        {
            switch (element)
            {
                case MagicType.Fire: return new Color(1f, 0.4f, 0.15f);
                case MagicType.Ice: return new Color(0.4f, 0.85f, 1f);
                case MagicType.Rock: return new Color(0.7f, 0.5f, 0.3f);
                case MagicType.Lightning: return new Color(1f, 0.95f, 0.3f);
                case MagicType.Holy: return new Color(0.7f, 1f, 0.7f);
                default: return Color.white;
            }
        }
    }
}
