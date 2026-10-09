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
    // 분열·폭발·치유 슬라임과 최종 보스의 행동을 실제 전투 씬 위에서 확인한다.
    public sealed class ActTwoLateEnemyPlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const string SlimeFolder = "Assets/Prefabs/Combat/Enemies/Slime/";
        const string AffinityTablePath = "Assets/ScriptableObjects/Combat/Magic Affinity Table.asset";
        readonly string[] keys = { "TutorialEnded", "StagePosition" };
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
        public IEnumerator 분열_슬라임이_쓰러지면_작은_슬라임_두_마리가_웨이브에_더해진다()
        {
            Component split = CreateEnemy("SplitSlime", 4f, 40, 10, "Fire");
            yield return null;
            Call(split, "Kill");
            yield return null;

            Component[] children = AliveEnemiesNamed("SplitSlimeSmall");
            Assert.That(children.Length, Is.EqualTo(2));
            Component wave = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            var tracked = (IList)wave.GetType().GetField("activatedEnemies", AnyInstance).GetValue(wave);
            foreach (Component child in children)
                Assert.That(tracked.Contains(child.gameObject), Is.True, "작은 슬라임을 쓰러뜨리기 전에 웨이브가 끝난다");
            foreach (GameObject go in children.Select(c => c.gameObject)) spawned.Add(go);
        }

        [UnityTest]
        public IEnumerator 폭발_슬라임은_한_턴_예고한_뒤_터져_아군_슬라임_전체에_피해를_준다()
        {
            Component front = SpawnAlly("Fire");
            Component rear = SpawnAlly("Ice");
            Component bomber = CreateEnemy("ExplodeSlime", front.transform.position.x + 2f, 25, 15, "Lightning");
            yield return null;

            Call(bomber, "PlayTurnAction", FrontLineDistance(bomber));
            Assert.That(Property(bomber, "IsPrimed"), Is.True);
            Assert.That(AllyHp(front), Is.EqualTo(8), "예고한 턴에는 터지지 않는다");

            Call(bomber, "PlayTurnAction", FrontLineDistance(bomber));
            Assert.That(AllyHp(front), Is.EqualTo(0));
            Assert.That(AllyHp(rear), Is.EqualTo(0));
            Assert.That(Property(bomber, "IsAlive"), Is.False);
        }

        [UnityTest]
        public IEnumerator 아군이_없으면_폭발은_워드가_받는다()
        {
            Component bomber = CreateEnemy("ExplodeSlime", Player.transform.position.x + 2f, 25, 15, "Lightning");
            yield return null;
            int hp = PlayerHp;
            Call(bomber, "PlayTurnAction", 2f);
            Call(bomber, "PlayTurnAction", 2f);
            Assert.That(PlayerHp, Is.EqualTo(hp - 15));
        }

        [UnityTest]
        public IEnumerator 예고한_폭발_슬라임을_먼저_쓰러뜨리면_주변_적이_피해를_받는다()
        {
            Component bomber = CreateEnemy("ExplodeSlime", 3f, 25, 15, "Lightning");
            Component near = CreateEnemy("MortarSlime", 4.5f, 100, 10, "Lightning");
            Component far = CreateEnemy("MortarSlime", 7f, 100, 10, "Lightning");
            yield return null;
            Call(bomber, "PlayTurnAction", 2f);
            Call(bomber, "Kill");
            Assert.That(EnemyHp(near), Is.EqualTo(85));
            Assert.That(EnemyHp(far), Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator 치유_슬라임은_가장_많이_다친_적을_공격력만큼_회복한다()
        {
            Component healer = CreateEnemy("HealSlime", 8f, 30, 10, "Ice");
            Component scratched = CreateEnemy("MortarSlime", 4f, 100, 10, "Lightning");
            Component wounded = CreateEnemy("MortarSlime", 5f, 100, 10, "Lightning");
            yield return null;
            Call(scratched, "TakeHit", 5);
            Call(wounded, "TakeHit", 30);

            Call(healer, "PlayTurnAction", 10f);
            Assert.That(EnemyHp(wounded), Is.EqualTo(80));
            Assert.That(EnemyHp(scratched), Is.EqualTo(95));
        }

        [UnityTest]
        public IEnumerator 최종_보스는_포격을_번갈아_쓰고_HP가_절반_아래면_분열_슬라임을_부른다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6f, 100, 12, "Undead");
            yield return null;
            int hp = PlayerHp;

            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            Assert.That(Property(boss, "HasMarker"), Is.True);
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            yield return new WaitForSeconds(0.8f);
            Assert.That(PlayerHp, Is.EqualTo(hp - 12), "아군이 없으면 포격은 워드에게 떨어진다");

            Call(boss, "TakeHit", 60);
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            yield return null;
            Assert.That(Property(boss, "HasSummoned"), Is.True);
            Component[] children = AliveEnemiesNamed("SplitSlimeSmall");
            Assert.That(children.Length, Is.EqualTo(2));
            foreach (GameObject go in children.Select(c => c.gameObject)) spawned.Add(go);

            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            Assert.That(AliveEnemiesNamed("SplitSlimeSmall").Length, Is.EqualTo(2), "부르기는 한 번뿐이다");
        }
    }
}
#endif
