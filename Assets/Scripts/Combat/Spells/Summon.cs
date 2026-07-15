using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Spells
{

    public class Summon : MonoBehaviour
    {
        public GameObject player;
        private float summonRadius = 2.0f;
        public GameObject SummonfirePrefab;
        public GameObject SummonicePrefab;
        public GameObject SummonrockPrefab;
        public GameObject SummonlightningPrefab;
        public GameObject SummonHolyPrefab;

        public void summon(MagicType magicType, SelectableObject target, WordVenture.Combat.MagicAffinityTable magicAffinityTable)
        {

            GameObject prefabToInstantiate = null;

            switch (magicType)
            {
                case MagicType.Fire:
                    prefabToInstantiate = SummonfirePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = SummonicePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = SummonrockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = SummonlightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = SummonHolyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                //Vector3 instantiatePos = //GetRndPos(target.transform.position + new Vector3(0, -1 * target.transform.position.y, 0), summonRadius);

                GameObject obj = Instantiate(prefabToInstantiate, target.transform.position + new Vector3(0, -1 * target.transform.position.y, 0), Quaternion.identity);
                obj.GetComponent<SpellObj>().InitSpell(MagicType.Summon, magicType, target, magicAffinityTable);
            }
        }

        //private Vector3 GetRndPos(Vector3 center, float radius)
        //{
        //    Vector3 randomPos = Random.insideUnitSphere * radius;
        //    randomPos.y = Mathf.Abs(randomPos.y); // y축 양수제한

        //    return center + randomPos;
        //}
    }

}
