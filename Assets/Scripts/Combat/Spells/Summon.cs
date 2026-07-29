using Cards;
using Combat.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Spells
{

    public class Summon : MonoBehaviour
    {
        public GameObject player;
        private float summonRadius = 2.0f;
        [FormerlySerializedAs("SummonfirePrefab")] public GameObject summonfirePrefab;
        [FormerlySerializedAs("SummonicePrefab")] public GameObject summonicePrefab;
        [FormerlySerializedAs("SummonrockPrefab")] public GameObject summonrockPrefab;
        [FormerlySerializedAs("SummonlightningPrefab")] public GameObject summonlightningPrefab;
        [FormerlySerializedAs("SummonHolyPrefab")] public GameObject summonHolyPrefab;

        public void Run(MagicType magicType, SelectableObject target, MagicAffinityTable magicAffinityTable)
        {

            GameObject prefabToInstantiate = null;

            switch (magicType)
            {
                case MagicType.Fire:
                    prefabToInstantiate = summonfirePrefab;
                    break;
                case MagicType.Ice:
                    prefabToInstantiate = summonicePrefab;
                    break;
                case MagicType.Rock:
                    prefabToInstantiate = summonrockPrefab;
                    break;
                case MagicType.Lightning:
                    prefabToInstantiate = summonlightningPrefab;
                    break;
                case MagicType.Holy:
                    prefabToInstantiate = summonHolyPrefab;
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
