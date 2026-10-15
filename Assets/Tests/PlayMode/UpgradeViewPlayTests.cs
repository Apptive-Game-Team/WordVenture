#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace WordVenture.Tests
{
    // 맵에서 연 업그레이드 창이 마력 조각을 쓰고 단계를 올리는지 확인한다.
    public sealed class UpgradeViewPlayTests
    {
        static readonly string[] Kinds = { "Attack", "MaxHp", "HandLimit", "Ally" };
        readonly string[] keys = new[] { "TutorialEnded", "StagePosition", "UpgradeShards", "Language", "SeenTips" }
            .Concat(Kinds.Select(k => "UpgradeLevel" + k)).ToArray();
        int[] values;
        bool[] existed;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static bool IsLocked => (bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);
        static Component CurrentView => (Component)Runtime("Map.UpgradeView").GetProperty("Current").GetValue(null);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.SetInt("SeenTips", int.MaxValue); // 안내 대사가 입력을 잡지 않게 한다.
            PlayerPrefs.SetInt("StagePosition", 0);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 0);
            SharedDialoguePlayTests.SetLanguage("Korean");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, PlayerPrefs.GetInt("StagePosition"));
            SharedDialoguePlayTests.ResetLanguage();
        }

        [UnityTest]
        public IEnumerator 조각이_충분하면_단계가_오르고_조각이_줄며_모자라면_오르지_않는다()
        {
            PlayerPrefs.SetInt("UpgradeShards", 4);
            foreach (string kind in Kinds) PlayerPrefs.DeleteKey("UpgradeLevel" + kind);
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            yield return null;

            GameObject.Find("UpgradeButton").GetComponentInChildren<Button>().onClick.Invoke();
            yield return null;
            Component view = CurrentView;
            Assert.That(view, Is.Not.Null, "버튼을 눌렀는데 업그레이드 창이 열리지 않았다");
            Assert.That(IsLocked, Is.True);

            Button attack = view.transform.Find("Window/Background/List/Attack/Button").GetComponent<Button>();
            Button hand = view.transform.Find("Window/Background/List/HandLimit/Button").GetComponent<Button>();
            Assert.That(attack.interactable, Is.True);
            Assert.That(hand.interactable, Is.False, "비용 5 인데 조각 4 로 누를 수 있다");

            attack.onClick.Invoke();
            Assert.That(PlayerPrefs.GetInt("UpgradeLevelAttack"), Is.EqualTo(1));
            Assert.That(PlayerPrefs.GetInt("UpgradeShards"), Is.EqualTo(1));
            Assert.That(attack.interactable, Is.False, "조각 1 로 다음 단계(5)를 누를 수 있다");

            attack.onClick.Invoke();
            Assert.That(PlayerPrefs.GetInt("UpgradeLevelAttack"), Is.EqualTo(1));
            Assert.That(PlayerPrefs.GetInt("UpgradeShards"), Is.EqualTo(1));

            Runtime("Map.UpgradeView").GetMethod("Close").Invoke(view, null);
            yield return null;
            Assert.That(CurrentView == null, Is.True);
            Assert.That(IsLocked, Is.False, "창을 닫았는데 맵 입력 잠금이 남았다");
        }

        [UnityTest]
        public IEnumerator Spawn_해금_전에는_아군_업그레이드를_누를_수_없고_해금_후에는_누를_수_있다()
        {
            PlayerPrefs.SetInt("UpgradeShards", 100);
            foreach (string kind in Kinds) PlayerPrefs.DeleteKey("UpgradeLevel" + kind);
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            yield return null;

            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 5);
            GameObject.Find("UpgradeButton").GetComponentInChildren<Button>().onClick.Invoke();
            yield return null;
            Component view = CurrentView;
            Button ally = view.transform.Find("Window/Background/List/Ally/Button").GetComponent<Button>();
            Assert.That(ally.interactable, Is.False, "Spawn 해금 전인데 아군 업그레이드를 누를 수 있다");

            Runtime("Map.UpgradeView").GetMethod("Close").Invoke(view, null);
            yield return null;

            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 6);
            GameObject.Find("UpgradeButton").GetComponentInChildren<Button>().onClick.Invoke();
            yield return null;
            view = CurrentView;
            ally = view.transform.Find("Window/Background/List/Ally/Button").GetComponent<Button>();
            Assert.That(ally.interactable, Is.True, "Spawn 해금 후인데 아군 업그레이드를 누를 수 없다");

            Runtime("Map.UpgradeView").GetMethod("Close").Invoke(view, null);
            yield return null;
        }
    }
}
#endif
