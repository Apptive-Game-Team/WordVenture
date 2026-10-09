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
        readonly string[] keys = { "TutorialEnded", "StagePosition" };
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
        public IEnumerator 서리_마을을_끝내도_전투가_없는_벼락_협곡에는_들어갈_수_없다()
        {
            yield return LoadMap(6);
            Assert.That(IsUnlocked(0), Is.True);
            Assert.That(IsUnlocked(1), Is.False);
            object stageLabel = Field(MapMove, "stage");
            Assert.That(stageLabel.GetType().GetProperty("text").GetValue(stageLabel), Does.Contain("준비 중"));
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
