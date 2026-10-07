#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class TutorialPlayTests
    {
        bool hadTutorial, hadStage;
        int savedTutorial, savedStage;
        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);
        static object Field(object target, string name) => target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(target);
        static void SetField(object target, string name, object value) => target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(target, value);
        static Component Tutorial => Object.FindObjectOfType(RuntimeType("Tutorial.TutorialController")) as Component;
        static bool Locked => (bool)RuntimeType("Core.InteractionLock").GetProperty("IsLocked").GetValue(null);
        static int Flag => Convert.ToInt32(Field(Tutorial, "currentFlag"));

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            hadTutorial = PlayerPrefs.HasKey("TutorialEnded");
            hadStage = PlayerPrefs.HasKey("StagePosition");
            savedTutorial = PlayerPrefs.GetInt("TutorialEnded");
            savedStage = PlayerPrefs.GetInt("StagePosition");
            PlayerPrefs.DeleteKey("TutorialEnded");
            PlayerPrefs.SetInt("StagePosition", 0);
            RuntimeType("Map.MapMove").GetField("StagePosition").SetValue(null, 0);
            if (Object.FindObjectOfType(RuntimeType("Combat.Stage.StageDataSingleton")) == null)
                new GameObject("TestStageData").AddComponent(RuntimeType("Combat.Stage.StageDataSingleton"));
            yield return SceneManager.LoadSceneAsync("Map_scene");
            yield return null;
            Assert.That(Tutorial, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (string type in new[] { "Tutorial.TutorialController", "Combat.Stage.StageDataSingleton", "Core.SaveLoadController" })
            {
                Component component = Object.FindObjectOfType(RuntimeType(type), true) as Component;
                if (component != null) Object.Destroy(component.gameObject);
            }
            yield return SceneManager.LoadSceneAsync("TitleScene");
            yield return null;
            if (hadTutorial) PlayerPrefs.SetInt("TutorialEnded", savedTutorial); else PlayerPrefs.DeleteKey("TutorialEnded");
            if (hadStage) PlayerPrefs.SetInt("StagePosition", savedStage); else PlayerPrefs.DeleteKey("StagePosition");
            PlayerPrefs.Save();
        }

        static void Acknowledge()
        {
            Call(Tutorial, "AcknowledgeDialogue");
            Call(Tutorial, "AcknowledgeDialogue");
        }

        static IEnumerator Reach(int flag)
        {
            for (int i = 0; i < 30 && Flag < flag; i++)
            {
                Acknowledge();
                yield return null;
            }
            Assert.That(Flag, Is.EqualTo(flag));
        }

        static IEnumerator StartBattle()
        {
            Acknowledge();
            yield return SceneManager.LoadSceneAsync("TurnBattleScene");
            yield return Reach(4);
            Acknowledge();
            yield return null;
        }

        static object Manager => Object.FindObjectOfType(RuntimeType("Cards.CardManager"));

        // 배치 모드에서는 WaitForEndOfFrame이 재개되지 않으므로 카메라를 명시적으로 렌더한다.
        static void CapturePreview(string path)
        {
            Camera camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>()
                .Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            Camera[] cameras = canvases.Select(canvas => canvas.worldCamera).ToArray();
            float[] distances = canvases.Select(canvas => canvas.planeDistance).ToArray();
            RenderTexture previous = camera.targetTexture, active = RenderTexture.active;
            var render = new RenderTexture(Screen.width, Screen.height, 24);
            var image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = render;
                foreach (Canvas canvas in canvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1;
                }
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = render;
                image.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
                image.Apply();
                System.IO.File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceOverlay;
                    canvases[i].worldCamera = cameras[i];
                    canvases[i].planeDistance = distances[i];
                }
                Object.Destroy(image);
                render.Release();
                Object.Destroy(render);
                Canvas.ForceUpdateCanvases();
            }
        }
        static Component Zone => Object.FindObjectOfType(RuntimeType("Combat.UI.CombineZone"), true) as Component;
        static IList Slots(string name) => (IList)Field(Zone, name);

        static Component Place(string tag, string pushField)
        {
            Component card = Call(Manager, "GetTutorialCard", tag) as Component;
            Assert.That(card, Is.Not.Null);
            SetField(Manager, "selectCard", card);
            Call(Manager, "CardMouseDown");
            SetField(Manager, pushField, true);
            Call(Manager, "CardMouseUp");
            SetField(Manager, pushField, false);
            return card;
        }

        [UnityTest]
        public IEnumerator 첫_대사_스킵을_취소하면_대사를_유지하고_확정하면_저장한다()
        {
            Component tutorial = Tutorial;
            Transform ui = tutorial.transform.Find("TutorialOverlay");
            float expectedWidth = 250 * Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), 0.4f, 2f);
            Assert.That(ui.Find("SkipTutorial").GetComponent<RectTransform>().rect.width
                * tutorial.GetComponent<Canvas>().scaleFactor, Is.EqualTo(expectedWidth).Within(1));
            ui.Find("SkipTutorial").GetComponent<Button>().onClick.Invoke();
            Assert.That(Locked, Is.True);
            Call(tutorial, "AcknowledgeDialogue");
            Assert.That(Flag, Is.EqualTo(1));
            Call(tutorial, "CancelSkip");
            yield return null;
            Assert.That(Locked, Is.True, "취소 후 대사 입력 차단을 유지한다");
            Assert.That(Flag, Is.EqualTo(1));
            Call(tutorial, "RequestSkip");
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                CapturePreview("Logs/tutorial-skip-confirmation.png");
            }
            ui.Find("SkipConfirmation/ConfirmationPanel/ConfirmSkip").GetComponent<Button>().onClick.Invoke();
            Assert.That(PlayerPrefs.GetInt("TutorialEnded"), Is.EqualTo(1));
            yield return null;
            yield return null;
            Assert.That(Tutorial, Is.Null);
            Assert.That(Locked, Is.False);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Map_scene"));
        }

        [UnityTest]
        public IEnumerator 카드_회수와_재개방을_안내하고_실제_시전과_대상선택을_기다린다()
        {
            yield return StartBattle();
            Assert.That(Flag, Is.EqualTo(4));
            object openButton = Object.FindObjectOfType(RuntimeType("Combat.UI.CombineButton"));
            Call(openButton, "OnButtonClick");
            yield return Reach(5);
            Acknowledge();
            yield return null;
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                yield return new WaitForSeconds(0.8f); // 최초 카드 정렬 애니메이션을 기다린다.
                Graphic guide = Tutorial.transform.Find("TutorialOverlay/ActionGuidance").GetComponent<Graphic>();
                using (var mesh = new VertexHelper())
                {
                    guide.GetType().GetMethod("OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                        null, new[] { typeof(VertexHelper) }, null).Invoke(guide, new object[] { mesh });
                    Assert.That(mesh.currentVertCount, Is.GreaterThan(32), "카드·슬롯 모서리 강조가 생성되어야 한다");
                }
                Mesh rendered = guide.canvasRenderer.GetMesh();
                Assert.That(rendered, Is.Not.Null);
                Assert.That(rendered.vertexCount, Is.GreaterThan(32), "실제 CanvasRenderer에 강조가 전달되어야 한다");
                Transform demo = Tutorial.transform.Find("TutorialOverlay/DragDemo");
                Assert.That(demo.Find("DemoHand").gameObject.activeInHierarchy, Is.True);
                Assert.That(demo.Find("DemoHand").GetComponent<Image>().sprite, Is.Not.Null);
                Assert.That(demo.Find("DemoCard").GetComponent<Image>().sprite, Is.Not.Null);
                Component animation = demo.GetComponent(RuntimeType("Tutorial.TutorialDragDemo"));
                SetField(animation, "cycleStart", Time.unscaledTime - 1.2f);
                yield return null;
                yield return null;
                CapturePreview("Logs/tutorial-drag-guidance.png");
                Component original = Call(Manager, "GetTutorialCard", "Spell") as Component;
                SetField(Manager, "selectCard", original);
                Call(Manager, "CardMouseDown");
                yield return null;
                yield return null;
                Assert.That(demo.Find("DemoHand").gameObject.activeInHierarchy, Is.False, "실제 드래그를 가리지 않는다");
                Assert.That(demo.Find("DemoCard").gameObject.activeInHierarchy, Is.False);
                Call(Manager, "CancelDrag");
            }
            Component spell = Place("Spell", "onPushArea1");
            yield return Reach(6);
            Acknowledge();
            yield return null;
            SetField(Manager, "selectCard", spell);
            Call(Manager, "CardMouseDown");
            Call(Manager, "CardMouseUp");
            yield return null;
            Assert.That(Slots("spellCards").Count, Is.Zero);
            Assert.That(Flag, Is.EqualTo(6));
            Call(openButton, "OnButtonClick");
            yield return null;
            Assert.That(Zone.gameObject.activeSelf, Is.False);
            Call(openButton, "OnButtonClick");
            Place("Spell", "onPushArea1");
            Place("MagicType", "onPushArea2");
            yield return Reach(7);
            Acknowledge();
            yield return null;
            Assert.That(Flag, Is.EqualTo(7), "배치만으로 시전 완료로 판단하지 않는다");
            Call(Zone, "OnButtonClick");
            yield return Reach(9);
            Acknowledge();
            yield return null;
            Assert.That(Flag, Is.EqualTo(9));
            Transform target = Call(Zone, "GetTutorialTarget") as Transform;
            Assert.That(target, Is.Not.Null);
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Transform demo = Tutorial.transform.Find("TutorialOverlay/DragDemo");
                Component animation = demo.GetComponent(RuntimeType("Tutorial.TutorialDragDemo"));
                SetField(animation, "cycleStart", Time.unscaledTime - 0.3f);
                yield return null;
                yield return null;
                RectTransform hand = demo.Find("DemoHand").GetComponent<RectTransform>();
                Assert.That(hand.gameObject.activeInHierarchy, Is.True, "적 선택에도 손 안내가 보여야 한다");
                Assert.That(demo.Find("DemoCard").gameObject.activeInHierarchy, Is.False);
                Vector2 pressed = hand.anchoredPosition;
                CapturePreview("Logs/tutorial-click-guidance.png");
                SetField(animation, "cycleStart", Time.unscaledTime - 0.9f);
                yield return null;
                yield return null;
                Assert.That(Vector2.Distance(pressed, hand.anchoredPosition), Is.GreaterThan(1), "클릭을 시연하며 손이 되돌아와야 한다");
                Call(Tutorial, "RequestSkip");
                Assert.That(hand.gameObject.activeInHierarchy, Is.False);
                Call(Tutorial, "CancelSkip");
                yield return null;
            }
            Call(Zone, "SetTarget", target.GetComponent(RuntimeType("Combat.Enemies.SelectableObject")));
            yield return Reach(10);
            yield return null;
            Transform overlayRoot = Tutorial.transform.Find("TutorialOverlay");
            Assert.That(overlayRoot.Find("ActionGuidance").gameObject.activeInHierarchy, Is.False, "대사 중에는 강조를 숨긴다");
            Assert.That(overlayRoot.Find("DragDemo").gameObject.activeInHierarchy, Is.False, "대사 중에는 손 시연을 숨긴다");
            Assert.That(overlayRoot.Find("ActionHint").gameObject.activeInHierarchy, Is.False);
            object turns = Object.FindObjectOfType(RuntimeType("Battle.Turns.TurnBattleSystem"));
            Call(turns, "TurnEndButton");
            Assert.That((bool)Field(Tutorial, "turnEnded"), Is.False, "대사 중에는 턴 종료도 막는다");
            Image dim = ((GameObject)Field(Tutorial, "inputBlocker")).GetComponent<Image>();
            Assert.That(dim.gameObject.activeInHierarchy, Is.True);
            Assert.That(dim.color.a, Is.InRange(0.3f, 0.7f));
            Assert.That(dim.transform.GetSiblingIndex(), Is.LessThan(((Component)Field(Tutorial, "tutorialChatWindow")).transform.GetSiblingIndex()));
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                CapturePreview("Logs/tutorial-dialogue-dim.png");
            Acknowledge();
            yield return null;
            yield return null;
            Assert.That(Flag, Is.EqualTo(10), "대상 선택만으로 턴 종료 단계가 지나가지 않는다");
            Assert.That(dim.gameObject.activeInHierarchy, Is.False);
            Assert.That(overlayRoot.Find("ActionGuidance").gameObject.activeInHierarchy, Is.True);
            Assert.That(overlayRoot.Find("DragDemo").gameObject.activeInHierarchy, Is.True);
            Button endButton = Field(Tutorial, "turnEndButton") as Button;
            Assert.That(endButton, Is.Not.Null);
            endButton.onClick.Invoke();
            yield return Reach(11);
        }

        [UnityTest]
        public IEnumerator 드래그_중_스킵은_배치를_복원하고_주문_대기_중_스킵은_주문을_유지한다()
        {
            yield return StartBattle();
            Call(Object.FindObjectOfType(RuntimeType("Combat.UI.CombineButton")), "OnButtonClick");
            yield return Reach(5);
            Acknowledge();
            yield return null;
            Component spell = Place("Spell", "onPushArea1");
            yield return Reach(6);
            Acknowledge();
            yield return null;
            Vector3 position = spell.transform.position;
            SetField(Manager, "selectCard", spell);
            Call(Manager, "CardMouseDown");
            spell.transform.position += Vector3.up;
            Call(Tutorial, "RequestSkip");
            Assert.That(spell.transform.position, Is.EqualTo(position));
            Assert.That(Slots("spellCards")[0], Is.EqualTo(spell.gameObject));
            Call(Tutorial, "CancelSkip");
            yield return null;
            Place("MagicType", "onPushArea2");
            yield return Reach(7);
            Acknowledge();
            yield return null;
            Call(Zone, "OnButtonClick");
            Component zone = Zone;
            Assert.That(zone.GetType().GetProperty("IsAwaitingTarget").GetValue(zone), Is.True);
            Call(Tutorial, "RequestSkip");
            Call(Tutorial, "ConfirmSkip");
            yield return null;
            yield return null;
            Assert.That(zone.GetType().GetProperty("IsAwaitingTarget").GetValue(zone), Is.True);
            Assert.That(Call(zone, "GetTutorialTarget"), Is.Not.Null);
            Assert.That(Locked, Is.False);
        }

        [UnityTest]
        public IEnumerator 마지막_대사는_읽은_뒤_종료하고_이어하기에서는_재등장하지_않는다()
        {
            Component tutorial = Tutorial;
            SetField(tutorial, "currentFlag", Enum.ToObject(RuntimeType("Tutorial.TutorialFlag"), 13));
            Call(tutorial, "StoryTelling");
            yield return null;
            Assert.That(Tutorial, Is.Not.Null);
            Assert.That(PlayerPrefs.GetInt("TutorialEnded", 0), Is.Zero);
            Acknowledge();
            yield return null;
            yield return null;
            Assert.That(Tutorial, Is.Null);
            yield return SceneManager.LoadSceneAsync("Map_scene");
            yield return null;
            Assert.That(Tutorial, Is.Null);
            object save = Object.FindObjectOfType(RuntimeType("Core.SaveLoadController"));
            if (save == null) save = new GameObject("SaveTest").AddComponent(RuntimeType("Core.SaveLoadController"));
            Call(save, "InitPlayData");
            RuntimeType("Map.MapMove").GetField("StagePosition").SetValue(null, 0);
            yield return SceneManager.LoadSceneAsync("Map_scene");
            yield return null;
            Assert.That(Tutorial, Is.Not.Null);
            Assert.That(Flag, Is.EqualTo(1));
        }
    }
}
#endif
