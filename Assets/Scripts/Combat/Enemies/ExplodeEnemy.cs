using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 맨 앞까지 다가와 한 적 턴 동안 "폭발 준비"를 띄우고, 다음 적 턴에 터진다. 터지면 아군 슬라임
    // 전체(아군이 없으면 워드)가 피해를 받고 자기도 쓰러진다. 터지기 전에 쓰러뜨리면 주변 적이 피해를 받는다.
    public class ExplodeEnemy : Enemy
    {
        const float BlastRadius = 2.5f;

        bool primed;
        bool exploding;

        public bool IsPrimed => primed;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (primed)
            {
                exploding = true;
                Animator.RangeAttack();
                AllyFormation.HitAllAllies(AttackDamage);
                Kill();
                return;
            }

            if (distanceToFrontLine <= attackRange)
            {
                primed = true;
                SetIntent("폭발 준비");
                return;
            }

            base.TakeTurnAction(distanceToFrontLine);
        }

        protected override void Death()
        {
            bool blastEnemies = primed && !exploding;
            primed = false;
            SetIntent(string.Empty);
            base.Death();
            if (blastEnemies) DamageNearbyEnemies();
        }

        void DamageNearbyEnemies()
        {
            float x = transform.position.x;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                if (enemy == this || !enemy.IsAlive) continue;
                if (Mathf.Abs(enemy.transform.position.x - x) <= BlastRadius) enemy.TakeHit(Damage);
            }
        }
    }
}
