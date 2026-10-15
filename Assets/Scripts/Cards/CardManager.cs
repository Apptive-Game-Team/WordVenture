using System;
using System.Collections.Generic;
using Battle.Turns;
using Combat.UI;
using Core;
using DG.Tweening;
using Map;
using Story;
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

        // Spawn 카드는 서리 마을(스테이지 5)을 클리어한 뒤 스테이지 6부터 손패에 섞인다.
        const int SpawnUnlockStage = PlayerUpgrades.SpawnUnlockStage;
        const int SpawnCardPercent = 3;

        [FormerlySerializedAs("wordSO")] [SerializeField] WordScriptableObject wordSo;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] List<Card> myCards;
        [SerializeField] Transform cardSpawnPoint;
        [FormerlySerializedAs("CardLeft")] [SerializeField] Transform cardLeft;
        [FormerlySerializedAs("CardRight")] [SerializeField] Transform cardRight;
        [FormerlySerializedAs("PushArea1")] [SerializeField] GameObject pushArea1;
        [FormerlySerializedAs("PushArea2")] [SerializeField] GameObject pushArea2;
        [FormerlySerializedAs("PushArea3")] [SerializeField] GameObject pushArea3;

        // 마법 카드와 타입 카드를 따로 섞어 둔다. 한 더미에서 뽑으면 한 턴에 같은 종류만
        // 두 장 들어와 조합할 수 없는 턴이 생긴다.
        readonly Dictionary<string, List<Word>> wordBuffers = new Dictionary<string, List<Word>>();
        public Card selectCard;
        bool isMyCardDrag;
        bool onCardArea;
        bool onPushArea1;
        bool onPushArea2;
        bool onPushArea3;
        Prs dragStartPrs;
        Card hoveredCard;

        // 손패는 카드 위쪽만 보이게 내려 둔다. 땅 위를 걷는 슬라임이 카드 뒤로 가려지지 않게 하려는 것이다.
        // 480x270 가상 화면의 29픽셀(1픽셀 = 1/27 유닛)이다. 카드 높이 1.85 중 약 절반이 화면 아래로 내려간다.
        // 버리기 칸과 불타는 자리는 cardLeft 기준 그대로라서 이 값을 적용하지 않는다.
        public const float HandLowerOffset = 29f / 27f;
        const float HoverMoveSeconds = 0.12f;

        public IReadOnlyList<Card> HandCards => myCards;
        public bool IsDraggingCard => isMyCardDrag;

        public Card GetTutorialCard(string tag)
        {
            foreach (Card card in myCards)
            {
                if (card == null || !card.gameObject.activeInHierarchy || !card.CompareTag(tag)) continue;
                if (CombineZone.Instance != null && (CombineZone.Instance.spellCards.Contains(card.gameObject)
                    || CombineZone.Instance.magicTypeCards.Contains(card.gameObject))) continue;
                if (tag == "MagicType" && card.cardType != MagicType.Fire) continue;
                return card;
            }
            return null;
        }

        public Transform GetTutorialSlot(string tag)
        {
            GameObject slot = tag == "Spell" ? pushArea1 : pushArea2;
            return slot != null ? slot.transform : null;
        }


        public Word PopWord(string tag)
        {
            if (!wordBuffers.TryGetValue(tag, out List<Word> wordBuffer) || wordBuffer.Count == 0)
                wordBuffer = SetupWordBuffer(tag);

            Word word = wordBuffer[0];
            wordBuffer.RemoveAt(0);
            return word;
        }

        List<Word> SetupWordBuffer(string tag)
        {
            List<Word> wordBuffer = new List<Word>();
            for (int i = 0; i < wordSo.words.Length; i++)
            {
                Word word = wordSo.words[i];
                if (word.tag != tag) continue;
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

            wordBuffers[tag] = wordBuffer;
            return wordBuffer;
        }

        void Start()
        {
            //WordVenture.Map.MapMove.StagePosition = 0;
            WordOS_state();
            CreateDiscardSpot();
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
                ReleaseHover(true);
                return;
            }

#if UNITY_EDITOR
            // 테스트용: 에디터에서 S 키로 Spawn 카드를 한 장 받는다. 빌드에는 들어가지 않는다.
            if (Input.GetKeyDown(KeyCode.S))
            {
                AddCard(FindWord(MagicType.Spawn));
            }
#endif

            // Util.MousePos는 접근할 때마다 Camera.main 조회와 좌표 변환을 다시 한다.
            // 한 프레임에 한 번만 구해서 돌려쓴다.
            Vector3 mouseWorldPosition = Util.MousePos;

            DetectCardArea(mouseWorldPosition);
            if (isMyCardDrag)
            {
                DragCard(mouseWorldPosition);
            }
            RefreshDiscardSpot(mouseWorldPosition);

        }

        public void AddCard(string tag)
        {
            AddCard(PopWord(tag));
        }

        void AddCard(Word word)
        {
            if (word == null) return;
            var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
            var card = cardObject.GetComponent<Card>();
            card.Setup(word);

            // 손패가 가득 차면 뽑은 카드는 손패에 들어가지 않고 불타 사라진다. 한도는 주문 카드와 속성 카드에 반씩 따로 건다.
            // 한 장을 버린 뒤 주문 카드만 들어오고 속성 카드가 타는 일이 반복되면 짝이 깨져 조합할 수 없게 된다.
            // 조합칸에서 돌아오는 카드는 이 길을 거치지 않으므로 한도를 넘어도 태우지 않는다.
            if (CountInHand(word.tag) >= PlayerUpgrades.HandLimitPerKind)
            {
                BurnCard(card);
                if (!GameTips.HasSeen(GameTip.HandLimit)) StartCoroutine(ShowHandLimitTip());
                return;
            }

            myCards.Add(card);

            SetOriginOrder();
            CardAlignment();
        }

        // 같은 턴에 두 장이 넘치면 나란히 서서 어긋나게 탄다.
        int activeBurns;

        void BurnCard(Card card)
        {
            float x = (cardLeft.position.x + cardRight.position.x) * 0.5f + (activeBurns % 2 == 0 ? -1f : 1f) * 1.2f;
            float y = cardLeft.position.y + 4f;
            BurnCard(card, new Vector3(x, y, card.transform.position.z));
        }

        void BurnCard(Card card, Vector3 burnPosition)
        {
            float delay = activeBurns * 0.15f;
            activeBurns++;
            CardBurnVfx.Play(card, burnPosition, delay);
            StartCoroutine(ReleaseBurnSlot());
        }

        // 카드가 타는 모습을 먼저 보여 준 뒤 설명한다. 턴 시작 뒤 적 턴이 아니라 플레이어 턴에 뜬다.
        System.Collections.IEnumerator ShowHandLimitTip()
        {
            yield return new WaitForSeconds(0.8f);
            yield return GameTips.ShowOnce(GameTip.HandLimit);
        }

        System.Collections.IEnumerator ReleaseBurnSlot()
        {
            yield return new WaitForSeconds(1.2f);
            activeBurns = Mathf.Max(0, activeBurns - 1);
        }

        #region Discard

        // 손패 오른쪽 옆. 가장자리 카드(손패 끝에서 약 0.7)와 겹치지 않고 화면 오른쪽 끝(월드 x 8.9) 안에 든다.
        // 손패 오른쪽은 적이 다가오는 자리라 워드와 손패 사이에 둔다.
        const float DiscardSpotOffsetX = -0.35f;
        // 한 턴에 버릴 수 있는 카드 수는 1장이다. 버린 턴 번호를 기억해 다음 턴이 되면 풀린다.
        int lastDiscardTurn = -1;
        DiscardSpot discardSpot;

        public bool CanDiscard
        {
            get
            {
                if (InteractionLock.IsLocked || IsDiscardBlockedByTutorial()) return false;
                TurnBattleSystem battle = TurnBattleSystem.Instance;
                if (battle == null || !battle.IsPlayerTurn || battle.IsEndingPlayerTurn) return false;
                CombineZone zone = CombineZone.Instance;
                if (zone != null && (zone.IsCasting || zone.IsAwaitingTarget)) return false;
                return lastDiscardTurn != battle.TurnNumber;
            }
        }

        // 튜토리얼은 정해진 카드만 정해진 칸에 놓게 안내하므로 끝나기 전에는 버릴 수 없다.
        static bool IsDiscardBlockedByTutorial()
        {
            return global::Tutorial.TutorialController.Instance != null && !SaveLoadController.IsTutorialEnded;
        }

        // 손패의 카드 한 장을 태워 버린다. 카드를 쓴 것이 아니므로 BattleStats.CardsUsed는 올리지 않는다.
        public bool TryDiscard(Card card)
        {
            if (card == null || !myCards.Contains(card) || !CanDiscard) return false;

            lastDiscardTurn = TurnBattleSystem.Instance.TurnNumber;
            // 조합칸에 올려 둔 카드도 버릴 수 있으므로 칸 목록에서 먼저 뺀다.
            CombineZone.Instance.spellCards.Remove(card.gameObject);
            CombineZone.Instance.magicTypeCards.Remove(card.gameObject);
            myCards.Remove(card);
            if (selectCard == card) selectCard = null;

            // 손패 정렬이 걸어 둔 이동 tween이 남아 있으면 불타는 카드와 위치를 다투므로 끊는다.
            card.transform.DOKill();
            Vector3 burnPosition = card.transform.position;
            if (discardSpot != null) burnPosition = new Vector3(discardSpot.Center.x, discardSpot.Center.y, burnPosition.z);
            BurnCard(card, burnPosition);
            SetOriginOrder();
            CardAlignment();
            return true;
        }

        void CreateDiscardSpot()
        {
            if (cardLeft == null || cardRight == null || cardPrefab == null) return;
            var cardRenderer = cardPrefab.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(cardLeft.position.x + DiscardSpotOffsetX, cardLeft.position.y + 1.2f, 0f);
            discardSpot = DiscardSpot.Create(position, cardRenderer != null ? cardRenderer.sortingLayerID : 0);
        }

        void RefreshDiscardSpot(Vector3 mouseWorldPosition)
        {
            if (discardSpot == null) return;
            bool tutorial = IsDiscardBlockedByTutorial();
            if (discardSpot.gameObject.activeSelf == tutorial) discardSpot.gameObject.SetActive(!tutorial);
            if (tutorial) return;
            discardSpot.SetState(CanDiscard, isMyCardDrag && discardSpot.Contains(mouseWorldPosition));
        }

        #endregion

        int CountInHand(string tag)
        {
            int count = 0;
            foreach (Card held in myCards)
                if (held != null && held.word.tag == tag) count++;
            return count;
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
            // 정렬이 모든 카드를 쉬는 자리로 보내므로 올라와 있던 카드도 함께 내려간다. 커서가 아직 카드 위면 다음 프레임에 다시 올라온다.
            ReleaseHover(false);
            List<Prs> originCardPrSs = new List<Prs>();
            originCardPrSs = RoundAlignment(cardLeft, cardRight, myCards.Count, 0.5f, Vector3.one * 0.4f);

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
                // 손패 기준점(CardLeft, CardRight)은 화면 아래 끝보다 조금 아래에 있다. 카드 높이 1.85의 절반보다 더 올려야 카드 아래 테두리가 화면에 들어온다.
                targetPos.y += 1.2f - HandLowerOffset;
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
                    // 스테이지 5 이후는 스테이지 4와 같은 비율을 쓴다.
                    wordSo.words[0].percent = 5;
                    wordSo.words[1].percent = 5;
                    wordSo.words[2].percent = 5;
                    wordSo.words[3].percent = 3;
                    wordSo.words[4].percent = 3;
                    wordSo.words[5].percent = 3;
                    wordSo.words[6].percent = 3;
                    wordSo.words[7].percent = 3;
                    break;
            }

            Word spawnWord = FindWord(MagicType.Spawn);
            if (spawnWord != null)
            {
                spawnWord.percent = MapMove.StagePosition >= SpawnUnlockStage ? SpawnCardPercent : 0;
            }
        }

        Word FindWord(MagicType magicType)
        {
            return Array.Find(wordSo.words, word => word.magicType == magicType);
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

        bool IsRestingInHand(Card card)
        {
            if (card == null || !myCards.Contains(card)) return false;
            CombineZone zone = CombineZone.Instance;
            return zone == null || (!zone.spellCards.Contains(card.gameObject) && !zone.magicTypeCards.Contains(card.gameObject));
        }

        // 올라온 카드는 한 장뿐이다. 다른 카드로 커서가 옮겨 가면 먼저 올라온 카드를 내린다.
        void RaiseCard(Card card)
        {
            if (hoveredCard == card || InteractionLock.IsLocked || !IsRestingInHand(card)) return;
            if (isMyCardDrag && selectCard != card) return;
            ReleaseHover(true);
            hoveredCard = card;
            card.GetComponent<Order>().SetMostFrontOrder(true);
            Vector3 raised = card.originPrs.pos + Vector3.up * HandLowerOffset;
            card.MoveTransform(new Prs(raised, card.originPrs.rot, card.originPrs.scale), true, HoverMoveSeconds);
        }

        void ReleaseHover(bool moveToRest)
        {
            Card card = hoveredCard;
            hoveredCard = null;
            if (card == null) return;
            card.GetComponent<Order>().SetMostFrontOrder(false);
            if (moveToRest && IsRestingInHand(card)) card.MoveTransform(card.originPrs, true, HoverMoveSeconds);
        }

        public void CardMouseDown()
        {
            // 씬이 바뀌는 프레임에는 조합창이 먼저 파괴되고 카드 클릭이 뒤늦게 들어올 수 있다.
            if (selectCard == null || CombineZone.Instance == null) return;
            // 주문이 준비되는 동안에는 조합창이 잠겨 있으므로 카드를 집지 않는다.
            if (CombineZone.Instance.IsCasting) return;

            // 칸은 조합창 안에 있어서 창이 닫혀 있으면 카드를 놓을 자리가 보이지 않는다.
            // 카드를 집는 순간 창을 연다.
            if (!CombineZone.Instance.gameObject.activeSelf)
            {
                CombineZone.Instance.gameObject.SetActive(true);
            }

            // Shift를 누른 채 클릭하면 끌지 않고 카드 종류에 맞는 칸에 바로 올린다.
            // 드래그가 시작되지 않으므로 이어지는 MouseUp은 CardMouseUp에서 무시된다.
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                QuickPlaceCard(selectCard);
                return;
            }

            // 올라온 카드를 잡았다면 취소할 때 올라온 자리가 아니라 쉬는 자리로 돌아가야 한다.
            dragStartPrs = selectCard == hoveredCard ? selectCard.originPrs : new Prs(selectCard.transform.position, selectCard.transform.rotation, selectCard.transform.localScale);
            isMyCardDrag = true;
        }

        public void CardMouseUp()
        {
            // 창을 닫아 취소된 드래그의 MouseUp은 조합 영역에 다시 놓지 않는다.
            if (!isMyCardDrag || selectCard == null || CombineZone.Instance == null)
            {
                return;
            }

            isMyCardDrag = false;
            // 놓은 카드는 더 이상 앞으로 그리지 않는다. 손패로 돌아가면 커서가 위에 있는 한 다음 프레임에 다시 올라온다.
            ReleaseHover(false);
            if (onPushArea1 && CanPlaceInSlot(selectCard, "Spell", CombineZone.Instance.spellCards))
            {
                PlaceCard(selectCard, pushArea1);
            }
            else if (onPushArea2 && CanPlaceInSlot(selectCard, "MagicType", CombineZone.Instance.magicTypeCards))
            {
                PlaceCard(selectCard, pushArea2);
            }
            else if (onPushArea3 && selectCard.CompareTag("Target"))
            {
                PlaceCard(selectCard, pushArea3);
            }
            else if (discardSpot != null && discardSpot.gameObject.activeSelf && discardSpot.Contains(Util.MousePos)
                && TryDiscard(selectCard))
            {
                // 버린 카드는 TryDiscard가 태운다.
            }
            else
            {
                ReturnCardToHand(selectCard);
            }
        }

        // 손패의 카드는 맞는 칸으로 올리고, 이미 칸에 있는 카드는 손패로 돌린다.
        // 칸을 다른 카드가 차지하고 있으면 그 카드를 손패로 돌리고 바꿔 넣는다.
        void QuickPlaceCard(Card card)
        {
            List<GameObject> slotCards;
            GameObject slot;
            if (card.CompareTag("Spell"))
            {
                slotCards = CombineZone.Instance.spellCards;
                slot = pushArea1;
            }
            else if (card.CompareTag("MagicType"))
            {
                slotCards = CombineZone.Instance.magicTypeCards;
                slot = pushArea2;
            }
            else
            {
                return;
            }

            if (slotCards.Contains(card.gameObject))
            {
                ReturnCardToHand(card);
                return;
            }

            for (int i = slotCards.Count - 1; i >= 0; i--)
            {
                Card occupant = slotCards[i] != null ? slotCards[i].GetComponent<Card>() : null;
                if (occupant != null)
                {
                    ReturnCardToHand(occupant);
                }
                else
                {
                    slotCards.RemoveAt(i);
                }
            }

            PlaceCard(card, slot);
        }

        static bool CanPlaceInSlot(Card card, string tag, List<GameObject> slotCards)
        {
            return card.CompareTag(tag) && (slotCards.Count == 0 || slotCards.Contains(card.gameObject));
        }

        void PlaceCard(Card card, GameObject slot)
        {
            slot.GetComponent<DropZone>().GetCard(card.gameObject);
            card.MoveTransform(new Prs(slot.transform.position, Util.Qi, card.originPrs.scale), false);
        }

        void ReturnCardToHand(Card card)
        {
            if (card.CompareTag("Spell"))
            {
                CombineZone.Instance.spellCards.Remove(card.gameObject);
            }
            if (card.CompareTag("MagicType"))
            {
                CombineZone.Instance.magicTypeCards.Remove(card.gameObject);
            }

            card.MoveTransform(card.originPrs, false);
        }

        public void ReturnCardsToHand()
        {
            CancelDrag();
            selectCard = null;
            onPushArea1 = false;
            onPushArea2 = false;
            onPushArea3 = false;
            CardAlignment();
        }

        public void CancelDrag()
        {
            if (!isMyCardDrag)
            {
                return;
            }

            isMyCardDrag = false;
            ReleaseHover(false);
            if (selectCard != null)
            {
                selectCard.MoveTransform(dragStartPrs, false);
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
            if (isEnlarge)
            {
                RaiseCard(card);
            }
            else if (hoveredCard == card && !isMyCardDrag)
            {
                ReleaseHover(true);
            }
        }

        #endregion
    }
}
