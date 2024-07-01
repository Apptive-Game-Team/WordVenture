using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TutorialSystem
{

    public enum TutorialFlag
    {
        NONE = 0,
        FLAG_START_TUTORIAL = 1,
        FLAG_BATTLE_START = 2,
        FLAG_TURN_START = 3,
        FLAG_COMBINATION = 4,
        FLAG_COMBINATION_DESCRIPT = 4,
        FLAG_SET_MAGIC = 5,
        FLAG_SET_ELEMENTAL = 6,
        FLAG_CAST_SPELL = 7,
        FLAG_CAST_END = 8,
        FLAG_CLICK_TO_SELECT = 9,
        FLAG_FINISH_SPELL = 10,
        FLAG_NEXT_ENEMY = 11,
        FLAG_END_BATTLE = 12,
        FLAG_END_TUTORIAL = 13,

    }

    public class TutorialChatWindow : ChatWindowController
    {
        private TMP_Text nameText;
        private TMP_Text chatText;

        private void Awake()
        {
            InitTmp_text();
        }

        public new void UpdateChatStream(string name, string text)
        {
            nameText.SetText(name);
            StartCoroutine(UpdateStreamingChat(text + " "));
        }

        IEnumerator UpdateStreamingChat(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                yield return new WaitForSeconds(0.1f);
                chatText.SetText(text.Substring(0, i));
            }
        }


        private void InitTmp_text()
        {
            TMP_Text[] tempTexts = GetComponentsInChildren<TMP_Text>();
            if (tempTexts[0].name == "ChatName")
            {
                nameText = tempTexts[0];
                chatText = tempTexts[1];
            }
            else
            {
                nameText = tempTexts[1];
                chatText = tempTexts[0];
            }
        }
    }

}

