using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Enemy
{
    public class SwordEnemy : Enemy
    {

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
    }
}