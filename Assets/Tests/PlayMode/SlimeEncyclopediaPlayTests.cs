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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    // 전투에서 만난 슬라임을 기록하고, 맵에서 연 도감이 그 기록대로 이름을 보여 주는지 확인한다.
    public sealed class SlimeEncyclopediaPlayTests
    {
        const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        readonly string[] keys =
        {
            "TutorialEnded", "StagePosition", "ShowsActOneMap", "ActOneDialogueSeen", "ActTwoDialogueSeen",
            "SeenSlimes0", "SeenSlimes1", "Language"
        };
        int[] values;
        bool[] existed;
        int oldPosition;

        static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static FieldInfo StagePosition => Runtime("Map.MapMove").GetField("StagePosition");
        static Type Save => Runtime("Core.SaveLoadController");
        static Type View => Runtime("Combat.UI.SlimeEncyclopediaView");
        static bool HasSeen(int id) => (bool)Save.GetMethod("HasSeenSlime").Invoke(null, new object[] { id });
        static bool IsLocked => (bool)Runtime("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);
        static Component CurrentView => (Component)View.GetProperty("Current").GetValue(null);
        static Object EnemyData => AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects/Combat/EnemyDataContainer.asset");
        // 도감에 실리는 적 수. displayName 이 빈 적(S0_Boss_Slime)은 빠진다.
        static int ListedCount()
        {
            var all = (IEnumerable)EnemyData.GetType().GetProperty("All").GetValue(EnemyData);
            return all.Cast<object>().Count(data =>
                !string.IsNullOrEmpty((string)data.GetType().GetField("displayName").GetValue(data)));
        }

        static string Text(Transform obj)
        {
            Component label = obj.GetComponent(Runtime("TMPro.TMP_Text"));
            return (string)label.GetType().GetProperty("text").GetValue(label);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetInt(k)).ToArray();
            oldPosition = (int)StagePosition.GetValue(null);
            PlayerPrefs.SetInt("TutorialEnded", 1);
            PlayerPrefs.DeleteKey("SeenSlimes0");
            PlayerPrefs.DeleteKey("SeenSlimes1");
            SharedDialoguePlayTests.SetLanguage("Korean");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            // 테스트 에디터 종료 시 SaveLoadController가 복원한 저장값을 덮어쓰지 않게 한다.
            Component save = Object.FindObjectOfType(Save) as Component;
            if (save != null) Object.Destroy(save.gameObject);
            yield return null;
            for (int i = 0; i < keys.Length; i++)
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            SharedDialoguePlayTests.ResetLanguage();
            StagePosition.SetValue(null, oldPosition);
        }

        [UnityTest]
        public IEnumerator 소환한_적은_도감에_기록되고_새_게임을_시작하면_지워진다()
        {
            var pool = new GameObject("Pool").AddComponent(Runtime("Combat.Enemies.EnemyPoolController"));
            pool.GetType().GetField("enemyDataContainer", AnyInstance).SetValue(pool, EnemyData);
            var spawn = pool.GetType().GetMethod("SpawnObject");
            var first = (GameObject)spawn.Invoke(pool, new object[] { 3f, 0f, 3 });
            // 31번부터는 두 번째 key 에 기록한다. 쓰지 않는 id 지만 기록 범위를 확인한다.
            Save.GetMethod("MarkSlimeSeen").Invoke(null, new object[] { 40 });
            yield return null;

            Assert.That(HasSeen(3), Is.True, "소환한 물방울 슬라임이 기록되지 않았다");
            Assert.That(HasSeen(4), Is.False);
            Assert.That(HasSeen(40), Is.True);
            Assert.That(PlayerPrefs.GetInt("SeenSlimes1"), Is.EqualTo(1 << 9));

            Object.Destroy(first);
            var saveObject = new GameObject("Save").AddComponent(Save);
            Save.GetMethod("InitPlayData").Invoke(saveObject, null);
            Assert.That(HasSeen(3), Is.False, "새 게임에 이전 도감 기록이 남았다");
            Assert.That(HasSeen(40), Is.False);
            Object.Destroy(pool.gameObject);
            Object.Destroy(saveObject.gameObject);
        }

        static IEnumerator LoadMap(int stagePosition)
        {
            StagePosition.SetValue(null, stagePosition);
            PlayerPrefs.SetInt("StagePosition", stagePosition);
            yield return SceneManager.LoadSceneAsync("MapScene");
            yield return null;
            yield return null;
        }

        // 도감 기록 전의 세이브: 기록은 비어 있고 StagePosition 8 은 스테이지 0~7 을 깼다는 뜻이다.
        [UnityTest]
        public IEnumerator 기존_세이브로_맵을_열면_깬_스테이지의_적과_그_적이_부른_적이_기록된다()
        {
            yield return LoadMap(8);

            int[] cleared = { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            foreach (int id in cleared) Assert.That(HasSeen(id), Is.True, "깬 스테이지의 적 " + id + " 이(가) 기록되지 않았다");
            Assert.That(HasSeen(25), Is.True, "잿빛 유적의 분열 슬라임이 나누어진 작은 분열 슬라임이 기록되지 않았다");
            // 2 는 어느 스테이지에도 없고, 26~29 는 아직 깨지 않은 뒤틀린 숲(8)과 세계수의 심장(9)에만 나온다.
            foreach (int id in new[] { 2, 26, 27, 28, 29 })
                Assert.That(HasSeen(id), Is.False, "만나지 않은 적 " + id + " 이(가) 기록되었다");
        }

        [UnityTest]
        public IEnumerator 맵에서_연_도감은_만난_슬라임만_이름을_보여_주고_닫으면_맵_입력이_돌아온다()
        {
            Save.GetMethod("MarkSlimeSeen").Invoke(null, new object[] { 6 });
            // 아직 깬 스테이지가 없어 맵이 다른 적을 기록하지 않는다.
            yield return LoadMap(0);

            var button = GameObject.Find("SlimeEncyclopediaButton").GetComponentInChildren<Button>();
            button.onClick.Invoke();
            yield return null;
            Component view = CurrentView;
            Assert.That(view, Is.Not.Null, "버튼을 눌렀는데 도감이 열리지 않았다");
            Assert.That(IsLocked, Is.True, "도감이 열려 있는 동안 맵 입력을 막지 않았다");

            Transform content = view.transform.Find("Window/Background/List/Content");
            int listed = ListedCount();
            Assert.That(listed, Is.GreaterThan(0));
            Assert.That(content.childCount, Is.EqualTo(listed), "이름이 빈 적만 빼고 실어야 한다");
            Assert.That(Text(content.Find("Slime6/Name")), Is.EqualTo("흙더미 슬라임"));
            Assert.That(Text(content.Find("Slime0/Name")), Is.EqualTo("???"));
            Assert.That(Text(view.transform.Find("Window/Background/Count")), Is.EqualTo("발견 1 / " + listed));

            Transform detail = view.transform.Find("Window/Background/Detail");
            Assert.That(Text(detail.Find("Name")), Is.EqualTo("흙더미 슬라임"), "처음에 만난 슬라임을 골라 두지 않았다");
            Assert.That(Text(detail.Find("Stats")), Is.EqualTo("체력 20\n공격력 10"));
            Assert.That(Text(detail.Find("Movement")), Is.EqualTo("이동 거리 5\n사거리 10"));
            string description = Text(detail.Find("Description"));
            Assert.That(description, Does.Contain("마법 탄을 쏜다"));
            // 바위 슬라임은 얼음 마법에 약하고 불 마법에 강하며 신성 마법에 회복된다.
            Assert.That(description, Does.Contain("얼음</color> 1.5배"));
            Assert.That(description, Does.Contain("불</color> 0.7배"));
            Assert.That(description, Does.Contain("맞으면 회복: <color=#FFF4C8>신성"));
            Assert.That(description, Does.Contain("균열이 저항을 절반만 무시한다."));

            View.GetMethod("Select").Invoke(view, new object[] { 0 });
            Assert.That(Text(detail.Find("Name")), Is.EqualTo("???"));
            Assert.That(Text(detail.Find("Stats")), Is.Empty, "만나지 않은 슬라임의 수치가 보인다");

            View.GetMethod("Close").Invoke(view, null);
            yield return null;
            Assert.That(CurrentView == null, Is.True);
            Assert.That(IsLocked, Is.False, "도감을 닫았는데 맵 입력 잠금이 남았다");
        }
    }
}
#endif
