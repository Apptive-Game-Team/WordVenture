using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicBattle
{
    public class Projectile : MonoBehaviour
    {
        Vector3 originPos;
        Vector3 destination;

        Vector3 travelDirection;
        float travelSpeed;

        DamageComponent damageComponent;

        public void SetDirection()
        {
            travelDirection = destination - originPos;
            travelDirection.Normalize();
        }

        private void Start()
        {
            transform.position = originPos;
            SetDirection();
        }

        private void Update()
        {
            transform.position += travelDirection * travelSpeed * Time.deltaTime;
        }
    }

}

