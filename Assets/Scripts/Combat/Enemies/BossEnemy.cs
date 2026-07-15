using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Stage;
using WordVenture.Combat.UI;

namespace WordVenture.Combat.Enemies
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
                Player.PlayerInt().TakeHit(damage);
            }
        }
        protected override void StopMove()
        {
            base.StopMove();
            animator.RangeAttack();
            GameObject projectile = Instantiate(fireShoot, transform.position, Quaternion.identity);
            projectile.GetComponent<EnemyProjectile>().InitProjectileDamage((int) (damage * 0.7f));
        }


    }
}


