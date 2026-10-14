// Editor-only measurement script. Copy into a project copy under Assets/Editor/PixelGrid/.
// Run: Unity -batchmode -nographics -projectPath <copy> -executeMethod PixelGridReport.Run -quit
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PixelGridReport
{
    // next to the project folder this runs in: <project>/pixel-report.json
    static string OutputPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "pixel-report.json");
    const float ScreenPixelsPerWorldUnit = 108f; // orthographic size 5 at 1080p

    static readonly StringBuilder Json = new StringBuilder();
    static int recordCount;
    static string currentContext;
    static string currentSource;
    static Transform currentRoot;

    public static void Run()
    {
        Json.Clear();
        recordCount = 0;
        Json.Append("[\n");

        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                currentContext = "scene";
                currentSource = buildScene.path;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    currentRoot = null;
                    VisitTree(root.transform);
                }
            }
            catch (Exception e) { Debug.LogError("Scene failed " + buildScene.path + ": " + e); }
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(path);
                currentContext = "prefab";
                currentSource = path;
                currentRoot = contents.transform;
                VisitTree(contents.transform);
            }
            catch (Exception e) { Debug.LogError("Prefab failed " + path + ": " + e); }
            finally { if (contents != null) PrefabUtility.UnloadPrefabContents(contents); }
        }

        Json.Append("\n]\n");
        File.WriteAllText(OutputPath, Json.ToString());
        Debug.Log("PixelGridReport wrote " + recordCount + " records to " + OutputPath);
    }

    static void VisitTree(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            try { VisitObject(t.gameObject); }
            catch (Exception e) { Debug.LogError("Object failed " + HierarchyPath(t) + ": " + e); }
        }
    }

    static void VisitObject(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Image img = go.GetComponent<Image>();
        if (sr != null && sr.sprite != null) Emit(sr, sr.sprite, null);
        if (img != null && img.sprite != null) Emit(img, img.sprite, null);

        foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
        {
            if (mb == null) continue;
            string ns = mb.GetType().Namespace ?? "";
            if (ns.StartsWith("UnityEngine.UI") || ns.StartsWith("TMPro") || ns.StartsWith("UnityEngine.EventSystems")) continue;
            SerializedObject so = new SerializedObject(mb);
            SerializedProperty p = so.GetIterator();
            bool enter = true;
            while (p.NextVisible(enter))
            {
                enter = true;
                if (p.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (p.propertyPath == "m_Script") continue;
                Sprite sp = p.objectReferenceValue as Sprite;
                if (sp == null) continue;
                string via = mb.GetType().Name + "." + p.propertyPath;
                Component target = sr != null ? (Component)sr : (img != null ? (Component)img : null);
                Emit(target, sp, via, go);
            }
        }

        Animator anim = go.GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            HashSet<AnimationClip> seen = new HashSet<AnimationClip>();
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip == null || !seen.Add(clip)) continue;
                foreach (EditorCurveBinding b in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                {
                    if (b.propertyName != "m_Sprite") continue;
                    Transform tt = string.IsNullOrEmpty(b.path) ? anim.transform : anim.transform.Find(b.path);
                    Component target = null;
                    if (tt != null)
                    {
                        if (b.type == typeof(Image)) target = tt.GetComponent<Image>();
                        else target = tt.GetComponent<SpriteRenderer>();
                    }
                    ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(clip, b);
                    HashSet<Sprite> done = new HashSet<Sprite>();
                    foreach (ObjectReferenceKeyframe k in keys)
                    {
                        Sprite sp = k.value as Sprite;
                        if (sp == null || !done.Add(sp)) continue;
                        Emit(target, sp, "anim:" + clip.name, tt != null ? tt.gameObject : go, b.path);
                    }
                }
            }
        }
    }

    static void Emit(Component target, Sprite sprite, string via, GameObject fallbackGo = null, string unresolvedPath = null)
    {
        GameObject go = target != null ? target.gameObject : fallbackGo;
        Dictionary<string, object> r = new Dictionary<string, object>();
        r["context"] = currentContext;
        r["scene_or_prefab"] = currentSource;
        r["gameObject"] = HierarchyPath(go.transform);
        r["via"] = via;
        r["sprite"] = sprite.name;
        r["texture"] = AssetDatabase.GetAssetPath(sprite.texture);
        Rect rc = sprite.rect;
        r["sprite_x"] = rc.x; r["sprite_y"] = rc.y; r["sprite_w"] = rc.width; r["sprite_h"] = rc.height;
        r["ppu"] = sprite.pixelsPerUnit;
        r["tex_w"] = sprite.texture.width; r["tex_h"] = sprite.texture.height;
        if (unresolvedPath != null && target == null) r["note"] = "animation target not resolved: " + unresolvedPath;

        Vector3 rel = Vector3.one;
        if (target is SpriteRenderer sr)
        {
            r["component"] = "SpriteRenderer";
            Vector3 s = RelativeScale(sr.transform);
            r["scale_x"] = s.x; r["scale_y"] = s.y;
            r["drawMode"] = sr.drawMode.ToString();
            if (sr.drawMode != SpriteDrawMode.Simple) { r["size_x"] = sr.size.x; r["size_y"] = sr.size.y; }
            r["texpx_x"] = Mathf.Abs(s.x) * ScreenPixelsPerWorldUnit / sprite.pixelsPerUnit;
            r["texpx_y"] = Mathf.Abs(s.y) * ScreenPixelsPerWorldUnit / sprite.pixelsPerUnit;
        }
        else if (target is Image img)
        {
            r["component"] = "Image";
            Canvas canvas = img.canvas != null ? img.canvas.rootCanvas : null;
            float refPpu = canvas != null ? canvas.referencePixelsPerUnit : 100f;
            r["canvas"] = canvas != null ? canvas.renderMode.ToString() : "none";
            RectTransform rt = img.rectTransform;
            r["imageType"] = img.type.ToString();
            r["preserveAspect"] = img.preserveAspect;
            r["rect_w"] = rt.rect.width; r["rect_h"] = rt.rect.height;
            r["anchorsPoint"] = rt.anchorMin == rt.anchorMax;
            r["ppuMultiplier"] = img.pixelsPerUnitMultiplier;
            Vector3 lossy = rt.lossyScale;
            Vector3 cl = canvas != null ? canvas.transform.lossyScale : Vector3.one;
            Vector3 s = new Vector3(Div(lossy.x, cl.x), Div(lossy.y, cl.y), 1f);
            if (currentContext == "prefab" && currentRoot != null && canvas == null) s = RelativeScale(rt);
            r["scale_x"] = s.x; r["scale_y"] = s.y;
            float dispW = rt.rect.width * Mathf.Abs(s.x), dispH = rt.rect.height * Mathf.Abs(s.y);
            float tx, ty;
            if (img.type == Image.Type.Sliced || img.type == Image.Type.Tiled)
            {
                tx = ty = refPpu / (sprite.pixelsPerUnit * Mathf.Max(0.0001f, img.pixelsPerUnitMultiplier));
            }
            else if (img.preserveAspect)
            {
                float f = Mathf.Min(dispW / rc.width, dispH / rc.height);
                tx = ty = f;
            }
            else { tx = dispW / rc.width; ty = dispH / rc.height; }
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            {
                // reference px -> world units -> screen px: lossy scale of the rect relative to world
                tx = Mathf.Abs(lossy.x) * rt.rect.width / rc.width * ScreenPixelsPerWorldUnit;
                ty = Mathf.Abs(lossy.y) * rt.rect.height / rc.height * ScreenPixelsPerWorldUnit;
                if (img.type == Image.Type.Sliced || img.type == Image.Type.Tiled)
                    tx = ty = Mathf.Abs(lossy.x) * ScreenPixelsPerWorldUnit / (sprite.pixelsPerUnit * img.pixelsPerUnitMultiplier) * refPpu / refPpu;
            }
            r["texpx_x"] = tx; r["texpx_y"] = ty;
        }
        else
        {
            r["component"] = "none";
            r["note"] = (r.ContainsKey("note") ? r["note"] + "; " : "") + "no SpriteRenderer/Image on the object";
        }
        Append(r);
    }

    static float Div(float a, float b) { return Mathf.Abs(b) < 1e-9f ? a : a / b; }

    // A prefab is measured as if it were instantiated with no parent, so its root keeps its own
    // scale (slimes 0.5, spell effects 5 or 10). In LoadPrefabContents the root has no parent,
    // so lossyScale already is that.
    static Vector3 RelativeScale(Transform t)
    {
        return t.lossyScale;
    }

    static string HierarchyPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }

    static void Append(Dictionary<string, object> r)
    {
        if (recordCount > 0) Json.Append(",\n");
        Json.Append("{");
        bool first = true;
        foreach (KeyValuePair<string, object> kv in r)
        {
            if (!first) Json.Append(",");
            first = false;
            Json.Append('"').Append(kv.Key).Append("\":");
            object v = kv.Value;
            if (v == null) Json.Append("null");
            else if (v is bool b) Json.Append(b ? "true" : "false");
            else if (v is float f) Json.Append(float.IsNaN(f) || float.IsInfinity(f) ? "null" : f.ToString("R", CultureInfo.InvariantCulture));
            else if (v is int i) Json.Append(i.ToString(CultureInfo.InvariantCulture));
            else Json.Append('"').Append(Escape(v.ToString())).Append('"');
        }
        Json.Append("}");
        recordCount++;
    }

    static string Escape(string s)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in s)
        {
            if (c == '"') sb.Append("\\\"");
            else if (c == '\\') sb.Append("\\\\");
            else if (c < 0x20) sb.Append(' ');
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
