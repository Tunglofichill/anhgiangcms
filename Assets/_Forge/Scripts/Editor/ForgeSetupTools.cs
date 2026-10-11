using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Forge.EditorTools
{
    public static class ForgeSetupTools
    {
        private const string ROOT_DIR = "Assets/_Forge";
        private const string SCENE_PATH = "Assets/_Forge/Scenes/ForgeDesk.unity";
        private const string BALANCE_ASSET_PATH = "Assets/_Forge/Data/Balance.asset";
        private const string GENERATORS_DIR = "Assets/_Forge/Data/Generators";
        private const string MATERIALS_DIR = "Assets/_Forge/Art/Materials";
        private const string PREFABS_DIR = "Assets/_Forge/Prefabs/Machines";
        private const string FX_PREFABS_DIR = "Assets/_Forge/Prefabs/FX";
        private const string UI_PREFABS_DIR = "Assets/_Forge/Prefabs/UI";
        private const string URP_SETTINGS_DIR = "Assets/_Forge/Settings/URP";

        [MenuItem("Forge/Setup/Create Everything", false, 0)]
        public static void CreateEverything()
        {
            CreateEverything(false);
        }

        public static void CreateEverything(bool quiet)
        {
            Debug.Log("[ForgeSetup] === Starting Forge Setup ===");

            EnsureDirectories();
            ConfigureAllArtTextures();
            SetupUrpPipeline();
            BalanceConfig balance = CreateOrLoadBalanceConfig();
            Dictionary<string, Material> mats = CreateMaterials();
            Dictionary<string, GameObject> machinePrefabs = CreateMachinePrefabs(mats);
            List<GeneratorDefinition> defs = CreateOrLoadGeneratorDefinitions(machinePrefabs);

            SetupScene(balance, defs, mats);
            AssignIconsToGenerators();
            ValidateScene();

            Debug.Log("[ForgeSetup] === Forge Setup Completed Successfully! ===");
            if (!Application.isBatchMode && !quiet)
            {
                EditorUtility.DisplayDialog("Forge Setup", "Rigged ForgeDesk scene, Balance.asset, 5 Generators, Lighting & UI successfully!", "OK");
            }
        }

        private static void EnsureDirectories()
        {
            string[] dirs = new string[]
            {
                "Assets/_Forge/Art/Models",
                "Assets/_Forge/Art/Textures",
                "Assets/_Forge/Art/Textures/Icons",
                "Assets/_Forge/Art/Materials",
                "Assets/_Forge/Art/VFX",
                "Assets/_Forge/Art/Audio",
                "Assets/_Forge/Data/Generators",
                "Assets/_Forge/Data/Milestones",
                "Assets/_Forge/Prefabs/Machines",
                "Assets/_Forge/Prefabs/FX",
                "Assets/_Forge/Prefabs/UI",
                "Assets/_Forge/Scenes",
                "Assets/_Forge/Scripts/Core",
                "Assets/_Forge/Scripts/Economy",
                "Assets/_Forge/Scripts/World",
                "Assets/_Forge/Scripts/UI",
                "Assets/_Forge/Scripts/Save",
                "Assets/_Forge/Scripts/FX",
                "Assets/_Forge/Scripts/Audio",
                "Assets/_Forge/Scripts/Editor",
                "Assets/_Forge/Settings/URP"
            };

            foreach (var d in dirs)
            {
                if (!AssetDatabase.IsValidFolder(d))
                {
                    Directory.CreateDirectory(d);
                }
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Forge/Setup/Build or Update UI (Main Menu & Navigation)", false, 1)]
        public static void BuildOrUpdateUI()
        {
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gm == null)
            {
                Debug.LogError("[ForgeSetup] GameManager not found in scene!");
                return;
            }

            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ForgeSetup] Canvas not found in scene!");
                return;
            }

            Font defaultFont = GetDefaultFont();

            // Check or create MainMenu
            var existingMainMenu = canvas.GetComponentInChildren<MainMenuView>(true);
            if (existingMainMenu != null)
            {
                UnityEngine.Object.DestroyImmediate(existingMainMenu.panelMainMenu != null ? existingMainMenu.panelMainMenu : existingMainMenu.gameObject);
                UnityEngine.Object.DestroyImmediate(existingMainMenu);
            }

            var mmView = CreateMainMenuPanel(canvas.gameObject, defaultFont, gm);
            gm.mainMenuView = mmView;

            // Check or update Settings
            var settingsView = canvas.GetComponentInChildren<SettingsView>(true);
            if (settingsView != null && settingsView.panelSettings != null)
            {
                var existingReturnBtn = settingsView.panelSettings.transform.Find("BtnReturnMainMenu");
                if (existingReturnBtn == null)
                {
                    float yPos = -350f;
                    var (btnReturn, imgReturn, txtReturn) = CreateButtonWithText(settingsView.panelSettings, "BtnReturnMainMenu", "🏠 VỀ MENU CHÍNH", defaultFont, 20, new Vector2(-190f, yPos), new Vector2(360f, 52f), new Vector2(0.5f, 1f));
                    imgReturn.color = new Color(0.20f, 0.35f, 0.55f, 0.95f);
                    settingsView.btnReturnMainMenu = btnReturn;
                    settingsView.txtReturnMainMenu = txtReturn;

                    if (settingsView.btnResetData != null)
                    {
                        var rtReset = settingsView.btnResetData.GetComponent<RectTransform>();
                        rtReset.anchoredPosition = new Vector2(190f, yPos);
                        rtReset.sizeDelta = new Vector2(360f, 52f);
                    }
                }
            }

            EditorUtility.SetDirty(canvas.gameObject);
            EditorUtility.SetDirty(gm);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("[ForgeSetup] Successfully built/updated Main Menu and Settings navigation in scene!");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Main Menu Setup", "Tạo và cấu hình thành công Main Menu & Nút Về Menu Chính!", "OK");
            }
        }

        [MenuItem("Forge/Setup/Configure Art Textures", false, 3)]
        public static void ConfigureAllArtTextures()
        {
            string[] spritePaths = new[]
            {
                "Assets/_Forge/Art/Textures/Hero_Furnace.png",
                "Assets/_Forge/Art/Textures/Hero_Anvil.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_Ore_Nugget.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_Tutorial_Hand.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G1_Hammer.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G2_Drill.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G3_Stove.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G4_Gear.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G5_Robot.png"
            };

            foreach (var p in spritePaths)
            {
                var importer = AssetImporter.GetAtPath(p) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
            }

            string woodPath = "Assets/_Forge/Art/Textures/Tex_WoodPlanks.png";
            var woodImp = AssetImporter.GetAtPath(woodPath) as TextureImporter;
            if (woodImp != null)
            {
                woodImp.textureType = TextureImporterType.Default;
                woodImp.wrapMode = TextureWrapMode.Repeat;
                woodImp.SaveAndReimport();
            }

            string bgPath = "Assets/_Forge/Art/Textures/Tex_WorkshopBackdrop.png";
            var bgImp = AssetImporter.GetAtPath(bgPath) as TextureImporter;
            if (bgImp != null)
            {
                bgImp.textureType = TextureImporterType.Default;
                bgImp.wrapMode = TextureWrapMode.Clamp;
                bgImp.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ForgeSetup] Configured art textures and sprites.");
        }

        private static Font GetDefaultFont()
        {
            Font defaultFont = null;
            try { defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (defaultFont == null)
            {
                try { defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            if (defaultFont == null)
            {
                try { defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 16); } catch { }
            }
            return defaultFont;
        }

        private static void SetupUrpPipeline()
        {
            string assetPath = "Assets/_Forge/Settings/URP/Forge_URP_Asset.asset";
            string rendererPath = "Assets/_Forge/Settings/URP/Forge_URP_Renderer.asset";

            var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
            if (urpAsset == null)
            {
                var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
                if (rendererData == null)
                {
                    rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                    rendererData.name = "Forge_URP_Renderer";
                    AssetDatabase.CreateAsset(rendererData, rendererPath);
                }

                try
                {
                    urpAsset = UniversalRenderPipelineAsset.Create(rendererData);
                }
                catch
                {
                    urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                }
                urpAsset.name = "Forge_URP_Asset";
                urpAsset.shadowDistance = 12f;
                urpAsset.supportsHDR = true;

                AssetDatabase.CreateAsset(urpAsset, assetPath);
                AssetDatabase.SaveAssets();

                try { GraphicsSettings.defaultRenderPipeline = urpAsset; } catch { }
                QualitySettings.renderPipeline = urpAsset;
                Debug.Log("[ForgeSetup] Configured URP Asset & Renderer.");
            }
        }

        private static BalanceConfig CreateOrLoadBalanceConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BalanceConfig>(BALANCE_ASSET_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BalanceConfig>();
                config.tapBase = 1.0;
                config.costMult = 1.15;
                config.outputMult = 1.12;
                config.g1TapBonusRate = 0.25;
                config.g5GlobalBonusRate = 0.05;
                config.maxOfflineSeconds = 21600f; // 6 hours
                config.tickRate = 0.1f;

                AssetDatabase.CreateAsset(config, BALANCE_ASSET_PATH);
                AssetDatabase.SaveAssets();
                Debug.Log("[ForgeSetup] Created Balance.asset");
            }
            return config;
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            var dict = new Dictionary<string, Material>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            Texture2D texWood = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Forge/Art/Textures/Tex_WoodPlanks.png");
            Texture2D texBackdrop = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Forge/Art/Textures/Tex_WorkshopBackdrop.png");

            Material CreateMat(string name, Color color, float metallic = 0f, float smoothness = 0.5f, Color? emission = null, Texture2D mainTex = null, Vector2? tiling = null)
            {
                string path = $"{MATERIALS_DIR}/Mat_{name}.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader) { name = $"Mat_{name}" };
                    AssetDatabase.CreateAsset(mat, path);
                }
                mat.shader = shader;
                mat.color = color;
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

                if (mainTex != null)
                {
                    mat.mainTexture = mainTex;
                    if (tiling.HasValue) mat.mainTextureScale = tiling.Value;
                }

                if (emission.HasValue)
                {
                    mat.EnableKeyword("_EMISSION");
                    if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
                }
                EditorUtility.SetDirty(mat);
                return mat;
            }

            dict["Desk"] = CreateMat("Desk", Color.white, 0.1f, 0.35f, null, texWood, new Vector2(2.5f, 1.5f));
            dict["Floor"] = CreateMat("Floor", new Color(0.22f, 0.20f, 0.18f), 0.1f, 0.25f, null, texWood, new Vector2(6f, 6f));
            dict["Wall"] = CreateMat("Wall", Color.white, 0f, 0.2f, new Color(0.12f, 0.09f, 0.07f), texBackdrop, new Vector2(1f, 1f));
            dict["Anvil"] = CreateMat("Anvil", new Color(0.25f, 0.25f, 0.28f), 0.7f, 0.4f);
            dict["FurnaceEmber"] = CreateMat("FurnaceEmber", new Color(0.16f, 0.16f, 0.18f), 0.65f, 0.45f, new Color(1.0f, 0.415f, 0.10f) * 1.5f);

            dict["Steel_Polished"] = CreateMat("Steel_Polished", new Color(0.85f, 0.88f, 0.92f), 0.85f, 0.75f);
            dict["Dark_Iron"] = CreateMat("Dark_Iron", new Color(0.20f, 0.20f, 0.24f), 0.75f, 0.35f);
            dict["Stone_Furnace"] = CreateMat("Stone_Furnace", new Color(0.38f, 0.36f, 0.35f), 0.08f, 0.20f);
            dict["Furnace_Fire"] = CreateMat("Furnace_Fire", new Color(1.0f, 0.45f, 0.10f), 0.1f, 0.8f, new Color(1.0f, 0.50f, 0.10f) * 3.5f);
            dict["Hot_Ingot"] = CreateMat("Hot_Ingot", new Color(1.0f, 0.35f, 0.05f), 0.3f, 0.6f, new Color(1.0f, 0.42f, 0.08f) * 2.8f);
            dict["Bronze_Brass"] = CreateMat("Bronze_Brass", new Color(0.85f, 0.65f, 0.28f), 0.80f, 0.60f);
            dict["Ore_Crystal"] = CreateMat("Ore_Crystal", new Color(0.20f, 0.85f, 0.80f), 0.40f, 0.85f, new Color(0.10f, 0.70f, 0.65f) * 1.6f);
            dict["Wood_Stump"] = CreateMat("Wood_Stump", new Color(0.38f, 0.28f, 0.20f), 0.05f, 0.25f, null, texWood, new Vector2(2f, 2f));

            // Greybox machine colors
            dict["G1"] = CreateMat("G1", new Color(0.85f, 0.64f, 0.25f), 0.5f, 0.5f);
            dict["G2"] = CreateMat("G2", new Color(0.43f, 0.54f, 0.35f), 0.5f, 0.5f);
            dict["G3"] = CreateMat("G3", new Color(0.77f, 0.36f, 0.15f), 0.5f, 0.5f);
            dict["G4"] = CreateMat("G4", new Color(0.48f, 0.52f, 0.58f), 0.6f, 0.6f);
            dict["G5"] = CreateMat("G5", new Color(0.36f, 0.49f, 1.00f), 0.7f, 0.7f);
            dict["Decor"] = CreateMat("Decor", new Color(0.95f, 0.85f, 0.45f), 0.5f, 0.5f);

            // Particle FX Materials (URP Particles/Unlit - fixes pink shaders)
            Material matSpark = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_DIR}/Mat_FX_Spark.mat");
            if (matSpark != null) dict["FX_Spark"] = matSpark;
            Material matOre = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_DIR}/Mat_FX_Ore.mat");
            if (matOre != null) dict["FX_Ore"] = matOre;
            Material matSmoke = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIALS_DIR}/Mat_FX_Smoke.mat");
            if (matSmoke != null) dict["FX_Smoke"] = matSmoke;

            AssetDatabase.SaveAssets();
            return dict;
        }

        private static Dictionary<string, GameObject> CreateMachinePrefabs(Dictionary<string, Material> mats)
        {
            var dict = new Dictionary<string, GameObject>();

            for (int i = 1; i <= 5; i++)
            {
                string id = $"G{i}";
                string prefabPath = $"{PREFABS_DIR}/Machine_{id}.prefab";

                GameObject tempGo = Forge3DBuilder.BuildMachine3D(id, null, Vector3.zero, Quaternion.identity, mats);
                tempGo.name = $"Machine_{id}";

                var prefab = PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);
                UnityEngine.Object.DestroyImmediate(tempGo);
                Debug.Log($"[ForgeSetup] Created 3D Prefab {prefabPath}");
                dict[id] = prefab;
            }
            AssetDatabase.SaveAssets();
            return dict;
        }

        private static List<GeneratorDefinition> CreateOrLoadGeneratorDefinitions(Dictionary<string, GameObject> machinePrefabs)
        {
            var list = new List<GeneratorDefinition>();

            var gData = new[]
            {
                new { id = "G1", name = "Tay Búa Tí Hon", cost = 15.0, outVal = 0.5, unlOre = 0.0, unlGen = "", unlLv = 0 },
                new { id = "G2", name = "Cúi Đào Quặng", cost = 100.0, outVal = 2.0, unlOre = 50.0, unlGen = "G1", unlLv = 1 },
                new { id = "G3", name = "Lò Than Tự Đun", cost = 750.0, outVal = 8.0, unlOre = 0.0, unlGen = "G2", unlLv = 10 },
                new { id = "G4", name = "Cối Xay Kim", cost = 6000.0, outVal = 40.0, unlOre = 0.0, unlGen = "G3", unlLv = 10 },
                new { id = "G5", name = "Robot Thợ Cả", cost = 50000.0, outVal = 200.0, unlOre = 0.0, unlGen = "G4", unlLv = 10 }
            };

            foreach (var item in gData)
            {
                string path = $"{GENERATORS_DIR}/Gen_{item.id}.asset";
                var def = AssetDatabase.LoadAssetAtPath<GeneratorDefinition>(path);
                if (def == null)
                {
                    def = ScriptableObject.CreateInstance<GeneratorDefinition>();
                    def.id = item.id;
                    def.machineName = item.name;
                    def.baseCost = item.cost;
                    def.baseOutput = item.outVal;
                    def.unlockOre = item.unlOre;
                    def.unlockGenId = item.unlGen;
                    def.unlockLevel = item.unlLv;

                    if (machinePrefabs != null && machinePrefabs.TryGetValue(item.id, out var p))
                    {
                        def.prefab = p;
                    }

                    AssetDatabase.CreateAsset(def, path);
                }
                else
                {
                    if (machinePrefabs != null && machinePrefabs.TryGetValue(item.id, out var p))
                    {
                        def.prefab = p;
                        EditorUtility.SetDirty(def);
                    }
                }
                list.Add(def);
            }
            AssetDatabase.SaveAssets();
            return list;
        }

        private static void SetupScene(BalanceConfig balance, List<GeneratorDefinition> defs, Dictionary<string, Material> mats)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            int clickableLayer = LayerMask.NameToLayer("Clickable");
            if (clickableLayer < 0) clickableLayer = 0;

            // 1. [GameSystems]
            GameObject goSystems = new GameObject("[GameSystems]");
            var gameManager = goSystems.AddComponent<GameManager>();
            var saveService = goSystems.AddComponent<SaveService>();
            var offlineService = goSystems.AddComponent<OfflineService>();
            var economyService = goSystems.AddComponent<EconomyService>();
            var milestoneService = goSystems.AddComponent<MilestoneService>();
            var audioService = goSystems.AddComponent<AudioService>();
            audioService.bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Forge/Art/Audio/Dwarven Forge Pulse.mp3");
            var settingsService = goSystems.AddComponent<SettingsService>();
            var tickRunner = goSystems.AddComponent<TickRunner>();

            gameManager.balanceConfig = balance;
            gameManager.generatorDefinitions = defs;
            gameManager.saveService = saveService;
            gameManager.offlineService = offlineService;
            gameManager.economyService = economyService;
            gameManager.milestoneService = milestoneService;
            gameManager.audioService = audioService;
            gameManager.settingsService = settingsService;
            gameManager.tickRunner = tickRunner;

            economyService.balanceConfig = balance;
            economyService.generatorDefinitions = defs;

            // 2. [World]
            GameObject goWorld = new GameObject("[World]");

            // 2.1 Lighting
            GameObject goLighting = new GameObject("Lighting");
            goLighting.transform.SetParent(goWorld.transform);

            // Sun_Key
            GameObject goSun = new GameObject("Sun_Key");
            goSun.transform.SetParent(goLighting.transform);
            goSun.transform.position = new Vector3(2.4f, 3.6f, -1.8f);
            goSun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            Light sunLight = goSun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1.0f, 0.88f, 0.74f);
            sunLight.intensity = 1.1f;
            sunLight.shadows = LightShadows.Soft;

            // Fill_Soft
            GameObject goFill = new GameObject("Fill_Soft");
            goFill.transform.SetParent(goLighting.transform);
            goFill.transform.position = new Vector3(-2.8f, 2.4f, -1.2f);
            goFill.transform.rotation = Quaternion.Euler(20f, 40f, 0f);
            Light fillLight = goFill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.56f, 0.66f, 0.85f);
            fillLight.intensity = 0.35f;
            fillLight.shadows = LightShadows.None;

            // Rim_Cool
            GameObject goRim = new GameObject("Rim_Cool");
            goRim.transform.SetParent(goLighting.transform);
            goRim.transform.position = new Vector3(0.2f, 2.8f, 2.6f);
            goRim.transform.rotation = Quaternion.Euler(15f, 180f, 0f);
            Light rimLight = goRim.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(0.50f, 0.75f, 0.95f);
            rimLight.intensity = 0.20f;
            rimLight.shadows = LightShadows.None;

            // Furnace_Point: warm amber glow inside 3D hearth (radiates onto desk & anvil)
            GameObject goFurnacePoint = new GameObject("Furnace_Point");
            goFurnacePoint.transform.SetParent(goLighting.transform);
            goFurnacePoint.transform.position = new Vector3(1.05f, 1.25f, -0.15f);
            Light furnaceLight = goFurnacePoint.AddComponent<Light>();
            furnaceLight.type = LightType.Point;
            furnaceLight.color = new Color(1.0f, 0.60f, 0.20f);
            furnaceLight.intensity = 1.8f;
            furnaceLight.range = 2.8f;

            // EmissiveHint_Furnace
            GameObject goEmissiveHint = new GameObject("EmissiveHint_Furnace");
            goEmissiveHint.transform.SetParent(goLighting.transform);
            goEmissiveHint.transform.position = new Vector3(1.05f, 1.15f, -0.15f);
            Light hintLight = goEmissiveHint.AddComponent<Light>();
            hintLight.type = LightType.Point;
            hintLight.color = new Color(1.0f, 0.40f, 0.10f);
            hintLight.intensity = 0.6f;
            hintLight.range = 1.2f;

            var lightCtrl = goLighting.AddComponent<ForgeLightController>();
            lightCtrl.furnacePointLight = furnaceLight;
            lightCtrl.defaultBaseIntensity = 1.8f;
            lightCtrl.upgradedBaseIntensity = 2.6f;

            // 2.2 Environment
            GameObject goEnv = new GameObject("Environment");
            goEnv.transform.SetParent(goWorld.transform);

            // FloorCard: pos (0, -0.02, 0) scale (10, 0.04, 10) - non-blocking visual
            GameObject goFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goFloor.name = "FloorCard";
            goFloor.transform.SetParent(goEnv.transform);
            goFloor.transform.position = new Vector3(0f, -0.02f, 0f);
            goFloor.transform.localScale = new Vector3(10f, 0.04f, 10f);
            var colFloor = goFloor.GetComponent<Collider>();
            if (colFloor != null) UnityEngine.Object.DestroyImmediate(colFloor);
            if (mats.TryGetValue("Floor", out var mFloor)) goFloor.GetComponent<Renderer>().sharedMaterial = mFloor;

            // WallCard: pos (0, 2.75, 2.5) scale (14.0, 7.87, 1) - 16:9 Quad backdrop fully covering sky
            GameObject goWall = GameObject.CreatePrimitive(PrimitiveType.Quad);
            goWall.name = "WallCard";
            goWall.transform.SetParent(goEnv.transform);
            goWall.transform.position = new Vector3(0f, 2.75f, 2.5f);
            goWall.transform.rotation = Quaternion.identity;
            goWall.transform.localScale = new Vector3(14.0f, 7.87f, 1f);
            var colWall = goWall.GetComponent<Collider>();
            if (colWall != null) UnityEngine.Object.DestroyImmediate(colWall);
            if (mats.TryGetValue("Wall", out var mWall)) goWall.GetComponent<Renderer>().sharedMaterial = mWall;

            // Desk: pos (0, 0.40, 0) scale (4.73408, 0.80, 1.5) - non-blocking visual
            GameObject goDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goDesk.name = "Desk";
            goDesk.transform.SetParent(goEnv.transform);
            goDesk.transform.position = new Vector3(0f, 0.40f, 0f);
            goDesk.transform.localScale = new Vector3(4.73408f, 0.80f, 1.5f);
            var colDesk = goDesk.GetComponent<Collider>();
            if (colDesk != null) UnityEngine.Object.DestroyImmediate(colDesk);
            if (mats.TryGetValue("Desk", out var mDesk)) goDesk.GetComponent<Renderer>().sharedMaterial = mDesk;

            // 3D Hero Anvil (Sculpted 3D Oak Stump, Base, Waist, Steel Face, Horn, Heel & Glowing Ingot)
            GameObject goAnvil = Forge3DBuilder.BuildHeroAnvil(goEnv.transform, new Vector3(-0.72f, 0.80f, -0.05f), Quaternion.Euler(0f, 18f, 0f), mats);
            goAnvil.name = "Anvil";
            goAnvil.layer = clickableLayer;

            // BoxCollider for Anvil - Full 3D Coverage
            var boxAnvil = goAnvil.AddComponent<BoxCollider>();
            boxAnvil.size = new Vector3(1.15f, 1.20f, 0.85f);
            boxAnvil.center = new Vector3(0f, 0.52f, 0f);

            var clickAnvil = goAnvil.AddComponent<ClickForge>();

            // 3D Interactive Smith Hammer (Búa Rèn 3D vung đập khi click)
            GameObject goHammer = Forge3DBuilder.BuildHeroHammer(goEnv.transform, new Vector3(-1.38f, 0.82f, -0.05f), Quaternion.Euler(30f, 22f, -20f), mats);

            // 3D Hero Furnace (Stone Masonry Hearth, Arched Chamber, Fiery Embers & Chimney Stack)
            GameObject goFurnace = Forge3DBuilder.BuildHeroFurnace(goEnv.transform, new Vector3(1.05f, 0.80f, 0.05f), Quaternion.Euler(0f, -24f, 0f), mats);
            goFurnace.name = "Furnace";
            goFurnace.tag = "Furnace";
            goFurnace.layer = clickableLayer;

            var boxFurnace = goFurnace.AddComponent<BoxCollider>();
            boxFurnace.size = new Vector3(1.30f, 1.85f, 1.30f);
            boxFurnace.center = new Vector3(0f, 0.90f, 0f);

            var furnaceVisual = goFurnace.AddComponent<FurnaceVisual>();
            furnaceVisual.visualTransform = goFurnace.transform;

            var clickForge = goFurnace.AddComponent<ClickForge>();
            clickForge.furnaceVisual = furnaceVisual;

            // Decors 3D (Non-blocking):
            // Decor_OreShelf 3D: pos (-1.75, 0.80, 0.35) - M3
            GameObject goOreShelf = Forge3DBuilder.BuildOreShelf3D(goEnv.transform, new Vector3(-1.75f, 0.80f, 0.35f), Quaternion.Euler(0f, 22f, 0f), mats);
            goOreShelf.name = "Decor_OreShelf";
            goOreShelf.SetActive(false);

            // Decor_SmokeStack 3D: pos (0.20, 0.80, 0.30) - M4
            GameObject goSmokeStack = new GameObject("Decor_SmokeStack");
            goSmokeStack.transform.SetParent(goEnv.transform, false);
            goSmokeStack.transform.position = new Vector3(0.20f, 0.80f, 0.30f);
            var pipeBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipeBase.name = "Flange";
            pipeBase.transform.SetParent(goSmokeStack.transform, false);
            pipeBase.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            pipeBase.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);
            var c1 = pipeBase.GetComponent<Collider>(); if (c1 != null) UnityEngine.Object.DestroyImmediate(c1);
            if (mats.TryGetValue("Dark_Iron", out var mDI)) pipeBase.GetComponent<Renderer>().sharedMaterial = mDI;
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "Pipe";
            pipe.transform.SetParent(goSmokeStack.transform, false);
            pipe.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            pipe.transform.localScale = new Vector3(0.12f, 0.20f, 0.12f);
            var c2 = pipe.GetComponent<Collider>(); if (c2 != null) UnityEngine.Object.DestroyImmediate(c2);
            if (mats.TryGetValue("Bronze_Brass", out var mBB)) pipe.GetComponent<Renderer>().sharedMaterial = mBB;
            goSmokeStack.SetActive(false);

            // Decor_Sign 3D: pos (0.00, 1.55, 0.55) rot (0, 180, 0) - M5
            GameObject goSign = new GameObject("Decor_Sign");
            goSign.transform.SetParent(goEnv.transform, false);
            goSign.transform.position = new Vector3(0.00f, 1.55f, 0.55f);
            goSign.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = "Board";
            signBoard.transform.SetParent(goSign.transform, false);
            signBoard.transform.localScale = new Vector3(0.70f, 0.25f, 0.06f);
            var c3 = signBoard.GetComponent<Collider>(); if (c3 != null) UnityEngine.Object.DestroyImmediate(c3);
            if (mats.TryGetValue("Wood_Stump", out var mWS)) signBoard.GetComponent<Renderer>().sharedMaterial = mWS;
            var signRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signRim.name = "Rim";
            signRim.transform.SetParent(goSign.transform, false);
            signRim.transform.localScale = new Vector3(0.74f, 0.03f, 0.07f);
            var c4 = signRim.GetComponent<Collider>(); if (c4 != null) UnityEngine.Object.DestroyImmediate(c4);
            if (mats.TryGetValue("Bronze_Brass", out mBB)) signRim.GetComponent<Renderer>().sharedMaterial = mBB;
            goSign.SetActive(false);

            // Decor_Chest 3D: pos (1.85, 0.80, 0.20) - M7
            GameObject goChest = Forge3DBuilder.BuildChest3D(goEnv.transform, new Vector3(1.85f, 0.80f, 0.20f), Quaternion.Euler(0f, -22f, 0f), mats);
            goChest.name = "Decor_Chest";
            goChest.SetActive(false);

            // 2.3 Slots & Machines 3D
            GameObject goSlots = new GameObject("Slots");
            goSlots.transform.SetParent(goWorld.transform);

            var slotPositions = new[]
            {
                new { name = "Slot_G1", pos = new Vector3(-0.85f, 0.80f, -0.35f), rot = Quaternion.Euler(0f, 15f, 0f) },
                new { name = "Slot_G2", pos = new Vector3(-0.45f, 0.80f, -0.45f), rot = Quaternion.Euler(0f, 5f, 0f) },
                new { name = "Slot_G3", pos = new Vector3(0.00f, 0.80f, -0.50f), rot = Quaternion.identity },
                new { name = "Slot_G4", pos = new Vector3(0.45f, 0.80f, -0.45f), rot = Quaternion.Euler(0f, -5f, 0f) },
                new { name = "Slot_G5", pos = new Vector3(0.90f, 0.80f, -0.25f), rot = Quaternion.Euler(0f, -18f, 0f) }
            };

            GameObject[] machineGos = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                var sInfo = slotPositions[i];
                GameObject goSlot = new GameObject(sInfo.name);
                goSlot.transform.SetParent(goSlots.transform);
                goSlot.transform.position = sInfo.pos;
                goSlot.transform.rotation = sInfo.rot;

                string mKey = $"G{i + 1}";
                GameObject goMach = Forge3DBuilder.BuildMachine3D(mKey, goSlot.transform, Vector3.zero, Quaternion.identity, mats);
                goMach.name = $"Machine_{mKey}";
                goMach.SetActive(false);
                machineGos[i] = goMach;
            }

            // 2.4 FX
            GameObject goFx = new GameObject("FX");
            goFx.transform.SetParent(goWorld.transform);

            // FX_HitSpark
            GameObject goSpark = new GameObject("FX_HitSpark");
            goSpark.transform.SetParent(goFx.transform);
            goSpark.transform.position = new Vector3(0f, 1.0f, 0.1f);
            var psSpark = goSpark.AddComponent<ParticleSystem>();
            var rendSpark = goSpark.GetComponent<ParticleSystemRenderer>();
            if (rendSpark != null && mats.TryGetValue("FX_Spark", out var mSpark)) rendSpark.sharedMaterial = mSpark;
            var sparkMain = psSpark.main;
            sparkMain.startLifetime = 0.25f;
            sparkMain.startSpeed = 3.5f;
            sparkMain.startSize = 0.08f;
            sparkMain.startColor = new Color(1.0f, 0.69f, 0.25f);
            sparkMain.playOnAwake = false;
            var sparkEmission = psSpark.emission;
            sparkEmission.rateOverTime = 0;
            sparkEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 16) });

            // FX_OreFly
            GameObject goOreFly = new GameObject("FX_OreFly");
            goOreFly.transform.SetParent(goFx.transform);
            goOreFly.transform.position = new Vector3(0f, 1.0f, 0.1f);
            var psOreFly = goOreFly.AddComponent<ParticleSystem>();
            var rendOreFly = goOreFly.GetComponent<ParticleSystemRenderer>();
            if (rendOreFly != null && mats.TryGetValue("FX_Ore", out var mOre)) rendOreFly.sharedMaterial = mOre;
            var oreFlyMain = psOreFly.main;
            oreFlyMain.startLifetime = 0.5f;
            oreFlyMain.startSpeed = 2.0f;
            oreFlyMain.startSize = 0.06f;
            oreFlyMain.startColor = new Color(1.0f, 0.85f, 0.35f);
            oreFlyMain.playOnAwake = false;
            var oreFlyEmission = psOreFly.emission;
            oreFlyEmission.rateOverTime = 0;
            oreFlyEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });

            // FX_Smoke (M4)
            GameObject goSmoke = new GameObject("FX_Smoke");
            goSmoke.transform.SetParent(goFx.transform);
            goSmoke.transform.position = new Vector3(0.20f, 1.30f, 0.30f);
            var psSmoke = goSmoke.AddComponent<ParticleSystem>();
            var rendSmoke = goSmoke.GetComponent<ParticleSystemRenderer>();
            if (rendSmoke != null && mats.TryGetValue("FX_Smoke", out var mSmoke)) rendSmoke.sharedMaterial = mSmoke;
            var smokeMain = psSmoke.main;
            smokeMain.startLifetime = 1.4f;
            smokeMain.startSpeed = 0.6f;
            smokeMain.startSize = 0.14f;
            smokeMain.startColor = new Color(0.35f, 0.35f, 0.40f, 0.45f);
            smokeMain.playOnAwake = false;
            var smokeEmission = psSmoke.emission;
            smokeEmission.rateOverTime = 4f;

            // FX_UnlockBurst
            GameObject goUnlock = new GameObject("FX_UnlockBurst");
            goUnlock.transform.SetParent(goFx.transform);
            goUnlock.transform.position = new Vector3(0f, 1.2f, 0f);
            var psUnlock = goUnlock.AddComponent<ParticleSystem>();
            var rendUnlock = goUnlock.GetComponent<ParticleSystemRenderer>();
            if (rendUnlock != null && mats.TryGetValue("FX_Spark", out var mUnlockSpark)) rendUnlock.sharedMaterial = mUnlockSpark;
            var unlockMain = psUnlock.main;
            unlockMain.startLifetime = 0.8f;
            unlockMain.startSpeed = 4.0f;
            unlockMain.startSize = 0.12f;
            unlockMain.startColor = new Color(1.0f, 0.9f, 0.3f);
            unlockMain.playOnAwake = false;
            var unlockEmission = psUnlock.emission;
            unlockEmission.rateOverTime = 0;
            unlockEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

            // WorldView component
            var worldView = goWorld.AddComponent<WorldView>();
            worldView.machineG1 = machineGos[0];
            worldView.machineG2 = machineGos[1];
            worldView.machineG3 = machineGos[2];
            worldView.machineG4 = machineGos[3];
            worldView.machineG5 = machineGos[4];
            worldView.decorLamp = null;
            worldView.decorOreShelf = goOreShelf;
            worldView.decorSmokeStack = goSmokeStack;
            worldView.decorSign = goSign;
            worldView.decorChest = goChest;
            worldView.fxHitSpark = psSpark;
            worldView.fxOreFly = psOreFly;
            worldView.fxSmoke = psSmoke;
            worldView.fxUnlockBurst = psUnlock;

            gameManager.worldView = worldView;

            // 2.5 CameraRig
            GameObject goCameraRig = new GameObject("CameraRig");
            goCameraRig.transform.SetParent(goWorld.transform);

            GameObject goCam = new GameObject("Main Camera");
            goCam.tag = "MainCamera";
            goCam.transform.SetParent(goCameraRig.transform);
            goCam.transform.position = new Vector3(0.00f, 2.15f, -2.55f);
            goCam.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            var cam = goCam.AddComponent<Camera>();
            cam.fieldOfView = 38f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            goCam.AddComponent<AudioListener>();
            var camPunch = goCam.AddComponent<CameraPunch>();
            var camParallax = goCam.AddComponent<Forge.CameraParallax3D>();
            var camData = goCam.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            // ImpulseSource
            GameObject goImpulse = new GameObject("ImpulseSource");
            goImpulse.transform.SetParent(goCameraRig.transform);

            // 2.6 Post Processing Volume (URP) - Calm Bloom & Vignette!
            GameObject goVolume = new GameObject("GlobalVolume");
            goVolume.transform.SetParent(goWorld.transform);
            var vol = goVolume.AddComponent<Volume>();
            vol.isGlobal = true;

            string volProfilePath = $"{URP_SETTINGS_DIR}/Forge_VolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(volProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "Forge_VolumeProfile";
                AssetDatabase.CreateAsset(profile, volProfilePath);

                var bloom = profile.Add<Bloom>();
                bloom.threshold.Override(1.0f);
                bloom.intensity.Override(0.35f);

                var colorAdj = profile.Add<ColorAdjustments>();
                colorAdj.contrast.Override(12f);
                colorAdj.saturation.Override(10f);

                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.26f);
                vignette.smoothness.Override(0.45f);

                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }
            vol.profile = profile;

            // Wire ClickForge (Furnace and Anvil)
            clickForge.cameraPunch = camPunch;
            clickForge.lightController = lightCtrl;
            clickForge.audioService = audioService;
            clickForge.worldView = worldView;

            clickAnvil.cameraPunch = camPunch;
            clickAnvil.lightController = lightCtrl;
            clickAnvil.audioService = audioService;
            clickAnvil.worldView = worldView;

            gameManager.lightController = lightCtrl;

            // 3. [UI] / Canvas
            SetupUI(gameManager);

            // Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddSceneToBuildSettings(SCENE_PATH);
            Debug.Log($"[ForgeSetup] Scene created and saved to {SCENE_PATH}");
        }

        private static void SetupUI(GameManager gm)
        {
            GameObject goUiRoot = new GameObject("[UI]");

            // EventSystem
            GameObject goEventSystem = new GameObject("EventSystem");
            goEventSystem.transform.SetParent(goUiRoot.transform);
            goEventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            goEventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Canvas
            GameObject goCanvas = new GameObject("Canvas");
            goCanvas.layer = LayerMask.NameToLayer("UI");
            goCanvas.transform.SetParent(goUiRoot.transform);
            var canvas = goCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = goCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            goCanvas.AddComponent<GraphicRaycaster>();

            Font defaultFont = GetDefaultFont();
            Sprite spOreIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Forge/Art/Textures/Icons/Icon_Ore_Nugget.png");
            Sprite spTutorialHand = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Forge/Art/Textures/Icons/Icon_Tutorial_Hand.png");

            // FloatingTextSpawner on Canvas
            var textSpawner = goCanvas.AddComponent<FloatingTextSpawner>();
            textSpawner.worldCamera = Camera.main != null ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();
            textSpawner.oreIconSprite = spOreIcon;
            textSpawner.font = defaultFont;

            // --- HUD Root with RectTransform stretched to screen ---
            GameObject goHud = new GameObject("HUD", typeof(RectTransform));
            goHud.transform.SetParent(goCanvas.transform, false);
            var rtHud = goHud.GetComponent<RectTransform>();
            rtHud.anchorMin = Vector2.zero;
            rtHud.anchorMax = Vector2.one;
            rtHud.offsetMin = Vector2.zero;
            rtHud.offsetMax = Vector2.zero;
            var hudView = goHud.AddComponent<HudView>();

            // 1. Currency Card (Top-Left: 35, -35, size 340 x 85)
            GameObject goCardCurrency = new GameObject("Card_Currency", typeof(RectTransform), typeof(Image));
            goCardCurrency.transform.SetParent(goHud.transform, false);
            var rtCardCurr = goCardCurrency.GetComponent<RectTransform>();
            rtCardCurr.anchorMin = new Vector2(0f, 1f);
            rtCardCurr.anchorMax = new Vector2(0f, 1f);
            rtCardCurr.pivot = new Vector2(0f, 1f);
            rtCardCurr.anchoredPosition = new Vector2(35f, -35f);
            rtCardCurr.sizeDelta = new Vector2(340f, 85f);
            var imgCardCurr = goCardCurrency.GetComponent<Image>();
            imgCardCurr.color = new Color(0.10f, 0.12f, 0.16f, 0.90f);

            // Card border line
            GameObject goCardLine = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            goCardLine.transform.SetParent(goCardCurrency.transform, false);
            var rtLine = goCardLine.GetComponent<RectTransform>();
            rtLine.anchorMin = new Vector2(0f, 0f);
            rtLine.anchorMax = new Vector2(1f, 0f);
            rtLine.sizeDelta = new Vector2(0f, 3f);
            rtLine.anchoredPosition = new Vector2(0f, 1.5f);
            goCardLine.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.9f);

            // Ore Icon in Currency Card
            GameObject goOreIcon = new GameObject("ImgOreIcon", typeof(RectTransform), typeof(Image));
            goOreIcon.transform.SetParent(goCardCurrency.transform, false);
            var rtOreIcon = goOreIcon.GetComponent<RectTransform>();
            rtOreIcon.anchorMin = new Vector2(0f, 0.5f);
            rtOreIcon.anchorMax = new Vector2(0f, 0.5f);
            rtOreIcon.pivot = new Vector2(0f, 0.5f);
            rtOreIcon.anchoredPosition = new Vector2(14f, 0f);
            rtOreIcon.sizeDelta = new Vector2(62f, 62f);
            var imgOreIcon = goOreIcon.GetComponent<Image>();
            if (spOreIcon != null) imgOreIcon.sprite = spOreIcon;
            imgOreIcon.preserveAspect = true;

            // TxtOre in Currency Card
            GameObject goTxtOre = new GameObject("TxtOre", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxtOre.transform.SetParent(goCardCurrency.transform, false);
            var txtOre = goTxtOre.GetComponent<Text>();
            txtOre.font = defaultFont;
            txtOre.fontSize = 26;
            txtOre.fontStyle = FontStyle.Bold;
            txtOre.alignment = TextAnchor.MiddleLeft;
            txtOre.color = new Color(1.0f, 0.88f, 0.35f);
            txtOre.text = "Quặng: 0";
            var rtOre = goTxtOre.GetComponent<RectTransform>();
            rtOre.anchorMin = new Vector2(0f, 0.5f);
            rtOre.anchorMax = new Vector2(0f, 0.5f);
            rtOre.pivot = new Vector2(0f, 0.5f);
            rtOre.anchoredPosition = new Vector2(88f, 18f);
            rtOre.sizeDelta = new Vector2(235f, 32f);
            var outOre = goTxtOre.GetComponent<Outline>();
            outOre.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            outOre.effectDistance = new Vector2(1.5f, -1.5f);

            // TxtDps in Currency Card
            GameObject goTxtDps = new GameObject("TxtDps", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxtDps.transform.SetParent(goCardCurrency.transform, false);
            var txtDps = goTxtDps.GetComponent<Text>();
            txtDps.font = defaultFont;
            txtDps.fontSize = 19;
            txtDps.fontStyle = FontStyle.Bold;
            txtDps.alignment = TextAnchor.MiddleLeft;
            txtDps.color = new Color(0.45f, 0.92f, 0.72f);
            txtDps.text = "+0/giây";
            var rtDps = goTxtDps.GetComponent<RectTransform>();
            rtDps.anchorMin = new Vector2(0f, 0.5f);
            rtDps.anchorMax = new Vector2(0f, 0.5f);
            rtDps.pivot = new Vector2(0f, 0.5f);
            rtDps.anchoredPosition = new Vector2(88f, -18f);
            rtDps.sizeDelta = new Vector2(235f, 26f);
            var outDps = goTxtDps.GetComponent<Outline>();
            outDps.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            outDps.effectDistance = new Vector2(1.5f, -1.5f);

            // 2. Rank Card (Top-Right: -105, -35, size 280 x 55)
            GameObject goCardRank = new GameObject("Card_Rank", typeof(RectTransform), typeof(Image));
            goCardRank.transform.SetParent(goHud.transform, false);
            var rtCardRank = goCardRank.GetComponent<RectTransform>();
            rtCardRank.anchorMin = new Vector2(1f, 1f);
            rtCardRank.anchorMax = new Vector2(1f, 1f);
            rtCardRank.pivot = new Vector2(1f, 1f);
            rtCardRank.anchoredPosition = new Vector2(-105f, -35f);
            rtCardRank.sizeDelta = new Vector2(280f, 55f);
            var imgCardRank = goCardRank.GetComponent<Image>();
            imgCardRank.color = new Color(0.10f, 0.12f, 0.16f, 0.90f);

            // Rank border line
            GameObject goRankLine = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            goRankLine.transform.SetParent(goCardRank.transform, false);
            var rtRLine = goRankLine.GetComponent<RectTransform>();
            rtRLine.anchorMin = new Vector2(0f, 0f);
            rtRLine.anchorMax = new Vector2(1f, 0f);
            rtRLine.sizeDelta = new Vector2(0f, 3f);
            rtRLine.anchoredPosition = new Vector2(0f, 1.5f);
            goRankLine.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.9f);

            // TxtRank in Rank Card
            GameObject goTxtRank = new GameObject("TxtRank", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxtRank.transform.SetParent(goCardRank.transform, false);
            var txtRank = goTxtRank.GetComponent<Text>();
            txtRank.font = defaultFont;
            txtRank.fontSize = 22;
            txtRank.fontStyle = FontStyle.Bold;
            txtRank.alignment = TextAnchor.MiddleCenter;
            txtRank.color = new Color(1.0f, 0.78f, 0.40f);
            txtRank.text = "Danh hiệu: Học việc";
            var rtRank = goTxtRank.GetComponent<RectTransform>();
            rtRank.anchorMin = Vector2.zero;
            rtRank.anchorMax = Vector2.one;
            rtRank.sizeDelta = Vector2.zero;
            var outRank = goTxtRank.GetComponent<Outline>();
            outRank.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            outRank.effectDistance = new Vector2(2f, -2f);

            // 2.5 Settings Button (Top-Right: -35, -35, size 55 x 55)
            GameObject goBtnSettings = new GameObject("Btn_Settings", typeof(RectTransform), typeof(Image), typeof(Button));
            goBtnSettings.transform.SetParent(goHud.transform, false);
            var rtBtnSettings = goBtnSettings.GetComponent<RectTransform>();
            rtBtnSettings.anchorMin = new Vector2(1f, 1f);
            rtBtnSettings.anchorMax = new Vector2(1f, 1f);
            rtBtnSettings.pivot = new Vector2(1f, 1f);
            rtBtnSettings.anchoredPosition = new Vector2(-35f, -35f);
            rtBtnSettings.sizeDelta = new Vector2(55f, 55f);
            var imgBtnSettings = goBtnSettings.GetComponent<Image>();
            imgBtnSettings.color = new Color(0.18f, 0.20f, 0.26f, 0.95f);
            var btnSettings = goBtnSettings.GetComponent<Button>();

            // Accent border on Settings Button
            GameObject goSettingsBorder = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            goSettingsBorder.transform.SetParent(goBtnSettings.transform, false);
            var rtSetBorder = goSettingsBorder.GetComponent<RectTransform>();
            rtSetBorder.anchorMin = new Vector2(0f, 0f);
            rtSetBorder.anchorMax = new Vector2(1f, 0f);
            rtSetBorder.sizeDelta = new Vector2(0f, 3f);
            rtSetBorder.anchoredPosition = new Vector2(0f, 1.5f);
            goSettingsBorder.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.9f);

            GameObject goSettingsTxt = new GameObject("Icon", typeof(RectTransform), typeof(Text));
            goSettingsTxt.transform.SetParent(goBtnSettings.transform, false);
            var txtSettings = goSettingsTxt.GetComponent<Text>();
            txtSettings.font = defaultFont;
            txtSettings.fontSize = 28;
            txtSettings.alignment = TextAnchor.MiddleCenter;
            txtSettings.color = new Color(1.0f, 0.90f, 0.50f);
            txtSettings.text = "⚙";
            var rtSetTxt = goSettingsTxt.GetComponent<RectTransform>();
            rtSetTxt.anchorMin = Vector2.zero;
            rtSetTxt.anchorMax = Vector2.one;
            rtSetTxt.sizeDelta = Vector2.zero;

            hudView.btnSettings = btnSettings;
            hudView.txtOre = txtOre;
            hudView.txtDps = txtDps;
            hudView.txtRank = txtRank;
            hudView.imgOreIcon = imgOreIcon;
            hudView.panelCurrencyCard = imgCardCurr;
            hudView.panelRankCard = imgCardRank;
            gm.hudView = hudView;

            // 3. Tutorial Hint Banner & Bouncing Hand (Non-blocking so clicks hit Furnace & Anvil directly!)
            GameObject goTutorial = new GameObject("TutorialHint", typeof(RectTransform), typeof(CanvasGroup));
            goTutorial.transform.SetParent(goCanvas.transform, false);
            var cgTut = goTutorial.GetComponent<CanvasGroup>();
            cgTut.blocksRaycasts = false;
            var rtTut = goTutorial.GetComponent<RectTransform>();
            rtTut.anchorMin = new Vector2(0.5f, 0.5f);
            rtTut.anchorMax = new Vector2(0.5f, 0.5f);
            rtTut.pivot = new Vector2(0.5f, 0.5f);
            rtTut.anchoredPosition = new Vector2(480f, 210f);
            rtTut.sizeDelta = new Vector2(460f, 110f);

            // Hand Icon
            GameObject goHand = new GameObject("HandIcon", typeof(RectTransform), typeof(Image));
            goHand.transform.SetParent(goTutorial.transform, false);
            var rtHand = goHand.GetComponent<RectTransform>();
            rtHand.anchorMin = new Vector2(0.5f, 1f);
            rtHand.anchorMax = new Vector2(0.5f, 1f);
            rtHand.pivot = new Vector2(0.5f, 0.5f);
            rtHand.anchoredPosition = new Vector2(0f, 15f);
            rtHand.sizeDelta = new Vector2(68f, 68f);
            var imgHand = goHand.GetComponent<Image>();
            if (spTutorialHand != null) imgHand.sprite = spTutorialHand;
            imgHand.preserveAspect = true;
            imgHand.raycastTarget = false;

            // Banner background
            GameObject goBanner = new GameObject("Banner", typeof(RectTransform), typeof(Image));
            goBanner.transform.SetParent(goTutorial.transform, false);
            var rtBanner = goBanner.GetComponent<RectTransform>();
            rtBanner.anchorMin = new Vector2(0.5f, 0f);
            rtBanner.anchorMax = new Vector2(0.5f, 0f);
            rtBanner.pivot = new Vector2(0.5f, 0.5f);
            rtBanner.anchoredPosition = new Vector2(0f, 0f);
            rtBanner.sizeDelta = new Vector2(460f, 50f);
            var imgBanner = goBanner.GetComponent<Image>();
            imgBanner.color = new Color(0.10f, 0.12f, 0.18f, 0.92f);
            imgBanner.raycastTarget = false;

            GameObject goTutTxt = new GameObject("TxtInstruction", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTutTxt.transform.SetParent(goBanner.transform, false);
            var txtTut = goTutTxt.GetComponent<Text>();
            txtTut.font = defaultFont;
            txtTut.fontSize = 22;
            txtTut.fontStyle = FontStyle.Bold;
            txtTut.alignment = TextAnchor.MiddleCenter;
            txtTut.color = new Color(1.0f, 0.92f, 0.55f);
            txtTut.text = "👆 Chạm vào Lò hoặc Đe để rèn quặng!";
            txtTut.raycastTarget = false;
            var rtTutTxt = goTutTxt.GetComponent<RectTransform>();
            rtTutTxt.anchorMin = Vector2.zero;
            rtTutTxt.anchorMax = Vector2.one;
            rtTutTxt.sizeDelta = Vector2.zero;
            var outTut = goTutTxt.GetComponent<Outline>();
            outTut.effectColor = new Color(0.1f, 0.05f, 0f, 0.95f);
            outTut.effectDistance = new Vector2(2f, -2f);

            var tutCtrl = goTutorial.AddComponent<TutorialHintController>();
            tutCtrl.hintContainer = rtTut;
            tutCtrl.canvasGroup = goTutorial.GetComponent<CanvasGroup>();
            tutCtrl.handIconRect = rtHand;
            tutCtrl.txtInstruction = txtTut;

            // 4. BtnShop: bottom-center (300 x 68)
            GameObject goBtnShop = new GameObject("BtnShop", typeof(RectTransform), typeof(Image), typeof(Button));
            goBtnShop.transform.SetParent(goCanvas.transform, false);
            var imgBtnShop = goBtnShop.GetComponent<Image>();
            imgBtnShop.color = new Color(0.85f, 0.48f, 0.14f);
            var btnShop = goBtnShop.GetComponent<Button>();
            var rtBtnShop = goBtnShop.GetComponent<RectTransform>();
            rtBtnShop.anchorMin = new Vector2(0.5f, 0f);
            rtBtnShop.anchorMax = new Vector2(0.5f, 0f);
            rtBtnShop.pivot = new Vector2(0.5f, 0f);
            rtBtnShop.anchoredPosition = new Vector2(0f, 25f);
            rtBtnShop.sizeDelta = new Vector2(300f, 68f);

            // Shop Button Accent Line
            GameObject goBtnLine = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            goBtnLine.transform.SetParent(goBtnShop.transform, false);
            var rtBLine = goBtnLine.GetComponent<RectTransform>();
            rtBLine.anchorMin = new Vector2(0f, 1f);
            rtBLine.anchorMax = new Vector2(1f, 1f);
            rtBLine.sizeDelta = new Vector2(0f, 3f);
            rtBLine.anchoredPosition = new Vector2(0f, -1.5f);
            goBtnLine.GetComponent<Image>().color = new Color(1.0f, 0.85f, 0.4f, 0.95f);

            GameObject goBtnShopText = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            goBtnShopText.transform.SetParent(goBtnShop.transform, false);
            var txtBtnShop = goBtnShopText.GetComponent<Text>();
            txtBtnShop.font = defaultFont;
            txtBtnShop.fontSize = 24;
            txtBtnShop.fontStyle = FontStyle.Bold;
            txtBtnShop.alignment = TextAnchor.MiddleCenter;
            txtBtnShop.color = Color.white;
            txtBtnShop.text = "CÔNG XƯỞNG & NÂNG CẤP";
            var rtBtnShopText = goBtnShopText.GetComponent<RectTransform>();
            rtBtnShopText.anchorMin = Vector2.zero;
            rtBtnShopText.anchorMax = Vector2.one;
            rtBtnShopText.sizeDelta = Vector2.zero;
            var outBtn = goBtnShopText.GetComponent<Outline>();
            outBtn.effectColor = new Color(0.2f, 0.08f, 0.02f, 0.9f);
            outBtn.effectDistance = new Vector2(2f, -2f);

            // Notification Badge on Shop Button (CÓ NÂNG CẤP!)
            GameObject goBadge = new GameObject("ShopNotificationBadge", typeof(RectTransform), typeof(Image));
            goBadge.transform.SetParent(goBtnShop.transform, false);
            var rtBadge = goBadge.GetComponent<RectTransform>();
            rtBadge.anchorMin = new Vector2(1f, 1f);
            rtBadge.anchorMax = new Vector2(1f, 1f);
            rtBadge.pivot = new Vector2(0.5f, 0.5f);
            rtBadge.anchoredPosition = new Vector2(-15f, 5f);
            rtBadge.sizeDelta = new Vector2(150f, 32f);
            var imgBadge = goBadge.GetComponent<Image>();
            imgBadge.color = new Color(0.92f, 0.22f, 0.22f, 0.95f);

            GameObject goBadgeTxt = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            goBadgeTxt.transform.SetParent(goBadge.transform, false);
            var txtBadge = goBadgeTxt.GetComponent<Text>();
            txtBadge.font = defaultFont;
            txtBadge.fontSize = 14;
            txtBadge.fontStyle = FontStyle.Bold;
            txtBadge.alignment = TextAnchor.MiddleCenter;
            txtBadge.color = Color.white;
            txtBadge.text = "CÓ NÂNG CẤP!";
            var rtBadgeTxt = goBadgeTxt.GetComponent<RectTransform>();
            rtBadgeTxt.anchorMin = Vector2.zero;
            rtBadgeTxt.anchorMax = Vector2.one;
            rtBadgeTxt.sizeDelta = Vector2.zero;
            var outBadge = goBadgeTxt.GetComponent<Outline>();
            outBadge.effectColor = new Color(0.2f, 0f, 0f, 0.9f);
            outBadge.effectDistance = new Vector2(1f, -1f);

            hudView.goShopBadge = goBadge;
            hudView.txtShopBadge = txtBadge;
            goBadge.SetActive(false);

            // --- PanelShop: height 490 ---
            GameObject goPanelShop = new GameObject("PanelShop");
            goPanelShop.transform.SetParent(goCanvas.transform, false);
            var imgPanelShop = goPanelShop.AddComponent<Image>();
            imgPanelShop.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);
            var rtPanelShop = goPanelShop.GetComponent<RectTransform>();
            rtPanelShop.anchorMin = new Vector2(0.5f, 0f);
            rtPanelShop.anchorMax = new Vector2(0.5f, 0f);
            rtPanelShop.pivot = new Vector2(0.5f, 0f);
            rtPanelShop.anchoredPosition = new Vector2(0f, 110f);
            rtPanelShop.sizeDelta = new Vector2(1150f, 490f);

            // BtnCloseShop
            GameObject goBtnClose = new GameObject("BtnCloseShop");
            goBtnClose.transform.SetParent(goPanelShop.transform, false);
            var imgClose = goBtnClose.AddComponent<Image>();
            imgClose.color = new Color(0.75f, 0.22f, 0.22f);
            var btnClose = goBtnClose.AddComponent<Button>();
            var rtClose = goBtnClose.GetComponent<RectTransform>();
            rtClose.anchorMin = new Vector2(1f, 1f);
            rtClose.anchorMax = new Vector2(1f, 1f);
            rtClose.pivot = new Vector2(1f, 1f);
            rtClose.anchoredPosition = new Vector2(-12f, -12f);
            rtClose.sizeDelta = new Vector2(44f, 44f);

            GameObject goCloseTxt = new GameObject("X");
            goCloseTxt.transform.SetParent(goBtnClose.transform, false);
            var txtClose = goCloseTxt.AddComponent<Text>();
            txtClose.font = defaultFont;
            txtClose.fontSize = 24;
            txtClose.fontStyle = FontStyle.Bold;
            txtClose.alignment = TextAnchor.MiddleCenter;
            txtClose.color = Color.white;
            txtClose.text = "X";
            var rtCloseTxt = goCloseTxt.GetComponent<RectTransform>();
            rtCloseTxt.anchorMin = Vector2.zero;
            rtCloseTxt.anchorMax = Vector2.one;
            rtCloseTxt.sizeDelta = Vector2.zero;

            // Title Shop
            GameObject goShopTitle = new GameObject("Title");
            goShopTitle.transform.SetParent(goPanelShop.transform, false);
            var txtShopTitle = goShopTitle.AddComponent<Text>();
            txtShopTitle.font = defaultFont;
            txtShopTitle.fontSize = 32;
            txtShopTitle.fontStyle = FontStyle.Bold;
            txtShopTitle.color = new Color(1.0f, 0.85f, 0.45f);
            txtShopTitle.text = "CÔNG XƯỞNG TỰ ĐỘNG HÓA";
            var rtShopTitle = goShopTitle.GetComponent<RectTransform>();
            rtShopTitle.anchorMin = new Vector2(0f, 1f);
            rtShopTitle.anchorMax = new Vector2(0f, 1f);
            rtShopTitle.pivot = new Vector2(0f, 1f);
            rtShopTitle.anchoredPosition = new Vector2(25f, -14f);
            rtShopTitle.sizeDelta = new Vector2(350f, 42f);

            // Tab 1: Machines
            GameObject goTabM = new GameObject("BtnTabMachines", typeof(RectTransform), typeof(Image), typeof(Button));
            goTabM.transform.SetParent(goPanelShop.transform, false);
            var imgTabM = goTabM.GetComponent<Image>();
            imgTabM.color = new Color(0.72f, 0.40f, 0.12f, 1f);
            var btnTabM = goTabM.GetComponent<Button>();
            var rtTabM = goTabM.GetComponent<RectTransform>();
            rtTabM.anchorMin = new Vector2(0f, 1f);
            rtTabM.anchorMax = new Vector2(0f, 1f);
            rtTabM.pivot = new Vector2(0f, 1f);
            rtTabM.anchoredPosition = new Vector2(380f, -10f);
            rtTabM.sizeDelta = new Vector2(240f, 40f);

            GameObject goTxtTabM = new GameObject("Txt", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxtTabM.transform.SetParent(goTabM.transform, false);
            var tTabM = goTxtTabM.GetComponent<Text>();
            tTabM.font = defaultFont;
            tTabM.fontSize = 20;
            tTabM.fontStyle = FontStyle.Bold;
            tTabM.alignment = TextAnchor.MiddleCenter;
            tTabM.color = new Color(1.0f, 0.95f, 0.80f);
            tTabM.text = "⚙️ DÂY CHUYỀN MÁY";
            var rtTM = goTxtTabM.GetComponent<RectTransform>();
            rtTM.anchorMin = Vector2.zero; rtTM.anchorMax = Vector2.one; rtTM.sizeDelta = Vector2.zero;
            var outTM = goTxtTabM.GetComponent<Outline>();
            outTM.effectColor = new Color(0.1f, 0.05f, 0.02f, 0.8f);
            outTM.effectDistance = new Vector2(1f, -1f);

            // Tab 2: Upgrades
            GameObject goTabU = new GameObject("BtnTabUpgrades", typeof(RectTransform), typeof(Image), typeof(Button));
            goTabU.transform.SetParent(goPanelShop.transform, false);
            var imgTabU = goTabU.GetComponent<Image>();
            imgTabU.color = new Color(0.18f, 0.22f, 0.30f, 0.9f);
            var btnTabU = goTabU.GetComponent<Button>();
            var rtTabU = goTabU.GetComponent<RectTransform>();
            rtTabU.anchorMin = new Vector2(0f, 1f);
            rtTabU.anchorMax = new Vector2(0f, 1f);
            rtTabU.pivot = new Vector2(0f, 1f);
            rtTabU.anchoredPosition = new Vector2(630f, -10f);
            rtTabU.sizeDelta = new Vector2(240f, 40f);

            GameObject goTxtTabU = new GameObject("Txt", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxtTabU.transform.SetParent(goTabU.transform, false);
            var tTabU = goTxtTabU.GetComponent<Text>();
            tTabU.font = defaultFont;
            tTabU.fontSize = 20;
            tTabU.fontStyle = FontStyle.Bold;
            tTabU.alignment = TextAnchor.MiddleCenter;
            tTabU.color = new Color(0.65f, 0.72f, 0.82f);
            tTabU.text = "🔨 BÚA & LÒ NUNG";
            var rtTU = goTxtTabU.GetComponent<RectTransform>();
            rtTU.anchorMin = Vector2.zero; rtTU.anchorMax = Vector2.one; rtTU.sizeDelta = Vector2.zero;
            var outTU = goTxtTabU.GetComponent<Outline>();
            outTU.effectColor = new Color(0.05f, 0.05f, 0.1f, 0.8f);
            outTU.effectDistance = new Vector2(1f, -1f);

            // Shop Rows Container (Machines)
            GameObject goShopList = new GameObject("ContainerMachines");
            goShopList.transform.SetParent(goPanelShop.transform, false);
            var rtShopList = goShopList.AddComponent<RectTransform>();
            rtShopList.anchorMin = new Vector2(0f, 0f);
            rtShopList.anchorMax = new Vector2(1f, 1f);
            rtShopList.offsetMin = new Vector2(20f, 15f);
            rtShopList.offsetMax = new Vector2(-20f, -62f);

            // Upgrades Container
            GameObject goUpgradeList = new GameObject("ContainerUpgrades");
            goUpgradeList.transform.SetParent(goPanelShop.transform, false);
            var rtUpgradeList = goUpgradeList.AddComponent<RectTransform>();
            rtUpgradeList.anchorMin = new Vector2(0f, 0f);
            rtUpgradeList.anchorMax = new Vector2(1f, 1f);
            rtUpgradeList.offsetMin = new Vector2(20f, 15f);
            rtUpgradeList.offsetMax = new Vector2(-20f, -62f);
            goUpgradeList.SetActive(false);

            var shopView = goCanvas.AddComponent<ShopView>();
            shopView.panelShop = goPanelShop;
            shopView.btnOpenShop = btnShop;
            shopView.btnCloseShop = btnClose;
            shopView.txtShopTitle = txtShopTitle;
            shopView.btnTabMachines = btnTabM;
            shopView.btnTabUpgrades = btnTabU;
            shopView.containerMachines = goShopList;
            shopView.containerUpgrades = goUpgradeList;

            // Generate 5 Rows with LARGE, CLEAR FONT and crisp layout
            string[] gNames = new[] { "Tay Búa Tí Hon", "Cũi Đào Quặng", "Lò Than Tự Đun", "Cối Xay Kim", "Robot Thợ Cả" };
            for (int i = 0; i < 5; i++)
            {
                GameObject goRow = new GameObject($"Row_G{i + 1}");
                goRow.transform.SetParent(goShopList.transform, false);
                var imgRow = goRow.AddComponent<Image>();
                imgRow.color = new Color(0.18f, 0.18f, 0.24f, 0.90f);
                var rtRow = goRow.GetComponent<RectTransform>();
                rtRow.anchorMin = new Vector2(0f, 1f);
                rtRow.anchorMax = new Vector2(1f, 1f);
                rtRow.pivot = new Vector2(0.5f, 1f);
                rtRow.anchoredPosition = new Vector2(0f, -(i * 78f));
                rtRow.sizeDelta = new Vector2(0f, 72f);

                var rowView = goRow.AddComponent<ShopRowView>();

                // Icon image preview
                GameObject goIcon = new GameObject("ImgIcon");
                goIcon.transform.SetParent(goRow.transform, false);
                var imgIcon = goIcon.AddComponent<Image>();
                imgIcon.preserveAspect = true;
                var rtIcon = goIcon.GetComponent<RectTransform>();
                rtIcon.anchorMin = new Vector2(0f, 0.5f);
                rtIcon.anchorMax = new Vector2(0f, 0.5f);
                rtIcon.pivot = new Vector2(0f, 0.5f);
                rtIcon.anchoredPosition = new Vector2(14f, 0f);
                rtIcon.sizeDelta = new Vector2(56f, 56f);

                // Name (Large & Bold: 26pt)
                GameObject goName = new GameObject("TxtName", typeof(RectTransform), typeof(Text), typeof(Outline));
                goName.transform.SetParent(goRow.transform, false);
                var tName = goName.GetComponent<Text>();
                tName.font = defaultFont;
                tName.fontSize = 26;
                tName.fontStyle = FontStyle.Bold;
                tName.alignment = TextAnchor.MiddleLeft;
                tName.color = Color.white;
                tName.text = gNames[i];
                var rtN = goName.GetComponent<RectTransform>();
                rtN.anchorMin = new Vector2(0f, 0.5f);
                rtN.anchorMax = new Vector2(0f, 0.5f);
                rtN.pivot = new Vector2(0f, 0.5f);
                rtN.anchoredPosition = new Vector2(80f, 12f);
                rtN.sizeDelta = new Vector2(260f, 32f);
                var outName = goName.GetComponent<Outline>();
                outName.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
                outName.effectDistance = new Vector2(1f, -1f);

                // Level (20pt)
                GameObject goLv = new GameObject("TxtLevel", typeof(RectTransform), typeof(Text));
                goLv.transform.SetParent(goRow.transform, false);
                var tLv = goLv.GetComponent<Text>();
                tLv.font = defaultFont;
                tLv.fontSize = 20;
                tLv.color = new Color(0.70f, 0.75f, 0.85f);
                tLv.text = "Cấp: 0";
                var rtLv = goLv.GetComponent<RectTransform>();
                rtLv.anchorMin = new Vector2(0f, 0.5f);
                rtLv.anchorMax = new Vector2(0f, 0.5f);
                rtLv.pivot = new Vector2(0f, 0.5f);
                rtLv.anchoredPosition = new Vector2(80f, -16f);
                rtLv.sizeDelta = new Vector2(220f, 26f);

                // Output (24pt Bold Emerald)
                GameObject goOut = new GameObject("TxtOutput", typeof(RectTransform), typeof(Text), typeof(Outline));
                goOut.transform.SetParent(goRow.transform, false);
                var tOut = goOut.GetComponent<Text>();
                tOut.font = defaultFont;
                tOut.fontSize = 24;
                tOut.fontStyle = FontStyle.Bold;
                tOut.alignment = TextAnchor.MiddleLeft;
                tOut.color = new Color(0.40f, 0.95f, 0.55f);
                tOut.text = "+0/s";
                var rtOut = goOut.GetComponent<RectTransform>();
                rtOut.anchorMin = new Vector2(0f, 0.5f);
                rtOut.anchorMax = new Vector2(0f, 0.5f);
                rtOut.pivot = new Vector2(0f, 0.5f);
                rtOut.anchoredPosition = new Vector2(350f, 0f);
                rtOut.sizeDelta = new Vector2(160f, 32f);
                var outOut = goOut.GetComponent<Outline>();
                outOut.effectColor = new Color(0.04f, 0.12f, 0.04f, 0.9f);
                outOut.effectDistance = new Vector2(1f, -1f);

                // Cost / Unlock text (Anchored to right, 390px wide with BestFit, safely 15px away from BtnBuy)
                GameObject goCost = new GameObject("TxtCost", typeof(RectTransform), typeof(Text), typeof(Outline));
                goCost.transform.SetParent(goRow.transform, false);
                var tCost = goCost.GetComponent<Text>();
                tCost.font = defaultFont;
                tCost.fontSize = 22;
                tCost.fontStyle = FontStyle.Bold;
                tCost.alignment = TextAnchor.MiddleRight;
                tCost.resizeTextForBestFit = true;
                tCost.resizeTextMinSize = 13;
                tCost.resizeTextMaxSize = 22;
                tCost.horizontalOverflow = HorizontalWrapMode.Wrap;
                tCost.color = new Color(1.0f, 0.85f, 0.35f);
                tCost.text = "Giá: 15";
                var rtCost = goCost.GetComponent<RectTransform>();
                rtCost.anchorMin = new Vector2(1f, 0.5f);
                rtCost.anchorMax = new Vector2(1f, 0.5f);
                rtCost.pivot = new Vector2(1f, 0.5f);
                rtCost.anchoredPosition = new Vector2(-190f, 0f);
                rtCost.sizeDelta = new Vector2(390f, 44f);
                var outCost = goCost.GetComponent<Outline>();
                outCost.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.9f);
                outCost.effectDistance = new Vector2(1f, -1f);

                // Buy Button (160 x 52)
                GameObject goBuy = new GameObject("BtnBuy", typeof(RectTransform), typeof(Image), typeof(Button));
                goBuy.transform.SetParent(goRow.transform, false);
                var imgBuy = goBuy.GetComponent<Image>();
                imgBuy.color = new Color(0.20f, 0.68f, 0.32f);
                var btnBuy = goBuy.GetComponent<Button>();
                var rtBuy = goBuy.GetComponent<RectTransform>();
                rtBuy.anchorMin = new Vector2(1f, 0.5f);
                rtBuy.anchorMax = new Vector2(1f, 0.5f);
                rtBuy.pivot = new Vector2(1f, 0.5f);
                rtBuy.anchoredPosition = new Vector2(-15f, 0f);
                rtBuy.sizeDelta = new Vector2(160f, 52f);

                GameObject goBuyTxt = new GameObject("Txt", typeof(RectTransform), typeof(Text), typeof(Outline));
                goBuyTxt.transform.SetParent(goBuy.transform, false);
                var tBuyTxt = goBuyTxt.GetComponent<Text>();
                tBuyTxt.font = defaultFont;
                tBuyTxt.fontSize = 22;
                tBuyTxt.fontStyle = FontStyle.Bold;
                tBuyTxt.alignment = TextAnchor.MiddleCenter;
                tBuyTxt.color = Color.white;
                tBuyTxt.text = "Nâng Cấp";
                var rtBt = goBuyTxt.GetComponent<RectTransform>();
                rtBt.anchorMin = Vector2.zero;
                rtBt.anchorMax = Vector2.one;
                rtBt.sizeDelta = Vector2.zero;
                var outBuy = goBuyTxt.GetComponent<Outline>();
                outBuy.effectColor = new Color(0.05f, 0.18f, 0.06f, 0.9f);
                outBuy.effectDistance = new Vector2(1.5f, -1.5f);

                rowView.txtName = tName;
                rowView.txtLevel = tLv;
                rowView.txtOutput = tOut;
                rowView.txtCost = tCost;
                rowView.btnBuy = btnBuy;
                rowView.imgIcon = imgIcon;

                shopView.shopRows.Add(rowView);
            }

            // Generate 4 Tool & Furnace Upgrade Rows in ContainerUpgrades
            string[] upIds = new[] { "UP_HAMMER", "UP_FURNACE", "UP_GLOVES", "UP_OIL" };
            string[] upNames = new[] { "Búa Rèn Thần Binh", "Lò Nung Nhiệt Hạch", "Găng Tay Da Rồng", "Dầu Tôi Huyền Bí" };
            string[] upSprites = new[] {
                "Assets/_Forge/Art/Textures/Icons/Icon_G1_Hammer.png",
                "Assets/_Forge/Art/Textures/Hero_Furnace.png",
                "Assets/_Forge/Art/Textures/Hero_Anvil.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_Ore_Nugget.png"
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject goRow = new GameObject($"Row_{upIds[i]}");
                goRow.transform.SetParent(goUpgradeList.transform, false);
                var imgRow = goRow.AddComponent<Image>();
                imgRow.color = new Color(0.20f, 0.18f, 0.26f, 0.90f);
                var rtRow = goRow.GetComponent<RectTransform>();
                rtRow.anchorMin = new Vector2(0f, 1f);
                rtRow.anchorMax = new Vector2(1f, 1f);
                rtRow.pivot = new Vector2(0.5f, 1f);
                rtRow.anchoredPosition = new Vector2(0f, -(i * 88f));
                rtRow.sizeDelta = new Vector2(0f, 80f);

                var rowView = goRow.AddComponent<ShopRowView>();

                // Icon preview
                GameObject goIcon = new GameObject("ImgIcon");
                goIcon.transform.SetParent(goRow.transform, false);
                var imgIcon = goIcon.AddComponent<Image>();
                imgIcon.preserveAspect = true;
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(upSprites[i]);
                if (sp != null) imgIcon.sprite = sp;
                var rtIcon = goIcon.GetComponent<RectTransform>();
                rtIcon.anchorMin = new Vector2(0f, 0.5f);
                rtIcon.anchorMax = new Vector2(0f, 0.5f);
                rtIcon.pivot = new Vector2(0f, 0.5f);
                rtIcon.anchoredPosition = new Vector2(14f, 0f);
                rtIcon.sizeDelta = new Vector2(58f, 58f);

                // Name
                GameObject goName = new GameObject("TxtName", typeof(RectTransform), typeof(Text), typeof(Outline));
                goName.transform.SetParent(goRow.transform, false);
                var tName = goName.GetComponent<Text>();
                tName.font = defaultFont;
                tName.fontSize = 26;
                tName.fontStyle = FontStyle.Bold;
                tName.alignment = TextAnchor.MiddleLeft;
                tName.color = new Color(1.0f, 0.95f, 0.85f);
                tName.text = upNames[i];
                var rtN = goName.GetComponent<RectTransform>();
                rtN.anchorMin = new Vector2(0f, 0.5f);
                rtN.anchorMax = new Vector2(0f, 0.5f);
                rtN.pivot = new Vector2(0f, 0.5f);
                rtN.anchoredPosition = new Vector2(82f, 12f);
                rtN.sizeDelta = new Vector2(280f, 32f);
                var outName = goName.GetComponent<Outline>();
                outName.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
                outName.effectDistance = new Vector2(1f, -1f);

                // Level
                GameObject goLv = new GameObject("TxtLevel", typeof(RectTransform), typeof(Text));
                goLv.transform.SetParent(goRow.transform, false);
                var tLv = goLv.GetComponent<Text>();
                tLv.font = defaultFont;
                tLv.fontSize = 20;
                tLv.color = new Color(0.70f, 0.75f, 0.85f);
                tLv.text = "Cấp: 1";
                var rtLv = goLv.GetComponent<RectTransform>();
                rtLv.anchorMin = new Vector2(0f, 0.5f);
                rtLv.anchorMax = new Vector2(0f, 0.5f);
                rtLv.pivot = new Vector2(0f, 0.5f);
                rtLv.anchoredPosition = new Vector2(82f, -16f);
                rtLv.sizeDelta = new Vector2(220f, 26f);

                // Output/Bonus stat
                GameObject goOut = new GameObject("TxtOutput", typeof(RectTransform), typeof(Text), typeof(Outline));
                goOut.transform.SetParent(goRow.transform, false);
                var tOut = goOut.GetComponent<Text>();
                tOut.font = defaultFont;
                tOut.fontSize = 22;
                tOut.fontStyle = FontStyle.Bold;
                tOut.alignment = TextAnchor.MiddleLeft;
                tOut.color = new Color(1.0f, 0.75f, 0.30f);
                tOut.text = "+0";
                var rtOut = goOut.GetComponent<RectTransform>();
                rtOut.anchorMin = new Vector2(0f, 0.5f);
                rtOut.anchorMax = new Vector2(0f, 0.5f);
                rtOut.pivot = new Vector2(0f, 0.5f);
                rtOut.anchoredPosition = new Vector2(370f, 0f);
                rtOut.sizeDelta = new Vector2(240f, 32f);
                var outOut = goOut.GetComponent<Outline>();
                outOut.effectColor = new Color(0.15f, 0.08f, 0.02f, 0.9f);
                outOut.effectDistance = new Vector2(1f, -1f);

                // Cost
                GameObject goCost = new GameObject("TxtCost", typeof(RectTransform), typeof(Text), typeof(Outline));
                goCost.transform.SetParent(goRow.transform, false);
                var tCost = goCost.GetComponent<Text>();
                tCost.font = defaultFont;
                tCost.fontSize = 22;
                tCost.fontStyle = FontStyle.Bold;
                tCost.alignment = TextAnchor.MiddleRight;
                tCost.resizeTextForBestFit = true;
                tCost.resizeTextMinSize = 13;
                tCost.resizeTextMaxSize = 22;
                tCost.color = new Color(1.0f, 0.85f, 0.35f);
                tCost.text = "Giá: 50";
                var rtCost = goCost.GetComponent<RectTransform>();
                rtCost.anchorMin = new Vector2(1f, 0.5f);
                rtCost.anchorMax = new Vector2(1f, 0.5f);
                rtCost.pivot = new Vector2(1f, 0.5f);
                rtCost.anchoredPosition = new Vector2(-190f, 0f);
                rtCost.sizeDelta = new Vector2(300f, 44f);
                var outCost = goCost.GetComponent<Outline>();
                outCost.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.9f);
                outCost.effectDistance = new Vector2(1f, -1f);

                // Buy button
                GameObject goBuy = new GameObject("BtnBuy", typeof(RectTransform), typeof(Image), typeof(Button));
                goBuy.transform.SetParent(goRow.transform, false);
                var imgBuy = goBuy.GetComponent<Image>();
                imgBuy.color = new Color(0.20f, 0.68f, 0.32f);
                var btnBuy = goBuy.GetComponent<Button>();
                var rtBuy = goBuy.GetComponent<RectTransform>();
                rtBuy.anchorMin = new Vector2(1f, 0.5f);
                rtBuy.anchorMax = new Vector2(1f, 0.5f);
                rtBuy.pivot = new Vector2(1f, 0.5f);
                rtBuy.anchoredPosition = new Vector2(-15f, 0f);
                rtBuy.sizeDelta = new Vector2(160f, 52f);

                GameObject goBuyTxt = new GameObject("Txt", typeof(RectTransform), typeof(Text), typeof(Outline));
                goBuyTxt.transform.SetParent(goBuy.transform, false);
                var tBuyTxt = goBuyTxt.GetComponent<Text>();
                tBuyTxt.font = defaultFont;
                tBuyTxt.fontSize = 22;
                tBuyTxt.fontStyle = FontStyle.Bold;
                tBuyTxt.alignment = TextAnchor.MiddleCenter;
                tBuyTxt.color = Color.white;
                tBuyTxt.text = "Nâng Cấp";
                var rtBt = goBuyTxt.GetComponent<RectTransform>();
                rtBt.anchorMin = Vector2.zero;
                rtBt.anchorMax = Vector2.one;
                rtBt.sizeDelta = Vector2.zero;
                var outBuy = goBuyTxt.GetComponent<Outline>();
                outBuy.effectColor = new Color(0.05f, 0.18f, 0.06f, 0.9f);
                outBuy.effectDistance = new Vector2(1.5f, -1.5f);

                rowView.txtName = tName;
                rowView.txtLevel = tLv;
                rowView.txtOutput = tOut;
                rowView.txtCost = tCost;
                rowView.btnBuy = btnBuy;
                rowView.imgIcon = imgIcon;

                shopView.upgradeRows.Add(rowView);
            }
            gm.shopView = shopView;

            // --- PanelOfflinePopup: 520x280 ---
            GameObject goPopup = new GameObject("PanelOfflinePopup");
            goPopup.transform.SetParent(goCanvas.transform, false);
            var imgPop = goPopup.AddComponent<Image>();
            imgPop.color = new Color(0.10f, 0.10f, 0.15f, 0.98f);
            var rtPop = goPopup.GetComponent<RectTransform>();
            rtPop.anchorMin = new Vector2(0.5f, 0.5f);
            rtPop.anchorMax = new Vector2(0.5f, 0.5f);
            rtPop.pivot = new Vector2(0.5f, 0.5f);
            rtPop.anchoredPosition = Vector2.zero;
            rtPop.sizeDelta = new Vector2(520f, 280f);

            // Title
            GameObject goPopTitle = new GameObject("Title");
            goPopTitle.transform.SetParent(goPopup.transform, false);
            var tPopTitle = goPopTitle.AddComponent<Text>();
            tPopTitle.font = defaultFont;
            tPopTitle.fontSize = 28;
            tPopTitle.fontStyle = FontStyle.Bold;
            tPopTitle.alignment = TextAnchor.MiddleCenter;
            tPopTitle.color = new Color(1.0f, 0.85f, 0.45f);
            tPopTitle.text = "SẢN LƯỢNG TREO MÁY";
            var rtPT = goPopTitle.GetComponent<RectTransform>();
            rtPT.anchorMin = new Vector2(0.5f, 1f);
            rtPT.anchorMax = new Vector2(0.5f, 1f);
            rtPT.pivot = new Vector2(0.5f, 1f);
            rtPT.anchoredPosition = new Vector2(0f, -20f);
            rtPT.sizeDelta = new Vector2(480f, 40f);

            // Message / Gain
            GameObject goGainTxt = new GameObject("TxtOfflineGain");
            goGainTxt.transform.SetParent(goPopup.transform, false);
            var tGain = goGainTxt.AddComponent<Text>();
            tGain.font = defaultFont;
            tGain.fontSize = 22;
            tGain.alignment = TextAnchor.MiddleCenter;
            tGain.color = Color.white;
            tGain.text = "Lò chạy hộ bạn 2 giờ 15 phút.\n+45.2K quặng!";
            var rtGain = goGainTxt.GetComponent<RectTransform>();
            rtGain.anchorMin = new Vector2(0.5f, 0.5f);
            rtGain.anchorMax = new Vector2(0.5f, 0.5f);
            rtGain.pivot = new Vector2(0.5f, 0.5f);
            rtGain.anchoredPosition = new Vector2(0f, 10f);
            rtGain.sizeDelta = new Vector2(460f, 90f);

            // Claim Button
            GameObject goClaim = new GameObject("BtnClaimOffline");
            goClaim.transform.SetParent(goPopup.transform, false);
            var imgClaim = goClaim.AddComponent<Image>();
            imgClaim.color = new Color(0.85f, 0.45f, 0.15f);
            var btnClaim = goClaim.AddComponent<Button>();
            var rtClaim = goClaim.GetComponent<RectTransform>();
            rtClaim.anchorMin = new Vector2(0.5f, 0f);
            rtClaim.anchorMax = new Vector2(0.5f, 0f);
            rtClaim.pivot = new Vector2(0.5f, 0f);
            rtClaim.anchoredPosition = new Vector2(0f, 25f);
            rtClaim.sizeDelta = new Vector2(240f, 58f);

            GameObject goClaimTxt = new GameObject("Txt");
            goClaimTxt.transform.SetParent(goClaim.transform, false);
            var tClaimTxt = goClaimTxt.AddComponent<Text>();
            tClaimTxt.font = defaultFont;
            tClaimTxt.fontSize = 24;
            tClaimTxt.fontStyle = FontStyle.Bold;
            tClaimTxt.alignment = TextAnchor.MiddleCenter;
            tClaimTxt.color = Color.white;
            tClaimTxt.text = "Thu Hoạch";
            var rtCt = goClaimTxt.GetComponent<RectTransform>();
            rtCt.anchorMin = Vector2.zero;
            rtCt.anchorMax = Vector2.one;
            rtCt.sizeDelta = Vector2.zero;

            var popupView = goCanvas.AddComponent<OfflinePopupView>();
            popupView.panelPopup = goPopup;
            popupView.txtTitle = tPopTitle;
            popupView.txtOfflineGain = tGain;
            popupView.btnClaim = btnClaim;

            gm.offlinePopupView = popupView;
            goPopup.SetActive(false);

            // 5. Settings Panel
            var settingsView = CreateSettingsPanel(goCanvas, defaultFont, hudView.btnSettings);
            gm.settingsView = settingsView;

            // 6. Main Menu Panel (Màn hình mở đầu game)
            var mainMenuView = CreateMainMenuPanel(goCanvas, defaultFont, gm);
            gm.mainMenuView = mainMenuView;

            // 7. Orientation Manager
            var orientMgr = goCanvas.AddComponent<OrientationManager>();
            orientMgr.mainCamera = Camera.main != null ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (orientMgr.mainCamera != null) orientMgr.cameraTransform = orientMgr.mainCamera.transform;
            orientMgr.canvasScaler = scaler;
            orientMgr.rtCardCurrency = rtCardCurr;
            orientMgr.rtCardRank = rtCardRank;
            orientMgr.rtBtnSettings = rtBtnSettings;
            orientMgr.rtBtnShop = rtBtnShop;
            orientMgr.rtTutorialHint = rtTut;
            orientMgr.rtPanelShop = rtPanelShop;
            orientMgr.rtPanelSettings = settingsView.panelSettings.GetComponent<RectTransform>();
        }

        private static Text CreateText(GameObject parent, string name, string text, Font font, int size, FontStyle style, Color color, TextAnchor align, Vector2 pos, Vector2 sizeDelta, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = pivot;
            rt.anchorMax = pivot;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.text = text;
            return t;
        }

        private static (Button, Image, Text) CreateButtonWithText(GameObject parent, string name, string text, Font font, int size, Vector2 pos, Vector2 sizeDelta, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = pivot;
            rt.anchorMax = pivot;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.22f, 0.25f, 0.32f, 0.95f);
            var btn = go.GetComponent<Button>();

            // Button accent line
            GameObject goLine = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            goLine.transform.SetParent(go.transform, false);
            var rtLine = goLine.GetComponent<RectTransform>();
            rtLine.anchorMin = new Vector2(0f, 0f);
            rtLine.anchorMax = new Vector2(1f, 0f);
            rtLine.sizeDelta = new Vector2(0f, 2f);
            rtLine.anchoredPosition = new Vector2(0f, 1f);
            var imgLine = goLine.GetComponent<Image>();
            imgLine.color = new Color(0.95f, 0.75f, 0.30f, 0.8f);
            imgLine.raycastTarget = false;

            GameObject goTxt = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxt.transform.SetParent(go.transform, false);
            var rtTxt = goTxt.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.sizeDelta = Vector2.zero;
            var txt = goTxt.GetComponent<Text>();
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = text;
            txt.raycastTarget = false;

            var outline = goTxt.GetComponent<Outline>();
            outline.effectColor = new Color(0.08f, 0.08f, 0.08f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            return (btn, img, txt);
        }

        private static Slider CreateSimpleSlider(GameObject parent, Vector2 pos, Vector2 sizeDelta, Vector2 pivot)
        {
            GameObject goSlider = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            goSlider.transform.SetParent(parent.transform, false);
            var rtSlider = goSlider.GetComponent<RectTransform>();
            rtSlider.anchorMin = pivot;
            rtSlider.anchorMax = pivot;
            rtSlider.pivot = pivot;
            rtSlider.anchoredPosition = pos;
            rtSlider.sizeDelta = sizeDelta;
            var slider = goSlider.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Background
            GameObject goBg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            goBg.transform.SetParent(goSlider.transform, false);
            var rtBg = goBg.GetComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0f, 0.25f);
            rtBg.anchorMax = new Vector2(1f, 0.75f);
            rtBg.sizeDelta = Vector2.zero;
            goBg.GetComponent<Image>().color = new Color(0.20f, 0.22f, 0.28f, 0.95f);

            // Fill Area
            GameObject goFillArea = new GameObject("Fill Area", typeof(RectTransform));
            goFillArea.transform.SetParent(goSlider.transform, false);
            var rtFA = goFillArea.GetComponent<RectTransform>();
            rtFA.anchorMin = new Vector2(0f, 0.25f);
            rtFA.anchorMax = new Vector2(1f, 0.75f);
            rtFA.sizeDelta = Vector2.zero;

            GameObject goFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            goFill.transform.SetParent(goFillArea.transform, false);
            var rtFill = goFill.GetComponent<RectTransform>();
            rtFill.sizeDelta = Vector2.zero;
            var imgFill = goFill.GetComponent<Image>();
            imgFill.color = new Color(0.95f, 0.75f, 0.30f, 1f);

            slider.fillRect = rtFill;

            // Handle Slide Area
            GameObject goHandleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            goHandleArea.transform.SetParent(goSlider.transform, false);
            var rtHA = goHandleArea.GetComponent<RectTransform>();
            rtHA.anchorMin = Vector2.zero;
            rtHA.anchorMax = Vector2.one;
            rtHA.sizeDelta = Vector2.zero;

            GameObject goHandle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            goHandle.transform.SetParent(goHandleArea.transform, false);
            var rtH = goHandle.GetComponent<RectTransform>();
            rtH.sizeDelta = new Vector2(24f, sizeDelta.y);
            var imgHandle = goHandle.GetComponent<Image>();
            imgHandle.color = Color.white;

            slider.handleRect = rtH;
            slider.targetGraphic = imgHandle;

            return slider;
        }

        private static SettingsView CreateSettingsPanel(GameObject goCanvas, Font defaultFont, Button btnOpenSettings)
        {
            // PanelSettings: 860x620
            GameObject goPanelSettings = new GameObject("PanelSettings", typeof(RectTransform), typeof(Image));
            goPanelSettings.transform.SetParent(goCanvas.transform, false);
            var imgPanel = goPanelSettings.GetComponent<Image>();
            imgPanel.color = new Color(0.11f, 0.12f, 0.16f, 0.98f);
            var rtPanel = goPanelSettings.GetComponent<RectTransform>();
            rtPanel.anchorMin = new Vector2(0.5f, 0.5f);
            rtPanel.anchorMax = new Vector2(0.5f, 0.5f);
            rtPanel.pivot = new Vector2(0.5f, 0.5f);
            rtPanel.anchoredPosition = Vector2.zero;
            rtPanel.sizeDelta = new Vector2(860f, 520f);

            // Gold Accent Top Line
            GameObject goLine = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            goLine.transform.SetParent(goPanelSettings.transform, false);
            var rtLine = goLine.GetComponent<RectTransform>();
            rtLine.anchorMin = new Vector2(0f, 1f);
            rtLine.anchorMax = new Vector2(1f, 1f);
            rtLine.sizeDelta = new Vector2(0f, 4f);
            rtLine.anchoredPosition = new Vector2(0f, -2f);
            goLine.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.95f);

            // Title
            GameObject goTitle = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTitle.transform.SetParent(goPanelSettings.transform, false);
            var txtTitle = goTitle.GetComponent<Text>();
            txtTitle.font = defaultFont;
            txtTitle.fontSize = 32;
            txtTitle.fontStyle = FontStyle.Bold;
            txtTitle.color = new Color(1.0f, 0.85f, 0.45f);
            txtTitle.text = "CÀI ĐẶT HỆ THỐNG";
            var rtTitle = goTitle.GetComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0f, 1f);
            rtTitle.anchorMax = new Vector2(1f, 1f);
            rtTitle.pivot = new Vector2(0.5f, 1f);
            rtTitle.anchoredPosition = new Vector2(0f, -16f);
            rtTitle.sizeDelta = new Vector2(0f, 42f);
            txtTitle.alignment = TextAnchor.MiddleCenter;

            // Close Button [X]
            GameObject goClose = new GameObject("BtnClose", typeof(RectTransform), typeof(Image), typeof(Button));
            goClose.transform.SetParent(goPanelSettings.transform, false);
            var imgClose = goClose.GetComponent<Image>();
            imgClose.color = new Color(0.75f, 0.22f, 0.22f);
            var btnClose = goClose.GetComponent<Button>();
            var rtClose = goClose.GetComponent<RectTransform>();
            rtClose.anchorMin = new Vector2(1f, 1f);
            rtClose.anchorMax = new Vector2(1f, 1f);
            rtClose.pivot = new Vector2(1f, 1f);
            rtClose.anchoredPosition = new Vector2(-15f, -15f);
            rtClose.sizeDelta = new Vector2(44f, 44f);

            GameObject goCloseTxt = new GameObject("X", typeof(RectTransform), typeof(Text));
            goCloseTxt.transform.SetParent(goClose.transform, false);
            var txtClose = goCloseTxt.GetComponent<Text>();
            txtClose.font = defaultFont;
            txtClose.fontSize = 24;
            txtClose.fontStyle = FontStyle.Bold;
            txtClose.alignment = TextAnchor.MiddleCenter;
            txtClose.color = Color.white;
            txtClose.text = "X";
            var rtCT = goCloseTxt.GetComponent<RectTransform>();
            rtCT.anchorMin = Vector2.zero;
            rtCT.anchorMax = Vector2.one;
            rtCT.sizeDelta = Vector2.zero;

            var settingsView = goCanvas.AddComponent<SettingsView>();
            settingsView.panelSettings = goPanelSettings;
            settingsView.btnCloseSettings = btnClose;
            settingsView.btnOpenSettings = btnOpenSettings;
            settingsView.txtTitle = txtTitle;

            // Content Rows Container (Đã bỏ chế độ Dọc theo yêu cầu)
            float yPos = -85f;
            float rowGap = 65f;

            // 1. SFX Row
            GameObject goSfxRow = new GameObject("Row_SFX", typeof(RectTransform));
            goSfxRow.transform.SetParent(goPanelSettings.transform, false);
            var rtSfxRow = goSfxRow.GetComponent<RectTransform>();
            rtSfxRow.anchorMin = new Vector2(0f, 1f);
            rtSfxRow.anchorMax = new Vector2(1f, 1f);
            rtSfxRow.pivot = new Vector2(0.5f, 1f);
            rtSfxRow.anchoredPosition = new Vector2(0f, yPos);
            rtSfxRow.sizeDelta = new Vector2(0f, 52f);

            var txtSfxLabel = CreateText(goSfxRow, "TxtLabel", "Âm Thanh (SFX):", defaultFont, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(40f, 0f), new Vector2(300f, 48f), new Vector2(0f, 0.5f));
            settingsView.txtSFXLabel = txtSfxLabel;

            var (btnSfx, imgSfx, txtSfxState) = CreateButtonWithText(goSfxRow, "BtnToggleSFX", "BẬT", defaultFont, 20, new Vector2(360f, 0f), new Vector2(110f, 48f), new Vector2(0f, 0.5f));
            settingsView.btnToggleSFX = btnSfx;
            settingsView.txtSFXState = txtSfxState;

            var sliderSfx = CreateSimpleSlider(goSfxRow, new Vector2(490f, 0f), new Vector2(310f, 24f), new Vector2(0f, 0.5f));
            settingsView.sliderSFX = sliderSfx;

            yPos -= rowGap;

            // 2. BGM Row
            GameObject goBgmRow = new GameObject("Row_BGM", typeof(RectTransform));
            goBgmRow.transform.SetParent(goPanelSettings.transform, false);
            var rtBgmRow = goBgmRow.GetComponent<RectTransform>();
            rtBgmRow.anchorMin = new Vector2(0f, 1f);
            rtBgmRow.anchorMax = new Vector2(1f, 1f);
            rtBgmRow.pivot = new Vector2(0.5f, 1f);
            rtBgmRow.anchoredPosition = new Vector2(0f, yPos);
            rtBgmRow.sizeDelta = new Vector2(0f, 52f);

            var txtBgmLabel = CreateText(goBgmRow, "TxtLabel", "Nhạc Nền (BGM):", defaultFont, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(40f, 0f), new Vector2(300f, 48f), new Vector2(0f, 0.5f));
            settingsView.txtBGMLabel = txtBgmLabel;

            var (btnBgm, imgBgm, txtBgmState) = CreateButtonWithText(goBgmRow, "BtnToggleBGM", "BẬT", defaultFont, 20, new Vector2(360f, 0f), new Vector2(110f, 48f), new Vector2(0f, 0.5f));
            settingsView.btnToggleBGM = btnBgm;
            settingsView.txtBGMState = txtBgmState;

            var sliderBgm = CreateSimpleSlider(goBgmRow, new Vector2(490f, 0f), new Vector2(310f, 24f), new Vector2(0f, 0.5f));
            settingsView.sliderBGM = sliderBgm;

            yPos -= rowGap;

            // 3. VFX & Haptics Row
            GameObject goFxRow = new GameObject("Row_FX", typeof(RectTransform));
            goFxRow.transform.SetParent(goPanelSettings.transform, false);
            var rtFxRow = goFxRow.GetComponent<RectTransform>();
            rtFxRow.anchorMin = new Vector2(0f, 1f);
            rtFxRow.anchorMax = new Vector2(1f, 1f);
            rtFxRow.pivot = new Vector2(0.5f, 1f);
            rtFxRow.anchoredPosition = new Vector2(0f, yPos);
            rtFxRow.sizeDelta = new Vector2(0f, 52f);

            var txtVfxLabel = CreateText(goFxRow, "TxtVfxLabel", "Hiệu Ứng (VFX):", defaultFont, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(40f, 0f), new Vector2(190f, 48f), new Vector2(0f, 0.5f));
            settingsView.txtVFXLabel = txtVfxLabel;

            var (btnVfx, imgVfx, txtVfxState) = CreateButtonWithText(goFxRow, "BtnToggleVFX", "BẬT", defaultFont, 20, new Vector2(235f, 0f), new Vector2(115f, 48f), new Vector2(0f, 0.5f));
            settingsView.btnToggleVFX = btnVfx;
            settingsView.txtVFXState = txtVfxState;

            var txtHapLabel = CreateText(goFxRow, "TxtHapLabel", "Rung & Lắc Màn:", defaultFont, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(400f, 0f), new Vector2(230f, 48f), new Vector2(0f, 0.5f));
            settingsView.txtHapticsLabel = txtHapLabel;

            var (btnHap, imgHap, txtHapState) = CreateButtonWithText(goFxRow, "BtnToggleHaptics", "BẬT", defaultFont, 20, new Vector2(640f, 0f), new Vector2(140f, 48f), new Vector2(0f, 0.5f));
            settingsView.btnToggleHaptics = btnHap;
            settingsView.txtHapticsState = txtHapState;

            yPos -= rowGap;

            // 4. Language Row
            GameObject goLangRow = new GameObject("Row_Language", typeof(RectTransform));
            goLangRow.transform.SetParent(goPanelSettings.transform, false);
            var rtLangRow = goLangRow.GetComponent<RectTransform>();
            rtLangRow.anchorMin = new Vector2(0f, 1f);
            rtLangRow.anchorMax = new Vector2(1f, 1f);
            rtLangRow.pivot = new Vector2(0.5f, 1f);
            rtLangRow.anchoredPosition = new Vector2(0f, yPos);
            rtLangRow.sizeDelta = new Vector2(0f, 52f);

            var txtLangLabel = CreateText(goLangRow, "TxtLabel", "Ngôn Ngữ:", defaultFont, 22, FontStyle.Bold, new Color(1f, 0.88f, 0.5f), TextAnchor.MiddleLeft, new Vector2(40f, 0f), new Vector2(300f, 48f), new Vector2(0f, 0.5f));
            settingsView.txtLangLabel = txtLangLabel;

            var (btnVi, imgVi, txtVi) = CreateButtonWithText(goLangRow, "BtnLangVi", "🇻🇳 Tiếng Việt", defaultFont, 20, new Vector2(360f, 0f), new Vector2(210f, 48f), new Vector2(0f, 0.5f));
            settingsView.btnLangVi = btnVi;
            settingsView.imgLangVi = imgVi;
            settingsView.txtLangVi = txtVi;

            var (btnEn, imgEn, txtEn) = CreateButtonWithText(goLangRow, "BtnLangEn", "🇬🇧 English", defaultFont, 20, new Vector2(590f, 0f), new Vector2(210f, 48f), new Vector2(0f, 0.5f));
            settingsView.btnLangEn = btnEn;
            settingsView.imgLangEn = imgEn;
            settingsView.txtLangEn = txtEn;

            yPos -= 70f;

            // 5. Navigation Row: Return to Main Menu & Reset Save Buttons
            var (btnReturn, imgReturn, txtReturn) = CreateButtonWithText(goPanelSettings, "BtnReturnMainMenu", "🏠 VỀ MENU CHÍNH", defaultFont, 20, new Vector2(-190f, yPos), new Vector2(360f, 52f), new Vector2(0.5f, 1f));
            imgReturn.color = new Color(0.20f, 0.35f, 0.55f, 0.95f);
            settingsView.btnReturnMainMenu = btnReturn;
            settingsView.txtReturnMainMenu = txtReturn;

            var (btnReset, imgReset, txtReset) = CreateButtonWithText(goPanelSettings, "BtnResetData", "⚠️ XÓA DỮ LIỆU", defaultFont, 20, new Vector2(190f, yPos), new Vector2(360f, 52f), new Vector2(0.5f, 1f));
            imgReset.color = new Color(0.75f, 0.18f, 0.18f, 0.95f);
            settingsView.btnResetData = btnReset;
            settingsView.txtResetData = txtReset;

            // 7. Reset Confirm Modal
            GameObject goConfirmModal = new GameObject("PanelResetConfirm", typeof(RectTransform), typeof(Image));
            goConfirmModal.transform.SetParent(goPanelSettings.transform, false);
            var imgModal = goConfirmModal.GetComponent<Image>();
            imgModal.color = new Color(0.08f, 0.08f, 0.11f, 0.99f);
            var rtModal = goConfirmModal.GetComponent<RectTransform>();
            rtModal.anchorMin = new Vector2(0.5f, 0.5f);
            rtModal.anchorMax = new Vector2(0.5f, 0.5f);
            rtModal.pivot = new Vector2(0.5f, 0.5f);
            rtModal.anchoredPosition = Vector2.zero;
            rtModal.sizeDelta = new Vector2(650f, 290f);

            var txtConfTitle = CreateText(goConfirmModal, "Title", "XÁC NHẬN RESET", defaultFont, 26, FontStyle.Bold, new Color(1f, 0.4f, 0.4f), TextAnchor.MiddleCenter, new Vector2(0f, -20f), new Vector2(600f, 40f), new Vector2(0.5f, 1f));
            var txtConfMsg = CreateText(goConfirmModal, "Msg", "Toàn bộ quặng và máy móc sẽ quay về 0.\nBạn có chắc muốn chơi lại từ đầu?", defaultFont, 20, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 15f), new Vector2(580f, 80f), new Vector2(0.5f, 0.5f));

            var (btnConf, _, txtConfBtn) = CreateButtonWithText(goConfirmModal, "BtnConfirm", "ĐỒNG Ý (RESET)", defaultFont, 20, new Vector2(-130f, 30f), new Vector2(220f, 50f), new Vector2(0.5f, 0f));
            var (btnCanc, _, txtCancBtn) = CreateButtonWithText(goConfirmModal, "BtnCancel", "HỦY BỎ", defaultFont, 20, new Vector2(130f, 30f), new Vector2(220f, 50f), new Vector2(0.5f, 0f));

            settingsView.panelResetConfirm = goConfirmModal;
            settingsView.txtConfirmTitle = txtConfTitle;
            settingsView.txtConfirmMsg = txtConfMsg;
            settingsView.btnConfirmReset = btnConf;
            settingsView.txtConfirmReset = txtConfBtn;
            settingsView.btnCancelReset = btnCanc;
            settingsView.txtCancelReset = txtCancBtn;

            goConfirmModal.SetActive(false);
            goPanelSettings.SetActive(false);

            return settingsView;
        }

        private static MainMenuView CreateMainMenuPanel(GameObject goCanvas, Font defaultFont, GameManager gm)
        {
            // PanelMainMenu: Fullscreen background image with Tex_MainMenu_BG
            GameObject goPanelMenu = new GameObject("PanelMainMenu", typeof(RectTransform), typeof(Image));
            goPanelMenu.transform.SetParent(goCanvas.transform, false);
            var imgPanel = goPanelMenu.GetComponent<Image>();
            
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Forge/Art/Textures/Tex_MainMenu_BG.jpg");
            if (bgSprite != null)
            {
                imgPanel.sprite = bgSprite;
                imgPanel.color = Color.white;
            }
            else
            {
                imgPanel.color = new Color(0.08f, 0.09f, 0.12f, 1f);
            }

            var rtPanel = goPanelMenu.GetComponent<RectTransform>();
            rtPanel.anchorMin = Vector2.zero;
            rtPanel.anchorMax = Vector2.one;
            rtPanel.sizeDelta = Vector2.zero;
            rtPanel.anchoredPosition = Vector2.zero;

            // Top Gold Accent Line
            GameObject goTopLine = new GameObject("TopAccentLine", typeof(RectTransform), typeof(Image));
            goTopLine.transform.SetParent(goPanelMenu.transform, false);
            var rtTL = goTopLine.GetComponent<RectTransform>();
            rtTL.anchorMin = new Vector2(0f, 1f);
            rtTL.anchorMax = new Vector2(1f, 1f);
            rtTL.pivot = new Vector2(0.5f, 1f);
            rtTL.sizeDelta = new Vector2(0f, 4f);
            rtTL.anchoredPosition = Vector2.zero;
            goTopLine.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.95f);

            var mainMenuView = goCanvas.AddComponent<MainMenuView>();
            mainMenuView.panelMainMenu = goPanelMenu;

            // Header Container (Center top)
            GameObject goHeader = new GameObject("HeaderContainer", typeof(RectTransform));
            goHeader.transform.SetParent(goPanelMenu.transform, false);
            var rtHeader = goHeader.GetComponent<RectTransform>();
            rtHeader.anchorMin = new Vector2(0.5f, 1f);
            rtHeader.anchorMax = new Vector2(0.5f, 1f);
            rtHeader.pivot = new Vector2(0.5f, 1f);
            rtHeader.anchoredPosition = new Vector2(0f, -80f);
            rtHeader.sizeDelta = new Vector2(900f, 220f);

            // Title: 54pt Bold Golden with deep shadow outline
            GameObject goTitle = new GameObject("TxtGameTitle", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTitle.transform.SetParent(goHeader.transform, false);
            var txtTitle = goTitle.GetComponent<Text>();
            txtTitle.font = defaultFont;
            txtTitle.fontSize = 54;
            txtTitle.fontStyle = FontStyle.Bold;
            txtTitle.color = new Color(1.0f, 0.88f, 0.45f);
            txtTitle.alignment = TextAnchor.MiddleCenter;
            txtTitle.text = "HUYỀN THOẠI LÒ RÈN";
            var rtT = goTitle.GetComponent<RectTransform>();
            rtT.anchorMin = new Vector2(0.5f, 1f);
            rtT.anchorMax = new Vector2(0.5f, 1f);
            rtT.pivot = new Vector2(0.5f, 1f);
            rtT.anchoredPosition = new Vector2(0f, 0f);
            rtT.sizeDelta = new Vector2(900f, 65f);
            var outTitle = goTitle.GetComponent<Outline>();
            outTitle.effectColor = new Color(0.18f, 0.08f, 0.02f, 0.95f);
            outTitle.effectDistance = new Vector2(3f, -3f);
            mainMenuView.txtGameTitle = txtTitle;

            // Title Accent Line (440x3)
            GameObject goUnderline = new GameObject("TitleLine", typeof(RectTransform), typeof(Image));
            goUnderline.transform.SetParent(goHeader.transform, false);
            var rtUL = goUnderline.GetComponent<RectTransform>();
            rtUL.anchorMin = new Vector2(0.5f, 1f);
            rtUL.anchorMax = new Vector2(0.5f, 1f);
            rtUL.pivot = new Vector2(0.5f, 1f);
            rtUL.anchoredPosition = new Vector2(0f, -70f);
            rtUL.sizeDelta = new Vector2(440f, 3f);
            goUnderline.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.8f);

            // Tagline: 22pt light gold
            var txtTagline = CreateText(goHeader, "TxtTagline", "🔥 Công Xưởng Hắc Thiết Cổ Đại 🔥", defaultFont, 22, FontStyle.Bold, new Color(0.92f, 0.82f, 0.65f), TextAnchor.MiddleCenter, new Vector2(0f, -82f), new Vector2(800f, 32f), new Vector2(0.5f, 1f));
            mainMenuView.txtTagline = txtTagline;

            // Version: 16pt muted silver
            var txtVer = CreateText(goHeader, "TxtVersion", "v1.0 • Phiên Bản Clicker Rèn Thần Binh", defaultFont, 16, FontStyle.Normal, new Color(0.65f, 0.70f, 0.80f), TextAnchor.MiddleCenter, new Vector2(0f, -118f), new Vector2(600f, 26f), new Vector2(0.5f, 1f));
            mainMenuView.txtVersion = txtVer;

            // Button Stack (3 buttons centered, UI Gen Art)
            var spBtnPlay = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Forge/Art/Textures/UI_Btn_Play.png");
            var spBtnSettings = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Forge/Art/Textures/UI_Btn_Settings.png");
            var spBtnQuit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Forge/Art/Textures/UI_Btn_Quit.png");

            // 1. Play Button (UI Gen Art Gold Forge, 440x100, y = 120)
            var (btnPlay, imgPlay, txtPlay) = CreateButtonWithText(goPanelMenu, "BtnPlayOrContinue", "🔨 BẮT ĐẦU CHƠI", defaultFont, 26, new Vector2(0f, 120f), new Vector2(440f, 100f), new Vector2(0.5f, 0.5f));
            if (spBtnPlay != null)
            {
                imgPlay.sprite = spBtnPlay;
                imgPlay.color = Color.white;
            }
            else
            {
                imgPlay.color = new Color(0.92f, 0.48f, 0.14f);
            }
            txtPlay.color = new Color(1f, 0.96f, 0.88f);
            mainMenuView.btnPlayOrContinue = btnPlay;
            mainMenuView.txtBtnPlay = txtPlay;

            // 2. Settings Button (UI Gen Art Steel Forge, 440x95, y = 0)
            var (btnSet, imgSet, txtSet) = CreateButtonWithText(goPanelMenu, "BtnSettings", "⚙️ CÀI ĐẶT", defaultFont, 23, new Vector2(0f, 0f), new Vector2(440f, 95f), new Vector2(0.5f, 0.5f));
            if (spBtnSettings != null)
            {
                imgSet.sprite = spBtnSettings;
                imgSet.color = Color.white;
            }
            else
            {
                imgSet.color = new Color(0.22f, 0.25f, 0.32f, 0.95f);
            }
            txtSet.color = new Color(0.92f, 0.94f, 0.98f);
            mainMenuView.btnSettings = btnSet;
            mainMenuView.txtBtnSettings = txtSet;

            // 3. Quit Button (UI Gen Art Ruby Forge, 440x90, y = -115)
            var (btnQuit, imgQuit, txtQuit) = CreateButtonWithText(goPanelMenu, "BtnQuit", "🚪 THOÁT GAME", defaultFont, 21, new Vector2(0f, -115f), new Vector2(440f, 90f), new Vector2(0.5f, 0.5f));
            if (spBtnQuit != null)
            {
                imgQuit.sprite = spBtnQuit;
                imgQuit.color = Color.white;
            }
            else
            {
                imgQuit.color = new Color(0.32f, 0.18f, 0.20f, 0.95f);
            }
            txtQuit.color = new Color(1f, 0.90f, 0.90f);
            mainMenuView.btnQuit = btnQuit;
            mainMenuView.txtBtnQuit = txtQuit;

            // Credits Modal (700 x 420)
            GameObject goCreditsModal = new GameObject("PanelCreditsModal", typeof(RectTransform), typeof(Image));
            goCreditsModal.transform.SetParent(goPanelMenu.transform, false);
            var imgCredModal = goCreditsModal.GetComponent<Image>();
            imgCredModal.color = new Color(0.09f, 0.10f, 0.14f, 0.99f);
            var rtCM = goCreditsModal.GetComponent<RectTransform>();
            rtCM.anchorMin = new Vector2(0.5f, 0.5f);
            rtCM.anchorMax = new Vector2(0.5f, 0.5f);
            rtCM.pivot = new Vector2(0.5f, 0.5f);
            rtCM.anchoredPosition = Vector2.zero;
            rtCM.sizeDelta = new Vector2(700f, 420f);

            // Modal Accent Top
            GameObject goCredLine = new GameObject("Line", typeof(RectTransform), typeof(Image));
            goCredLine.transform.SetParent(goCreditsModal.transform, false);
            var rtCL = goCredLine.GetComponent<RectTransform>();
            rtCL.anchorMin = new Vector2(0f, 1f);
            rtCL.anchorMax = new Vector2(1f, 1f);
            rtCL.sizeDelta = new Vector2(0f, 4f);
            rtCL.anchoredPosition = new Vector2(0f, -2f);
            goCredLine.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.25f, 0.95f);

            var txtCredTitle = CreateText(goCreditsModal, "Title", "GIỚI THIỆU & BẢN QUYỀN", defaultFont, 28, FontStyle.Bold, new Color(1.0f, 0.85f, 0.45f), TextAnchor.MiddleCenter, new Vector2(0f, -25f), new Vector2(650f, 40f), new Vector2(0.5f, 1f));
            var txtCredBody = CreateText(goCreditsModal, "Body", "Chào mừng bạn đến với Huyền Thoại Lò Rèn!\nTrải nghiệm clicker mô phỏng công xưởng rèn hắc thiết vô tận.\n\nPhát triển & Thiết kế bởi Trường Giang ✨", defaultFont, 20, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 30f), new Vector2(620f, 150f), new Vector2(0.5f, 0.5f));

            // Visit Studio Button
            var (btnVisit, imgVisit, txtVisit) = CreateButtonWithText(goCreditsModal, "BtnVisitStudio", "🌐 Tác giả: Trường Giang ✨", defaultFont, 20, new Vector2(0f, 85f), new Vector2(360f, 50f), new Vector2(0.5f, 0f));
            imgVisit.color = new Color(0.20f, 0.52f, 0.75f, 0.95f);

            // Close Credits Button
            var (btnCloseCred, _, txtCloseCred) = CreateButtonWithText(goCreditsModal, "BtnCloseCredits", "ĐÓNG", defaultFont, 20, new Vector2(0f, 25f), new Vector2(200f, 45f), new Vector2(0.5f, 0f));

            mainMenuView.panelCreditsModal = goCreditsModal;
            mainMenuView.txtCreditsTitle = txtCredTitle;
            mainMenuView.txtCreditsBody = txtCredBody;
            mainMenuView.btnVisitStudio = btnVisit;
            mainMenuView.txtBtnVisitStudio = txtVisit;
            mainMenuView.btnCloseCredits = btnCloseCred;

            goCreditsModal.SetActive(false);
            goPanelMenu.SetActive(true);

            gm.mainMenuView = mainMenuView;
            return mainMenuView;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int idx = scenes.FindIndex(s => s.path == scenePath);
            if (idx == -1)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        [MenuItem("Forge/Setup/Assign Icons to Generators", false, 1)]
        public static void AssignIconsToGenerators()
        {
            string[] iconFiles = new[]
            {
                "Assets/_Forge/Art/Textures/Icons/Icon_G1_Hammer.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G2_Drill.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G3_Stove.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G4_Gear.png",
                "Assets/_Forge/Art/Textures/Icons/Icon_G5_Robot.png"
            };

            for (int i = 0; i < 5; i++)
            {
                string iconPath = iconFiles[i];
                TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }

                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                string genPath = $"{GENERATORS_DIR}/Gen_G{i + 1}.asset";
                var def = AssetDatabase.LoadAssetAtPath<GeneratorDefinition>(genPath);
                if (def != null && sp != null)
                {
                    def.icon = sp;
                    EditorUtility.SetDirty(def);
                    Debug.Log($"[ForgeSetup] Assigned {sp.name} to {def.machineName}");
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[ForgeSetup] Successfully assigned all icons to GeneratorDefinitions!");
        }

        // ==================== DEBUG TOOLS ====================
        [MenuItem("Forge/Debug/Add 1000 Ore", false, 100)]
        public static void DebugAdd1000Ore()
        {
            if (Application.isPlaying && GameManager.Instance != null)
            {
                GameManager.Instance.DebugAddOre(1000);
            }
            else
            {
                Debug.LogWarning("[ForgeDebug] Game is not running in Play Mode!");
            }
        }

        [MenuItem("Forge/Debug/Add 1e6 Ore", false, 101)]
        public static void DebugAdd1M()
        {
            if (Application.isPlaying && GameManager.Instance != null)
            {
                GameManager.Instance.DebugAddOre(1000000);
            }
            else
            {
                Debug.LogWarning("[ForgeDebug] Game is not running in Play Mode!");
            }
        }

        [MenuItem("Forge/Debug/Unlock All Lv10", false, 102)]
        public static void DebugUnlockAllMilestones()
        {
            if (Application.isPlaying && GameManager.Instance != null)
            {
                GameManager.Instance.DebugUnlockAllLv10();
            }
            else
            {
                Debug.LogWarning("[ForgeDebug] Game is not running in Play Mode!");
            }
        }

        [MenuItem("Forge/Debug/Reset Save", false, 103)]
        public static void DebugResetSave()
        {
            bool confirm = EditorUtility.DisplayDialog("Reset Save", "Bạn có chắc chắn muốn xóa toàn bộ save game cục bộ không?", "Đồng ý", "Hủy");
            if (confirm)
            {
                string path = Path.Combine(Application.persistentDataPath, "forge_save.json");
                string tmp = Path.Combine(Application.persistentDataPath, "forge_save.tmp");
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(tmp)) File.Delete(tmp);
                Debug.Log($"[ForgeDebug] Deleted save file at: {path}");
                EditorUtility.DisplayDialog("Reset Save", "Đã xóa save file thành công! Khởi động lại game để bắt đầu từ đầu.", "OK");
            }
        }

        [MenuItem("Forge/Debug/Simulate Offline 2 Hours", false, 104)]
        public static void DebugSimulateOffline2Hours()
        {
            string path = Path.Combine(Application.persistentDataPath, "forge_save.json");
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Offline Simulation", "Chưa có file save! Hãy vào Play Mode, click lò rèn vài cái rồi tắt đi để sinh save.", "OK");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                DateTime twoHoursAgo = DateTime.UtcNow.AddHours(-2);
                data.lastSaveUtc = twoHoursAgo.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

                string newJson = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, newJson);

                Debug.Log($"[ForgeDebug] Simulated offline save: lastSaveUtc set to {data.lastSaveUtc}");
                EditorUtility.DisplayDialog("Offline Simulation", "Đã tua ngược thời gian save 2 tiếng trước!\nBấm Play Mode để xem Popup nhận thưởng.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ForgeDebug] Failed to simulate offline: {ex.Message}");
            }
        }

        [MenuItem("Forge/Debug/Print Save Path", false, 105)]
        public static void DebugPrintSavePath()
        {
            string path = Path.Combine(Application.persistentDataPath, "forge_save.json");
            Debug.Log($"[ForgeDebug] Save Path: {path}");
            EditorUtility.DisplayDialog("Save Path", path, "OK");
        }

        // ==================== VALIDATE SCENE ====================
        [MenuItem("Forge/Build/Validate Scene", false, 200)]
        public static void ValidateScene()
        {
            Debug.Log("[ForgeValidate] === Starting Full Spec Validation ===");
            var errors = new List<string>();

            if (!EditorSceneManager.GetActiveScene().name.Equals("ForgeDesk", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(SCENE_PATH))
                {
                    EditorSceneManager.OpenScene(SCENE_PATH);
                }
            }

            // 1. [GameSystems] & GameManager
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gm == null)
            {
                errors.Add("Thiếu GameManager trong scene!");
            }

            // 2. Slots & Machines
            for (int i = 1; i <= 5; i++)
            {
                var slot = GameObject.Find($"Slot_G{i}");
                if (slot == null)
                {
                    errors.Add($"Thiếu Slot_G{i}!");
                }
            }

            // 3. SaveService
            if (UnityEngine.Object.FindAnyObjectByType<SaveService>() == null)
            {
                errors.Add("Thiếu SaveService trong scene!");
            }

            // 4. Balance.asset
            var balance = AssetDatabase.LoadAssetAtPath<BalanceConfig>(BALANCE_ASSET_PATH);
            if (balance == null)
            {
                errors.Add("Thiếu Balance.asset trong Data!");
            }

            // 5. G1..G5 definitions
            for (int i = 1; i <= 5; i++)
            {
                var def = AssetDatabase.LoadAssetAtPath<GeneratorDefinition>($"{GENERATORS_DIR}/Gen_G{i}.asset");
                if (def == null || def.id != $"G{i}")
                {
                    errors.Add($"Thiếu hoặc sai ID của GeneratorDefinition G{i}!");
                }
            }

            // 6. Camera FOV 38
            var cam = Camera.main;
            if (cam == null || Math.Abs(cam.fieldOfView - 38f) > 0.1f)
            {
                errors.Add($"Camera chính thiếu hoặc FOV ({cam?.fieldOfView}) không đúng 38!");
            }

            if (errors.Count == 0)
            {
                Debug.Log("[ForgeValidate] Scene VALIDATION PASSED! All requirements satisfied.");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Validate Scene", "Hoàn hảo! Toàn bộ tiêu chuẩn spec đã được thỏa mãn 100%.", "Tuyệt vời");
                }
            }
            else
            {
                string msg = string.Join("\n• ", errors);
                Debug.LogError($"[ForgeValidate] Scene VALIDATION FAILED:\n• {msg}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Validate Scene", $"Phát hiện lỗi:\n• {msg}", "Xem lại");
                }
            }
        }

        [MenuItem("Forge/Setup/Fix Pink Materials (Force URP)", false, 2)]
        public static void FixPinkMaterials()
        {
            Debug.Log("[ForgeSetup] === Fixing Materials & Forcing URP ===");

            string assetPath = $"{URP_SETTINGS_DIR}/Forge_URP_Asset.asset";
            var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
            if (urpAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = urpAsset;
                QualitySettings.renderPipeline = urpAsset;
                Debug.Log("[ForgeSetup] Assigned Forge_URP_Asset to GraphicsSettings and QualitySettings.");
            }
            else
            {
                Debug.LogError("[ForgeSetup] Forge_URP_Asset.asset not found!");
            }

            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader == null)
            {
                Debug.LogError("[ForgeSetup] 'Universal Render Pipeline/Lit' shader not found in project!");
                return;
            }

            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { MATERIALS_DIR });
            int fixedCount = 0;

            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    if (mat.shader == null || mat.shader.name != "Universal Render Pipeline/Lit")
                    {
                        mat.shader = urpLitShader;
                        EditorUtility.SetDirty(mat);
                        fixedCount++;
                        Debug.Log($"[ForgeSetup] Reassigned URP Lit to {mat.name} at {path}");
                    }
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ForgeSetup] === All Materials Refreshed and URP Enforced! (Fixed {fixedCount} materials) ===");
        }
    }
}
