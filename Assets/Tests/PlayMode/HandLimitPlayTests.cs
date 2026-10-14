#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    // 손패 한도를 넘겨 뽑은 카드는 손패에 들어가지 않고, 불타는 동안 잡을 수 없으며, 끝나면 파괴되는지 확인한다.
    public sealed class HandLimitPlayTests
    {
        const int Stage = 1;
        const string HandLimitKey = "UpgradeLevelHandLimit";
        readonly string[] keys = { "TutorialEnded", "StagePosition", "SeenSlimes0", "SeenSlimes1", HandLimitKey };
        int[] values;
        bool[] existed;
        int oldPosition;
        bool oldLock;
        bool hadSingleton;
        int oldStage;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);
        static Component View => Object.FindObjectOfType(Runtime("Story.StageDialogueView")) as Component;
        static Component Manager => Object.FindObjectOfType(Runtime("Cards.CardManager")) as Component;
        static int HandCount => ((System.Collections.ICollection)Manager.GetType().GetProperty("HandCards").GetValue(Manager)).Count;
        static int HandLimit => (int)Runtime("Core.PlayerUpgrades").GetProperty("HandLimit").GetValue(null);
        static Object[] AllCards => Object.FindObjectsOfType(Runtime("Cards.Card"));

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            oldLock = (bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);
            Object singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            hadSingleton = singleton != null;
            if (hadSingleton) oldStage = (int)Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").GetValue(singleton);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.DeleteKey(HandLimitKey);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, false);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            Component singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton")) as Component;
            if (singleton != null)
            {
                if (hadSingleton) Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, oldStage);
                else Object.Destroy(singleton.gameObject);
            }
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, oldLock);
        }

        static IEnumerator LoadBattle()
        {
            var singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            if (singleton == null) singleton = new GameObject("StageData").AddComponent(Runtime("Combat.Stage.StageDataSingleton"));
            Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, Stage);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, Stage);
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 60 && View != null; i++)
            {
                Call(View, "Advance");
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator 손패_한도를_넘겨_뽑은_카드는_손패에_들어가지_않고_불탄_뒤_파괴된다()
        {
            yield return LoadBattle();
            Assert.That(HandLimit, Is.EqualTo(4));

            for (int i = 0; i < 6; i++) Manager.GetType().GetMethod("AddCard", new[] { typeof(string) }).Invoke(Manager, new object[] { i % 2 == 0 ? "Spell" : "MagicType" });
            Assert.That(HandCount, Is.EqualTo(HandLimit), "손패가 한도를 넘었다");
            Assert.That(AllCards.Length, Is.GreaterThan(HandLimit), "넘친 카드가 불타는 동안 보이지 않는다");

            // 불타는 카드는 태그와 콜라이더가 지워져 잡거나 조합창에 놓을 수 없다.
            var hand = ((System.Collections.IEnumerable)Manager.GetType().GetProperty("HandCards").GetValue(Manager)).Cast<Component>().ToList();
            Component burning = AllCards.Cast<Component>().First(c => !hand.Contains(c));
            Assert.That(burning.CompareTag("Untagged"), Is.True);
            Assert.That(burning.GetComponentsInChildren<Collider2D>().All(c => !c.enabled), Is.True);

            yield return new WaitForSeconds(1.6f);
            Assert.That(AllCards.Length, Is.EqualTo(HandCount), "불탄 카드가 파괴되지 않았다");
            Assert.That(HandCount, Is.EqualTo(HandLimit));
        }

        [UnityTest]
        public IEnumerator 카드를_쓰지_않고_턴을_넘겨도_손패가_한도를_넘지_않는다()
        {
            yield return LoadBattle();
            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            for (int turn = 0; turn < 4; turn++)
            {
                Call(battle, "TurnEndButton");
                yield return new WaitForSeconds(2.5f);
                Assert.That(HandCount, Is.LessThanOrEqualTo(HandLimit), $"{turn + 1}번째 턴 종료 뒤 손패가 한도를 넘었다");
            }
            Assert.That(HandCount, Is.EqualTo(HandLimit));
            yield return new WaitForSeconds(1.5f);
            Assert.That(AllCards.Length, Is.EqualTo(HandCount), "넘친 카드가 남아 있다");
        }
    }
}
#endif
