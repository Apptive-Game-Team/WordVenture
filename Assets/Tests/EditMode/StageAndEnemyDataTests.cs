using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace WordVenture.Tests
{
    /// <summary>
    /// 스테이지와 적 데이터가 서로 끊긴 참조 없이 이어져 있는지 검사한다.
    /// </summary>
    public sealed class StageAndEnemyDataTests
    {
        static IEnumerable<string> StagePaths()
        {
            return ProjectAssets.StagePaths();
        }

        [Test, TestCaseSource(nameof(StagePaths))]
        public void 스테이지가_웨이브_데이터를_들고_있다(string path)
        {
            SerializedProperty waveData = ProjectAssets.Load(path).FindProperty("waveData");
            Assert.That(waveData, Is.Not.Null, path + " 에 waveData 필드가 없다");

            SerializedProperty enemyWaves = waveData.FindPropertyRelative("enemyWaves");
            Assert.That(enemyWaves, Is.Not.Null, path + " 에 enemyWaves 필드가 없다");
            Assert.That(enemyWaves.arraySize, Is.GreaterThan(0), path + " 에 연결된 웨이브가 없다");

            for (int i = 0; i < enemyWaves.arraySize; i++)
            {
                Assert.That(enemyWaves.GetArrayElementAtIndex(i).objectReferenceValue, Is.Not.Null,
                    path + " 의 enemyWaves[" + i + "] 참조가 끊겼다");
            }
        }

        [Test]
        public void 스테이지_id가_겹치지_않는다()
        {
            List<int> ids = StagePaths()
                .Select(path => ProjectAssets.Load(path).FindProperty("stageID").intValue)
                .ToList();

            Assert.That(ids, Is.Unique, "stageID가 중복이다: " + string.Join(", ", ids));
        }

        [Test]
        public void 적_데이터가_온전하다()
        {
            SerializedProperty enemies = ProjectAssets.Load(ProjectAssets.EnemyDataContainerPath)
                .FindProperty("enemyDatas");

            Assert.That(enemies, Is.Not.Null, "EnemyDataContainer에 enemyDatas 필드가 없다");
            Assert.That(enemies.arraySize, Is.GreaterThan(0), "적 데이터가 비었다");

            List<int> ids = new List<int>();
            for (int i = 0; i < enemies.arraySize; i++)
            {
                SerializedProperty enemy = enemies.GetArrayElementAtIndex(i);
                string label = "enemyDatas[" + i + "]";

                ids.Add(enemy.FindPropertyRelative("id").intValue);
                Assert.That(enemy.FindPropertyRelative("prefab").objectReferenceValue, Is.Not.Null,
                    label + " 의 prefab 참조가 끊겼다");
                Assert.That(enemy.FindPropertyRelative("maxHp").intValue, Is.GreaterThan(0),
                    label + " 의 maxHp가 0 이하다");
            }

            Assert.That(ids, Is.Unique, "적 id가 중복이다: " + string.Join(", ", ids));
        }
    }
}
