using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace WordVenture.Tests
{
    public sealed class UpgradeCloudSaveTests
    {
        static readonly string[] Keys =
        {
            "UpgradeShards", "UpgradeLevelAttack", "UpgradeLevelMaxHp", "UpgradeLevelHandLimit", "UpgradeLevelAlly", "SeenTips",
            "StagePosition", "TutorialEnded", "ActOneDialogueSeen", "ActTwoDialogueSeen", "ShowsActOneMap",
        };

        static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        static Type UpgradeType => Find("Account.UpgradeCloudSave");

        static object NewSave(int shards, params int[] levels)
        {
            object save = Activator.CreateInstance(UpgradeType);
            UpgradeType.GetField("Shards").SetValue(save, shards);
            UpgradeType.GetField("Levels").SetValue(save, levels);
            return save;
        }

        static string Serialize(object save) => (string)UpgradeType.GetMethod("Serialize").Invoke(save, null);

        static bool TryParse(string value, out object save)
        {
            object[] args = { value, null };
            bool ok = (bool)UpgradeType.GetMethod("TryParse").Invoke(null, args);
            save = args[1];
            return ok;
        }

        [TestCase(0, 0, 0, 0, 0)]
        [TestCase(37, 6, 5, 2, 3)]
        [TestCase(99999, 6, 5, 2, 3)]
        public void 직렬화한_값을_다시_읽으면_같은_저장이_된다(int shards, int attack, int maxHp, int hand, int ally)
        {
            string text = Serialize(NewSave(shards, attack, maxHp, hand, ally));
            Assert.That(text, Is.EqualTo($"2|{shards}|{attack}|{maxHp}|{hand}|{ally}|0"));
            Assert.That(TryParse(text, out object parsed), Is.True, text);
            Assert.That((int)UpgradeType.GetField("Shards").GetValue(parsed), Is.EqualTo(shards));
            Assert.That((int[])UpgradeType.GetField("Levels").GetValue(parsed), Is.EqualTo(new[] { attack, maxHp, hand, ally }));
        }

        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(int.MinValue, int.MinValue)]
        public void 어떤_int_값이어도_서버_value_한도_31자_이하이고_다시_읽힌다(int shards, int level)
        {
            string text = Serialize(NewSave(shards, level, level, level, level));
            Assert.That(text.Length, Is.LessThanOrEqualTo(31), text);
            Assert.That(TryParse(text, out _), Is.True, text);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("2|1|0|0|0|0")]
        [TestCase("2|1|0|0|0|0|")]
        [TestCase("2|1|0|0|0|0|xyz")]
        [TestCase("2|1|0|0|0|0|123456789")]
        [TestCase("0|1|0|0|0|0")]
        [TestCase("1|1|0|0|0")]
        [TestCase("1|1|0|0|0|0|0")]
        [TestCase("1|x|0|0|0|0")]
        [TestCase("1||0|0|0|0")]
        [TestCase("1|-1|0|0|0|0")]
        [TestCase("1|100000|0|0|0|0")]
        [TestCase("1|1|7|0|0|0")]
        [TestCase("1|1|0|6|0|0")]
        [TestCase("1|1|0|0|3|0")]
        [TestCase("1|1|0|0|0|4")]
        [TestCase("1|1|-1|0|0|0")]
        [TestCase("1|1|a|0|0|0")]
        [TestCase("1|1|0|0|0|")]
        [TestCase("1|1| 1|0|0|0")]
        [TestCase("1|1|001|0|0|0")]
        [TestCase("1|1|0|0|0|٣")]
        public void 형식이_틀린_문자열은_거절한다(string text)
        {
            Assert.That(TryParse(text, out object parsed), Is.False);
            Assert.That(parsed, Is.Null);
        }

        [Test]
        public void 안내_기록이_없는_버전_1_저장도_읽는다()
        {
            Assert.That(TryParse("1|12|2|0|0|1", out object old), Is.True);
            Assert.That(UpgradeType.GetField("Shards").GetValue(old), Is.EqualTo(12));
            Assert.That(UpgradeType.GetField("SeenTips").GetValue(old), Is.Null);
            Assert.That(TryParse("2|12|2|0|0|1|1f", out object current), Is.True);
            Assert.That(UpgradeType.GetField("SeenTips").GetValue(current), Is.EqualTo(0x1f));
        }

        [Test]
        public void 로컬_업그레이드를_읽고_다시_쓰며_CloudSave_적용에_함께_실린다()
        {
            var backup = Keys.ToDictionary(key => key, key => PlayerPrefs.HasKey(key) ? (int?)PlayerPrefs.GetInt(key) : null);
            Type saveType = Find("Account.CloudSave");
            try
            {
                PlayerPrefs.SetInt("UpgradeShards", 12);
                PlayerPrefs.SetInt("UpgradeLevelAttack", 2);
                PlayerPrefs.SetInt("UpgradeLevelAlly", 1);
                object local = UpgradeType.GetMethod("FromLocal").Invoke(null, null);
                PlayerPrefs.SetInt("SeenTips", 0b10101);
                local = UpgradeType.GetMethod("FromLocal").Invoke(null, null);
                Assert.That(Serialize(local), Is.EqualTo("2|12|2|0|0|1|15"));

                // 서버에 upgrade 키가 없으면 Upgrades 가 null 이고 이 기기의 값이 그대로 남는다.
                object cloud = Activator.CreateInstance(saveType);
                saveType.GetMethod("ApplyToLocal").Invoke(cloud, null);
                Assert.That(PlayerPrefs.GetInt("UpgradeShards"), Is.EqualTo(12));
                Assert.That(PlayerPrefs.GetInt("UpgradeLevelAttack"), Is.EqualTo(2));

                saveType.GetField("Upgrades").SetValue(cloud, NewSave(5, 1, 1, 1, 1));
                saveType.GetMethod("ApplyToLocal").Invoke(cloud, null);
                Assert.That(PlayerPrefs.GetInt("UpgradeShards"), Is.EqualTo(5));
                Assert.That(PlayerPrefs.GetInt("UpgradeLevelAttack"), Is.EqualTo(1));
                Assert.That(PlayerPrefs.GetInt("UpgradeLevelHandLimit"), Is.EqualTo(1));
                // NewSave 는 안내 기록이 없는 버전 1 저장과 같아서 이 기기의 안내 기록을 그대로 둔다.
                Assert.That(Serialize(UpgradeType.GetMethod("FromLocal").Invoke(null, null)), Is.EqualTo("2|5|1|1|1|1|15"));

                object withTips = NewSave(5, 1, 1, 1, 1);
                UpgradeType.GetField("SeenTips").SetValue(withTips, (int?)0b11);
                saveType.GetField("Upgrades").SetValue(cloud, withTips);
                saveType.GetMethod("ApplyToLocal").Invoke(cloud, null);
                Assert.That(PlayerPrefs.GetInt("SeenTips"), Is.EqualTo(0b11), "서버의 안내 기록을 불러온다.");
            }
            finally
            {
                foreach (var pair in backup)
                {
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
                }
                Find("Map.MapMove").GetField("StagePosition").SetValue(null, PlayerPrefs.GetInt("StagePosition", -1));
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void InitPlayData는_업그레이드를_초기화한다()
        {
            var backup = Keys.ToDictionary(key => key, key => PlayerPrefs.HasKey(key) ? (int?)PlayerPrefs.GetInt(key) : null);
            Type controllerType = Find("Core.SaveLoadController");
            var holder = new GameObject("UpgradeResetTest");
            try
            {
                PlayerPrefs.SetInt("UpgradeShards", 9);
                PlayerPrefs.SetInt("UpgradeLevelMaxHp", 3);
                controllerType.GetMethod("InitPlayData").Invoke(holder.AddComponent(controllerType), null);
                Assert.That(PlayerPrefs.HasKey("UpgradeShards"), Is.False);
                Assert.That(PlayerPrefs.HasKey("UpgradeLevelMaxHp"), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
                foreach (var pair in backup)
                {
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
                }
                PlayerPrefs.Save();
            }
        }
    }
}
