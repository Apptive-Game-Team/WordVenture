using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Stage;

namespace WordVenture.Scenes
{
    public class GameClearController : MonoBehaviour
    {
        [SerializeField] WordScriptableObject wordSO;
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
                if (StageDataSingleton.Instance.StagePosition == 4)
                    SceneManager.LoadScene("EndingScene");
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
            print(WordVenture.Map.MapMove.StagePosition);
            print(StageDataSingleton.Instance.StagePosition);
            if (WordVenture.Map.MapMove.StagePosition - 1 == StageDataSingleton.Instance.StagePosition)
            {
                switch (StageDataSingleton.Instance.StagePosition)
                {
                    case 0: // 2
                        GameObject card1 = Instantiate(spellCard, new Vector3(0, 0, 0), Quaternion.identity);
                        card1.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[2].name);
                        card1.GetComponent<Order>().SetOrder(0);
                        break;
                    case 1: // rock 5
                        GameObject card4 = Instantiate(magicCard, new Vector3(0, 0, 0), Quaternion.identity);
                        card4.GetComponent<Order>().SetOrder(0);
                        card4.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[5].name);
                        break;
                    case 2: // 4 7
                        GameObject card5 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                        card5.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[4].name);
                        card5.GetComponent<Order>().SetOrder(0);
                        GameObject card6 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                        card6.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[7].name);
                        card6.GetComponent<Order>().SetOrder(0);
                        break;

                    case 3: // 1 6
                        GameObject card7 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                        card7.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[1].name);
                        card7.GetComponent<Order>().SetOrder(0);
                        GameObject card8 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                        card8.GetComponentInChildren<TMP_Text>().SetText(wordSO.words[6].name);
                        card8.GetComponent<Order>().SetOrder(0);
                        break;
                }
            }

        }
    }

}
