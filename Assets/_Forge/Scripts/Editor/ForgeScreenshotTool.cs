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

            // Pre-warm particle systems so ambient floating embers and sparks appear mid-air
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                string pName = r.transform.parent != null ? r.transform.parent.name : "root";
                if (pName.Contains("Anvil") || pName.Contains("Furnace") || pName.Contains("Desk") || pName.Contains("Props") || r.name.Contains("Desk"))
                {
                    string matsStr = "";
                    if (r.sharedMaterials != null)
                    {
                        foreach (var m in r.sharedMaterials) matsStr += (m != null ? $"{m.name}({m.shader.name}) " : "null ");
                    }
                    Debug.Log($"[Diag] {r.gameObject.name} under {pName} has mats: {matsStr}");
                }
            }
            var allPs = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in allPs)
            {
                ps.Simulate(1.8f, true, true);
            }

            int width = 1920;
            int height = 1080;
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
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
        }

        public static void CreateAndCapture()
        {
            ForgeSetupTools.CreateEverything(true);
            CaptureScreenshot();
        }
    }
}
