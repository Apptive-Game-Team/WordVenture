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
    // 손패가 아래로 내려가 카드 위쪽만 보이고, 마우스를 올린 카드만 전부 보이게 올라오는지 확인한다.
    public sealed class HandLoweredPlayTests
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
        static float LowerOffset => (float)Runtime("Cards.CardManager").GetField("HandLowerOffset").GetValue(null);
        static Transform CardLeft => (Transform)Manager.GetType()
            .GetField("cardLeft", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Manager);
        static Bounds ColliderOf(Component card) => card.GetComponent<Collider2D>().bounds;
        static float ViewBottom => Camera.main.transform.position.y - Camera.main.orthographicSize;
        static float ViewTop => Camera.main.transform.position.y + Camera.main.orthographicSize;
        static void Hover(Component card, bool over)
        {
            Manager.GetType().GetField("onCardArea", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Manager, true);
            Call(Manager, over ? "CardMouseOver" : "CardMouseExit", card);
        }

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
            yield return new WaitForSeconds(1f);
        }

        [UnityTest]
        public IEnumerator 손패는_원래_줄보다_내려_놓여_카드_위쪽만_보인다()
        {
            yield return LoadBattle();
            Assert.That(Hand.Count, Is.GreaterThan(0));
            float restY = CardLeft.position.y + 1.2f - LowerOffset;
            foreach (Component card in Hand)
            {
                Assert.That(card.transform.position.y, Is.EqualTo(restY).Within(0.01f));
                Bounds bounds = ColliderOf(card);
                Assert.That(bounds.min.y, Is.LessThan(ViewBottom), "카드 아래쪽이 화면 밖으로 내려가 있지 않다");
                float visible = (bounds.max.y - ViewBottom) / bounds.size.y;
                Assert.That(visible, Is.InRange(0.4f, 0.6f), "보이는 부분이 카드의 절반 안팎이 아니다");
            }
        }

        [UnityTest]
        public IEnumerator 마우스를_올린_카드는_화면_안으로_전부_올라오고_벗어나면_다시_내려간다()
        {
            yield return LoadBattle();
            Component first = Hand[0];
            Component second = Hand[1];
            float restY = CardLeft.position.y + 1.2f - LowerOffset;

            Hover(first, true);
            yield return new WaitForSeconds(0.4f);
            Bounds raised = ColliderOf(first);
            Assert.That(raised.min.y, Is.GreaterThanOrEqualTo(ViewBottom - 0.001f), "올라온 카드의 아래쪽이 화면 밖이다");
            Assert.That(raised.max.y, Is.LessThanOrEqualTo(ViewTop));
            Assert.That(first.transform.position.y, Is.EqualTo(restY + LowerOffset).Within(0.01f));
            Assert.That(second.transform.position.y, Is.EqualTo(restY).Within(0.01f));

            // 다른 카드로 옮기면 먼저 올라온 카드가 내려가 한 장만 올라와 있다.
            Hover(second, true);
            yield return new WaitForSeconds(0.4f);
            Assert.That(first.transform.position.y, Is.EqualTo(restY).Within(0.01f));
            Assert.That(second.transform.position.y, Is.EqualTo(restY + LowerOffset).Within(0.01f));

            Hover(second, false);
            yield return new WaitForSeconds(0.4f);
            Assert.That(second.transform.position.y, Is.EqualTo(restY).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator 버리기_칸은_손패를_내려도_원래_자리에_있다()
        {
            yield return LoadBattle();
            GameObject spot = GameObject.Find("DiscardSpot");
            Assert.That(spot, Is.Not.Null);
            Vector3 center = (Vector3)Runtime("Cards.DiscardSpot").GetProperty("Center").GetValue(spot.GetComponent(Runtime("Cards.DiscardSpot")));
            Assert.That(center.y, Is.EqualTo(CardLeft.position.y + 1.2f).Within(0.01f));
        }
    }
}
#endif
