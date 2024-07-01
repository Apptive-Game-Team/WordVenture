using System.Collections;
using System.Collections.Generic;
using TutorialSystem;
using UnityEngine;

public class TutorialController : StoryController
{
    [SerializeField] TutorialChatWindow tutorialChatWindow;
    [SerializeField] TutorialScriptContainer tutorialScript;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        StartCoroutine(StoryTelling());
    }

    IEnumerator StoryTelling()
    {
        for (int i = 0; i < tutorialScript.GetScriptNum(); i++)
        {
            tutorialChatWindow.UpdateChatStream(tutorialScript.GetScriptData(i).name, tutorialScript.GetScriptData(i).text);

            yield return new WaitForSeconds(tutorialScript.GetScriptData(i).text.Length * 0.1f);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
    }
}
