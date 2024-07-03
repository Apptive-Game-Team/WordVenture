using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public class Constant
    {
        public static float CHAT_CLOSE_TIME = 5f;
        public static float CHAT_REMAIN_TIME = 3f;
    }

    public enum TutorialFlag
    {
        NONE = 0,
        FLAG_001_START_TUTORIAL = 1,
        FLAG_002_BATTLE_START = 2,
        FLAG_003_TURN_START = 3,
        FLAG_004_COMBINATION = 4,
        FLAG_005_COMBINATION_DESCRIPT = 5,
        FLAG_006_SET_MAGIC = 6,
        FLAG_007_SET_ELEMENTAL = 7,
        FLAG_008_CAST_SPELL = 8,
        FLAG_009_CAST_END = 9,
        FLAG_010_CLICK_TO_SELECT = 10,
        FLAG_011_FINISH_SPELL = 11,
        FLAG_012_NEXT_ENEMY = 12,
        FLAG_013_END_BATTLE = 13,
        FLAG_014_END_TUTORIAL = 14,
        END = 15,
    }

    public enum ChatStatus
    {
        DEFAULT = 0,
        UPDATING = 1,
    }

    public class TutorialChatWindow : ChatWindowController
    {
        private TMP_Text nameText;
        private TMP_Text descriptText;
        [SerializeField] Image speakerImage;
        
        ChatStatus status;
        public float chatCloseTime;
        public float chatRemainTime;
        public ChatStatus ChatStatus 
        { 
            get { return status; }
            set { status = value; }
        }

        private void Awake()
        {
            InitSetting();
        }

        public new void UpdateChatStream(string name, string text)
        {
            nameText.SetText(name);
            StartCoroutine(UpdateStreamingChat(text + " "));
        }

        IEnumerator UpdateStreamingChat(string text)
        {
            ChatStatus = ChatStatus.UPDATING;

            for (int i = 0; i < text.Length; i++)
            {
                yield return new WaitForSeconds(0.1f);
                descriptText.SetText(text.Substring(0, i));
            }

            ChatStatus = ChatStatus.DEFAULT;
            chatCloseTime = Time.time + Constant.CHAT_CLOSE_TIME;
            chatRemainTime = Time.time + Constant.CHAT_REMAIN_TIME;
        }

        public void SetSpeakerImage(Sprite image)
        {
            speakerImage.sprite = image;
        }

        private void InitSetting()
        {
            TMP_Text[] tempTexts = GetComponentsInChildren<TMP_Text>();
            if (tempTexts[0].name == "ChatName")
            {
                nameText = tempTexts[0];
                descriptText = tempTexts[1];
            }
            else
            {
                nameText = tempTexts[1];
                descriptText = tempTexts[0];
            }
        }
    }

}

