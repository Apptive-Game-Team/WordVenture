using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    public class BossEnemy : Enemy
    {
        [SerializeField] GameObject fireShoot;

        public override string AttackDescription => "다가와 근접 공격하고, 이동을 마칠 때마다 공격력의 70% 로 마법 탄을 쏜다.";

        protected override void Start()
        {
            base.Start();
        }

        public override void Attack(float distanceToFrontLine)
        {
            base.Attack(distanceToFrontLine);
            if (IsInAttackRange(distanceToFrontLine))
            {
                AllyFormation.HitFrontLine(AttackDamage, transform.position.x);
            }
        }
        protected override void StopMove()
        {
            base.StopMove();
            Animator.RangeAttack();
            GameObject projectile = Instantiate(fireShoot, transform.position, Quaternion.identity);
            projectile.GetComponent<EnemyProjectile>().InitProjectileDamage((int) (AttackDamage * 0.7f));
        }


    }
}


