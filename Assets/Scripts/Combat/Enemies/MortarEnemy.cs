using UnityEngine;

namespace Combat.Enemies
{
    // 움직이지 않고 한 적 턴에는 떨어질 자리를 표시하고, 다음 적 턴에 그 자리에 탄을 떨어뜨린다.
    // 앞줄 아군 슬라임을 넘어 맨 뒤 슬라임이나 워드를 노린다.
    public class MortarEnemy : Enemy
    {
        [SerializeField] Sprite markerSprite;
        MortarStrike strike;

        MortarStrike Strike => strike ??= new MortarStrike(markerSprite);
        public bool HasMarker => Strike.IsAimed;
        public float MarkerX => Strike.TargetX;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (!Strike.IsAimed)
            {
                Strike.Aim();
                SetIntent("포격 조준");
                return;
            }

            Animator.RangeAttack();
            Strike.Fire(AttackDamage);
            SetIntent(string.Empty);
        }

        void OnDisable()
        {
            strike?.Clear();
        }
    }
}
