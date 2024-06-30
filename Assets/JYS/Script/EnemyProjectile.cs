using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyProjectile : MonoBehaviour
    {
        float speed = 10;
        Vector3 moveVector;
        int damage;

        public void InitProjectileDamage(int damage)
        {
            this.damage = damage;
        }

        void Start()
        {
            moveVector = new Vector3(-1 * speed, 0, 0);
        }

        void Update()
        {
            if (transform.position.x < -10)
            {
                Destroy(gameObject);
            }
            else
                transform.position = transform.position + moveVector * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Me")) {
                print(collision.gameObject.tag);
                Player.PlayerInt().TakeHit(damage);
                Destroy(gameObject);
            }
        }
    }
}


