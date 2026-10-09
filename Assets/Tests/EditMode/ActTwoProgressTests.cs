using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    /// <summary>
    /// 2부 맵 연결과 2부 대화 읽음 기록을 검사한다.
    /// </summary>
    public sealed class ActTwoProgressTests
    {
        const string ActTwoMapPath = "Assets/ScriptableObjects/Map/ActTwoMap.asset";
        const string MapScenePath = "Assets/Scenes/MapScene.unity";
        const string DialoguePath = "Assets/Resources/Story/ActOneDialogues.asset";
        const string ActOneSeenKey = "ActOneDialogueSeen";
        const string ActTwoSeenKey = "ActTwoDialogueSeen";

        static Type TypeOf(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        static Type SaveType => TypeOf("Core.SaveLoadController");

        [Test]
        public void 맵_씬이_2부_맵을_들고_있다()
        {
            string guid = AssetDatabase.AssetPathToGUID(ActTwoMapPath);
            Assert.That(guid, Is.Not.Empty, ActTwoMapPath + " 이(가) 없다");
            Assert.That(File.ReadAllText(MapScenePath), Does.Contain("actTwoMap: {fileID: 11400000, guid: " + guid),
                "MapMove의 actTwoMap 참조가 끊겼다");
        }

        [Test]
        public void 서리_마을부터_시작하는_2부_맵이_지점_5곳을_배경_안에_둔다()
        {
            SerializedObject map = ProjectAssets.Load(ActTwoMapPath);
            Assert.That(map.FindProperty("firstStageID").intValue, Is.EqualTo(5));
            Assert.That(map.FindProperty("lastPlayableStageID").intValue, Is.EqualTo(6),
                "전투 데이터가 있는 2부 지역은 서리 마을(5)과 벼락 협곡(6)이다");
            Assert.That(map.FindProperty("background").objectReferenceValue, Is.Not.Null);

            SerializedProperty points = map.FindProperty("stagePoints");
            Assert.That(points.arraySize, Is.EqualTo(5), "MapMove의 지점 오브젝트는 5개다");
            for (int i = 0; i < points.arraySize; i++)
            {
                Vector2 point = points.GetArrayElementAtIndex(i).vector2Value;
                // 배경은 1536x1024 그림을 100 px/unit, 가로 1.1913배로 원점에 놓는다.
                Assert.That(Mathf.Abs(point.x), Is.LessThan(9.15f), "지점 " + i + " 이(가) 배경 밖에 있다");
                Assert.That(Mathf.Abs(point.y), Is.LessThan(5.12f), "지점 " + i + " 이(가) 배경 밖에 있다");
            }
        }

        [Test]
        public void 지역_대화의_읽음_비트가_서로_겹치지_않고_2부는_1부_비트를_쓰지_않는다()
        {
            Object dialogues = AssetDatabase.LoadAssetAtPath<Object>(DialoguePath);
            var chapters = (IEnumerable)dialogues.GetType().GetField("chapters").GetValue(dialogues);
            var bits = new List<int>();
            foreach (object chapter in chapters)
            {
                int stageID = (int)chapter.GetType().GetField("stageID").GetValue(chapter);
                int bit = (int)chapter.GetType().GetProperty("SeenBit").GetValue(chapter);
                Assert.That(bit, Is.GreaterThanOrEqualTo(0), "스테이지 " + stageID + " 대화에 읽음 비트가 없다");
                if (stageID >= 5) Assert.That(bit, Is.GreaterThanOrEqualTo(32), "2부 대화가 1부 비트를 쓴다");
                else Assert.That(bit, Is.LessThan(31));
                bits.Add(bit);
            }
            Assert.That(bits, Is.Unique);
        }

        [Test]
        public void 대화_기록에서_2부_비트는_1부_저장값을_건드리지_않는다()
        {
            bool hadActOne = PlayerPrefs.HasKey(ActOneSeenKey);
            bool hadActTwo = PlayerPrefs.HasKey(ActTwoSeenKey);
            bool hadStage = PlayerPrefs.HasKey("StagePosition");
            int actOne = PlayerPrefs.GetInt(ActOneSeenKey);
            int actTwo = PlayerPrefs.GetInt(ActTwoSeenKey);
            int stage = PlayerPrefs.GetInt("StagePosition");
            try
            {
                PlayerPrefs.SetInt(ActOneSeenKey, 0b101);
                PlayerPrefs.DeleteKey(ActTwoSeenKey);

                // 서리 마을(5) 입장 대화의 비트는 32 + 5 + 0이다.
                SaveType.GetMethod("MarkStageDialogueSeen").Invoke(null, new object[] { 37 });

                Assert.That(PlayerPrefs.GetInt(ActOneSeenKey), Is.EqualTo(0b101));
                Assert.That(PlayerPrefs.GetInt(ActTwoSeenKey), Is.EqualTo(1 << 5));
                Assert.That((bool)SaveType.GetMethod("HasSeenStageDialogue").Invoke(null, new object[] { 37 }), Is.True);
                Assert.That((bool)SaveType.GetMethod("HasSeenStageDialogue").Invoke(null, new object[] { 5 }), Is.False,
                    "1부 고원 입장(5) 기록과 섞였다");
                Assert.That((bool)SaveType.GetMethod("HasSeenStageDialogue").Invoke(null, new object[] { 2 }), Is.True);
            }
            finally
            {
                if (hadActOne) PlayerPrefs.SetInt(ActOneSeenKey, actOne); else PlayerPrefs.DeleteKey(ActOneSeenKey);
                if (hadActTwo) PlayerPrefs.SetInt(ActTwoSeenKey, actTwo); else PlayerPrefs.DeleteKey(ActTwoSeenKey);
                if (hadStage) PlayerPrefs.SetInt("StagePosition", stage); else PlayerPrefs.DeleteKey("StagePosition");
                PlayerPrefs.Save();
            }
        }
    }
}
