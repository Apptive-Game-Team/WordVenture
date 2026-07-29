using Cards;
using Combat.Stage;
using Core;
using Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Scenes
{
    public class GameClearController : MonoBehaviour
    {
        [FormerlySerializedAs("wordSO")] [SerializeField] WordScriptableObject wordSo;
        [SerializeField] GameObject spellCard;

        [SerializeField] GameObject magicCard;

        [FormerlySerializedAs("TEXT")] [SerializeField] GameObject text;

        bool flag = false;

        private string sceneName;

        private void Start()
        {
            sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "GameClearScene")
            {
                if (StageDataSingleton.Instance.stagePosition == 4)
                    SceneManager.LoadScene("EndingScene");
                text.SetActive(false);
            }
        }

        void Update()
        {
            // 튜토리얼 대사를 넘기는 키가 클리어 화면 진행으로도 먹히면 안 된다.
            if (InteractionLock.IsLocked)
            {
                return;
            }

            if (sceneName == "GameClearScene")
            {
                if (Input.anyKeyDown)
                {
                    if (!flag)
                    {
                        text.SetActive(true);
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
            print(MapMove.StagePosition);
            print(StageDataSingleton.Instance.stagePosition);
            if (MapMove.StagePosition - 1 == StageDataSingleton.Instance.stagePosition)
            {
                switch (StageDataSingleton.Instance.stagePosition)
                {
                    case 0: // 2
                        GameObject card1 = Instantiate(spellCard, new Vector3(0, 0, 0), Quaternion.identity);
                        card1.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[2].name);
                        card1.GetComponent<Order>().SetOrder(0);
                        break;
                    case 1: // rock 5
                        GameObject card4 = Instantiate(magicCard, new Vector3(0, 0, 0), Quaternion.identity);
                        card4.GetComponent<Order>().SetOrder(0);
                        card4.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[5].name);
                        break;
                    case 2: // 4 7
                        GameObject card5 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                        card5.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[4].name);
                        card5.GetComponent<Order>().SetOrder(0);
                        GameObject card6 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                        card6.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[7].name);
                        card6.GetComponent<Order>().SetOrder(0);
                        break;

                    case 3: // 1 6
                        GameObject card7 = Instantiate(magicCard, new Vector3(-2, 0, 0), Quaternion.identity);
                        card7.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[1].name);
                        card7.GetComponent<Order>().SetOrder(0);
                        GameObject card8 = Instantiate(magicCard, new Vector3(2, 0, 0), Quaternion.identity);
                        card8.GetComponentInChildren<TMP_Text>().SetText(wordSo.words[6].name);
                        card8.GetComponent<Order>().SetOrder(0);
                        break;
                }
            }

        }
    }

}
