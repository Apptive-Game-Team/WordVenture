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

        public override void Attack(float distanceToPlayer)
        {
            base.Attack(distanceToPlayer);
            if (distanceToPlayer < attackRange)
            {
                Player.PlayerInt().TakeHit(AttackDamage);
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


