using System.Collections;
using System.Collections.Generic;
using Cards;
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
        [FormerlySerializedAs("Summon")] public GameObject summon;

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
            SetActivateButtonVisible(spellCards.Count == 1 && magicTypeCards.Count == 1);
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
            SetActivateButtonVisible(spellCards.Count == 1 && magicTypeCards.Count == 1);
        }

        SelectableObject target = null;

        public async void OnButtonClick()
        {
            if (spellCards.Count == 1 && magicTypeCards.Count == 1)// && targetCards.Count == 1)
            {
                StartCoroutine(CastSpell());
            }
            ClearDropZone();
        }
        IEnumerator CastSpell()
        {
            InitSelectableObjectList();
            SetAllSelectable(true);
            Cards.MagicType spellType = spellCards[0].GetComponent<Card>().cardType;
            Cards.MagicType magicType = magicTypeCards[0].GetComponent<Card>().cardType;

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
            else if (spellType == MagicType.Summon)
            {
                summon.GetComponent<Summon>().Run(magicType, target, magicAffinityTable);
            }
            SetAllSelectable(false);

            target = null;
        }

        public void SetTarget(SelectableObject selectableObject)
        {
            target = selectableObject;
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
