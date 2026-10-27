using Battle.Turns;
using UnityEngine;

namespace Combat.Enemies
{
    // 전투 턴이 3번 지날 때마다 모든 적의 공격력이 10%씩 더해서 커진다. 전투 전체에 걸리므로 나중 웨이브의 적도 현재 단계로 나온다.
    public static class EnemyRage
    {
        public const int TurnsPerLevel = 3;
        public const float AttackPerLevel = 0.1f;
        public const float ScalePerLevel = 0.06f;
        public const float MaxScaleBonus = 0.3f;

        // 첫 플레이어 턴이 1이다. 턴 1~3은 0단계, 4~6은 1단계다.
        public static int LevelFromTurn(int turnNumber)
        {
            return Mathf.Max(0, (turnNumber - 1) / TurnsPerLevel);
        }

        public static float AttackMultiplier(int level)
        {
            return 1f + AttackPerLevel * Mathf.Max(0, level);
        }

        public static float BodyScaleMultiplier(int level)
        {
            return 1f + Mathf.Min(MaxScaleBonus, ScalePerLevel * Mathf.Max(0, level));
        }

        // 내림하되 분노가 없을 때의 값보다 작아지지 않는다.
        public static int ApplyToDamage(int damage, int level)
        {
            return Mathf.Max(damage, Mathf.FloorToInt(damage * AttackMultiplier(level)));
        }

        // 테스트나 미리보기 씬처럼 TurnBattleSystem 이 없으면 0단계다.
        public static int CurrentLevel
        {
            get
            {
                TurnBattleSystem system = TurnBattleSystem.Instance;
                return system == null ? 0 : LevelFromTurn(system.TurnNumber);
            }
        }
    }
}
