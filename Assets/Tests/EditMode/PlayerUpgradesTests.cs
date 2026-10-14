using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace WordVenture.Tests
{
    public sealed class PlayerUpgradesTests
    {
        static readonly string[] Keys =
            { "UpgradeShards", "UpgradeLevelAttack", "UpgradeLevelMaxHp", "UpgradeLevelHandLimit", "UpgradeLevelAlly" };

        static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        static Type Upgrades => Find("Core.PlayerUpgrades");
        static Type Reward => Find("Core.StageReward");
        static object Kind(string name) => Enum.Parse(Find("Core.UpgradeKind"), name);

        static object Call(Type type, string method, params object[] args) =>
            type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(info => info.Name == method && info.GetParameters().Length == args.Length)
                .Invoke(null, args);

        static T Read<T>(Type type, string property) => (T)type.GetProperty(property).GetValue(null);
        static T Read<T>(object target, string property) => (T)target.GetType().GetProperty(property).GetValue(target);
        static T Field<T>(object target, string field) => (T)target.GetType().GetField(field).GetValue(target);

        object[] saved;

        [SetUp]
        public void BackUp()
        {
            saved = Keys.Select(key => PlayerPrefs.HasKey(key) ? (object)PlayerPrefs.GetInt(key) : null).ToArray();
            foreach (string key in Keys) PlayerPrefs.DeleteKey(key);
        }

        [TearDown]
        public void Restore()
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                if (saved[i] == null) PlayerPrefs.DeleteKey(Keys[i]);
                else PlayerPrefs.SetInt(Keys[i], (int)saved[i]);
            }
            PlayerPrefs.Save();
        }

        [Test]
        public void StartsAtBaseValues()
        {
            Assert.AreEqual(0, Read<int>(Upgrades, "Shards"));
            Assert.AreEqual(10, Read<int>(Upgrades, "SpellBaseDamage"));
            Assert.AreEqual(100, Read<int>(Upgrades, "PlayerMaxHp"));
            Assert.AreEqual(4, Read<int>(Upgrades, "HandLimit"));
            Assert.AreEqual(8, Read<int>(Upgrades, "AllyMaxHp"));
            Assert.AreEqual(4, Read<int>(Upgrades, "AllyAttackDamage"));
        }

        [Test]
        public void UpgradeSpendsShardsAndRaisesValue()
        {
            Call(Upgrades, "AddShards", 10);
            Assert.IsTrue((bool)Call(Upgrades, "TryUpgrade", Kind("Attack")));
            Assert.AreEqual(7, Read<int>(Upgrades, "Shards"));
            Assert.AreEqual(1, (int)Call(Upgrades, "GetLevel", Kind("Attack")));
            Assert.AreEqual(15, Read<int>(Upgrades, "SpellBaseDamage"));
        }

        [Test]
        public void HandLimitGrowsByOnePairOfCards()
        {
            Call(Upgrades, "AddShards", 100);
            Call(Upgrades, "TryUpgrade", Kind("HandLimit"));
            Assert.AreEqual(6, Read<int>(Upgrades, "HandLimit"));
            Call(Upgrades, "TryUpgrade", Kind("HandLimit"));
            Assert.AreEqual(8, Read<int>(Upgrades, "HandLimit"));
        }

        [Test]
        public void CannotUpgradeWithoutEnoughShards()
        {
            Call(Upgrades, "AddShards", 2);
            Assert.IsFalse((bool)Call(Upgrades, "TryUpgrade", Kind("Attack")));
            Assert.AreEqual(2, Read<int>(Upgrades, "Shards"));
            Assert.AreEqual(0, (int)Call(Upgrades, "GetLevel", Kind("Attack")));
        }

        [Test]
        public void StopsAtMaxLevel()
        {
            Call(Upgrades, "AddShards", 1000);
            object kind = Kind("HandLimit");
            int max = (int)Call(Upgrades, "MaxLevel", kind);
            for (int i = 0; i < max; i++) Assert.IsTrue((bool)Call(Upgrades, "TryUpgrade", kind));
            Assert.IsFalse((bool)Call(Upgrades, "TryUpgrade", kind));
            Assert.AreEqual(-1, (int)Call(Upgrades, "NextCost", kind));
        }

        // 10개 스테이지를 처음, 빨리 깨서 받는 재화로 모든 능력치를 다 올릴 수 없어야 고르는 의미가 있다.
        [Test]
        public void FullRunCannotBuyEveryUpgrade()
        {
            int income = Enumerable.Range(0, 10).Sum(stage => Read<int>(Call(Reward, "Calculate", stage, true, 1), "Total"));
            int allCosts = 0;
            Call(Upgrades, "AddShards", 100000);
            foreach (object kind in Enum.GetValues(Find("Core.UpgradeKind")))
            {
                int cost;
                while ((cost = (int)Call(Upgrades, "NextCost", kind)) >= 0)
                {
                    allCosts += cost;
                    Call(Upgrades, "TryUpgrade", kind);
                }
            }
            Assert.Less(income, allCosts);
        }

        [Test]
        public void RestoreClampsLevels()
        {
            Call(Upgrades, "Restore", 7, new[] { 99, -3, 1 });
            Assert.AreEqual(7, Read<int>(Upgrades, "Shards"));
            Assert.AreEqual((int)Call(Upgrades, "MaxLevel", Kind("Attack")), (int)Call(Upgrades, "GetLevel", Kind("Attack")));
            Assert.AreEqual(0, (int)Call(Upgrades, "GetLevel", Kind("MaxHp")));
            Assert.AreEqual(1, (int)Call(Upgrades, "GetLevel", Kind("HandLimit")));
            Assert.AreEqual(0, (int)Call(Upgrades, "GetLevel", Kind("Ally")));
        }

        [Test]
        public void ResetAllClearsShardsAndLevels()
        {
            Call(Upgrades, "AddShards", 20);
            Call(Upgrades, "TryUpgrade", Kind("MaxHp"));
            Call(Upgrades, "ResetAll");
            Assert.AreEqual(0, Read<int>(Upgrades, "Shards"));
            Assert.AreEqual(0, (int)Call(Upgrades, "GetLevel", Kind("MaxHp")));
        }

        [Test]
        public void FirstClearWithinTargetTurnsGetsBonus()
        {
            int target = (int)Call(Reward, "FastClearTurns", 4);
            object fast = Call(Reward, "Calculate", 4, true, target);
            object slow = Call(Reward, "Calculate", 4, true, target + 1);
            Assert.IsTrue(Read<bool>(fast, "IsFastClear"));
            Assert.IsFalse(Read<bool>(slow, "IsFastClear"));
            Assert.AreEqual(Read<int>(slow, "Total") + 2, Read<int>(fast, "Total"));
        }

        [Test]
        public void ReplayGivesTwentyPercentWithoutBonus()
        {
            object first = Call(Reward, "Calculate", 8, true, 1);
            object replay = Call(Reward, "Calculate", 8, false, 1);
            Assert.AreEqual(11, Field<int>(first, "BaseShards"));
            Assert.AreEqual(2, Read<int>(replay, "Total"));
            Assert.IsFalse(Read<bool>(replay, "IsFastClear"));
            Assert.AreEqual(1, Read<int>(Call(Reward, "Calculate", 0, false, 1), "Total"));
        }
    }
}
