#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class TitleMenuPlayTests
    {
        const string StagePositionKey = "StagePosition";
        bool existed;
        int oldValue;
        int oldPosition;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);

        [SetUp]
        public void SetUp()
        {
            existed = PlayerPrefs.HasKey(StagePositionKey);
            oldValue = PlayerPrefs.GetInt(StagePositionKey);
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (existed) PlayerPrefs.SetInt(StagePositionKey, oldValue); else PlayerPrefs.DeleteKey(StagePositionKey);
            PlayerPrefs.Save();
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            yield return null;
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        [UnityTest]
        public IEnumerator 저장이_없으면_이어하기_자리를_비우지_않고_새_게임과_종료_버튼을_위로_붙인다()
        {
            PlayerPrefs.SetInt(StagePositionKey, -1);
            yield return LoadTitle();

            Assert.That(Button("continue").activeSelf, Is.False);
            Assert.That(Y("MapSceneButton"), Is.EqualTo(-60f).Within(0.01f), "새 게임 버튼이 맨 위 자리로 올라간다.");
            Assert.That(Y("ExitButton"), Is.EqualTo(-220f).Within(0.01f), "종료 버튼이 새 게임 버튼 바로 아래에 붙는다.");
            SharedDialoguePlayTests.Capture("title-menu-no-save.png");
        }

        [UnityTest]
        public IEnumerator 저장이_있으면_이어하기를_맨_위에_두고_새_게임과_종료를_차례로_놓는다()
        {
            PlayerPrefs.SetInt(StagePositionKey, 0);
            yield return LoadTitle();

            Assert.That(Button("continue").activeSelf, Is.True);
            Assert.That(Y("continue"), Is.EqualTo(-60f).Within(0.01f));
            Assert.That(Y("MapSceneButton"), Is.EqualTo(-220f).Within(0.01f));
            Assert.That(Y("ExitButton"), Is.EqualTo(-380f).Within(0.01f));
            SharedDialoguePlayTests.Capture("title-menu-saved.png");
        }

        [UnityTest]
        public IEnumerator 저장이_있으면_시작하기가_확인_창을_띄우고_취소하면_진행도를_그대로_둔다()
        {
            PlayerPrefs.SetInt(StagePositionKey, 3);
            yield return LoadTitle();

            Button("MapSceneButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;

            Transform confirmation = GameObject.Find("Canvas").transform.Find("NewGameConfirmation");
            Assert.That(confirmation, Is.Not.Null, "저장이 있으면 시작하기가 확인 창을 띄운다.");
            Assert.That(confirmation.gameObject.activeSelf, Is.True);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TitleScene"));
            Assert.That(PlayerPrefs.GetInt(StagePositionKey), Is.EqualTo(3), "확인 전에는 진행도를 지우지 않는다.");
            SharedDialoguePlayTests.Capture("title-new-game-confirmation.png");

            confirmation.Find("ConfirmationPanel/CancelNewGame").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;

            Assert.That(confirmation.gameObject.activeSelf, Is.False);
            Assert.That(PlayerPrefs.GetInt(StagePositionKey), Is.EqualTo(3));
        }

        static IEnumerator LoadTitle()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(Group);
        }

        static RectTransform Group => (RectTransform)GameObject.Find("MenuButtons").transform;

        static GameObject Button(string name) => Group.Find(name).gameObject;

        static float Y(string name) => ((RectTransform)Group.Find(name)).anchoredPosition.y;
    }
}
#endif
