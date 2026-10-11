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

            Debug.Log("[ForgeTextures] Generating HD Procedural PBR Textures...");

            SaveTexture("Tex_Oak_Stump.png", GenerateWoodBarkTexture(512, 512));
            SaveTexture("Tex_Damascus_Steel.png", GenerateDamascusSteelTexture(512, 512));
            SaveTexture("Tex_Hammered_Dark_Iron.png", GenerateHammeredIronTexture(512, 512));
            SaveTexture("Tex_Stone_Bricks.png", GenerateStoneBricksTexture(512, 512));
            SaveTexture("Tex_Burning_Embers.png", GenerateEmbersTexture(512, 512));
            SaveTexture("Tex_Polished_Bronze.png", GenerateBronzeTexture(512, 512));
            SaveTexture("Tex_Leather_Wrap.png", GenerateLeatherTexture(256, 256));
            SaveTexture("Tex_Hot_Ingot_Glow.png", GenerateHotIngotTexture(256, 256));
            SaveTexture("Tex_Crystal_Ore.png", GenerateCrystalTexture(256, 256));

            AssetDatabase.Refresh();
            ConfigureImportSettings();
            Debug.Log("[ForgeTextures] All HD PBR Textures successfully generated!");
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
        }

        /// <summary>
        /// Rich, aged oak bark and grain with growth rings and cracks.
        /// </summary>
        public static Texture2D GenerateWoodBarkTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkBrown = new Color(0.24f, 0.16f, 0.10f);
            Color warmWood = new Color(0.46f, 0.32f, 0.19f);
            Color lightGrain = new Color(0.58f, 0.42f, 0.26f);
            Color ironRing = new Color(0.18f, 0.12f, 0.08f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Vertical wood fibers
                    float fiber = Mathf.PerlinNoise(u * 18f, v * 3.5f);
                    float micro = Mathf.PerlinNoise(u * 45f, v * 12f) * 0.35f;
                    float grain = Mathf.Sin((u + fiber * 0.12f) * 60f) * 0.5f + 0.5f;

                    float mix = Mathf.Clamp01(fiber * 0.6f + micro + grain * 0.25f);
                    Color baseCol = Color.Lerp(darkBrown, warmWood, mix);

                    // Occasional knot / cracks
                    float knot = Mathf.PerlinNoise(u * 6f, v * 6f);
                    if (knot > 0.72f)
                    {
                        baseCol = Color.Lerp(baseCol, ironRing, (knot - 0.72f) * 3f);
                    }

                    tex.SetPixel(x, y, baseCol);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Legendary folded Damascus steel pattern with wave undulations and iridescent sheen.
        /// </summary>
        public static Texture2D GenerateDamascusSteelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkSteel = new Color(0.18f, 0.20f, 0.23f);
            Color lightSteel = new Color(0.78f, 0.82f, 0.88f);
            Color mediumSteel = new Color(0.45f, 0.48f, 0.54f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // Damascus folding wave equations
                    float wave1 = Mathf.Sin(u * 35f + Mathf.Cos(v * 20f) * 3f);
                    float wave2 = Mathf.Cos(v * 28f + Mathf.Sin(u * 15f) * 2.5f);
                    float noise = Mathf.PerlinNoise(u * 25f, v * 25f) * 0.4f;

                    float pattern = Mathf.Clamp01((wave1 + wave2) * 0.35f + 0.5f + noise);

                    Color c = Color.Lerp(darkSteel, mediumSteel, pattern);
                    if (pattern > 0.65f)
                    {
                        c = Color.Lerp(c, lightSteel, (pattern - 0.65f) * 2.8f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Hammer-forged dark cast iron with micro pitting, hammer dings, and subtle runic textures.
        /// </summary>
        public static Texture2D GenerateHammeredIronTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color ironBase = new Color(0.16f, 0.17f, 0.20f);
            Color ironHighlight = new Color(0.38f, 0.40f, 0.45f);
            Color pitShadow = new Color(0.08f, 0.08f, 0.10f);

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
        /// Ancient hand-carved stone hearth granite masonry with pronounced mortar channels and warm chiselled highlights.
        /// </summary>
        public static Texture2D GenerateStoneBricksTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color stoneLight = new Color(0.85f, 0.80f, 0.74f);
            Color stoneDark = new Color(0.64f, 0.58f, 0.52f);
            Color mortar = new Color(0.32f, 0.28f, 0.25f);
            Color soot = new Color(0.20f, 0.16f, 0.14f);

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
                        tex.SetPixel(x, y, Color.Lerp(mortar, soot, mNoise));
                    }
                    else
                    {
                        // Stone variation per brick
                        float brickSeed = (row * 13 + col * 37) % 100 * 0.01f;
                        float stoneNoise = Mathf.PerlinNoise(u * 20f + brickSeed, v * 20f + brickSeed);
                        float sootNoise = Mathf.PerlinNoise(u * 5f, v * 5f);

                        Color brickCol = Color.Lerp(stoneDark, stoneLight, stoneNoise);
                        if (sootNoise > 0.70f)
                        {
                            brickCol = Color.Lerp(brickCol, soot, (sootNoise - 0.70f) * 1.8f);
                        }

                        tex.SetPixel(x, y, brickCol);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Glowing burning coal embers bed with incandescent cracks and deep ash crust.
        /// </summary>
        public static Texture2D GenerateEmbersTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color deepAsh = new Color(0.12f, 0.06f, 0.04f);
            Color darkCoal = new Color(0.22f, 0.10f, 0.05f);
            Color glowingOrange = new Color(1.0f, 0.42f, 0.08f);
            Color blazingYellow = new Color(1.0f, 0.88f, 0.35f);

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
                    if (val > 0.85f)
                    {
                        float heat = (val - 0.85f) / 0.4f;
                        tex.SetPixel(x, y, Color.Lerp(glowingOrange, blazingYellow, heat));
                    }
                    else if (val > 0.65f)
                    {
                        float heat = (val - 0.65f) / 0.20f;
                        tex.SetPixel(x, y, Color.Lerp(darkCoal, glowingOrange, heat));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.Lerp(deepAsh, darkCoal, val / 0.65f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Gleaming minted gold bullion and polished brass with brilliant highlights.
        /// </summary>
        public static Texture2D GenerateBronzeTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color bronzeGold = new Color(0.98f, 0.80f, 0.26f);
            Color bronzeShadow = new Color(0.55f, 0.38f, 0.12f);
            Color bronzeHighlight = new Color(1.0f, 0.95f, 0.70f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    float p1 = Mathf.PerlinNoise(u * 15f, v * 15f);
                    float p2 = Mathf.PerlinNoise(u * 35f, v * 35f) * 0.3f;
                    float pattern = p1 + p2;

                    Color c = Color.Lerp(bronzeShadow, bronzeGold, pattern);
                    if (pattern > 0.72f)
                    {
                        c = Color.Lerp(c, bronzeHighlight, (pattern - 0.72f) * 2.8f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Stitched criss-cross leather strap grip texture for hammer and tool handles.
        /// </summary>
        public static Texture2D GenerateLeatherTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color darkLeather = new Color(0.20f, 0.12f, 0.08f);
            Color medLeather = new Color(0.38f, 0.24f, 0.15f);
            Color highlight = new Color(0.50f, 0.35f, 0.22f);

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
            Color whiteHot = new Color(1.0f, 0.98f, 0.85f);
            Color moltenYellow = new Color(1.0f, 0.75f, 0.18f);
            Color fieryOrange = new Color(1.0f, 0.35f, 0.05f);
            Color coolCrust = new Color(0.32f, 0.12f, 0.08f);

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
        /// Faceted glowing mystical crystal ore with sharp spectral bands.
        /// </summary>
        public static Texture2D GenerateCrystalTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color cyanDeep = new Color(0.08f, 0.45f, 0.65f);
            Color cyanBright = new Color(0.20f, 0.88f, 0.95f);
            Color cyanGlow = new Color(0.70f, 1.0f, 0.98f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    float f1 = Mathf.Sin(u * 24f + v * 12f);
                    float f2 = Mathf.Cos(v * 30f - u * 15f);
                    float facet = Mathf.Abs(f1 + f2) * 0.5f;

                    Color c = Color.Lerp(cyanDeep, cyanBright, facet);
                    if (facet > 0.75f)
                    {
                        c = Color.Lerp(c, cyanGlow, (facet - 0.75f) * 3f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
