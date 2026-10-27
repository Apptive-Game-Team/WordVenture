#if UNITY_EDITOR
using System;
using System.Collections;
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
    // 전투 씬에서 적 슬라임에 마우스를 올렸을 때 뜨는 정보 창을 확인한다. 마우스 대신 Show/Hide 를 직접 부른다.
    public sealed class EnemyHoverInfoPlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const string SlimePrefab = "Assets/Prefabs/Combat/Enemies/Slime/MeleeStone.prefab";
        const string AffinityTablePath = "Assets/ScriptableObjects/Combat/Magic Affinity Table.asset";
        readonly string[] keys = { "TutorialEnded", "StagePosition", "SeenSlimes0", "SeenSlimes1", "Language" };
        readonly string[] spellElements = { "Fire", "Ice", "Rock", "Lightning", "Holy" };
        int[] values;
        bool[] existed;
        int oldPosition;
        bool hadSingleton;
        int oldStage;
        GameObject slime;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object Magic(string name) => Enum.Parse(Runtime("Cards.MagicType"), name);
        static Type InfoType => Runtime("Combat.UI.EnemyHoverInfo");
        static Component Info => (Component)InfoType.GetProperty("Current").GetValue(null);
        static bool IsVisible => (bool)InfoType.GetProperty("IsVisible").GetValue(Info);
        static string PanelText => (string)InfoType.GetProperty("Text").GetValue(Info);
        static PropertyInfo Locked => Runtime("Core.InteractionLock").GetProperty("IsLocked");

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
            SharedDialoguePlayTests.SetLanguage("Korean");
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return new WaitForSeconds(0.3f);
            foreach (Object enemy in Object.FindObjectsOfType(Runtime("Combat.Enemies.Enemy")))
                ((Component)enemy).gameObject.SetActive(false);
            Locked.SetValue(null, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Locked.SetValue(null, false);
            if (slime != null) Object.Destroy(slime);
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
            SharedDialoguePlayTests.ResetLanguage();
            Runtime("Map.MapMove").GetField("StagePosition").SetValue(null, oldPosition);
        }

        // 도감과 같은 상성표에서 약점이 하나라도 있는 속성을 찾는다.
        static string ElementWithWeakness()
        {
            Object table = AssetDatabase.LoadAssetAtPath<Object>(AffinityTablePath);
            MethodInfo get = table.GetType().GetMethod("GetAffinity");
            foreach (string defense in new[] { "Fire", "Ice", "Rock", "Lightning", "Holy", "Undead" })
                foreach (string spell in new[] { "Fire", "Ice", "Rock", "Lightning", "Holy" })
                    if ((float)get.Invoke(table, new[] { Magic(spell), Magic(defense) }) > 1f) return defense;
            Assert.Fail("약점이 있는 속성이 상성표에 없다");
            return null;
        }

        Component CreateEnemy(string element, string displayName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefab);
            slime = Object.Instantiate(prefab, new Vector3(4f, -3f, 0f), Quaternion.identity);
            Component enemy = slime.GetComponent(Runtime("Combat.Enemies.Enemy"));
            object data = Activator.CreateInstance(Runtime("Combat.Enemies.EnemyData"));
            Type dataType = data.GetType();
            dataType.GetField("displayName").SetValue(data, displayName);
            dataType.GetField("maxHp").SetValue(data, 12);
            dataType.GetField("moveDistance").SetValue(data, 5f);
            dataType.GetField("attackRange").SetValue(data, 3f);
            dataType.GetField("damage").SetValue(data, 3);
            dataType.GetField("type").SetValue(data, Magic(element));
            enemy.GetType().GetMethod("InitEnemyData").Invoke(enemy, new[] { data });
            return enemy;
        }

        [UnityTest]
        public IEnumerator 마우스를_올리면_이름_체력_공격력과_약점을_보여_주고_벗어나면_숨긴다()
        {
            Component enemy = CreateEnemy(ElementWithWeakness(), "돌 슬라임");
            yield return null;
            Assert.That(Info, Is.Not.Null);
            Assert.That(IsVisible, Is.False);

            InfoType.GetMethod("Show").Invoke(Info, new object[] { enemy });
            yield return null;
            Assert.That(IsVisible, Is.True);
            Assert.That(PanelText, Does.Contain("돌 슬라임"));
            Assert.That(PanelText, Does.Contain("체력 12 / 12"));
            Assert.That(PanelText, Does.Contain("공격력 3"));
            Assert.That(PanelText, Does.Contain("약점"));
            SharedDialoguePlayTests.Capture("enemy-hover-info.png");

            InfoType.GetMethod("Hide").Invoke(Info, null);
            yield return null;
            Assert.That(IsVisible, Is.False);
        }

        [UnityTest]
        public IEnumerator 체력이_줄면_보이는_동안_글자를_갱신한다()
        {
            Component enemy = CreateEnemy("Fire", "돌 슬라임");
            InfoType.GetMethod("Show").Invoke(Info, new object[] { enemy });
            yield return null;
            enemy.GetType().GetMethod("TakeHit").Invoke(enemy, new object[] { 5 });
            yield return null;
            Assert.That(PanelText, Does.Contain("체력 7 / 12"));
        }

        [UnityTest]
        public IEnumerator 대화나_튜토리얼이_입력을_잡고_있으면_띄우지_않는다()
        {
            Component enemy = CreateEnemy("Fire", "돌 슬라임");
            Locked.SetValue(null, true);
            InfoType.GetMethod("Show").Invoke(Info, new object[] { enemy });
            yield return null;
            Assert.That(IsVisible, Is.False);
            Locked.SetValue(null, false);
            yield return null;
            Assert.That(IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator 적이_쓰러지면_정보_창을_숨긴다()
        {
            Component enemy = CreateEnemy("Fire", "돌 슬라임");
            InfoType.GetMethod("Show").Invoke(Info, new object[] { enemy });
            yield return null;
            Assert.That(IsVisible, Is.True);
            enemy.gameObject.SetActive(false);
            yield return null;
            Assert.That(IsVisible, Is.False);
        }
    }
}
#endif
