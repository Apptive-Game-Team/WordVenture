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
    public sealed class LocalizationPlayTests
    {
        readonly string[] keys = { "TutorialEnded", "StagePosition", "Language", "SeenSlimes0", "SeenSlimes1", "SeenTips" };
        int[] values;
        bool[] existed;
        int oldPosition;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Call(object target, string method, params object[] args) => target.GetType()
            .GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
        static object Property(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
        static string Text(Component label) => (string)Property(label, "text");
        static Component Presentation => Object.FindObjectOfType(Runtime("Story.DialogueWindowPresentation")) as Component;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            SharedDialoguePlayTests.SetLanguage("English");
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            PlayerPrefs.DeleteKey("TutorialEnded");
            PlayerPrefs.SetInt("SeenTips", int.MaxValue); // 안내 대사가 입력을 잡지 않게 한다.
            PlayerPrefs.SetInt("StagePosition", 0);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 0);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Component tutorial = Object.FindObjectOfType(Runtime("Tutorial.TutorialController")) as Component;
            if (tutorial != null) Object.Destroy(tutorial.gameObject);
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            SharedDialoguePlayTests.ResetLanguage();
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            yield return null;
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        [UnityTest]
        public IEnumerator 영어를_고르면_설정_메뉴_스토리_튜토리얼이_영어로_나오고_설정_메뉴로_되돌린다()
        {
            SettingsMenuPlayTests.Open();
            Assert.That(Text(Label(SettingsMenuPlayTests.MenuButton("languageButton"))), Is.EqualTo("한국어"),
                "버튼에는 바꿀 언어를 적는다.");
            Assert.That(Text(Label(SettingsMenuPlayTests.MenuButton("creditsButton"))), Is.EqualTo("Credits"));
            SharedDialoguePlayTests.Capture("localization-title-english.png");
            SettingsMenuPlayTests.Close();

            yield return SceneManager.LoadSceneAsync("StoryScene");
            yield return null;
            Component chat = Presentation.GetComponentInParent(Runtime("Story.ChatWindowController"));
            Call(chat, "CompleteStream");
            yield return null;
            Assert.That(Text((Component)Property(Presentation, "TextLabel")), Does.StartWith("In a peaceful mountain valley"));
            Assert.That(Text((Component)Property(Presentation, "PromptLabel")), Is.EqualTo("Click / Any key - Next"));
            Assert.That(((Component)Property(Presentation, "NameLabel")).gameObject.activeSelf, Is.False,
                "번역한 이름이 아니라 원문 이름으로 나레이션을 판단한다.");
            SharedDialoguePlayTests.Capture("localization-story-english.png");

            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            Component tutorial = Object.FindObjectOfType(Runtime("Tutorial.TutorialController")) as Component;
            chat = Presentation.GetComponentInParent(Runtime("Story.ChatWindowController"));
            Call(chat, "CompleteStream");
            yield return null;
            // 튜토리얼은 할아버지의 기록을 읽는 나레이션이라 이름을 숨긴다.
            Assert.That(((Component)Property(Presentation, "NameLabel")).gameObject.activeSelf, Is.False);
            Assert.That(Text(tutorial.transform.Find("TutorialOverlay/SkipTutorial").GetComponentsInChildren<Component>(true)
                .First(c => c.GetType().Name == "TextMeshProUGUI")), Is.EqualTo("Skip Tutorial"));
            SharedDialoguePlayTests.Capture("localization-tutorial-english.png");
            Object.Destroy(tutorial.gameObject);

            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            SettingsMenuPlayTests.Open();
            SettingsMenuPlayTests.MenuButton("languageButton").onClick.Invoke();
            yield return null;
            yield return null;
            Assert.That(SettingsMenuPlayTests.IsOpen, Is.False, "타이틀에서는 씬을 다시 열면서 메뉴를 닫는다.");
            SettingsMenuPlayTests.Open();
            Assert.That(Text(Label(SettingsMenuPlayTests.MenuButton("creditsButton"))), Is.EqualTo("크레딧"));
            Assert.That(Text(Label(SettingsMenuPlayTests.MenuButton("languageButton"))), Is.EqualTo("English"));
            SettingsMenuPlayTests.Close();
            Assert.That(PlayerPrefs.GetInt("Language"), Is.EqualTo(0), "고른 언어를 저장한다.");
        }

        static Component Label(Button button) => button.GetComponentsInChildren<Component>(true)
            .First(c => c.GetType().Name == "TextMeshProUGUI");
    }
}
#endif
