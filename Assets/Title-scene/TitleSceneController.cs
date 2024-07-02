using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    SaveLoadController saveLoadController;


    private void Start()
    {
        saveLoadController = GameObject.Find("SaveLoadController").GetComponent<SaveLoadController>();
    }

    public void LoadStoryScene()
    {
        if (saveLoadController.LoadPlayData() == -1)
        {
            Map_scene.MapMove.StagePosition = 0;
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
    }


    public void QuitGame()
    {
        SaveLoadController.Instance.SavePlayData();
        SaveLoadController.Instance.QuitGame();
    }
}
