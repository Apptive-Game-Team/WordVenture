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
            // 투사체를 기다리는 동안 새 웨이브가 끼어들지 않게 고정한다.
            ((MonoBehaviour)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"))).StopAllCoroutines();
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
            Assert.That(Property(boss, "IsExhausted"), Is.True);
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
            Assert.That(PlayerHp, Is.EqualTo(before), "파동이 도착하기 전에는 피해를 주지 않는다");
            yield return new WaitForSeconds(1.1f);
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
            yield return new WaitForSeconds(1.1f);
            Assert.That(PlayerHp, Is.EqualTo(before - 11));
        }

        [UnityTest]
        public IEnumerator 접목은_소환_아군을_예고_흡수하고_대열을_정리한_뒤_그_속성으로_공격한다()
        {
            Component front = SpawnAlly("Fire");
            Component behind = SpawnAlly("Rock");
            int before = AllyHp(behind);
            float oldFrontX = front.transform.position.x;
            Component boss = CreateEnemy("GraftBoss", 6, 180, 10, "Rock");
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Assert.That(Property(boss, "MarkedAlly"), Is.EqualTo(front));
            Call(boss, "PlayTurnAction", 12f);
            Assert.That(Property(front, "IsAlive"), Is.False);
            Assert.That(Property(boss, "AbsorbedElement").ToString(), Is.EqualTo("Fire"));
            Assert.That(Property(FormationType.GetProperty("Current").GetValue(null) as Component, "Front"), Is.EqualTo(behind));
            yield return new WaitForSeconds(1.15f);
            Assert.That(behind.transform.position.x, Is.EqualTo(oldFrontX).Within(.02f));
            Assert.That(Property(Intent(boss), "Icon").ToString(), Is.EqualTo("Shoot"));
            Call(boss, "PlayTurnAction", 12f);
            Assert.That(AllyHp(behind), Is.EqualTo(before), "속성 탄이 닿기 전에 피해를 주지 않는다");
            Assert.That(Object.FindObjectsOfType(Runtime("Combat.Enemies.EnemyMagicVisual")), Is.Not.Empty);
            yield return new WaitForSeconds(.7f);
            Assert.That(AllyHp(behind), Is.LessThan(before));
            Assert.That(behind.GetComponent(Runtime("Combat.Allies.FriendlyElementalStatus")), Is.Not.Null);
            Assert.That(Property(boss, "AbsorbedElement"), Is.Null);
        }

        [UnityTest]
        public IEnumerator 지목된_소환_아군이_죽으면_남은_아군을_바로_흡수하지_않고_다시_예고한다()
        {
            Component front = SpawnAlly("Ice");
            Component behind = SpawnAlly("Holy");
            Component boss = CreateEnemy("GraftBoss", 6, 180, 10, "Rock");
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Call(front, "TakeSpellDamage", 9999);
            yield return new WaitForSeconds(.3f);
            Call(boss, "PlayTurnAction", 12f);
            Assert.That(Property(boss, "MarkedAlly"), Is.EqualTo(behind));
            Assert.That(Property(boss, "AbsorbedElement"), Is.Null);
            Assert.That(Property(boss, "IsAbsorbing"), Is.False);
            Assert.That(Property(behind, "IsAlive"), Is.True);
        }

        [UnityTest]
        public IEnumerator 근접_실제_데이터는_거리3에서_접근하고_붙은_다음_턴에_공격한다()
        {
            GameObject instance = (GameObject)Runtime("Combat.Enemies.BattleWaveController").GetMethod("SpawnExtraEnemy").Invoke(null, new object[] { 17, Player.transform.position.x + 3f });
            Component enemy = instance.GetComponent(Runtime("Combat.Enemies.Enemy"));
            yield return null;
            int before = PlayerHp;
            Call(enemy, "PlayTurnAction", 3f);
            Assert.That(PlayerHp, Is.EqualTo(before));
            yield return new WaitForSeconds(1.15f);
            Assert.That(FrontLineDistance(enemy), Is.EqualTo(1.2f).Within(.02f));
            Assert.That(PlayerHp, Is.EqualTo(before));
            Call(enemy, "PlayTurnAction", FrontLineDistance(enemy));
            Assert.That(PlayerHp, Is.LessThan(before));
        }

        [UnityTest]
        public IEnumerator 원거리_실제_데이터는_멀리서_탄을_쏘고_탄이_닿을_때_피해를_준다()
        {
            GameObject instance = (GameObject)Runtime("Combat.Enemies.BattleWaveController").GetMethod("SpawnExtraEnemy").Invoke(null, new object[] { 16, Player.transform.position.x + 6f });
            Component enemy = instance.GetComponent(Runtime("Combat.Enemies.Enemy"));
            yield return null;
            int before = PlayerHp;
            Call(enemy, "PlayTurnAction", 6f);
            Assert.That(PlayerHp, Is.EqualTo(before));
            Assert.That(Object.FindObjectsOfType(Runtime("Combat.Enemies.EnemyProjectile")), Is.Not.Empty);
            yield return new WaitForSeconds(.85f);
            Assert.That(PlayerHp, Is.LessThan(before));
        }

        [UnityTest]
        public IEnumerator 뿌리를_뻗던_접목이_죽으면_원거리_피해를_취소한다()
        {
            Component boss = CreateEnemy("GraftBoss", 6, 180, 10, "Rock");
            yield return null;
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", 12f);
            yield return new WaitForSeconds(.2f);
            Call(boss, "Kill");
            yield return new WaitForSeconds(.7f);
            Assert.That(PlayerHp, Is.EqualTo(before));
            Assert.That(GameObject.Find("AdvancingRoots"), Is.Null);
        }

        [UnityTest]
        public IEnumerator 접목_대상이_없으면_뿌리가_앞줄에_닿아야_피해를_준다()
        {
            Component boss = CreateEnemy("GraftBoss", 6, 180, 10, "Rock");
            yield return null;
            int before = PlayerHp;
            Call(boss, "PlayTurnAction", 12f);
            Assert.That(PlayerHp, Is.EqualTo(before));
            Assert.That(GameObject.Find("AdvancingRoots"), Is.Not.Null);
            yield return new WaitForSeconds(.7f);
            Assert.That(PlayerHp, Is.EqualTo(before - 10));
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
            yield return new WaitForSeconds(1.15f);
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
        public IEnumerator 접목_대상의_모습이_보스로_끌려가_작아진_뒤_기술_예고로_바뀐다()
        {
            Component boss = CreateEnemy("GraftBoss", 7, 180, 10, "Undead");
            Component target = CreateEnemy("MortarSlime", 2, 30, 10, "Rock");
            Runtime("Combat.Enemies.Enemy").GetField("id", AnyInstance).SetValue(target, 20);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Call(boss, "PlayTurnAction", 12f);
            Assert.That(Property(boss, "IsAbsorbing"), Is.True);
            Component effect = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.GraftAbsorptionVfx"));
            Transform clone = (Transform)Property(effect, "AbsorbedBody");
            float startX = clone.position.x, scale = Mathf.Abs(clone.lossyScale.x);
            Assert.That(target.gameObject.activeSelf, Is.False, "사본은 중복 공격하거나 웨이브 적으로 남지 않는다");
            Assert.That(Property(Intent(boss), "Icon").ToString(), Is.EqualTo("Absorb"));
            yield return new WaitForSeconds(.45f);
            Assert.That(clone.position.x, Is.GreaterThan(startX + .2f));
            Assert.That(Mathf.Abs(clone.lossyScale.x), Is.LessThan(scale));
            yield return new WaitForSeconds(.75f);
            Assert.That(Property(boss, "IsAbsorbing"), Is.False);
            Assert.That(effect == null, Is.True);
            Assert.That(Property(Intent(boss), "Icon").ToString(), Is.EqualTo("Drop"));
            Assert.That(Property(boss, "HasMarker"), Is.True);
        }

        [UnityTest]
        public IEnumerator 접목_흡수를_기다리는_동안_턴이_넘어가지_않고_사망하면_사본을_정리한다()
        {
            Component boss = CreateEnemy("GraftBoss", 7, 180, 10, "Undead");
            Component target = CreateEnemy("ChargeSlime", 2, 30, 10, "Lightning");
            Runtime("Combat.Enemies.Enemy").GetField("id", AnyInstance).SetValue(target, 21);
            yield return null;
            Component battle = (Component)Object.FindObjectOfType(Runtime("Battle.Turns.TurnBattleSystem"));
            int turn = (int)Property(battle, "TurnNumber");
            try
            {
                Time.timeScale = .5f;
                Call(boss, "PlayTurnAction", 12f); Call(boss, "PlayTurnAction", 12f);
                Call(battle, "StartEnemyTurnCounter");
                yield return new WaitForSecondsRealtime(1.2f);
                Assert.That(Property(boss, "IsAbsorbing"), Is.True);
                Assert.That(Property(battle, "TurnNumber"), Is.EqualTo(turn));
                Call(boss, "Kill");
                yield return null;
                Assert.That(Object.FindObjectOfType(Runtime("Combat.Enemies.GraftAbsorptionVfx")), Is.Null);
                Assert.That(Runtime("Combat.Enemies.GraftBossEnemy").GetProperty("IsAbsorbingAny").GetValue(null), Is.False);
            }
            finally { Time.timeScale = 1; }
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

        static Component Intent(Component enemy) => enemy.GetComponentInChildren(Runtime("Combat.Enemies.BossIntent"));

        [UnityTest]
        public IEnumerator 눈덩이_예고는_글자_없이_성장과_투척_단계를_표시하고_파괴하면_초기화된다()
        {
            Component boss = CreateEnemy("SnowBoss", 6, 160, 24, "Ice");
            yield return null;
            Component icon = Intent(boss);
            Assert.That(Property(icon, "Icon").ToString(), Is.EqualTo("Snowball"));
            Assert.That(icon.GetComponentInChildren(Runtime("TMPro.TMP_Text")), Is.Null);
            Assert.That(Property(icon, "Level"), Is.EqualTo(0));
            for (int i = 0; i < 3; i++) Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(icon, "Level"), Is.EqualTo(3));
            Assert.That(Property(icon, "IsImminent"), Is.True);
            Component ball = (Component)Property(boss, "Snowball");
            Call(ball, "Kill");
            yield return null;
            yield return null;
            Assert.That(Property(icon, "Level"), Is.EqualTo(0));
            Assert.That(Property(icon, "IsImminent"), Is.False);
        }

        [UnityTest]
        public IEnumerator 돌진_탈진과_과열의_아이콘은_행동_단계와_함께_바뀐다()
        {
            Component boss = CreateEnemy("ThunderBoss", 6, 180, 20, "Lightning");
            yield return null;
            Component icon = Intent(boss);
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(icon, "Icon").ToString(), Is.EqualTo("Charge"));
            Assert.That(Property(icon, "IsImminent"), Is.True);
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(icon, "Icon").ToString(), Is.EqualTo("Exhaust"));
            yield return new WaitForSeconds(.35f);
            Call(boss, "PlayTurnAction", 10f);
            Assert.That(Property(icon, "Icon").ToString(), Is.EqualTo("Exhaust"));
            Assert.That(Property(icon, "IsImminent"), Is.False);
            Component furnace = CreateEnemy("FurnaceBoss", 7, 200, 10, "Fire");
            yield return null;
            Component heat = Intent(furnace);
            for (int i = 0; i < 3; i++) Call(furnace, "TakeSpellHit", Magic("Rock"), Magic("Shoot"), 1f, 1f);
            Assert.That(Property(heat, "Icon").ToString(), Is.EqualTo("Heat"));
            Assert.That(Property(heat, "Level"), Is.EqualTo(3));
            Assert.That(Property(heat, "DamagePreview"), Is.EqualTo(19));
            Call(furnace, "PlayTurnAction", 10f);
            Assert.That(Property(heat, "Level"), Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator 흡수_대상과_보스는_같은_문양이고_흡수하면_가져온_기술로_바뀐다()
        {
            Component boss = CreateEnemy("GraftBoss", 7, 180, 10, "Undead");
            Component target = CreateEnemy("ChargeSlime", 5, 30, 10, "Fire");
            Runtime("Combat.Enemies.Enemy").GetField("id", AnyInstance).SetValue(target, 21);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Component own = Intent(boss), marked = Intent(target);
            Assert.That(Property(own, "Glyph"), Is.SameAs(Property(marked, "Glyph")));
            Assert.That(Property(own, "IsImminent"), Is.True);
            Call(target, "Kill");
            yield return null;
            yield return null;
            Assert.That(marked == null || !marked.gameObject.activeInHierarchy, Is.True);
            Assert.That(Property(own, "IsImminent"), Is.False);
            Component mortar = CreateEnemy("MortarSlime", 5, 30, 10, "Rock");
            Runtime("Combat.Enemies.Enemy").GetField("id", AnyInstance).SetValue(mortar, 20);
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Call(boss, "PlayTurnAction", 12f);
            yield return new WaitForSeconds(1.15f);
            Assert.That(Property(own, "Icon").ToString(), Is.EqualTo("Drop"));
        }

        [UnityTest]
        public IEnumerator 모방_예고는_카드_중앙_문양과_겹친_테두리를_쓰고_확대와_반전에도_크기가_같다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6, 100, 12, "Undead");
            yield return null;
            Component icon = Intent(boss);
            Object catalog = Resources.Load("Combat/BossIntentArt");
            foreach (string spell in new[] { "Shoot", "Drop", "Explode" })
            {
                Call(boss, "Remember", Magic(spell));
                Assert.That(Property(icon, "Icon").ToString(), Is.EqualTo(spell));
                Assert.That(Property(icon, "IsCopied"), Is.True);
                Sprite glyph = (Sprite)Property(icon, "Glyph");
                Sprite card = (Sprite)catalog.GetType().GetField(spell.ToLowerInvariant()).GetValue(catalog);
                Assert.That(glyph.texture, Is.SameAs(card.texture));
                Assert.That(glyph.rect.height, Is.LessThan(card.rect.height));
                Assert.That(icon.GetComponentInChildren(Runtime("TMPro.TMP_Text")), Is.Null);
            }
            float width = icon.transform.Find("Glyph").GetComponent<SpriteRenderer>().bounds.size.x;
            Vector3 scale = boss.transform.localScale;
            boss.transform.localScale = new Vector3(-scale.x * 1.4f, scale.y * 1.4f, scale.z);
            yield return null;
            yield return null;
            Assert.That(icon.transform.Find("Glyph").GetComponent<SpriteRenderer>().bounds.size.x, Is.EqualTo(width).Within(.01f));
            Assert.That(Physics2D.OverlapPointAll(icon.transform.position).Any(h => h.gameObject == icon.gameObject), Is.True);
        }

        [UnityTest]
        public IEnumerator 세계수_연속시전_중에는_턴을_넘기지_않고_죽으면_남은_마법을_취소한다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6, 300, 12, "Undead");
            yield return null;
            foreach (string element in new[] { "Fire", "Ice", "Lightning" })
            {
                Call(boss, "RememberElement", Magic("Shoot"), Magic(element));
                Call(boss, "PlayTurnAction", 10f);
            }
            Component battle = (Component)Object.FindObjectOfType(Runtime("Battle.Turns.TurnBattleSystem"));
            int turn = (int)Property(battle, "TurnNumber");
            Call(battle, "StartEnemyTurnCounter");
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That(Property(boss, "IsCasting"), Is.True);
            Assert.That(Property(battle, "TurnNumber"), Is.EqualTo(turn));
            Call(boss, "Kill");
            int hp = PlayerHp;
            yield return new WaitForSeconds(1.2f);
            Assert.That(PlayerHp, Is.EqualTo(hp), "사망 후에는 남은 주문이 피해를 주지 않는다");
            Assert.That(Runtime("Combat.Enemies.SurgeBossEnemy").GetProperty("IsCastingAny").GetValue(null), Is.False);
        }

        [UnityTest]
        public IEnumerator 모방_원소는_화상과_냉기_감전을_실제_대상에_적용하고_원본_그림을_쓴다()
        {
            var statusType = Runtime("Combat.Allies.FriendlyElementalStatus");
            var hit = statusType.GetMethod("Hit");
            hit.Invoke(null, new object[] { Player, Magic("Fire"), Magic("Shoot"), 5 });
            Component receiver = Player.GetComponent(statusType);
            object status = Property(receiver, "Status");
            int Value(string name) => (int)status.GetType().GetProperty(name).GetValue(status);
            Assert.That(Value("BurnTurns"), Is.EqualTo(2));
            hit.Invoke(null, new object[] { Player, Magic("Ice"), Magic("Shoot"), 5 });
            Assert.That(Value("BurnTurns"), Is.Zero);
            Assert.That(Value("Chill"), Is.EqualTo(2));
            hit.Invoke(null, new object[] { Player, Magic("Lightning"), Magic("Shoot"), 5 });
            Assert.That(Value("ShockTurns"), Is.EqualTo(2));
            Object catalog = Resources.Load("Combat/EnemyMagicCatalog");
            foreach (string spell in new[] { "Shoot", "Drop", "Explode" })
                foreach (string element in new[] { "Holy", "Fire", "Ice", "Rock", "Lightning" })
                {
                    GameObject prefab = (GameObject)catalog.GetType().GetMethod("Prefab").Invoke(catalog, new[] { Magic(spell), Magic(element) });
                    Assert.That(prefab, Is.Not.Null, element + spell);
                    Assert.That(prefab.GetComponent<Animator>(), Is.Not.Null);
                    Assert.That(catalog.GetType().GetMethod("Element").Invoke(catalog, new[] { Magic(element) }), Is.Not.Null);
                }
            yield return null;
        }

        [UnityTest]
        public IEnumerator 각_마법_아이콘은_해당_속성과_행동만_짧게_설명한다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6, 100, 12, "Undead");
            yield return null;
            Call(boss, "RememberElement", Magic("Drop"), Magic("Fire"));
            Call(boss, "PlayTurnAction", 10f);
            Call(boss, "RememberElement", Magic("Shoot"), Magic("Ice"));
            Component[] icons = boss.GetComponentsInChildren(Runtime("Combat.Enemies.BossIntent"));
            Component hover = (Component)Runtime("Combat.UI.EnemyHoverInfo").GetMethod("EnsureExists").Invoke(null, null);
            Call(hover, "ShowIntent", icons[0]);
            yield return null;
            Assert.That(Property(hover, "Text").ToString(), Does.Contain("Drop").And.Not.Contain("Shoot").And.Not.Contain("체력"));
            Call(hover, "ShowIntent", icons[1]);
            yield return null;
            Assert.That(Property(hover, "Text").ToString(), Does.Contain("Shoot").And.Not.Contain("Drop"));
            Call(boss, "RememberElement", Magic("Explode"), Magic("Lightning"));
            yield return null;
            Assert.That(Property(hover, "Text").ToString(), Does.Contain("Explode").And.Not.Contain("Shoot"));
        }

        [UnityTest]
        public IEnumerator 마우스_정보는_체력이_같아도_다음_행동과_언어_변경을_따라간다()
        {
            Type localization = Runtime("Core.Localization");
            object previousLanguage = localization.GetProperty("Current").GetValue(null);
            bool hadLanguage = PlayerPrefs.HasKey("Language");
            int oldPreference = PlayerPrefs.GetInt("Language");
            try
            {
                object Lang(string name) => Enum.Parse(Runtime("Core.Language"), name);
                localization.GetMethod("SetLanguage").Invoke(null, new[] { Lang("Korean") });
                Component boss = CreateEnemy("TreeShellBoss", 2.5f, 130, 14, "Rock");
                yield return null;
                Component hover = (Component)Runtime("Combat.UI.EnemyHoverInfo").GetMethod("EnsureExists").Invoke(null, null);
                Call(hover, "Show", boss);
                yield return null;
                Assert.That(Property(hover, "Text").ToString(), Does.Contain("다음 행동").And.Contain("뿌리 공격 준비"));
                int hp = EnemyHp(boss);
                Call(boss, "PlayTurnAction", 10f);
                yield return null;
                Assert.That(EnemyHp(boss), Is.EqualTo(hp));
                Assert.That(Property(hover, "Text").ToString(), Does.Contain("다음 턴 뿌리 공격"));
                Assert.That(Property(Intent(boss), "IsImminent"), Is.True);
                localization.GetMethod("SetLanguage").Invoke(null, new[] { Lang("English") });
                yield return null;
                Assert.That(Property(hover, "Text").ToString(), Does.Contain("Next action").And.Not.Contain("뿌리"));
            }
            finally
            {
                localization.GetMethod("SetLanguage").Invoke(null, new[] { previousLanguage });
                if (hadLanguage) PlayerPrefs.SetInt("Language", oldPreference); else PlayerPrefs.DeleteKey("Language");
                PlayerPrefs.Save();
            }
        }
    }
}
#endif
