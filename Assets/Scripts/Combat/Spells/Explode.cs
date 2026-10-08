using Cards;
using Combat.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Spells
{

    public class Explode : MonoBehaviour
    {
        public GameObject player;
        [FormerlySerializedAs("SummonfirePrefab")] [FormerlySerializedAs("summonfirePrefab")] public GameObject firePrefab;
        [FormerlySerializedAs("SummonicePrefab")] [FormerlySerializedAs("summonicePrefab")] public GameObject icePrefab;
        [FormerlySerializedAs("SummonrockPrefab")] [FormerlySerializedAs("summonrockPrefab")] public GameObject rockPrefab;
        [FormerlySerializedAs("SummonlightningPrefab")] [FormerlySerializedAs("summonlightningPrefab")] public GameObject lightningPrefab;
        [FormerlySerializedAs("SummonHolyPrefab")] [FormerlySerializedAs("summonHolyPrefab")] public GameObject holyPrefab;

        public void Run(MagicType magicType, SelectableObject target, MagicAffinityTable magicAffinityTable)
        {

            GameObject prefabToInstantiate = null;

            switch (magicType)
            {
                case MagicType.Fire:
                    prefabToInstantiate = firePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = icePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = rockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = lightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = holyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {

                GameObject obj = Instantiate(prefabToInstantiate, target.transform.position + new Vector3(0, -1 * target.transform.position.y, 0), Quaternion.identity);
                obj.GetComponent<SpellObj>().InitSpell(MagicType.Explode, magicType, target, magicAffinityTable);
            }
        }

    }

}
