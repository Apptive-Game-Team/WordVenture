using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Deck_Manage {


    public enum MagicType
    {
        Shoot,
        Summon,
        Drop,
        Holy,
        Fire,
        Ice,
        Rock,
        Lightning,
        Undead
    }

    public class Card : MonoBehaviour
    {
        [SerializeField] TMP_Text nameTMP;
        [SerializeField] Sprite MagicCard;
        [SerializeField] Sprite TypeCard;

        public MagicType cardType;

        public Word word;
        public PRS originPRS;

        public void Setup(Word word)
        {
            this.word = word;
            if (word.tag == "Spell")
                this.GetComponent<SpriteRenderer>().sprite = MagicCard;
            else
                this.GetComponent<SpriteRenderer>().sprite = TypeCard;
            nameTMP.text = this.word.name;
            cardType = this.word.magicType;
            gameObject.tag = this.word.tag;
        }

        public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
        {
            if (useDotween)
            {
                transform.DOMove(prs.pos, dotweenTime);
                transform.DORotateQuaternion(prs.rot, dotweenTime);
                transform.DOScale(prs.scale, dotweenTime);
            }
            else
            {
                transform.position = prs.pos;
                transform.rotation = prs.rot;
                transform.localScale = prs.scale;
            }
        }

        void OnMouseDown() 
        {
            // CardManager.Inst.CardMouseDown();
            // CardManager.Inst.selectCard = this;
            CheckHighestCard();
        }
        
        void OnMouseUp()
        {
            CardManager.Inst.CardMouseUp();
            CardManager.Inst.selectCard = this;
        }

        void CheckHighestCard()
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);
            
            if (hits.Length > 0)
            {
                RaycastHit2D topLayerHit = hits[0];
                SpriteRenderer topSpriteRenderer = topLayerHit.transform.gameObject.GetComponent<SpriteRenderer>();

                int highestSortingOrder = (topSpriteRenderer != null) ? topSpriteRenderer.sortingOrder : int.MinValue;

                foreach (RaycastHit2D hit in hits)
                {
                    SpriteRenderer spriteRenderer = hit.transform.gameObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        if (spriteRenderer.sortingOrder > highestSortingOrder)
                        {
                            highestSortingOrder = spriteRenderer.sortingOrder;
                            topLayerHit = hit;
                        }
                    }
                }

                Card card = topLayerHit.transform.gameObject.GetComponent<Card>();
                if (card != null)
                {
                    CardManager.Inst.CardMouseDown();
                    CardManager.Inst.selectCard = card;
                }
            }
        }
    }
}
