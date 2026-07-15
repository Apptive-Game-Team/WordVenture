using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Stage;
using WordVenture.Combat.UI;

namespace WordVenture.Combat.Enemies
{
    public class ShieldEnemy : Enemy
    {

        void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Attack"))
            {
                hp -= 1;
            }
            if (other.CompareTag("Heal"))
            {
                hp += 1;
            }
            if (hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}