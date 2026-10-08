using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WordVenture.Tests
{
    /// <summary>
    /// 인스펙터에서만 연결되는 참조들이 살아 있는지 검사한다. 직렬화 필드 이름이 바뀌거나
    /// 프리팹이 편집되면 참조가 조용히 끊기고, 게임은 컴파일된 채로 잘못 동작한다.
    /// </summary>
    public sealed class PrefabWiringTests
    {
        const string MagicCardPrefab = "Assets/Prefabs/Cards/MagicCard.prefab";
        const string TypeCardPrefab = "Assets/Prefabs/Cards/TypeCard.prefab";
        const string TutorialPrefab = "Assets/Prefabs/Tutorial/TutorialController.prefab";

        [Test]
        public void 폭발_카드가_기존_마법_번호를_유지한다()
        {
            var words = ProjectAssets.Load("Assets/ScriptableObjects/Cards/WordList.asset")
                .FindProperty("words");
            SerializedProperty explode = null;
            for (int i = 0; i < words.arraySize; i++)
            {
                var word = words.GetArrayElementAtIndex(i);
                Assert.That(word.FindPropertyRelative("name").stringValue, Is.Not.EqualTo("Summon"));
                if (word.FindPropertyRelative("name").stringValue == "Explode") explode = word;
            }
            Assert.That(explode, Is.Not.Null, "Explode 카드가 없다");
            var magicType = explode.FindPropertyRelative("magicType");
            Assert.That(magicType.intValue, Is.EqualTo(1), "기존 카드의 마법 번호가 바뀌었다");
            Assert.That(magicType.enumNames[magicType.enumValueIndex], Is.EqualTo("Explode"));
            Assert.That(explode.FindPropertyRelative("tag").stringValue, Is.EqualTo("Spell"));
        }

        [Test]
        public void 폭발_마법의_씬_연결과_속성_프리팹이_유지된다()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                "Assets/Scenes/TurnBattleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);
            try
            {
                SerializedObject combine = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    combine = ProjectAssets.FindComponentWithProperty(root, "explode");
                    if (combine != null) break;
                }
                Assert.That(combine, Is.Not.Null, "조합창의 explode 필드가 없다");
                var spellObject = combine.FindProperty("explode").objectReferenceValue as GameObject;
                Assert.That(spellObject, Is.Not.Null, "폭발 마법의 씬 참조가 끊겼다");
                var spell = ProjectAssets.FindComponentWithProperty(spellObject, "firePrefab");
                Assert.That(spell, Is.Not.Null, "폭발 마법 컴포넌트가 없다");
                Assert.That(spell.targetObject.GetType().Name, Is.EqualTo("Explode"));
                foreach (string field in new[] { "firePrefab", "icePrefab", "rockPrefab", "lightningPrefab", "holyPrefab" })
                {
                    var prefab = spell.FindProperty(field).objectReferenceValue as GameObject;
                    Assert.That(prefab, Is.Not.Null, field + " 참조가 끊겼다");
                    Assert.That(System.Array.Exists(prefab.GetComponents<MonoBehaviour>(),
                        component => component != null && component.GetType().Name == "SpellObj"), Is.True,
                        field + " 에 마법 컴포넌트가 없다");
                }
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            }
        }

        [TestCase(MagicCardPrefab)]
        [TestCase(TypeCardPrefab)]
        public void 카드_프리팹이_두_스프라이트를_들고_있다(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path + " 을(를) 로드하지 못했다");

            SerializedObject card = ProjectAssets.FindComponentWithProperty(prefab, "magicCard");
            Assert.That(card, Is.Not.Null, path + " 에 magicCard 필드를 가진 컴포넌트가 없다");

            Assert.That(card.FindProperty("magicCard").objectReferenceValue, Is.Not.Null,
                path + " 의 magicCard 스프라이트 참조가 끊겼다");
            Assert.That(card.FindProperty("typeCard").objectReferenceValue, Is.Not.Null,
                path + " 의 typeCard 스프라이트 참조가 끊겼다");
        }

        [Test]
        public void 튜토리얼_컨트롤러가_대화창과_blocker를_들고_있다()
        {
            SerializedObject controller = TutorialControllerComponent();

            Assert.That(controller.FindProperty("tutorialChatWindow").objectReferenceValue, Is.Not.Null,
                "tutorialChatWindow 참조가 끊겼다");
            Assert.That(controller.FindProperty("inputBlocker").objectReferenceValue, Is.Not.Null,
                "inputBlocker 참조가 끊겼다. 대화창 뒤의 UI가 그대로 눌린다");
        }

        [Test]
        public void blocker가_화면_전체를_덮고_클릭을_받는다()
        {
            GameObject blocker = (GameObject)TutorialControllerComponent()
                .FindProperty("inputBlocker").objectReferenceValue;

            Graphic graphic = blocker.GetComponent<Graphic>();
            Assert.That(graphic, Is.Not.Null, "blocker에 Graphic이 없어 레이캐스트를 받지 못한다");
            Assert.That(graphic.raycastTarget, Is.True, "blocker의 raycastTarget이 꺼져 있다");

            RectTransform rect = blocker.GetComponent<RectTransform>();
            Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero), "blocker가 화면 전체를 덮지 않는다");
            Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one), "blocker가 화면 전체를 덮지 않는다");
            Assert.That(rect.sizeDelta, Is.EqualTo(Vector2.zero), "blocker의 sizeDelta가 0이 아니다");
        }

        [Test]
        public void 튜토리얼_캔버스가_게임_캔버스보다_위에_있다()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TutorialPrefab);
            Canvas canvas = prefab.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null, "튜토리얼 프리팹에 Canvas가 없다");

            int highestInScenes = SceneCanvasOrder.Highest();
            Assert.That(canvas.sortingOrder, Is.GreaterThan(highestInScenes),
                "씬 캔버스(최대 " + highestInScenes + ")가 blocker 위에 깔린다");
        }

        static SerializedObject TutorialControllerComponent()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TutorialPrefab);
            Assert.That(prefab, Is.Not.Null, TutorialPrefab + " 을(를) 로드하지 못했다");

            SerializedObject controller = ProjectAssets.FindComponentWithProperty(prefab, "inputBlocker");
            Assert.That(controller, Is.Not.Null, "inputBlocker 필드를 가진 컴포넌트가 없다");
            return controller;
        }
    }
}
