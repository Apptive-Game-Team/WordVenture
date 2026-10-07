using Core;
using Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
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
                MapMove.StagePosition = 0;
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
