#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class SettingsMenuPlayTests
    {
        readonly string[] keys = { "TutorialEnded", "StagePosition", "Language" };
        int[] values;
        bool[] existed;
        int oldPosition;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static Component Menu => (Component)Runtime("Scenes.SettingsMenu").GetProperty("Instance").GetValue(null);
        static object Field(string name) => Menu.GetType()
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Menu);
        static bool Locked => (bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);

        internal static bool IsOpen => (bool)Menu.GetType().GetProperty("IsOpen").GetValue(Menu);
        internal static void Open() => Menu.GetType().GetMethod("Open").Invoke(Menu, null);
        internal static void Close() => Menu.GetType().GetMethod("Close").Invoke(Menu, null);
        internal static Button MenuButton(string field) => (Button)Field(field);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            SharedDialoguePlayTests.SetLanguage("Korean");
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.SetInt("StagePosition", 2);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 2);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (Menu != null) Close();
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            SharedDialoguePlayTests.ResetLanguage();
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        [UnityTest]
        public IEnumerator 메뉴를_열면_시간과_입력이_멈추고_닫으면_되돌린다()
        {
            Assert.That(Menu, Is.Not.Null, "씬에 배치한 설정 메뉴가 있다.");
            Assert.That(MenuButton("openButton").gameObject.activeInHierarchy, Is.True, "왼쪽 위 메뉴 버튼이 보인다.");

            Open();
            Assert.That(Time.timeScale, Is.EqualTo(0));
            Assert.That(Locked, Is.True);
            Assert.That(MenuButton("titleButton").gameObject.activeSelf, Is.False, "타이틀에서는 타이틀로 항목을 숨긴다.");
            Assert.That(MenuButton("resumeButton").navigation.selectOnUp, Is.EqualTo(MenuButton("creditsButton")),
                "방향키로 메뉴 아래의 타이틀 버튼을 고르지 않는다.");
            SharedDialoguePlayTests.Capture("settings-menu-title.png");

            Close();
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(Locked, Is.True, "메뉴를 닫은 입력이 같은 프레임에 게임으로 넘어가지 않는다.");
            yield return null;
            Assert.That(Locked, Is.False);
        }

        [UnityTest]
        public IEnumerator 맵에서_타이틀로를_누르면_저장하고_타이틀로_간다()
        {
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            Assert.That(((RectTransform)MenuButton("openButton").transform).anchoredPosition.y, Is.EqualTo(-170),
                "맵에서는 메뉴 버튼을 로고 아래로 내린다.");
            Open();
            Assert.That(MenuButton("titleButton").gameObject.activeSelf, Is.True);
            SharedDialoguePlayTests.Capture("settings-menu-map.png");
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 3);

            MenuButton("titleButton").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TitleScene"));
            Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(3), "이동하기 전에 진행을 저장한다.");
            Assert.That(IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(Object.FindObjectsOfType(Runtime("Scenes.SettingsMenu")).Length, Is.EqualTo(1),
                "이전 씬의 메뉴는 씬과 함께 사라지고 새 씬의 메뉴 하나만 남는다.");
        }

        [UnityTest]
        public IEnumerator 대화_중에_언어를_바꾸면_씬을_다시_열지_않고_안내한다()
        {
            yield return SceneManager.LoadSceneAsync("StoryScene");
            yield return null;
            var marker = new GameObject("SettingsMenuTestMarker");
            Open();

            MenuButton("languageButton").onClick.Invoke();
            yield return null;

            Assert.That(marker != null, Is.True, "대화 씬을 다시 열면 진행을 잃는다.");
            Assert.That(IsOpen, Is.True);
            Assert.That(((Component)Field("languageNotice")).gameObject.activeSelf, Is.True);
            Assert.That(PlayerPrefs.GetInt("Language"), Is.EqualTo(1), "고른 언어를 저장한다.");
            SharedDialoguePlayTests.Capture("settings-menu-story-language.png");
        }

        [UnityTest]
        public IEnumerator 전투에서_메뉴를_열면_적이_멈춘다()
        {
            Type stageData = Runtime("Combat.Stage.StageDataSingleton");
            Object singleton = Object.FindObjectOfType(stageData);
            bool hadSingleton = singleton != null;
            if (!hadSingleton) singleton = new GameObject("StageData").AddComponent(stageData);
            object oldStage = stageData.GetField("stagePosition").GetValue(singleton);
            stageData.GetField("stagePosition").SetValue(singleton, 1);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 1);
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(0.5f);
            // 전투 시작 대화가 있으면 넘겨서 적이 움직이는 상태에서 메뉴를 연다.
            Type view = Runtime("Story.StageDialogueView");
            for (int i = 0; i < 60 && Object.FindObjectOfType(view) != null; i++)
            {
                view.GetMethod("Advance").Invoke(Object.FindObjectOfType(view), null);
                yield return null;
            }
            yield return new WaitForSeconds(1f);

            Open();
            Transform[] enemies = Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")).Cast<Component>()
                .Select(enemy => enemy.transform).ToArray();
            Vector3[] before = enemies.Select(enemy => enemy.position).ToArray();
            yield return new WaitForSecondsRealtime(1f);
            SharedDialoguePlayTests.Capture("settings-menu-battle.png");

            Assert.That(enemies, Is.Not.Empty);
            Assert.That(enemies.Select(enemy => enemy.position).ToArray(), Is.EqualTo(before), "메뉴가 열려 있는 동안 적이 움직였다.");
            Close();
            if (hadSingleton) stageData.GetField("stagePosition").SetValue(singleton, oldStage);
            else Object.Destroy(((Component)singleton).gameObject);
        }

        [UnityTest]
        public IEnumerator 크레딧을_열고_닫는다()
        {
            Open();
            MenuButton("creditsButton").onClick.Invoke();
            yield return null;
            var credits = (Component)Field("credits");
            Assert.That(credits.gameObject.activeSelf, Is.True);
            SharedDialoguePlayTests.Capture("settings-menu-credits.png");

            credits.GetType().GetMethod("Hide").Invoke(credits, null);
            Assert.That(credits.gameObject.activeSelf, Is.False);
            Assert.That(IsOpen, Is.True, "크레딧을 닫으면 메뉴로 돌아간다.");
        }
    }
}
#endif
