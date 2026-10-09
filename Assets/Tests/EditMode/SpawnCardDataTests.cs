using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WordVenture.Tests
{
    /// <summary>
    /// Spawn 카드와 아군 슬라임 prefab의 데이터를 검사한다.
    /// </summary>
    public sealed class SpawnCardDataTests
    {
        const string WordListPath = "Assets/ScriptableObjects/Cards/WordList.asset";
        const string AllySlimePrefabPath = "Assets/Resources/Combat/AllySlime.prefab";

        static Type TypeOf(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        // 카드 데이터와 적 데이터가 MagicType을 정수로 저장한다. 기존 값이 밀리면
        // 저장된 카드와 적의 속성이 조용히 바뀐다.
        [TestCase("Shoot", 0)]
        [TestCase("Explode", 1)]
        [TestCase("Drop", 2)]
        [TestCase("Holy", 3)]
        [TestCase("Fire", 4)]
        [TestCase("Ice", 5)]
        [TestCase("Rock", 6)]
        [TestCase("Lightning", 7)]
        [TestCase("Undead", 8)]
        [TestCase("Spawn", 9)]
        public void MagicType_값이_저장된_정수와_같다(string name, int value)
        {
            Assert.That((int)Enum.Parse(TypeOf("Cards.MagicType"), name), Is.EqualTo(value));
        }

        [Test]
        public void 단어_목록에_Spawn_주문_카드가_있고_1부에서는_나오지_않는다()
        {
            SerializedProperty words = ProjectAssets.Load(WordListPath).FindProperty("words");
            SerializedProperty spawn = null;
            for (int i = 0; i < words.arraySize; i++)
            {
                SerializedProperty word = words.GetArrayElementAtIndex(i);
                if (word.FindPropertyRelative("magicType").intValue == 9) spawn = word;
            }

            Assert.That(spawn, Is.Not.Null, "WordList에 Spawn 단어가 없다");
            Assert.That(spawn.FindPropertyRelative("name").stringValue, Is.EqualTo("Spawn"));
            Assert.That(spawn.FindPropertyRelative("tag").stringValue, Is.EqualTo("Spell"),
                "Spawn은 주문 칸에 올라가야 한다");
            Assert.That(spawn.FindPropertyRelative("percent").intValue, Is.EqualTo(0),
                "1부 손패에 Spawn이 섞이면 안 된다. 출현 확률은 CardManager가 스테이지 5부터 올린다");
        }

        [Test]
        public void 아군_슬라임_prefab이_적으로_취급되지_않는다()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AllySlimePrefabPath);
            Assert.That(prefab, Is.Not.Null, AllySlimePrefabPath + " 을(를) 로드하지 못했다");

            Assert.That(prefab.GetComponent(TypeOf("Combat.Allies.AllySlime")), Is.Not.Null);
            Assert.That(prefab.GetComponent(TypeOf("Combat.Enemies.Enemy")), Is.Null,
                "Enemy가 붙으면 적 턴에 행동하고 주문 대상이 된다");
            Assert.That(prefab.GetComponent(TypeOf("Combat.Enemies.SelectableObject")), Is.Null,
                "아군 슬라임은 주문 대상으로 고를 수 없다");
            Assert.That(prefab.CompareTag("Enemy"), Is.False);
            Assert.That(prefab.transform.localScale.x, Is.GreaterThan(0), "아군 슬라임은 적이 있는 오른쪽을 본다");

            Collider2D collider = prefab.GetComponent<Collider2D>();
            Assert.That(collider, Is.Not.Null, "적의 원거리 탄이 아군 슬라임에 닿지 못한다");
            Assert.That(collider.isTrigger, Is.True);

            SerializedObject animator = ProjectAssets.FindComponentWithProperty(prefab, "sprites");
            Assert.That(animator, Is.Not.Null, "SlimeAnimator가 없다");
            Assert.That(animator.FindProperty("sprites").arraySize, Is.GreaterThanOrEqualTo(6),
                "SlimeAnimator는 대기·공격·피격·사망 프레임 6장 이상을 쓴다");
        }
    }
}
