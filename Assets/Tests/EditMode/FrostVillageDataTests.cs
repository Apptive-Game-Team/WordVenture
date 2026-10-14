using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace WordVenture.Tests
{
    /// <summary>
    /// 서리 마을(스테이지 5) 데이터가 전투 씬과 대사 데이터에 빠짐없이 연결돼 있는지 검사한다.
    /// 적은 소환할 때 id로 만들어지므로, 웨이브가 없는 id를 가리키면 전투 중에 에러가 난다.
    /// </summary>
    public sealed class FrostVillageDataTests
    {
        const int FrostVillageStageId = 5;
        const string BattleScenePath = "Assets/Scenes/TurnBattleScene.unity";
        const string DialoguePath = "Assets/Resources/Story/ActOneDialogues.asset";

        static IEnumerable<string> WaveDataPaths()
        {
            return ProjectAssets.WaveDataPaths();
        }

        [Test]
        public void 서리_마을_스테이지의_웨이브가_적_데이터에_있는_id만_쓴다()
        {
            string stagePath = ProjectAssets.StagePaths().FirstOrDefault(path =>
                ProjectAssets.Load(path).FindProperty("stageID").intValue == FrostVillageStageId);
            Assert.That(stagePath, Is.Not.Null, "stageID " + FrostVillageStageId + " 인 스테이지 에셋이 없다");

            SerializedProperty enemyWaves = ProjectAssets.Load(stagePath)
                .FindProperty("waveData").FindPropertyRelative("enemyWaves");
            Assert.That(enemyWaves.arraySize, Is.GreaterThan(0), stagePath + " 에 연결된 웨이브가 없다");

            HashSet<int> knownIds = KnownEnemyIds();
            for (int i = 0; i < enemyWaves.arraySize; i++)
            {
                UnityEngine.Object waveAsset = enemyWaves.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.That(waveAsset, Is.Not.Null, stagePath + " enemyWaves[" + i + "] 이 비어 있다");

                foreach (int enemyId in SpawnedEnemyIds(AssetDatabase.GetAssetPath(waveAsset)))
                {
                    Assert.That(knownIds, Contains.Item(enemyId),
                        stagePath + " 의 웨이브가 EnemyDataContainer에 없는 id " + enemyId + " 를 소환한다");
                }
            }
        }

        [Test, TestCaseSource(nameof(WaveDataPaths))]
        public void 모든_웨이브가_적_데이터에_있는_id만_소환한다(string path)
        {
            HashSet<int> knownIds = KnownEnemyIds();

            foreach (int enemyId in SpawnedEnemyIds(path))
            {
                Assert.That(knownIds, Contains.Item(enemyId),
                    path + " 가 EnemyDataContainer에 없는 id " + enemyId + " 를 소환한다");
            }
        }

        [Test]
        public void 전투_씬의_스테이지_목록에_모든_스테이지가_들어_있다()
        {
            string sceneText = File.ReadAllText(BattleScenePath);

            foreach (string stagePath in ProjectAssets.StagePaths())
            {
                string guid = AssetDatabase.AssetPathToGUID(stagePath);
                Assert.That(sceneText, Does.Contain("guid: " + guid),
                    BattleScenePath + " 의 stageDataList에 " + stagePath + " 가 없다");
            }
        }

        [Test]
        public void 서리_마을_대사는_입장과_클리어가_하나씩_있다()
        {
            SerializedProperty chapters = ProjectAssets.Load(DialoguePath).FindProperty("chapters");
            Assert.That(chapters, Is.Not.Null, DialoguePath + " 에 chapters 필드가 없다");

            int enterCount = 0;
            int clearCount = 0;
            for (int i = 0; i < chapters.arraySize; i++)
            {
                SerializedProperty chapter = chapters.GetArrayElementAtIndex(i);
                if (chapter.FindPropertyRelative("stageID").intValue != FrostVillageStageId)
                {
                    continue;
                }

                // StageDialogueMoment: Clear = 0, Enter = 1
                int moment = chapter.FindPropertyRelative("moment").enumValueIndex;
                if (moment == 1) enterCount++;
                if (moment == 0) clearCount++;
            }

            Assert.That(enterCount, Is.EqualTo(1), "스테이지 5 입장 대사 개수");
            Assert.That(clearCount, Is.EqualTo(1), "스테이지 5 클리어 대사 개수");
        }

        static IEnumerable<int> SpawnedEnemyIds(string waveDataPath)
        {
            SerializedProperty waves = ProjectAssets.Load(waveDataPath).FindProperty("battleWaveDatas");
            for (int i = 0; i < waves.arraySize; i++)
            {
                SerializedProperty spawns = waves.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("enemySpawnDatasInWave");
                for (int j = 0; j < spawns.arraySize; j++)
                {
                    yield return spawns.GetArrayElementAtIndex(j).FindPropertyRelative("enemyId").intValue;
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
