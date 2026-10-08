using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    // 배치 검증용: 실제 게임 프리팹과 생성된 오버레이를 Unity로 렌더링한다.
    public static class ElementalStatusPreview
    {
        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);

        public static void Render()
        {
            RenderPrefab("BossCactus", "slime-overlay-preview");
            RenderPrefab("MeleeNeon", "slime-overlay-neon");
            RenderPrefab("RangedCat", "slime-overlay-cat");
        }

        static void RenderPrefab(string prefabName, string filename)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D image = null;
            Camera camera = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                var cameraObject = new GameObject("StatusPreviewCamera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                camera = cameraObject.AddComponent<Camera>();
                camera.scene = scene;
                camera.orthographic = true;
                camera.orthographicSize = 6.5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.13f, 0.21f, 0.28f);
                camera.transform.position = new Vector3(0, 0, -10);
                string[] labels = { "기본", "화상", "냉기", "빙결", "감전", "균열" };
                string[] elements = { "Holy", "Fire", "Ice", "Ice", "Lightning", "Rock" };
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Combat/Enemies/Slime/{prefabName}.prefab");
                var effects = new System.Collections.Generic.List<Component>();
                Type enemyType = RuntimeType("Combat.Enemies.Enemy");
                Type magicType = RuntimeType("Cards.MagicType");
                Type vfxType = RuntimeType("Combat.Enemies.ElementalStatusVfx");
                Type textType = RuntimeType("TMPro.TextMeshPro");
                Object font = AssetDatabase.LoadAssetAtPath<Object>("Assets/Art/Fonts/NeoDunggeunmoPro-Regular SDF.asset");
                for (int i = 0; i < labels.Length; i++)
                {
                    GameObject slime = Object.Instantiate(prefab);
                    SceneManager.MoveGameObjectToScene(slime, scene);
                    slime.transform.position = new Vector3((i % 3 - 1) * 7f, i < 3 ? 2.3f : -3.3f, 0);
                    slime.transform.localScale = new Vector3(-0.85f, 0.85f, 0.85f);
                    Component enemy = slime.GetComponent(enemyType);
                    if (slime.GetComponent(vfxType) == null)
                        enemyType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(enemy, null);
                    object state = enemyType.GetProperty("Status").GetValue(enemy);
                    if (i != 0)
                    {
                        object[] args = { Enum.Parse(magicType, elements[i]), Enum.Parse(magicType, "Shoot"), 20f, 1f, false };
                        state.GetType().GetMethod("Hit").Invoke(state, args);
                        if (i == 3) state.GetType().GetMethod("Hit").Invoke(state, args);
                    }
                    enemyType.GetMethod("UpdateIndicator").Invoke(enemy, null);
                    Component vfx = slime.GetComponent(vfxType);
                    effects.Add(vfx);
                    vfxType.GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(vfx, null);
                    var labelObject = new GameObject("Label");
                    SceneManager.MoveGameObjectToScene(labelObject, scene);
                    Component label = labelObject.AddComponent(textType);
                    textType.GetProperty("font").SetValue(label, font);
                    textType.GetProperty("text").SetValue(label, labels[i]);
                    textType.GetProperty("fontSize").SetValue(label, 3f);
                    textType.GetProperty("alignment").SetValue(label, Enum.ToObject(RuntimeType("TMPro.TextAlignmentOptions"), 514));
                    labelObject.transform.position = slime.transform.position + new Vector3(0, 2.3f, 0);
                    labelObject.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 1);
                    textType.GetMethod("ForceMeshUpdate").Invoke(label, new object[] { true, true });
                }
                target = new RenderTexture(1920, 1080, 24);
                camera.targetTexture = target;
                image = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
                string directory = Path.Combine(Directory.GetCurrentDirectory(), "Logs/StatusEffects");
                Directory.CreateDirectory(directory);
                for (int frame = 0; frame < 4; frame++)
                {
                    foreach (Component effect in effects)
                        vfxType.GetMethod("RenderFrame", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(effect, new object[] { frame });
                    camera.Render();
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                    image.Apply();
                    string suffix = frame == 0 ? string.Empty : "-frame" + frame;
                    File.WriteAllBytes(Path.Combine(directory, filename + suffix + ".png"), image.EncodeToPNG());
                }
                Debug.Log("상태 이펙트 미리보기 렌더링 완료");
            }
            finally
            {
                RenderTexture.active = previous;
                if (camera != null) camera.targetTexture = null;
                if (target != null) Object.DestroyImmediate(target);
                if (image != null) Object.DestroyImmediate(image);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
