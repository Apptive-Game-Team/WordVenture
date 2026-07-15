using WordVenture.Cards;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Spells
{
    //using System.Collections;
    //using System.Collections.Generic;
    //using UnityEngine;

    //public class Heal : MonoBehaviour
    //{
    //    public GameObject HealfirePrefab;
    //    public GameObject HealicePrefab;
    //    public GameObject HealrockPrefab;
    //    public GameObject HeallightningPrefab;

    //    public void heal(WordVenture.Cards.MagicType magicType1, WordVenture.Cards.MagicType magicType2, SelectableObject target)
    //    {

    //        GameObject prefabToInstantiate = null;

    //        switch (magicType1)
    //        {
    //            case WordVenture.Cards.MagicType.Fire:
    //                prefabToInstantiate = HealfirePrefab;
    //                break;
    //            case WordVenture.Cards.MagicType.Ice:
    //                prefabToInstantiate = HealicePrefab;
    //                break;
    //            case WordVenture.Cards.MagicType.Rock:
    //                prefabToInstantiate = HealrockPrefab;
    //                break;
    //            case WordVenture.Cards.MagicType.Lightning:
    //                prefabToInstantiate = HeallightningPrefab;
    //                break;
    //        }

    //        if (prefabToInstantiate != null)
    //        {
    //            GameObject obj = Instantiate(prefabToInstantiate, target.transform.position, Quaternion.identity);
    //            obj.GetComponent<SpellObj>().InitSpell(WordVenture.Cards.MagicType.Heal, magicType1, target);
    //        }
    //    }
    //}

}
