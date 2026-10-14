// Editor-only baseline capture. Copy into a project copy under Assets/Editor/PixelGrid/.
// Run (without -nographics): Unity -batchmode -projectPath <copy> -executeMethod PixelGridCapture.Run -quit
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PixelGridCapture
{
    const string OutputDirectory = @"C:\temp\wv-pixel-base\captures";

    public static void Run()
    {
        Directory.CreateDirectory(OutputDirectory);
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;
            try
            {
                EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                CaptureCurrentScene(Path.GetFileNameWithoutExtension(buildScene.path));
            }
            catch (Exception e) { Debug.LogError("Capture failed " + buildScene.path + ": " + e); }
        }
    }

    static void CaptureCurrentScene(string sceneName)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            foreach (Camera c in UnityEngine.Object.FindObjectsOfType<Camera>()) { cam = c; break; }
        }
        if (cam == null) { Debug.LogWarning("No camera in " + sceneName); return; }

        foreach (Canvas canvas in UnityEngine.Object.FindObjectsOfType<Canvas>(true))
        {
            if (!canvas.isRootCanvas) continue;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            bool scaled = scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize;
            if (scaled) scaler.enabled = false; // its OnDisable resets the scale factor, so set ours afterwards
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 1f;
            }
            if (scaled)
            {
                Vector2 rf = scaler.referenceResolution;
                float logW = Mathf.Log(1920f / rf.x, 2f), logH = Mathf.Log(1080f / rf.y, 2f);
                float factor;
                if (scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand)
                    factor = Mathf.Min(1920f / rf.x, 1080f / rf.y);
                else if (scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Shrink)
                    factor = Mathf.Max(1920f / rf.x, 1080f / rf.y);
                else
                    factor = Mathf.Pow(2f, Mathf.Lerp(logW, logH, scaler.matchWidthOrHeight));
                canvas.scaleFactor = factor;
            }
        }
        Canvas.ForceUpdateCanvases();

        RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        RenderTexture prevTarget = cam.targetTexture;
        RenderTexture prevActive = RenderTexture.active;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(OutputDirectory, sceneName + ".png"), tex.EncodeToPNG());
        cam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;
        UnityEngine.Object.DestroyImmediate(tex);
        rt.Release();
        UnityEngine.Object.DestroyImmediate(rt);
        Debug.Log("Captured " + sceneName);
    }
}
