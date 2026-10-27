using System;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cards
{
    public enum MagicType
    {
        Shoot,
        Explode,
        Drop,
        Holy,
        Fire,
        Ice,
        Rock,
        Lightning,
        Undead,
        // 카드 데이터가 enum을 정수로 저장한다. 새 값은 맨 끝에만 추가한다.
        Spawn,
        Water,
        Plant
    }

    [Serializable]
    public struct WordSprite
    {
        public string name;
        public Sprite sprite;
    }

    public class Card : MonoBehaviour
    {
        // 이름 TMP는 계층에서 첫 TMP_Text여야 한다. GetComponentInChildren<TMP_Text>()로 카드 이름을 찾는 코드가 있다.
        [SerializeField] TMP_Text nameTMP;
        [SerializeField] TMP_Text kindTMP;
        [FormerlySerializedAs("MagicCard")] [SerializeField] Sprite magicCard;
        [FormerlySerializedAs("TypeCard")] [SerializeField] Sprite typeCard;
        [SerializeField] WordSprite[] wordSprites;

        public MagicType cardType;

        public Word word;
        [FormerlySerializedAs("originPRS")] public Prs originPrs;

        public void Setup(Word word)
        {
            this.word = word;
            bool isSpell = word.tag == "Spell";
            Sprite sprite = FindWordSprite(word.name);
            if (sprite == null) sprite = isSpell ? magicCard : typeCard;
            this.GetComponent<SpriteRenderer>().sprite = sprite;
            nameTMP.text = this.word.name;
            if (kindTMP != null)
                kindTMP.text = Localization.Translate(isSpell ? "마법" : "속성");
            cardType = this.word.magicType;
            gameObject.tag = this.word.tag;
        }

        Sprite FindWordSprite(string wordName)
        {
            if (wordSprites == null) return null;
            foreach (WordSprite wordSprite in wordSprites)
            {
                if (wordSprite.name == wordName && wordSprite.sprite != null) return wordSprite.sprite;
            }
            return null;
        }

        public void MoveTransform(Prs prs, bool useDotween, float dotweenTime = 0)
        {
            // 올라오거나 내려가는 tween이 남아 있으면 새 이동이나 드래그를 덮어쓴다.
            transform.DOKill();
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

        void OnMouseOver()
        {
            if (InteractionLock.IsLocked)
            {
                return;
            }

            CardManager.Inst.CardMouseOver(this);
        }

        void OnMouseExit()
        {
            CardManager.Inst.CardMouseExit(this);
        }

        void OnMouseDown()
        {
            // 튜토리얼 대화창은 UI라서 콜라이더 클릭을 가리지 못한다. 직접 막는다.
            if (InteractionLock.IsLocked)
            {
                return;
            }

            // CardManager.Inst.CardMouseDown();
            // CardManager.Inst.selectCard = this;
            CheckHighestCard();
        }

        void OnMouseUp()
        {
            if (InteractionLock.IsLocked)
            {
                return;
            }

            CardManager.Inst.CardMouseUp();
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
                    CardManager.Inst.selectCard = card;
                    CardManager.Inst.CardMouseDown();
                }
            }
        }
    }
}
