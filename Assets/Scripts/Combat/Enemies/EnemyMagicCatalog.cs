using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Combat.Enemies
{
    // 플레이어와 같은 마법 애니메이션을 쓰되 충돌과 피해는 적 공격 경로가 처리한다.
    public sealed class EnemyMagicCatalog : ScriptableObject
    {
        public GameObject[] shoot, drop, explode;
        public Sprite[] elements;
        public AudioClip impactClip;
        public Sprite[] boundedSprites;
        public Vector4[] visibleBounds;
        Dictionary<Sprite, Bounds> bounds;
        public Bounds VisibleBounds(Sprite sprite)
        {
            if (bounds == null)
            {
                bounds = new Dictionary<Sprite, Bounds>();
                for (int i = 0; i < boundedSprites.Length; i++)
                {
                    Vector4 value = visibleBounds[i];
                    bounds[boundedSprites[i]] = new Bounds(new Vector3(value.x, value.y, 0), new Vector3(value.z, value.w, 0));
                }
            }
            return bounds.TryGetValue(sprite, out Bounds result) ? result : ElementalStatusVfx.GetVisibleBodyBounds(sprite);
        }
        public GameObject Prefab(MagicType spell, MagicType element)
        {
            int index = (int)element - (int)MagicType.Holy;
            var prefabs = spell == MagicType.Drop ? drop : spell == MagicType.Explode ? explode : shoot;
            return index >= 0 && index < prefabs.Length ? prefabs[index] : null;
        }
        public Sprite Element(MagicType element)
        {
            int index = (int)element - (int)MagicType.Holy;
            return index >= 0 && index < elements.Length ? elements[index] : null;
        }
    }
}
