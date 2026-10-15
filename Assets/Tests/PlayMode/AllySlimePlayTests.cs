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

        Component CreateEnemy(float x, string defense = "Fire")
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
            data.GetType().GetField("type").SetValue(data, Magic(defense));
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
            Assert.That(AllyHp(ally), Is.EqualTo(12), "바위 슬라임의 HP 16에서 피해 6의 70%(4)만 줄어야 한다");
        }

        // AllyFormation은 Func<Enemy>를 받는데, 테스트 어셈블리는 Enemy 타입을 참조할 수 없다.
        IEnumerator RunAttacks(Func<Component> findFront)
        {
            Type enemyType = TypeOf("Combat.Enemies.Enemy");
            Delegate finder = Expression.Lambda(typeof(Func<>).MakeGenericType(enemyType),
                Expression.Convert(Expression.Invoke(Expression.Constant(findFront)), enemyType)).Compile();
            yield return (IEnumerator)Call(formation, "AttackFrontEnemies", finder);
        }

        static object Status(Component enemy) => TypeOf("Combat.Enemies.Enemy").GetProperty("Status").GetValue(enemy);
        static object StatusValue(Component enemy, string name)
        {
            object status = Status(enemy);
            return status.GetType().GetProperty(name).GetValue(status);
        }

        Component SpellOnAlly(string spell, string element, Component ally)
        {
            var go = new GameObject("TestAllySpell");
            objects.Add(go);
            Component spellObj = go.AddComponent(TypeOf("Combat.Spells.SpellObj"));
            Call(spellObj, "InitSpell", Magic(spell), Magic(element),
                ally.GetComponent(TypeOf("Combat.Enemies.SelectableObject")), affinityTable);
            return spellObj;
        }

        [UnityTest]
        public IEnumerator 바위_슬라임은_HP가_2배이고_적의_공격은_30퍼센트_덜_받고_공격력은_절반이다()
        {
            Component rock = Spawn("Rock");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Assert.That(AllyHp(rock), Is.EqualTo(16));

            CallStatic("HitFrontLine", 10, 10f, 1);
            Assert.That(AllyHp(rock), Is.EqualTo(9), "피해 10의 70%인 7만 들어가야 한다");
            Call(rock, "TakeSpellDamage", 3);
            Assert.That(AllyHp(rock), Is.EqualTo(6), "주문 피해는 바위의 피해 감소를 받지 않는다");

            yield return RunAttacks(() => enemy);
            Assert.That(EnemyHp(enemy), Is.LessThan(100));
            Assert.That(EnemyHp(enemy), Is.GreaterThanOrEqualTo(100 - 2 * 2), "바위의 공격력은 4의 절반인 2다");
        }

        [UnityTest]
        public IEnumerator 불_슬라임은_공격력이_가장_높고_맞은_적에게_화상을_건다()
        {
            Component fire = Spawn("Fire");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Assert.That((int)fire.GetType().GetProperty("AttackValue").GetValue(fire), Is.EqualTo(6));

            yield return RunAttacks(() => enemy);
            Assert.That(EnemyHp(enemy), Is.LessThan(100));
            Assert.That((int)StatusValue(enemy, "BurnTurns"), Is.GreaterThan(0), "불 슬라임의 공격이 화상을 걸지 않았다");
        }

        [UnityTest]
        public IEnumerator 얼음_슬라임은_공격력이_낮고_맞은_적에게_냉기를_쌓는다()
        {
            Component ice = Spawn("Ice");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Assert.That((int)ice.GetType().GetProperty("AttackValue").GetValue(ice), Is.EqualTo(3));

            yield return RunAttacks(() => enemy);
            Assert.That((int)StatusValue(enemy, "Chill"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator 번개_슬라임은_맨_앞_적과_그_뒤의_적을_함께_공격한다()
        {
            Spawn("Lightning");
            Component near = CreateEnemy(2f);
            Component far = CreateEnemy(6f);
            Component farthest = CreateEnemy(9f);
            yield return null;

            yield return RunAttacks(() => near);
            Assert.That(EnemyHp(near), Is.LessThan(100));
            Assert.That(EnemyHp(far), Is.LessThan(100), "뒤의 적이 연쇄 공격을 받지 않았다");
            Assert.That(EnemyHp(far), Is.GreaterThanOrEqualTo(EnemyHp(near)), "두 번째 적의 피해는 0.75배다");
            Assert.That(EnemyHp(farthest), Is.EqualTo(100), "세 번째 적까지 맞으면 안 된다");
        }

        [UnityTest]
        public IEnumerator 신성_슬라임은_가장_많이_다친_다른_아군을_회복하고_적은_공격하지_않는다()
        {
            Component rock = Spawn("Rock");
            Component ice = Spawn("Ice");
            Component holy = Spawn("Holy");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Call(rock, "TakeSpellDamage", 4);
            Call(ice, "TakeSpellDamage", 6);
            Call(holy, "TakeSpellDamage", 7);

            yield return RunAttacks(() => enemy);
            Assert.That(AllyHp(ice), Is.EqualTo(2 + 4), "가장 많이 다친 아군은 자신을 뺀 얼음 슬라임(결손 6)이다");
            Assert.That(AllyHp(rock), Is.EqualTo(12));
            Assert.That(AllyHp(holy), Is.EqualTo(1), "다른 아군이 다쳐 있으면 자신은 회복하지 않는다");
        }

        [UnityTest]
        public IEnumerator 신성_슬라임은_다친_아군이_자신뿐이면_자신을_회복하고_최대_HP를_넘기지_않는다()
        {
            Component holy = Spawn("Holy");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Call(holy, "TakeSpellDamage", 2);

            yield return RunAttacks(() => enemy);
            Assert.That(AllyHp(holy), Is.EqualTo(8));
            Assert.That(EnemyHp(enemy), Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator 아군_공격은_투사체가_모두_도착한_뒤에_끝난다()
        {
            Spawn("Fire");
            Spawn("Ice");
            Component enemy = CreateEnemy(6f, "Holy");
            yield return null;

            yield return RunAttacks(() => enemy);
            Type projectile = TypeOf("Combat.Allies.AllyProjectile");
            Assert.That((int)projectile.GetProperty("ActiveCount").GetValue(null), Is.EqualTo(0));
            Assert.That(EnemyHp(enemy), Is.EqualTo(91), "불 피해 6과 얼음 피해 3이 모두 도착해야 한다");
            Assert.That((int)StatusValue(enemy, "BurnTurns"), Is.EqualTo(0), "뒤에 도착한 얼음이 앞서 걸린 화상을 지운다");
            Assert.That((int)StatusValue(enemy, "Chill"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator 아군_슬라임은_주문의_대상으로_고를_수_있지만_평소에는_선택되지_않는다()
        {
            Component ally = Spawn("Fire");
            yield return null;
            Component selectable = ally.GetComponent(TypeOf("Combat.Enemies.SelectableObject"));
            Assert.That(selectable, Is.Not.Null);
            Assert.That(ally.CompareTag("Ally"), Is.True);
            Assert.That((bool)Call(selectable, "GetSelectable"), Is.False);
        }

        [UnityTest]
        public IEnumerator 신성_주문을_아군에게_쓰면_회복하고_최대_HP를_넘기지_않는다()
        {
            Component ally = Spawn("Fire");
            yield return null;
            Call(ally, "TakeSpellDamage", 3);
            Component spell = SpellOnAlly("Shoot", "Holy", ally);
            Call(spell, "OnTriggerEnter2D", ally.GetComponent<Collider2D>());
            Assert.That(AllyHp(ally), Is.EqualTo(8), "회복은 최대 HP 8에서 멈춰야 한다");
        }

        [UnityTest]
        public IEnumerator 불_주문을_아군에게_쓰면_실제_피해를_준다()
        {
            Component ally = Spawn("Rock");
            yield return null;
            Component spell = SpellOnAlly("Shoot", "Fire", ally);
            Call(spell, "OnTriggerEnter2D", ally.GetComponent<Collider2D>());
            Assert.That(AllyHp(ally), Is.LessThan(16));
        }

        [UnityTest]
        public IEnumerator 폭발_주문은_범위에_닿은_적과_아군을_모두_한_번씩_맞힌다()
        {
            Component target = Spawn("Rock");
            Component other = Spawn("Ice");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Component spell = SpellOnAlly("Explode", "Fire", target);
            foreach (Component unit in new[] { target, other, enemy, target })
                Call(spell, "OnTriggerEnter2D", unit.GetComponentInChildren<Collider2D>());
            int targetHp = AllyHp(target);
            Assert.That(targetHp, Is.LessThan(16), "고른 아군이 맞지 않았다");
            Assert.That(AllyHp(other), Is.LessThan(8), "범위 안의 다른 아군이 맞지 않았다");
            Assert.That(EnemyHp(enemy), Is.LessThan(100), "범위 안의 적이 맞지 않았다");
            Call(spell, "OnTriggerEnter2D", target.GetComponentInChildren<Collider2D>());
            Assert.That(AllyHp(target), Is.EqualTo(targetHp), "같은 아군이 두 번 맞았다");
        }

        [UnityTest]
        public IEnumerator 신성_폭발은_범위에_닿은_적과_아군을_모두_회복한다()
        {
            Component ally = Spawn("Fire");
            Component enemy = CreateEnemy(6f);
            yield return null;
            Call(ally, "TakeSpellDamage", 5);
            Call(enemy, "TakeHit", 30);
            Component spell = SpellOnAlly("Explode", "Holy", ally);
            Call(spell, "OnTriggerEnter2D", ally.GetComponentInChildren<Collider2D>());
            Call(spell, "OnTriggerEnter2D", enemy.GetComponentInChildren<Collider2D>());
            Assert.That(AllyHp(ally), Is.GreaterThan(3), "아군이 회복되지 않았다");
            Assert.That(EnemyHp(enemy), Is.GreaterThan(70), "적이 회복되지 않았다");
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
