#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    // 실제 전투 씬에서 Spawn으로 세운 아군 슬라임이 턴 종료 버튼을 누르면 적을 공격하는지 확인한다.
    public sealed class AllySlimeBattleScenePlayTests
    {
        const int Stage = 1;
        readonly string[] keys = { "TutorialEnded", "StagePosition" };
        int[] values;
        bool[] existed;
        int oldPosition;
        bool oldLock;
        bool hadSingleton;
        int oldStage;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);
        static Component View => Object.FindObjectOfType(Runtime("Story.StageDialogueView")) as Component;
        static int EnemyHp(Component enemy) => (int)Runtime("Combat.Enemies.Enemy")
            .GetField("Hp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enemy);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            oldLock = (bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);
            Object singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            hadSingleton = singleton != null;
            if (hadSingleton) oldStage = (int)Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").GetValue(singleton);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, false);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            // 테스트 에디터 종료 시 SaveLoadController가 복원한 저장값을 덮어쓰지 않게 한다.
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            Component singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton")) as Component;
            if (singleton != null)
            {
                if (hadSingleton) Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, oldStage);
                else Object.Destroy(singleton.gameObject);
            }
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, oldLock);
        }

        static IEnumerator LoadBattle()
        {
            var singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            if (singleton == null) singleton = new GameObject("StageData").AddComponent(Runtime("Combat.Stage.StageDataSingleton"));
            Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, Stage);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, Stage);
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(0.5f);
            // 전투 시작 대화가 있으면 넘긴다.
            for (int i = 0; i < 60 && View != null; i++)
            {
                Call(View, "Advance");
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator 턴_종료_버튼을_누르면_아군_슬라임이_가장_앞의_적을_공격하고_신성_슬라임은_워드를_회복한다()
        {
            yield return LoadBattle();
            Type formationType = Runtime("Combat.Allies.AllyFormation");
            Component player = (Component)Runtime("Combat.Enemies.Player").GetMethod("PlayerInt").Invoke(null, null);
            Object affinity = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/Magic Affinity Table.asset");
            object formation = formationType.GetMethod("GetOrCreate").Invoke(null, new object[] { player.transform });
            object element = Enum.Parse(Runtime("Cards.MagicType"), "Rock");
            Assert.That(Call(formation, "Spawn", element, affinity), Is.Not.Null,
                "Resources/Combat/AllySlime prefab을 불러오지 못했다");
            Call(formation, "Spawn", Enum.Parse(Runtime("Cards.MagicType"), "Holy"), affinity);
            FieldInfo playerHp = Runtime("Combat.Enemies.Player").GetField("Hp", BindingFlags.Instance | BindingFlags.NonPublic);
            int playerHpBefore = (int)playerHp.GetValue(player);

            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            Component enemyManager = (Component)battle.GetType().GetField("enemyManager").GetValue(battle);
            Component front = (Component)Call(enemyManager, "FindFrontEnemy");
            Assert.That(front, Is.Not.Null, "전투 씬에 적이 없다");
            int hpBefore = EnemyHp(front);

            Call(battle, "TurnEndButton");
            yield return new WaitForSeconds(0.2f);
            Assert.That(EnemyHp(front), Is.LessThan(hpBefore), "아군 슬라임이 턴 종료 때 공격하지 않았다");

            // 두 번째 슬라임은 0.4초 뒤에 행동하고, 적 턴은 그 뒤 0.4초가 지나야 시작한다.
            yield return new WaitForSeconds(0.4f);
            Assert.That((int)playerHp.GetValue(player), Is.EqualTo(playerHpBefore + 4),
                "신성 슬라임이 워드를 회복하지 않았다");
        }
    }
}
#endif
