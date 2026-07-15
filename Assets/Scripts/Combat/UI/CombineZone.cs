using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;
using WordVenture.Combat.Spells;
using static WordVenture.Battle.Player;

namespace WordVenture.Combat.UI
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

        [SerializeField] WordVenture.Combat.MagicAffinityTable magicAffinityTable;

        public Button activateButton;
        public GameObject Shoot;
        public GameObject Drop;
        public GameObject Summon;

        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            activateButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (spellCards.Count == 1 && magicTypeCards.Count == 1) // && targetCards.Count == 1)
            {
                activateButton.gameObject.SetActive(true);
                activateButton.onClick.RemoveAllListeners();
                activateButton.onClick.AddListener(OnButtonClick);
            } else
            {
                activateButton.gameObject.SetActive(false);
            }
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
            if (spellCards.Count == 1 && magicTypeCards.Count == 1) // && targetCards.Count == 1)
            {
                activateButton.gameObject.SetActive(true);
                activateButton.onClick.RemoveAllListeners();
                activateButton.onClick.AddListener(OnButtonClick);
            }
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
            WordVenture.Cards.MagicType spellType = spellCards[0].GetComponent<WordVenture.Cards.Card>().cardType;
            WordVenture.Cards.MagicType magicType = magicTypeCards[0].GetComponent<WordVenture.Cards.Card>().cardType;

            while (target == null)
            {
                yield return new WaitForSeconds(0.01f);
            }

            WordVenture.Combat.Enemies.Player.PlayerInt().AttackAnima();
            yield return new WaitForSeconds(0.5f);
            magicEffectSource.Play();
            if (spellType == WordVenture.Cards.MagicType.Shoot)
            {

                Shoot.GetComponent<Shoot>().shoot(magicType, target, magicAffinityTable);
            }
            else if (spellType == WordVenture.Cards.MagicType.Drop)
            {
                Drop.GetComponent<Drop>().drop(magicType, target, magicAffinityTable);
            }
            else if (spellType == WordVenture.Cards.MagicType.Summon)
            {
                Summon.GetComponent<Summon>().summon(magicType, target, magicAffinityTable);
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
                    WordVenture.Cards.Card spellCard = card.GetComponent<WordVenture.Cards.Card>();
                    WordVenture.Cards.CardManager.Inst.PopCard(spellCard);
                    Destroy(card);
                }

            }
            foreach (GameObject card in magicTypeCards)
            {
                if(card != null)
                {
                    WordVenture.Cards.Card magicTypeCard = card.GetComponent<WordVenture.Cards.Card>();
                    WordVenture.Cards.CardManager.Inst.PopCard(magicTypeCard);
                    Destroy(card);
                }
            }

            WordVenture.Cards.CardManager.Inst.CardAlignment();

            spellCards.Clear();
            magicTypeCards.Clear();
            activateButton.gameObject.SetActive(false);
        }
    }

}
