using Combat.Stage;
using Combat.UI;
using UnityEngine.SceneManagement;

namespace Tutorial
{
    public interface ITutorialCondition
    {
        public bool IsMeetCondition();
        public ITutorialCondition GetNextCondition();
    }

    public class TutorialConditon002 : ITutorialCondition
    {
        string turnBattleScene = "TurnBattleScene";
        public bool IsMeetCondition()
        {
            return (StageDataSingleton.Instance.stagePosition == 0) &&
                SceneManager.GetActiveScene().name == turnBattleScene
                && TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_001_START_TUTORIAL);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon003();
        }

    }
    public class TutorialConditon003 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_002_BATTLE_START);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon004();
        }

    }
    public class TutorialConditon004 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_003_TURN_START);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon005();
        }

    }
    public class TutorialConditon005 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_004_COMBINATION)
                && CombineZone.Instance.gameObject.activeSelf;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon006();
        }

    }
    public class TutorialConditon006 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_005_COMBINATION_DESCRIPT)
                && CombineZone.Instance.spellCards.Count == 1;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon007();
        }

    }
    public class TutorialConditon007 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_006_SET_MAGIC)
                && CombineZone.Instance.spellCards.Count == 1
                && CombineZone.Instance.magicTypeCards.Count == 1;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon008();
        }

    }
    public class TutorialConditon008 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_007_SET_ELEMENTAL)
                && CombineZone.Instance.spellCards.Count == 0
                && CombineZone.Instance.magicTypeCards.Count == 0;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon009();
        }

    }
    public class TutorialConditon009 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_008_CAST_SPELL);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon010();
        }
    }
    public class TutorialConditon010 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_009_CAST_END);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon011();
        }

    }
    public class TutorialConditon011 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_010_CLICK_TO_SELECT);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon012();
        }

    }
    public class TutorialConditon012 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_011_FINISH_SPELL)
                && SceneManager.GetActiveScene().name.Equals("GameClearScene");
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon013();
        }

    }
    public class TutorialConditon013 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_012_NEXT_ENEMY);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon014();
        }

    }
    public class TutorialConditon014 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_013_END_BATTLE);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon015();
        }
    }
    public class TutorialConditon015 : ITutorialCondition
    {
        public bool IsMeetCondition()
        {
            return true;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon015();
        }

    }
}

