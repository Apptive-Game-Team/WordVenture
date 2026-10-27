using UnityEngine;

namespace Combat.Enemies
{
    // 카드와 예고가 같은 그림을 참조한다. 런타임에는 중앙 문양만 잘라 쓴다.
    public sealed class BossIntentArt : ScriptableObject
    {
        public Sprite shoot;
        public Sprite drop;
        public Sprite explode;
    }
}
