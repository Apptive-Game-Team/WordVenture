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
            Cards.MagicType spellType = spellCards[0].GetComponent<Card>().cardType;
            Cards.MagicType magicType = magicTypeCards[0].GetComponent<Card>().cardType;

            while (target == null)
            {
                yield return new WaitForSeconds(0.01f);
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
            activateButton.gameObject.SetActive(false);
        }
    }

}
