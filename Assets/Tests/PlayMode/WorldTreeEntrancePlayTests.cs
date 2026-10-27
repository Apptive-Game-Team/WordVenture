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
            if (Object.FindObjectOfType<AudioListener>() == null) cameraObject.AddComponent<AudioListener>();
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
        public IEnumerator 비행음은_다가올수록_커지고_일시정지와_스킵에_따라_멈춘다()
        {
            Component boss = Spawn();
            yield return null;
            IEnumerator playback = Begin(boss);
            Component entrance = (Component)Object.FindObjectOfType(Entrance);
            var sources = entrance.GetComponentsInChildren<AudioSource>();
            Assert.That(sources.Length, Is.EqualTo(4));
            foreach (var source in sources)
            {
                Assert.That(source.clip, Is.Not.Null);
                Assert.That(source.clip.channels, Is.EqualTo(1));
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.ignoreListenerVolume, Is.False);
            }
            AudioSource approach = sources.Single(s => s.name == "Approach");
            yield return new WaitForSeconds(1.3f);
            Assert.That(approach.isPlaying, Is.True);
            float distantVolume = approach.volume;
            yield return new WaitForSeconds(.3f);
            Assert.That(approach.volume, Is.GreaterThan(distantVolume));
            Pause(true);
            yield return null;
            yield return null;
            int sample = approach.timeSamples;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(approach.timeSamples, Is.EqualTo(sample));
            Pause(false);
            yield return null; // 메뉴를 닫은 프레임은 InteractionLock이 입력을 막는다.
            Entrance.GetMethod("Skip").Invoke(null, null);
            yield return null;
            yield return null;
            Assert.That(Playing, Is.False);
            Assert.That(Object.FindObjectsOfType<AudioSource>().Any(s => s.transform.parent != null && s.transform.parent.name == "WorldTreeCinematicAudio"), Is.False);
            ((IDisposable)playback).Dispose();
        }

        [UnityTest]
        public IEnumerator 프레임이_늦어_착지로_넘어가면_지난_반짝임과_비행음은_몰아서_재생하지_않는다()
        {
            var host = new GameObject("AudioTest");
            objects.Add(host);
            object audio = Activator.CreateInstance(Runtime("Combat.Enemies.WorldTreeCinematicAudio"), new object[] { host.transform });
            audio.GetType().GetMethod("Entrance").Invoke(audio, new object[] { 2.2f, .5f });
            var sources = host.GetComponentsInChildren<AudioSource>();
            Assert.That(sources.Single(s => s.name == "Flash").isPlaying, Is.False);
            Assert.That(sources.Single(s => s.name == "Approach").isPlaying, Is.False);
            Assert.That(sources.Single(s => s.name == "Landing").isPlaying, Is.True);
            audio.GetType().GetMethod("Stop").Invoke(audio, null);
            yield return null;
            Assert.That(host.GetComponentsInChildren<AudioSource>(), Is.Empty);
        }

        [UnityTest]
        public IEnumerator 본체_응집음과_등장음은_시간배율과_취소를_따른다()
        {
            var host = new GameObject("CoreAudioTest");
            objects.Add(host);
            object audio = Activator.CreateInstance(Runtime("Combat.Enemies.WorldTreeCinematicAudio"), new object[] { host.transform });
            audio.GetType().GetMethod("Transition").Invoke(audio, new object[] { 1.1f, .5f });
            var sources = host.GetComponentsInChildren<AudioSource>();
            Assert.That(sources.Single(s => s.name == "Landing").isPlaying, Is.False, "지나간 외피 붕괴음을 다시 내지 않는다");
            Assert.That(sources.Single(s => s.name == "CoreRise").isPlaying, Is.True);
            audio.GetType().GetMethod("Transition").Invoke(audio, new object[] { 1.75f, .5f });
            Assert.That(sources.Single(s => s.name == "Flash").isPlaying, Is.True);
            Time.timeScale = .5f;
            audio.GetType().GetMethod("SetPaused").Invoke(audio, new object[] { false });
            Assert.That(sources.All(s => Mathf.Abs(s.pitch - .5f) < .001f), Is.True);
            audio.GetType().GetMethod("Stop").Invoke(audio, null);
            yield return null;
            Assert.That(host.GetComponentsInChildren<AudioSource>(), Is.Empty);
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
            GameObject shellObject = (GameObject)waveType.GetMethod("SpawnExtraEnemy").Invoke(null, new object[] { 35, 2.5f });
            Component shell = shellObject.GetComponent(Runtime("Combat.Enemies.Enemy"));
            yield return null;
            bool record = Environment.GetEnvironmentVariable("BOSS_CAPTURE_FRAMES") == "1";
            if (record) Time.captureFramerate = 30;
            int frameNumber = 0;
            float started = Time.realtimeSinceStartup;
            const float Deadline = 120;
            void RecordFrame()
            {
                if (record) Capture("frames/" + (++frameNumber).ToString("D3") + ".png", 960, 540);
            }
            wave.StartCoroutine((IEnumerator)Entrance.GetMethod("Play").Invoke(null, new object[] { shell }));
            float[] moments = { .18f, .275f, .50f, .62f, .85f };
            string[] names = { "01-distant-tree-pinprick", "02-tree-flash", "03-giant-tree-flight", "04-giant-tree-landing", "05-giant-tree-name" };
            for (int i = 0; i < moments.Length; i++)
            {
                while (Playing && Progress < moments[i] && Time.realtimeSinceStartup - started < Deadline)
                { RecordFrame(); yield return null; }
                Assert.That(Playing, Is.True);
                Capture(names[i] + ".png");
            }
            while (Playing && Time.realtimeSinceStartup - started < Deadline) { RecordFrame(); yield return null; }
            Capture("06-phase-one-battle.png");
            // 첫 형태가 행동한 뒤 정상 사망 경로로 변신시킨다.
            shell.GetType().GetMethod("PlayTurnAction").Invoke(shell, new object[] { 12f });
            shell.GetType().GetMethod("Kill").Invoke(shell, null);
            Type transition = Runtime("Combat.Enemies.WorldTreeTransition");
            bool TransitionPlaying() => (bool)transition.GetProperty("IsPlaying").GetValue(null);
            float FluidProgress() => (float)transition.GetProperty("FluidProgress").GetValue(null);
            float[] liquidMoments = { .12f, .35f, .55f, .72f, .92f };
            string[] liquidNames = { "07-tree-breaks", "08-gold-leaks-at-roots", "09-gold-gathers", "10-core-emerges-here", "11-core-takes-shape" };
            for (int i = 0; i < liquidMoments.Length; i++)
            {
                while (TransitionPlaying() && FluidProgress() < liquidMoments[i] && Time.realtimeSinceStartup - started < Deadline)
                { RecordFrame(); yield return null; }
                Assert.That(Playing, Is.False, "본체는 다시 날아오면 안 된다");
                Capture(liquidNames[i] + ".png");
            }
            while (TransitionPlaying() && Time.realtimeSinceStartup - started < Deadline) { RecordFrame(); yield return null; }
            Assert.That(TransitionPlaying(), Is.False);
            Capture("12-core-battle-here.png");
            Component core = Object.FindObjectsOfType(Runtime("Combat.Enemies.SurgeBossEnemy")).Cast<Component>().Single();
            Assert.That(core.GetComponent<SpriteRenderer>().enabled, Is.True);
            Assert.That(core.transform.position.x, Is.EqualTo(2.5f).Within(.01f));
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
            yield return new WaitForSeconds(.4f);
            Capture("10-furnace-heat.png");
            Clear("TwistedForest");
            boss = Spawn(33, 7);
            Spawn(20, 5);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            yield return null;
            Capture("11-graft-mark.png");
            Call(boss, "PlayTurnAction", 12f);
            yield return new WaitForSeconds(1.15f);
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

        [UnityTest]
        public IEnumerator 아이콘의_다음_행동_설명은_마우스_정보창에서만_보인다()
        {
            var wave = (MonoBehaviour)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            wave.StopAllCoroutines();
            foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy"))) enemy.gameObject.SetActive(false);
            Component boss = ((GameObject)Runtime("Combat.Enemies.BattleWaveController").GetMethod("SpawnExtraEnemy")
                .Invoke(null, new object[] { 32, 6f })).GetComponent(Runtime("Combat.Enemies.Enemy"));
            yield return null;
            object Magic(string name) => Enum.Parse(Runtime("Cards.MagicType"), name);
            for (int i = 0; i < 3; i++) boss.GetType().GetMethod("TakeSpellHit").Invoke(boss,
                new[] { Magic("Rock"), Magic("Shoot"), (object)1f, 1f });
            yield return new WaitForSeconds(.4f);
            Component hover = (Component)Runtime("Combat.UI.EnemyHoverInfo").GetMethod("EnsureExists").Invoke(null, null);
            hover.GetType().GetMethod("Show").Invoke(hover, new object[] { boss });
            yield return null;
            Capture("14-intent-hover-korean.png");
            Runtime("Core.Localization").GetMethod("SetLanguage").Invoke(null, new[] { Enum.Parse(Runtime("Core.Language"), "English") });
            yield return null;
            Capture("15-intent-hover-english.png");
        }

        [UnityTest]
        public IEnumerator 개선된_열기_방출과_3턴_모방과_후퇴_도약_폭발_캡처()
        {
            var wave = (MonoBehaviour)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            wave.StopAllCoroutines();
            Component Spawn(int id, float x) => ((GameObject)Runtime("Combat.Enemies.BattleWaveController").GetMethod("SpawnExtraEnemy")
                .Invoke(null, new object[] { id, x })).GetComponent(Runtime("Combat.Enemies.Enemy"));
            object Magic(string name) => Enum.Parse(Runtime("Cards.MagicType"), name);
            void Call(object target, string name, params object[] args) => target.GetType().GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);
            void Clear()
            {
                foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy"))) enemy.gameObject.SetActive(false);
            }
            int effectFrame = 0;
            void EffectFrame() => Capture("effect-frames/" + (++effectFrame).ToString("D3") + ".png", 960, 540);
            Time.captureFramerate = 30;
            Clear();
            Component furnace = Spawn(32, 6);
            yield return null;
            for (int i = 0; i < 3; i++) Call(furnace, "TakeSpellHit", Magic("Rock"), Magic("Shoot"), 1f, 1f);
            yield return new WaitForSeconds(.4f);
            Call(furnace, "PlayTurnAction", 12f);
            for (int frame = 0; frame < 40; frame++)
            {
                EffectFrame();
                if (frame == 5) Capture("16-furnace-gather.png");
                if (frame == 17) Capture("17-furnace-wave.png");
                if (frame == 27) Capture("18-furnace-impact.png");
                yield return null;
            }
            Clear();
            Component core = Spawn(29, 6);
            yield return null;
            Call(core, "RememberElement", Magic("Drop"), Magic("Fire")); Call(core, "PlayTurnAction", 12f);
            Call(core, "RememberElement", Magic("Shoot"), Magic("Ice")); Call(core, "PlayTurnAction", 12f);
            Call(core, "RememberElement", Magic("Explode"), Magic("Lightning"));
            yield return null; yield return null;
            Capture("19-three-stored-spells.png");
            Component hover = (Component)Runtime("Combat.UI.EnemyHoverInfo").GetMethod("EnsureExists").Invoke(null, null);
            Component[] icons = core.GetComponentsInChildren(Runtime("Combat.Enemies.BossIntent"));
            Call(hover, "ShowIntent", icons[1]);
            yield return null;
            Capture("26-single-spell-tooltip.png");
            Call(hover, "Hide");
            yield return null;
            int oldRate = Time.captureFramerate;
            Time.captureFramerate = 30;
            try
            {
                Call(core, "PlayTurnAction", 12f);
                for (int frame = 0; frame < 140; frame++)
                {
                    Capture("spell-frames/" + frame.ToString("D3") + ".png", 960, 540);
                    EffectFrame();
                    if (frame == 20) Capture("20-mimic-fire-drop.png");
                    if (frame == 58) Capture("21-mimic-ice-shoot.png");
                    if (frame == 124) Capture("22-mimic-lightning-explode.png");
                    yield return null;
                }
            }
            finally { Time.captureFramerate = oldRate; }
            Clear();
            Component thunder = Spawn(31, 6);
            yield return null;
            Call(thunder, "PlayTurnAction", 10f); Call(thunder, "PlayTurnAction", 10f);
            yield return new WaitForSeconds(.4f);
            Call(thunder, "PlayTurnAction", 10f);
            Call(thunder, "PlayTurnAction", 10f);
            for (int frame = 0; frame < 23; frame++)
            {
                EffectFrame();
                if (frame == 6) Capture("23-thunder-back-jump.png");
                if (frame == 15) Capture("24-thunder-landing-windup.png");
                yield return null;
            }
            Clear();
            Component bomb;
            GameObject bombObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Enemies/Slime/ExplodeSlime.prefab"), new Vector3(3, -3, 0), Quaternion.identity);
            bomb = bombObject.GetComponent(Runtime("Combat.Enemies.Enemy"));
            object data = Activator.CreateInstance(Runtime("Combat.Enemies.EnemyData"));
            data.GetType().GetField("maxHp").SetValue(data, 25);
            data.GetType().GetField("damage").SetValue(data, 1);
            data.GetType().GetField("attackRange").SetValue(data, 3f);
            Call(bomb, "InitEnemyData", data);
            yield return null;
            Call(bomb, "PlayTurnAction", 2f);
            yield return new WaitForSeconds(.2f);
            Call(bomb, "PlayTurnAction", 2f);
            for (int frame = 0; frame < 22; frame++)
            {
                EffectFrame();
                if (frame == 4) Capture("25-bomb-body-centered.png");
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator 접목_대상이_보스_몸으로_끌려가는_흡수_연출_캡처()
        {
            var wave = (MonoBehaviour)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            wave.StopAllCoroutines();
            foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy"))) enemy.gameObject.SetActive(false);
            GameObject.Find("PlainBackground").GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Battle/Backgrounds/ActTwo/TwistedForest.png");
            Component Spawn(int id, float x) => ((GameObject)Runtime("Combat.Enemies.BattleWaveController").GetMethod("SpawnExtraEnemy")
                .Invoke(null, new object[] { id, x })).GetComponent(Runtime("Combat.Enemies.Enemy"));
            void Act(Component target) => target.GetType().GetMethod("PlayTurnAction").Invoke(target, new object[] { 12f });
            Component boss = Spawn(33, 7);
            Spawn(20, 2);
            yield return null;
            Act(boss);
            yield return null;
            Capture("27-graft-mark-before-pull.png");
            Time.captureFramerate = 30;
            Act(boss);
            for (int frame = 0; frame < 43; frame++)
            {
                Capture("graft-frames/" + frame.ToString("D3") + ".png", 960, 540);
                if (frame == 13) Capture("28-graft-pull.png");
                if (frame == 27) Capture("29-graft-merge.png");
                if (frame == 39) Capture("30-graft-technique-ready.png");
                yield return null;
            }
        }

        [UnityTest, Explicit("실제 전투 화면 캡처"), Category("WorldTreeEntranceCaptures")]
        public IEnumerator 소환_아군_흡수와_원거리_뿌리_근접_접근_캡처()
        {
            var wave = (MonoBehaviour)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            wave.StopAllCoroutines();
            foreach (Component enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy"))) enemy.gameObject.SetActive(false);
            GameObject.Find("PlainBackground").GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Battle/Backgrounds/ActTwo/TwistedForest.png");
            Component Spawn(int id, float x) => ((GameObject)Runtime("Combat.Enemies.BattleWaveController").GetMethod("SpawnExtraEnemy")
                .Invoke(null, new object[] { id, x })).GetComponent(Runtime("Combat.Enemies.Enemy"));
            void Act(Component target, float distance = 12) => target.GetType().GetMethod("PlayTurnAction").Invoke(target, new object[] { distance });
            Component player = (Component)Runtime("Combat.Enemies.Player").GetMethod("PlayerInt").Invoke(null, null);
            object formation = Runtime("Combat.Allies.AllyFormation").GetMethod("GetOrCreate").Invoke(null, new object[] { player.transform });
            object fire = Enum.Parse(Runtime("Cards.MagicType"), "Fire");
            object ice = Enum.Parse(Runtime("Cards.MagicType"), "Ice");
            var affinity = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/Magic Affinity Table.asset");
            formation.GetType().GetMethod("Spawn").Invoke(formation, new[] { fire, affinity });
            formation.GetType().GetMethod("Spawn").Invoke(formation, new[] { ice, affinity });
            Component boss = Spawn(33, 7);
            yield return null;
            Act(boss);
            yield return null;
            Capture("31-graft-ally-marked.png");
            Act(boss);
            Time.captureFramerate = 30;
            for (int frame = 0; frame < 42; frame++)
            {
                Capture("ally-graft-frames/" + frame.ToString("D3") + ".png", 960, 540);
                if (frame == 17) Capture("32-graft-ally-pull.png");
                if (frame == 39) Capture("33-graft-ally-magic-ready.png");
                yield return null;
            }
            Act(boss);
            for (int frame = 0; frame < 26; frame++)
            {
                Capture("ally-graft-frames/" + (frame + 42).ToString("D3") + ".png", 960, 540);
                if (frame == 12) Capture("34-graft-ally-magic.png");
                yield return null;
            }
            boss.gameObject.SetActive(false);
            Object.Destroy(((Component)formation).gameObject);
            yield return null;
            Component shell = Spawn(35, 5);
            yield return null;
            Act(shell); Act(shell);
            for (int frame = 0; frame < 28; frame++)
            {
                Capture("root-frames/" + frame.ToString("D3") + ".png", 960, 540);
                if (frame == 13) Capture("35-worldtree-roots.png");
                yield return null;
            }
            shell.gameObject.SetActive(false);
            Component melee = Spawn(17, player.transform.position.x + 3);
            yield return null;
            Capture("36-melee-before-approach.png");
            Act(melee, 3);
            yield return new WaitForSeconds(1.15f);
            Act(melee, 1.2f);
            yield return null;
            Capture("37-melee-contact.png");
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
                string directory = Path.Combine(Directory.GetCurrentDirectory(), ".plan/assets/2026-10-16-boss-combat-feedback");
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
