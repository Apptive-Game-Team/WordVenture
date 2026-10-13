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
    public sealed class ElementalStatusPlayTests
    {
        readonly List<GameObject> objects = new List<GameObject>();
        Component enemy;
        static Type TypeOf(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Magic(string name) => Enum.Parse(TypeOf("Cards.MagicType"), name);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
        object Status => TypeOf("Combat.Enemies.Enemy").GetProperty("Status").GetValue(enemy);
        int Hp => (int)TypeOf("Combat.Enemies.Enemy").GetField("Hp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enemy);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
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
            yield return null;
        }

        [UnityTest]
        public IEnumerator 한_Explode_공격의_중복_충돌은_피해와_냉기를_중복시키지_않는다()
        {
            var go = new GameObject("TestIceExplode"); objects.Add(go);
            Component spell = go.AddComponent(TypeOf("Combat.Spells.SpellObj"));
            Object affinity = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/Magic Affinity Table.asset");
            Call(spell, "InitSpell", Magic("Explode"), Magic("Ice"),
                enemy.GetComponent(TypeOf("Combat.Enemies.SelectableObject")), affinity);
            Collider2D collider = enemy.GetComponent<Collider2D>();
            Call(spell, "OnTriggerEnter2D", collider);
            int afterHit = Hp;
            Call(spell, "OnTriggerEnter2D", collider);
            Assert.That(Hp, Is.EqualTo(afterHit));
            Assert.That(Hp, Is.LessThan(100));
            Assert.That(Status.GetType().GetProperty("Chill").GetValue(Status), Is.EqualTo(1));
            Assert.That(TypeOf("Combat.Spells.SpellObj").GetProperty("HasActiveSpells").GetValue(null), Is.True);
            Object.Destroy(go);
            yield return null;
            Assert.That(TypeOf("Combat.Spells.SpellObj").GetProperty("HasActiveSpells").GetValue(null), Is.False);
        }

        [UnityTest]
        public IEnumerator 냉기는_실제_이동_거리를_절반으로_줄인다()
        {
            Call(enemy, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 10f, 1f);
            float start = enemy.transform.position.x;
            ((MonoBehaviour)enemy).StartCoroutine((IEnumerator)Call(enemy, "MoveDistance", 4f));
            yield return new WaitForSeconds(1.2f);
            Assert.That(start - enemy.transform.position.x, Is.EqualTo(2f).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator 빙결은_실제_적_턴의_이동을_막고_화상은_턴_종료에서만_준다()
        {
            Call(enemy, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 10f, 1f);
            Call(enemy, "TakeSpellHit", Magic("Ice"), Magic("Drop"), 10f, 1f);
            float start = enemy.transform.position.x;
            Call(enemy, "PlayTurnAction", 100f);
            yield return new WaitForSeconds(1.1f);
            // 피격 넉백은 더한 만큼 빼서 되돌리므로 float 오차만 남는다.
            Assert.That(enemy.transform.position.x, Is.EqualTo(start).Within(0.001f));
            Call(enemy, "EndTurnStatuses");
            Assert.That(Status.GetType().GetProperty("Frozen").GetValue(Status), Is.False);
            Call(enemy, "TakeSpellHit", Magic("Fire"), Magic("Shoot"), 10f, 1f);
            int beforeTurn = Hp;
            Call(enemy, "PlayTurnAction", 100f);
            Assert.That(Hp, Is.EqualTo(beforeTurn));
            yield return new WaitForSeconds(1.1f);
            Call(enemy, "EndTurnStatuses");
            Assert.That(Hp, Is.EqualTo(beforeTurn - 1), "불 속성 적은 화상 피해를 절반으로 줄인다");
            Call(enemy, "EndTurnStatuses");
            Assert.That(Hp, Is.EqualTo(beforeTurn - 1), "같은 턴 종료를 두 번 처리하지 않는다");
        }
    }
}
#endif
