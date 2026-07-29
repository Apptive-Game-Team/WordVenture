using System;
using Story;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public static class Extensions
    {

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(arr, src) + 1;
            return (arr.Length == j) ? arr[0] : arr[j];
        }
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

    public class TutorialChatWindow : ChatWindowController
    {
        [SerializeField] Image speakerImage;

        public void SetSpeakerImage(Sprite image)
        {
            speakerImage.sprite = image;
        }
    }

}
