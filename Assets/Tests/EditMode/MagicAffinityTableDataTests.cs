using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace WordVenture.Tests
{
    /// <summary>
    /// 상성표가 물·풀 열을 포함하고, 모든 적의 속성이 상성표의 대상 열 안에 있는지 검사한다.
    /// </summary>
    public sealed class MagicAffinityTableDataTests
    {
        const string TablePath = "Assets/ScriptableObjects/Combat/Magic Affinity Table.asset";

        // MagicType 정수 값. 상성표의 대상 열 순서는 Fire, Ice, Rock, Lightning, Holy, Undead, Water, Plant 이다.
        static readonly int[] TargetTypes = { 4, 5, 6, 7, 3, 8, 10, 11 };

        // 행은 공격 속성(Fire, Ice, Rock, Lightning, Holy), 값은 Water, Plant 열이다.
        static readonly float[][] WaterAndPlant =
        {
            new[] { 1f, 1.5f },
            new[] { 1.5f, 1f },
            new[] { 1f, 0.7f },
            new[] { 1.5f, 1f },
            new[] { -1f, -1f },
        };

        static SerializedProperty Rows() =>
            ProjectAssets.Load(TablePath).FindProperty("table").FindPropertyRelative("rows");

        [Test]
        public void 상성표는_공격_속성_5행과_대상_속성_8열이다()
        {
            SerializedProperty rows = Rows();
            Assert.That(rows.arraySize, Is.EqualTo(5));
            for (int row = 0; row < rows.arraySize; row++)
            {
                Assert.That(rows.GetArrayElementAtIndex(row).FindPropertyRelative("targets").arraySize,
                    Is.EqualTo(8), "행 " + row);
            }
        }

        [Test]
        public void 물과_풀_열의_값이_정해진_상성과_같다()
        {
            SerializedProperty rows = Rows();
            for (int row = 0; row < WaterAndPlant.Length; row++)
            {
                SerializedProperty targets = rows.GetArrayElementAtIndex(row).FindPropertyRelative("targets");
                Assert.That(targets.GetArrayElementAtIndex(6).floatValue, Is.EqualTo(WaterAndPlant[row][0]), "물, 행 " + row);
                Assert.That(targets.GetArrayElementAtIndex(7).floatValue, Is.EqualTo(WaterAndPlant[row][1]), "풀, 행 " + row);
            }
        }

        [Test]
        public void 모든_적의_속성이_상성표의_대상_열에_있다()
        {
            SerializedProperty enemies = ProjectAssets.Load(ProjectAssets.EnemyDataContainerPath)
                .FindProperty("enemyDatas");
            for (int i = 0; i < enemies.arraySize; i++)
            {
                SerializedProperty enemy = enemies.GetArrayElementAtIndex(i);
                int type = enemy.FindPropertyRelative("type").intValue;
                Assert.That(TargetTypes.Contains(type), Is.True,
                    "적 " + enemy.FindPropertyRelative("id").intValue + " 의 속성 " + type + " 이 상성표에 없다");
            }
        }
    }
}
