using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class TargetSelectionMergeTests
    {
        GameObject zoneObject;
        GameObject targetObject;
        Component zone;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        static object Call(object target, string method, params object[] args) => target.GetType()
            .GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);

        [SetUp]
        public void SetUp()
        {
            zoneObject = new GameObject("TargetSelectionZone");
            zone = zoneObject.AddComponent(Runtime("Combat.UI.CombineZone"));
            ((Behaviour)zone).enabled = false;
            Set(zone, "<IsAwaitingTarget>k__BackingField", true);
            // 시전 대기가 시작된 다음 웨이브에서 생긴 대상도 선택할 수 있어야 한다.
            targetObject = new GameObject("NewWaveTarget");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(targetObject);
            Object.DestroyImmediate(zoneObject);
        }

        [Test]
        public void 대기_시작_후_생성된_대상도_선택한다()
        {
            Component selectable = targetObject.AddComponent(Runtime("Combat.Enemies.SelectableObject"));
            Assert.That(Call(selectable, "GetSelectable"), Is.True);
            Call(zone, "SetTarget", selectable);
            Assert.That(zone.GetType().GetProperty("IsAwaitingTarget").GetValue(zone), Is.False);
            Assert.That(Call(selectable, "GetSelectable"), Is.False);
        }

        [TestCase(0, true)]
        [TestCase(5, false)]
        public void 아군은_살아있을_때만_대상으로_선택한다(int hp, bool stillWaiting)
        {
            Component ally = targetObject.AddComponent(Runtime("Combat.Allies.AllySlime"));
            Set(ally, "hp", hp);
            Component selectable = targetObject.AddComponent(Runtime("Combat.Enemies.SelectableObject"));
            Call(zone, "SetTarget", selectable);
            Assert.That(zone.GetType().GetProperty("IsAwaitingTarget").GetValue(zone), Is.EqualTo(stillWaiting));
        }

        [Test]
        public void 호버_종료는_분노로_커진_크기를_복원한다()
        {
            Component selectable = targetObject.AddComponent(Runtime("Combat.Enemies.SelectableObject"));
            Vector3 enraged = new Vector3(2f, 3f, 1f);
            targetObject.transform.localScale = enraged;
            Call(selectable, "OnMouseEnter");
            Call(selectable, "OnMouseEnter");
            Assert.That(targetObject.transform.localScale, Is.EqualTo(enraged * 1.2f));
            Call(selectable, "OnMouseExit");
            Assert.That(targetObject.transform.localScale, Is.EqualTo(enraged));
        }
    }
}
