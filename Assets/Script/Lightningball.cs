using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightningball : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 25;
    public float lifetime = 1.5f; 

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
