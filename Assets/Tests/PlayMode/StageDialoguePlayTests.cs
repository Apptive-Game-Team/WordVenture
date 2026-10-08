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

        static void PlayStage(int stage)
        {
            var singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            if (singleton == null) singleton = new GameObject("StageData").AddComponent(Runtime("Combat.Stage.StageDataSingleton"));
            Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, stage);
        }

        static IEnumerator ClearStage(int stage)
        {
            PlayStage(stage);
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
            CapturePreview("act-one-dialogue-preview.png");
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

        static object Chapter(int stage, string moment, int wave = 0)
        {
            Object data = Resources.Load("Story/ActOneDialogues");
            object value = Enum.Parse(Runtime("Story.StageDialogueMoment"), moment);
            return Call(data, "FindChapter", stage, value, wave);
        }

        static object Get(object target, string name) => target.GetType().GetField(name).GetValue(target);

        [Test]
        public void EachRegionMeetsDifferentResidentAndFirstReturnsOnlyAtTheEnd()
        {
            object first = Chapter(0, "Clear");
            string[] names = Enumerable.Range(0, 4).Select(i => (string)Get(Chapter(i, "Clear"), "speakerName")).ToArray();
            Assert.That(names.Distinct().Count(), Is.EqualTo(4), "평원·해안·고원·빗길에서 서로 다른 주민을 만난다.");
            Sprite[] portraits = Enumerable.Range(0, 4).Select(i => (Sprite)Get(Chapter(i, "Clear"), "speakerPortrait")).ToArray();
            Assert.That(portraits.All(p => p != null) && portraits.Distinct().Count() == 4, Is.True);
            object ending = Chapter(4, "Clear");
            Assert.That(Get(ending, "speakerName"), Is.EqualTo(Get(first, "speakerName")));
            Assert.That(Get(ending, "speakerPortrait"), Is.SameAs(Get(first, "speakerPortrait")));
            Assert.That(Get(Chapter(4, "Wave", 2), "speakerName"), Is.EqualTo("슬라임 마왕"));
            Assert.That(Get(Chapter(4, "Wave", 3), "speakerName"), Is.EqualTo("언데드 슬라임 마왕"));
            Assert.That(Chapter(4, "Wave", 1), Is.Null);
        }

        [UnityTest]
        public IEnumerator BossStageEntryPlaysDialogueBeforeBattle()
        {
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 4);
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            Component map = Object.FindObjectOfType(Runtime("Map.MapMove")) as Component;
            map.GetType().GetField("position", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map, 4);
            Call(map, "SelectStage", 4);
            yield return null;
            yield return null;
            Assert.That(View, Is.Not.Null, "마왕 성에 들어가기 전에 대화를 보여 준다.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MapScene"));
            Call(View, "Advance");
            CapturePreview("act-one-castle-dialogue-preview.png");
            yield return FinishDialogue();
            for (int i = 0; i < 10 && SceneManager.GetActiveScene().name != "TurnBattleScene"; i++) yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TurnBattleScene"));
            Assert.That(PlayerPrefs.GetInt("ActOneDialogueSeen") & (1 << 9), Is.Not.Zero);
        }

        [UnityTest]
        public IEnumerator DemonKingTalksBeforeAppearingAndBeforeReviving()
        {
            PlayStage(4);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 4);
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return null;
            Component waves = Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController")) as Component;
            FieldInfo waveField = waves.GetType().GetField("wave", BindingFlags.Instance | BindingFlags.NonPublic);
            var enemies = (System.Collections.Generic.List<GameObject>)waves.GetType()
                .GetField("activatedEnemies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(waves);
            foreach (int wave in new[] { 2, 3 })
            {
                // 앞 wave 를 바로 끝내서 다음 wave 직전의 대화를 확인한다.
                waveField.SetValue(waves, wave - 1);
                foreach (GameObject enemy in enemies) enemy.SetActive(false);
                int spawned = enemies.Count;
                // wave 가 끝나면 1초 기다린 뒤 다음 wave 로 넘어간다.
                float deadline = Time.time + 5f;
                while (View == null && Time.time < deadline) yield return null;
                Assert.That(View, Is.Not.Null, "wave " + wave + " 앞에서 마왕이 말을 건다.");
                Assert.That(enemies.Count, Is.EqualTo(spawned), "대화가 끝나기 전에는 적이 나오지 않는다.");
                Assert.That(View.transform.Find("Backdrop"), Is.Null, "전투 화면을 가리지 않는다.");
                Call(View, "Advance");
                if (wave == 2) CapturePreview("act-one-boss-dialogue-preview.png");
                yield return FinishDialogue();
                yield return null;
                Assert.That(enemies.Count, Is.GreaterThan(spawned));
            }
            Assert.That(PlayerPrefs.GetInt("ActOneDialogueSeen") & (1 << 28 | 1 << 29), Is.EqualTo(1 << 28 | 1 << 29));
        }

        static void CapturePreview(string file)
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
                // 실제 화면은 Overlay 라 전투 카드보다 위에 그린다. 카메라로 찍을 때도 가장 위 sorting layer 에 둔다.
                canvas.planeDistance = camera.nearClipPlane + 0.01f;
                canvas.sortingLayerID = SortingLayer.layers[SortingLayer.layers.Length - 1].id;
                Canvas.ForceUpdateCanvases();
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                string path = Path.Combine(Directory.GetCurrentDirectory(), "docs/design", file);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                canvas.sortingLayerID = 0;
                RenderTexture.active = previous;
                Object.Destroy(image);
                Object.Destroy(target);
            }
        }
    }
}
#endif
