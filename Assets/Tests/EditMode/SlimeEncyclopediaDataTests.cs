using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WordVenture.Tests
{
    /// <summary>
    /// 슬라임 도감이 읽는 이름, 공격 방식, 그림이 전투에 나오는 적마다 있는지 검사한다.
    /// </summary>
    public sealed class SlimeEncyclopediaDataTests
    {
        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        static IReadOnlyDictionary<string, string> EnglishTable() =>
            (IReadOnlyDictionary<string, string>)RuntimeType("Core.Localization")
                .GetProperty("EnglishTable").GetValue(null);

        static SerializedProperty Enemies() =>
            ProjectAssets.Load(ProjectAssets.EnemyDataContainerPath).FindProperty("enemyDatas");

        static string DisplayName(SerializedProperty enemy) => enemy.FindPropertyRelative("displayName").stringValue;

        // 웨이브에 적힌 적과, 분열 슬라임·최종 보스가 웨이브 도중에 부르는 적.
        static HashSet<int> EnemyIdsInBattle()
        {
            var ids = new HashSet<int>();
            foreach (string path in ProjectAssets.WaveDataPaths())
            {
                SerializedProperty waves = ProjectAssets.Load(path).FindProperty("battleWaveDatas");
                for (int wave = 0; wave < waves.arraySize; wave++)
                {
                    SerializedProperty spawns = waves.GetArrayElementAtIndex(wave).FindPropertyRelative("enemySpawnDatasInWave");
                    for (int i = 0; i < spawns.arraySize; i++)
                        ids.Add(spawns.GetArrayElementAtIndex(i).FindPropertyRelative("enemyId").intValue);
                }
            }

            SerializedProperty enemies = Enemies();
            for (int i = 0; i < enemies.arraySize; i++)
            {
                var prefab = (GameObject)enemies.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue;
                foreach (string field in new[] { "childEnemyId", "splitEnemyId" })
                {
                    SerializedObject summoner = ProjectAssets.FindComponentWithProperty(prefab, field);
                    if (summoner != null) ids.Add(summoner.FindProperty(field).intValue);
                }
            }
            return ids;
        }

        [Test]
        public void 전투에_나오는_적은_모두_도감_이름이_있다()
        {
            HashSet<int> inBattle = EnemyIdsInBattle();
            SerializedProperty enemies = Enemies();
            var missing = new List<string>();
            for (int i = 0; i < enemies.arraySize; i++)
            {
                SerializedProperty enemy = enemies.GetArrayElementAtIndex(i);
                int id = enemy.FindPropertyRelative("id").intValue;
                if (inBattle.Contains(id) && string.IsNullOrEmpty(DisplayName(enemy)))
                    missing.Add(id + " " + enemy.FindPropertyRelative("name").stringValue);
            }
            Assert.That(inBattle, Is.Not.Empty);
            Assert.That(missing, Is.Empty, "도감에 실리지 않아 만나도 기록을 볼 수 없다");
        }

        [Test]
        public void 도감에_실린_적은_영어_이름과_공격_방식과_그림이_있다()
        {
            IReadOnlyDictionary<string, string> english = EnglishTable();
            Type enemyType = RuntimeType("Combat.Enemies.Enemy");
            Type animatorType = RuntimeType("Combat.Enemies.SlimeAnimator");
            SerializedProperty enemies = Enemies();
            var problems = new List<string>();
            var names = new List<string>();
            for (int i = 0; i < enemies.arraySize; i++)
            {
                SerializedProperty enemy = enemies.GetArrayElementAtIndex(i);
                string name = DisplayName(enemy);
                if (string.IsNullOrEmpty(name)) continue;
                names.Add(name);
                string label = "enemyDatas[" + i + "] " + name;
                var prefab = (GameObject)enemy.FindPropertyRelative("prefab").objectReferenceValue;

                if (!english.ContainsKey(name)) problems.Add(label + ": 영어 이름이 없다");
                Component behaviour = prefab.GetComponent(enemyType);
                string attack = behaviour == null ? null
                    : (string)enemyType.GetProperty("AttackDescription").GetValue(behaviour);
                if (string.IsNullOrEmpty(attack)) problems.Add(label + ": 공격 방식 설명이 없다");
                else if (!english.ContainsKey(attack)) problems.Add(label + ": 공격 방식의 영어 번역이 없다");
                Component animator = prefab.GetComponent(animatorType);
                if (animator == null || animatorType.GetProperty("FirstFrame").GetValue(animator) == null)
                    problems.Add(label + ": 도감 그림(SlimeAnimator 첫 프레임)이 없다");
            }
            Assert.That(names, Is.Unique, "도감 이름이 겹친다");
            Assert.That(problems, Is.Empty);
        }
    }
}
