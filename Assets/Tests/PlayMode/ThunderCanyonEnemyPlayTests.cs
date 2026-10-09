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
    // 곡사·돌진·방패 슬라임의 행동을 실제 전투 씬(워드와 턴 시스템이 있는 씬) 위에서 확인한다.
    public sealed class ThunderCanyonEnemyPlayTests
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

        [UnityTest]
        public IEnumerator 곡사_슬라임은_한_턴_전에_맨_뒤_아군_자리를_표시하고_다음_턴에_그_자리를_맞힌다()
        {
            Component front = SpawnAlly("Fire");
            Component rear = SpawnAlly("Ice");
            Component mortar = CreateEnemy("MortarSlime", 8f, 25, 10, "Lightning");
            yield return null;

            Call(mortar, "PlayTurnAction", FrontLineDistance(mortar));
            Assert.That(Property(mortar, "HasMarker"), Is.True);
            Assert.That((float)Property(mortar, "MarkerX"), Is.EqualTo(rear.transform.position.x).Within(0.01f));
            Assert.That(AllyHp(rear), Is.EqualTo(8), "표시한 턴에는 맞히지 않는다");

            Call(mortar, "PlayTurnAction", FrontLineDistance(mortar));
            Assert.That(GameObject.Find("MortarShell"), Is.Not.Null, "쏜 턴에는 탄이 날아간다");
            Assert.That(AllyHp(rear), Is.EqualTo(8), "탄이 떨어지기 전에는 맞히지 않는다");
            Assert.That(Property(mortar, "HasMarker"), Is.False);

            yield return new WaitForSeconds(0.8f);
            Assert.That(AllyHp(rear), Is.EqualTo(0));
            Assert.That(AllyHp(front), Is.EqualTo(8), "앞줄 슬라임은 곡사 탄을 막지 못한다");
            Assert.That(GameObject.Find("MortarShell"), Is.Null, "떨어진 탄은 사라진다");
            Assert.That(GameObject.Find("MortarMarker"), Is.Null, "떨어진 자리의 표시도 사라진다");
        }

        [UnityTest]
        public IEnumerator 아군이_없으면_곡사_슬라임은_워드를_노린다()
        {
            Component mortar = CreateEnemy("MortarSlime", 8f, 25, 10, "Lightning");
            yield return null;
            int hp = PlayerHp;

            Call(mortar, "PlayTurnAction", FrontLineDistance(mortar));
            Assert.That((float)Property(mortar, "MarkerX"), Is.EqualTo(Player.transform.position.x).Within(0.01f));
            Call(mortar, "PlayTurnAction", FrontLineDistance(mortar));
            yield return new WaitForSeconds(0.8f);
            Assert.That(PlayerHp, Is.EqualTo(hp - 10));
        }

        [UnityTest]
        public IEnumerator 빙결된_곡사_슬라임은_그_턴에_표시하지_않는다()
        {
            Component mortar = CreateEnemy("MortarSlime", 8f, 100, 10, "Lightning");
            yield return null;
            Call(mortar, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 1f, 1f);
            Call(mortar, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 1f, 1f);
            object status = Runtime("Combat.Enemies.Enemy").GetProperty("Status").GetValue(mortar);
            Assert.That(status.GetType().GetProperty("Frozen").GetValue(status), Is.True);

            Call(mortar, "PlayTurnAction", FrontLineDistance(mortar));
            Assert.That(Property(mortar, "HasMarker"), Is.False);
        }

        [UnityTest]
        public IEnumerator 돌진_슬라임은_한_턴_예고한_뒤_달려와_아군_슬라임에게_두_배_피해를_준다()
        {
            Component ally = SpawnAlly("Rock");
            float frontX = ally.transform.position.x;
            Component charger = CreateEnemy("ChargeSlime", frontX + 5f, 35, 10, "Rock");
            yield return null;

            Call(charger, "PlayTurnAction", FrontLineDistance(charger));
            Assert.That(Property(charger, "IsCharging"), Is.True);
            Assert.That(charger.transform.position.x, Is.EqualTo(frontX + 5f).Within(0.01f), "예고한 턴에는 움직이지 않는다");

            Call(charger, "PlayTurnAction", FrontLineDistance(charger));
            yield return new WaitForSeconds(0.5f);
            Assert.That(charger.transform.position.x, Is.EqualTo(frontX + 1.2f).Within(0.05f));
            Assert.That(AllyHp(ally), Is.EqualTo(0), "공격력 10의 두 배가 HP 8을 넘는다");
            Assert.That(Property(charger, "IsCharging"), Is.False);
        }

        [UnityTest]
        public IEnumerator 돌진_예고는_글자_없이_뒤로_물러나_웅크리는_프레임으로_보인다()
        {
            Component charger = CreateEnemy("ChargeSlime", 0f, 100, 10, "Rock");
            yield return null;
            Call(charger, "PlayTurnAction", 3f);
            // 피격 애니메이션이 끝난 뒤에도 대기 대신 예고 프레임으로 돌아와야 한다.
            Call(charger, "TakeSpellHit", Magic("Rock"), Magic("Spawn"), 4f, 1f);
            yield return new WaitForSeconds(0.6f);

            string frame = charger.GetComponent<SpriteRenderer>().sprite.name;
            Assert.That(frame, Is.EqualTo("ChargeSlime_09").Or.EqualTo("ChargeSlime_10"));
            Assert.That(charger.transform.position.x, Is.GreaterThan(0.3f), "아군 반대쪽으로 물러난다");
            Assert.That(charger.GetComponentsInChildren<Transform>().Any(t => t.name == "ElementalStatusText"), Is.False);

            Call(charger, "PlayTurnAction", 3f);
            yield return new WaitForSeconds(0.6f);
            frame = charger.GetComponent<SpriteRenderer>().sprite.name;
            Assert.That(frame, Is.Not.EqualTo("ChargeSlime_09").And.Not.EqualTo("ChargeSlime_10"), "돌진한 뒤에는 예고 프레임을 멈춘다");
        }

        [UnityTest]
        public IEnumerator 방패_슬라임_바로_뒤의_적은_주문_피해를_절반만_받는다()
        {
            CreateEnemy("ShieldSlime", 2f, 60, 5, "Rock");
            Component guarded = CreateEnemy("MortarSlime", 3.5f, 100, 10, "Lightning");
            Component exposed = CreateEnemy("MortarSlime", 6f, 100, 10, "Lightning");
            yield return null;

            Call(guarded, "TakeSpellHit", Magic("Fire"), Magic("Shoot"), 20f, 1f);
            Call(exposed, "TakeSpellHit", Magic("Fire"), Magic("Shoot"), 20f, 1f);
            Assert.That(EnemyHp(guarded), Is.EqualTo(90));
            Assert.That(EnemyHp(exposed), Is.EqualTo(80));
        }
    }
}
#endif
