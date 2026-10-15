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
    // 손패의 카드를 턴마다 한 장 버릴 수 있는지, 버린 카드가 손패에서 빠지고 파괴되는지 확인한다.
    public sealed class DiscardPlayTests
    {
        const int Stage = 1;
        readonly string[] keys = { "TutorialEnded", "StagePosition", "SeenSlimes0", "SeenSlimes1", "SeenTips" };
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
        static System.Collections.Generic.List<Component> Hand =>
            ((System.Collections.IEnumerable)Manager.GetType().GetProperty("HandCards").GetValue(Manager)).Cast<Component>().ToList();
        static bool CanDiscard => (bool)Manager.GetType().GetProperty("CanDiscard").GetValue(Manager);
        static bool TryDiscard(Component card) => (bool)Call(Manager, "TryDiscard", card);
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
            PlayerPrefs.SetInt("SeenTips", int.MaxValue); // 안내 대사가 입력을 잡지 않게 한다.
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
        public IEnumerator 카드를_버리면_손패에서_빠지고_파괴되며_같은_턴에는_다시_버릴_수_없다()
        {
            yield return LoadBattle();
            int cardsUsedBefore = (int)Runtime("Core.BattleStats").GetProperty("CardsUsed").GetValue(null);
            Assert.That(CanDiscard, Is.True);

            int before = Hand.Count;
            Component first = Hand[0];
            Component second = Hand[1];
            Assert.That(TryDiscard(first), Is.True);
            Assert.That(Hand.Count, Is.EqualTo(before - 1));
            Assert.That(Hand.Contains(first), Is.False);
            Assert.That(CanDiscard, Is.False, "한 턴에 두 번 버릴 수 있다");
            Assert.That(TryDiscard(second), Is.False);
            Assert.That(Hand.Contains(second), Is.True);
            Assert.That(Runtime("Core.BattleStats").GetProperty("CardsUsed").GetValue(null), Is.EqualTo(cardsUsedBefore));

            yield return new WaitForSeconds(1.8f);
            Assert.That(first == null, Is.True, "버린 카드가 파괴되지 않았다");
            Assert.That(AllCards.Length, Is.EqualTo(Hand.Count));
        }

        [UnityTest]
        public IEnumerator 턴을_넘기면_다시_카드를_버릴_수_있다()
        {
            yield return LoadBattle();
            Assert.That(TryDiscard(Hand[0]), Is.True);

            Component battle = (Component)Runtime("Battle.Turns.TurnBattleSystem").GetField("Instance").GetValue(null);
            Call(battle, "TurnEndButton");
            yield return new WaitForSeconds(2.5f);

            Assert.That(CanDiscard, Is.True, "다음 플레이어 턴에도 버릴 수 없다");
            int before = Hand.Count;
            Assert.That(TryDiscard(Hand[0]), Is.True);
            Assert.That(Hand.Count, Is.EqualTo(before - 1));
        }

        [UnityTest]
        public IEnumerator 상호작용이_잠겨_있으면_카드를_버릴_수_없다()
        {
            yield return LoadBattle();
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, true);
            Assert.That(CanDiscard, Is.False);
            Assert.That(TryDiscard(Hand[0]), Is.False);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, false);
            Assert.That(CanDiscard, Is.True);
        }
    }
}
#endif
