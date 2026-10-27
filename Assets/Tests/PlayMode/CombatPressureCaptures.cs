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
            "UpgradeShards", "UpgradeLevelAttack", "UpgradeLevelMaxHp", "UpgradeLevelHandLimit", "UpgradeLevelAlly", "SeenTips" };
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
            PlayerPrefs.SetInt("SeenTips", int.MaxValue); // 안내 대사가 입력을 잡지 않게 한다.
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

        // 넘친 카드가 위쪽부터 타 들어가는 과정을 여러 시점으로 찍는다.
        [UnityTest]
        public IEnumerator 카드_소각_과정()
        {
            yield return EnterBattle(6);
            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            object cards = battle.GetType().GetField("cardManager").GetValue(battle);
            for (int i = 0; i < 2; i++) Call(cards, "AddCard", i % 2 == 0 ? "Spell" : "MagicType");
            yield return new WaitForSeconds(0.6f);
            Call(cards, "AddCard", "Spell");
            float start = Time.time;
            float[] moments = { 0.35f, 0.6f, 0.85f, 1.1f, 1.35f };
            for (int i = 0; i < moments.Length; i++)
            {
                while (Time.time - start < moments[i]) yield return null;
                SharedDialoguePlayTests.Capture("card-burn-" + i + ".png");
            }
        }

        // 손패가 처음 넘칠 때 뜨는 할아버지의 기록 안내.
        [UnityTest]
        public IEnumerator 손패_안내()
        {
            yield return EnterBattle(6);
            PlayerPrefs.SetInt("SeenTips", 0);
            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            object cards = battle.GetType().GetField("cardManager").GetValue(battle);
            for (int i = 0; i < 3; i++) Call(cards, "AddCard", "Spell");
            yield return new WaitForSeconds(4f);
            // 캡처는 Overlay Canvas 를 카메라 공간으로 바꿔 찍어서, 정렬 순서가 높은 카드가 대화창 위로 올라온다.
            // 실제 게임에서는 대화창이 카드를 덮으므로 캡처할 때만 카드와 버리기 칸을 숨긴다.
            foreach (Component card in Object.FindObjectsOfType(Runtime("Cards.Card")).Cast<Component>()) card.gameObject.SetActive(false);
            GameObject spot = GameObject.Find("DiscardSpot");
            if (spot != null) spot.SetActive(false);
            SharedDialoguePlayTests.Capture("tip-hand-limit.png");
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

        // 손패가 예전에 슬라임을 가리던 자리에 슬라임을 세우고, 카드 한 장에 마우스를 올린 모습.
        [UnityTest]
        public IEnumerator 손패_내림()
        {
            yield return EnterBattle(6);
            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            object cards = battle.GetType().GetField("cardManager").GetValue(battle);
            foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")).Cast<Component>().Take(1))
                enemy.transform.position = new Vector3(0f, enemy.transform.position.y, enemy.transform.position.z);
            yield return new WaitForSeconds(0.8f);
            Component hovered = ((System.Collections.IEnumerable)cards.GetType().GetProperty("HandCards").GetValue(cards)).Cast<Component>().ElementAt(1);
            cards.GetType().GetField("onCardArea", Any).SetValue(cards, true);
            Call(cards, "CardMouseOver", hovered);
            yield return new WaitForSeconds(0.5f);
            SharedDialoguePlayTests.Capture("hand-lowered.png");
        }

        [UnityTest]
        public IEnumerator 클리어_결과()
        {
            PlayerPrefs.SetInt("UpgradeShards", 9);
            Type stageType = Runtime("Combat.Stage.StageDataSingleton");
            var singleton = Object.FindObjectOfType(stageType) ?? new GameObject("StageData").AddComponent(stageType);
            stageType.GetField("stagePosition").SetValue(singleton, 6);
            stageType.GetField("isFirstClear").SetValue(singleton, true);
            object score = Activator.CreateInstance(Runtime("Core.StageScore"), 13, 14, 35, 3, 300, 180, 195, 150);
            stageType.GetField("lastScore").SetValue(singleton, score);
            object reward = Activator.CreateInstance(Runtime("Core.StageRewardResult"), 7, 2, true);
            stageType.GetField("lastReward").SetValue(singleton, reward);
            stageType.GetField("hasResult").SetValue(singleton, true);
            yield return SceneManager.LoadSceneAsync("GameClearScene");
            yield return new WaitForSeconds(0.5f);
            Type viewType = Runtime("Scenes.StageResultView");
            var view = Object.FindObjectOfType(viewType);
            if (view == null) view = (Object)Call(viewType, "Show", singleton);
            viewType.GetMethod("Skip").Invoke(view, null);
            yield return new WaitForSeconds(0.3f);
            SharedDialoguePlayTests.Capture("clear-result.png");
        }

        // 아군 슬라임이 속성마다 다른 투사체를 쏘는 장면.
        [UnityTest]
        public IEnumerator 아군_투사체()
        {
            yield return EnterBattle(6);
            Component player = (Component)Runtime("Combat.Enemies.Player").GetMethod("PlayerInt").Invoke(null, null);
            Object affinity = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/Magic Affinity Table.asset");
            Type formationType = Runtime("Combat.Allies.AllyFormation");
            object formation = formationType.GetMethod("GetOrCreate").Invoke(null, new object[] { player.transform });
            foreach (string element in new[] { "Rock", "Fire", "Lightning" })
                Call(formation, "Spawn", Enum.Parse(Runtime("Cards.MagicType"), element), affinity);
            yield return new WaitForSeconds(0.5f);
            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            Call(battle, "TurnEndButton");
            yield return new WaitForSeconds(0.12f);
            SharedDialoguePlayTests.Capture("ally-projectile.png");
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
