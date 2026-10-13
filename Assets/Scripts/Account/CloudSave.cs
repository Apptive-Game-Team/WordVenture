using System;
using System.Globalization;
using Core;
using Map;
using UnityEngine;

namespace Account
{
    [Serializable]
    public class CloudSave
    {
        public const string StagePositionKey = "StagePosition";
        public const int MaxStagePosition = 9999;
        const string Version = "1";
        const char Separator = '|';

        public int StagePosition = -1;
        public bool TutorialEnded;
        public int ActOneDialogueSeen;
        public int ActTwoDialogueSeen;
        public bool ShowsActOneMap;

        public static CloudSave FromLocal() => new CloudSave
        {
            StagePosition = PlayerPrefs.GetInt(StagePositionKey, -1),
            TutorialEnded = PlayerPrefs.GetInt(SaveLoadController.TutorialEndedKey, 0) == 1,
            ActOneDialogueSeen = PlayerPrefs.GetInt(SaveLoadController.StageDialogueSeenKey, 0),
            ActTwoDialogueSeen = PlayerPrefs.GetInt(SaveLoadController.ActTwoDialogueSeenKey, 0),
            ShowsActOneMap = PlayerPrefs.GetInt(SaveLoadController.ShowsActOneMapKey, 0) == 1,
        };

        public void ApplyToLocal()
        {
            PlayerPrefs.SetInt(StagePositionKey, StagePosition);
            PlayerPrefs.SetInt(SaveLoadController.TutorialEndedKey, TutorialEnded ? 1 : 0);
            PlayerPrefs.SetInt(SaveLoadController.StageDialogueSeenKey, ActOneDialogueSeen);
            PlayerPrefs.SetInt(SaveLoadController.ActTwoDialogueSeenKey, ActTwoDialogueSeen);
            PlayerPrefs.SetInt(SaveLoadController.ShowsActOneMapKey, ShowsActOneMap ? 1 : 0);
            MapMove.StagePosition = StagePosition;
            PlayerPrefs.Save();
        }

        // 서버 value 칸이 31자라서 비트마스크는 16진수로 쓴다. 최대 28자이다(버전 1, stage 4, 마스크 8 + 8, 플래그 2, 구분자 5).
        public string Serialize()
        {
            int stage = Mathf.Clamp(StagePosition, -1, MaxStagePosition);
            return string.Join(Separator.ToString(), Version,
                stage.ToString(CultureInfo.InvariantCulture),
                TutorialEnded ? "1" : "0",
                ActOneDialogueSeen.ToString("x"),
                ActTwoDialogueSeen.ToString("x"),
                ShowsActOneMap ? "1" : "0");
        }

        public static bool TryParse(string value, out CloudSave save)
        {
            save = null;
            if (string.IsNullOrEmpty(value)) return false;
            string[] parts = value.Split(Separator);
            if (parts.Length != 6 || parts[0] != Version) return false;
            if (!TryParseStage(parts[1], out int stage)) return false;
            if (!TryParseFlag(parts[2], out bool tutorial) || !TryParseFlag(parts[5], out bool actOneMap)) return false;
            if (!TryParseMask(parts[3], out int actOne) || !TryParseMask(parts[4], out int actTwo)) return false;
            save = new CloudSave
            {
                StagePosition = stage,
                TutorialEnded = tutorial,
                ActOneDialogueSeen = actOne,
                ActTwoDialogueSeen = actTwo,
                ShowsActOneMap = actOneMap,
            };
            return true;
        }

        static bool TryParseStage(string text, out int stage)
        {
            stage = 0;
            if (text.Length == 0 || text.Length > 5) return false;
            for (int i = 0; i < text.Length; i++)
                if (!(char.IsDigit(text[i]) && text[i] < 128) && !(i == 0 && text[i] == '-')) return false;
            return int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out stage)
                && stage >= -1 && stage <= MaxStagePosition;
        }

        static bool TryParseFlag(string text, out bool flag)
        {
            flag = text == "1";
            return text == "0" || text == "1";
        }

        static bool TryParseMask(string text, out int mask)
        {
            mask = 0;
            if (text.Length == 0 || text.Length > 8) return false;
            foreach (char c in text)
                if (!Uri.IsHexDigit(c)) return false;
            mask = unchecked((int)uint.Parse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
            return true;
        }
    }
}
