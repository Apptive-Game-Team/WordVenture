using System.Collections;
using System;
using System.Collections.Generic;
using Cards;
using Battle.Turns;
using Core;
using Combat.Allies;
using Combat.Enemies;
using Combat.Spells;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Combat.UI
{

    public class CombineZone : MonoBehaviour
    {

        [SerializeField] AudioSource magicEffectSource;

        public static CombineZone Instance;
        public event Action SpellCastStarted;
        public event Action<SelectableObject> TargetSelected;
        public bool IsAwaitingTarget { get; private set; }
        bool isCasting;
        public bool IsCasting => isCasting;

        public Transform GetTutorialTarget()
        {
            foreach (SelectableObject selectable in allSelectableObjects)
                if (selectable != null && selectable.gameObject.activeInHierarchy
                    && selectable.CompareTag("Enemy") && selectable.GetSelectable())
                    return selectable.transform;
            return null;
        }

        public List<GameObject> spellCards = new List<GameObject>();
        public List<GameObject> magicTypeCards = new List<GameObject>();

        private List<SelectableObject> allSelectableObjects = new List<SelectableObject>();

        void InitSelectableObjectList()
        {
            allSelectableObjects.Clear();

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject gameObject in gameObjects)
            {
                allSelectableObjects.Add(gameObject.GetComponent<SelectableObject>());
            }
            allSelectableObjects.Add(GameObject.FindGameObjectWithTag("Me").GetComponent<SelectableObject>());
        }

        void SetAllSelectable(bool selectable)
        {
            foreach (SelectableObject gameObject in allSelectableObjects)
            {
                gameObject.SetSelectable(selectable);
            }
        }

        [SerializeField] MagicAffinityTable magicAffinityTable;

        public Button activateButton;
        [FormerlySerializedAs("Shoot")] public GameObject shoot;
        [FormerlySerializedAs("Drop")] public GameObject drop;
        [FormerlySerializedAs("Summon")] public GameObject explode;

        private void Awake()
        {
            Instance = this;
        }
        // activateButton의 현재 표시 상태. SetActive를 같은 값으로 다시 부르지 않기 위해
        // 따로 들고 있는다.
        bool activateButtonVisible;

        void Start()
        {
            // 리스너는 여기서 한 번만 연결한다. 매 프레임 RemoveAllListeners와
            // AddListener를 반복하면 델리게이트와 UnityEvent 내부 호출 목록이
            // 프레임마다 새로 만들어진다.
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(OnButtonClick);

            SetActivateButtonVisible(false);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            // 카드는 ClearDropZone을 거치지 않고 빠지기도 한다. CardManager가 조합 영역
            // 밖에 카드를 놓으면 목록만 비우므로, 버튼 상태는 여기서 계속 맞춰야 한다.
            SetActivateButtonVisible(CanCombine());
        }

        // 아군 자리가 모두 차 있으면 Spawn은 조합할 수 없다. 턴 종료 후 아군이 공격하는 동안에는
        // 어떤 주문도 조합할 수 없다. 그 사이에 시작한 주문은 적 턴에 날아간다.
        bool CanCombine()
        {
            if (spellCards.Count != 1 || magicTypeCards.Count != 1) return false;
            TurnBattleSystem battle = TurnBattleSystem.Instance;
            if (battle != null && battle.IsEndingPlayerTurn) return false;
            Card spellCard = spellCards[0] != null ? spellCards[0].GetComponent<Card>() : null;
            return spellCard == null || spellCard.cardType != MagicType.Spawn || AllyFormation.CanSpawn;
        }

        private void OnDisable()
        {
            // 카드는 조합창의 자식이 아니므로 창을 숨겨도 직접 손패로 돌려야 한다.
            if (CardManager.Inst != null)
            {
                CardManager.Inst.ReturnCardsToHand();
            }

            spellCards.Clear();
            magicTypeCards.Clear();
            SetActivateButtonVisible(false);
        }

        void SetActivateButtonVisible(bool visible)
        {
            if (activateButtonVisible == visible)
            {
                return;
            }

            activateButtonVisible = visible;
            activateButton.gameObject.SetActive(visible);
        }

        public void AddCard(GameObject card)
        {
            if (card.CompareTag("Spell") && spellCards.Count < 1)
            {
                spellCards.Add(card);
            }
            else if (card.CompareTag("MagicType") && magicTypeCards.Count < 1)
            {
                magicTypeCards.Add(card);
            }
            SetActivateButtonVisible(CanCombine());
        }

        SelectableObject target = null;

        public void OnButtonClick()
        {
            if (InteractionLock.IsLocked || isCasting || !CanCombine()) return;
            isCasting = true;
            // 조합창을 닫아도 대상 선택 중인 주문은 유지한다. 카드 관리자는 전투 씬과 수명이 같다.
            CardManager.Inst.StartCoroutine(CastSpell());
            ClearDropZone();
            SpellCastStarted?.Invoke();
        }
        IEnumerator CastSpell()
        {
            Cards.MagicType spellType = spellCards[0].GetComponent<Card>().cardType;
            Cards.MagicType magicType = magicTypeCards[0].GetComponent<Card>().cardType;
            if (spellType == MagicType.Spawn)
            {
                yield return CastSpawn(magicType);
                yield break;
            }

            InitSelectableObjectList();
            SetAllSelectable(true);
            IsAwaitingTarget = true;

            // 대상을 고를 때까지 기다린다. 0.01초는 프레임 간격보다 짧아 어차피 한
            // 프레임마다 깨어났고, 그때마다 대기 객체만 새로 만들어졌다. 대기가
            // 얼마나 길어질지는 플레이어에게 달렸으므로 그동안 계속 쌓인다.
            while (target == null)
            {
                yield return null;
            }

            Player.PlayerInt().AttackAnima();
            yield return new WaitForSeconds(0.5f);
            magicEffectSource.Play();
            if (spellType == MagicType.Shoot)
            {

                shoot.GetComponent<Shoot>().Run(magicType, target, magicAffinityTable);
            }
            else if (spellType == MagicType.Drop)
            {
                drop.GetComponent<Drop>().Run(magicType, target, magicAffinityTable);
            }
            else if (spellType == MagicType.Explode)
            {
                explode.GetComponent<Explode>().Run(magicType, target, magicAffinityTable);
            }
            SetAllSelectable(false);

            target = null;
            isCasting = false;
        }

        // Spawn은 대상을 고르지 않고 워드 앞 아군 자리에 슬라임을 세운다.
        IEnumerator CastSpawn(MagicType element)
        {
            Player player = Player.PlayerInt();
            player.AttackAnima();
            yield return new WaitForSeconds(0.5f);
            magicEffectSource.Play();
            AllyFormation.GetOrCreate(player.transform).Spawn(element, magicAffinityTable);
            isCasting = false;
        }

        public void SetTarget(SelectableObject selectableObject)
        {
            if (InteractionLock.IsLocked || !IsAwaitingTarget || selectableObject == null
                || !selectableObject.GetSelectable()) return;
            Enemy selectedEnemy = selectableObject.GetComponent<Enemy>();
            if (selectedEnemy != null && !selectedEnemy.IsAlive) return;
            target = selectableObject;
            IsAwaitingTarget = false;
            TargetSelected?.Invoke(selectableObject);
        }

        public void ClearDropZone()
        {
            foreach (GameObject card in spellCards)
            {
                if(card != null)
                {
                    Card spellCard = card.GetComponent<Card>();
                    CardManager.Inst.PopCard(spellCard);
                    Destroy(card);
                }

            }
            foreach (GameObject card in magicTypeCards)
            {
                if(card != null)
                {
                    Card magicTypeCard = card.GetComponent<Card>();
                    CardManager.Inst.PopCard(magicTypeCard);
                    Destroy(card);
                }
            }

            CardManager.Inst.CardAlignment();

            spellCards.Clear();
            magicTypeCards.Clear();
            SetActivateButtonVisible(false);
        }
    }

}
