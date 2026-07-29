using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace WordVenture.Tests
{
    /// <summary>
    /// 웨이브 데이터가 실제로 로드되는지 검사한다. 직렬화 필드 이름을 바꾸면서
    /// FormerlySerializedAs를 빠뜨리면 값이 조용히 0으로 로드되는데, 컴파일은 통과하고
    /// 플레이해 보기 전까지 아무도 모른다.
    /// </summary>
    public sealed class WaveDataAssetTests
    {
        static IEnumerable<string> WaveDataPaths()
        {
            return ProjectAssets.WaveDataPaths();
        }

        [Test]
        public void 웨이브_데이터_에셋이_존재한다()
        {
            Assert.That(WaveDataPaths().ToList(), Is.Not.Empty,
                ProjectAssets.WaveDataFolder + " 에 BattleWaveData 에셋이 없다");
        }

        [Test, TestCaseSource(nameof(WaveDataPaths))]
        public void 웨이브가_비어_있지_않다(string path)
        {
            SerializedProperty waves = ProjectAssets.Load(path).FindProperty("battleWaveDatas");

            Assert.That(waves, Is.Not.Null, path + " 에 battleWaveDatas 필드가 없다");
            Assert.That(waves.arraySize, Is.GreaterThan(0), path + " 의 웨이브가 비었다");

            for (int i = 0; i < waves.arraySize; i++)
            {
                SerializedProperty spawns = waves.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("enemySpawnDatasInWave");

                Assert.That(spawns, Is.Not.Null, path + " wave " + i + " 에 spawn 목록이 없다");
                Assert.That(spawns.arraySize, Is.GreaterThan(0),
                    path + " wave " + i + " 에 소환할 적이 없다");
            }
        }

        [Test, TestCaseSource(nameof(WaveDataPaths))]
        public void 소환_위치가_기본값으로_뭉개지지_않았다(string path)
        {
            SerializedProperty waves = ProjectAssets.Load(path).FindProperty("battleWaveDatas");

            for (int i = 0; i < waves.arraySize; i++)
            {
                SerializedProperty spawns = waves.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("enemySpawnDatasInWave");

                for (int j = 0; j < spawns.arraySize; j++)
                {
                    SerializedProperty spawn = spawns.GetArrayElementAtIndex(j);
                    float x = spawn.FindPropertyRelative("spawnPositionX").floatValue;

                    // 플레이어 위치가 0 근처라 소환 위치 0은 저장된 값이 안 읽혔다는 뜻이다.
                    Assert.That(x, Is.Not.EqualTo(0f),
                        path + " wave " + i + " spawn " + j + " 의 spawnPositionX가 0이다");
                }
            }
        }

        [Test, TestCaseSource(nameof(WaveDataPaths))]
        public void 소환하는_적_id가_적_데이터에_있다(string path)
        {
            HashSet<int> knownIds = KnownEnemyIds();
            SerializedProperty waves = ProjectAssets.Load(path).FindProperty("battleWaveDatas");

            for (int i = 0; i < waves.arraySize; i++)
            {
                SerializedProperty spawns = waves.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("enemySpawnDatasInWave");

                for (int j = 0; j < spawns.arraySize; j++)
                {
                    int enemyId = spawns.GetArrayElementAtIndex(j)
                        .FindPropertyRelative("enemyId").intValue;

                    Assert.That(knownIds, Contains.Item(enemyId),
                        path + " wave " + i + " 가 EnemyDataContainer에 없는 id " + enemyId + " 를 소환한다");
                }
            }
        }

        static HashSet<int> KnownEnemyIds()
        {
            SerializedProperty enemies = ProjectAssets.Load(ProjectAssets.EnemyDataContainerPath)
                .FindProperty("enemyDatas");
            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < enemies.arraySize; i++)
            {
                ids.Add(enemies.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue);
            }

            return ids;
        }
    }
}
