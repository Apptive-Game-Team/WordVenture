#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class HitImpactPlayTests
    {
        readonly List<GameObject> objects = new List<GameObject>();
        Component enemy;
        Camera camera;
        static Type TypeOf(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Magic(string name) => Enum.Parse(TypeOf("Cards.MagicType"), name);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
        object LastMark => TypeOf("Combat.Enemies.AffinityMarkVfx").GetProperty("LastMark")
            .GetValue(((Component)enemy).GetComponent(TypeOf("Combat.Enemies.AffinityMarkVfx")));
        object LastStrength => TypeOf("Combat.Enemies.HitFlashVfx").GetProperty("LastStrength")
            .GetValue(((Component)enemy).GetComponent(TypeOf("Combat.Enemies.HitFlashVfx")));

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var cameraObject = new GameObject("TestCamera") { tag = "MainCamera" };
            objects.Add(cameraObject);
            camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Enemies/Slime/MeleeNeon.prefab");
            GameObject slime = Object.Instantiate(prefab);
            objects.Add(slime);
            enemy = slime.GetComponent(TypeOf("Combat.Enemies.Enemy"));
            object data = Activator.CreateInstance(TypeOf("Combat.Enemies.EnemyData"));
            data.GetType().GetField("maxHp").SetValue(data, 100);
            data.GetType().GetField("moveDistance").SetValue(data, 4f);
            data.GetType().GetField("attackRange").SetValue(data, 3f);
            data.GetType().GetField("damage").SetValue(data, 10);
            data.GetType().GetField("type").SetValue(data, Magic("Fire"));
            Call(enemy, "InitEnemyData", data);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject go in objects) if (go != null) Object.Destroy(go);
            objects.Clear();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator 약점_타격은_시간을_잠깐_멈추고_카메라와_위치를_되돌린다()
        {
            Vector3 cameraStart = camera.transform.position;
            float enemyStart = enemy.transform.position.x;
            Call(enemy, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 10f, 1.5f);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(LastMark.ToString(), Is.EqualTo("Weak"));
            Assert.That(LastStrength.ToString(), Is.EqualTo("Weak"));
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(camera.transform.position, Is.EqualTo(cameraStart));
            Assert.That(enemy.transform.position.x, Is.EqualTo(enemyStart).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator 저항_타격은_시간을_멈추지_않고_저항_표시를_띄운다()
        {
            Call(enemy, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 10f, 0.7f);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(LastMark.ToString(), Is.EqualTo("Resisted"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator 회복은_타격_효과를_내지_않는다()
        {
            Call(enemy, "TakeSpellHit", Magic("Holy"), Magic("Shoot"), 10f, -1f);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(LastMark, Is.Null);
            Assert.That(LastStrength, Is.Null);
            yield return null;
        }
    }
}
#endif
