namespace Combat.Enemies
{
    // 움직이지 않고 매 적 턴에 가장 많이 다친 다른 적을 공격력만큼 회복한다. 공격하지 않는다.
    public class HealEnemy : Enemy
    {
        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            Enemy target = FindMostWounded();
            if (target == null) return;
            Animator.RangeAttack();
            target.Heal(AttackDamage);
        }

        Enemy FindMostWounded()
        {
            Enemy target = null;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                if (enemy == this || enemy.MissingHp <= 0) continue;
                if (target == null || enemy.MissingHp > target.MissingHp) target = enemy;
            }
            return target;
        }
    }
}
