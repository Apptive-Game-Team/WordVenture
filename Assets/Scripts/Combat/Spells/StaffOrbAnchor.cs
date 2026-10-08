using System;
using UnityEngine;

namespace Combat.Spells
{
    // 워드의 지팡이 구슬 위치를 지금 그려지는 스프라이트에 맞춰 알려 준다.
    // 공격 애니메이션에서는 지팡이를 휘둘러 구슬이 프레임마다 크게 옮겨 가므로
    // SpellChargeEffect가 이 위치를 따라간다.
    [RequireComponent(typeof(SpriteRenderer))]
    public class StaffOrbAnchor : MonoBehaviour
    {
        [Serializable]
        struct SpriteOrbPosition
        {
            public Sprite sprite;
            // 스프라이트 피벗 기준 로컬 좌표(유닛).
            public Vector2 position;
        }

        // 표에 없는 스프라이트(대기 프레임 등)에서 쓰는 구슬 위치.
        [SerializeField] Vector2 defaultPosition = new Vector2(1.86f, 1.43f);
        [SerializeField] SpriteOrbPosition[] spritePositions = Array.Empty<SpriteOrbPosition>();

        SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public Vector3 GetWorldPosition()
        {
            return transform.TransformPoint(GetLocalPosition(spriteRenderer.sprite));
        }

        Vector2 GetLocalPosition(Sprite sprite)
        {
            foreach (SpriteOrbPosition entry in spritePositions)
            {
                if (entry.sprite == sprite)
                {
                    return entry.position;
                }
            }
            return defaultPosition;
        }
    }
}
