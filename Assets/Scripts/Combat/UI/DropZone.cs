using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;
using WordVenture.Combat.Spells;

namespace WordVenture.Combat.UI
{

    public class DropZone : MonoBehaviour
    {
        [SerializeField] CombineZone combineZone;

        public void GetCard(GameObject card)
        {
            combineZone.AddCard(card);
        }
    }

}
