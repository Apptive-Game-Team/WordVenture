#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class StageMusicPlayTests
    {
        readonly List<Object> objects = new List<Object>();
        readonly string[] seenKeys = { "SeenSlimes0", "SeenSlimes1" };
        bool[] hadSeen;
        int[] seen;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static void Set(object target, string name, object value) => target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [SetUp]
        public void SetUp()
        {
            hadSeen = seenKeys.Select(PlayerPrefs.HasKey).ToArray();
            seen = seenKeys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in objects) Object.DestroyImmediate(item);
            objects.Clear();
            for (int i = 0; i < seenKeys.Length; i++)
            {
                if (hadSeen[i]) PlayerPrefs.SetInt(seenKeys[i], seen[i]);
                else PlayerPrefs.DeleteKey(seenKeys[i]);
            }
            PlayerPrefs.Save();
        }

        [TestCase("BossStage", 15)]
        [TestCase("WorldTreeHeartStage", 35)]
        public void 일반적은_지역곡을_유지하고_보스는_전용곡으로_한번만_전환한다(string stageName, int bossId)
        {
            var root = new GameObject("MusicTest");
            objects.Add(root);
            var source = root.AddComponent<AudioSource>();
            var manager = (Behaviour)root.AddComponent(Runtime("Combat.Stage.StageManager"));
            manager.enabled = false; // 전체 전투 초기화 없이 음악 전환 경계만 검사한다.
            var stage = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/ScriptableObjects/Stages/" + stageName + ".asset");
            var serialized = new SerializedObject(stage);
            AudioClip region = serialized.FindProperty("music").objectReferenceValue as AudioClip;
            AudioClip bossMusic = serialized.FindProperty("bossMusic").objectReferenceValue as AudioClip;
            source.clip = region;
            source.volume = .17f;
            source.pitch = .5f;
            Set(manager, "audioSource", source);
            Set(manager, "currentStageData", stage);
            var enemyObject = new GameObject("EnemyTest");
            enemyObject.SetActive(false);
            objects.Add(enemyObject);
            Component ordinary = enemyObject.AddComponent(Runtime("Combat.Enemies.Enemy"));
            MethodInfo notify = manager.GetType().GetMethod("OnEnemySpawned");
            notify.Invoke(manager, new object[] { ordinary });
            Assert.That(source.clip, Is.EqualTo(region));

            Component pool = root.AddComponent(Runtime("Combat.Enemies.EnemyPoolController"));
            Set(pool, "enemyDataContainer", AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/ScriptableObjects/Combat/EnemyDataContainer.asset"));
            MethodInfo spawn = pool.GetType().GetMethod("SpawnObject");
            var bossObject = (GameObject)spawn.Invoke(pool, new object[] { 5f, 0f, bossId });
            objects.Add(bossObject);
            Component boss = bossObject.GetComponent(Runtime("Combat.Enemies.Enemy"));
            Assert.That(source.clip, Is.EqualTo(bossMusic));
            Assert.That(source.loop, Is.True);
            Assert.That(source.pitch, Is.EqualTo(1f));
            Assert.That(source.volume, Is.EqualTo(.17f), "설정 음량을 덮어쓰지 않는다");
            source.timeSamples = 48000;
            notify.Invoke(manager, new object[] { boss });
            Assert.That(source.timeSamples, Is.EqualTo(48000), "본체 등장/중복 알림이 음악을 재시작하면 안 된다");
            if (bossId == 35)
            {
                var core = (GameObject)spawn.Invoke(pool, new object[] { 5f, 0f, 29 });
                objects.Add(core);
                Assert.That(source.clip, Is.EqualTo(bossMusic));
                Assert.That(source.timeSamples, Is.EqualTo(48000), "세계수 본체 소환도 곡을 이어서 재생한다");
            }
        }

        [Test]
        public void 전용곡이_없는_지역의_보스는_기존곡을_유지한다()
        {
            var root = new GameObject("NoBossMusicTest");
            objects.Add(root);
            AudioSource source = root.AddComponent<AudioSource>();
            var manager = (Behaviour)root.AddComponent(Runtime("Combat.Stage.StageManager"));
            manager.enabled = false;
            var stage = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/ScriptableObjects/Stages/PlainStage.asset");
            source.clip = new SerializedObject(stage).FindProperty("music").objectReferenceValue as AudioClip;
            AudioClip original = source.clip;
            Set(manager, "audioSource", source);
            Set(manager, "currentStageData", stage);
            var enemy = new GameObject("OtherBoss");
            enemy.SetActive(false);
            objects.Add(enemy);
            Component boss = enemy.AddComponent(Runtime("Combat.Enemies.BossEnemy"));
            manager.GetType().GetMethod("OnEnemySpawned").Invoke(manager, new object[] { boss });
            Assert.That(source.clip, Is.EqualTo(original));
        }
    }
}
#endif
