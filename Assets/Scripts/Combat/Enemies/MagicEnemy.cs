using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Stage;
using WordVenture.Combat.UI;

namespace WordVenture.Combat.Enemies
{
    public class MagicEnemy : Enemy
    {
        [SerializeField] GameObject fireShoot;

        void Start()
        {
            base.Start();
        }

        public override void Attack(float distanceToPlayer)
        {
            animator.RangeAttack();
            GameObject projectile = Instantiate(fireShoot, transform.position,Quaternion.identity);
            projectile.GetComponent<EnemyProjectile>().InitProjectileDamage(damage);
        }
        //private void OnTriggerEnter2D(Collider2D other)
        //{
        //    if (other.CompareTag("Attack"))
        //    {
        //        hp -= 1;
        //    }
        //    if (other.CompareTag("Heal"))
        //    {
        //        hp += 1;
        //    }
        //    if (hp <= 0)
        //    {
        //        Destroy(gameObject);
        //    }
        //}
    }
}
