using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace WordVenture.Battle
{
    public class Enemy : MonoBehaviour
    {
        public int maxHealth = 100;
        private int currentHealth;

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            Debug.Log("Enemy took " + amount + " damage. Current health: " + currentHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Enemy Died");
            Destroy(gameObject);
        }
    }

}

