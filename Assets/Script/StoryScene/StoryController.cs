using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryController : MonoBehaviour
{

    public ChatWindowController chatWindowController;
    [SerializeField] ChatWindowScriptContainer scriptContainer;

    [SerializeField] List<GameObject> backgorunds = new List<GameObject>();

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip badMood;

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
            }
            else
            {
                backgorunds[i].SetActive(false);
            }
                
        }
        if (id == 1)
        {
            audioSource.clip = badMood;
            audioSource.Play();
        }
    }

    
    IEnumerator StoryTelling()
    {
        for (int i = 0; i < scriptContainer.GetScriptNum(); i++)
        {
            SwitchBackground(scriptContainer.GetScriptData(i).background);
            chatWindowController.UpdateChatStream(scriptContainer.GetScriptData(i).name, scriptContainer.GetScriptData(i).text);

            yield return new WaitForSeconds(scriptContainer.GetScriptData(i).text.Length * 0.03f);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        LoadMapScene();
    }

    private void LoadMapScene()
    {
        if (Map_scene.MapScene.StagePosition == 5)
            SceneManager.LoadScene("TitleScene");
        else
            SceneManager.LoadScene("Map_scene");
    }
}
