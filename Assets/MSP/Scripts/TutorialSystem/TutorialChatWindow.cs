using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TutorialSystem
{
    public static class Extensions
    {

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }


    public enum TutorialFlag
    {
        NONE = 0,
        FLAG_START_TUTORIAL = 1,
        FLAG_BATTLE_START = 2,
        FLAG_TURN_START = 3,
        FLAG_COMBINATION = 4,
        FLAG_COMBINATION_DESCRIPT = 5,
        FLAG_SET_MAGIC = 6,
        FLAG_SET_ELEMENTAL = 7,
        FLAG_CAST_SPELL = 8,
        FLAG_CAST_END = 9,
        FLAG_CLICK_TO_SELECT = 10,
        FLAG_FINISH_SPELL = 11,
        FLAG_NEXT_ENEMY = 12,
        FLAG_END_BATTLE = 13,
        FLAG_END_TUTORIAL = 14,
        END = 15,
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

