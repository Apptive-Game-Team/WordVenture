#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
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
    // 실제 최종 웨이브에서 외피 사망과 본체 전환, 일시정지와 스킵을 확인한다.
    public sealed class TwoPhaseBossPlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        readonly string[] keys = { "TutorialEnded", "StagePosition", "SeenSlimes0", "SeenSlimes1", "SeenTips" };
        readonly List<GameObject> spawned = new List<GameObject>();
        int[] values;
        bool[] existed;
        int oldPosition;
        bool hadSingleton;
        int oldStage;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, AnyInstance).Invoke(target, args);
        static Component Player => (Component)Runtime("Combat.Enemies.Player").GetMethod("PlayerInt").Invoke(null, null);
        static int PlayerHp => (int)Runtime("Combat.Enemies.Player").GetField("Hp", AnyInstance).GetValue(Player);
        static object Property(Component target, string name) => target.GetType().GetProperty(name).GetValue(target);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            Object singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            hadSingleton = singleton != null;
            if (hadSingleton) oldStage = (int)Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").GetValue(singleton);
            else singleton = new GameObject("StageData").AddComponent(Runtime("Combat.Stage.StageDataSingleton"));
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.SetInt("SeenTips", int.MaxValue); // 안내 대사가 입력을 잡지 않게 한다.
            Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, 1);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, 1);
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(0.3f);
            // 실제 최종 웨이브를 앞선 전투 없이 시작한다.
            foreach (Object enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")))
                ((Component)enemy).gameObject.SetActive(false);
            Component wave = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            ((MonoBehaviour)wave).StopAllCoroutines();
            Object data = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/WorldTreeHeartBattleWaveData.asset");
            wave.GetType().GetField("battleScript").SetValue(wave, data);
            var waves = (IList)Call(data, "GetBattleWaveDatas");
            int last = waves.Count - 1;
            wave.GetType().GetField("wave", AnyInstance).SetValue(wave, last);
            ((IList)wave.GetType().GetField("activatedEnemies", AnyInstance).GetValue(wave)).Clear();
            Call(wave, "StartWave", last);
            yield return null;
            Assert.That((bool)Runtime("Combat.Enemies.WorldTreeEntrance").GetProperty("IsPlaying").GetValue(null), Is.True,
                "최종 웨이브는 거대 세계수 슬라임의 비행으로 시작한다");
            Assert.That(Shell.GetComponent<SpriteRenderer>().enabled, Is.False);
            Runtime("Combat.Enemies.WorldTreeEntrance").GetMethod("Skip").Invoke(null, null);
            yield return null;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, false);
            foreach (GameObject go in spawned) if (go != null) Object.Destroy(go);
            spawned.Clear();
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
        }

        static Type Transition => Runtime("Combat.Enemies.WorldTreeTransition");
        static bool TransitionPlaying => (bool)Transition.GetProperty("IsPlaying").GetValue(null);
        static Component[] Cores() => Object.FindObjectsOfType(Runtime("Combat.Enemies.SurgeBossEnemy")).Cast<Component>()
            .Where(e => (bool)Property(e, "IsAlive")).ToArray();
        static Component Shell => (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.TreeShellBossEnemy"));
        static void SkipTransition() => Transition.GetMethod("Skip").Invoke(null, null);

        [UnityTest]
        public IEnumerator 외피는_한턴_예고하고_뿌리로_앞줄을_공격한다()
        {
            yield return null;
            Assert.That(Shell, Is.Not.Null);
            int before = PlayerHp;
            int expectedDamage = (int)Runtime("Combat.Enemies.Enemy").GetField("Damage", AnyInstance).GetValue(Shell);
            Call(Shell, "PlayTurnAction", 10f);
            Assert.That(Property(Shell, "RootsReady"), Is.True);
            Assert.That(PlayerHp, Is.EqualTo(before));
            Call(Shell, "PlayTurnAction", 10f);
            Assert.That(PlayerHp, Is.EqualTo(before), "뿌리가 도착하기 전에는 피해가 없다");
            yield return new WaitForSeconds(.7f);
            Assert.That(PlayerHp, Is.EqualTo(before - expectedDamage));
            Assert.That(Property(Shell, "RootsReady"), Is.False);
        }

        [UnityTest]
        public IEnumerator 외피_사망은_본체를_정확히_한번_웨이브에_등록하고_미리_클리어하지_않는다()
        {
            Component shell = Shell;
            Assert.That(shell, Is.Not.Null);
            Call(shell, "Kill");
            Assert.That(TransitionPlaying, Is.True, "시체가 사라지기 전에 전환 잠금이 시작돼야 한다");
            yield return new WaitForSeconds(.4f);
            Assert.That(shell.gameObject.activeSelf, Is.False);
            Assert.That(Cores(), Is.Empty, "액체가 모이기 전에 본체를 전투에 노출하지 않는다");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TurnBattleScene"));
            SkipTransition();
            yield return null;
            yield return null;
            Assert.That(Cores().Length, Is.EqualTo(1));
            Assert.That(TransitionPlaying, Is.False, "건너뛰기는 사망과 응집 연출 전체를 끝낸다");
            Component core = Cores()[0];
            Assert.That(Property(core, "EnemyId"), Is.EqualTo(29));
            Component wave = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            var tracked = (IList)wave.GetType().GetField("activatedEnemies", AnyInstance).GetValue(wave);
            Assert.That(tracked.Contains(core.gameObject), Is.True);
            SkipTransition();
            yield return new WaitForSeconds(.15f);
            SkipTransition();
            yield return new WaitForSeconds(1.2f);
            Assert.That(Cores().Length, Is.EqualTo(1));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("TurnBattleScene"), "본체가 살아 있는데 최종 웨이브가 끝나면 안 된다");
            Assert.That(TransitionPlaying, Is.False);
        }

        [UnityTest]
        public IEnumerator 화상으로_외피가_죽어도_변신이_끝나기_전에는_새_턴을_시작하지_않는다()
        {
            Component shell = Shell;
            object Magic(string name) => Enum.Parse(Runtime("Cards.MagicType"), name);
            Call(shell, "TakeSpellHit", Magic("Fire"), Magic("Shoot"), 10f, 1f);
            Runtime("Combat.Enemies.Enemy").GetField("Hp", AnyInstance).SetValue(shell, 1);
            Component manager = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.EnemyTestManager"));
            Call(manager, "PlayTurn");
            Component battle = (Component)Object.FindObjectOfType(Runtime("Battle.Turns.TurnBattleSystem"));
            int turn = (int)Property(battle, "TurnNumber");
            Call(battle, "StartEnemyTurnCounter");
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That(TransitionPlaying, Is.True);
            Assert.That(Property(battle, "TurnNumber"), Is.EqualTo(turn));
            SkipTransition();
            yield return new WaitForSeconds(.2f);
            Assert.That(Property(battle, "TurnNumber"), Is.EqualTo(turn + 1));
        }

        [UnityTest]
        public IEnumerator 금빛_액체_전환은_일시정지_중_멈추고_건너뛰기로_본체를_중복_생성하지_않는다()
        {
            Component defeatedShell = Shell;
            Call(defeatedShell, "Kill");
            yield return new WaitForSeconds(.2f);
            Assert.That(defeatedShell.GetComponent<SpriteRenderer>().enabled, Is.False);
            float progress = (float)Transition.GetProperty("FluidProgress").GetValue(null);
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, true);
            Time.timeScale = 0;
            SkipTransition();
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That((float)Transition.GetProperty("FluidProgress").GetValue(null), Is.EqualTo(progress));
            Assert.That(Cores(), Is.Empty);
            Runtime("Core.InteractionLock").GetProperty("IsPaused").SetValue(null, false);
            Time.timeScale = 1;
            yield return null;
            SkipTransition();
            yield return null;
            yield return null;
            SkipTransition();
            yield return new WaitForSeconds(.2f);
            Assert.That(Cores().Length, Is.EqualTo(1));
            Assert.That(TransitionPlaying, Is.False);
            Assert.That((bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null), Is.False);
        }

        [UnityTest]
        public IEnumerator 웨이브_컨트롤러가_꺼지면_전환_플래그와_입력_잠금을_정리한다()
        {
            Call(Shell, "Kill");
            yield return null;
            Component wave = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            wave.gameObject.SetActive(false);
            yield return null;
            Assert.That(TransitionPlaying, Is.False);
            Assert.That((bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null), Is.False);
        }
        [UnityTest]
        public IEnumerator 본체는_죽은_거대_슬라임의_자리에서_나타나고_다시_날아오지_않는다()
        {
            float x = Shell.transform.position.x;
            Call(Shell, "Kill");
            yield return new WaitForSeconds(2f);
            Assert.That((bool)Runtime("Combat.Enemies.WorldTreeEntrance").GetProperty("IsPlaying").GetValue(null), Is.False);
            Assert.That(Cores().Length, Is.EqualTo(1));
            Component core = Cores()[0];
            Assert.That(core.transform.position.x, Is.EqualTo(x).Within(.01f));
            Assert.That(core.GetComponent<SpriteRenderer>().enabled, Is.False, "응집 중에는 연출용 본체만 보인다");
            yield return new WaitForSeconds(1f);
            Assert.That(TransitionPlaying, Is.False);
            Assert.That(core.GetComponent<SpriteRenderer>().enabled, Is.True);
            Assert.That((bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null), Is.False);
        }
    }
}
#endif
