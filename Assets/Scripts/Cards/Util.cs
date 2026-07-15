using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Combat.UI;
using WordVenture.Core;

namespace WordVenture.Cards
{
    [System.Serializable]
    public class PRS
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;

        public PRS(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            this.pos = pos;
            this.rot = rot;
            this.scale = scale;
        }
    }

    public class Util : MonoBehaviour
    {
        public static Quaternion QI => Quaternion.identity;

        public static Vector3 MousePos
        {
            get
            {
                Vector3 result = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                result.z = -10;
                return result;
            }
        }
    }
}
