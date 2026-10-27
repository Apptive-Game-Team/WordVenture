using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace WordVenture.Tests
{
    public sealed class StageMusicAssetTests
    {
        static IEnumerable<string> Stages() => ProjectAssets.StagePaths();

        [Test]
        public void 전후_대화는_지역곡이_연결되고_전투중_대화는_음악을_바꾸지_않는다()
        {
            var musicByStage = new Dictionary<int, Object>();
            foreach (string path in Stages())
            {
                SerializedObject stage = ProjectAssets.Load(path);
                musicByStage.Add(stage.FindProperty("stageID").intValue,
                    stage.FindProperty("music").objectReferenceValue);
            }
            SerializedProperty chapters = ProjectAssets.Load("Assets/Resources/Story/ActOneDialogues.asset")
                .FindProperty("chapters");
            for (int i = 0; i < chapters.arraySize; i++)
            {
                SerializedProperty chapter = chapters.GetArrayElementAtIndex(i);
                Object music = chapter.FindPropertyRelative("music").objectReferenceValue;
                if (chapter.FindPropertyRelative("moment").intValue == 2)
                    Assert.That(music, Is.Null);
                else
                    Assert.That(music, Is.EqualTo(musicByStage[chapter.FindPropertyRelative("stageID").intValue]));
            }
        }

        [Test, TestCaseSource(nameof(Stages))]
        public void 모든_스테이지에_새_음악이_연결되고_두_최종전에만_보스곡이_있다(string path)
        {
            SerializedObject stage = ProjectAssets.Load(path);
            int id = stage.FindProperty("stageID").intValue;
            var music = stage.FindProperty("music").objectReferenceValue as AudioClip;
            Assert.That(music, Is.Not.Null, path);
            Assert.That(AssetDatabase.GetAssetPath(music), Does.StartWith("Assets/Audio/StageMusic/" + id.ToString("00") + "-"));
            Assert.That(music.length, Is.GreaterThan(20f));
            Assert.That(music.length, Is.LessThan(40f), "무음 꼬리가 있는 생성 원본 대신 반복용 파일을 사용한다");
            var boss = stage.FindProperty("bossMusic").objectReferenceValue as AudioClip;
            if (id == 4 || id == 9)
            {
                Assert.That(boss, Is.Not.Null, path);
                Assert.That(AssetDatabase.GetAssetPath(boss), Does.EndWith(
                    id == 4 ? "12 Pixel Tracks/Pixel 5.wav" : "09-world-tree-boss-v5.ogg"));
                Assert.That(boss, Is.Not.EqualTo(music));
            }
            else Assert.That(boss, Is.Null, path);
        }
    }
}
