using UnityEngine;

namespace Core
{
    public readonly struct StageRewardResult
    {
        public readonly int BaseShards;
        public readonly int FastClearBonus;
        public readonly bool IsFirstClear;

        public StageRewardResult(int baseShards, int fastClearBonus, bool isFirstClear)
        {
            BaseShards = baseShards;
            FastClearBonus = fastClearBonus;
            IsFirstClear = isFirstClear;
        }

        public int Total => BaseShards + FastClearBonus;
        public bool IsFastClear => FastClearBonus > 0;
    }

    // 스테이지를 깨고 받는 업그레이드 재화. 빨리 깰수록 더 받아서, 3턴마다 적이 강해지는 규칙과 함께 시간을 끌지 않게 한다.
    public static class StageReward
    {
        public const int FastClearBonus = 2;
        // 이미 깬 스테이지를 다시 깨면 기본 재화의 20%만 준다. 쉬운 스테이지를 반복해 모든 능력치를 올리지 못하게 한다.
        public const float ReplayRate = 0.2f;

        // 이 턴 수 안에 깨면 빠른 클리어다. 3~9는 자동 전투 기록(docs/design/act-two-balance)에 여유를 더한 값이고,
        // 0~2는 아직 기록이 없어 웨이브 수로 정했다. 사람 플레이 기록을 보고 다시 맞춘다.
        static readonly int[] TargetTurns = { 10, 12, 14, 20, 18, 16, 22, 25, 22, 30 };

        public static int BaseShards(int stageId) => 3 + Mathf.Max(0, stageId) / 2 * 2;

        public static int FastClearTurns(int stageId) =>
            TargetTurns[Mathf.Clamp(stageId, 0, TargetTurns.Length - 1)];

        public static StageRewardResult Calculate(int stageId, bool isFirstClear, int turnsTaken)
        {
            int baseShards = BaseShards(stageId);
            if (!isFirstClear)
                return new StageRewardResult(Mathf.Max(1, Mathf.RoundToInt(baseShards * ReplayRate)), 0, false);
            int bonus = turnsTaken <= FastClearTurns(stageId) ? FastClearBonus : 0;
            return new StageRewardResult(baseShards, bonus, true);
        }
    }
}
