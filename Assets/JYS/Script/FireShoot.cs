using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class FireShoot : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position = transform.position + new Vector3(-10, 0, 0) * Time.deltaTime;
        }
    }

}