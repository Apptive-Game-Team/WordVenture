using System;
using System.Linq;
using NUnit.Framework;

namespace WordVenture.Tests
{
    public sealed class HitImpactTests
    {
        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Strength(string name) => Enum.Parse(RuntimeType("Combat.HitStrength"), name);
        static object CallImpact(string name, params object[] args)
            => RuntimeType("Combat.HitImpact").GetMethod(name).Invoke(null, args);
        static string Classify(float affinity, bool reaction = false) => CallImpact("Classify", affinity, reaction).ToString();
        static float HitStop(string strength) => (float)CallImpact("GetHitStopSeconds", Strength(strength));
        static float Shake(string strength) => (float)CallImpact("GetShakeAmplitude", Strength(strength));

        [TestCase(-1f, "None")]
        [TestCase(0f, "None")]
        [TestCase(0.7f, "Resisted")]
        [TestCase(1f, "Normal")]
        [TestCase(1.5f, "Weak")]
        public void 적용된_상성으로_타격_세기를_고른다(float affinity, string strength)
        {
            Assert.That(Classify(affinity), Is.EqualTo(strength));
        }

        [TestCase(0.5f, "Reaction")]
        [TestCase(1f, "Reaction")]
        [TestCase(1.5f, "Reaction")]
        [TestCase(-1f, "None")]
        public void 원소_반응은_회복이_아니면_가장_강한_타격이다(float affinity, string strength)
        {
            Assert.That(Classify(affinity, true), Is.EqualTo(strength));
        }

        [Test]
        public void 회복은_멈추거나_흔들지_않는다()
        {
            Assert.That(HitStop("None"), Is.Zero);
            Assert.That(Shake("None"), Is.Zero);
        }

        [Test]
        public void 히트_스톱과_흔들림은_세기가_클수록_커진다()
        {
            Assert.That(HitStop("Resisted"), Is.LessThanOrEqualTo(HitStop("Normal")));
            Assert.That(HitStop("Normal"), Is.LessThan(HitStop("Weak")));
            Assert.That(HitStop("Weak"), Is.LessThan(HitStop("Reaction")));
            Assert.That(Shake("Resisted"), Is.LessThanOrEqualTo(Shake("Normal")));
            Assert.That(Shake("Normal"), Is.LessThan(Shake("Weak")));
            Assert.That(Shake("Weak"), Is.LessThan(Shake("Reaction")));
        }
    }
}
