#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    // 1부를 끝낸 세이브(StagePosition 5 이상)로 맵에 들어가면 2부 맵이 보이는지 확인한다.
    public sealed class ActTwoMapPlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        readonly string[] keys = { "TutorialEnded", "StagePosition", "ShowsActOneMap" };
        int[] values;
        bool[] existed;
        int oldPosition;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static FieldInfo StagePosition => Runtime("Map.MapMove").GetField("StagePosition");

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)StagePosition.GetValue(null);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.DeleteKey("ShowsActOneMap");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            // 테스트 에디터 종료 시 SaveLoadController가 복원한 저장값을 덮어쓰지 않게 한다.
            Component save = Object.FindObjectOfType(Runtime("Core.SaveLoadController")) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            StagePosition.SetValue(null, oldPosition);
        }

        static IEnumerator LoadMap(int stagePosition)
        {
            StagePosition.SetValue(null, stagePosition);
            PlayerPrefs.SetInt("StagePosition", stagePosition);
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            yield return null;
        }

        static Component MapMove => (Component)Object.FindObjectOfType(Runtime("Map.MapMove"));
        static object Field(Component target, string name) => target.GetType().GetField(name, AnyInstance).GetValue(target);
        static bool IsUnlocked(int locationIndex) =>
            (bool)MapMove.GetType().GetMethod("IsStageUnlocked", AnyInstance).Invoke(MapMove, new object[] { locationIndex });

        [UnityTest]
        public IEnumerator 일부를_끝낸_세이브는_2부_맵의_서리_마을에서_시작한다()
        {
            yield return LoadMap(5);
            Object chapter = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Map/ActTwoMap.asset");
            var background = (GameObject)Field(MapMove, "background");
            Assert.That(background.GetComponent<SpriteRenderer>().sprite,
                Is.SameAs(chapter.GetType().GetField("background").GetValue(chapter)));

            var points = (Vector2[])chapter.GetType().GetField("stagePoints").GetValue(chapter);
            var character = (GameObject)Field(MapMove, "character");
            Assert.That(Vector2.Distance(character.transform.position, points[0]), Is.LessThan(0.01f),
                "캐릭터가 서리 마을에 서지 않았다");
            Assert.That(IsUnlocked(0), Is.True);
            Assert.That(IsUnlocked(1), Is.False);
        }

        [UnityTest]
        public IEnumerator 이부를_모두_끝내면_다섯_지역에_모두_들어갈_수_있다()
        {
            yield return LoadMap(10);
            for (int i = 0; i < 5; i++) Assert.That(IsUnlocked(i), Is.True, "지점 " + i + " 이(가) 잠겨 있다");
            object stageLabel = Field(MapMove, "stage");
            Assert.That(stageLabel.GetType().GetProperty("text").GetValue(stageLabel), Does.Not.Contain("준비 중"));
        }

        [UnityTest]
        public IEnumerator 이부를_끝내고_엔딩_씬에_들어가면_이부_에필로그를_보여_준다()
        {
            StagePosition.SetValue(null, 10);
            yield return SceneManager.LoadSceneAsync("EndingScene");
            yield return null;
            Component story = (Component)Object.FindObjectOfType(Runtime("Story.StoryController"));
            Object epilogue = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/ActTwoEndingScript.asset");
            Assert.That(Field(story, "scriptContainer"), Is.SameAs(epilogue));
        }

        static UnityEngine.UI.Button ChapterSwitchButton => (UnityEngine.UI.Button)Field(MapMove, "chapterSwitchButton");
        static Vector2 CharacterPosition => ((GameObject)Field(MapMove, "character")).transform.position;

        [UnityTest]
        public IEnumerator 장_전환_버튼으로_1부_맵과_2부_맵을_오가고_마지막_장을_기억한다()
        {
            yield return LoadMap(7);
            Assert.That(ChapterSwitchButton.gameObject.activeInHierarchy, Is.True, "1부를 끝냈는데 장 전환 버튼이 숨겨져 있다");
            Vector2 actOneBoss = ((Vector3[])Field(MapMove, "actOneStageLocations"))[4];

            ChapterSwitchButton.onClick.Invoke();
            yield return null;
            var background = (GameObject)Field(MapMove, "background");
            Assert.That(Field(MapMove, "chapter"), Is.Null);
            Assert.That(background.GetComponent<SpriteRenderer>().sprite, Is.SameAs(Field(MapMove, "stage4")));
            Assert.That(Vector2.Distance(CharacterPosition, actOneBoss), Is.LessThan(0.01f), "캐릭터가 1부 마왕 지점에 서지 않았다");
            for (int i = 0; i < 5; i++) Assert.That(IsUnlocked(i), Is.True, "1부 지점 " + i + " 이(가) 잠겨 있다");

            // 전투를 다녀온 것처럼 맵 씬을 다시 열면 1부 맵이 그대로 열린다.
            yield return LoadMap(7);
            Assert.That(Field(MapMove, "chapter"), Is.Null, "다시 연 맵이 마지막으로 본 1부 맵을 잊었다");

            ChapterSwitchButton.onClick.Invoke();
            yield return null;
            Object chapter = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Map/ActTwoMap.asset");
            var points = (Vector2[])chapter.GetType().GetField("stagePoints").GetValue(chapter);
            Assert.That(Field(MapMove, "chapter"), Is.SameAs(chapter));
            Assert.That(Vector2.Distance(CharacterPosition, points[2]), Is.LessThan(0.01f), "캐릭터가 잿빛 유적(7)에 서지 않았다");
            Assert.That(PlayerPrefs.GetInt("ShowsActOneMap"), Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator 일부_진행_중에는_장_전환_버튼을_숨긴다()
        {
            yield return LoadMap(3);
            Assert.That(ChapterSwitchButton.gameObject.activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator 일부_진행_중에는_기존_맵을_그대로_보여_준다()
        {
            yield return LoadMap(2);
            var background = (GameObject)Field(MapMove, "background");
            Assert.That(background.GetComponent<SpriteRenderer>().sprite, Is.SameAs(Field(MapMove, "stage2")));
            Assert.That(Field(MapMove, "chapter"), Is.Null);
            Assert.That(IsUnlocked(2), Is.True);
            Assert.That(IsUnlocked(3), Is.False);
        }
    }
}
#endif
