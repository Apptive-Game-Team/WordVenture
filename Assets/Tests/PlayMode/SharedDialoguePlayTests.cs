#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
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
    public sealed class SharedDialoguePlayTests
    {
        readonly string[] keys = { "TutorialEnded", "StagePosition", "ActOneDialogueSeen" };
        int[] values;
        bool[] existed;
        int oldPosition;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Call(object target, string method, params object[] args) => target.GetType()
            .GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
        static object Property(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
        static Component Presentation => Object.FindObjectOfType(Runtime("Story.DialogueWindowPresentation")) as Component;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            PlayerPrefs.DeleteKey("TutorialEnded");
            PlayerPrefs.SetInt("StagePosition", 0);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 0);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Component tutorial = Object.FindObjectOfType(Runtime("Tutorial.TutorialController")) as Component;
            if (tutorial != null) Object.Destroy(tutorial.gameObject);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        [UnityTest]
        public IEnumerator StoryAndTutorialShareStyleAndKeepTheirOwnPortraitModes()
        {
            yield return SceneManager.LoadSceneAsync("StoryScene");
            yield return null;
            Component storyView = Presentation;
            Assert.That(storyView, Is.Not.Null);
            var storyName = (Component)Property(storyView, "NameLabel");
            var storyText = (Component)Property(storyView, "TextLabel");
            Assert.That(storyName.gameObject.activeSelf, Is.False, "나레이션에서는 이름을 숨긴다.");
            Assert.That(((Image)Property(storyView, "LeftPortrait")).gameObject.activeSelf, Is.False);
            Assert.That(((Image)Property(storyView, "RightPortrait")).gameObject.activeSelf, Is.False);
            object font = Property(storyText, "font");
            object size = Property(storyText, "fontSize");
            Color panelColor = storyView.transform.Find("DialoguePanel").GetComponent<Image>().color;
            Component chat = storyView.GetComponentInParent(Runtime("Story.ChatWindowController"));
            Call(chat, "CompleteStream");
            yield return null;
            Capture("shared-dialogue-story.png");

            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            Component tutorialView = Presentation;
            Assert.That(tutorialView, Is.Not.Null);
            Assert.That(Property((Component)Property(tutorialView, "TextLabel"), "font"), Is.SameAs(font));
            Assert.That(Property((Component)Property(tutorialView, "TextLabel"), "fontSize"), Is.EqualTo(size));
            Assert.That(tutorialView.transform.Find("DialoguePanel").GetComponent<Image>().color, Is.EqualTo(panelColor));
            Assert.That(((Component)Property(tutorialView, "NameLabel")).gameObject.activeSelf, Is.True);
            Assert.That(((Image)Property(tutorialView, "LeftPortrait")).sprite, Is.Not.Null);
            Assert.That(((Image)Property(tutorialView, "RightPortrait")).gameObject.activeSelf, Is.False);
            Component tutorial = Object.FindObjectOfType(Runtime("Tutorial.TutorialController")) as Component;
            Assert.That(tutorial.transform.Find("TutorialOverlay/SkipTutorial").gameObject.activeInHierarchy, Is.True);
            chat = tutorialView.GetComponentInParent(Runtime("Story.ChatWindowController"));
            Call(chat, "CompleteStream");
            yield return null;
            Component prompt = (Component)Property(tutorialView, "PromptLabel");
            Assert.That(prompt.gameObject.activeInHierarchy, Is.True, "튜토리얼에서도 공통 진행 안내를 표시한다.");
            Assert.That(Property(prompt, "text"), Is.EqualTo("클릭 / 아무 키 · 다음"));
            Capture("shared-dialogue-tutorial.png");

            Call(tutorial, "AcknowledgeDialogue");
            yield return null;
            yield return null;
            Assert.That(tutorialView.gameObject.activeInHierarchy, Is.False, "행동 안내 단계에서는 창과 초상화를 함께 숨긴다.");
            Assert.That((bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null), Is.False);
            // 숨겼다가 다시 켜도 타이핑 상태가 남지 않아 새 대화를 정상 재생한다.
            Call(tutorial, "StoryTelling");
            Assert.That((bool)Property(chat, "IsStreaming"), Is.True);
            Call(chat, "CompleteStream");
            Assert.That((bool)Property(chat, "IsStreaming"), Is.False);
        }

        [UnityTest]
        public IEnumerator EndingUsesTheSameNarrationWindow()
        {
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 5);
            yield return SceneManager.LoadSceneAsync("EndingScene");
            yield return null;
            Assert.That(Presentation, Is.Not.Null);
            Assert.That(((Component)Property(Presentation, "NameLabel")).gameObject.activeSelf, Is.False);
            Assert.That(((Image)Property(Presentation, "LeftPortrait")).gameObject.activeSelf, Is.False);
            Assert.That(((Image)Property(Presentation, "RightPortrait")).gameObject.activeSelf, Is.False);
        }

        static void Capture(string filename)
        {
            Camera camera = Camera.main;
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>()
                .Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            Camera[] cameras = canvases.Select(c => c.worldCamera).ToArray();
            float[] distances = canvases.Select(c => c.planeDistance).ToArray();
            RenderTexture target = new RenderTexture(1920, 1080, 24);
            RenderTexture oldTarget = camera.targetTexture, previous = RenderTexture.active;
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                foreach (Canvas canvas in canvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1;
                }
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                string directory = Path.Combine(Directory.GetCurrentDirectory(), "docs/design");
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, filename), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = previous;
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceOverlay;
                    canvases[i].worldCamera = cameras[i];
                    canvases[i].planeDistance = distances[i];
                }
                Object.Destroy(image);
                target.Release();
                Object.Destroy(target);
            }
        }
    }
}
#endif
