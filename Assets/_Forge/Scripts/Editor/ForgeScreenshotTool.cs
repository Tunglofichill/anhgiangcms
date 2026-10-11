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

            // 1. Disable all reflection probes across scene
            var probes = Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
            foreach (var p in probes) p.enabled = false;
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }

            // 2. Stylized fantasy material tune-up (True pure PBR)
            Material matBB = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Forge/Art/Materials/Mat_Bronze_Brass.mat");
            if (matBB != null)
            {
                matBB.SetColor("_BaseColor", new Color(1.0f, 0.86f, 0.20f));
                matBB.SetFloat("_Metallic", 0.30f);
                matBB.SetFloat("_Smoothness", 0.65f);
                matBB.SetFloat("_EnvironmentReflections", 0f);
                matBB.DisableKeyword("_EMISSION");
                matBB.SetColor("_EmissionColor", Color.black);
                matBB.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }

            Material matSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Forge/Art/Materials/Mat_Steel_Polished.mat");
            if (matSteel != null)
            {
                matSteel.SetColor("_BaseColor", new Color(0.92f, 0.96f, 1.0f));
                matSteel.SetFloat("_Metallic", 0.35f);
                matSteel.SetFloat("_Smoothness", 0.70f);
                matSteel.SetFloat("_EnvironmentReflections", 0f);
                matSteel.DisableKeyword("_EMISSION");
                matSteel.SetColor("_EmissionColor", Color.black);
                matSteel.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }

            Material matDarkIron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Forge/Art/Materials/Mat_Dark_Iron.mat");
            if (matDarkIron != null)
            {
                matDarkIron.SetColor("_BaseColor", new Color(0.26f, 0.28f, 0.32f));
                matDarkIron.SetFloat("_Metallic", 0.25f);
                matDarkIron.SetFloat("_Smoothness", 0.45f);
                matDarkIron.SetFloat("_EnvironmentReflections", 0f);
                matDarkIron.DisableKeyword("_EMISSION");
                matDarkIron.SetColor("_EmissionColor", Color.black);
                matDarkIron.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }

            Material matStone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Forge/Art/Materials/Mat_Stone_Furnace.mat");
            if (matStone != null)
            {
                Texture2D texBasalt = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Forge/Art/Textures/PBR/Tex_Volcanic_Basalt.png");
                matStone.SetColor("_BaseColor", new Color(0.68f, 0.72f, 0.78f));
                matStone.SetFloat("_Metallic", 0.04f);
                matStone.SetFloat("_Smoothness", 0.15f);
                matStone.SetFloat("_EnvironmentReflections", 0f);
                if (texBasalt != null)
                {
                    matStone.SetTexture("_BaseMap", texBasalt);
                    matStone.mainTexture = texBasalt;
                }
                matStone.DisableKeyword("_EMISSION");
                matStone.SetColor("_EmissionColor", Color.black);
                matStone.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }

            Material matHotIngot = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Forge/Art/Materials/Mat_Hot_Ingot.mat");
            if (matHotIngot != null)
            {
                matHotIngot.SetColor("_BaseColor", new Color(1.0f, 0.38f, 0.05f));
                matHotIngot.SetFloat("_Metallic", 0.05f);
                matHotIngot.SetFloat("_Smoothness", 0.15f);
                matHotIngot.SetFloat("_EnvironmentReflections", 0f);
                matHotIngot.EnableKeyword("_EMISSION");
                matHotIngot.SetColor("_EmissionColor", new Color(1.0f, 0.40f, 0.05f) * 6.5f);
                matHotIngot.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }

            Material matFire = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Forge/Art/Materials/Mat_Furnace_Fire.mat");
            if (matFire != null)
            {
                matFire.SetColor("_BaseColor", Color.white);
                matFire.SetFloat("_Metallic", 0.05f);
                matFire.SetFloat("_Smoothness", 0.15f);
                matFire.SetFloat("_EnvironmentReflections", 0f);
                matFire.EnableKeyword("_EMISSION");
                matFire.SetColor("_EmissionColor", new Color(1.0f, 0.48f, 0.06f) * 7.5f);
                matFire.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            }

            // Tighten Crystal Light so it stays focused on crystals without leaking onto anvil
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.gameObject.name.Contains("Crystal"))
                {
                    l.range = 0.38f;
                    l.intensity = 0.32f;
                }
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
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            ForgeSetupTools.CreateEverything(true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            CaptureScreenshot();
        }
    }

    [InitializeOnLoad]
    public static class ForgeAutoRunner
    {
        private const string FLAG_FILE = "Temp/forge_auto_run.flag";

        static ForgeAutoRunner()
        {
            EditorApplication.delayCall += CheckAndRun;
        }

        public static void CheckAndRun()
        {
            if (File.Exists(FLAG_FILE))
            {
                try
                {
                    File.Delete(FLAG_FILE);
                }
                catch { }

                Debug.Log("[ForgeAutoRunner] Execution flag found! Running CreateAndCapture inside Unity Editor...");
                ForgeScreenshotTool.CreateAndCapture();
            }
        }
    }
}
