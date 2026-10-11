using System.Collections.Generic;
using UnityEngine;
using Forge.World;

namespace Forge.EditorTools
{
    /// <summary>
    /// Master 3D Sculptor for Legend of the Forge.
    /// Builds high-fidelity, detailed procedural 3D assets with rich PBR materials,
    /// authentic blacksmith details (roots, rivets, runes, glowing embers, bellows, quench bucket),
    /// and dynamic animations.
    /// </summary>
    public static class Forge3DBuilder
    {
        public static GameObject CreatePart(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.transform.localRotation = localRot;

            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sharedMaterial = mat;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                rend.receiveShadows = true;
            }
            return go;
        }

        /// <summary>
        /// Builds the Master Artisan 3D Dwarven Workbench:
        /// Rich beveled oak slab tabletop, vertical timber apron with rhythmic battens,
        /// front protective iron armor plate with gleaming bronze rivets, and heavy trestle foundation.
        /// </summary>
        public static GameObject BuildHeroWorkbench(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Desk");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matWood = mats["Desk"];
            Material matDarkIron = mats["Dark_Iron"];
            Material matBronze = mats["Bronze_Brass"];

#if UNITY_EDITOR
            var modelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Forge/Art/Models/Workbench_Master.obj");
            if (modelPrefab != null)
            {
                var inst = Object.Instantiate(modelPrefab, root.transform);
                inst.name = "Workbench_Mesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;

                var rends = inst.GetComponentsInChildren<Renderer>();
                foreach (var r in rends)
                {
                    string n = r.gameObject.name.ToLowerInvariant();
                    if (n.Contains("wood") || n.Contains("oak") || n.Contains("plank") || n.Contains("stump")) r.sharedMaterial = matWood;
                    else if (n.Contains("dark") || n.Contains("iron")) r.sharedMaterial = matDarkIron;
                    else if (n.Contains("bronze") || n.Contains("brass") || n.Contains("gold")) r.sharedMaterial = matBronze;
                    else
                    {
                        if (r.sharedMaterials != null && r.sharedMaterials.Length >= 3)
                            r.sharedMaterials = new Material[] { matWood, matDarkIron, matBronze };
                        else
                            r.sharedMaterial = matWood;
                    }
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
                return root;
            }
#endif
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Desk_Fallback";
            cube.transform.SetParent(root.transform, false);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(4.734f, 0.80f, 1.50f);
            var rend = cube.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = matWood;
            var col = cube.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            return root;
        }

        /// <summary>
        /// Creates a subtle dark contact shadow decal beneath heavy props (Anvil, Furnace, Materials)
        /// to firmly ground them on the workbench and eliminate any floating sensation.
        /// </summary>
        public static GameObject CreateContactShadow(Transform parent, Vector3 localPos, Vector2 size, float opacity = 0.55f)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Contact_Shadow";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
#if UNITY_EDITOR
                string matPath = "Assets/_Forge/Art/Materials/Mat_Contact_Shadow.mat";
                Material shadowMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (shadowMat == null)
                {
                    Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                    shadowMat = new Material(unlit) { name = "Mat_Contact_Shadow" };
                    shadowMat.color = new Color(0.04f, 0.02f, 0.01f, opacity);
                    shadowMat.SetFloat("_Surface", 1f); // 1 = Transparent
                    shadowMat.SetFloat("_Blend", 0f); // 0 = Alpha blend
                    shadowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    shadowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    shadowMat.SetInt("_ZWrite", 0);
                    shadowMat.renderQueue = 3000;
                    UnityEditor.AssetDatabase.CreateAsset(shadowMat, matPath);
                }
                rend.sharedMaterial = shadowMat;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;
#endif
            }
            return go;
        }

        /// <summary>
        /// Builds a Masterpiece 3D Dwarven Blacksmith Anvil:
        /// Giant aged oak stump with 4 heavy root buttresses, double iron bands with 3D rivets,
        /// cast-iron flared anvil base, graceful waist curves, runic side relief,
        /// mirror-polished Damascus steel face, tapered horn, heel with hardy & pritchel holes,
        /// red-hot glowing forged sword blank on the anvil, and smith's companion hammer.
        /// </summary>
        public static GameObject BuildHeroAnvil(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Anvil_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matDarkIron = mats["Dark_Iron"];
            Material matSteel = mats["Steel_Polished"];
            Material matWood = mats["Wood_Stump"];
            Material matIngot = mats["Hot_Ingot"];
            Material matBronze = mats["Bronze_Brass"];
            Material matFloor = mats["Floor"];

#if UNITY_EDITOR
            var modelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Forge/Art/Models/Anvil_Dwarven.obj");
            if (modelPrefab != null)
            {
                var inst = Object.Instantiate(modelPrefab, root.transform);
                inst.name = "Anvil_Mesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
                var rends = inst.GetComponentsInChildren<Renderer>();
                foreach (var r in rends)
                {
                    string n = r.gameObject.name.ToLowerInvariant();
                    if (n.Contains("wood")) r.sharedMaterial = matWood;
                    else if (n.Contains("dark") || n.Contains("iron")) r.sharedMaterial = matDarkIron;
                    else if (n.Contains("steel")) r.sharedMaterial = matSteel;
                    else if (n.Contains("ingot") || n.Contains("hot")) r.sharedMaterial = matIngot;
                    else
                    {
                        if (r.sharedMaterials != null && r.sharedMaterials.Length >= 4)
                            r.sharedMaterials = new Material[] { matWood, matDarkIron, matSteel, matIngot };
                        else
                            r.sharedMaterial = matDarkIron;
                    }
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
                return root;
            }
#endif
            // 1. ANCIENT OAK STUMP BASE (Chân Đe Gốc Gỗ Cổ Thụ)
            CreatePart(PrimitiveType.Cylinder, "Oak_Stump_Base", root.transform,
                new Vector3(0f, 0.24f, 0f), new Vector3(0.72f, 0.24f, 0.72f), Quaternion.identity, matWood);

            // 4 Heavy Root Buttresses splaying into the ground
            Vector3[] rootDirs = { new Vector3(0.36f, 0.08f, 0.36f), new Vector3(-0.36f, 0.08f, 0.36f),
                                   new Vector3(0.36f, 0.08f, -0.36f), new Vector3(-0.36f, 0.08f, -0.36f) };
            for (int r = 0; r < rootDirs.Length; r++)
            {
                CreatePart(PrimitiveType.Cube, $"Root_Flange_{r}", root.transform,
                    rootDirs[r], new Vector3(0.18f, 0.16f, 0.18f), Quaternion.Euler(0f, 45f * r, 0f), matWood);
            }

            // Top & Bottom Heavy Iron Reinforcement Hoops
            CreatePart(PrimitiveType.Cylinder, "Iron_Strap_Upper", root.transform,
                new Vector3(0f, 0.40f, 0f), new Vector3(0.75f, 0.032f, 0.75f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cylinder, "Iron_Strap_Lower", root.transform,
                new Vector3(0f, 0.10f, 0f), new Vector3(0.76f, 0.032f, 0.76f), Quaternion.identity, matDarkIron);

            // 8 Heavy 3D Iron Rivets around upper strap
            for (int i = 0; i < 8; i++)
            {
                float ang = i * Mathf.PI * 0.25f;
                Vector3 rivetPos = new Vector3(Mathf.Cos(ang) * 0.38f, 0.40f, Mathf.Sin(ang) * 0.38f);
                CreatePart(PrimitiveType.Sphere, $"Rivet_Upper_{i}", root.transform,
                    rivetPos, new Vector3(0.035f, 0.035f, 0.035f), Quaternion.identity, matBronze);
            }

            // 2 Heavy Iron Securing Staples (nêm sắt đóng chặt chân đe)
            CreatePart(PrimitiveType.Cube, "Securing_Staple_L", root.transform,
                new Vector3(-0.28f, 0.48f, 0f), new Vector3(0.045f, 0.10f, 0.07f), Quaternion.Euler(0f, 0f, 15f), matDarkIron);
            CreatePart(PrimitiveType.Cube, "Securing_Staple_R", root.transform,
                new Vector3(0.28f, 0.48f, 0f), new Vector3(0.045f, 0.10f, 0.07f), Quaternion.Euler(0f, 0f, -15f), matDarkIron);

            // -------------------------------------------------------------
            // 2. DWARVEN MASTER ANVIL (Thân Đe Đúc Gang Nghệ Thuật)
            // -------------------------------------------------------------
            // Flared Cast-Iron Footing Base
            CreatePart(PrimitiveType.Cube, "Anvil_Base_Foot", root.transform,
                new Vector3(0f, 0.51f, 0f), new Vector3(0.54f, 0.075f, 0.34f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cube, "Anvil_Base_Arch", root.transform,
                new Vector3(0f, 0.56f, 0f), new Vector3(0.44f, 0.055f, 0.28f), Quaternion.identity, matDarkIron);

            // Graceful Tapered Waist (Eo đe thon gọn)
            CreatePart(PrimitiveType.Cube, "Anvil_Waist_Lower", root.transform,
                new Vector3(0.02f, 0.63f, 0f), new Vector3(0.26f, 0.09f, 0.22f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cube, "Anvil_Waist_Upper", root.transform,
                new Vector3(0.03f, 0.72f, 0f), new Vector3(0.30f, 0.09f, 0.23f), Quaternion.identity, matDarkIron);

            // 3D Runic Relief Carvings on front and back waist sides
            CreatePart(PrimitiveType.Cube, "Runic_Emboss_Front", root.transform,
                new Vector3(0.03f, 0.68f, -0.125f), new Vector3(0.14f, 0.07f, 0.02f), Quaternion.Euler(0f, 0f, 45f), matBronze);
            CreatePart(PrimitiveType.Cube, "Runic_Emboss_Back", root.transform,
                new Vector3(0.03f, 0.68f, 0.125f), new Vector3(0.14f, 0.07f, 0.02f), Quaternion.Euler(0f, 0f, 45f), matBronze);

            // Anvil Body Crown
            CreatePart(PrimitiveType.Cube, "Anvil_Upper_Body", root.transform,
                new Vector3(0.04f, 0.80f, 0f), new Vector3(0.46f, 0.10f, 0.26f), Quaternion.identity, matDarkIron);

            // Mirror-Polished Damascus Steel Face / Table (Mặt đe rèn phẳng lì)
            CreatePart(PrimitiveType.Cube, "Anvil_Face_Table", root.transform,
                new Vector3(0.06f, 0.865f, 0f), new Vector3(0.40f, 0.035f, 0.24f), Quaternion.identity, matSteel);

            // Sừng đe tròn vuốt nhọn hướng sang trái (Horn)
            CreatePart(PrimitiveType.Cylinder, "Anvil_Horn_Root", root.transform,
                new Vector3(-0.24f, 0.82f, 0f), new Vector3(0.20f, 0.18f, 0.20f), Quaternion.Euler(0f, 0f, 75f), matSteel);
            CreatePart(PrimitiveType.Cylinder, "Anvil_Horn_Cone", root.transform,
                new Vector3(-0.36f, 0.85f, 0f), new Vector3(0.12f, 0.14f, 0.12f), Quaternion.Euler(0f, 0f, 80f), matSteel);
            CreatePart(PrimitiveType.Sphere, "Anvil_Horn_Tip", root.transform,
                new Vector3(-0.43f, 0.865f, 0f), new Vector3(0.06f, 0.06f, 0.06f), Quaternion.identity, matSteel);

            // Gót đe vuông vươn sang phải (Heel)
            CreatePart(PrimitiveType.Cube, "Anvil_Heel", root.transform,
                new Vector3(0.30f, 0.82f, 0f), new Vector3(0.16f, 0.09f, 0.22f), Quaternion.identity, matDarkIron);

            // Lỗ đục vuông (Hardy Hole) & Lỗ đột tròn (Pritchel Hole)
            CreatePart(PrimitiveType.Cube, "Hardy_Hole", root.transform,
                new Vector3(0.30f, 0.875f, 0.035f), new Vector3(0.045f, 0.02f, 0.045f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cylinder, "Pritchel_Hole", root.transform,
                new Vector3(0.30f, 0.875f, -0.045f), new Vector3(0.028f, 0.02f, 0.028f), Quaternion.identity, matDarkIron);

            // -------------------------------------------------------------
            // 3. GLOWING RED-HOT SWORD BLADE BLANK (Phôi Kiếm Thần Binh)
            // -------------------------------------------------------------
            // Hot blade section
            CreatePart(PrimitiveType.Cube, "Hot_Blade_Body", root.transform,
                new Vector3(-0.02f, 0.90f, 0f), new Vector3(0.26f, 0.032f, 0.075f), Quaternion.Euler(0f, -12f, 0f), matIngot);
            // Tapered hot blade tip extending onto horn
            CreatePart(PrimitiveType.Cube, "Hot_Blade_Tip", root.transform,
                new Vector3(-0.16f, 0.905f, 0.03f), new Vector3(0.09f, 0.028f, 0.055f), Quaternion.Euler(0f, -22f, 0f), matIngot);
            // Tang of the blade (chuôi rèn)
            CreatePart(PrimitiveType.Cube, "Hot_Blade_Tang", root.transform,
                new Vector3(0.12f, 0.895f, -0.03f), new Vector3(0.08f, 0.025f, 0.035f), Quaternion.Euler(0f, -10f, 0f), matDarkIron);

            // -------------------------------------------------------------
            // 4. COMPANION SMITH HAMMER RESTING BESIDE INVENTORY (Búa Phụ)
            // -------------------------------------------------------------
            CreatePart(PrimitiveType.Cylinder, "Resting_Hammer_Handle", root.transform,
                new Vector3(0.18f, 0.91f, 0.15f), new Vector3(0.035f, 0.26f, 0.035f), Quaternion.Euler(0f, 40f, 85f), matWood);
            CreatePart(PrimitiveType.Cube, "Resting_Hammer_Head", root.transform,
                new Vector3(0.08f, 0.915f, 0.06f), new Vector3(0.085f, 0.065f, 0.065f), Quaternion.Euler(0f, 40f, 0f), matDarkIron);
            CreatePart(PrimitiveType.Cube, "Resting_Hammer_Face", root.transform,
                new Vector3(0.05f, 0.915f, 0.035f), new Vector3(0.04f, 0.068f, 0.068f), Quaternion.Euler(0f, 40f, 0f), matSteel);

            return root;
        }

        /// <summary>
        /// Builds an Epic 3D Dwarven Stone Masonry Furnace:
        /// Stone masonry foundation, arched stone firebox, glowing 3D coal embers bed,
        /// heavy iron smokehood with rivets, tall chimney flue with iron crown,
        /// and authentic accessories: working leather bellows, wooden quench water bucket,
        /// firewood pile, and tongs.
        /// </summary>
        public static GameObject BuildHeroFurnace(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Furnace_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matStone = mats["Stone_Furnace"];
            Material matDarkIron = mats["Dark_Iron"];
            Material matSteel = mats["Steel_Polished"];
            Material matFire = mats["Furnace_Fire"];
            Material matEmber = mats["FurnaceEmber"];
            Material matWood = mats["Wood_Stump"];
            Material matBronze = mats["Bronze_Brass"];
            Material matFloor = mats["Floor"];

#if UNITY_EDITOR
            var modelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Forge/Art/Models/Furnace_Dwarven.obj");
            if (modelPrefab != null)
            {
                var inst = Object.Instantiate(modelPrefab, root.transform);
                inst.name = "Furnace_Mesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
                var rends = inst.GetComponentsInChildren<Renderer>();
                foreach (var r in rends)
                {
                    string n = r.gameObject.name.ToLowerInvariant();
                    if (n.Contains("stone")) r.sharedMaterial = matStone;
                    else if (n.Contains("bronze") || n.Contains("brass")) r.sharedMaterial = matBronze;
                    else if (n.Contains("dark") || n.Contains("iron")) r.sharedMaterial = matDarkIron;
                    else if (n.Contains("fire") || n.Contains("ember")) r.sharedMaterial = matFire;
                    else if (n.Contains("wood") || n.Contains("stump") || n.Contains("bellow") || n.Contains("bucket")) r.sharedMaterial = matWood;
                    else
                    {
                        if (r.sharedMaterials != null && r.sharedMaterials.Length >= 5)
                            r.sharedMaterials = new Material[] { matStone, matDarkIron, matBronze, matFire, matWood };
                        else if (r.sharedMaterials != null && r.sharedMaterials.Length >= 4)
                            r.sharedMaterials = new Material[] { matStone, matDarkIron, matFire, matWood };
                        else
                            r.sharedMaterial = matStone;
                    }
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
                return root;
            }
#endif

            // -------------------------------------------------------------
            // 1. STONE HEARTH FOUNDATION (Bệ Móng Đá Hoa Cương Xếp Khối)
            // -------------------------------------------------------------
            CreatePart(PrimitiveType.Cube, "Stone_Foundation_Plinth", root.transform,
                new Vector3(0f, 0.12f, 0f), new Vector3(1.15f, 0.24f, 1.15f), Quaternion.identity, matStone);

            // Stepped hearth apron
            CreatePart(PrimitiveType.Cube, "Hearth_Apron_Sill", root.transform,
                new Vector3(0f, 0.26f, -0.40f), new Vector3(0.95f, 0.08f, 0.35f), Quaternion.identity, matDarkIron);

            // -------------------------------------------------------------
            // 2. ARCHED FIRE CHAMBER WALLS & HOOD (Vòm Lò Khắc Đá & Nón Sắt)
            // -------------------------------------------------------------
            // Left stone jamb
            CreatePart(PrimitiveType.Cube, "Pillar_Left", root.transform,
                new Vector3(-0.38f, 0.60f, -0.15f), new Vector3(0.26f, 0.68f, 0.70f), Quaternion.identity, matStone);
            // Right stone jamb
            CreatePart(PrimitiveType.Cube, "Pillar_Right", root.transform,
                new Vector3(0.38f, 0.60f, -0.15f), new Vector3(0.26f, 0.68f, 0.70f), Quaternion.identity, matStone);
            // Back stone masonry wall
            CreatePart(PrimitiveType.Cube, "Back_Wall", root.transform,
                new Vector3(0f, 0.60f, 0.26f), new Vector3(0.90f, 0.68f, 0.28f), Quaternion.identity, matStone);

            // Arched Iron Lintel Header
            CreatePart(PrimitiveType.Cube, "Arch_Lintel_Iron", root.transform,
                new Vector3(0f, 0.88f, -0.28f), new Vector3(0.85f, 0.12f, 0.22f), Quaternion.identity, matDarkIron);

            // Keystone Embellishment with Bronze Emblem
            CreatePart(PrimitiveType.Cube, "Arch_Keystone", root.transform,
                new Vector3(0f, 0.95f, -0.32f), new Vector3(0.16f, 0.16f, 0.18f), Quaternion.Euler(0f, 0f, 45f), matDarkIron);
            CreatePart(PrimitiveType.Sphere, "Keystone_Emblem", root.transform,
                new Vector3(0f, 0.95f, -0.42f), new Vector3(0.08f, 0.08f, 0.04f), Quaternion.identity, matBronze);

            // -------------------------------------------------------------
            // 3. INTERNAL GLOWING 3D COAL BED & EMBERS (Lòng Than Nóng Đỏ Rực)
            // -------------------------------------------------------------
            CreatePart(PrimitiveType.Cube, "Fire_Chamber_Grate", root.transform,
                new Vector3(0f, 0.32f, -0.05f), new Vector3(0.60f, 0.06f, 0.45f), Quaternion.identity, matDarkIron);

            // Bed of 3D burning coal nuggets (các khối than đá lồi lõm)
            Vector3[] coalOffsets = {
                new Vector3(-0.16f, 0.38f, -0.12f), new Vector3(0.14f, 0.39f, -0.10f),
                new Vector3(0f, 0.41f, -0.05f), new Vector3(-0.12f, 0.39f, 0.08f),
                new Vector3(0.16f, 0.38f, 0.06f), new Vector3(0f, 0.44f, -0.15f)
            };
            for (int c = 0; c < coalOffsets.Length; c++)
            {
                Material m = (c % 2 == 0) ? matFire : matEmber;
                CreatePart(PrimitiveType.Sphere, $"Coal_Nugget_{c}", root.transform,
                    coalOffsets[c], new Vector3(0.16f, 0.11f, 0.15f), Quaternion.Euler(15f * c, 35f * c, 0f), m);
            }

            // Central Blazing Heart (Lõi lửa rực sáng cao nhất)
            CreatePart(PrimitiveType.Sphere, "Blazing_Fire_Heart", root.transform,
                new Vector3(0f, 0.46f, -0.08f), new Vector3(0.28f, 0.18f, 0.24f), Quaternion.identity, matFire);

            // -------------------------------------------------------------
            // 4. HAMMERED IRON SMOKEHOOD & CHIMNEY STACK (Nón Hút & Ống Khói)
            // -------------------------------------------------------------
            // Pyramid/Tapered Smoke Hood (Vòm thu khói bằng sắt)
            CreatePart(PrimitiveType.Cylinder, "Iron_Smoke_Hood", root.transform,
                new Vector3(0f, 1.15f, 0f), new Vector3(0.78f, 0.22f, 0.78f), Quaternion.identity, matDarkIron);

            // 4 Corner Rivet Straps on Hood
            CreatePart(PrimitiveType.Cylinder, "Hood_Rivet_Band", root.transform,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.82f, 0.035f, 0.82f), Quaternion.identity, matDarkIron);

            // Tall Chimney Flue Stack
            CreatePart(PrimitiveType.Cylinder, "Chimney_Flue", root.transform,
                new Vector3(0f, 1.55f, 0.05f), new Vector3(0.38f, 0.38f, 0.38f), Quaternion.identity, matStone);

            // Flue Joint Iron Ring
            CreatePart(PrimitiveType.Cylinder, "Chimney_Band", root.transform,
                new Vector3(0f, 1.50f, 0.05f), new Vector3(0.44f, 0.04f, 0.44f), Quaternion.identity, matDarkIron);

            // Chimney Flue Crown & Spark Cowl (Vành vương miện & nắp che khói)
            CreatePart(PrimitiveType.Cylinder, "Chimney_Crown", root.transform,
                new Vector3(0f, 1.95f, 0.05f), new Vector3(0.48f, 0.05f, 0.48f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cylinder, "Chimney_Cap", root.transform,
                new Vector3(0f, 2.08f, 0.05f), new Vector3(0.55f, 0.025f, 0.55f), Quaternion.identity, matDarkIron);

            // -------------------------------------------------------------
            // 5. AUTHENTIC WORKSHOP ACCESSORIES (Bễ Thổi, Xô Nước, Củi Đun)
            // -------------------------------------------------------------
            // A. Blacksmith Leather Bellows (Bễ thổi da gắn bên sườn phải)
            GameObject goBellows = new GameObject("Bellows_Assembly");
            goBellows.transform.SetParent(root.transform, false);
            goBellows.transform.localPosition = new Vector3(0.62f, 0.58f, -0.05f);
            goBellows.transform.localRotation = Quaternion.Euler(0f, 25f, -12f);

            CreatePart(PrimitiveType.Cube, "Bellows_Plates_Top", goBellows.transform,
                new Vector3(0f, 0.12f, 0f), new Vector3(0.24f, 0.035f, 0.42f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Bellows_Leather_Body", goBellows.transform,
                Vector3.zero, new Vector3(0.22f, 0.18f, 0.38f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Bellows_Plates_Bottom", goBellows.transform,
                new Vector3(0f, -0.12f, 0f), new Vector3(0.24f, 0.035f, 0.42f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cylinder, "Bellows_Nozzle", goBellows.transform,
                new Vector3(-0.16f, 0f, 0f), new Vector3(0.045f, 0.16f, 0.045f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);

            // B. Quench Water Bucket (Xô gỗ tôi thép đặt bên trái)
            GameObject goBucket = new GameObject("Quench_Bucket");
            goBucket.transform.SetParent(root.transform, false);
            goBucket.transform.localPosition = new Vector3(-0.62f, 0.18f, -0.32f);

            CreatePart(PrimitiveType.Cylinder, "Bucket_Wood_Tub", goBucket.transform,
                Vector3.zero, new Vector3(0.26f, 0.16f, 0.26f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cylinder, "Bucket_Iron_Hoop_Top", goBucket.transform,
                new Vector3(0f, 0.10f, 0f), new Vector3(0.28f, 0.02f, 0.28f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cylinder, "Bucket_Water_Surface", goBucket.transform,
                new Vector3(0f, 0.12f, 0f), new Vector3(0.24f, 0.01f, 0.24f), Quaternion.identity, matSteel);

            // C. Firewood Stack (Đống củi xẻ khô xếp cạnh chân bệ)
            for (int w = 0; w < 4; w++)
            {
                CreatePart(PrimitiveType.Cylinder, $"Firewood_Log_{w}", root.transform,
                    new Vector3(0.45f + (w % 2) * 0.10f, 0.06f + (w / 2) * 0.08f, -0.38f),
                    new Vector3(0.07f, 0.22f, 0.07f), Quaternion.Euler(0f, 0f, 90f), matWood);
            }

            return root;
        }

        /// <summary>
        /// Builds the Interactive 3D Smithing Hammer with ergonomic curved wooden shaft,
        /// criss-cross leather wrapping, carved bronze pommel, heavy octagonal sledge head,
        /// and dual bevel-chamfered Damascus steel striking faces.
        /// </summary>
        public static GameObject BuildHeroHammer(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Hammer_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            GameObject pivot = new GameObject("Hammer_Pivot");
            pivot.transform.SetParent(root.transform, false);

            Material matDarkIron = mats["Dark_Iron"];
            Material matSteel = mats["Steel_Polished"];
            Material matWood = mats["Wood_Stump"];
            Material matBronze = mats["Bronze_Brass"];

#if UNITY_EDITOR
            var modelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Forge/Art/Models/Hammer_Dwarven.obj");
            if (modelPrefab != null)
            {
                var inst = Object.Instantiate(modelPrefab, pivot.transform);
                inst.name = "Hammer_Mesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
                var rends = inst.GetComponentsInChildren<Renderer>();
                foreach (var r in rends)
                {
                    string n = r.gameObject.name.ToLowerInvariant();
                    if (n.Contains("wood") || n.Contains("shaft")) r.sharedMaterial = matWood;
                    else if (n.Contains("dark") || n.Contains("iron")) r.sharedMaterial = matDarkIron;
                    else if (n.Contains("steel")) r.sharedMaterial = matSteel;
                    else
                    {
                        if (r.sharedMaterials != null && r.sharedMaterials.Length >= 3)
                            r.sharedMaterials = new Material[] { matWood, matDarkIron, matSteel };
                        else
                            r.sharedMaterial = matDarkIron;
                    }
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
                var hVisualObj = root.AddComponent<HammerVisual3D>();
                hVisualObj.hammerPivot = pivot.transform;
                hVisualObj.restLocalEuler = worldRot.eulerAngles;
                return root;
            }
#endif

            // 1. Shaft (Cán búa gỗ sồi d=0.048, dài 0.58m)
            CreatePart(PrimitiveType.Cylinder, "Shaft_Wood", pivot.transform,
                new Vector3(0f, 0.28f, 0f), new Vector3(0.048f, 0.30f, 0.048f), Quaternion.identity, matWood);

            // 2. Leather Grip Wrap (Lớp quấn da thuộc chống trơn trượt)
            CreatePart(PrimitiveType.Cylinder, "Leather_Grip", pivot.transform,
                new Vector3(0f, 0.12f, 0f), new Vector3(0.056f, 0.11f, 0.056f), Quaternion.identity, matDarkIron);

            // 3. Bronze Pommel & Butt Cap (Chuôi bịt đồng chạm khắc)
            CreatePart(PrimitiveType.Cylinder, "Pommel_Cap", pivot.transform,
                new Vector3(0f, 0.015f, 0f), new Vector3(0.072f, 0.022f, 0.072f), Quaternion.identity, matBronze);
            CreatePart(PrimitiveType.Sphere, "Pommel_Ball", pivot.transform,
                new Vector3(0f, 0.005f, 0f), new Vector3(0.045f, 0.045f, 0.045f), Quaternion.identity, matBronze);

            // 4. Reinforced Metal Collar below hammerhead
            CreatePart(PrimitiveType.Cylinder, "Head_Collar", pivot.transform,
                new Vector3(0f, 0.52f, 0f), new Vector3(0.070f, 0.04f, 0.070f), Quaternion.identity, matDarkIron);

            // 5. Heavy Octagonal Sledge Hammer Head Block (Khối đầu búa chính)
            CreatePart(PrimitiveType.Cube, "Hammer_Head_Block", pivot.transform,
                new Vector3(0f, 0.58f, 0f), new Vector3(0.26f, 0.12f, 0.12f), Quaternion.identity, matDarkIron);

            // Bronze accent bands around hammer eye
            CreatePart(PrimitiveType.Cube, "Head_Accent_Band", pivot.transform,
                new Vector3(0f, 0.58f, 0f), new Vector3(0.12f, 0.128f, 0.128f), Quaternion.identity, matBronze);

            // 6. Dual Beveled Damascus Steel Striking Faces (Hai mặt đập thép tôi)
            CreatePart(PrimitiveType.Cylinder, "Face_Front", pivot.transform,
                new Vector3(0.138f, 0.58f, 0f), new Vector3(0.11f, 0.022f, 0.11f), Quaternion.Euler(0f, 0f, 90f), matSteel);
            CreatePart(PrimitiveType.Cylinder, "Face_Back", pivot.transform,
                new Vector3(-0.138f, 0.58f, 0f), new Vector3(0.095f, 0.022f, 0.095f), Quaternion.Euler(0f, 0f, 90f), matSteel);

            // 7. Top Eye Securing Wedge
            CreatePart(PrimitiveType.Cube, "Top_Eye_Wedge", pivot.transform,
                new Vector3(0f, 0.655f, 0f), new Vector3(0.04f, 0.035f, 0.04f), Quaternion.identity, matSteel);

            var hVisual = root.AddComponent<HammerVisual3D>();
            hVisual.hammerPivot = pivot.transform;
            hVisual.restLocalEuler = worldRot.eulerAngles;

            return root;
        }

        /// <summary>
        /// Builds the 3D Polished Workshop Materials & Tools on the workbench:
        /// 1. Stack of 3 mirror-polished Damascus steel ingots with stamped hallmarks
        /// 2. Stack of 2 gleaming gold bullion ingots
        /// 3. Faceted glowing crystal ore cluster on matrix stone
        /// 4. Finished craftsman dagger with brass crossguard & pommel on leather craft mat
        /// 5. Blacksmith tongs clamping a fresh glowing hot metal billet
        /// </summary>
        public static GameObject BuildHeroDeskProps(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Desk_Props_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matSteel = mats["Steel_Polished"];
            Material matBronze = mats["Bronze_Brass"];
            Material matDarkIron = mats["Dark_Iron"];
            Material matCrystal = mats["Ore_Crystal"];
            Material matHotIngot = mats["Hot_Ingot"];
            Material matWood = mats["Wood_Stump"];
            Material matLeather = mats.ContainsKey("Leather") ? mats["Leather"] : matWood;

#if UNITY_EDITOR
            var modelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Forge/Art/Models/Desk_Materials.obj");
            if (modelPrefab != null)
            {
                var inst = Object.Instantiate(modelPrefab, root.transform);
                inst.name = "Desk_Materials_Mesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = new Vector3(1.35f, 1.35f, 1.35f);
                var rends = inst.GetComponentsInChildren<Renderer>();
                foreach (var r in rends)
                {
                    string n = r.gameObject.name.ToLowerInvariant();
                    if (n.Contains("steel")) r.sharedMaterial = matSteel;
                    else if (n.Contains("bronze") || n.Contains("brass") || n.Contains("gold")) r.sharedMaterial = matBronze;
                    else if (n.Contains("crystal") || n.Contains("ore")) r.sharedMaterial = matCrystal;
                    else if (n.Contains("hot") || n.Contains("ingot")) r.sharedMaterial = matHotIngot;
                    else if (n.Contains("wood") || n.Contains("leather") || n.Contains("stump")) r.sharedMaterial = matLeather;
                    else if (n.Contains("dark") || n.Contains("iron")) r.sharedMaterial = matDarkIron;
                    else
                    {
                        if (r.sharedMaterials != null && r.sharedMaterials.Length >= 6)
                            r.sharedMaterials = new Material[] { matSteel, matBronze, matLeather, matCrystal, matDarkIron, matHotIngot };
                        else
                            r.sharedMaterial = matSteel;
                    }
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
                return root;
            }
#endif
            return root;
        }

        /// <summary>
        /// Builds unique, richly textured 3D mechanical models for all 5 automation tiers (G1..G5),
        /// and wires up the MachineAnimator3D component for realistic real-time animated operations.
        /// </summary>
        public static GameObject BuildMachine3D(string genId, Transform parent, Vector3 localPos, Quaternion localRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject($"Machine_{genId}_Model");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot;

            Material matWood = mats["Wood_Stump"];
            Material matDarkIron = mats["Dark_Iron"];
            Material matSteel = mats["Steel_Polished"];
            Material matBronze = mats["Bronze_Brass"];
            Material matFire = mats["Furnace_Fire"];
            Material matCrystal = mats["Ore_Crystal"];

            var anim = root.AddComponent<MachineAnimator3D>();

            switch (genId)
            {
                case "G1": // Tay Búa Tí Hon (Mechanical Clockwork Auto-Hammer)
                    anim.machineType = MachineAnimator3D.MachineType.G1_Hammer;

                    // Base Pedestal
                    CreatePart(PrimitiveType.Cylinder, "Pedestal", root.transform,
                        new Vector3(0f, 0.15f, 0f), new Vector3(0.38f, 0.15f, 0.38f), Quaternion.identity, matWood);
                    CreatePart(PrimitiveType.Cube, "Post", root.transform,
                        new Vector3(-0.08f, 0.40f, 0f), new Vector3(0.12f, 0.45f, 0.12f), Quaternion.identity, matDarkIron);

                    // Clockwork Cam Gear
                    CreatePart(PrimitiveType.Cylinder, "Cam_Gear", root.transform,
                        new Vector3(-0.08f, 0.48f, 0.09f), new Vector3(0.16f, 0.03f, 0.16f), Quaternion.Euler(90f, 0f, 0f), matBronze);

                    // Miniature Anvil Base underneath
                    CreatePart(PrimitiveType.Cube, "Mini_Anvil", root.transform,
                        new Vector3(0.10f, 0.34f, 0f), new Vector3(0.16f, 0.12f, 0.14f), Quaternion.identity, matSteel);

                    // Reciprocating Hammer Arm
                    GameObject armG1 = new GameObject("Hammer_Arm");
                    armG1.transform.SetParent(root.transform, false);
                    armG1.transform.localPosition = new Vector3(-0.08f, 0.58f, 0f);

                    CreatePart(PrimitiveType.Cube, "Rocker_Bar", armG1.transform,
                        new Vector3(0.12f, 0f, 0f), new Vector3(0.24f, 0.035f, 0.045f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Mini_Head", armG1.transform,
                        new Vector3(0.22f, -0.06f, 0f), new Vector3(0.075f, 0.09f, 0.065f), Quaternion.identity, matSteel);

                    anim.g1HammerArm = armG1.transform;
                    break;

                case "G2": // Xe Khoan Quặng (Ore Mining Drill Rig)
                    anim.machineType = MachineAnimator3D.MachineType.G2_Drill;

                    // Heavy Timber Mine Cart Chassis
                    CreatePart(PrimitiveType.Cube, "Cart_Body", root.transform,
                        new Vector3(0f, 0.18f, 0f), new Vector3(0.48f, 0.16f, 0.38f), Quaternion.identity, matWood);

                    // 4 Flanged Iron Wheels
                    Vector3[] wheelPos = { new Vector3(-0.20f, 0.08f, 0.18f), new Vector3(0.20f, 0.08f, 0.18f),
                                           new Vector3(-0.20f, 0.08f, -0.18f), new Vector3(0.20f, 0.08f, -0.18f) };
                    for (int w = 0; w < 4; w++)
                    {
                        CreatePart(PrimitiveType.Cylinder, $"Wheel_{w}", root.transform,
                            wheelPos[w], new Vector3(0.14f, 0.035f, 0.14f), Quaternion.Euler(90f, 0f, 0f), matDarkIron);
                    }

                    // A-Frame Drill Rigging Mast
                    CreatePart(PrimitiveType.Cube, "Drill_Mast_L", root.transform,
                        new Vector3(-0.12f, 0.44f, 0f), new Vector3(0.045f, 0.42f, 0.045f), Quaternion.Euler(0f, 0f, -12f), matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Drill_Mast_R", root.transform,
                        new Vector3(0.12f, 0.44f, 0f), new Vector3(0.045f, 0.42f, 0.045f), Quaternion.Euler(0f, 0f, 12f), matDarkIron);

                    // Drill Assembly with Rotating Bit
                    GameObject drillAssembly = new GameObject("Drill_Assembly");
                    drillAssembly.transform.SetParent(root.transform, false);
                    drillAssembly.transform.localPosition = new Vector3(0f, 0.52f, 0f);

                    CreatePart(PrimitiveType.Cylinder, "Drill_Motor", drillAssembly.transform,
                        new Vector3(0f, 0.08f, 0f), new Vector3(0.12f, 0.10f, 0.12f), Quaternion.identity, matBronze);

                    GameObject drillBit = new GameObject("Drill_Bit");
                    drillBit.transform.SetParent(drillAssembly.transform, false);
                    drillBit.transform.localPosition = new Vector3(0f, -0.12f, 0f);

                    CreatePart(PrimitiveType.Cylinder, "Spiral_Shaft", drillBit.transform,
                        Vector3.zero, new Vector3(0.045f, 0.18f, 0.045f), Quaternion.identity, matSteel);
                    CreatePart(PrimitiveType.Cylinder, "Drill_Cone", drillBit.transform,
                        new Vector3(0f, -0.16f, 0f), new Vector3(0.10f, 0.08f, 0.10f), Quaternion.identity, matSteel);

                    anim.g2DrillBit = drillBit.transform;
                    break;

                case "G3": // Lò Luyện Kim Tự Đun (Crucible Smelter)
                    anim.machineType = MachineAnimator3D.MachineType.G3_Smelter;

                    // Heavy Iron Tripod Legs
                    for (int leg = 0; leg < 3; leg++)
                    {
                        float ang = leg * 120f * Mathf.Deg2Rad;
                        Vector3 lPos = new Vector3(Mathf.Cos(ang) * 0.22f, 0.18f, Mathf.Sin(ang) * 0.22f);
                        CreatePart(PrimitiveType.Cylinder, $"Leg_{leg}", root.transform,
                            lPos, new Vector3(0.045f, 0.20f, 0.045f), Quaternion.Euler(Mathf.Sin(ang) * 15f, 0f, -Mathf.Cos(ang) * 15f), matDarkIron);
                    }

                    // Boiling Crucible Vat Pot
                    GameObject pot = new GameObject("Crucible_Pot");
                    pot.transform.SetParent(root.transform, false);
                    pot.transform.localPosition = new Vector3(0f, 0.36f, 0f);

                    CreatePart(PrimitiveType.Cylinder, "Vat_Belly", pot.transform,
                        Vector3.zero, new Vector3(0.40f, 0.18f, 0.40f), Quaternion.identity, matBronze);
                    CreatePart(PrimitiveType.Cylinder, "Molten_Surface", pot.transform,
                        new Vector3(0f, 0.17f, 0f), new Vector3(0.34f, 0.02f, 0.34f), Quaternion.identity, matFire);

                    // Steam Vent Cap
                    GameObject ventCap = new GameObject("Steam_Vent_Cap");
                    ventCap.transform.SetParent(root.transform, false);
                    ventCap.transform.localPosition = new Vector3(0f, 0.46f, 0f);

                    CreatePart(PrimitiveType.Cylinder, "Vent_Pipe", ventCap.transform,
                        Vector3.zero, new Vector3(0.09f, 0.08f, 0.09f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Sphere, "Vent_Ball_Valve", ventCap.transform,
                        new Vector3(0f, 0.08f, 0f), new Vector3(0.06f, 0.06f, 0.06f), Quaternion.identity, matBronze);

                    anim.g3CrucibleVat = pot.transform;
                    anim.g3SteamVent = ventCap.transform;
                    break;

                case "G4": // Cối Xay Hơi Nước (Steam Mill & Heavy Flywheel)
                    anim.machineType = MachineAnimator3D.MachineType.G4_SteamEngine;

                    // Cast-Iron Engine Bed Plate
                    CreatePart(PrimitiveType.Cube, "Engine_Bed", root.transform,
                        new Vector3(0f, 0.06f, 0f), new Vector3(0.56f, 0.08f, 0.42f), Quaternion.identity, matDarkIron);

                    // Large Brass Spoked Flywheel Gear
                    GameObject flywheel = new GameObject("Flywheel_Gear");
                    flywheel.transform.SetParent(root.transform, false);
                    flywheel.transform.localPosition = new Vector3(-0.16f, 0.34f, 0f);

                    CreatePart(PrimitiveType.Cylinder, "Flywheel_Rim", flywheel.transform,
                        Vector3.zero, new Vector3(0.44f, 0.045f, 0.44f), Quaternion.Euler(0f, 0f, 90f), matBronze);
                    CreatePart(PrimitiveType.Cube, "Spoke_1", flywheel.transform,
                        Vector3.zero, new Vector3(0.035f, 0.40f, 0.035f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Spoke_2", flywheel.transform,
                        Vector3.zero, new Vector3(0.035f, 0.035f, 0.40f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);

                    // Steam Cylinder & Reciprocating Piston
                    CreatePart(PrimitiveType.Cylinder, "Steam_Cylinder", root.transform,
                        new Vector3(0.14f, 0.32f, 0f), new Vector3(0.18f, 0.16f, 0.18f), Quaternion.Euler(0f, 0f, 90f), matSteel);

                    GameObject pistonAssembly = new GameObject("Piston_Assembly");
                    pistonAssembly.transform.SetParent(root.transform, false);
                    pistonAssembly.transform.localPosition = new Vector3(0.02f, 0.32f, 0f);

                    GameObject pistonRod = new GameObject("Piston_Rod");
                    pistonRod.transform.SetParent(pistonAssembly.transform, false);
                    CreatePart(PrimitiveType.Cylinder, "Rod_Shaft", pistonRod.transform,
                        Vector3.zero, new Vector3(0.045f, 0.14f, 0.045f), Quaternion.Euler(0f, 0f, 90f), matSteel);

                    // Pressure Gauge
                    CreatePart(PrimitiveType.Cylinder, "Pressure_Gauge", root.transform,
                        new Vector3(0.14f, 0.52f, 0.06f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.Euler(90f, 0f, 0f), matBronze);

                    anim.g4Flywheel = flywheel.transform;
                    anim.g4Piston = pistonRod.transform;
                    break;

                case "G5": // Golem Thợ Rèn Tự Hành (Master Automaton Golem)
                    anim.machineType = MachineAnimator3D.MachineType.G5_Golem;

                    // Heavy Armored Stilt Legs
                    CreatePart(PrimitiveType.Cube, "Leg_L", root.transform,
                        new Vector3(-0.14f, 0.20f, 0f), new Vector3(0.11f, 0.38f, 0.11f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Leg_R", root.transform,
                        new Vector3(0.14f, 0.20f, 0f), new Vector3(0.11f, 0.38f, 0.11f), Quaternion.identity, matDarkIron);

                    // Articulated Torso
                    GameObject torso = new GameObject("Torso");
                    torso.transform.SetParent(root.transform, false);
                    torso.transform.localPosition = new Vector3(0f, 0.48f, 0f);

                    CreatePart(PrimitiveType.Cube, "Chest_Armor", torso.transform,
                        new Vector3(0f, 0.12f, 0f), new Vector3(0.38f, 0.24f, 0.26f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Shoulder_Pauldrons", torso.transform,
                        new Vector3(0f, 0.22f, 0f), new Vector3(0.48f, 0.08f, 0.28f), Quaternion.identity, matBronze);

                    // Glowing Arcane Power Core in Chest
                    GameObject core = new GameObject("Magic_Core");
                    core.transform.SetParent(torso.transform, false);
                    core.transform.localPosition = new Vector3(0f, 0.12f, -0.14f);
                    CreatePart(PrimitiveType.Sphere, "Arcane_Gem", core.transform,
                        Vector3.zero, new Vector3(0.09f, 0.09f, 0.07f), Quaternion.identity, matCrystal);

                    // Helmet with Visor Slit
                    CreatePart(PrimitiveType.Cube, "Golem_Head", torso.transform,
                        new Vector3(0f, 0.32f, 0f), new Vector3(0.18f, 0.16f, 0.18f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Visor_Slit", torso.transform,
                        new Vector3(0f, 0.32f, -0.10f), new Vector3(0.12f, 0.035f, 0.02f), Quaternion.identity, matFire);

                    // Right Arm (Holding Sledgehammer)
                    GameObject armR = new GameObject("Arm_Right");
                    armR.transform.SetParent(torso.transform, false);
                    armR.transform.localPosition = new Vector3(0.24f, 0.18f, 0f);

                    CreatePart(PrimitiveType.Cube, "Limb_R", armR.transform,
                        new Vector3(0f, -0.14f, 0f), new Vector3(0.08f, 0.26f, 0.08f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cylinder, "Golem_Hammer_Handle", armR.transform,
                        new Vector3(0f, -0.26f, 0.10f), new Vector3(0.025f, 0.28f, 0.025f), Quaternion.Euler(90f, 0f, 0f), matWood);
                    CreatePart(PrimitiveType.Cube, "Golem_Hammer_Head", armR.transform,
                        new Vector3(0f, -0.26f, 0.24f), new Vector3(0.09f, 0.09f, 0.09f), Quaternion.identity, matSteel);

                    // Left Arm (Holding Smithing Tongs)
                    GameObject armL = new GameObject("Arm_Left");
                    armL.transform.SetParent(torso.transform, false);
                    armL.transform.localPosition = new Vector3(-0.24f, 0.18f, 0f);

                    CreatePart(PrimitiveType.Cube, "Limb_L", armL.transform,
                        new Vector3(0f, -0.14f, 0f), new Vector3(0.08f, 0.26f, 0.08f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Tongs_Jaw", armL.transform,
                        new Vector3(0f, -0.28f, -0.08f), new Vector3(0.04f, 0.04f, 0.16f), Quaternion.identity, matBronze);

                    anim.g5Torso = torso.transform;
                    anim.g5RightArm = armR.transform;
                    anim.g5LeftArm = armL.transform;
                    anim.g5Core = core.transform;
                    break;
            }

            return root;
        }

        /// <summary>
        /// Builds 3D Shelving unit holding glistening faceted crystal ore clusters.
        /// </summary>
        public static GameObject BuildOreShelf3D(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Decor_OreShelf_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matWood = mats["Wood_Stump"];
            Material matCrystal = mats["Ore_Crystal"];
            Material matBronze = mats["Bronze_Brass"];
            Material matFire = mats["Furnace_Fire"];

            // Upright Shelving Frame
            CreatePart(PrimitiveType.Cube, "Shelf_Post_L", root.transform,
                new Vector3(-0.35f, 0.45f, 0f), new Vector3(0.06f, 0.90f, 0.32f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Shelf_Post_R", root.transform,
                new Vector3(0.35f, 0.45f, 0f), new Vector3(0.06f, 0.90f, 0.32f), Quaternion.identity, matWood);

            // 3 Horizontal Shelves
            CreatePart(PrimitiveType.Cube, "Shelf_Tier_Bottom", root.transform,
                new Vector3(0f, 0.15f, 0f), new Vector3(0.74f, 0.04f, 0.30f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Shelf_Tier_Mid", root.transform,
                new Vector3(0f, 0.50f, 0f), new Vector3(0.74f, 0.04f, 0.30f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Shelf_Tier_Top", root.transform,
                new Vector3(0f, 0.85f, 0f), new Vector3(0.74f, 0.04f, 0.30f), Quaternion.identity, matWood);

            // Glowing Crystal Geodes & Ore Ingot Piles on tiers
            CreatePart(PrimitiveType.Sphere, "Cyan_Geode_1", root.transform,
                new Vector3(-0.18f, 0.58f, 0f), new Vector3(0.14f, 0.18f, 0.14f), Quaternion.Euler(15f, 25f, 0f), matCrystal);
            CreatePart(PrimitiveType.Sphere, "Cyan_Geode_2", root.transform,
                new Vector3(-0.06f, 0.56f, 0.04f), new Vector3(0.10f, 0.12f, 0.10f), Quaternion.Euler(-10f, 40f, 0f), matCrystal);

            CreatePart(PrimitiveType.Cube, "Gold_Ore_Stack", root.transform,
                new Vector3(0.18f, 0.55f, 0f), new Vector3(0.16f, 0.08f, 0.12f), Quaternion.identity, matBronze);

            CreatePart(PrimitiveType.Sphere, "Ruby_Core_Bottom", root.transform,
                new Vector3(0.12f, 0.22f, 0f), new Vector3(0.15f, 0.14f, 0.15f), Quaternion.identity, matFire);

            return root;
        }

        /// <summary>
        /// Builds 3D Dwarven Treasure Chest with open lid and gleaming gold ingots.
        /// </summary>
        public static GameObject BuildChest3D(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Decor_Chest_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matWood = mats["Wood_Stump"];
            Material matDarkIron = mats["Dark_Iron"];
            Material matBronze = mats["Bronze_Brass"];

            // Chest Box Base
            CreatePart(PrimitiveType.Cube, "Chest_Box", root.transform,
                new Vector3(0f, 0.16f, 0f), new Vector3(0.55f, 0.28f, 0.36f), Quaternion.identity, matWood);

            // Iron Corner Reinforcements
            CreatePart(PrimitiveType.Cube, "Band_L", root.transform,
                new Vector3(-0.20f, 0.16f, 0f), new Vector3(0.04f, 0.29f, 0.37f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cube, "Band_R", root.transform,
                new Vector3(0.20f, 0.16f, 0f), new Vector3(0.04f, 0.29f, 0.37f), Quaternion.identity, matDarkIron);

            // Open Tilted Lid
            GameObject lid = new GameObject("Chest_Lid");
            lid.transform.SetParent(root.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.30f, 0.16f);
            lid.transform.localRotation = Quaternion.Euler(-55f, 0f, 0f);

            CreatePart(PrimitiveType.Cylinder, "Lid_Arch", lid.transform,
                new Vector3(0f, 0.06f, -0.16f), new Vector3(0.34f, 0.27f, 0.34f), Quaternion.Euler(0f, 0f, 90f), matWood);
            CreatePart(PrimitiveType.Cube, "Lid_Lock_Hasp", lid.transform,
                new Vector3(0f, 0.02f, -0.32f), new Vector3(0.06f, 0.06f, 0.035f), Quaternion.identity, matBronze);

            // Gleaming Gold Ingots Inside
            Vector3[] barPositions = {
                new Vector3(-0.10f, 0.28f, -0.04f), new Vector3(0.10f, 0.28f, -0.04f),
                new Vector3(0f, 0.32f, -0.02f), new Vector3(-0.06f, 0.28f, 0.06f)
            };
            for (int b = 0; b < barPositions.Length; b++)
            {
                CreatePart(PrimitiveType.Cube, $"Gold_Ingot_{b}", root.transform,
                    barPositions[b], new Vector3(0.14f, 0.045f, 0.07f), Quaternion.Euler(0f, 18f * b, 0f), matBronze);
            }

            return root;
        }
    }
}
