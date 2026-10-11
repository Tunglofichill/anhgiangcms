using System.IO;
using UnityEditor;
using UnityEngine;

namespace Forge.EditorTools
{
    public static class ForgeTextureGenerator
    {
        private const string TEXTURE_DIR = "Assets/_Forge/Art/Textures/PBR";

        [MenuItem("Forge/Art/Generate HD Textures", false, 50)]
        public static void GenerateAllTextures()
        {
            if (!Directory.Exists(TEXTURE_DIR))
            {
                Directory.CreateDirectory(TEXTURE_DIR);
                AssetDatabase.Refresh();
            }

            Debug.Log("[ForgeTextures] Generating HD Procedural PBR Textures with Rich Fantasy Palette...");

            // Core PBR Textures
            SaveTexture("Tex_Oak_Stump.png", GenerateWoodBarkTexture(512, 512));
            SaveTexture("Tex_Workbench_Wood.png", GenerateWorkbenchPlanksTexture(512, 512));
            SaveTexture("Tex_Damascus_Steel.png", GenerateDamascusSteelTexture(512, 512));
            SaveTexture("Tex_Hammered_Dark_Iron.png", GenerateHammeredIronTexture(512, 512));
            SaveTexture("Tex_Stone_Bricks.png", GenerateStoneBricksTexture(512, 512));
            SaveTexture("Tex_Burning_Embers.png", GenerateEmbersTexture(512, 512));
            SaveTexture("Tex_Polished_Bronze.png", GenerateBronzeTexture(512, 512));
            SaveTexture("Tex_Leather_Wrap.png", GenerateLeatherTexture(256, 256));
            SaveTexture("Tex_Hot_Ingot_Glow.png", GenerateHotIngotTexture(256, 256));
            SaveTexture("Tex_Crystal_Ore.png", GenerateCrystalTexture(256, 256));

            // Also update legacy Tex_WoodPlanks.png so any fallback surfaces get warm rich oak
            string legacyWoodPath = "Assets/_Forge/Art/Textures/Tex_WoodPlanks.png";
            byte[] woodBytes = GenerateWorkbenchPlanksTexture(512, 512).EncodeToPNG();
            File.WriteAllBytes(legacyWoodPath, woodBytes);

            AssetDatabase.Refresh();
            ConfigureImportSettings();
            Debug.Log("[ForgeTextures] All HD PBR Textures successfully generated with vibrant colors!");
        }

        private static void SaveTexture(string fileName, Texture2D tex)
        {
            string fullPath = Path.Combine(TEXTURE_DIR, fileName);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(tex);
        }

        private static void ConfigureImportSettings()
        {
            string[] files = Directory.GetFiles(TEXTURE_DIR, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.anisoLevel = 8;
                    importer.SaveAndReimport();
                }
            }

            // Also configure legacy wood planks
            string legacyWoodPath = "Assets/_Forge/Art/Textures/Tex_WoodPlanks.png";
            TextureImporter legacyImporter = AssetImporter.GetAtPath(legacyWoodPath) as TextureImporter;
            if (legacyImporter != null)
            {
                legacyImporter.wrapMode = TextureWrapMode.Repeat;
                legacyImporter.filterMode = FilterMode.Bilinear;
                legacyImporter.anisoLevel = 8;
                legacyImporter.SaveAndReimport();
            }
        }

