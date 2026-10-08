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
    // 밸런스 점검용 자동 전투. 실제 전투 씬에서 카드를 조합하고 대상을 고르고 턴을 넘긴다.
    // 오래 걸리므로 기본 테스트 실행에는 들어가지 않는다. Test Runner에서 직접 고르거나
    // batchmode에서 -testFilter WordVenture.Tests.BalanceSimulation 으로 실행한다.
    // 결과는 프로젝트 루트의 balance-report.jsonl 에 한 줄씩 남는다.
    [Explicit]
    public sealed class BalanceSimulation
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        const int MaxTurns = 40;
        const int CastsPerTurn = 4;
        const int HealBelowHp = 45;
        const int MaxAllies = 2;
        // Explode의 공격 범위 반지름. docs/design/spell-effects/README.md
        const float ExplodeRadius = 4f;
        // BALANCE_STAGES(쉼표 구분)와 BALANCE_RUNS 환경 변수로 지역과 판 수를 바꿀 수 있다.
        static readonly int[] Stages = ReadStages();
        static readonly int RunsPerCase = ReadRuns();

        readonly string[] keys = { "TutorialEnded", "StagePosition", "ActOneDialogueSeen", "ActTwoDialogueSeen" };
        int[] values;
        bool[] existed;
        int oldPosition;
        float oldTimeScale;
        Object affinityTable;

        static int[] ReadStages()
        {
            string value = Environment.GetEnvironmentVariable("BALANCE_STAGES");
            if (string.IsNullOrEmpty(value)) return new[] { 3, 4, 5, 6, 7, 8, 9 };
            return value.Split(',').Select(int.Parse).ToArray();
        }

        static int ReadRuns()
        {
            string value = Environment.GetEnvironmentVariable("BALANCE_RUNS");
            return int.TryParse(value, out int runs) && runs > 0 ? runs : 3;
        }

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, AnyInstance).Invoke(target, args);
        static object Get(object target, string name)
        {
            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(name, AnyInstance);
            return property != null ? property.GetValue(target) : type.GetField(name, AnyInstance).GetValue(target);
        }
        static object StaticGet(string typeName, string name)
        {
            Type type = Runtime(typeName);
            PropertyInfo property = type.GetProperty(name, AnyStatic);
            return property != null ? property.GetValue(null) : type.GetField(name, AnyStatic).GetValue(null);
        }

        static bool InBattle => SceneManager.GetActiveScene().name == "TurnBattleScene";
        static Component Player => (Component)Runtime("Combat.Enemies.Player").GetMethod("PlayerInt").Invoke(null, null);
        static int PlayerHp => (int)Get(Player, "Hp");
        static Component CombineZone => (Component)StaticGet("Combat.UI.CombineZone", "Instance");
        static Component Battle => (Component)StaticGet("Battle.Turns.TurnBattleSystem", "Instance");

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)Runtime("Map.MapMove").GetField("StagePosition").GetValue(null);
            oldTimeScale = Time.timeScale;
            affinityTable = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/Magic Affinity Table.asset");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = oldTimeScale;
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            // 테스트 에디터 종료 시 SaveLoadController가 복원한 저장값을 덮어쓰지 않게 한다.
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            Component singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton")) as Component;
            if (singleton != null) Object.Destroy(singleton.gameObject);
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        [UnityTest, Timeout(7200000)]
        public IEnumerator 일부_후반과_이부_지역을_자동으로_싸워_결과를_남긴다()
        {
            string reportPath = Path.Combine(Application.dataPath, "..", "balance-report.jsonl");
            File.WriteAllText(reportPath, string.Empty);
            foreach (int stage in Stages)
            {
                foreach (bool useSpawn in new[] { true, false })
                {
                    if (useSpawn && stage < 5) continue;
                    for (int run = 0; run < RunsPerCase; run++)
                    {
                        var result = new BattleResult { stage = stage, useSpawn = useSpawn, seed = 1000 * stage + run };
                        yield return PlayBattle(result);
                        File.AppendAllText(reportPath, JsonUtility.ToJson(result) + "\n");
                    }
                }
            }
            Assert.Pass();
        }

        [Serializable]
        sealed class BattleResult
        {
            public int stage;
            public bool useSpawn;
            public int seed;
            public string outcome;
            public int turns;
            public int damageTaken;
            public int healed;
            public int finalHp;
            public int spawnsCast;
            public int spellsCast;
        }

        IEnumerator PlayBattle(BattleResult result)
        {
            UnityEngine.Random.InitState(result.seed);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            // 지역 대화는 자동 전투에서 넘기므로 이미 본 것으로 둔다.
            PlayerPrefs.SetInt("ActOneDialogueSeen", int.MaxValue);
            PlayerPrefs.SetInt("ActTwoDialogueSeen", int.MaxValue);
            var singleton = Object.FindObjectOfType(Runtime("Combat.Stage.StageDataSingleton"));
            if (singleton == null) singleton = new GameObject("StageData").AddComponent(Runtime("Combat.Stage.StageDataSingleton"));
            Runtime("Combat.Stage.StageDataSingleton").GetField("stagePosition").SetValue(singleton, result.stage);
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, result.stage);
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(0.5f);
            Time.timeScale = 3f;

            int lastHp = PlayerHp;
            for (result.turns = 1; result.turns <= MaxTurns && InBattle; result.turns++)
            {
                yield return SkipDialogue();
                for (int cast = 0; cast < CastsPerTurn && InBattle; cast++)
                {
                    bool casted = false;
                    yield return CastBestAction(result, done => casted = done);
                    if (!casted) break;
                    TrackHp(result, ref lastHp);
                }
                if (!InBattle) break;
                Call(Battle, "TurnEndButton");
                yield return WaitForPlayerTurn();
                TrackHp(result, ref lastHp);
            }

            string scene = SceneManager.GetActiveScene().name;
            // 장의 마지막 지역을 이기면 클리어 화면이 곧바로 엔딩 씬으로 넘어간다.
            result.outcome = scene == "GameClearScene" || scene == "EndingScene" ? "win" : scene == "GameOverScene" ? "lose" : "timeout";
            result.finalHp = result.outcome == "lose" ? 0 : lastHp;
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
        }

        void TrackHp(BattleResult result, ref int lastHp)
        {
            if (!InBattle || Player == null) return;
            int hp = PlayerHp;
            if (hp < lastHp) result.damageTaken += lastHp - hp;
            else result.healed += hp - lastHp;
            lastHp = hp;
        }

        static IEnumerator SkipDialogue()
        {
            for (int i = 0; i < 200; i++)
            {
                Component view = Object.FindObjectOfType(Runtime("Story.StageDialogueView")) as Component;
                if (view == null) yield break;
                Call(view, "Advance");
                yield return null;
            }
        }

        static IEnumerator WaitForPlayerTurn()
        {
            object playerTurn = StaticGet("Battle.Turns.TurnBattleSystem", "PlayerTurn");
            float deadline = Time.realtimeSinceStartup + 20f;
            yield return null;
            while (InBattle && Time.realtimeSinceStartup < deadline)
            {
                yield return SkipDialogue();
                if (!InBattle || Battle == null) yield break;
                bool ending = (bool)Get(Battle, "IsEndingPlayerTurn");
                if (!ending && Get(Battle, "currentTurn") == playerTurn) break;
                yield return null;
            }
            // 적 이동과 상태 처리가 끝나도록 잠시 기다린다.
            yield return new WaitForSeconds(0.3f);
        }

        struct Candidate
        {
            public Component spell;
            public Component element;
            public Component target;
            public float value;
            public bool isSpawn;
        }

        IEnumerator CastBestAction(BattleResult result, Action<bool> done)
        {
            Candidate? best = ChooseAction(result.useSpawn);
            if (best == null)
            {
                done(false);
                yield break;
            }

            Candidate action = best.Value;
            Component zone = CombineZone;
            // 앞선 시도가 조합창에 남긴 카드가 있으면 새 카드가 들어가지 않는다.
            ((IList)Get(zone, "spellCards")).Clear();
            ((IList)Get(zone, "magicTypeCards")).Clear();
            Call(zone, "AddCard", action.spell.gameObject);
            Call(zone, "AddCard", action.element.gameObject);
            Call(zone, "OnButtonClick");
            if (!(bool)Get(zone, "IsCasting"))
            {
                done(false);
                yield break;
            }
            if (!action.isSpawn) Call(zone, "SetTarget", action.target.GetComponent(Runtime("Combat.Enemies.SelectableObject")));

            float deadline = Time.realtimeSinceStartup + 10f;
            while (InBattle && Time.realtimeSinceStartup < deadline)
            {
                bool busy = (bool)Get(zone, "IsCasting") || (bool)Get(zone, "IsAwaitingTarget")
                    || (bool)StaticGet("Combat.Spells.SpellObj", "HasActiveSpells");
                if (!busy) break;
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            if (action.isSpawn) result.spawnsCast++;
            else result.spellsCast++;
            done(true);
        }

        Candidate? ChooseAction(bool useSpawn)
        {
            if (!InBattle || CombineZone == null) return null;
            var hand = ((IEnumerable)Get(StaticGet("Cards.CardManager", "Inst"), "HandCards")).Cast<Component>()
                .Where(card => card != null && card.gameObject.activeInHierarchy).ToList();
            var spells = hand.Where(card => card.CompareTag("Spell")).ToList();
            var elements = hand.Where(card => card.CompareTag("MagicType")).ToList();
            if (spells.Count == 0 || elements.Count == 0) return null;

            Type enemyType = Runtime("Combat.Enemies.Enemy");
            var enemies = Object.FindObjectsOfType(enemyType).Cast<Component>()
                .Where(enemy => (bool)Get(enemy, "IsAlive")).OrderBy(enemy => enemy.transform.position.x).ToList();
            if (enemies.Count == 0) return null;

            Component holy = elements.FirstOrDefault(card => CardType(card) == "Holy");
            Component healSpell = spells.FirstOrDefault(card => CardType(card) == "Shoot" || CardType(card) == "Drop");
            if (PlayerHp <= HealBelowHp && holy != null && healSpell != null)
                return new Candidate { spell = healSpell, element = holy, target = Player, value = 1 };

            if (useSpawn)
            {
                Component spawn = spells.FirstOrDefault(card => CardType(card) == "Spawn");
                object formation = StaticGet("Combat.Allies.AllyFormation", "Current");
                int allies = formation == null ? 0 : ((ICollection)Get(formation, "Allies")).Count;
                if (spawn != null && allies < MaxAllies)
                {
                    Component bestElement = elements.Where(card => CardType(card) != "Holy")
                        .OrderByDescending(card => Affinity(card, enemies[0])).FirstOrDefault();
                    if (bestElement != null) return new Candidate { spell = spawn, element = bestElement, isSpawn = true, value = 1 };
                }
            }

            Candidate? best = null;
            foreach (Component spell in spells)
            {
                string spellName = CardType(spell);
                if (spellName == "Spawn") continue;
                float baseDamage = (float)Runtime("Combat.Spells.SpellObj").GetMethod("GetCurrentBaseDamage")
                    .Invoke(null, new[] { Enum.Parse(Runtime("Cards.MagicType"), spellName) });
                foreach (Component element in elements)
                {
                    foreach (Component target in spellName == "Shoot" ? enemies.Take(1) : enemies)
                    {
                        float value = 0f;
                        IEnumerable<Component> hit = spellName == "Explode"
                            ? enemies.Where(e => Mathf.Abs(e.transform.position.x - target.transform.position.x) <= ExplodeRadius)
                            : new[] { target };
                        foreach (Component enemy in hit)
                        {
                            float damage = baseDamage * Affinity(element, enemy);
                            value += Mathf.Min(damage, (int)Get(enemy, "Hp"));
                        }
                        if (value > 0f && (best == null || value > best.Value.value))
                            best = new Candidate { spell = spell, element = element, target = target, value = value };
                    }
                }
            }
            return best;
        }

        static string CardType(Component card) => Get(card, "cardType").ToString();

        float Affinity(Component element, Component enemy)
        {
            object elementType = Get(element, "cardType");
            object enemyElement = Get(enemy, "enemyType");
            return (float)Call(affinityTable, "GetAffinity", elementType, enemyElement);
        }
    }
}
#endif
