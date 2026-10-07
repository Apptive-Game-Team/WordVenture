using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class TutorialTests
    {
        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        [TestCase(1, "battleStarted")]
        [TestCase(2, "handReady")]
        [TestCase(4, "combineOpen")]
        [TestCase(5, "hasSpell")]
        [TestCase(7, "castStarted")]
        [TestCase(8, "castStarted")]
        [TestCase(9, "targetSelected")]
        [TestCase(10, "turnEnded")]
        [TestCase(11, "battleCleared")]
        public void 행동_전에는_진행하지_않고_완료하면_진행한다(int flag, string field)
        {
            Type stateType = RuntimeType("Tutorial.TutorialActionState");
            object state = Activator.CreateInstance(stateType);
            MethodInfo advance = RuntimeType("Tutorial.TutorialCondition").GetMethod("CanAdvance");
            object step = Enum.ToObject(RuntimeType("Tutorial.TutorialFlag"), flag);
            Assert.That(advance.Invoke(null, new[] { step, state }), Is.False);
            stateType.GetField(field).SetValue(state, true);
            Assert.That(advance.Invoke(null, new[] { step, state }), Is.True);
        }

        [Test]
        public void 속성_배치에는_마법도_필요하고_카드를_회수하면_기다린다()
        {
            Type stateType = RuntimeType("Tutorial.TutorialActionState");
            object state = Activator.CreateInstance(stateType);
            MethodInfo advance = RuntimeType("Tutorial.TutorialCondition").GetMethod("CanAdvance");
            object step = Enum.ToObject(RuntimeType("Tutorial.TutorialFlag"), 6);
            stateType.GetField("hasElemental").SetValue(state, true);
            Assert.That(advance.Invoke(null, new[] { step, state }), Is.False);
            stateType.GetField("hasSpell").SetValue(state, true);
            Assert.That(advance.Invoke(null, new[] { step, state }), Is.True);
            stateType.GetField("hasSpell").SetValue(state, false);
            Assert.That(advance.Invoke(null, new[] { step, state }), Is.False);
            stateType.GetField("castStarted").SetValue(state, true);
            Assert.That(advance.Invoke(null, new[] { step, state }), Is.True, "이미 시전한 행동을 다시 요구하지 않는다");
        }

        [Test]
        public void 모든_대사_단계가_한번씩_있고_초상화가_연결되어_있다()
        {
            SerializedObject asset = ProjectAssets.Load("Assets/ScriptableObjects/TutorialScript.asset");
            SerializedProperty script = asset.FindProperty("script");
            SerializedProperty portraits = asset.FindProperty("speakerImage");
            Assert.That(script.arraySize, Is.EqualTo(13));
            int[] flags = new int[script.arraySize];
            for (int i = 0; i < script.arraySize; i++)
            {
                SerializedProperty line = script.GetArrayElementAtIndex(i);
                flags[i] = line.FindPropertyRelative("tutorialFlag").intValue;
                Assert.That(line.FindPropertyRelative("text").stringValue, Is.Not.Empty);
                int portrait = line.FindPropertyRelative("portraitID").intValue;
                Assert.That(portrait, Is.InRange(0, portraits.arraySize - 1));
                Assert.That(portraits.GetArrayElementAtIndex(portrait).objectReferenceValue, Is.Not.Null);
            }
            Assert.That(flags, Is.EquivalentTo(Enumerable.Range(1, 13)));
        }

        [Test]
        public void 스킵_UI는_blocker_위에_있고_안내는_입력을_막지_않는다()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tutorial/TutorialController.prefab");
            GameObject instance = Object.Instantiate(prefab);
            try
            {
                SerializedObject controller = ProjectAssets.FindComponentWithProperty(instance, "inputBlocker");
                Object overlay = controller.FindProperty("overlay").objectReferenceValue;
                Assert.That(overlay, Is.Not.Null);
                Assert.That(new SerializedObject(overlay).FindProperty("font").objectReferenceValue, Is.Not.Null);
                Assert.That(new SerializedObject(overlay).FindProperty("dragHand").objectReferenceValue, Is.Not.Null);
                overlay.GetType().GetMethod("Initialize").Invoke(overlay, new[] { controller.targetObject });
                Transform root = instance.transform.Find("TutorialOverlay");
                Transform blocker = ((GameObject)controller.FindProperty("inputBlocker").objectReferenceValue).transform;
                Assert.That(root.GetSiblingIndex(), Is.GreaterThan(blocker.GetSiblingIndex()));
                Assert.That(root.Find("SkipTutorial").GetComponent<Button>(), Is.Not.Null);
                Assert.That(root.Find("ActionGuidance").GetComponent<Graphic>().raycastTarget, Is.False);
                Assert.That(root.Find("ActionGuidance").GetComponent<CanvasRenderer>(), Is.Not.Null);
                Assert.That(root.Find("DragDemo/DemoHand").GetComponent<Image>().raycastTarget, Is.False);
                Assert.That(root.Find("DragDemo/DemoCard").GetComponent<Image>().raycastTarget, Is.False);
                Assert.That(root.Find("ActionHint").GetComponent<Graphic>().raycastTarget, Is.False);
                overlay.GetType().GetMethod("ShowConfirmation").Invoke(overlay, new object[] { true });
                Transform modal = root.Find("SkipConfirmation");
                Assert.That(modal.gameObject.activeSelf, Is.True);
                Assert.That(modal.GetComponent<Graphic>().raycastTarget, Is.True);
                Assert.That(modal.GetSiblingIndex(), Is.GreaterThan(root.Find("SkipTutorial").GetSiblingIndex()));
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void 종료_선택은_저장되고_새_게임에서_초기화된다()
        {
            bool hadTutorial = PlayerPrefs.HasKey("TutorialEnded"), hadStage = PlayerPrefs.HasKey("StagePosition");
            int tutorial = PlayerPrefs.GetInt("TutorialEnded"), stage = PlayerPrefs.GetInt("StagePosition");
            GameObject saveObject = new GameObject("TutorialSaveTest");
            try
            {
                Type saveType = RuntimeType("Core.SaveLoadController");
                PlayerPrefs.DeleteKey("TutorialEnded");
                Assert.That(saveType.GetProperty("IsTutorialEnded").GetValue(null), Is.False);
                saveType.GetMethod("MarkTutorialEnded").Invoke(null, null);
                Assert.That(saveType.GetProperty("IsTutorialEnded").GetValue(null), Is.True);
                Component save = saveObject.AddComponent(saveType);
                saveType.GetMethod("InitPlayData").Invoke(save, null);
                Assert.That(saveType.GetProperty("IsTutorialEnded").GetValue(null), Is.False);
                Assert.That(PlayerPrefs.GetInt("StagePosition"), Is.EqualTo(-1));
            }
            finally
            {
                Object.DestroyImmediate(saveObject);
                if (hadTutorial) PlayerPrefs.SetInt("TutorialEnded", tutorial); else PlayerPrefs.DeleteKey("TutorialEnded");
                if (hadStage) PlayerPrefs.SetInt("StagePosition", stage); else PlayerPrefs.DeleteKey("StagePosition");
                PlayerPrefs.Save();
            }
        }
    }
}

