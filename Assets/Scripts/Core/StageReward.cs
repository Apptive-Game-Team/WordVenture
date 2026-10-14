using UnityEngine;

namespace Core
{
    public enum StageGrade
    {
        C,
        B,
        A,
        S
    }

    public readonly struct StageScore
    {
        public readonly int Turns;
        public readonly int CardsUsed;
        public readonly int DamageTaken;
        public readonly int Combos;
        public readonly int TurnPoints;
        public readonly int CardPoints;
        public readonly int DamagePoints;
        public readonly int ComboPoints;

        public StageScore(int turns, int cardsUsed, int damageTaken, int combos,
            int turnPoints, int cardPoints, int damagePoints, int comboPoints)
        {
            Turns = turns;
            CardsUsed = cardsUsed;
            DamageTaken = damageTaken;
            Combos = combos;
            TurnPoints = turnPoints;
            CardPoints = cardPoints;
            DamagePoints = damagePoints;
            ComboPoints = comboPoints;
        }

        public int Total => TurnPoints + CardPoints + DamagePoints + ComboPoints;
        public StageGrade Grade => StageReward.GradeFor(Total);
    }

    public readonly struct StageRewardResult
    {
        public readonly int BaseShards;
        public readonly int GradeBonus;
        public readonly bool IsFirstClear;

        public StageRewardResult(int baseShards, int gradeBonus, bool isFirstClear)
        {
            BaseShards = baseShards;
            GradeBonus = gradeBonus;
            IsFirstClear = isFirstClear;
        }

        public int Total => BaseShards + GradeBonus;
    }

    // 클리어 결과를 1000점 만점으로 매기고, 등급에 따라 업그레이드 재화를 준다.
    // 턴 300, 깎인 HP 300, 쓴 카드 200, 원소 연계 200점이다.
    public static class StageReward
    {
        public const int MaxTurnPoints = 300;
        public const int MaxDamagePoints = 300;
        public const int MaxCardPoints = 200;
        public const int MaxComboPoints = 200;
        const int PointsPerExtraTurn = 25;
        const int PointsPerExtraCard = 10;
        const int PointsPerCombo = 50;
        // 이미 깬 스테이지를 다시 깨면 기본 재화의 20%만 주고 등급 보너스는 없다. 쉬운 스테이지를 반복해 모든 능력치를 올리지 못하게 한다.
        public const float ReplayRate = 0.2f;

        // 이 턴 수 안에 깨면 턴 점수가 만점이다. 3~9는 자동 전투 기록(docs/design/act-two-balance)에 여유를 더한 값이고,
        // 0~2는 아직 기록이 없어 웨이브 수로 정했다. 사람 플레이 기록을 보고 다시 맞춘다.
        static readonly int[] TargetTurnTable = { 10, 12, 14, 20, 18, 16, 22, 25, 22, 30 };

        public static int BaseShards(int stageId) => 3 + Mathf.Max(0, stageId) / 2 * 2;

        public static int TargetTurns(int stageId) =>
            TargetTurnTable[Mathf.Clamp(stageId, 0, TargetTurnTable.Length - 1)];

        // 한 턴에 주문 카드와 속성 카드를 1장씩 뽑는다. 목표 턴 동안 뽑는 카드의 절반을 쓰면 만점이다.
        public static int TargetCards(int stageId) => TargetTurns(stageId);

        public static StageScore Score(int stageId, int turns, int cardsUsed, int damageTaken, int combos, int playerMaxHp)
        {
            int turnPoints = Mathf.Clamp(MaxTurnPoints - PointsPerExtraTurn * (turns - TargetTurns(stageId)), 0, MaxTurnPoints);
            int cardPoints = Mathf.Clamp(MaxCardPoints - PointsPerExtraCard * (cardsUsed - TargetCards(stageId)), 0, MaxCardPoints);
            float lostRate = playerMaxHp > 0 ? (float)damageTaken / playerMaxHp : 1f;
            int damagePoints = Mathf.Clamp(Mathf.RoundToInt(MaxDamagePoints * (1f - lostRate)), 0, MaxDamagePoints);
            int comboPoints = Mathf.Clamp(PointsPerCombo * combos, 0, MaxComboPoints);
            return new StageScore(turns, cardsUsed, damageTaken, combos, turnPoints, cardPoints, damagePoints, comboPoints);
        }

        public static StageGrade GradeFor(int total) =>
            total >= 850 ? StageGrade.S : total >= 700 ? StageGrade.A : total >= 500 ? StageGrade.B : StageGrade.C;

        public static int GradeBonus(StageGrade grade) => (int)grade;

        public static StageRewardResult Calculate(int stageId, bool isFirstClear, StageGrade grade)
        {
            int baseShards = BaseShards(stageId);
            if (!isFirstClear)
                return new StageRewardResult(Mathf.Max(1, Mathf.RoundToInt(baseShards * ReplayRate)), 0, false);
            return new StageRewardResult(baseShards, GradeBonus(grade), true);
        }
    }
}
