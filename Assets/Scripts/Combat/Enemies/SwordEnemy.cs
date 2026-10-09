using Combat.Allies;

namespace Combat.Enemies
{
    public class SwordEnemy : Enemy
    {

        protected override void Start()
        {
            base.Start();
        }
        public override void Attack(float distanceToFrontLine)
        {
            base.Attack(distanceToFrontLine);
            if (distanceToFrontLine < attackRange)
            {
                AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
            }
        }
    }
}
