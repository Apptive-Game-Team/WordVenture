using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;
using Quaternion = UnityEngine.Quaternion;
using System;
using Unity.VisualScripting;

namespace Deck_Manage {
    public class CardManager : MonoBehaviour
    {
        public static CardManager Inst {get; private set;}
        void Awake() => Inst = this;

        public WordSO wordSO;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] List<Card> myCards;
        [SerializeField] Transform cardSpawnPoint;
        [SerializeField] Transform CardLeft;
        [SerializeField] Transform CardRight;
        [SerializeField] GameObject CardArea;
        [SerializeField] GameObject PushArea1;
        [SerializeField] GameObject PushArea2;
        [SerializeField] GameObject PushArea3;

        List<Word> wordBuffer;
        public Card selectCard;
        bool isMyCardDrag;
        bool onCardArea;
        bool onPushArea1;
        bool onPushArea2;
        bool onPushArea3;
        

        public Word PopWord()
        {
            if (wordBuffer.Count == 0)
                SetupWordBuffer();

            Word word = wordBuffer[0];
            wordBuffer.RemoveAt(0);
            return word;
        }

        void SetupWordBuffer()
        {
            wordBuffer = new List<Word>();
            for (int i = 0; i < wordSO.words.Length; i++)
            {
                Word word = wordSO.words[i];
                for (int j = 0;j < word.percent;j++)
                    wordBuffer.Add(word);
            }

            for (int i = 0; i < wordBuffer.Count; i++)
            {
                int rand = Random.Range(i, wordBuffer.Count);
                Word temp = wordBuffer[i];
                wordBuffer[i] = wordBuffer[rand];
                wordBuffer[rand] = temp;
            }
        }

