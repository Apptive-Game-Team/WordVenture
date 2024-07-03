using Enemy;
using System.Collections;
using System.Collections.Generic;
using TurnBattle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TutorialSystem
{
    
    public interface ITutorialCondition
    {
        public bool isMeetCondition();
        public ITutorialCondition GetNextCondition();
    }

    public class TutorialConditon_002 : ITutorialCondition
    {
        string turnBattleScene = "TurnBattleScene";
        public bool isMeetCondition()
        {
            return (StageDataSingleton.Instance.StagePosition == 0) &&
                SceneManager.GetActiveScene().name == turnBattleScene
                && TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_001_START_TUTORIAL);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_003();
        }

    }
    public class TutorialConditon_003 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_002_BATTLE_START);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_004();
        }

    }
    public class TutorialConditon_004 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_003_TURN_START);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_005();
        }

    }
    public class TutorialConditon_005 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_004_COMBINATION)
                && CombineZone.Instance.gameObject.activeSelf;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_006();
        }

    }
    public class TutorialConditon_006 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_005_COMBINATION_DESCRIPT)
                && CombineZone.Instance.spellCards.Count == 1;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_007();
        }

    }
    public class TutorialConditon_007 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_006_SET_MAGIC) 
                && CombineZone.Instance.spellCards.Count == 1
                && CombineZone.Instance.magicTypeCards.Count == 1;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_008();
        }

    }
    public class TutorialConditon_008 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_007_SET_ELEMENTAL)
                && CombineZone.Instance.spellCards.Count == 0
                && CombineZone.Instance.magicTypeCards.Count == 0;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_009();
        }

    }
    public class TutorialConditon_009 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_008_CAST_SPELL) 
                && Input.anyKeyDown;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_010();
        }
    }
    public class TutorialConditon_010 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_009_CAST_END);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_011();
        }

    }
    public class TutorialConditon_011 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_010_CLICK_TO_SELECT);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_012();
        }

    }
    public class TutorialConditon_012 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_011_FINISH_SPELL)
                && SceneManager.GetActiveScene().name.Equals("GameClearScene");
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_013();
        }

    }
    public class TutorialConditon_013 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_012_NEXT_ENEMY);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_014();
        }

    }
    public class TutorialConditon_014 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_013_END_BATTLE);
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_015();
        }
    }
    public class TutorialConditon_015 : ITutorialCondition
    {
        public bool isMeetCondition()
        {
            return true;
        }

        public ITutorialCondition GetNextCondition()
        {
            return new TutorialConditon_015();
        }

    }
}

