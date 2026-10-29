#if UNITY_EDITOR && ARTEL_GAME_FUNCTIONS
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
    public sealed class ArtelTestCheatsPlayTests
    {
        readonly string[] keys = { "StagePosition", "TutorialEnded", "ActOneDialogueSeen", "ActTwoDialogueSeen",
            "ShowsActOneMap", "SeenSlimes0", "SeenSlimes1" };
        int[] values;
        bool[] existed;
        int oldPosition;
        float oldTimeScale;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static Type Cheats => Runtime("Core.ArtelTestCheats");
        static FieldInfo Position => Runtime("Map.MapMove").GetField("StagePosition");
        static object Call(string name, params object[] args) => Cheats.GetMethod(name).Invoke(null, args);

        static Component CreateTestPool()
        {
            // 실제 적 prefab/데이터를 사용하되 자동 웨이브/스토리 진행은 분리한다.
            Scene scene = SceneManager.CreateScene("TurnBattleScene");
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("CheatTestPool");
            var pool = root.AddComponent(Runtime("Combat.Enemies.EnemyPoolController"));
            pool.GetType().GetField("enemyDataContainer", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(pool, AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/EnemyDataContainer.asset"));
            var wave = root.AddComponent(Runtime("Combat.Enemies.BattleWaveController"));
            wave.GetType().GetField("ememyPool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(wave, pool);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, false);
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, false);
            return pool;
        }

        static Component Spawn(Component pool, int id)
        {
            var go = (GameObject)pool.GetType().GetMethod("SpawnObject").Invoke(pool, new object[] { 5f, 0f, id });
            return go.GetComponent(Runtime("Combat.Enemies.Enemy"));
        }

        static int Hp(Component enemy) => (int)Runtime("Combat.Enemies.Enemy")
            .GetField("Hp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enemy);

        [UnityTest]
        public IEnumerator 지정_피해는_활성_적에만_적용하고_잘못된_수치를_거절한다()
        {
            var pool = CreateTestPool();
            var enemy = Spawn(pool, 3);
            var inactive = Spawn(pool, 3);
            yield return null;
            inactive.gameObject.SetActive(false);
            int hp = Hp(enemy);
            int inactiveHp = Hp(inactive);
            foreach (int damage in new[] { -1, 0, 1000001 })
                Assert.Throws<TargetInvocationException>(() => Call("DamageAllEnemies", damage));
            Assert.That(Hp(enemy), Is.EqualTo(hp));
            Assert.That(Call("DamageAllEnemies", 1), Is.EqualTo("affected=1 damage=1"));
            Assert.That(Hp(enemy), Is.EqualTo(hp - 1));
            Assert.That(Hp(inactive), Is.EqualTo(inactiveHp));
        }

        [UnityTest]
        public IEnumerator 전멸은_분열_자식도_처리하고_이후에_생긴_적은_건드리지_않는다()
        {
            var pool = CreateTestPool();
            Spawn(pool, 24);
            yield return null;
            var routine = (IEnumerator)Call("KillAllEnemies");
            string result = null;
            while (routine.MoveNext())
            {
                if (routine.Current is string message) result = message;
                else yield return routine.Current;
            }
            Assert.That(result, Is.EqualTo("killed=3"));
            Assert.That(Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")).Cast<Component>()
                .All(e => Hp(e) <= 0), Is.True);
            var next = Spawn(pool, 3);
            int hp = Hp(next);
            yield return null;
            Assert.That(Hp(next), Is.EqualTo(hp));
        }

        [UnityTest]
        public IEnumerator 전투_밖과_메뉴_중에는_적_치트를_거절한다()
        {
            Assert.Throws<TargetInvocationException>(() => Call("DamageAllEnemies", 1));
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator)Call("KillAllEnemies")).MoveNext());
            var pool = CreateTestPool();
            var enemy = Spawn(pool, 3);
            yield return null;
            int hp = Hp(enemy);
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, true);
            Assert.Throws<TargetInvocationException>(() => Call("DamageAllEnemies", 1));
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator)Call("KillAllEnemies")).MoveNext());
            Assert.That(Hp(enemy), Is.EqualTo(hp));
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, false);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            oldPosition = (int)Position.GetValue(null);
            oldTimeScale = Time.timeScale;
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            var save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Position.SetValue(null, oldPosition);
            Time.timeScale = oldTimeScale;
        }

        [UnityTest]
        public IEnumerator 저장_준비는_진행도와_튜토리얼을_맞추고_기존_발견을_지운다()
        {
            PlayerPrefs.SetInt("SeenSlimes0", 7);
            PlayerPrefs.SetInt("ActTwoDialogueSeen", 3);
            Call("PrepareTestSave", 7, true, true, true);
            Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(7));
            Assert.That(Position.GetValue(null), Is.EqualTo(7));
            Assert.That(PlayerPrefs.GetInt("TutorialEnded"), Is.EqualTo(1));
            Assert.That(PlayerPrefs.GetInt("ShowsActOneMap"), Is.EqualTo(1));
            Assert.That(PlayerPrefs.HasKey("SeenSlimes0"), Is.False);
            Assert.That(PlayerPrefs.HasKey("ActTwoDialogueSeen"), Is.False);
            Call("PrepareTestSave", -1, true, true, false);
            Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(-1));
            Assert.That(PlayerPrefs.HasKey("TutorialEnded"), Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator 잘못된_입력과_동의_없는_호출은_저장을_바꾸지_않는다()
        {
            Call("PrepareTestSave", 3, true, false, false);
            foreach (object[] args in new[] {
                new object[] { 11, true, true, false }, new object[] { -2, true, true, false },
                new object[] { 7, false, true, false }, new object[] { 3, true, true, true } })
            {
                Assert.Throws<TargetInvocationException>(() => Call("PrepareTestSave", args));
                Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(3));
                Assert.That(PlayerPrefs.HasKey("TutorialEnded"), Is.False);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator 맵_밖에서는_전투_진입을_거절한다()
        {
            var routine = (IEnumerator)Call("EnterTestBattle", 0);
            Assert.Throws<InvalidOperationException>(() => routine.MoveNext());
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TitleScene"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator 타이틀_복귀는_일시정지와_입력_잠금을_해제한다()
        {
            Call("PrepareTestSave", 7, true, true, false);
            var interaction = Runtime("Core.InteractionLock");
            interaction.GetProperty("IsLocked").SetValue(null, true);
            interaction.GetProperty("IsPaused").SetValue(null, true);
            Time.timeScale = 0;
            yield return (IEnumerator)Call("ReturnToTestTitle");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TitleScene"));
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(interaction.GetProperty("IsLocked").GetValue(null), Is.False);
            Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(7));
        }
    }
}
#endif
