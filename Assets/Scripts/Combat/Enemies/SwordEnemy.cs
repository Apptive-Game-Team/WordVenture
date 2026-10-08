namespace Combat.Enemies
{
    public class SwordEnemy : Enemy
    {

        protected override void Start()
        {
            base.Start();
        }
        public override void Attack(float distanceToPlayer)
        {
            base.Attack(distanceToPlayer);
            if (distanceToPlayer < attackRange)
            {
                Player.PlayerInt().TakeHit(AttackDamage);
            }
        }
    }
}
