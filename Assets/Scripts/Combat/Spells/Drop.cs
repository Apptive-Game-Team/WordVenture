using Cards;
using Combat.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Spells
{

    public class Drop : MonoBehaviour
    {
        [FormerlySerializedAs("DropfirePrefab")] public GameObject dropfirePrefab;
        [FormerlySerializedAs("DropicePrefab")] public GameObject dropicePrefab;
        [FormerlySerializedAs("DroprockPrefab")] public GameObject droprockPrefab;
        [FormerlySerializedAs("DroplightningPrefab")] public GameObject droplightningPrefab;
        [FormerlySerializedAs("DropholyPrefab")] public GameObject dropholyPrefab;

        public void Run(MagicType magicType1, SelectableObject target, MagicAffinityTable magicAffinityTable)
        {
            //GameObject target = GameObject.FindGameObjectWithTag(magicType2.ToString());

            GameObject prefabToInstantiate = null;

            switch (magicType1)
            {
                case MagicType.Fire:
                    prefabToInstantiate = dropfirePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = dropicePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = droprockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = droplightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = dropholyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                Vector3 instantiatePos = target.transform.position + new Vector3 (0f ,10f ,0f) ;
                GameObject obj =  Instantiate(prefabToInstantiate, instantiatePos , prefabToInstantiate.transform.rotation);
                obj.GetComponent<SpellObj>().InitSpell(MagicType.Drop, magicType1, target, magicAffinityTable);
            }
        }
    }

}
