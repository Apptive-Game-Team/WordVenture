using UnityEngine;

namespace Combat.Enemies
{
    // 움직이지 않고 한 적 턴에는 떨어질 자리를 표시하고, 다음 적 턴에 그 자리로 탄을 쏜다.
    // 앞줄 아군 슬라임을 넘어 맨 뒤 슬라임이나 워드를 노린다.
    public class MortarEnemy : Enemy
    {
        [SerializeField] Sprite markerSprite;
        [SerializeField] Sprite shellSprite;
        MortarStrike strike;

        MortarStrike Strike => strike ??= new MortarStrike(markerSprite, shellSprite);
        public bool HasMarker => Strike.IsAimed;
        public float MarkerX => Strike.TargetX;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (!Strike.IsAimed)
            {
                Strike.Aim();
                return;
            }

            Animator.RangeAttack();
            Strike.Fire(transform.position, AttackDamage);
        }

        void OnDisable()
        {
            strike?.Clear();
        }
    }
}
