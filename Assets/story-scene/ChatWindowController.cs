using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatWindowController : MonoBehaviour
{
    private TMP_Text chatName;
    private TMP_Text chatText;
    private void Awake()
    {
        InitTmp_text();
    }

    public void UpdateChatStream(string name, string text)
    {
        chatName.SetText(name);
        StartCoroutine(UpdateStreamingChat(text+" "));
    }
    IEnumerator UpdateStreamingChat(string text)
    {
        for (int i = 0; i< text.Length; i++)
        {
            yield return new WaitForSeconds(0.03f);
            chatText.SetText(text.Substring(0, i));
        }
    }


    private void InitTmp_text()
    {
        TMP_Text[] tempTexts = GetComponentsInChildren<TMP_Text>();
        if (tempTexts[0].name == "ChatName")
        {
            chatName = tempTexts[0];
            chatText = tempTexts[1];
        }
        else
        {
            chatName = tempTexts[1];
            chatText = tempTexts[0];
        }
    }

}
