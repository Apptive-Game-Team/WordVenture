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
    // 후방 도약의 턴 순서와 실제 몸 중심의 폭발을 전투 씬에서 확인한다.
    public sealed class BossMotionAndBlastPlayTests
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

        [UnityTest]
        public IEnumerator 돌진_후_한_턴_탈진하고_회복_도약_착지_다음_턴에_바로_다시_돌진한다()
        {
            Component boss = CreateEnemy("ThunderBoss", 3f, 180, 4, "Lightning");
            yield return null;
            float homeX = boss.transform.position.x;
            int hp = PlayerHp;
            Call(boss, "PlayTurnAction", 10f); // 첫 예고
            Assert.That(PlayerHp, Is.EqualTo(hp));
            Call(boss, "PlayTurnAction", 10f); // 첫 돌진
            yield return new WaitForSeconds(0.4f);
            Assert.That(PlayerHp, Is.EqualTo(hp - 4));
            float dashX = boss.transform.position.x;
            Call(boss, "PlayTurnAction", 2f); // 탈진 턴은 이동도 공격도 없다.
            yield return new WaitForSeconds(0.1f);
            Assert.That(Property(boss, "IsExhausted"), Is.True);
            Assert.That(boss.transform.position.x, Is.EqualTo(dashX).Within(0.01f));
            Assert.That(PlayerHp, Is.EqualTo(hp - 4));
            Call(boss, "PlayTurnAction", 2f); // 회복 턴에는 뒤로 도약하고 착지하며 예고
            yield return new WaitForSeconds(0.25f);
            Assert.That(boss.transform.position.y, Is.GreaterThan(-2.9f), "순간이동이나 지상 후퇴 대신 실제 도약한다");
            yield return new WaitForSeconds(0.25f);
            Assert.That(boss.transform.position.x, Is.EqualTo(homeX).Within(0.06f));
            Assert.That(boss.transform.position.y, Is.EqualTo(-3f).Within(0.01f));
            Assert.That(Property(boss, "IsCharging"), Is.True);
            Component intent = boss.GetComponentInChildren(Runtime("Combat.Enemies.BossIntent"));
            Assert.That(Property(intent, "IsImminent"), Is.True);
            Assert.That(Object.FindObjectOfType(Runtime("Combat.Enemies.ThunderLandingVfx")), Is.Not.Null);
            Assert.That(PlayerHp, Is.EqualTo(hp - 4));
            Call(boss, "PlayTurnAction", 10f);
            yield return new WaitForSeconds(0.4f);
            Assert.That(PlayerHp, Is.EqualTo(hp - 8), "착지 다음 턴에 추가 예고 턴 없이 돌진한다");
        }

        [UnityTest]
        public IEnumerator 최초_위치가_화면_밖이어도_회복_도약은_몸_전체가_보이는_곳에_착지한다()
        {
            Camera camera = Camera.main;
            float depth = Mathf.Abs(camera.transform.position.z);
            float outside = camera.ViewportToWorldPoint(new Vector3(1, 0.5f, depth)).x + 4f;
            Component boss = CreateEnemy("ThunderBoss", outside, 180, 4, "Lightning");
            yield return null;
            Call(boss, "PlayTurnAction", 12f);
            Call(boss, "PlayTurnAction", 12f);
            yield return new WaitForSeconds(0.4f);
            Call(boss, "PlayTurnAction", 2f);
            Call(boss, "PlayTurnAction", 2f);
            yield return new WaitForSeconds(0.5f);
            SpriteRenderer body = boss.GetComponent<SpriteRenderer>();
            Bounds visible = (Bounds)Runtime("Combat.Enemies.ElementalStatusVfx").GetMethod("GetVisibleBodyBounds")
                .Invoke(null, new object[] { body.sprite });
            Vector3 a = body.transform.TransformPoint(visible.min);
            Vector3 b = body.transform.TransformPoint(visible.max);
            float left = Mathf.Min(camera.WorldToViewportPoint(a).x, camera.WorldToViewportPoint(b).x);
            float right = Mathf.Max(camera.WorldToViewportPoint(a).x, camera.WorldToViewportPoint(b).x);
            Assert.That(left, Is.GreaterThanOrEqualTo(0f));
            Assert.That(right, Is.LessThanOrEqualTo(1f));
        }

        [UnityTest]
        public IEnumerator 후방_도약_도중_쓰러져도_착지_후_새_예고나_공격을_남기지_않는다()
        {
            Component boss = CreateEnemy("ThunderBoss", 6f, 180, 4, "Lightning");
            yield return null;
            Call(boss, "PlayTurnAction", 10f);
            Call(boss, "PlayTurnAction", 10f);
            yield return new WaitForSeconds(0.4f);
            Call(boss, "PlayTurnAction", 2f);
            Call(boss, "PlayTurnAction", 2f);
            int hp = PlayerHp;
            yield return new WaitForSeconds(0.1f);
            Call(boss, "Kill");
            yield return new WaitForSeconds(0.5f);
            Assert.That(boss.gameObject.activeSelf, Is.False);
            Assert.That(PlayerHp, Is.EqualTo(hp));
            Assert.That(Object.FindObjectOfType(Runtime("Combat.Enemies.ThunderLandingVfx")), Is.Null);
        }

        [UnityTest]
        public IEnumerator 폭발은_발밑이_아닌_실제_몸_중심에서_몸_두_배_이상_크기로_나온다()
        {
            Component bomber = CreateEnemy("ExplodeSlime", 3f, 25, 8, "Lightning");
            yield return null;
            Call(bomber, "PlayTurnAction", 2f);
            SpriteRenderer body = bomber.GetComponent<SpriteRenderer>();
            Bounds visible = (Bounds)Runtime("Combat.Enemies.ElementalStatusVfx").GetMethod("GetVisibleBodyBounds")
                .Invoke(null, new object[] { body.sprite });
            Vector3 localCenter = visible.center;
            if (body.flipX) localCenter.x = -localCenter.x;
            if (body.flipY) localCenter.y = -localCenter.y;
            Vector3 center = body.transform.TransformPoint(localCenter);
            float width = visible.size.x * Mathf.Abs(body.transform.lossyScale.x);
            Call(bomber, "PlayTurnAction", 2f);
            Component blast = (Component)Object.FindObjectOfType(Runtime("Combat.Enemies.BlastVfx"));
            Assert.That(blast, Is.Not.Null);
            Assert.That(Vector3.Distance(blast.transform.position, center), Is.LessThan(0.01f));
            Assert.That((float)Property(blast, "Diameter"), Is.GreaterThanOrEqualTo(Mathf.Max(5f, width * 2f)));
            Assert.That(blast.transform.parent, Is.Null);
            Assert.That(blast.transform.Find("Flash"), Is.Not.Null);
            Assert.That(blast.GetComponentsInChildren<SpriteRenderer>().Length, Is.GreaterThan(10));
            yield return new WaitForSeconds(0.4f);
            Assert.That(bomber.gameObject.activeSelf, Is.False);
            Assert.That(blast != null, Is.True, "폭발 효과는 시체 비활성화 뒤에도 남는다");
        }
    }
}
#endif
