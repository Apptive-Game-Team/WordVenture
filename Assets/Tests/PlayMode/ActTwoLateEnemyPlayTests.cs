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

        static string LastBurst(Component enemy)
        {
            Component vfx = enemy.GetComponent(Runtime("Combat.Enemies.ReactionBurstVfx"));
            return Property(vfx, "LastBurst")?.ToString();
        }

        static Component[] Blasts() => Object.FindObjectsOfType(Runtime("Combat.Enemies.BlastVfx"))
            .Cast<Component>().ToArray();

        [UnityTest]
        public IEnumerator 폭발_슬라임이_스스로_터지면_자식이_아닌_폭발_효과가_끝까지_재생된다()
        {
            Component bomber = CreateEnemy("ExplodeSlime", 3f, 25, 15, "Lightning");
            yield return null;
            Call(bomber, "PlayTurnAction", 2f);
            Assert.That(Blasts(), Is.Empty, "예고하는 동안에는 터지지 않는다");

            Call(bomber, "PlayTurnAction", 2f);
            Component[] blasts = Blasts();
            Assert.That(blasts.Length, Is.EqualTo(1));
            Assert.That(blasts[0].transform.parent, Is.Null, "슬라임이 꺼져도 남도록 자식이 아니다");
            Assert.That(blasts[0].transform.position.x, Is.EqualTo(bomber.transform.position.x).Within(0.01f));

            yield return new WaitForSeconds(0.4f);
            Assert.That(bomber.gameObject.activeSelf, Is.False);
            Assert.That(Blasts().Length, Is.EqualTo(1), "슬라임이 꺼진 뒤에도 재생 중이다");
            yield return new WaitForSeconds(0.4f);
            Assert.That(Blasts(), Is.Empty, "재생이 끝나면 스스로 지운다");
        }

        [UnityTest]
        public IEnumerator 예고한_폭발_슬라임을_쓰러뜨리면_폭발_효과가_나오고_예고_전에는_나오지_않는다()
        {
            Component calm = CreateEnemy("ExplodeSlime", 3f, 25, 15, "Lightning");
            Component primed = CreateEnemy("ExplodeSlime", 6f, 25, 15, "Lightning");
            yield return null;

            Call(calm, "Kill");
            Assert.That(Blasts(), Is.Empty, "예고하지 않은 슬라임은 터지지 않는다");

            Call(primed, "PlayTurnAction", 2f);
            Call(primed, "Kill");
            Assert.That(Blasts().Length, Is.EqualTo(1));
        }

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
        public IEnumerator 분열_슬라임이_사거리_끝에_멈추면_다음_턴에_워드를_때린다()
        {
            float frontLineX = (float)FormationType.GetMethod("FrontLineX").Invoke(null, new object[] { Player.transform.position.x });
            Component split = CreateEnemy("SplitSlime", frontLineX + 3f, 40, 10, "Fire");
            yield return null;
            int before = PlayerHp;

            Call(split, "PlayTurnAction", FrontLineDistance(split));
            yield return null;

            Assert.That(PlayerHp, Is.LessThan(before), "사거리 3에 선 근접 적이 공격 동작만 하고 피해를 주지 않는다");
        }

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
            yield return null;
            string frame = bomber.GetComponent<SpriteRenderer>().sprite.name;
            Assert.That(frame, Is.EqualTo("ExplodeSlime_09").Or.EqualTo("ExplodeSlime_10"), "예고하는 동안 예고 프레임을 보여 준다");

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
            Assert.That(LastBurst(wounded), Is.EqualTo("Heal"), "회복은 글자 대신 효과로 보인다");
            Assert.That(LastBurst(scratched), Is.Null);
        }

        [UnityTest]
        public IEnumerator 세계수는_턴별_마지막_속성_마법을_저장하고_3턴에_연속_시전한다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6f, 100, 12, "Undead");
            yield return null;
            int hp = PlayerHp;
            Call(boss, "TakeSpellHit", Magic("Fire"), Magic("Shoot"), 1f, 1f);
            Call(boss, "TakeSpellHit", Magic("Ice"), Magic("Drop"), 1f, 1f);
            Call(boss, "TakeSpellHit", Magic("Fire"), Magic("Spawn"), 1f, 1f);
            Assert.That(Property(boss, "RememberedSpell"), Is.EqualTo(Magic("Drop")));
            Assert.That(Property(boss, "RememberedElement"), Is.EqualTo(Magic("Ice")));
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            var stored = (IList)Property(boss, "StoredSpells");
            Assert.That(stored.Count, Is.EqualTo(1));
            Assert.That(stored[0].GetType().GetField("Element").GetValue(stored[0]), Is.EqualTo(Magic("Ice")));
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            Assert.That(stored.Count, Is.EqualTo(1), "무주문 턴에도 저장분을 보존한다");
            Assert.That(PlayerHp, Is.EqualTo(hp), "첫 두 턴에는 축적한다");
            Call(boss, "TakeSpellHit", Magic("Lightning"), Magic("Explode"), 1f, 1f);
            Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            Assert.That(Property(boss, "IsCasting"), Is.True);
            float deadline = Time.realtimeSinceStartup + 6;
            while ((bool)Property(boss, "IsCasting") && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(Property(boss, "IsCasting"), Is.False);
            Assert.That(PlayerHp, Is.LessThan(hp));
            Assert.That(stored.Count, Is.Zero);
            Assert.That(Property(boss, "ChargeTurns"), Is.EqualTo(0));
            Component status = Player.GetComponent(Runtime("Combat.Allies.FriendlyElementalStatus"));
            object elemental = Property(status, "Status");
            Assert.That(elemental.GetType().GetProperty("ShockTurns").GetValue(elemental), Is.EqualTo(2));
            Assert.That(AliveEnemiesNamed("SplitSlimeSmall").Length, Is.Zero);
        }

        [UnityTest]
        public IEnumerator 세계수_연속_폭발은_약화를_적용하고_아군이_있으면_워드를_때리지_않는다()
        {
            Component front = SpawnAlly("Fire"), middle = SpawnAlly("Rock"), rear = SpawnAlly("Ice");
            Component boss = CreateEnemy("SurgeBoss", 6f, 100, 12, "Undead");
            yield return null;
            int hp = PlayerHp;
            Call(boss, "TakeSpellHit", Magic("Lightning"), Magic("Shoot"), 1f, 1f);
            Call(boss, "TakeSpellHit", Magic("Ice"), Magic("Shoot"), 1f, 1f);
            Call(boss, "Remember", Magic("Explode"));
            for (int i = 0; i < 3; i++) Call(boss, "PlayTurnAction", FrontLineDistance(boss));
            float deadline = Time.realtimeSinceStartup + 4;
            while ((bool)Property(boss, "IsCasting") && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(AllyHp(front), Is.EqualTo(6));
            Assert.That(AllyHp(rear), Is.EqualTo(6), "약화된 공격력 8의 절반에 폭발 배율을 적용해 2 피해");
            Assert.That(AllyHp(middle), Is.EqualTo(15), "바위는 2 피해를 30% 줄인 1을 받는다");
            Assert.That(PlayerHp, Is.EqualTo(hp));
        }

        [UnityTest]
        public IEnumerator 세계수는_같은턴의_새_주문만_덮어쓰고_아군_공격은_기억하지_않는다()
        {
            Component boss = CreateEnemy("SurgeBoss", 6f, 100, 12, "Undead");
            yield return null;
            Call(boss, "RememberElement", Magic("Drop"), Magic("Fire"));
            Assert.That(Property(boss, "HasMarker"), Is.True);
            Call(boss, "RememberElement", Magic("Shoot"), Magic("Ice"));
            Assert.That(Property(boss, "HasMarker"), Is.False);
            Call(boss, "TakeSpellHit", Magic("Fire"), Magic("Spawn"), 4f, 1f);
            Assert.That(Property(boss, "RememberedSpell"), Is.EqualTo(Magic("Shoot")));
            Assert.That(Property(boss, "RememberedElement"), Is.EqualTo(Magic("Ice")));
        }

        [UnityTest]
        public IEnumerator 새_보스의_발_피벗에서도_실제_Shoot와_Drop이_몸에_명중한다()
        {
            var wave = (MonoBehaviour)Object.FindObjectOfType(Runtime("Combat.Enemies.BattleWaveController"));
            wave.StopAllCoroutines();
            Component zone = (Component)Runtime("Combat.UI.CombineZone").GetField("Instance").GetValue(null);
            Object affinity = AssetDatabase.LoadAssetAtPath<Object>(AffinityTablePath);
            foreach (string prefab in new[] { "SnowBoss", "ThunderBoss", "FurnaceBoss", "GraftBoss", "SurgeBoss", "TreeShellBoss" })
            {
                Component boss = CreateEnemy(prefab, 6f, 1000, 10, "Undead");
                Component selectable = boss.GetComponent(Runtime("Combat.Enemies.SelectableObject"));
                yield return null;
                foreach (string spell in new[] { "Shoot", "Drop" })
                {
                    int before = EnemyHp(boss);
                    GameObject runner = (GameObject)zone.GetType().GetField(spell.ToLowerInvariant()).GetValue(zone);
                    Call(runner.GetComponent(Runtime("Combat.Spells." + spell)), "Run", Magic("Fire"), selectable, affinity);
                    yield return new WaitForSeconds(2f);
                    Assert.That(EnemyHp(boss), Is.LessThan(before), prefab + " " + spell + "이 실제 충돌체에 명중해야 한다");
                    foreach (Component projectile in Object.FindObjectsOfType(Runtime("Combat.Spells.SpellObj"))) Object.Destroy(projectile.gameObject);
                    yield return null;
                }
                boss.gameObject.SetActive(false);
            }
        }
    }
}
#endif
