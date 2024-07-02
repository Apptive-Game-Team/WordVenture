// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Player : MonoBehaviour
//     {
//         protected int hp = 1;
//         protected int maxHp = 1;
//         void Start()
//         {
            
//         }
//         private void OnTriggerEnter2D(Collider2D other) 
//         {
//             if (other.CompareTag("Attack"))
//             {
//                 hp -= 1;
//             } 
//             if (other.CompareTag("Heal"))
//             {
//                 hp += 1;
//             }
//             if (hp <= 0)
//             {
//                 Destroy(gameObject);
//             }
//         } 
//     }
