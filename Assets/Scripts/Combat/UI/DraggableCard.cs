using UnityEngine.EventSystems;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;
using WordVenture.Combat.Spells;

namespace WordVenture.Combat.UI
{

    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private CombineZone combineZone;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;

            if (combineZone != null && combineZone.CompareTag(tag))
            {
                combineZone.AddCard(gameObject);
            }
            else
            {
                transform.SetParent(originalParent);
                transform.localPosition = Vector3.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(tag))
            {
                combineZone = other.GetComponent<CombineZone>();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(tag))
            {
                combineZone = null;
            }
        }
    }

}
