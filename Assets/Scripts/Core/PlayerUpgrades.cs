using System;
using Map;
using UnityEngine;

namespace Core
{
    // 순서가 PlayerPrefs 키와 클라우드 저장 형식에 쓰인다. 새 값은 맨 끝에만 추가한다.
    public enum UpgradeKind
    {
        Attack,
        MaxHp,
        HandLimit,
        Ally
    }

    // 스테이지를 깨고 받은 재화로 올리는 능력치. 주문 피해가 스테이지를 따라 자동으로 오르던 것을 대신한다.
    public static class PlayerUpgrades
    {
        public const string ShardsKey = "UpgradeShards";
        const string LevelKeyPrefix = "UpgradeLevel";

        public const int BaseSpellDamage = 10;
        public const int SpellDamagePerLevel = 5;
        public const int BasePlayerMaxHp = 100;
        public const int PlayerMaxHpPerLevel = 20;
        // 매 턴 주문 카드와 속성 카드를 1장씩 뽑고 1장씩 쓰므로 손패는 항상 짝수다. 그래서 2장씩 늘린다.
        public const int BaseHandLimit = 4;
        public const int HandLimitPerLevel = 2;
        public const int BaseAllyMaxHp = 8;
        public const int AllyMaxHpPerLevel = 4;
        public const int BaseAllyAttackDamage = 4;
        public const int AllyAttackDamagePerLevel = 2;

        public static readonly UpgradeKind[] Kinds = (UpgradeKind[])Enum.GetValues(typeof(UpgradeKind));

        // 다음 단계로 올리는 비용. 배열 길이가 최대 단계다. 모두 올리는 데 111이 들고,
        // 10개 스테이지를 처음 깨서 받는 재화는 등급에 따라 70~100이라 모든 능력치를 다 올릴 수는 없다.
        static int[] Costs(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Attack: return new[] { 3, 5, 7, 9, 11, 13 };
                case UpgradeKind.MaxHp: return new[] { 2, 4, 6, 8, 10 };
                case UpgradeKind.HandLimit: return new[] { 5, 10 };
                case UpgradeKind.Ally: return new[] { 3, 6, 9 };
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        // 5 스테이지를 처음 깨고 Spawn 카드를 받은 뒤부터 Spawn 이 나온다. 그 전에는 아군 슬라임 업그레이드도 잠근다.
        public const int SpawnUnlockStage = 6;

        public static bool IsUnlocked(UpgradeKind kind) =>
            kind != UpgradeKind.Ally || MapMove.StagePosition >= SpawnUnlockStage;

        public static int Shards => Mathf.Max(0, PlayerPrefs.GetInt(ShardsKey, 0));

        public static int MaxLevel(UpgradeKind kind) => Costs(kind).Length;

        public static int GetLevel(UpgradeKind kind) =>
            Mathf.Clamp(PlayerPrefs.GetInt(LevelKey(kind), 0), 0, MaxLevel(kind));

        // 최대 단계면 -1이다.
        public static int NextCost(UpgradeKind kind)
        {
            int level = GetLevel(kind);
            return level < MaxLevel(kind) ? Costs(kind)[level] : -1;
        }

        public static bool CanUpgrade(UpgradeKind kind)
        {
            int cost = NextCost(kind);
            return IsUnlocked(kind) && cost >= 0 && Shards >= cost;
        }

        public static bool TryUpgrade(UpgradeKind kind)
        {
            if (!CanUpgrade(kind)) return false;
            PlayerPrefs.SetInt(ShardsKey, Shards - NextCost(kind));
            PlayerPrefs.SetInt(LevelKey(kind), GetLevel(kind) + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void AddShards(int amount)
        {
            if (amount <= 0) return;
            PlayerPrefs.SetInt(ShardsKey, Shards + amount);
            PlayerPrefs.Save();
        }

        // 클라우드에서 불러온 값을 덮어쓴다. levels는 UpgradeKind 순서이고, 범위를 넘는 값은 잘라 낸다.
        public static void Restore(int shards, int[] levels)
        {
            PlayerPrefs.SetInt(ShardsKey, Mathf.Max(0, shards));
            foreach (UpgradeKind kind in Kinds)
            {
                int level = levels != null && (int)kind < levels.Length ? levels[(int)kind] : 0;
                PlayerPrefs.SetInt(LevelKey(kind), Mathf.Clamp(level, 0, MaxLevel(kind)));
            }
            PlayerPrefs.Save();
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(ShardsKey);
            foreach (UpgradeKind kind in Kinds) PlayerPrefs.DeleteKey(LevelKey(kind));
            PlayerPrefs.Save();
        }

        public static int SpellBaseDamage => BaseSpellDamage + SpellDamagePerLevel * GetLevel(UpgradeKind.Attack);
        public static int PlayerMaxHp => BasePlayerMaxHp + PlayerMaxHpPerLevel * GetLevel(UpgradeKind.MaxHp);
        public static int HandLimit => BaseHandLimit + HandLimitPerLevel * GetLevel(UpgradeKind.HandLimit);
        // 주문 카드와 속성 카드는 각각 손패 한도의 절반까지 든다.
        public static int HandLimitPerKind => HandLimit / 2;
        public static int AllyMaxHp => BaseAllyMaxHp + AllyMaxHpPerLevel * GetLevel(UpgradeKind.Ally);
        public static int AllyAttackDamage => BaseAllyAttackDamage + AllyAttackDamagePerLevel * GetLevel(UpgradeKind.Ally);

        static string LevelKey(UpgradeKind kind) => LevelKeyPrefix + kind;
    }
}
