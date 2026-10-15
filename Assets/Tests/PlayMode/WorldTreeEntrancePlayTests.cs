#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class WorldTreeEntrancePlayTests
    {
        readonly List<GameObject> objects = new List<GameObject>();
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);
        static Type Entrance => Runtime("Combat.Enemies.WorldTreeEntrance");
        static bool Playing => (bool)Entrance.GetProperty("IsPlaying").GetValue(null);
        static float Progress => (float)Entrance.GetProperty("Progress").GetValue(null);
        static void Pause(bool value) => Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, value);

        Component Spawn()
        {
            var cameraObject = new GameObject("EntranceTestCamera") { tag = "MainCamera" };
            objects.Add(cameraObject);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.transform.position = new Vector3(0, 0, -10);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Enemies/Slime/SurgeBoss.prefab");
            GameObject instance = Object.Instantiate(prefab);
            objects.Add(instance);
            instance.transform.position = new Vector3(4, -2, 0);
            return instance.GetComponent(Runtime("Combat.Enemies.Enemy"));
        }

        static IEnumerator Begin(Component enemy)
        {
            var playback = (IEnumerator)Entrance.GetMethod("Play").Invoke(null, new object[] { enemy });
            Assert.That(playback.MoveNext(), Is.True);
            return playback;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Pause(false);
            Time.timeScale = 1;
            Entrance.GetMethod("Skip").Invoke(null, null);
            foreach (GameObject go in objects) if (go != null) Object.Destroy(go);
            objects.Clear();
            foreach (Object entrance in Object.FindObjectsOfType(Entrance)) Object.Destroy(((Component)entrance).gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator 스킵은_원래_가시성과_위치를_복원하고_다시_재생할_수_있다()
        {
            Component boss = Spawn();
            yield return null;
            Renderer[] renderers = boss.GetComponentsInChildren<Renderer>(true);
            bool[] enabled = renderers.Select(renderer => renderer.enabled).ToArray();
            Vector3 position = boss.transform.position;
            Vector3 scale = boss.transform.localScale;
            IEnumerator playback = Begin(boss);
            Assert.That(Playing, Is.True);
            Assert.That(renderers.All(renderer => !renderer.enabled), Is.True);
            Entrance.GetMethod("Skip").Invoke(null, null);
            yield return null;
            yield return null;
            Assert.That(Playing, Is.False);
            for (int i = 0; i < renderers.Length; i++) Assert.That(renderers[i].enabled, Is.EqualTo(enabled[i]));
            Assert.That(boss.transform.position, Is.EqualTo(position));
            Assert.That(boss.transform.localScale, Is.EqualTo(scale));
            Assert.That(playback.MoveNext(), Is.False);
            IEnumerator replay = Begin(boss);
            Assert.That(Playing, Is.True);
            ((IDisposable)replay).Dispose();
            Assert.That(Playing, Is.False, "부모 코루틴 중단도 즉시 복원한다");
        }

        [UnityTest]
        public IEnumerator 설정_일시정지_중_진행과_스킵을_멈추고_보스가_꺼지면_정리한다()
        {
            Component boss = Spawn();
            yield return null;
            IEnumerator playback = Begin(boss);
            Pause(true);
            float before = Progress;
            Entrance.GetMethod("Skip").Invoke(null, null);
            yield return null;
            yield return null;
            Assert.That(Playing, Is.True);
            Assert.That(Progress, Is.EqualTo(before));
            boss.gameObject.SetActive(false);
            yield return null;
            yield return null;
            Assert.That(Playing, Is.False);
            Assert.That(playback.MoveNext(), Is.False);
            Assert.That(boss.GetComponent<SpriteRenderer>().enabled, Is.True);
        }

        [Test]
        public void 비행은_먼_배경에서_출발해_마지막에_가속하고_정확히_착지한다()
        {
            Vector2 start = new Vector2(0, 250);
            Vector2 end = new Vector2(500, -200);
            Vector2 At(float t) => (Vector2)Entrance.GetMethod("EvaluateFlight").Invoke(null, new object[] { start, end, t });
            Assert.That(Vector2.Distance(At(0), start), Is.LessThan(.001f));
            Assert.That(Vector2.Distance(At(1), end), Is.LessThan(.001f));
            Assert.That(Vector2.Distance(At(.9f), At(1)), Is.GreaterThan(Vector2.Distance(At(.1f), At(.2f))));
        }

        [UnityTest]
        public IEnumerator 액체가_모인_화면_위치에서_반짝임을_시작한다()
        {
            Component boss = Spawn();
            yield return null;
            Vector2 requested = new Vector2(.22f, .31f);
            var playback = (IEnumerator)Entrance.GetMethod("PlayFrom").Invoke(null, new object[] { boss, requested });
            Assert.That(playback.MoveNext(), Is.True);
            var sparkle = GameObject.Find("DistantPinprick").GetComponent<RectTransform>();
            float width = 1080f * Screen.width / Mathf.Max(1, Screen.height);
            Assert.That(sparkle.anchoredPosition.x, Is.EqualTo((requested.x - .5f) * width).Within(.01f));
            Assert.That(sparkle.anchoredPosition.y, Is.EqualTo((requested.y - .5f) * 1080).Within(.01f));
            ((IDisposable)playback).Dispose();
            Assert.That(Playing, Is.False);
        }
    }

    // 명시적으로 선택했을 때만 실제 세계수 전장 위의 등장 연출을 PNG로 남긴다.
    [Explicit]
    public sealed class WorldTreeEntranceCaptures
    {
        readonly string[] keys = { "TutorialEnded", "StagePosition", "ActOneDialogueSeen", "ActTwoDialogueSeen", "SeenTips", "Language", "SeenSlimes0", "SeenSlimes1" };
        bool[] existed;
        int[] values;
        int oldPosition, oldStage;
        bool hadSingleton;
        int previousCaptureRate;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);
        static Type Entrance => Runtime("Combat.Enemies.WorldTreeEntrance");
        static bool Playing => (bool)Entrance.GetProperty("IsPlaying").GetValue(null);
        static float Progress => (float)Entrance.GetProperty("Progress").GetValue(null);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousCaptureRate = Time.captureFramerate;
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(key => PlayerPrefs.GetInt(key)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            Type stageType = Runtime("Combat.Stage.StageDataSingleton");
            Object singleton = Object.FindObjectOfType(stageType);
            hadSingleton = singleton != null;
            if (hadSingleton) oldStage = (int)stageType.GetField("stagePosition").GetValue(singleton);
            else singleton = new GameObject("EntranceCaptureStage").AddComponent(stageType);
            stageType.GetField("stagePosition").SetValue(singleton, 9);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 9);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.SetInt("ActOneDialogueSeen", int.MaxValue);
            PlayerPrefs.SetInt("ActTwoDialogueSeen", int.MaxValue);
            PlayerPrefs.SetInt("SeenTips", int.MaxValue);
            Runtime("Core.Localization").GetMethod("SetLanguage").Invoke(null,
                new[] { Enum.Parse(Runtime("Core.Language"), "Korean") });
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return null;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.captureFramerate = previousCaptureRate;
            Time.timeScale = 1;
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, false);
            Runtime("Core.InteractionLock").GetProperty("IsLocked").SetValue(null, false);
            yield return SceneManager.LoadSceneAsync("TitleScene");
            Type stageType = Runtime("Combat.Stage.StageDataSingleton");
            Component singleton = Object.FindObjectOfType(stageType) as Component;
            if (singleton != null)
            {
                if (hadSingleton) stageType.GetField("stagePosition").SetValue(singleton, oldStage);
                else Object.Destroy(singleton.gameObject);
            }
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
            // SetLanguage는 내부 캐시도 갱신한다. PlayerPrefs 유무까지 원래대로 되돌린다.
            int languageIndex = Array.IndexOf(keys, "Language");
            Runtime("Core.Localization").GetMethod("SetLanguage").Invoke(null,
                new[] { Enum.ToObject(Runtime("Core.Language"), values[languageIndex]) });
            if (!existed[languageIndex]) PlayerPrefs.DeleteKey("Language");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator 세계수_먼_반짝임부터_착지까지_실제_전장_캡처()
        {
            Type waveType = Runtime("Combat.Enemies.BattleWaveController");
            var wave = (MonoBehaviour)Object.FindObjectOfType(waveType);
            wave.StopAllCoroutines();
            foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")))
                enemy.gameObject.SetActive(false);
            GameObject shellObject = (GameObject)waveType.GetMethod("SpawnExtraEnemy").Invoke(null, new object[] { 35, 0f });
            Assert.That(shellObject, Is.Not.Null);
            shellObject.transform.position = new Vector3(0, -1.1f, 0);
            Component shell = shellObject.GetComponent(Runtime("Combat.Enemies.Enemy"));
            yield return null;
            Capture("00-worldtree-slime-phase-one.png");
            bool record = Environment.GetEnvironmentVariable("BOSS_CAPTURE_FRAMES") == "1";
            if (record) Time.captureFramerate = 30;
            int frameNumber = 0;
            float started = Time.realtimeSinceStartup;
            const float Deadline = 120;
            Type transition = Runtime("Combat.Enemies.WorldTreeTransition");
            bool TransitionPlaying() => (bool)transition.GetProperty("IsPlaying").GetValue(null);
            float FluidProgress() => (float)transition.GetProperty("FluidProgress").GetValue(null);
            void RecordFrame()
            {
                if (record) Capture("frames/" + (++frameNumber).ToString("D3") + ".png", 960, 540);
            }
            shell.GetType().GetMethod("Kill").Invoke(shell, null);
            float[] liquidMoments = { .04f, .25f, .55f, .90f };
            string[] liquidNames = { "01-fallen-shell", "02-gold-leaking", "03-gold-puddle", "04-core-congealing" };
            for (int i = 0; i < liquidMoments.Length; i++)
            {
                while (FluidProgress() < liquidMoments[i] && !Playing && Time.realtimeSinceStartup - started < Deadline)
                {
                    RecordFrame();
                    yield return null;
                }
                Assert.That(TransitionPlaying(), Is.True, "1페이즈 사망이 마력 전환을 시작해야 한다");
                Capture(liquidNames[i] + ".png");
            }
            while (!Playing && TransitionPlaying() && Time.realtimeSinceStartup - started < Deadline)
            {
                RecordFrame();
                yield return null;
            }
            Assert.That(Playing, Is.True, "모인 액체에서 2페이즈의 비행이 시작돼야 한다");
            float[] moments = { .20f, .275f, .50f, .62f, .85f };
            string[] names = { "05-tiny-core-pinprick", "06-core-flash", "07-core-flight", "08-core-landing", "09-core-name-and-memory" };
            for (int i = 0; i < moments.Length; i++)
            {
                while (Playing && Progress < moments[i] && Time.realtimeSinceStartup - started < Deadline)
                {
                    RecordFrame();
                    yield return null;
                }
                Assert.That(Playing, Is.True, "연출은 캡처 전에 자연 종료되면 안 된다");
                Capture(names[i] + ".png");
            }
            while ((Playing || TransitionPlaying()) && Time.realtimeSinceStartup - started < Deadline)
            {
                RecordFrame();
                yield return null;
            }
            Assert.That(Playing, Is.False);
            Assert.That(TransitionPlaying(), Is.False);
            Capture("10-core-battle-ready.png");
            Component core = Object.FindObjectsOfType(Runtime("Combat.Enemies.SurgeBossEnemy")).Cast<Component>().Single();
            Assert.That(core.GetComponent<SpriteRenderer>().enabled, Is.True);
            Assert.That((bool)Runtime("Combat.Enemies.Enemy").GetProperty("IsAlive").GetValue(core), Is.True);
        }

        [UnityTest]
        public IEnumerator 지역_보스_예고와_세계수_모방_상태_캡처()
        {
            Type waveType = Runtime("Combat.Enemies.BattleWaveController");
            MonoBehaviour wave = (MonoBehaviour)Object.FindObjectOfType(waveType);
            wave.StopAllCoroutines();
            object Spell(string name) => Enum.Parse(Runtime("Cards.MagicType"), name);
            object Call(object target, string method, params object[] args) => target.GetType().GetMethod(method,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
            Component Spawn(int id, float x) => ((GameObject)waveType.GetMethod("SpawnExtraEnemy")
                .Invoke(null, new object[] { id, x })).GetComponent(Runtime("Combat.Enemies.Enemy"));
            void Clear(string background)
            {
                foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy"))) enemy.gameObject.SetActive(false);
                GameObject.Find("PlainBackground").GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Battle/Backgrounds/ActTwo/" + background + ".png");
            }
            Clear("FrostVillage");
            Component boss = Spawn(30, 6);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Call(boss, "PlayTurnAction", 12f);
            Call(boss, "PlayTurnAction", 12f);
            yield return null;
            Capture("07-snowball-ready.png");
            Clear("ThunderCanyon");
            boss = Spawn(31, 6);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            yield return new WaitForSeconds(.15f);
            Capture("08-charge-windup.png");
            Call(boss, "PlayTurnAction", 12f);
            yield return new WaitForSeconds(.45f);
            Capture("09-charge-exhausted.png");
            Clear("AshenRuins");
            boss = Spawn(32, 6);
            yield return null;
            for (int i = 0; i < 3; i++) Call(boss, "TakeSpellHit", Spell("Rock"), Spell("Shoot"), 1f, 1f);
            yield return null;
            Capture("10-furnace-heat.png");
            Clear("TwistedForest");
            boss = Spawn(33, 7);
            Spawn(20, 5);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            yield return null;
            Capture("11-graft-mark.png");
            Call(boss, "PlayTurnAction", 12f);
            yield return null;
            Capture("12-graft-mortar.png");
            Clear("WorldTreeHeart");
            boss = Spawn(29, 6);
            yield return null;
            foreach (string spell in new[] { "Shoot", "Drop", "Explode" })
            {
                Call(boss, "Remember", Spell(spell));
                yield return null;
                Capture("13-worldtree-echo-" + spell.ToLowerInvariant() + ".png");
            }
        }

        static void Capture(string filename, int width = 1920, int height = 1080)
        {
            Camera camera = Camera.main;
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>().Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            // SDK의 개발용 입력 계기만 캡처에서 제외하고 즉시 복원한다.
            Canvas[] instruments = canvases.Where(canvas => canvas.name == "Artel Keyboard Status Canvas").ToArray();
            foreach (Canvas instrument in instruments) instrument.enabled = false;
            Camera[] previousCameras = canvases.Select(canvas => canvas.worldCamera).ToArray();
            float[] distances = canvases.Select(canvas => canvas.planeDistance).ToArray();
            var target = new RenderTexture(width, height, 24);
            RenderTexture oldTarget = camera.targetTexture, previous = RenderTexture.active;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
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
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                string directory = Path.Combine(Directory.GetCurrentDirectory(), ".plan/assets/2026-10-15-act-two-boss-design");
                Directory.CreateDirectory(directory);
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(directory, filename)));
                File.WriteAllBytes(Path.Combine(directory, filename), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = previous;
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceOverlay;
                    canvases[i].worldCamera = previousCameras[i];
                    canvases[i].planeDistance = distances[i];
                }
                foreach (Canvas instrument in instruments) instrument.enabled = true;
                Object.Destroy(image);
                target.Release();
                Object.Destroy(target);
            }
        }
    }
}
#endif
