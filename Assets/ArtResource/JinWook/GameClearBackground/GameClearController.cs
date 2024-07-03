using System.Collections;
using System.Collections.Generic;
using Deck_Manage;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameClearController : MonoBehaviour
{
    [SerializeField] WordSO wordSO;
    [SerializeField] GameObject spellCard;

    [SerializeField] GameObject magicCard;

    [SerializeField] GameObject TEXT;

    bool flag = false;

    private string sceneName;

    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameClearScene")
        {
            TEXT.SetActive(false);
        }
    }

    void Update()
    {
        if (sceneName == "GameClearScene")
        {
            if (Input.anyKeyDown)
            {
                if (!flag)
                {
                    TEXT.SetActive(true);
                    ShowGettedCard();
                    flag = true;
                }
                else
                {
                    SceneManager.LoadScene("Map_scene");
                }

            }
        } else
        {
            if (Input.anyKeyDown)
            {

                SceneManager.LoadScene("Map_scene");

            }
        }

        
    }

    void ShowGettedCard()
    {
        print(Map_scene.MapMove.StagePosition);
        print(StageDataSingleton.Instance.StagePosition);
        if (Map_scene.MapMove.StagePosition - 1 == StageDataSingleton.Instance.StagePosition)
        {
            switch (Map_scene.MapMove.StagePosition - 1)
            {
                case 0: // summon, Drop 1, 2
                    GameObject card1 = Instantiate(spellCard, new Vector3(-2, 0, 0), Quaternion.identity);
                    card1.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[1].name);
                    card1.GetComponent<Order>().SetOrder(0);
                    GameObject card2 = Instantiate(spellCard, new Vector3(2, 0, 0), Quaternion.identity);
                    card2.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[2].name);
                    card2.GetComponent<Order>().SetOrder(0);
                    break;
                case 1: // rock 4
                    GameObject card4 = Instantiate(magicCard, new Vector3(0, 0, 0), Quaternion.identity);
                    card4.GetComponent<Order>().SetOrder(0);
                    card4.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[4].name);
                    break;
                case 2: // ice, lightning 5, 6
                    GameObject card5 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                    card5.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[5].name);
                    card5.GetComponent<Order>().SetOrder(0);
                    GameObject card6 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                    card6.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[6].name);
                    card6.GetComponent<Order>().SetOrder(0);
                    break;

                case 3: // holy 7
                    GameObject card7 = Instantiate(magicCard, new Vector3(0, 0, 0), Quaternion.identity);
                    card7.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[7].name);
                    card7.GetComponent<Order>().SetOrder(0);
                    break;
            }
        }

    }
}
