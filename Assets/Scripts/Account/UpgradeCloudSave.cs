using System;
using System.Globalization;
using Core;
using Story;
using UnityEngine;

namespace Account
{
    // "save" 키와 별개인 "upgrade" 키에 저장한다. 서버 value 칸이 31자라 CloudSave 에 덧붙일 수 없다.
    // 형식은 "2|shards|attack|maxHp|hand|ally|seenTips"이다. 재화는 5자리, 단계는 최대 2자리, 본 안내 비트는 16진수 최대 8자리라
    // 최대 길이는 1 + 5 + 2 * 4 + 8 + 6 = 28자이다. 안내 기록이 없던 버전 1("1|shards|attack|maxHp|hand|ally")도 읽는다.
    [Serializable]
    public class UpgradeCloudSave
    {
        public const string ServerKey = "upgrade";
        public const int MaxShards = 99999;
        const string Version = "2";
        const string VersionWithoutTips = "1";
        const char Separator = '|';
        const int MaxLevelDigits = 2;

        public int Shards;
        public int[] Levels = new int[PlayerUpgrades.Kinds.Length];
        // 본 안내(GameTips) 비트. 버전 1 저장에는 없어서 null 이고, 그때는 이 기기의 기록을 그대로 둔다.
        public int? SeenTips;

        public static UpgradeCloudSave FromLocal()
        {
            var save = new UpgradeCloudSave { Shards = PlayerUpgrades.Shards, SeenTips = PlayerPrefs.GetInt(GameTips.SeenKey, 0) };
            foreach (UpgradeKind kind in PlayerUpgrades.Kinds) save.Levels[(int)kind] = PlayerUpgrades.GetLevel(kind);
            return save;
        }

        public void ApplyToLocal()
        {
            PlayerUpgrades.Restore(Shards, Levels);
            if (SeenTips.HasValue) PlayerPrefs.SetInt(GameTips.SeenKey, SeenTips.Value);
            PlayerPrefs.Save();
        }

        public string Serialize()
        {
            var parts = new string[3 + PlayerUpgrades.Kinds.Length];
            parts[0] = Version;
            parts[1] = Math.Min(Math.Max(Shards, 0), MaxShards).ToString(CultureInfo.InvariantCulture);
            foreach (UpgradeKind kind in PlayerUpgrades.Kinds)
            {
                int level = (int)kind < Levels.Length ? Levels[(int)kind] : 0;
                parts[2 + (int)kind] = Math.Min(Math.Max(level, 0), PlayerUpgrades.MaxLevel(kind)).ToString(CultureInfo.InvariantCulture);
            }
            parts[2 + PlayerUpgrades.Kinds.Length] = (SeenTips ?? 0).ToString("x", CultureInfo.InvariantCulture);
            return string.Join(Separator.ToString(), parts);
        }

        public static bool TryParse(string value, out UpgradeCloudSave save)
        {
            save = null;
            if (string.IsNullOrEmpty(value)) return false;
            string[] parts = value.Split(Separator);
            int levelCount = PlayerUpgrades.Kinds.Length;
            bool withTips = parts[0] == Version && parts.Length == 3 + levelCount;
            bool withoutTips = parts[0] == VersionWithoutTips && parts.Length == 2 + levelCount;
            if (!withTips && !withoutTips) return false;
            if (!TryParseNumber(parts[1], 5, MaxShards, out int shards)) return false;
            var result = new UpgradeCloudSave { Shards = shards };
            foreach (UpgradeKind kind in PlayerUpgrades.Kinds)
            {
                if (!TryParseNumber(parts[2 + (int)kind], MaxLevelDigits, PlayerUpgrades.MaxLevel(kind), out int level)) return false;
                result.Levels[(int)kind] = level;
            }
            if (withTips)
            {
                if (!TryParseMask(parts[2 + levelCount], out int tips)) return false;
                result.SeenTips = tips;
            }
            save = result;
            return true;
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

        static bool TryParseNumber(string text, int maxLength, int max, out int number)
        {
            number = 0;
            if (text.Length == 0 || text.Length > maxLength) return false;
            foreach (char c in text)
                if (c < '0' || c > '9') return false;
            number = int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);
            return number <= max;
        }
    }
}
