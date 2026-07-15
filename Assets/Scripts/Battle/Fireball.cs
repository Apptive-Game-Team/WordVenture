using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace WordVenture.Battle
{
    public class Fireball : MonoBehaviour
    {
        public float speed = 10f;
        public int damage = 20;
        public float lifetime = 2f;

        private Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            Destroy(gameObject, lifetime);

            Vector2 shootDirection = -transform.right;
            rb.velocity = shootDirection * speed;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                Destroy(gameObject);
            }
        }
    }
}



