using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    public class BossEnemy : Enemy
    {
        [SerializeField] GameObject fireShoot;

        void Start()
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
        protected override void StopMove()
        {
            base.StopMove();
            Animator.RangeAttack();
            GameObject projectile = Instantiate(fireShoot, transform.position, Quaternion.identity);
            projectile.GetComponent<EnemyProjectile>().InitProjectileDamage((int) (AttackDamage * 0.7f));
        }


    }
}


