using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Core;

namespace WordVenture.Scenes
{

    public class TitleSceneManager : MonoBehaviour
    {
        SaveLoadController saveLoadController;
        [SerializeField] GameObject continueButton;

        private void Start()
        {
            saveLoadController = GameObject.Find("SaveLoadController").GetComponent<SaveLoadController>();
            if (saveLoadController.LoadPlayData() == -1)
            {
                continueButton.SetActive(false);
            }
        }


        public void LoadStoryScene()
        {
            if (saveLoadController.LoadPlayData() == -1)
            {
                WordVenture.Map.MapMove.StagePosition = 0;
                saveLoadController.SavePlayData();
                SceneManager.LoadScene("StoryScene");

            } else
            {
                SceneManager.LoadScene("Map_scene");
            }


        }

        public void InitPlayerData()
        {
            SaveLoadController.Instance.InitPlayData();
            LoadStoryScene();
        }


        public void QuitGame()
        {
            SaveLoadController.Instance.SavePlayData();
            SaveLoadController.Instance.QuitGame();
        }
    }

}
