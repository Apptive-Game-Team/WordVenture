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
    // 서리 마을(스테이지 5) 전투가 새 적 데이터로 시작하는지 확인한다.
    public sealed class FrostVillageBattlePlayTests
    {
        const int Stage = 5;
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
        public IEnumerator 서리_마을_전투는_첫_웨이브의_얼음_원거리_슬라임과_바위_근접_슬라임으로_시작한다()
        {
            yield return LoadBattle();
            Type enemyType = Runtime("Combat.Enemies.Enemy");
            var elements = Object.FindObjectsOfType(enemyType).Cast<Component>()
                .Where(enemy => (bool)enemyType.GetProperty("IsAlive").GetValue(enemy))
                .Select(enemy => enemyType.GetField("enemyType").GetValue(enemy).ToString())
                .OrderBy(name => name).ToArray();
            Assert.That(elements, Is.EqualTo(new[] { "Ice", "Rock" }));

            Component stageManager = (Component)Object.FindObjectOfType(Runtime("Combat.Stage.StageManager"));
            var audio = (AudioSource)stageManager.GetType().GetField("audioSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stageManager);
            Assert.That(audio.clip, Is.Not.Null, "스테이지 5의 음악이 없다");
        }
    }
}
#endif
