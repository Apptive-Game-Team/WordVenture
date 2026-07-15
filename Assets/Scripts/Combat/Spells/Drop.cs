using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Spells
{

    public class Drop : MonoBehaviour
    {
        public GameObject DropfirePrefab;
        public GameObject DropicePrefab;
        public GameObject DroprockPrefab;
        public GameObject DroplightningPrefab;
        public GameObject DropholyPrefab;

        public void drop(WordVenture.Cards.MagicType magicType1, SelectableObject target, WordVenture.Combat.MagicAffinityTable magicAffinityTable)
        {
            //GameObject target = GameObject.FindGameObjectWithTag(magicType2.ToString());

            GameObject prefabToInstantiate = null;

            switch (magicType1)
            {
                case WordVenture.Cards.MagicType.Fire:
                    prefabToInstantiate = DropfirePrefab;
                    break;
                case WordVenture.Cards.MagicType.Ice:
                    prefabToInstantiate = DropicePrefab;
                    break;
                case WordVenture.Cards.MagicType.Rock:
                    prefabToInstantiate = DroprockPrefab;
                    break;
                case WordVenture.Cards.MagicType.Lightning:
                    prefabToInstantiate = DroplightningPrefab;
                    break;
                case WordVenture.Cards.MagicType.Holy:
                    prefabToInstantiate = DropholyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                Vector3 InstantiatePos = target.transform.position + new Vector3 (0f ,10f ,0f) ;
                GameObject obj =  Instantiate(prefabToInstantiate, InstantiatePos , prefabToInstantiate.transform.rotation);
                obj.GetComponent<SpellObj>().InitSpell(MagicType.Drop, magicType1, target, magicAffinityTable);
            }
        }
    }

}
