using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WordVenture.Tests
{
    /// <summary>
    /// 2부 마무리(스테이지 7~9, 새 슬라임 다섯 종류, 2부 대사와 에필로그) 데이터를 검사한다.
    /// 그림은 docs/design/act-two-art/manifest.json 에 적힌 고정 GUID 로 연결한다.
    /// </summary>
    public sealed class ActTwoCompleteDataTests
    {
        const string ManifestPath = "docs/design/act-two-art/manifest.json";
        const string DialoguePath = "Assets/Resources/Story/ActOneDialogues.asset";
        const string SlimePrefabFolder = "Assets/Prefabs/Combat/Enemies/Slime/";
        const string EndingScenePath = "Assets/Scenes/EndingScene.unity";
        const string ActTwoEndingPath = "Assets/ScriptableObjects/ActTwoEndingScript.asset";
        const int SplitSmallEnemyId = 25;

        // StageDialogueMoment: Clear = 0, Enter = 1, Wave = 2
        const int Clear = 0;
        const int Enter = 1;
        const int Wave = 2;

        // 프리팹 이름 -> manifest 의 적 이름
        static readonly (string Prefab, string Art)[] EnemyArt =
        {
            ("MortarSlime", "Mortar"),
            ("ChargeSlime", "Charge"),
            ("ShieldSlime", "Shield"),
            ("SplitSlime", "Split"),
            ("SplitSlimeSmall", "Split"),
            ("ExplodeSlime", "Explode"),
            ("HealSlime", "Heal"),
            ("SurgeBoss", "Surge"),
        };

        static string Manifest => File.ReadAllText(ManifestPath);

        static List<string> ManifestGuids(string section, string name)
        {
            // manifest 에서 "<name>" 블록 이후의 path/guid 쌍을 순서대로 읽는다.
            string text = Manifest;
            int start = text.IndexOf("\"" + name + "\"", text.IndexOf("\"" + section + "\""));
            Assert.That(start, Is.GreaterThanOrEqualTo(0), name + " 이(가) manifest 에 없다");
            int end = text.IndexOf(']', start);
            if (section == "backgrounds")
            {
                end = text.IndexOf('}', start);
            }

            var matches = System.Text.RegularExpressions.Regex.Matches(
                text.Substring(start, end - start), "\"guid\":\\s*\"([0-9a-f]{32})\"");
            return matches.Cast<System.Text.RegularExpressions.Match>().Select(m => m.Groups[1].Value).ToList();
        }

        static HashSet<string> AllBackgroundGuids() => new HashSet<string>(
            new[] { "FrostVillage", "ThunderCanyon", "AshenRuins", "TwistedForest", "WorldTreeHeart" }
                .SelectMany(name => ManifestGuids("backgrounds", name)));

        static string GuidOf(SerializedProperty reference)
        {
            string path = AssetDatabase.GetAssetPath(reference.objectReferenceValue);
            return AssetDatabase.AssetPathToGUID(path);
        }

        static string StagePath(int stageId) => ProjectAssets.StagePaths().FirstOrDefault(path =>
            ProjectAssets.Load(path).FindProperty("stageID").intValue == stageId);

        static HashSet<int> KnownEnemyIds()
        {
            SerializedProperty enemies = ProjectAssets.Load(ProjectAssets.EnemyDataContainerPath)
                .FindProperty("enemyDatas");
            var ids = new HashSet<int>();
            for (int i = 0; i < enemies.arraySize; i++)
            {
                ids.Add(enemies.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue);
            }

            return ids;
        }

        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void 마무리_스테이지가_존재하고_웨이브가_등록된_적만_쓴다(int stageId)
        {
            string stagePath = StagePath(stageId);
            Assert.That(stagePath, Is.Not.Null, "stageID " + stageId + " 인 스테이지 에셋이 없다");

            SerializedObject stage = ProjectAssets.Load(stagePath);
            Assert.That(stage.FindProperty("stageName").stringValue, Is.Not.Empty);
            Assert.That(stage.FindProperty("music").objectReferenceValue, Is.Not.Null, stagePath + " 음악이 비어 있다");

            SerializedProperty enemyWaves = stage.FindProperty("waveData").FindPropertyRelative("enemyWaves");
            Assert.That(enemyWaves.arraySize, Is.GreaterThan(0), stagePath + " 에 연결된 웨이브가 없다");

            HashSet<int> knownIds = KnownEnemyIds();
            for (int i = 0; i < enemyWaves.arraySize; i++)
            {
                Object waveAsset = enemyWaves.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.That(waveAsset, Is.Not.Null, stagePath + " enemyWaves[" + i + "] 이 비어 있다");

                SerializedProperty waves = ProjectAssets.Load(AssetDatabase.GetAssetPath(waveAsset))
                    .FindProperty("battleWaveDatas");
                Assert.That(waves.arraySize, Is.EqualTo(3), stagePath + " 웨이브 수");
                for (int w = 0; w < waves.arraySize; w++)
                {
                    SerializedProperty spawns = waves.GetArrayElementAtIndex(w)
                        .FindPropertyRelative("enemySpawnDatasInWave");
                    for (int s = 0; s < spawns.arraySize; s++)
                    {
                        int enemyId = spawns.GetArrayElementAtIndex(s).FindPropertyRelative("enemyId").intValue;
                        Assert.That(knownIds, Contains.Item(enemyId),
                            stagePath + " 의 웨이브가 등록되지 않은 적 id " + enemyId + " 를 소환한다");
                    }
                }
            }
        }

        [TestCase(5, "FrostVillage")]
        [TestCase(6, "ThunderCanyon")]
        [TestCase(7, "AshenRuins")]
        [TestCase(8, "TwistedForest")]
        [TestCase(9, "WorldTreeHeart")]
        public void 둘째_부_스테이지_배경이_manifest의_해당_지역_그림이다(int stageId, string artName)
        {
            string stagePath = StagePath(stageId);
            Assert.That(stagePath, Is.Not.Null);

            SerializedProperty background = ProjectAssets.Load(stagePath).FindProperty("background");
            string expected = ManifestGuids("backgrounds", artName).Single();
            Assert.That(AllBackgroundGuids(), Contains.Item(expected));
            Assert.That(GuidOf(background), Is.EqualTo(expected), stagePath + " 의 배경 GUID");
        }

        [Test]
        // 기본 8장 뒤에 돌진·폭발 슬라임의 예고 프레임 2장이 더 붙는다.
        public void 둘째_부_적_prefab의_애니메이션이_manifest의_프레임과_같다()
        {
            foreach ((string prefabName, string art) in EnemyArt)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabFolder + prefabName + ".prefab");
                Assert.That(prefab, Is.Not.Null, prefabName + " 을(를) 로드하지 못했다");

                SerializedObject animator = ProjectAssets.FindComponentWithProperty(prefab, "sprites");
                Assert.That(animator, Is.Not.Null, prefabName + " 에 SlimeAnimator 가 없다");

                List<string> expected = ManifestGuids("enemies", art);
                SerializedProperty sprites = animator.FindProperty("sprites");
                Assert.That(sprites.arraySize, Is.EqualTo(expected.Count), prefabName + " 프레임 수");
                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.That(GuidOf(sprites.GetArrayElementAtIndex(i)), Is.EqualTo(expected[i]),
                        prefabName + " 프레임 " + (i + 1));
                }
            }
        }

        [Test]
        public void 분열_슬라임과_폭주_보스가_작은_분열_슬라임_id를_가리킨다()
        {
            Assert.That(KnownEnemyIds(), Contains.Item(SplitSmallEnemyId));

            GameObject split = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabFolder + "SplitSlime.prefab");
            SerializedObject splitSerialized = ProjectAssets.FindComponentWithProperty(split, "childEnemyId");
            Assert.That(splitSerialized, Is.Not.Null, "SplitSlime 에 childEnemyId 필드가 없다");
            Assert.That(splitSerialized.FindProperty("childEnemyId").intValue, Is.EqualTo(SplitSmallEnemyId));

            GameObject boss = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabFolder + "SurgeBoss.prefab");
            SerializedObject bossSerialized = ProjectAssets.FindComponentWithProperty(boss, "splitEnemyId");
            Assert.That(bossSerialized, Is.Not.Null, "SurgeBoss 에 splitEnemyId 필드가 없다");
            Assert.That(bossSerialized.FindProperty("splitEnemyId").intValue, Is.EqualTo(SplitSmallEnemyId));
            Assert.That(bossSerialized.FindProperty("markerSprite").objectReferenceValue, Is.Not.Null,
                "SurgeBoss 의 markerSprite 가 비어 있다");
        }

        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void 둘째_부_스테이지마다_입장과_클리어_대사가_하나씩_있다(int stageId)
        {
            Assert.That(CountChapters(stageId, Enter, null), Is.EqualTo(1), "입장 대사 개수");
            Assert.That(CountChapters(stageId, Clear, null), Is.EqualTo(1), "클리어 대사 개수");
        }

        [TestCase(7, 2)]
        [TestCase(8, 1)]
        [TestCase(9, 2)]
        public void 마무리_스테이지에_웨이브_대사가_정해진_웨이브_앞에_하나씩_있다(int stageId, int wave)
        {
            Assert.That(CountChapters(stageId, Wave, wave), Is.EqualTo(1),
                "스테이지 " + stageId + " 웨이브 " + wave + " 앞 대사 개수");
        }

        static int CountChapters(int stageId, int moment, int? wave)
        {
            SerializedProperty chapters = ProjectAssets.Load(DialoguePath).FindProperty("chapters");
            int count = 0;
            for (int i = 0; i < chapters.arraySize; i++)
            {
                SerializedProperty chapter = chapters.GetArrayElementAtIndex(i);
                if (chapter.FindPropertyRelative("stageID").intValue == stageId
                    && chapter.FindPropertyRelative("moment").enumValueIndex == moment
                    && (wave == null || chapter.FindPropertyRelative("wave").intValue == wave))
                {
                    count++;
                }
            }

            return count;
        }

        [Test]
        public void 엔딩_씬이_2부_에필로그_스크립트를_가리킨다()
        {
            string guid = AssetDatabase.AssetPathToGUID(ActTwoEndingPath);
            Assert.That(guid, Is.Not.Empty, ActTwoEndingPath + " 이(가) 없다");
            Assert.That(File.ReadAllText(EndingScenePath), Does.Contain("actTwoEndingScript: {fileID: 11400000, guid: " + guid),
                "StoryController 의 actTwoEndingScript 참조가 끊겼다");
        }

        [Test]
        public void 둘째_부_에필로그의_마지막_줄이_마법의_신_대사다()
        {
            SerializedProperty script = ProjectAssets.Load(ActTwoEndingPath).FindProperty("script");
            Assert.That(script.arraySize, Is.GreaterThan(0));
            string last = script.GetArrayElementAtIndex(script.arraySize - 1).FindPropertyRelative("text").stringValue;
            Assert.That(last, Is.EqualTo("훗날 사람들은 그녀를 마법의 신, 워드라 불렀다."));
        }
    }
}
