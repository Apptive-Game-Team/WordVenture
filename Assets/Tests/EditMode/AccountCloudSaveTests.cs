using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace WordVenture.Tests
{
    public sealed class AccountCloudSaveTests
    {
        static readonly string[] Keys = { "StagePosition", "TutorialEnded", "ActOneDialogueSeen", "ActTwoDialogueSeen", "ShowsActOneMap" };

        static Type SaveType => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Account.CloudSave")).First(type => type != null);

        static object NewSave(int stage, bool tutorial, int actOne, int actTwo, bool map)
        {
            object save = Activator.CreateInstance(SaveType);
            Set(save, "StagePosition", stage);
            Set(save, "TutorialEnded", tutorial);
            Set(save, "ActOneDialogueSeen", actOne);
            Set(save, "ActTwoDialogueSeen", actTwo);
            Set(save, "ShowsActOneMap", map);
            return save;
        }

        static void Set(object target, string field, object value) => SaveType.GetField(field).SetValue(target, value);
        static T Get<T>(object target, string field) => (T)SaveType.GetField(field).GetValue(target);
        static string Serialize(object save) => (string)SaveType.GetMethod("Serialize").Invoke(save, null);

        static bool TryParse(string value, out object save)
        {
            object[] args = { value, null };
            bool ok = (bool)SaveType.GetMethod("TryParse").Invoke(null, args);
            save = args[1];
            return ok;
        }

        [TestCase(-1, false, 0, 0, false)]
        [TestCase(3, true, 5, 9, true)]
        [TestCase(9999, true, int.MaxValue, int.MaxValue, true)]
        [TestCase(12, false, -1, -1, false)]
        public void 직렬화한_값을_다시_읽으면_같은_저장이_된다(int stage, bool tutorial, int actOne, int actTwo, bool map)
        {
            string text = Serialize(NewSave(stage, tutorial, actOne, actTwo, map));
            Assert.That(TryParse(text, out object parsed), Is.True, text);
            Assert.That(Get<int>(parsed, "StagePosition"), Is.EqualTo(stage));
            Assert.That(Get<bool>(parsed, "TutorialEnded"), Is.EqualTo(tutorial));
            Assert.That(Get<int>(parsed, "ActOneDialogueSeen"), Is.EqualTo(actOne));
            Assert.That(Get<int>(parsed, "ActTwoDialogueSeen"), Is.EqualTo(actTwo));
            Assert.That(Get<bool>(parsed, "ShowsActOneMap"), Is.EqualTo(map));
        }

        [TestCase(int.MaxValue, int.MaxValue, int.MaxValue)]
        [TestCase(int.MinValue, int.MinValue, int.MinValue)]
        [TestCase(-1, -1, -1)]
        public void 어떤_int_값이어도_서버_value_한도_31자_이하이다(int stage, int actOne, int actTwo)
        {
            string text = Serialize(NewSave(stage, true, actOne, actTwo, true));
            Assert.That(text.Length, Is.LessThanOrEqualTo(31), text);
            Assert.That(TryParse(text, out _), Is.True, text);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("2|1|0|0|0|0")]
        [TestCase("0|1|0|0|0|0")]
        [TestCase("1|1|0|0|0")]
        [TestCase("1|1|0|0|0|0|0")]
        [TestCase("1|x|0|0|0|0")]
        [TestCase("1||0|0|0|0")]
        [TestCase("1|-2|0|0|0|0")]
        [TestCase("1|10000|0|0|0|0")]
        [TestCase("1|1|2|0|0|0")]
        [TestCase("1|1|0|0|0|true")]
        [TestCase("1|1|0|g|0|0")]
        [TestCase("1|1|0||0|0")]
        [TestCase("1|1|0|123456789|0|0")]
        [TestCase("1|1|0|0| 1|0")]
        [TestCase("save")]
        public void 형식이_틀린_문자열은_거절한다(string text)
        {
            Assert.That(TryParse(text, out object parsed), Is.False);
            Assert.That(parsed, Is.Null);
        }

        [Test]
        public void 로컬_저장을_읽고_다시_쓴다()
        {
            var backup = Keys.ToDictionary(key => key, key => PlayerPrefs.HasKey(key) ? (int?)PlayerPrefs.GetInt(key) : null);
            Type moveType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Map.MapMove")).First(type => type != null);
            FieldInfo stageField = moveType.GetField("StagePosition");
            object previousStage = stageField.GetValue(null);
            try
            {
                PlayerPrefs.SetInt("StagePosition", 4);
                PlayerPrefs.SetInt("TutorialEnded", 1);
                PlayerPrefs.SetInt("ActOneDialogueSeen", 0b1011);
                PlayerPrefs.SetInt("ActTwoDialogueSeen", 0b110);
                PlayerPrefs.SetInt("ShowsActOneMap", 1);
                object local = SaveType.GetMethod("FromLocal").Invoke(null, null);
                Assert.That(Serialize(local), Is.EqualTo("1|4|1|b|6|1"));

                foreach (string key in Keys) PlayerPrefs.DeleteKey(key);
                object empty = SaveType.GetMethod("FromLocal").Invoke(null, null);
                Assert.That(Serialize(empty), Is.EqualTo("1|-1|0|0|0|0"), "저장이 없으면 StagePosition -1");

                SaveType.GetMethod("ApplyToLocal").Invoke(local, null);
                Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(4));
                Assert.That(PlayerPrefs.GetInt("TutorialEnded"), Is.EqualTo(1));
                Assert.That(PlayerPrefs.GetInt("ActOneDialogueSeen"), Is.EqualTo(0b1011));
                Assert.That(PlayerPrefs.GetInt("ActTwoDialogueSeen"), Is.EqualTo(0b110));
                Assert.That(PlayerPrefs.GetInt("ShowsActOneMap"), Is.EqualTo(1));
                Assert.That(stageField.GetValue(null), Is.EqualTo(4));
            }
            finally
            {
                foreach (var pair in backup)
                {
                    if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
                }
                stageField.SetValue(null, previousStage);
                PlayerPrefs.Save();
            }
        }
    }
}
