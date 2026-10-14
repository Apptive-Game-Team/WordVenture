using System;
using System.Globalization;
using Core;

namespace Account
{
    // "save" 키와 별개인 "upgrade" 키에 저장한다. 서버 value 칸이 31자라 CloudSave 에 덧붙일 수 없다.
    // 형식은 "1|shards|attack|maxHp|hand|ally"이고 재화는 5자리, 단계는 최대 2자리까지 받으므로 최대 길이는 1 + 5 + 2 * 4 + 5 = 19자이다.
    [Serializable]
    public class UpgradeCloudSave
    {
        public const string ServerKey = "upgrade";
        public const int MaxShards = 99999;
        const string Version = "1";
        const char Separator = '|';
        const int MaxLevelDigits = 2;

        public int Shards;
        public int[] Levels = new int[PlayerUpgrades.Kinds.Length];

        public static UpgradeCloudSave FromLocal()
        {
            var save = new UpgradeCloudSave { Shards = PlayerUpgrades.Shards };
            foreach (UpgradeKind kind in PlayerUpgrades.Kinds) save.Levels[(int)kind] = PlayerUpgrades.GetLevel(kind);
            return save;
        }

        public void ApplyToLocal() => PlayerUpgrades.Restore(Shards, Levels);

        public string Serialize()
        {
            var parts = new string[2 + PlayerUpgrades.Kinds.Length];
            parts[0] = Version;
            parts[1] = Math.Min(Math.Max(Shards, 0), MaxShards).ToString(CultureInfo.InvariantCulture);
            foreach (UpgradeKind kind in PlayerUpgrades.Kinds)
            {
                int level = (int)kind < Levels.Length ? Levels[(int)kind] : 0;
                parts[2 + (int)kind] = Math.Min(Math.Max(level, 0), PlayerUpgrades.MaxLevel(kind)).ToString(CultureInfo.InvariantCulture);
            }
            return string.Join(Separator.ToString(), parts);
        }

        public static bool TryParse(string value, out UpgradeCloudSave save)
        {
            save = null;
            if (string.IsNullOrEmpty(value)) return false;
            string[] parts = value.Split(Separator);
            if (parts.Length != 2 + PlayerUpgrades.Kinds.Length || parts[0] != Version) return false;
            if (!TryParseNumber(parts[1], 5, MaxShards, out int shards)) return false;
            var result = new UpgradeCloudSave { Shards = shards };
            foreach (UpgradeKind kind in PlayerUpgrades.Kinds)
            {
                if (!TryParseNumber(parts[2 + (int)kind], MaxLevelDigits, PlayerUpgrades.MaxLevel(kind), out int level)) return false;
                result.Levels[(int)kind] = level;
            }
            save = result;
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
