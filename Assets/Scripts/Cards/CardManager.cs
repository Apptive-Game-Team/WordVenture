using System;
using System.Collections.Generic;
using Combat.UI;
using Core;
using Map;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Cards
{
    public class CardManager : MonoBehaviour
    {
        public static CardManager Inst {get; private set;}

        // 레이어 이름은 실행 중 바뀌지 않는다. NameToLayer는 문자열 조회라 매 프레임
        // 부를 이유가 없다.
        int cardAreaLayer;
        int pushArea1Layer;
        int pushArea2Layer;
        int pushArea3Layer;

        // RaycastAll은 호출마다 새 배열을 만든다. 결과 List와 필터를 재사용하면
        // 프레임당 할당이 사라진다.
        readonly List<RaycastHit2D> areaHits = new List<RaycastHit2D>();
        ContactFilter2D areaHitFilter;

        void Awake()
        {
            Inst = this;

            cardAreaLayer = LayerMask.NameToLayer("CardArea");
            pushArea1Layer = LayerMask.NameToLayer("PushArea1");
            pushArea2Layer = LayerMask.NameToLayer("PushArea2");
            pushArea3Layer = LayerMask.NameToLayer("PushArea3");

            areaHitFilter = new ContactFilter2D().NoFilter();
        }

        [FormerlySerializedAs("wordSO")] [SerializeField] WordScriptableObject wordSo;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] List<Card> myCards;
        [SerializeField] Transform cardSpawnPoint;
        [FormerlySerializedAs("CardLeft")] [SerializeField] Transform cardLeft;
        [FormerlySerializedAs("CardRight")] [SerializeField] Transform cardRight;
        [FormerlySerializedAs("PushArea1")] [SerializeField] GameObject pushArea1;
        [FormerlySerializedAs("PushArea2")] [SerializeField] GameObject pushArea2;
        [FormerlySerializedAs("PushArea3")] [SerializeField] GameObject pushArea3;

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
            for (int i = 0; i < wordSo.words.Length; i++)
            {
                Word word = wordSo.words[i];
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
            //WordVenture.Map.MapMove.StagePosition = 0;
            WordOS_state();
            SetupWordBuffer();
        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.U) && WordVenture.Map.MapMove.StagePosition <= 4)
            //{
            //    WordVenture.Map.MapMove.StagePosition++;
            //    WordOS_state();
            //    SetupWordBuffer();
            //}

            //if (Input.GetKeyDown(KeyCode.Space) && !isMyCardDrag)
            //    AddCard();

            // 대화창이 떠 있는 동안 잡고 있던 카드는 놓지 못한다. 마우스 버튼을 떼는 입력도
            // 막히기 때문에, 잠기는 순간 드래그를 취소해 카드가 커서에 붙어 있지 않게 한다.
            if (InteractionLock.IsLocked)
            {
                CancelDrag();
                return;
            }

            // Util.MousePos는 접근할 때마다 Camera.main 조회와 좌표 변환을 다시 한다.
            // 한 프레임에 한 번만 구해서 돌려쓴다.
            Vector3 mouseWorldPosition = Util.MousePos;

            DetectCardArea(mouseWorldPosition);
            if (isMyCardDrag)
            {
                DragCard(mouseWorldPosition);
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

        public void PopCard(Card card)
        {
            myCards.Remove(card);
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
            List<Prs> originCardPrSs = new List<Prs>();
            originCardPrSs = RoundAlignment(cardLeft, cardRight, myCards.Count, 0.5f, new Vector3(1.896733f, 2.1f, 1) * 0.2f);

            var targetCards = myCards;

            for (int i = 0;i < targetCards.Count;i++)
            {
                var targetCard = targetCards[i];

                targetCard.originPrs = originCardPrSs[i];
                //targetCard.originPRS = new PRS(Vector3.zero, Util.QI, new Vector3(1.896733f, 2.910432f, 1));
                targetCard.MoveTransform(targetCard.originPrs,true,0.7f);
            }

            CombineZone.Instance.spellCards.Clear();
            CombineZone.Instance.magicTypeCards.Clear();
        }

        List<Prs> RoundAlignment(Transform left, Transform right, int objCount, float height, Vector3 scale)
        {
            float[] objLerps = new float[objCount];
            List<Prs> results = new List<Prs>(objCount);

            float interval = 1f / (objCount+1);
            for (int i = 0;i < objCount;i++)
                objLerps[i] = interval * (i+1);

            for (int i = 0;i< objCount;i++)
            {
                var targetPos = Vector3.Lerp(left.position, right.position, objLerps[i]);
                targetPos.y += 0.5f;
                var targetRot = Quaternion.identity;

                // float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                // targetPos.y += curve;
                // targetRot = Quaternion.Slerp(Left.rotation, Right.rotation, objLerps[i]);

                results.Add(new Prs(targetPos, targetRot, scale));
            }
            return results;
        }



        void WordOS_state()
        {
            switch(MapMove.StagePosition)
            {
                case 0:
                    wordSo.words[0].percent = 1;
                    wordSo.words[1].percent = 0;
                    wordSo.words[2].percent = 0;
                    wordSo.words[3].percent = 1;
                    wordSo.words[4].percent = 0;
                    wordSo.words[5].percent = 0;
                    wordSo.words[6].percent = 0;
                    wordSo.words[7].percent = 0;
                    break;
                case 1:
                    wordSo.words[0].percent = 1;
                    wordSo.words[1].percent = 0;
                    wordSo.words[2].percent = 1;
                    wordSo.words[3].percent = 2;
                    wordSo.words[4].percent = 0;
                    wordSo.words[5].percent = 0;
                    wordSo.words[6].percent = 0;
                    wordSo.words[7].percent = 0;
                    break;
                case 2:
                    wordSo.words[0].percent = 1;
                    wordSo.words[1].percent = 0;
                    wordSo.words[2].percent = 1;
                    wordSo.words[3].percent = 1;
                    wordSo.words[4].percent = 0;
                    wordSo.words[5].percent = 1;
                    wordSo.words[6].percent = 0;
                    wordSo.words[7].percent = 0;
                    break;
                case 3:
                    wordSo.words[0].percent = 2;
                    wordSo.words[1].percent = 0;
                    wordSo.words[2].percent = 2;
                    wordSo.words[3].percent = 1;
                    wordSo.words[4].percent = 1;
                    wordSo.words[5].percent = 1;
                    wordSo.words[6].percent = 0;
                    wordSo.words[7].percent = 1;
                    break;
                case 4:
                    wordSo.words[0].percent = 5;
                    wordSo.words[1].percent = 5;
                    wordSo.words[2].percent = 5;
                    wordSo.words[3].percent = 3;
                    wordSo.words[4].percent = 3;
                    wordSo.words[5].percent = 3;
                    wordSo.words[6].percent = 3;
                    wordSo.words[7].percent = 3;
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
                pushArea1.GetComponent<DropZone>().GetCard(selectCard.gameObject);
                selectCard.MoveTransform(new Prs(pushArea1.transform.position, Util.Qi, selectCard.originPrs.scale), false);
            }
            else if (onPushArea2 && selectCard.CompareTag("MagicType") && CombineZone.Instance.magicTypeCards.Count == 0)
            {
                pushArea2.GetComponent<DropZone>().GetCard(selectCard.gameObject);
                selectCard.MoveTransform(new Prs(pushArea2.transform.position, Util.Qi, selectCard.originPrs.scale), false);
            }
            else if (onPushArea3 && selectCard.CompareTag("Target"))
            {
                pushArea3.GetComponent<DropZone>().GetCard(selectCard.gameObject);
                selectCard.MoveTransform(new Prs(pushArea3.transform.position, Util.Qi, selectCard.originPrs.scale), false);
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

                selectCard.MoveTransform(selectCard.originPrs, false);
            }
        }

        void CancelDrag()
        {
            if (!isMyCardDrag)
            {
                return;
            }

            isMyCardDrag = false;
            if (selectCard != null)
            {
                selectCard.MoveTransform(selectCard.originPrs, false);
            }
        }

        void DragCard(Vector3 mouseWorldPosition)
        {
            selectCard.MoveTransform(new Prs(mouseWorldPosition, Util.Qi, selectCard.originPrs.scale), false);
        }

        void DetectCardArea(Vector3 mouseWorldPosition)
        {
            onCardArea = false;
            onPushArea1 = false;
            onPushArea2 = false;
            onPushArea3 = false;

            // 원래 방향 인자는 Vector3.forward였는데, Vector2로 변환되며 z가 잘려
            // 사실상 Vector2.zero였다. 같은 값을 그대로 명시한다.
            Physics2D.Raycast(mouseWorldPosition, Vector2.zero, areaHitFilter, areaHits);

            // 레이어별로 Array.Exists를 따로 돌리면 결과를 네 번 훑고 그때마다
            // 지역 변수를 캡처한 델리게이트가 새로 할당된다. 한 번만 순회한다.
            for (int i = 0; i < areaHits.Count; i++)
            {
                int layer = areaHits[i].collider.gameObject.layer;

                if (layer == cardAreaLayer)
                {
                    onCardArea = true;
                }
                else if (layer == pushArea1Layer)
                {
                    onPushArea1 = true;
                }
                else if (layer == pushArea2Layer)
                {
                    onPushArea2 = true;
                }
                else if (layer == pushArea3Layer)
                {
                    onPushArea3 = true;
                }
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
