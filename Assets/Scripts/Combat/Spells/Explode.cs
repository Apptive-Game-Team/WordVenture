using Cards;
using Combat.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Spells
{

    public class Explode : MonoBehaviour
    {
        public GameObject player;
        [FormerlySerializedAs("SummonfirePrefab")] public GameObject explodeFirePrefab;
        [FormerlySerializedAs("SummonicePrefab")] public GameObject explodeIcePrefab;
        [FormerlySerializedAs("SummonrockPrefab")] public GameObject explodeRockPrefab;
        [FormerlySerializedAs("SummonlightningPrefab")] public GameObject explodeLightningPrefab;
        [FormerlySerializedAs("SummonHolyPrefab")] public GameObject explodeHolyPrefab;

        public void Run(MagicType magicType, SelectableObject target, MagicAffinityTable magicAffinityTable)
        {

            GameObject prefabToInstantiate = null;

            switch (magicType)
            {
                case MagicType.Fire:
                    prefabToInstantiate = explodeFirePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = explodeIcePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = explodeRockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = explodeLightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = explodeHolyPrefab;
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
