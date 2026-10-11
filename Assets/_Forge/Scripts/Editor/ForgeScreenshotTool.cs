using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Forge.EditorTools
{
    public static class ForgeScreenshotTool
    {
        [MenuItem("Forge/Debug/Capture Scene Screenshot", false, 100)]
        public static void CaptureScreenshot()
        {
            string scenePath = "Assets/_Forge/Scenes/ForgeDesk.unity";
            EditorSceneManager.OpenScene(scenePath);

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[Screenshot] No Main Camera found!");
                return;
            }

            // Deep Diagnostics: Inspect Desk and all scene renderers
            Debug.Log("[Diag] ===== DEEP SCENE DIAGNOSTICS START =====");
            var allRends = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var r in allRends)
            {
                string path = r.gameObject.name;
                Transform curr = r.transform.parent;
                while (curr != null)
                {
                    path = curr.name + "/" + path;
                    curr = curr.parent;
                }
                string matsStr = "";
                if (r.sharedMaterials != null)
                {
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m != null)
                        {
                            Color c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
                            Texture t = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : m.mainTexture;
                            float met = m.HasProperty("_Metallic") ? m.GetFloat("_Metallic") : 0f;
                            float sm = m.HasProperty("_Smoothness") ? m.GetFloat("_Smoothness") : 0f;
                            matsStr += $"{m.name}[tex={(t != null ? t.name : "none")}, col=({c.r:F2},{c.g:F2},{c.b:F2}), m={met:F2}, s={sm:F2}] ";
                        }
                        else matsStr += "null ";
                    }
                }
                Debug.Log($"[DiagRender] Obj: {path} | Pos: {r.transform.position} | Bounds: {r.bounds.center},{r.bounds.size} | Mats: {matsStr}");
            }
            Debug.Log("[Diag] ===== DEEP SCENE DIAGNOSTICS END =====");
            var allPs = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in allPs)
            {
                ps.Simulate(1.8f, true, true);
            }

            int width = 1920;
            int height = 1080;
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture prevRt = cam.targetTexture;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            cam.targetTexture = prevRt;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            Object.DestroyImmediate(screenShot);

            string outDir = "Assets/_Forge/Art/Textures/Screenshots";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            string outPath = Path.Combine(outDir, "Forge_3D_Scene_Capture.png");
            File.WriteAllBytes(outPath, bytes);
            Debug.Log($"[Screenshot] Saved capture to {outPath}");

            string artifactPath = @"C:\Users\tungnhat\.gemini\antigravity\brain\6ffa076c-c17f-4e65-89e2-9771466dcd3a\Forge_3D_Scene_Capture.png";
            try
            {
                File.WriteAllBytes(artifactPath, bytes);
                Debug.Log($"[Screenshot] Also copied capture directly to artifact: {artifactPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Screenshot] Failed copying to artifact: {ex.Message}");
            }
        }

        public static void CreateAndCapture()
        {
            ForgeTextureGenerator.GenerateAllTextures();
            ForgeObjModelGenerator.GenerateAllObjModels();
            ForgeSetupTools.CreateEverything(true);
            CaptureScreenshot();
        }
    }
}
