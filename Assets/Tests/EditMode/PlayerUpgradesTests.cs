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
        static object Grade(string name) => Enum.Parse(Find("Core.StageGrade"), name);

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

        // 10개 스테이지를 처음, S 등급으로 깨서 받는 재화로도 모든 능력치를 다 올릴 수 없어야 고르는 의미가 있다.
        [Test]
        public void FullRunCannotBuyEveryUpgrade()
        {
            int income = Enumerable.Range(0, 10).Sum(stage => Read<int>(Call(Reward, "Calculate", stage, true, Grade("S")), "Total"));
            int allCosts = 0;
            Call(Upgrades, "AddShards", 100000);
            FieldInfo stagePosition = Find("Map.MapMove").GetField("StagePosition");
            object oldStage = stagePosition.GetValue(null);
            stagePosition.SetValue(null, 9);
            try
            {
                foreach (object kind in Enum.GetValues(Find("Core.UpgradeKind")))
                {
                    int cost;
                    while ((cost = (int)Call(Upgrades, "NextCost", kind)) >= 0)
                    {
                        allCosts += cost;
                        Assert.IsTrue((bool)Call(Upgrades, "TryUpgrade", kind));
                    }
                }
            }
            finally
            {
                stagePosition.SetValue(null, oldStage);
            }
            Assert.Less(income, allCosts);
        }

        [Test]
        public void AllyUpgradeIsLockedUntilSpawnUnlocks()
        {
            FieldInfo stagePosition = Find("Map.MapMove").GetField("StagePosition");
            object oldStage = stagePosition.GetValue(null);
            Call(Upgrades, "AddShards", 100);
            try
            {
                stagePosition.SetValue(null, 5);
                Assert.IsFalse((bool)Call(Upgrades, "IsUnlocked", Kind("Ally")));
                Assert.IsFalse((bool)Call(Upgrades, "TryUpgrade", Kind("Ally")));
                Assert.IsTrue((bool)Call(Upgrades, "IsUnlocked", Kind("Attack")));
                stagePosition.SetValue(null, 6);
                Assert.IsTrue((bool)Call(Upgrades, "TryUpgrade", Kind("Ally")));
            }
            finally
            {
                stagePosition.SetValue(null, oldStage);
            }
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

        static object Score(int stage, int turns, int cards, int damage, int combos) =>
            Call(Reward, "Score", stage, turns, cards, damage, combos, 100);

        [Test]
        public void PerfectBattleScoresFullMarksAndGradeS()
        {
            int target = (int)Call(Reward, "TargetTurns", 4);
            object score = Score(4, target, (int)Call(Reward, "TargetCards", 4), 0, 4);
            Assert.AreEqual(1000, Read<int>(score, "Total"));
            Assert.AreEqual(Grade("S"), Read<object>(score, "Grade"));
        }

        [Test]
        public void SlowWastefulBattleLosesPoints()
        {
            int target = (int)Call(Reward, "TargetTurns", 4);
            object score = Score(4, target + 4, (int)Call(Reward, "TargetCards", 4) + 5, 50, 1);
            Assert.AreEqual(200, Field<int>(score, "TurnPoints"));
            Assert.AreEqual(150, Field<int>(score, "CardPoints"));
            Assert.AreEqual(150, Field<int>(score, "DamagePoints"));
            Assert.AreEqual(50, Field<int>(score, "ComboPoints"));
            Assert.AreEqual(Grade("B"), Read<object>(score, "Grade"));
        }

        [Test]
        public void PointsNeverGoNegative()
        {
            object score = Score(0, 999, 999, 999, 0);
            Assert.AreEqual(0, Read<int>(score, "Total"));
            Assert.AreEqual(Grade("C"), Read<object>(score, "Grade"));
        }

        [Test]
        public void FirstClearGetsGradeBonus()
        {
            Assert.AreEqual(7, Read<int>(Call(Reward, "Calculate", 4, true, Grade("C")), "Total"));
            Assert.AreEqual(10, Read<int>(Call(Reward, "Calculate", 4, true, Grade("S")), "Total"));
        }

        [Test]
        public void ReplayGivesTwentyPercentWithoutGradeBonus()
        {
            object first = Call(Reward, "Calculate", 8, true, Grade("C"));
            object replay = Call(Reward, "Calculate", 8, false, Grade("S"));
            Assert.AreEqual(11, Field<int>(first, "BaseShards"));
            Assert.AreEqual(2, Read<int>(replay, "Total"));
            Assert.AreEqual(0, Field<int>(replay, "GradeBonus"));
            Assert.AreEqual(1, Read<int>(Call(Reward, "Calculate", 0, false, Grade("S")), "Total"));
        }
    }
}
