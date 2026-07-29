using Cards;
using Combat.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Spells
{
    public class Shoot : MonoBehaviour
    {
        [FormerlySerializedAs("ShootfirePrefab")] public GameObject shootfirePrefab;
        [FormerlySerializedAs("ShooticePrefab")] public GameObject shooticePrefab;
        [FormerlySerializedAs("ShootrockPrefab")] public GameObject shootrockPrefab;
        [FormerlySerializedAs("ShootlightningPrefab")] public GameObject shootlightningPrefab;
        [FormerlySerializedAs("ShootHolyPrefab")] public GameObject shootHolyPrefab;

        public void Run(MagicType magicType1, SelectableObject target, MagicAffinityTable magicAffinityTable)
        {

            GameObject prefabToInstantiate = null;

            switch (magicType1)
            {
                case MagicType.Fire:
                    prefabToInstantiate = shootfirePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = shooticePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = shootrockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = shootlightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = shootHolyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                GameObject obj = Instantiate(prefabToInstantiate, Player.PlayerInt().transform.position, prefabToInstantiate.transform.rotation);

                obj.GetComponent<SpellObj>().InitSpell(MagicType.Shoot, magicType1, target, magicAffinityTable);
            }
        }
    }
}
