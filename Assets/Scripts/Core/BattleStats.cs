namespace Core
{
    // 한 전투에서 쌓는 결과 정산 기록. 전투가 시작할 때 비우고, 클리어 화면에서 점수로 바꾼다.
    public static class BattleStats
    {
        public static int CardsUsed { get; private set; }
        // 회복으로 줄지 않는다. 회복으로 점수를 메우지 못하게 받은 피해만 더한다.
        public static int DamageTaken { get; private set; }
        public static int Combos { get; private set; }

        public static void Reset()
        {
            CardsUsed = 0;
            DamageTaken = 0;
            Combos = 0;
        }

        public static void RecordCardsUsed(int count)
        {
            if (count > 0) CardsUsed += count;
        }

        public static void RecordDamageTaken(int damage)
        {
            if (damage > 0) DamageTaken += damage;
        }

        public static void RecordCombo() => Combos++;
    }
}
