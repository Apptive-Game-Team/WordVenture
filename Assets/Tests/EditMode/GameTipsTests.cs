using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace WordVenture.Tests
{
    public sealed class GameTipsTests
    {
        const string Key = "SeenTips";
        bool existed;
        int saved;

        static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        static Type Tips => Find("Story.GameTips");
        static object Tip(string name) => Enum.Parse(Find("Story.GameTip"), name);
        static bool HasSeen(string tip) => (bool)Tips.GetMethod("HasSeen").Invoke(null, new[] { Tip(tip) });
        static void MarkSeen(string tip) => Tips.GetMethod("MarkSeen").Invoke(null, new[] { Tip(tip) });

        [SetUp]
        public void SetUp()
        {
            existed = PlayerPrefs.HasKey(Key);
            saved = PlayerPrefs.GetInt(Key);
            PlayerPrefs.DeleteKey(Key);
        }

        [TearDown]
        public void TearDown()
        {
            if (existed) PlayerPrefs.SetInt(Key, saved); else PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

        [Test]
        public void 처음에는_어떤_안내도_보지_않은_상태다()
        {
            foreach (string tip in Enum.GetNames(Find("Story.GameTip"))) Assert.IsFalse(HasSeen(tip), tip);
        }

        [Test]
        public void 읽음_기록은_안내마다_따로_남는다()
        {
            MarkSeen("EnemyRage");
            Assert.IsTrue(HasSeen("EnemyRage"));
            Assert.IsFalse(HasSeen("HandLimit"));
            Assert.IsFalse(HasSeen("Result"));
            Assert.AreEqual(1 << (int)Tip("EnemyRage"), PlayerPrefs.GetInt(Key));
        }

        [Test]
        public void 같은_안내를_다시_기록해도_다른_안내_기록은_유지된다()
        {
            MarkSeen("HandLimit");
            MarkSeen("Upgrade");
            MarkSeen("HandLimit");
            Assert.IsTrue(HasSeen("HandLimit"));
            Assert.IsTrue(HasSeen("Upgrade"));
            Assert.IsFalse(HasSeen("AllyUpgrade"));
        }

        [Test]
        public void 모든_안내는_본문이_있고_세_줄을_넘지_않는다()
        {
            foreach (object tip in Enum.GetValues(Find("Story.GameTip")))
            {
                var texts = (string[])Tips.GetMethod("TextsOf").Invoke(null, new[] { tip });
                Assert.That(texts.Length, Is.InRange(1, 3), tip.ToString());
                Assert.That(texts.All(text => !string.IsNullOrWhiteSpace(text)), tip.ToString());
            }
        }

        [Test]
        public void 안내_본문은_영어_번역이_있다()
        {
            string json = System.IO.File.ReadAllText("Assets/Resources/Localization/English.json");
            foreach (object tip in Enum.GetValues(Find("Story.GameTip")))
                foreach (string text in (string[])Tips.GetMethod("TextsOf").Invoke(null, new[] { tip }))
                    StringAssert.Contains("\"ko\": \"" + text + "\"", json, text);
            StringAssert.Contains("\"ko\": \"할아버지의 기록\"", json);
            StringAssert.Contains("\"ko\": \"" + (string)Find("Story.GameTips").GetField("IntroLine").GetValue(null) + "\"", json);
        }
    }
}
