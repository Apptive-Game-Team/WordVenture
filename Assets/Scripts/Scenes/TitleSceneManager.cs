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
        [SerializeField] TitleStageView stageView;

        private void Start()
        {
            saveLoadController = GameObject.Find("SaveLoadController").GetComponent<SaveLoadController>();
            ShowSavedProgress();
        }

        // 서버에서 진행도를 불러온 뒤에도 다시 불러 이어하기 버튼과 배경을 맞춘다.
        public void ShowSavedProgress()
        {
            int stagePosition = saveLoadController.LoadPlayData();
            continueButton.SetActive(stagePosition != -1);
            stageView.Show(stagePosition);
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
                SceneManager.LoadScene("MapScene");
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
