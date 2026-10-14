#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    // 손패 상한, 적 공격력 상승, 업그레이드 창, 클리어 재화 화면을 docs/design 에 캡처한다. 기본 테스트 실행에는 들어가지 않는다.
    [Explicit]
    public sealed class CombatPressureCaptures
    {
        const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        readonly string[] keys = { "TutorialEnded", "StagePosition", "ActOneDialogueSeen", "ActTwoDialogueSeen",
            "UpgradeShards", "UpgradeLevelAttack", "UpgradeLevelMaxHp", "UpgradeLevelHandLimit", "UpgradeLevelAlly" };
        bool[] existed;
        int[] values;
        int oldPosition;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);
        static object Call(object target, string method, params object[] args) =>
            (target as Type ?? target.GetType()).GetMethods(Any)
                .First(info => info.Name == method && info.GetParameters().Length == args.Length)
                .Invoke(target is Type ? null : target, args);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.SetInt("ActOneDialogueSeen", int.MaxValue);
            PlayerPrefs.SetInt("ActTwoDialogueSeen", int.MaxValue);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 6);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("TitleScene");
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        [UnityTest]
        public IEnumerator 업그레이드_창()
        {
            PlayerPrefs.SetInt("UpgradeShards", 14);
            PlayerPrefs.SetInt("UpgradeLevelAttack", 2);
            PlayerPrefs.SetInt("UpgradeLevelMaxHp", 1);
            PlayerPrefs.SetInt("UpgradeLevelHandLimit", 2);
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return new WaitForSeconds(0.5f);
            SharedDialoguePlayTests.Capture("upgrade-map-button.png");
            Call(Runtime("Map.UpgradeView"), "Open");
            yield return new WaitForSeconds(0.3f);
            SharedDialoguePlayTests.Capture("upgrade-window.png");
        }

        [UnityTest]
        public IEnumerator 적_공격력_상승과_카드_소각()
        {
            yield return EnterBattle(6);
            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            // 7번째 턴이면 분노 2단계다.
            for (int i = 0; i < 6; i++) Call(battle, "BeginPlayerTurn");
            yield return new WaitForSeconds(0.8f);
            SharedDialoguePlayTests.Capture("enemy-rage.png");

            object cards = battle.GetType().GetField("cardManager").GetValue(battle);
            for (int i = 0; i < 4; i++) Call(cards, "AddCard", i % 2 == 0 ? "Spell" : "MagicType");
            yield return new WaitForSeconds(0.55f);
            SharedDialoguePlayTests.Capture("card-burn.png");
        }

        [UnityTest]
        public IEnumerator 클리어_재화()
        {
            PlayerPrefs.SetInt("UpgradeShards", 9);
            Type stageType = Runtime("Combat.Stage.StageDataSingleton");
            var singleton = Object.FindObjectOfType(stageType) ?? new GameObject("StageData").AddComponent(stageType);
            stageType.GetField("stagePosition").SetValue(singleton, 4);
            stageType.GetField("isFirstClear").SetValue(singleton, true);
            stageType.GetField("lastClearTurns").SetValue(singleton, 13);
            object reward = Activator.CreateInstance(Runtime("Core.StageRewardResult"), 7, 2, true);
            stageType.GetField("lastReward").SetValue(singleton, reward);
            yield return SceneManager.LoadSceneAsync("GameClearScene");
            yield return new WaitForSeconds(0.5f);
            if (Object.FindObjectOfType(Runtime("Scenes.StageRewardView")) == null)
                Call(Runtime("Scenes.StageRewardView"), "Show", singleton);
            yield return new WaitForSeconds(0.3f);
            SharedDialoguePlayTests.Capture("clear-reward.png");
        }

        IEnumerator EnterBattle(int stage)
        {
            Type stageType = Runtime("Combat.Stage.StageDataSingleton");
            var singleton = Object.FindObjectOfType(stageType) ?? new GameObject("StageData").AddComponent(stageType);
            stageType.GetField("stagePosition").SetValue(singleton, stage);
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(1.5f);
        }
    }
}
#endif
