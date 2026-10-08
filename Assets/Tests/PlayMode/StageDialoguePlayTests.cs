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
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class StageDialoguePlayTests
    {
        readonly string[] keys = { "ActOneDialogueSeen", "TutorialEnded", "StagePosition" };
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
        static bool Locked => (bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            oldLock = Locked;
            Object singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            hadSingleton = singleton != null;
            if (hadSingleton) oldStage = (int)Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").GetValue(singleton);
            PlayerPrefs.SetInt("ActOneDialogueSeen", 0);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, false);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (View != null) Object.Destroy(View.gameObject);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            // 테스트 에디터 종료 시 SaveLoadController가 복원한 저장값을 덮어쓰지 않게 한다.
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

        static IEnumerator ClearStage(int stage)
        {
            var singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            if (singleton == null) singleton = new GameObject("StageData").AddComponent(Runtime("Combat.Stage.StageDataSingleton"));
            Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, stage);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, stage + 1);
            yield return SceneManager.LoadSceneAsync("GameClearScene");
            yield return null;
        }

        static IEnumerator FinishDialogue()
        {
            for (int i = 0; i < 30 && View != null; i++)
            {
                Call(View, "Advance");
                yield return null;
            }
            yield return null;
            Assert.That(View, Is.Null);
            Assert.That(Locked, Is.False);
        }

        [UnityTest]
        public IEnumerator RewardExitShowsPortraitDialogueThenReturnsToMap()
        {
            yield return ClearStage(1);
            MonoBehaviour clear = Object.FindObjectOfType(Runtime("Scenes.GameClearController")) as MonoBehaviour;
            clear.StartCoroutine((IEnumerator)Call(clear, "ShowDialogueThenLeave", "MapScene"));
            yield return null;
            yield return null;
            Assert.That(View, Is.Not.Null);
            Assert.That(Locked, Is.True);
            Assert.That(View.transform.Find("ChatWindow/DialogueWindow/WordPortrait").GetComponent<UnityEngine.UI.Image>().sprite, Is.Not.Null);
            Assert.That(View.transform.Find("ChatWindow/DialogueWindow/OtherPortrait").GetComponent<UnityEngine.UI.Image>().sprite, Is.Not.Null);
            Call(View, "Advance"); // 첫 입력은 대사를 펼치고 다음 대사로 넘어가지 않는다.
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GameClearScene"));
            CapturePreview();
            yield return FinishDialogue();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MapScene"));
            Assert.That(PlayerPrefs.GetInt("ActOneDialogueSeen"), Is.EqualTo(2));
            yield return ClearStage(1);
            clear = Object.FindObjectOfType(Runtime("Scenes.GameClearController")) as MonoBehaviour;
            clear.StartCoroutine((IEnumerator)Call(clear, "ShowDialogueThenLeave", "MapScene"));
            yield return null;
            yield return null;
            Assert.That(View, Is.Null, "이미 읽은 대사는 재도전에서 반복하지 않는다.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MapScene"));
        }

        [UnityTest]
        public IEnumerator BossClearWaitsForConversationBeforeEnding()
        {
            yield return ClearStage(4);
            yield return null;
            Assert.That(View, Is.Not.Null);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GameClearScene"));
            yield return FinishDialogue();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("EndingScene"));
            Assert.That(PlayerPrefs.GetInt("ActOneDialogueSeen"), Is.EqualTo(16));
        }

        [UnityTest]
        public IEnumerator NewGameResetsDialogueHistory()
        {
            PlayerPrefs.SetInt("ActOneDialogueSeen", 31);
            Object save = Object.FindObjectOfType(Runtime("Core.SaveLoadController"));
            Call(save, "InitPlayData");
            Assert.That(PlayerPrefs.HasKey("ActOneDialogueSeen"), Is.False);
            yield return ClearStage(4);
            yield return null;
            Assert.That(View, Is.Not.Null);
            yield return FinishDialogue();
        }

        static void CapturePreview()
        {
            Camera camera = Camera.main;
            Canvas canvas = View.GetComponent<Canvas>();
            RenderTexture target = new RenderTexture(1920, 1080, 24);
            RenderTexture previous = RenderTexture.active;
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases();
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                string path = Path.Combine(Directory.GetCurrentDirectory(), "docs/design/act-one-dialogue-preview.png");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                RenderTexture.active = previous;
                Object.Destroy(image);
                Object.Destroy(target);
            }
        }
    }
}
#endif
