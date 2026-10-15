using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace WordVenture.Tests
{
    public sealed class EnemyRageTests
    {
        static Type RageType => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Combat.Enemies.EnemyRage")).First(type => type != null);

        static object Call(string name, params object[] args) => RageType.GetMethod(name, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 1)]
        [TestCase(6, 1)]
        [TestCase(7, 2)]
        [TestCase(13, 4)]
        public void 턴_번호로_분노_단계를_구한다(int turn, int level)
        {
            Assert.That((int)Call("LevelFromTurn", turn), Is.EqualTo(level));
        }

        [TestCase(0, 1f)]
        [TestCase(1, 1.1f)]
        [TestCase(3, 1.3f)]
        public void 단계마다_공격력_배율이_더해서_커진다(int level, float multiplier)
        {
            Assert.That((float)Call("AttackMultiplier", level), Is.EqualTo(multiplier).Within(0.0001f));
        }

        [TestCase(10, 0, 10)]
        [TestCase(10, 1, 11)]
        [TestCase(7, 1, 7)]
        [TestCase(7, 2, 8)]
        [TestCase(1, 1, 1)]
        [TestCase(0, 5, 0)]
        public void 피해는_내림하고_분노가_없을_때보다_작아지지_않는다(int damage, int level, int expected)
        {
            Assert.That((int)Call("ApplyToDamage", damage, level), Is.EqualTo(expected));
        }

        [TestCase(0, 1f)]
        [TestCase(1, 1.06f)]
        [TestCase(4, 1.24f)]
        [TestCase(5, 1.3f)]
        [TestCase(20, 1.3f)]
        public void 몸집_배율은_6퍼센트씩_커지고_30퍼센트에서_멈춘다(int level, float scale)
        {
            Assert.That((float)Call("BodyScaleMultiplier", level), Is.EqualTo(scale).Within(0.0001f));
        }

        [Test]
        public void 전투_시스템이_없으면_0단계다()
        {
            Assert.That((int)RageType.GetProperty("CurrentLevel").GetValue(null), Is.EqualTo(0));
        }
    }
}
