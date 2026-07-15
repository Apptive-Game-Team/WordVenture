using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Battle.Turns;

namespace WordVenture.Battle
{
    public class Player : MonoBehaviour
    {
        private TurnBattleSystem battleSystem;
        private int health = 100;

        public Enemy enemy;
        public GameObject fireballPrefab;
        public GameObject iceballPrefab;
        public GameObject rockballPrefab;
        public GameObject lightningballPrefab;
        public Transform firePoint;

        public enum MagicType
        {
            Shoot,
            Heal,
            Summon,
            Drop,
            Fire,
            Ice,
            Rock,
            Lightning,
            Enemy,
            Ally,
            Me
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) //shoot fire
            {
                ShootFireball();
            }

            if (Input.GetKeyDown(KeyCode.S)) //shoot ice
            {
                ShootIceball();
            }

            if (Input.GetKeyDown(KeyCode.D)) // shoot rock
            {
                ShootRockball();
            }

            if (Input.GetKeyDown(KeyCode.F)) // shoot lightning
            {
                ShootLightningball();
            }

            if (Input.GetKeyDown(KeyCode.Q)) //fire heal
            {
                IncreaseHealth(10);
            }

            if (Input.GetKeyDown(KeyCode.W)) //Ice heal
            {
                IncreaseHealth(20);
            }

            if (Input.GetKeyDown(KeyCode.E)) //Rock heal
            {
                IncreaseHealth(30);
            }

            if (Input.GetKeyDown(KeyCode.R)) //Lightning heal
            {
                IncreaseHealth(40);
            }
        }

        void AttackEnemy(int damage)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("Enemy reference is not set in the Player script.");
            }
        }

        void ShootFireball()
        {
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);

            Rigidbody rb = fireball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * fireball.GetComponent<Fireball>().speed;
            }
        }

        void ShootIceball()
        {
            GameObject iceball = Instantiate(iceballPrefab, firePoint.position, firePoint.rotation);

            Rigidbody2D rb = iceball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = firePoint.right * iceball.GetComponent<Iceball>().speed;
            }
        }

        void ShootRockball()
        {
            GameObject rockball = Instantiate(rockballPrefab, firePoint.position, firePoint.rotation);

            Rigidbody2D rb = rockball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = firePoint.right * rockball.GetComponent<Rockball>().speed;
            }
        }

        void ShootLightningball()
        {
            GameObject lightningball = Instantiate(lightningballPrefab, firePoint.position, firePoint.rotation);

            Rigidbody2D rb = lightningball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = firePoint.right * lightningball.GetComponent<Lightningball>().speed;
            }
        }

        void IncreaseHealth(int amount)
        {
            health += amount;
            Debug.Log("Player's health increased by " + amount + ". Current health: " + health);
        }
    }
}


