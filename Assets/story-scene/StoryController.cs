using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryController : MonoBehaviour
{

    public ChatWindowController chatWindowController;
    [SerializeField] ChatWindowScriptContainer scriptContainer;

    [SerializeField] List<GameObject> backgorunds = new List<GameObject>();


    void Start()
    {
        InitBackground();
        StartCoroutine(StoryTelling());
    }

    private void InitBackground()
    {
        backgorunds[0].SetActive(true);
        for (int i = 1; i < backgorunds.Count; i++)
        {
            backgorunds[i].SetActive(false);
        }
    }

    private void SwitchBackground(int id)
    {
        for (int i = 0; i < backgorunds.Count; i++)
        {
            if (i == id)
            {
                backgorunds[i].SetActive(true);
            } else
                backgorunds[i].SetActive(false);
        }
    }

    
    IEnumerator StoryTelling()
    {
        for (int i = 0; i < scriptContainer.GetScriptNum(); i++)
        {
            SwitchBackground(scriptContainer.GetScriptData(i).background);
            chatWindowController.UpdateChatStream(scriptContainer.GetScriptData(i).name, scriptContainer.GetScriptData(i).text);

            yield return new WaitForSeconds(scriptContainer.GetScriptData(i).text.Length * 0.1f);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        LoadStoryScene();
    }

    private void LoadStoryScene()
    {
        SceneManager.LoadScene("StoryScene");
    }
}
