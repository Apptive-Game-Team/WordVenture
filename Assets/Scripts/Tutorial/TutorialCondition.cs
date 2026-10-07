namespace Tutorial
{
    // Unity 오브젝트와 분리해 행동 완료 조건을 검증한다.
    public struct TutorialActionState
    {
        public bool battleStarted, handReady, combineOpen, hasSpell, hasElemental;
        public bool castStarted, targetSelected, turnEnded, battleCleared;
    }

    public static class TutorialCondition
    {
        public static bool CanAdvance(TutorialFlag flag, TutorialActionState state)
        {
            switch (flag)
            {
                case TutorialFlag.FLAG_001_START_TUTORIAL: return state.battleStarted;
                case TutorialFlag.FLAG_002_BATTLE_START: return state.handReady;
                case TutorialFlag.FLAG_003_TURN_START: return true;
                case TutorialFlag.FLAG_004_COMBINATION: return state.combineOpen || state.castStarted;
                case TutorialFlag.FLAG_005_COMBINATION_DESCRIPT: return state.hasSpell || state.castStarted;
                case TutorialFlag.FLAG_006_SET_MAGIC: return (state.hasSpell && state.hasElemental) || state.castStarted;
                case TutorialFlag.FLAG_007_SET_ELEMENTAL: return state.castStarted;
                case TutorialFlag.FLAG_008_CAST_SPELL: return state.castStarted;
                case TutorialFlag.FLAG_009_CAST_END: return state.targetSelected;
                case TutorialFlag.FLAG_010_END_TURN: return state.turnEnded || state.battleCleared;
                case TutorialFlag.FLAG_011_FINISH_SPELL: return state.battleCleared;
                case TutorialFlag.FLAG_012_NEXT_ENEMY: return true;
                default: return false;
            }
        }
    }
}
