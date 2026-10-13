using Combat.Allies;

namespace Combat.Enemies
{
    public class SwordEnemy : Enemy
    {

        public override string AttackDescription => "사거리 안까지 다가와 맨 앞의 아군 슬라임이나 워드를 근접 공격한다.";

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
