using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using DG.Tweening;

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

        public MagicType cardType;

        public Word word;
        public PRS originPRS;

        public void Setup(Word word)
        {
            this.word = word;
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

        void OnMouseOver()
        {
            CardManager.Inst.CardMouseOver(this);
        }

        void OnMouseExit()
        {
            CardManager.Inst.CardMouseExit(this);
        }

        void OnMouseDown() 
        {
            CardManager.Inst.CardMouseDown();
            CardManager.Inst.selectCard = this;
        }
        
        void OnMouseUp()
        {
            CardManager.Inst.CardMouseUp();
            CardManager.Inst.selectCard = this;
        }
    }
}
