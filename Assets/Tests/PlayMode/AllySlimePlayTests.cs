#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class AllySlimePlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        const string AffinityTablePath = "Assets/ScriptableObjects/Combat/Magic Affinity Table.asset";

        readonly List<GameObject> objects = new List<GameObject>();
        Object affinityTable;
        Component formation;

        static Type TypeOf(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static Type FormationType => TypeOf("Combat.Allies.AllyFormation");
        static object Magic(string name) => Enum.Parse(TypeOf("Cards.MagicType"), name);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, AnyInstance).Invoke(target, args);
        static object CallStatic(string name, params object[] args) => FormationType
            .GetMethod(name, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);
        static int AllyHp(Component ally) => (int)ally.GetType().GetField("hp", AnyInstance).GetValue(ally);
        static int EnemyHp(Component enemy) =>
            (int)TypeOf("Combat.Enemies.Enemy").GetField("Hp", AnyInstance).GetValue(enemy);

        List<Component> Allies => ((IEnumerable)FormationType.GetProperty("Allies").GetValue(formation))
            .Cast<Component>().ToList();
        bool CanSpawn => (bool)FormationType.GetProperty("CanSpawn").GetValue(null);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            affinityTable = AssetDatabase.LoadAssetAtPath<Object>(AffinityTablePath);
            var anchor = new GameObject("TestWord");
            objects.Add(anchor);
            formation = (Component)CallStatic("GetOrCreate", anchor.transform);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (formation != null) Object.Destroy(formation.gameObject);
            foreach (GameObject go in objects) if (go != null) Object.Destroy(go);
            objects.Clear();
            yield return null;
        }

        Component Spawn(string element) => (Component)Call(formation, "Spawn", Magic(element), affinityTable);

        Component CreateEnemy(float x)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Enemies/Slime/MeleeNeon.prefab");
            GameObject slime = Object.Instantiate(prefab, new Vector3(x, -3f, 0f), Quaternion.identity);
            objects.Add(slime);
            Component enemy = slime.GetComponent(TypeOf("Combat.Enemies.Enemy"));
            object data = Activator.CreateInstance(TypeOf("Combat.Enemies.EnemyData"));
            data.GetType().GetField("maxHp").SetValue(data, 100);
            data.GetType().GetField("moveDistance").SetValue(data, 4f);
            data.GetType().GetField("attackRange").SetValue(data, 3f);
            data.GetType().GetField("damage").SetValue(data, 10);
            data.GetType().GetField("type").SetValue(data, Magic("Fire"));
            Call(enemy, "InitEnemyData", data);
            return enemy;
        }

        [UnityTest]
        public IEnumerator 아군_자리는_3칸이고_먼저_소환한_슬라임이_맨_앞에_선다()
        {
            Component first = Spawn("Fire");
            Spawn("Ice");
            Spawn("Rock");
            yield return null;

            Assert.That(Allies.Count, Is.EqualTo(3));
            Assert.That(CanSpawn, Is.False);
            Assert.That(Spawn("Lightning"), Is.Null, "자리가 찼는데 네 번째 슬라임이 소환됐다");
            Assert.That(FormationType.GetProperty("Front").GetValue(formation), Is.SameAs(first));
            float frontX = first.transform.position.x;
            Assert.That(Allies.All(ally => ally.transform.position.x <= frontX), Is.True,
                "나중에 소환한 슬라임은 맨 앞 슬라임보다 워드 쪽에 선다");
            Assert.That((float)CallStatic("FrontLineX", -7f), Is.EqualTo(frontX));
        }

        [UnityTest]
        public IEnumerator 아군이_없으면_적의_기준선은_워드다()
        {
            yield return null;
            Assert.That((float)CallStatic("FrontLineX", -7f), Is.EqualTo(-7f));
            Assert.That(CanSpawn, Is.True);
        }

        [UnityTest]
        public IEnumerator 근접_공격은_맨_앞_슬라임이_받고_쓰러지면_뒤_슬라임이_앞으로_나온다()
        {
            Component first = Spawn("Fire");
            Component second = Spawn("Ice");
            float frontX = first.transform.position.x;
            yield return null;

            CallStatic("HitFrontLine", 5, 10f, 1);
            Assert.That(AllyHp(first), Is.EqualTo(3));
            Assert.That(AllyHp(second), Is.EqualTo(8));

            CallStatic("HitFrontLine", 10, 10f, 1);
            Assert.That(Allies, Is.EqualTo(new[] { second }));
            Assert.That(CanSpawn, Is.True);
            yield return new WaitForSeconds(0.5f);
            Assert.That(second.transform.position.x, Is.EqualTo(frontX).Within(0.01f));
            Assert.That(first == null, Is.True, "쓰러진 슬라임이 남아 있다");
        }

        [UnityTest]
        public IEnumerator 맨_앞_칸보다_워드_쪽에_온_적은_자기와_워드_사이의_슬라임을_때린다()
        {
            Component first = Spawn("Fire");
            Component second = Spawn("Ice");
            yield return null;

            // 워드(x=0) 앞의 칸은 x=4.2, 2.8이다. x=3.5의 적은 맨 앞 슬라임을 이미 지나쳤다.
            CallStatic("HitFrontLine", 5, 3.5f, 1);
            Assert.That(AllyHp(first), Is.EqualTo(8));
            Assert.That(AllyHp(second), Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator 적의_원거리_탄은_아군_슬라임에_맞는다()
        {
            Component ally = Spawn("Rock");
            var projectileObject = new GameObject("TestProjectile");
            objects.Add(projectileObject);
            Component projectile = projectileObject.AddComponent(TypeOf("Combat.Enemies.EnemyProjectile"));
            Call(projectile, "InitProjectileDamage", 6);
            yield return null;

            Call(projectile, "OnTriggerEnter2D", ally.GetComponent<Collider2D>());
            Assert.That(AllyHp(ally), Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator 아군은_앞줄부터_가장_앞의_적을_속성으로_공격한다()
        {
            Spawn("Ice");
            Spawn("Ice");
            Component near = CreateEnemy(2f);
            Component far = CreateEnemy(6f);
            yield return null;

            // AllyFormation은 Func<Enemy>를 받는데, 테스트 어셈블리는 Enemy 타입을 참조할 수 없다.
            Func<Component> findFront = () => EnemyHp(near) > 0 ? near : far;
            Type enemyType = TypeOf("Combat.Enemies.Enemy");
            Delegate finder = Expression.Lambda(typeof(Func<>).MakeGenericType(enemyType),
                Expression.Convert(Expression.Invoke(Expression.Constant(findFront)), enemyType)).Compile();
            yield return (IEnumerator)Call(formation, "AttackFrontEnemies", finder);

            Assert.That(EnemyHp(near), Is.LessThan(100), "가장 앞의 적이 공격받지 않았다");
            Assert.That(EnemyHp(near), Is.GreaterThanOrEqualTo(100 - 2 * 4 * 2),
                "아군 슬라임 두 마리의 공격력 4가 상성 2배를 넘어서 들어갔다");
            Assert.That(EnemyHp(far), Is.EqualTo(100));
            object status = TypeOf("Combat.Enemies.Enemy").GetProperty("Status").GetValue(near);
            Assert.That(status.GetType().GetProperty("Chill").GetValue(status), Is.EqualTo(2),
                "얼음 슬라임 두 마리의 공격이 냉기를 1씩 쌓아야 한다");
        }
    }
}
#endif
