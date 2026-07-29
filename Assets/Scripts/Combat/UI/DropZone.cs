using UnityEngine;

namespace Combat.UI
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
