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

        // Scene.name은 접근할 때마다 문자열을 새로 만든다. 이 조건은 매 프레임
        // 검사되므로, 가장 싸고 선별력이 높은 플래그 검사를 앞에 두어 플래그가
        // 맞는 프레임에만 씬 이름을 읽게 한다. 피연산자에 부수 효과가 없어
        // 순서를 바꿔도 결과는 같다.
        public bool IsMeetCondition()
        {
            return TutorialController.Instance.IsFlagEqual(TutorialFlag.FLAG_001_START_TUTORIAL)
                && StageDataSingleton.Instance.stagePosition == 0
                && SceneManager.GetActiveScene().name == turnBattleScene;
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

