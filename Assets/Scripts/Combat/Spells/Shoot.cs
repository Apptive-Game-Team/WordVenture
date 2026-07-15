using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Spells
{
    public class Shoot : MonoBehaviour
    {
        public GameObject ShootfirePrefab;
        public GameObject ShooticePrefab;
        public GameObject ShootrockPrefab;
        public GameObject ShootlightningPrefab;
        public GameObject ShootHolyPrefab;

        public void shoot(MagicType magicType1, SelectableObject target, WordVenture.Combat.MagicAffinityTable magicAffinityTable)
        {

            GameObject prefabToInstantiate = null;

            switch (magicType1)
            {
                case MagicType.Fire:
                    prefabToInstantiate = ShootfirePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = ShooticePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = ShootrockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = ShootlightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = ShootHolyPrefab;
                    break;
            }

            if (prefabToInstantiate != null)
            {
                GameObject obj = Instantiate(prefabToInstantiate, WordVenture.Combat.Enemies.Player.PlayerInt().transform.position, prefabToInstantiate.transform.rotation);

                obj.GetComponent<SpellObj>().InitSpell(MagicType.Shoot, magicType1, target, magicAffinityTable);
            }
        }
    }
}