        void Start()
        {
            //Map_scene.MapMove.StagePosition = 0;
            WordOS_state();
            SetupWordBuffer();
        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.U) && Map_scene.MapMove.StagePosition <= 4)
            //{
            //    Map_scene.MapMove.StagePosition++;
            //    WordOS_state();
            //    SetupWordBuffer();
            //}

            //if (Input.GetKeyDown(KeyCode.Space) && !isMyCardDrag)
            //    AddCard();

            DetectCardArea();
            if (isMyCardDrag)
            {
                // Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // mousePosition = new Vector3(mousePosition.x, mousePosition.y, selectCard.transform.position.z);
                // selectCard.transform.position = mousePosition;
                DragCard();
            }
            
        }

        public void AddCard()
        {
            var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
            var card = cardObject.GetComponent<Card>();
            card.Setup(PopWord());
            myCards.Add(card);

            SetOriginOrder();
            CardAlignment();
        }

        public void PopCard(Card Card)
        {
            myCards.Remove(Card);
        }

        void SetOriginOrder()
        {
            int count = myCards.Count;
            for (int i = 0;i < count;i++)
            {
                var targetCard = myCards[i];
                targetCard?.GetComponent<Order>().SetOriginOrder(i);
            }
        }

        public void CardAlignment()
        {
            List<PRS> originCardPRSs = new List<PRS>();
            originCardPRSs = RoundAlignment(CardLeft, CardRight, myCards.Count, 0.5f, new Vector3(1.896733f, 2.1f, 1) * 0.2f);

            var targetCards = myCards;

            for (int i = 0;i < targetCards.Count;i++) 
            {
                var targetCard = targetCards[i];

                targetCard.originPRS = originCardPRSs[i];
                //targetCard.originPRS = new PRS(Vector3.zero, Util.QI, new Vector3(1.896733f, 2.910432f, 1));
                targetCard.MoveTransform(targetCard.originPRS,true,0.7f);
            }

            CombineZone.Instance.spellCards.Clear();
            CombineZone.Instance.magicTypeCards.Clear(); 
        }

        List<PRS> RoundAlignment(Transform Left, Transform Right, int objCount, float height, Vector3 scale)
        {
            float[] objLerps = new float[objCount];
            List<PRS> results = new List<PRS>(objCount);

            float interval = 1f / (objCount+1);
            for (int i = 0;i < objCount;i++)
                objLerps[i] = interval * (i+1);
            
            for (int i = 0;i< objCount;i++)
            {
                var targetPos = Vector3.Lerp(Left.position, Right.position, objLerps[i]);
                targetPos.y += 0.5f;
                var targetRot = Quaternion.identity;

                // float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                // targetPos.y += curve;
                // targetRot = Quaternion.Slerp(Left.rotation, Right.rotation, objLerps[i]);

                results.Add(new PRS(targetPos, targetRot, scale));
            }
            return results;
        }



        void WordOS_state()
        {
            switch(Map_scene.MapMove.StagePosition)
            {
                case 0:
                    wordSO.words[0].percent = 1;
                    wordSO.words[1].percent = 0;
                    wordSO.words[2].percent = 0;
                    wordSO.words[3].percent = 1;
                    wordSO.words[4].percent = 0;
                    wordSO.words[5].percent = 0;
                    wordSO.words[6].percent = 0;
                    wordSO.words[7].percent = 0;
                    break;
                case 1:
                    wordSO.words[0].percent = 1;
                    wordSO.words[1].percent = 0;
                    wordSO.words[2].percent = 1;
                    wordSO.words[3].percent = 2;
                    wordSO.words[4].percent = 0;
                    wordSO.words[5].percent = 0;
                    wordSO.words[6].percent = 0;
                    wordSO.words[7].percent = 0;
                    break;
                case 2:
                    wordSO.words[0].percent = 1;
                    wordSO.words[1].percent = 0;
                    wordSO.words[2].percent = 1;
                    wordSO.words[3].percent = 1;
                    wordSO.words[4].percent = 0;
                    wordSO.words[5].percent = 1;
                    wordSO.words[6].percent = 0;
                    wordSO.words[7].percent = 0;
                    break;
                case 3:
                    wordSO.words[0].percent = 2;
                    wordSO.words[1].percent = 0;
                    wordSO.words[2].percent = 2;
                    wordSO.words[3].percent = 1;
                    wordSO.words[4].percent = 1;
                    wordSO.words[5].percent = 1;
                    wordSO.words[6].percent = 0;
                    wordSO.words[7].percent = 1;
                    break;
                case 4:
                    wordSO.words[0].percent = 5;
                    wordSO.words[1].percent = 5;
                    wordSO.words[2].percent = 5;
                    wordSO.words[3].percent = 3;
                    wordSO.words[4].percent = 3;
                    wordSO.words[5].percent = 3;
                    wordSO.words[6].percent = 3;
                    wordSO.words[7].percent = 3;
                    break;
                default:
                    break;
            }
        }

        #region MyCard

        public void CardMouseOver(Card card)
        {
            if(onCardArea)
            {
                EnlargeCard(true, card);
            }
        }

        public void CardMouseExit(Card card)
        {
            if(!onPushArea1 && !onPushArea2 && !onPushArea3)
            {
                EnlargeCard(false, card);
            }
        }

        public void CardMouseDown()
        {
            isMyCardDrag = true;
        }

        public void CardMouseUp()
        {
            isMyCardDrag = false;
            if (onPushArea1 && selectCard.CompareTag("Spell") && CombineZone.Instance.spellCards.Count == 0)
            {
                PushArea1.GetComponent<DropZone>().GetCard(selectCard.gameObject);
                selectCard.MoveTransform(new PRS(PushArea1.transform.position, Util.QI, selectCard.originPRS.scale), false);
            }
            else if (onPushArea2 && selectCard.CompareTag("MagicType") && CombineZone.Instance.magicTypeCards.Count == 0)
            {
                PushArea2.GetComponent<DropZone>().GetCard(selectCard.gameObject);
                selectCard.MoveTransform(new PRS(PushArea2.transform.position, Util.QI, selectCard.originPRS.scale), false);
            }
            else if (onPushArea3 && selectCard.CompareTag("Target"))
            {
                PushArea3.GetComponent<DropZone>().GetCard(selectCard.gameObject);
                selectCard.MoveTransform(new PRS(PushArea3.transform.position, Util.QI, selectCard.originPRS.scale), false);
            }
            else
            {
                if (selectCard.CompareTag("Spell"))
                {
                    CombineZone.Instance.spellCards.Clear();
                }
                if (selectCard.CompareTag("MagicType"))
                {
                    CombineZone.Instance.magicTypeCards.Clear();
                }

                selectCard.MoveTransform(selectCard.originPRS, false);
            }
        }

        void DragCard()
        {
            selectCard.MoveTransform(new PRS(Util.MousePos, Util.QI, selectCard.originPRS.scale), false);
        }

        void DetectCardArea()
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(Util.MousePos, Vector3.forward);

            onCardArea = false;
            onPushArea1 = false;
            onPushArea2 = false;

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == CardArea)
                    onCardArea = true;
                else if (hit.collider.gameObject == PushArea1)
                    onPushArea1 = true;
                else if (hit.collider.gameObject == PushArea2)
                    onPushArea2 = true;
            }
        }

        void EnlargeCard(bool isEnlarge, Card card)
        {
            // if (isEnlarge)
            // {
            //     Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -3f, -10f);
            //     card.MoveTransform(new PRS(enlargePos, Util.QI, new Vector3(1.896733f, 2.1f, 1) * 0.6f), false);
            // }
            // else
            //     card.MoveTransform(card.originPRS, false);

            // card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
        }

        #endregion
    }
}
