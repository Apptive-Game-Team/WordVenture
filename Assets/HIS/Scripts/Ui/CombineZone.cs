using System.Collections;
using System.Collections.Generic;
using Deck_Manage;
using UnityEngine;
using UnityEngine.UI;
using static BattleSystem.Player;

public class CombineZone : MonoBehaviour
{
    public List<GameObject> spellCards = new List<GameObject>();
    public List<GameObject> magicTypeCards = new List<GameObject>();

    public Button activateButton;
    public GameObject Shoot;
    public GameObject Drop;
    public GameObject Summon;

    void Start()
    {
        activateButton.gameObject.SetActive(false);
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
        Deck_Manage.MagicType spellType = spellCards[0].GetComponent<Deck_Manage.Card>().cardType;
        Deck_Manage.MagicType magicType = magicTypeCards[0].GetComponent<Deck_Manage.Card>().cardType;

        while (target == null)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if (spellType == Deck_Manage.MagicType.Shoot)
        {

            Shoot.GetComponent<Shoot>().shoot(magicType, target);
        }
        else if (spellType == Deck_Manage.MagicType.Drop)
        {
            Drop.GetComponent<Drop>().drop(magicType, target);
        }
        else if (spellType == Deck_Manage.MagicType.Summon)
        {
            Summon.GetComponent<Summon>().summon(magicType);
        }
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
                Deck_Manage.Card spellCard = card.GetComponent<Deck_Manage.Card>();
                Deck_Manage.CardManager.Inst.PopCard(spellCard);
                Destroy(card);
            }
                
        }
        foreach (GameObject card in magicTypeCards)
        {
            if(card != null)
            {
                Deck_Manage.Card magicTypeCard = card.GetComponent<Deck_Manage.Card>();
                Deck_Manage.CardManager.Inst.PopCard(magicTypeCard);
                Destroy(card);
            }  
        }

        spellCards.Clear();
        magicTypeCards.Clear();
        activateButton.gameObject.SetActive(false);
    }
}
