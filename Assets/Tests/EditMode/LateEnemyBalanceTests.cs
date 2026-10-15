using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class LateEnemyBalanceTests
    {
        const string DataPath = "Assets/ScriptableObjects/Combat/EnemyDataContainer.asset";
        const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static SerializedProperty Entry(int id) => new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>(DataPath))
            .FindProperty("enemyDatas").GetArrayElementAtIndex(id);
        static int Hp(int id) => Entry(id).FindPropertyRelative("maxHp").intValue;
        static int Damage(int id) => Entry(id).FindPropertyRelative("damage").intValue;
        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);

        [Test]
        public void 후반_원거리와_지역_보스는_고정된_지역_순서로_체력이_증가한다()
        {
            int[] ranged = { 16, 19, 23, 28 };
            int[] bosses = { 30, 31, 32, 33 };
            for (int i = 1; i < ranged.Length; i++) Assert.That(Hp(ranged[i]), Is.GreaterThan(Hp(ranged[i - 1])));
            for (int i = 1; i < bosses.Length; i++) Assert.That(Hp(bosses[i]), Is.GreaterThan(Hp(bosses[i - 1])));
            foreach (int id in ranged)
                Assert.That(Hp(id), Is.InRange(40, 100), "공격 강화 4~6단계의 중립 주문에 2~4회 이내로 대응하는 일반 적");
            Assert.That(Hp(35) + Hp(29), Is.InRange(350, 420), "최종전의 두 페이즈 합계가 한 지역 보스보다 충분히 길어야 한다");
        }

        [Test]
        public void 방패_돌진_곡사의_역할_차이와_약한_소환체를_유지한다()
        {
            Assert.That(Hp(22), Is.GreaterThan(Hp(21)));
            Assert.That(Hp(21), Is.GreaterThan(Hp(20)));
            Assert.That(Damage(22), Is.LessThan(Damage(20)));
            Assert.That(Hp(25), Is.LessThanOrEqualTo(30), "작은 분열체는 공격 강화 4단계 중립 주문 한 번으로 정리할 수 있다");
            Assert.That(Hp(34), Is.LessThanOrEqualTo(30), "눈덩이에 한 주문을 쓸지 본체를 공격할지 선택할 여지가 있어야 한다");
            Assert.That(Damage(26), Is.GreaterThan(Damage(28)), "일회성 폭발의 위험은 일반 원거리 공격보다 커야 한다");
        }

        [Test]
        public void 공격_강화_4단계와_6단계에서_보스_공략_횟수가_남고_성장_효과도_유지된다()
        {
            foreach (int id in new[] { 30, 31, 32, 33, 35, 29 })
            {
                int moderate = Mathf.CeilToInt(Hp(id) / 30f);
                int high = Mathf.CeilToInt(Hp(id) / 40f);
                Assert.That(high, Is.GreaterThanOrEqualTo(3), "중립 주문 한두 번으로 보스 페이즈가 사라지지 않아야 한다");
                Assert.That(high, Is.LessThan(moderate), "추가 공격 강화로 필요한 중립 주문 횟수가 줄어야 한다");
            }
        }

        [Test]
        public void 적_초기화는_플레이어의_실제_강화_수치를_따라_증폭하지_않는다()
        {
            string[] keys = { "UpgradeLevelAttack", "UpgradeLevelMaxHp" };
            bool[] existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            int[] previous = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            GameObject instance = null;
            try
            {
                Object container = AssetDatabase.LoadAssetAtPath<Object>(DataPath);
                Type enemyType = Runtime("Combat.Enemies.Enemy");
                foreach (int upgrade in new[] { 0, 6 })
                {
                    foreach (string key in keys) PlayerPrefs.SetInt(key, upgrade);
                    foreach (int id in new[] { 16, 28, 30, 33, 35, 29 })
                    {
                        object data = container.GetType().GetMethod("GetGearData").Invoke(container, new object[] { id });
                        var prefab = (GameObject)data.GetType().GetField("prefab").GetValue(data);
                        instance = Object.Instantiate(prefab);
                        Component enemy = instance.GetComponent(enemyType);
                        enemyType.GetMethod("InitEnemyData").Invoke(enemy, new[] { data });
                        Assert.That(enemyType.GetField("Hp", Instance).GetValue(enemy), Is.EqualTo(Hp(id)));
                        Assert.That(enemyType.GetField("Damage", Instance).GetValue(enemy), Is.EqualTo(Damage(id)));
                        Object.DestroyImmediate(instance);
                        instance = null;
                    }
                }
            }
            finally
            {
                if (instance != null) Object.DestroyImmediate(instance);
                for (int i = 0; i < keys.Length; i++)
                    if (existed[i]) PlayerPrefs.SetInt(keys[i], previous[i]); else PlayerPrefs.DeleteKey(keys[i]);
            }
        }
    }
}
