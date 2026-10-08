using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WordVenture.Tests
{
    /// <summary>
    /// 벼락 협곡(스테이지 6) 데이터와 새 슬라임 세 종류(곡사, 돌진, 방패)의 prefab을 검사한다.
    /// </summary>
    public sealed class ThunderCanyonDataTests
    {
        const int ThunderCanyonStageId = 6;
        const string DialoguePath = "Assets/Resources/Story/ActOneDialogues.asset";
        const string SlimePrefabFolder = "Assets/Prefabs/Combat/Enemies/Slime/";

        static Type TypeOf(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        [Test]
        public void 벼락_협곡_스테이지의_웨이브가_적_데이터에_있는_id만_쓴다()
        {
            string stagePath = ProjectAssets.StagePaths().FirstOrDefault(path =>
                ProjectAssets.Load(path).FindProperty("stageID").intValue == ThunderCanyonStageId);
            Assert.That(stagePath, Is.Not.Null, "stageID " + ThunderCanyonStageId + " 인 스테이지 에셋이 없다");

            SerializedProperty enemyWaves = ProjectAssets.Load(stagePath)
                .FindProperty("waveData").FindPropertyRelative("enemyWaves");
            Assert.That(enemyWaves.arraySize, Is.GreaterThan(0), stagePath + " 에 연결된 웨이브가 없다");

            HashSet<int> knownIds = KnownEnemyIds();
            for (int i = 0; i < enemyWaves.arraySize; i++)
            {
                UnityEngine.Object waveAsset = enemyWaves.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.That(waveAsset, Is.Not.Null, stagePath + " enemyWaves[" + i + "] 이 비어 있다");

                SerializedProperty waves = ProjectAssets.Load(AssetDatabase.GetAssetPath(waveAsset))
                    .FindProperty("battleWaveDatas");
                for (int w = 0; w < waves.arraySize; w++)
                {
                    SerializedProperty spawns = waves.GetArrayElementAtIndex(w)
                        .FindPropertyRelative("enemySpawnDatasInWave");
                    for (int s = 0; s < spawns.arraySize; s++)
                    {
                        int enemyId = spawns.GetArrayElementAtIndex(s).FindPropertyRelative("enemyId").intValue;
                        Assert.That(knownIds, Contains.Item(enemyId),
                            stagePath + " 의 웨이브가 EnemyDataContainer에 없는 id " + enemyId + " 를 소환한다");
                    }
                }
            }
        }

        [TestCase("MortarSlime", "MortarEnemy")]
        [TestCase("ChargeSlime", "ChargeEnemy")]
        [TestCase("ShieldSlime", "ShieldEnemy")]
        public void 새_슬라임_prefab이_새_스크립트와_기본_구성을_갖는다(string prefabName, string scriptName)
        {
            string path = SlimePrefabFolder + prefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path + " 을(를) 로드하지 못했다");

            Assert.That(prefab.GetComponent(TypeOf("Combat.Enemies." + scriptName)), Is.Not.Null,
                prefabName + " 에 " + scriptName + " 가 없다");
            Assert.That(prefab.tag, Is.EqualTo("Enemy"));
            Assert.That(prefab.GetComponent(TypeOf("Combat.Enemies.SelectableObject")), Is.Not.Null,
                prefabName + " 에 SelectableObject가 없다");
            Assert.That(prefab.GetComponent<Collider2D>(), Is.Not.Null,
                prefabName + " 에 Collider2D가 없다");
        }

        [Test]
        public void 곡사_슬라임_prefab에_착탄_표시_스프라이트가_연결돼_있다()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabFolder + "MortarSlime.prefab");
            Assert.That(prefab, Is.Not.Null);

            SerializedObject mortar = ProjectAssets.FindComponentWithProperty(prefab, "markerSprite");
            Assert.That(mortar, Is.Not.Null, "MortarSlime에 markerSprite 필드가 없다");
            Assert.That(mortar.FindProperty("markerSprite").objectReferenceValue, Is.Not.Null,
                "MortarSlime의 markerSprite가 비어 있다");
        }

        [Test]
        public void 벼락_협곡_대사는_입장과_클리어가_하나씩_있다()
        {
            SerializedProperty chapters = ProjectAssets.Load(DialoguePath).FindProperty("chapters");
            Assert.That(chapters, Is.Not.Null, DialoguePath + " 에 chapters 필드가 없다");

            int enterCount = 0;
            int clearCount = 0;
            for (int i = 0; i < chapters.arraySize; i++)
            {
                SerializedProperty chapter = chapters.GetArrayElementAtIndex(i);
                if (chapter.FindPropertyRelative("stageID").intValue != ThunderCanyonStageId)
                {
                    continue;
                }

                // StageDialogueMoment: Clear = 0, Enter = 1
                int moment = chapter.FindPropertyRelative("moment").enumValueIndex;
                if (moment == 1) enterCount++;
                if (moment == 0) clearCount++;
            }

            Assert.That(enterCount, Is.EqualTo(1), "스테이지 6 입장 대사 개수");
            Assert.That(clearCount, Is.EqualTo(1), "스테이지 6 클리어 대사 개수");
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
