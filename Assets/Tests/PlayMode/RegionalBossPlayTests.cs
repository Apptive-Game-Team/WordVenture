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
    // 지역 보스의 예고, 피해와 행동 순서를 실제 전투 씬 위에서 확인한다.
    public sealed class RegionalBossPlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const string SlimeFolder = "Assets/Prefabs/Combat/Enemies/Slime/";
        const string AffinityTablePath = "Assets/ScriptableObjects/Combat/Magic Affinity Table.asset";
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
        static object Magic(string name) => Enum.Parse(Runtime("Cards.MagicType"), name);
        static Type FormationType => Runtime("Combat.Allies.AllyFormation");
        static Component Player => (Component)Runtime("Combat.Enemies.Player").GetMethod("PlayerInt").Invoke(null, null);
        static int PlayerHp => (int)Runtime("Combat.Enemies.Player").GetField("Hp", AnyInstance).GetValue(Player);
        static int EnemyHp(Component enemy) => (int)Runtime("Combat.Enemies.Enemy").GetField("Hp", AnyInstance).GetValue(enemy);
        static int AllyHp(Component ally) => (int)ally.GetType().GetField("hp", AnyInstance).GetValue(ally);
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
            // 웨이브 적이 테스트 위치와 겹치지 않게 치운다.
            foreach (Object enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")))
                ((Component)enemy).gameObject.SetActive(false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
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

        Component CreateEnemy(string prefabName, float x, int maxHp, int damage, string element)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimeFolder + prefabName + ".prefab");
            GameObject slime = Object.Instantiate(prefab, new Vector3(x, -3f, 0f), Quaternion.identity);
            spawned.Add(slime);
            Component enemy = slime.GetComponent(Runtime("Combat.Enemies.Enemy"));
            object data = Activator.CreateInstance(Runtime("Combat.Enemies.EnemyData"));
            data.GetType().GetField("maxHp").SetValue(data, maxHp);
            data.GetType().GetField("moveDistance").SetValue(data, 5f);
            data.GetType().GetField("attackRange").SetValue(data, 3f);
            data.GetType().GetField("damage").SetValue(data, damage);
            data.GetType().GetField("type").SetValue(data, Magic(element));
            Call(enemy, "InitEnemyData", data);
            return enemy;
        }

        Component SpawnAlly(string element)
        {
            object formation = FormationType.GetMethod("GetOrCreate").Invoke(null, new object[] { Player.transform });
            Object affinity = AssetDatabase.LoadAssetAtPath<Object>(AffinityTablePath);
            return (Component)Call(formation, "Spawn", Magic(element), affinity);
        }

        static float FrontLineDistance(Component enemy) => enemy.transform.position.x
            - (float)FormationType.GetMethod("FrontLineX").Invoke(null, new object[] { Player.transform.position.x });

        static Component[] AliveEnemiesNamed(string prefabName) => Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy"))
            .Cast<Component>().Where(e => e.name.StartsWith(prefabName + "(") && (bool)Property(e, "IsAlive")).ToArray();

        [UnityTest]
        public IEnumerator 눈덩이는_3단계까지_자란_다음_턴에만_던지고_웨이브에_등록된다()
        {
            Component boss = CreateEnemy("SnowBoss", 6, 160, 24, "Ice");
            yield return null;
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", 10f);
            Component ball = (Component)Property(boss, "Snowball");
            Assert.That(ball, Is.Not.Null);
            spawned.Add(ball.gameObject);
            Assert.That(Property(ball, "Growth"), Is.EqualTo(1));
            Component wave = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            var tracked = (IList)wave.GetType().GetField("activatedEnemies", AnyInstance).GetValue(wave);
            Assert.That(tracked.Contains(ball.gameObject), Is.True);
            Call(boss, "PlayTurnAction", 10f);
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(ball, "Growth"), Is.EqualTo(3));
            Assert.That(PlayerHp, Is.EqualTo(before));
            Call(boss, "PlayTurnAction", 10f);
            yield return new WaitForSeconds(0.55f);
            Assert.That(ball.gameObject.activeSelf, Is.False);
            Assert.That(EnemyHp(boss), Is.EqualTo(160), "투척은 반격 피해를 발생시키지 않는다");
            Assert.That(PlayerHp, Is.EqualTo(before - 24));
        }

        [UnityTest]
        public IEnumerator 큰_눈덩이를_파괴하면_본체가_28_피해를_받고_본체_사망은_눈덩이를_정리한다()
        {
            Component boss = CreateEnemy("SnowBoss", 6, 160, 24, "Ice");
            yield return null;
            Call(boss, "PlayTurnAction", 10f);
            Component ball = (Component)Property(boss, "Snowball");
            spawned.Add(ball.gameObject);
            Call(boss, "PlayTurnAction", 10f);
            Call(boss, "PlayTurnAction", 10f);
            Call(ball, "Kill");
            Assert.That(EnemyHp(boss), Is.EqualTo(132));
            Call(boss, "PlayTurnAction", 10f);
            Component secondBall = (Component)Property(boss, "Snowball");
            spawned.Add(secondBall.gameObject);
            Call(boss, "Kill");
            Assert.That(secondBall.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator 돌진은_앞줄_한_대상만_때리고_다음_턴에_쉰다()
        {
            Component front = SpawnAlly("Fire");
            Component boss = CreateEnemy("ThunderBoss", front.transform.position.x + 5, 160, 20, "Lightning");
            yield return null;
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            Assert.That(Property(boss, "IsCharging"), Is.True);
            Assert.That(AllyHp(front), Is.GreaterThan(0));
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            yield return new WaitForSeconds(0.4f);
            Assert.That(AllyHp(front), Is.EqualTo(0));
            Assert.That(PlayerHp, Is.EqualTo(before), "막은 아군이 죽어도 워드까지 관통하지 않는다");
            Assert.That(Property(boss, "IsExhausted"), Is.True);
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            Assert.That(PlayerHp, Is.EqualTo(before));
            Assert.That(Property(boss, "IsExhausted"), Is.False);
        }

        [UnityTest]
        public IEnumerator 과열은_직접_타격만_3까지_쌓이고_공격하면_모두_소모된다()
        {
            Component boss = CreateEnemy("FurnaceBoss", 6, 200, 10, "Fire");
            yield return null;
            for (int i = 0; i < 4; i++) Call(boss, "TakeSpellHit", Magic("Fire"), Magic("Shoot"), 1f, 1f);
            Assert.That(Property(boss, "Heat"), Is.EqualTo(3));
            Call(boss, "TakeHit", 2);
            Assert.That(Property(boss, "Heat"), Is.EqualTo(3), "지속 피해는 직접 타격이 아니다");
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(PlayerHp, Is.EqualTo(before - 19));
            Assert.That(Property(boss, "Heat"), Is.EqualTo(0));
            Call(boss, "EndTurnStatuses");
            Assert.That(Property(boss, "Heat"), Is.EqualTo(0), "화상 피해는 열기를 올리지 않는다");
        }

        [UnityTest]
        public IEnumerator 과열_추가_피해도_보스_공격_약화를_받는다()
        {
            Component boss = CreateEnemy("FurnaceBoss", 6, 200, 10, "Fire");
            yield return null;
            Call(boss, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 1f, 1f);
            Call(boss, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 1f, 1f);
            Assert.That(Property(boss, "Heat"), Is.EqualTo(2));
            Assert.That(Property(boss, "NextAttackDamage"), Is.EqualTo(11));
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(PlayerHp, Is.EqualTo(before - 11));
        }

        [UnityTest]
        public IEnumerator 접목은_예고_흡수_기술_순서이고_흡수한_기술은_한번만_쓴다()
        {
            Component boss = CreateEnemy("GraftBoss", 6, 180, 10, "Undead");
            Component target = CreateEnemy("ChargeSlime", 5, 30, 10, "Lightning");
            Runtime("Combat.Enemies.Enemy").GetField("id", AnyInstance).SetValue(target, 21);
            yield return null;
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(boss, "MarkedEnemy"), Is.EqualTo(target));
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(target.gameObject.activeSelf, Is.False);
            Assert.That(Property(boss, "AbsorbedEnemyId"), Is.EqualTo(21));
            Assert.That(PlayerHp, Is.EqualTo(before));
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            yield return new WaitForSeconds(0.4f);
            Assert.That(PlayerHp, Is.EqualTo(before - 10));
            Assert.That(Property(boss, "AbsorbedEnemyId"), Is.EqualTo(-1));
        }

        [UnityTest]
        public IEnumerator 접목_대상을_먼저_죽이면_흡수하지_않고_회복도_하지_않는다()
        {
            Component boss = CreateEnemy("GraftBoss", 6, 180, 10, "Undead");
            Component target = CreateEnemy("MortarSlime", 5, 30, 10, "Lightning");
            Runtime("Combat.Enemies.Enemy").GetField("id", AnyInstance).SetValue(target, 20);
            yield return null;
            Call(boss, "TakeHit", 20);
            Call(boss, "PlayTurnAction", 10f);
            Call(target, "Kill");
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(boss, "AbsorbedEnemyId"), Is.EqualTo(-1));
            Assert.That(EnemyHp(boss), Is.EqualTo(160));
        }
        [UnityTest]
        public IEnumerator 진행중인_적턴_타이머는_세계수_등장_중_턴을_넘기지_않는다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6, 260, 15, "Undead");
            yield return null;
            Component battle = (Component)Object.FindObjectOfType(Runtime("Battle.Turns.TurnBattleSystem"));
            int before = (int)Property(battle, "TurnNumber");
            Call(battle, "StartEnemyTurnCounter");
            var entranceType = Runtime("Combat.Enemies.WorldTreeEntrance");
            var entrance = (IEnumerator)entranceType.GetMethod("Play").Invoke(null, new object[] { boss });
            ((MonoBehaviour)battle).StartCoroutine(entrance);
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That((bool)entranceType.GetProperty("IsPlaying").GetValue(null), Is.True);
            Assert.That(Property(battle, "TurnNumber"), Is.EqualTo(before), "기존 타이머도 등장 중에는 턴과 분노를 진행하지 않는다");
            entranceType.GetMethod("Skip").Invoke(null, null);
            yield return new WaitForSeconds(0.15f);
            Assert.That(Property(battle, "TurnNumber"), Is.EqualTo(before + 1), "건너뛰면 대기하던 턴을 한 번만 진행한다");
        }

        [UnityTest]
        public IEnumerator 진행중인_아군_연속_공격은_세계수_등장_동안_다음_공격을_보류한다()
        {
            SpawnAlly("Fire");
            SpawnAlly("Ice");
            Component boss = CreateEnemy("SurgeBoss", 6, 260, 15, "Undead");
            yield return null;
            Component battle = (Component)Object.FindObjectOfType(Runtime("Battle.Turns.TurnBattleSystem"));
            Component manager = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.EnemyTestManager"));
            object formation = FormationType.GetProperty("Current").GetValue(null);
            Type finderType = typeof(Func<>).MakeGenericType(Runtime("Combat.Enemies.Enemy"));
            Delegate findFront = Delegate.CreateDelegate(finderType, manager, manager.GetType().GetMethod("FindFrontEnemy"));
            var sequence = (IEnumerator)Call(formation, "AttackFrontEnemies", findFront);
            ((MonoBehaviour)battle).StartCoroutine(sequence);
            float attackDeadline = Time.time + 3f;
            while (EnemyHp(boss) == 260 && Time.time < attackDeadline) yield return null;
            int afterFirstAttack = EnemyHp(boss);
            Assert.That(afterFirstAttack, Is.LessThan(260));
            var entranceType = Runtime("Combat.Enemies.WorldTreeEntrance");
            var entrance = (IEnumerator)entranceType.GetMethod("Play").Invoke(null, new object[] { boss });
            ((MonoBehaviour)battle).StartCoroutine(entrance);
            yield return new WaitForSeconds(0.6f);
            Assert.That(EnemyHp(boss), Is.EqualTo(afterFirstAttack), "다음 아군은 등장 중인 보스를 때리지 않는다");
            entranceType.GetMethod("Skip").Invoke(null, null);
            float resumeDeadline = Time.time + 3f;
            while (EnemyHp(boss) == afterFirstAttack && Time.time < resumeDeadline) yield return null;
            Assert.That(EnemyHp(boss), Is.LessThan(afterFirstAttack), "등장이 끝나면 남은 아군 공격을 이어간다");
        }
    }
}
#endif