        /// <summary>
        /// Rich, aged golden-honey oak bark and deep wood fibers with warm amber undertones.
        /// </summary>
        public static Texture2D GenerateWoodBarkTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkBrown = new Color(0.42f, 0.26f, 0.14f);
            Color warmWood = new Color(0.70f, 0.48f, 0.28f);
            Color lightGrain = new Color(0.86f, 0.64f, 0.38f);
            Color goldenGrain = new Color(0.96f, 0.74f, 0.46f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Vertical wood fibers
                    float fiber = Mathf.PerlinNoise(u * 16f, v * 3.5f);
                    float micro = Mathf.PerlinNoise(u * 40f, v * 12f) * 0.35f;
                    float grain = Mathf.Sin((u + fiber * 0.12f) * 55f) * 0.5f + 0.5f;

                    float mix = Mathf.Clamp01(fiber * 0.55f + micro + grain * 0.25f);
                    Color baseCol = Color.Lerp(darkBrown, warmWood, mix);

                    if (mix > 0.65f)
                    {
                        baseCol = Color.Lerp(baseCol, lightGrain, (mix - 0.65f) * 2.5f);
                    }

                    // Occasional golden warm ring
                    float knot = Mathf.PerlinNoise(u * 5f, v * 5f);
                    if (knot > 0.75f)
                    {
                        baseCol = Color.Lerp(baseCol, goldenGrain, (knot - 0.75f) * 2.8f);
                    }

                    tex.SetPixel(x, y, baseCol);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Hand-crafted master artisan workbench wooden planks with rich honey wax polish,
        /// bevelled plank seams, and warm golden highlights.
        /// </summary>
        public static Texture2D GenerateWorkbenchPlanksTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color plankDark = new Color(0.40f, 0.24f, 0.12f);
            Color plankMain = new Color(0.66f, 0.42f, 0.22f);
            Color plankLight = new Color(0.80f, 0.56f, 0.32f);
            Color plankHighlight = new Color(0.90f, 0.68f, 0.42f);
            Color seamShadow = new Color(0.18f, 0.10f, 0.05f);

            int plankCount = 6;

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float plankCoord = u * plankCount;
                    int plankIdx = Mathf.FloorToInt(plankCoord);
                    float plankFrac = plankCoord - plankIdx;

                    // Plank seam edge (deep groove shadow)
                    bool isSeam = (plankFrac < 0.04f || plankFrac > 0.96f);

                    if (isSeam)
                    {
                        float seamNoise = Mathf.PerlinNoise(u * 30f, v * 30f) * 0.2f;
                        tex.SetPixel(x, y, Color.Lerp(seamShadow, plankDark, seamNoise));
                    }
                    else
                    {
                        // Individual plank variation
                        float plankSeed = (plankIdx * 17) % 100 * 0.01f;
                        float woodGrain = Mathf.PerlinNoise(u * 4f + plankSeed, v * 22f);
                        float fineFiber = Mathf.PerlinNoise(u * 25f, v * 60f) * 0.3f;
                        float val = Mathf.Clamp01(woodGrain * 0.7f + fineFiber);

                        Color c = Color.Lerp(plankMain, plankLight, val);

                        // Chamfer highlight near seams
                        if (plankFrac > 0.04f && plankFrac < 0.12f)
                        {
                            float chamfer = 1f - ((plankFrac - 0.04f) / 0.08f);
                            c = Color.Lerp(c, plankHighlight, chamfer * 0.6f);
                        }
                        else if (plankFrac < 0.96f && plankFrac > 0.88f)
                        {
                            float chamfer = (plankFrac - 0.88f) / 0.08f;
                            c = Color.Lerp(c, plankDark, chamfer * 0.4f);
                        }

                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Legendary folded Damascus steel pattern with wave undulations and brilliant mirror reflection.
        /// </summary>
        public static Texture2D GenerateDamascusSteelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkSteel = new Color(0.35f, 0.40f, 0.48f);
            Color mediumSteel = new Color(0.68f, 0.74f, 0.82f);
            Color lightSteel = new Color(0.92f, 0.96f, 1.00f);
            Color mirrorSpecular = new Color(1.00f, 1.00f, 1.00f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Damascus folding wave equations
                    float wave1 = Mathf.Sin(u * 32f + Mathf.Cos(v * 18f) * 2.8f);
                    float wave2 = Mathf.Cos(v * 26f + Mathf.Sin(u * 14f) * 2.2f);
                    float noise = Mathf.PerlinNoise(u * 22f, v * 22f) * 0.35f;

                    float pattern = Mathf.Clamp01((wave1 + wave2) * 0.35f + 0.5f + noise);

                    Color c = Color.Lerp(darkSteel, mediumSteel, pattern);
                    if (pattern > 0.62f)
                    {
                        c = Color.Lerp(c, lightSteel, (pattern - 0.62f) * 2.6f);
                    }
                    if (pattern > 0.85f)
                    {
                        c = Color.Lerp(c, mirrorSpecular, (pattern - 0.85f) * 5.0f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Hammer-forged dark cast iron with micro pitting, hammer dings, and metallic gunmetal sheen.
        /// </summary>
        public static Texture2D GenerateHammeredIronTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color ironBase = new Color(0.24f, 0.25f, 0.28f);
            Color ironHighlight = new Color(0.50f, 0.53f, 0.58f);
            Color pitShadow = new Color(0.14f, 0.14f, 0.16f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Multi-scale hammer dimples
                    float largeDing = Mathf.PerlinNoise(u * 12f, v * 12f);
                    float medDing = Mathf.PerlinNoise(u * 28f, v * 28f) * 0.5f;
                    float fineGrain = Mathf.PerlinNoise(u * 60f, v * 60f) * 0.25f;

                    float val = largeDing + medDing + fineGrain;

                    Color c = Color.Lerp(ironBase, ironHighlight, Mathf.Clamp01(val * 0.7f));
                    if (val < 0.55f)
                    {
                        c = Color.Lerp(pitShadow, c, val / 0.55f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Warm Dwarven Golden Sandstone & Granite Masonry with warm golden chiselled highlights.
        /// Banishes all cold grey tones and produces glowing royal hearth masonry!
        /// </summary>
        public static Texture2D GenerateStoneBricksTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color stoneLight = new Color(0.68f, 0.70f, 0.72f);
            Color stoneDark = new Color(0.36f, 0.38f, 0.40f);
            Color stoneCool = new Color(0.50f, 0.52f, 0.55f);
            Color mortar = new Color(0.20f, 0.22f, 0.24f);
            Color chiselHighlight = new Color(0.80f, 0.82f, 0.85f);

            int brickRows = 8;
            int brickCols = 4;

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                int row = Mathf.FloorToInt(v * brickRows);
                float rowFrac = (v * brickRows) - row;

                // Alternate row stagger
                float uShift = (row % 2 == 0) ? 0f : 0.5f;

                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float shiftedU = (u + uShift) % 1.0f;
                    int col = Mathf.FloorToInt(shiftedU * brickCols);
                    float colFrac = (shiftedU * brickCols) - col;

                    // Mortar seams
                    bool isMortar = (rowFrac < 0.07f || rowFrac > 0.93f || colFrac < 0.05f || colFrac > 0.95f);

                    if (isMortar)
                    {
                        float mNoise = Mathf.PerlinNoise(u * 40f, v * 40f) * 0.3f;
                        tex.SetPixel(x, y, Color.Lerp(mortar, stoneDark, mNoise));
                    }
                    else
                    {
                        // Stone variation per brick
                        float brickSeed = (row * 13 + col * 37) % 100 * 0.01f;
                        float stoneNoise = Mathf.PerlinNoise(u * 18f + brickSeed, v * 18f + brickSeed);
                        float fineChisel = Mathf.PerlinNoise(u * 50f, v * 50f) * 0.25f;

                        Color brickCol = Color.Lerp(stoneDark, stoneLight, stoneNoise);
                        brickCol = Color.Lerp(brickCol, stoneCool, fineChisel);

                        // Chiselled cool highlight edge
                        if (rowFrac > 0.86f || colFrac < 0.12f)
                        {
                            brickCol = Color.Lerp(brickCol, chiselHighlight, 0.40f);
                        }

                        tex.SetPixel(x, y, brickCol);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Glowing burning coal embers bed with incandescent magma cracks and golden hot veins.
        /// </summary>
        public static Texture2D GenerateEmbersTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color deepAsh = new Color(0.28f, 0.12f, 0.06f);
            Color darkCoal = new Color(0.42f, 0.18f, 0.08f);
            Color glowingOrange = new Color(1.0f, 0.45f, 0.08f);
            Color blazingYellow = new Color(1.0f, 0.90f, 0.35f);
            Color whiteHot = new Color(1.0f, 0.98f, 0.85f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Magma crack lines
                    float crack1 = Mathf.PerlinNoise(u * 14f, v * 14f);
                    float crack2 = Mathf.PerlinNoise(u * 28f, v * 28f) * 0.5f;
                    float val = crack1 + crack2;

                    // Sharp threshold for incandescent veins
                    if (val > 0.90f)
                    {
                        float heat = (val - 0.90f) / 0.35f;
                        tex.SetPixel(x, y, Color.Lerp(blazingYellow, whiteHot, heat));
                    }
                    else if (val > 0.75f)
                    {
                        float heat = (val - 0.75f) / 0.25f;
                        tex.SetPixel(x, y, Color.Lerp(glowingOrange, blazingYellow, heat));
                    }
                    else if (val > 0.55f)
                    {
                        float heat = (val - 0.55f) / 0.20f;
                        tex.SetPixel(x, y, Color.Lerp(darkCoal, glowingOrange, heat));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.Lerp(deepAsh, darkCoal, val / 0.55f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Gleaming minted 24K pure gold bullion and polished royal brass with brilliant specular reflections.
        /// </summary>
        public static Texture2D GenerateBronzeTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color goldDeep = new Color(0.85f, 0.62f, 0.12f);
            Color goldPure = new Color(1.00f, 0.84f, 0.24f);
            Color goldHighlight = new Color(1.00f, 0.96f, 0.65f);
            Color goldSpecular = new Color(1.00f, 1.00f, 0.92f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    float p1 = Mathf.PerlinNoise(u * 14f, v * 14f);
                    float p2 = Mathf.PerlinNoise(u * 32f, v * 32f) * 0.3f;
                    float pattern = p1 + p2;

                    Color c = Color.Lerp(goldDeep, goldPure, pattern);
                    if (pattern > 0.70f)
                    {
                        c = Color.Lerp(c, goldHighlight, (pattern - 0.70f) * 2.8f);
                    }
                    if (pattern > 0.90f)
                    {
                        c = Color.Lerp(c, goldSpecular, (pattern - 0.90f) * 8.0f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Stitched criss-cross rich burgundy leather strap grip texture for hammer and tool handles.
        /// </summary>
        public static Texture2D GenerateLeatherTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkLeather = new Color(0.32f, 0.18f, 0.12f);
            Color medLeather = new Color(0.52f, 0.32f, 0.20f);
            Color highlight = new Color(0.68f, 0.46f, 0.30f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Diamond wrap diagonal lines
                    float diag1 = Mathf.Sin((u + v * 3f) * 20f);
                    float diag2 = Mathf.Sin((u - v * 3f) * 20f);
                    float weave = Mathf.Abs(diag1 * diag2);

                    float noise = Mathf.PerlinNoise(u * 20f, v * 20f) * 0.2f;
                    Color c = Color.Lerp(darkLeather, medLeather, weave + noise);
                    if (weave > 0.7f)
                    {
                        c = Color.Lerp(c, highlight, (weave - 0.7f) * 2f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// White-hot glowing steel ingot with gradient cooling crust at edges.
        /// </summary>
        public static Texture2D GenerateHotIngotTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color whiteHot = new Color(1.0f, 0.98f, 0.88f);
            Color moltenYellow = new Color(1.0f, 0.80f, 0.22f);
            Color fieryOrange = new Color(1.0f, 0.42f, 0.08f);
            Color coolCrust = new Color(0.48f, 0.18f, 0.10f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width - 0.5f;
                    float v = (float)y / height - 0.5f;
                    float dist = Mathf.Sqrt(u * u * 1.5f + v * v * 4f); // Oblong heat gradient

                    float noise = Mathf.PerlinNoise((u + 0.5f) * 10f, (v + 0.5f) * 10f) * 0.15f;
                    dist += noise;

                    Color c;
                    if (dist < 0.22f)
                        c = Color.Lerp(whiteHot, moltenYellow, dist / 0.22f);
                    else if (dist < 0.42f)
                        c = Color.Lerp(moltenYellow, fieryOrange, (dist - 0.22f) / 0.20f);
                    else
                        c = Color.Lerp(fieryOrange, coolCrust, Mathf.Clamp01((dist - 0.42f) / 0.25f));

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Faceted glowing mystical arcane crystal ore with sharp spectral bands and radiant cyan-emerald brilliance.
        /// </summary>
        public static Texture2D GenerateCrystalTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color cyanDeep = new Color(0.02f, 0.55f, 0.78f);
            Color cyanBright = new Color(0.15f, 0.92f, 1.00f);
            Color cyanGlow = new Color(0.75f, 1.00f, 1.00f);
            Color pureWhite = new Color(1.00f, 1.00f, 1.00f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    float f1 = Mathf.Sin(u * 22f + v * 14f);
                    float f2 = Mathf.Cos(v * 28f - u * 16f);
                    float facet = Mathf.Abs(f1 + f2) * 0.5f;

                    Color c = Color.Lerp(cyanDeep, cyanBright, facet);
                    if (facet > 0.70f)
                    {
                        c = Color.Lerp(c, cyanGlow, (facet - 0.70f) * 3f);
                    }
                    if (facet > 0.90f)
                    {
                        c = Color.Lerp(c, pureWhite, (facet - 0.90f) * 8f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
