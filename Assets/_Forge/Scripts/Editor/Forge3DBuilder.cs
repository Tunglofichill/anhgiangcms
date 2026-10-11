using System.Collections.Generic;
using UnityEngine;

namespace Forge.EditorTools
{
    public static class Forge3DBuilder
    {
        private static GameObject CreatePart(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
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
        /// Builds a majestic 3D Dwarven Blacksmith Anvil with solid oak stump base, flared cast-iron feet,
        /// polished forged steel table, horn, heel, and glowing red-hot iron ingot on top.
        /// </summary>
        public static GameObject BuildHeroAnvil(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Anvil_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matDarkIron = mats.ContainsKey("Dark_Iron") ? mats["Dark_Iron"] : mats["Anvil"];
            Material matSteel = mats.ContainsKey("Steel_Polished") ? mats["Steel_Polished"] : matDarkIron;
            Material matWood = mats.ContainsKey("Wood_Stump") ? mats["Wood_Stump"] : mats["Desk"];
            Material matIngot = mats.ContainsKey("Hot_Ingot") ? mats["Hot_Ingot"] : mats["FurnaceEmber"];

            // 1. Solid Oak Tree Stump Base (Cylinder: d=0.70, h=0.48)
            CreatePart(PrimitiveType.Cylinder, "Wood_Stump", root.transform,
                new Vector3(0f, 0.24f, 0f), new Vector3(0.70f, 0.24f, 0.70f), Quaternion.identity, matWood);

            // Iron Reinforcement Strap on Stump
            CreatePart(PrimitiveType.Cylinder, "Stump_Iron_Band_Top", root.transform,
                new Vector3(0f, 0.40f, 0f), new Vector3(0.72f, 0.035f, 0.72f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cylinder, "Stump_Iron_Band_Bottom", root.transform,
                new Vector3(0f, 0.10f, 0f), new Vector3(0.73f, 0.035f, 0.73f), Quaternion.identity, matDarkIron);

            // 2. Anvil Base Flange (Flared feet: 0.50 x 0.08 x 0.32)
            CreatePart(PrimitiveType.Cube, "Anvil_Base_Foot", root.transform,
                new Vector3(0f, 0.52f, 0f), new Vector3(0.50f, 0.08f, 0.32f), Quaternion.identity, matDarkIron);

            // Anvil Base Arch Cutout (Cylinder carving underneath)
            CreatePart(PrimitiveType.Cube, "Anvil_Base_Riser", root.transform,
                new Vector3(0f, 0.58f, 0f), new Vector3(0.42f, 0.06f, 0.28f), Quaternion.identity, matDarkIron);

            // 3. Anvil Waist / Stem (Thon gọn ở giữa)
            CreatePart(PrimitiveType.Cube, "Anvil_Waist", root.transform,
                new Vector3(0.02f, 0.69f, 0f), new Vector3(0.24f, 0.18f, 0.22f), Quaternion.identity, matDarkIron);

            // 4. Anvil Upper Body & Stepped Table
            CreatePart(PrimitiveType.Cube, "Anvil_Upper_Body", root.transform,
                new Vector3(0.04f, 0.81f, 0f), new Vector3(0.44f, 0.10f, 0.24f), Quaternion.identity, matDarkIron);

            // Polished Steel Face / Table (Mặt đe rèn bóng loáng)
            CreatePart(PrimitiveType.Cube, "Anvil_Face_Table", root.transform,
                new Vector3(0.06f, 0.87f, 0f), new Vector3(0.38f, 0.04f, 0.22f), Quaternion.identity, matSteel);

            // 5. Anvil Horn (Mũi đe hình nón nhọn vươn ra bên trái: x < 0)
            CreatePart(PrimitiveType.Cylinder, "Anvil_Horn", root.transform,
                new Vector3(-0.24f, 0.83f, 0f), new Vector3(0.18f, 0.16f, 0.18f), Quaternion.Euler(0f, 0f, 75f), matSteel);
            CreatePart(PrimitiveType.Cylinder, "Anvil_Horn_Tip", root.transform,
                new Vector3(-0.36f, 0.86f, 0f), new Vector3(0.10f, 0.10f, 0.10f), Quaternion.Euler(0f, 0f, 82f), matSteel);

            // 6. Anvil Heel (Gót đe vuông vươn ra bên phải: x > 0)
            CreatePart(PrimitiveType.Cube, "Anvil_Heel", root.transform,
                new Vector3(0.28f, 0.83f, 0f), new Vector3(0.16f, 0.10f, 0.20f), Quaternion.identity, matDarkIron);

            // Hardy Hole Detail on Heel (Lỗ đục vuông)
            CreatePart(PrimitiveType.Cube, "Hardy_Hole_Detail", root.transform,
                new Vector3(0.28f, 0.885f, 0f), new Vector3(0.045f, 0.015f, 0.045f), Quaternion.identity, matDarkIron);

            // 7. Red-Hot Glowing Ore Ingot (Thỏi quặng nung đỏ rực nằm sẵn trên mặt đe!)
            CreatePart(PrimitiveType.Cube, "Glowing_Hot_Ingot", root.transform,
                new Vector3(0.02f, 0.91f, 0f), new Vector3(0.18f, 0.045f, 0.09f), Quaternion.Euler(0f, -8f, 0f), matIngot);

            // Contact Shadow for Anvil
            CreatePart(PrimitiveType.Quad, "Anvil_Contact_Shadow", root.transform,
                new Vector3(0f, 0.01f, 0f), new Vector3(0.85f, 0.85f, 1f), Quaternion.Euler(90f, 0f, 0f), mats["Floor"]);

            return root;
        }

        /// <summary>
        /// Builds an impressive 3D Dwarven Stone Masonry Furnace with arched combustion chamber,
        /// glowing internal ember bed, heavy iron bands, and chimney flue stack.
        /// </summary>
        public static GameObject BuildHeroFurnace(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Furnace_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            Material matStone = mats.ContainsKey("Stone_Furnace") ? mats["Stone_Furnace"] : mats["Floor"];
            Material matDarkIron = mats.ContainsKey("Dark_Iron") ? mats["Dark_Iron"] : mats["Anvil"];
            Material matFire = mats.ContainsKey("Furnace_Fire") ? mats["Furnace_Fire"] : mats["FurnaceEmber"];
            Material matEmber = mats["FurnaceEmber"];

            // 1. Stone Hearth Base (Cylinder: d=1.05, h=0.25)
            CreatePart(PrimitiveType.Cylinder, "Furnace_Base_Stone", root.transform,
                new Vector3(0f, 0.125f, 0f), new Vector3(1.05f, 0.125f, 1.05f), Quaternion.identity, matStone);

            // 2. Furnace Combustion Body (Vòm thân lò: d=0.92, h=0.75)
            CreatePart(PrimitiveType.Cylinder, "Furnace_Body_Lower", root.transform,
                new Vector3(0f, 0.55f, 0f), new Vector3(0.92f, 0.32f, 0.92f), Quaternion.identity, matStone);

            // Iron Reinforcing Bands around furnace belly
            CreatePart(PrimitiveType.Cylinder, "Furnace_Iron_Strap_Mid", root.transform,
                new Vector3(0f, 0.42f, 0f), new Vector3(0.95f, 0.035f, 0.95f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cylinder, "Furnace_Iron_Strap_Upper", root.transform,
                new Vector3(0f, 0.72f, 0f), new Vector3(0.88f, 0.035f, 0.88f), Quaternion.identity, matDarkIron);

            // 3. Arched Fire Chamber Opening (Cửa lò hình vòm khoét sâu vào lòng lò)
            // Left jamb
            CreatePart(PrimitiveType.Cube, "Arch_Pillar_L", root.transform,
                new Vector3(-0.24f, 0.48f, -0.38f), new Vector3(0.10f, 0.36f, 0.14f), Quaternion.identity, matDarkIron);
            // Right jamb
            CreatePart(PrimitiveType.Cube, "Arch_Pillar_R", root.transform,
                new Vector3(0.24f, 0.48f, -0.38f), new Vector3(0.10f, 0.36f, 0.14f), Quaternion.identity, matDarkIron);
            // Arch lintel (Top)
            CreatePart(PrimitiveType.Cube, "Arch_Lintel", root.transform,
                new Vector3(0f, 0.68f, -0.38f), new Vector3(0.52f, 0.10f, 0.14f), Quaternion.identity, matDarkIron);
            // Keystone / Embellishment
            CreatePart(PrimitiveType.Cube, "Arch_Keystone", root.transform,
                new Vector3(0f, 0.74f, -0.40f), new Vector3(0.12f, 0.12f, 0.15f), Quaternion.Euler(0f, 0f, 45f), matDarkIron);

            // Grate Iron Hearth Sill
            CreatePart(PrimitiveType.Cube, "Hearth_Iron_Sill", root.transform,
                new Vector3(0f, 0.28f, -0.36f), new Vector3(0.56f, 0.06f, 0.18f), Quaternion.identity, matDarkIron);

            // 4. Burning Hearth Interior (Lớp than hồng rực lửa bên trong lòng lò)
            CreatePart(PrimitiveType.Cube, "Internal_Fire_Cavity", root.transform,
                new Vector3(0f, 0.45f, -0.15f), new Vector3(0.42f, 0.26f, 0.35f), Quaternion.identity, matEmber);

            // Glowing Fire Core / Embers Bed (Cực sáng, chiếu ánh lửa cam đỏ)
            CreatePart(PrimitiveType.Cube, "Glowing_Fire_Core", root.transform,
                new Vector3(0f, 0.42f, -0.22f), new Vector3(0.36f, 0.16f, 0.24f), Quaternion.identity, matFire);

            // 5. Tapered Furnace Dome & Chimney Stack (Nón thu khói và ống khói vươn cao)
            CreatePart(PrimitiveType.Cylinder, "Furnace_Dome_Taper", root.transform,
                new Vector3(0f, 0.98f, 0f), new Vector3(0.72f, 0.16f, 0.72f), Quaternion.identity, matStone);
            CreatePart(PrimitiveType.Cylinder, "Furnace_Chimney_Flue", root.transform,
                new Vector3(0f, 1.35f, 0f), new Vector3(0.42f, 0.30f, 0.42f), Quaternion.identity, matStone);

            // Chimney Flue Iron Crown (Vành đai sắt miệng ống khói)
            CreatePart(PrimitiveType.Cylinder, "Chimney_Iron_Crown", root.transform,
                new Vector3(0f, 1.62f, 0f), new Vector3(0.48f, 0.05f, 0.48f), Quaternion.identity, matDarkIron);

            // Contact Shadow for Furnace
            CreatePart(PrimitiveType.Quad, "Furnace_Contact_Shadow", root.transform,
                new Vector3(0f, 0.01f, 0f), new Vector3(1.2f, 1.2f, 1f), Quaternion.Euler(90f, 0f, 0f), mats["Floor"]);

            return root;
        }

        /// <summary>
        /// Builds a majestic 3D Smith Hammer with wooden handle, grip wrap,
        /// heavy octagonal steel hammerhead, ready for interactive strike animations.
        /// </summary>
        public static GameObject BuildHeroHammer(Transform parent, Vector3 worldPos, Quaternion worldRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Hammer_3D");
            root.transform.SetParent(parent, false);
            root.transform.position = worldPos;
            root.transform.rotation = worldRot;

            GameObject pivot = new GameObject("Hammer_Pivot");
            pivot.transform.SetParent(root.transform, false);

            Material matDarkIron = mats.ContainsKey("Dark_Iron") ? mats["Dark_Iron"] : mats["Anvil"];
            Material matSteel = mats.ContainsKey("Steel_Polished") ? mats["Steel_Polished"] : matDarkIron;
            Material matWood = mats.ContainsKey("Wood_Stump") ? mats["Wood_Stump"] : mats["Desk"];

            // Handle (Cán gỗ sồi dài 0.55m, hướng lên)
            CreatePart(PrimitiveType.Cylinder, "Handle_Shaft", pivot.transform,
                new Vector3(0f, 0.26f, 0f), new Vector3(0.045f, 0.28f, 0.045f), Quaternion.identity, matWood);

            // Grip wrapping on base of handle
            CreatePart(PrimitiveType.Cylinder, "Handle_Grip_Wrap", pivot.transform,
                new Vector3(0f, 0.10f, 0f), new Vector3(0.052f, 0.09f, 0.052f), Quaternion.identity, matDarkIron);

            // Handle Butt Cap (Đế cán búa)
            CreatePart(PrimitiveType.Cylinder, "Handle_Pommel", pivot.transform,
                new Vector3(0f, 0.015f, 0f), new Vector3(0.065f, 0.018f, 0.065f), Quaternion.identity, matDarkIron);

            // Metal Collar below head
            CreatePart(PrimitiveType.Cylinder, "Hammer_Collar", pivot.transform,
                new Vector3(0f, 0.50f, 0f), new Vector3(0.065f, 0.035f, 0.065f), Quaternion.identity, matDarkIron);

            // Heavy Octagonal Sledge Hammer Head (Khối đầu búa chính)
            CreatePart(PrimitiveType.Cube, "Hammer_Head_Block", pivot.transform,
                new Vector3(0f, 0.56f, 0f), new Vector3(0.24f, 0.11f, 0.11f), Quaternion.identity, matDarkIron);

            // Striking Faces (Mặt đập thép vát mép bên trái & bên phải)
            CreatePart(PrimitiveType.Cylinder, "Striking_Face_Front", pivot.transform,
                new Vector3(0.125f, 0.56f, 0f), new Vector3(0.10f, 0.02f, 0.10f), Quaternion.Euler(0f, 0f, 90f), matSteel);
            CreatePart(PrimitiveType.Cylinder, "Striking_Face_Back", pivot.transform,
                new Vector3(-0.125f, 0.56f, 0f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.Euler(0f, 0f, 90f), matSteel);

            // Fastening Wedge on Top of Eye
            CreatePart(PrimitiveType.Cube, "Head_Top_Wedge", pivot.transform,
                new Vector3(0f, 0.63f, 0f), new Vector3(0.035f, 0.035f, 0.035f), Quaternion.identity, matSteel);

            var hVisual = root.AddComponent<HammerVisual3D>();
            hVisual.hammerPivot = pivot.transform;
            hVisual.restLocalEuler = worldRot.eulerAngles;

            return root;
        }

        /// <summary>
        /// Builds distinct, recognizable 3D models for each of the 5 automation tiers (G1..G5).
        /// </summary>
        public static GameObject BuildMachine3D(string genId, Transform parent, Vector3 localPos, Quaternion localRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject($"Machine_{genId}_Model");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot;

            Material matWood = mats.ContainsKey("Wood_Stump") ? mats["Wood_Stump"] : mats["Desk"];
            Material matDarkIron = mats.ContainsKey("Dark_Iron") ? mats["Dark_Iron"] : mats["Anvil"];
            Material matSteel = mats.ContainsKey("Steel_Polished") ? mats["Steel_Polished"] : matDarkIron;
            Material matBronze = mats.ContainsKey("Bronze_Brass") ? mats["Bronze_Brass"] : mats["G1"];
            Material matFire = mats.ContainsKey("Furnace_Fire") ? mats["Furnace_Fire"] : mats["FurnaceEmber"];
            Material matCrystal = mats.ContainsKey("Ore_Crystal") ? mats["Ore_Crystal"] : mats["G5"];

            switch (genId)
            {
                case "G1": // Tay Búa Tí Hon (Mechanical Clockwork Auto-Hammer)
                    // Timber Pedestal Post
                    CreatePart(PrimitiveType.Cylinder, "Post", root.transform,
                        new Vector3(0f, 0.15f, 0f), new Vector3(0.18f, 0.15f, 0.18f), Quaternion.identity, matWood);
                    // Iron Anvillet Mini
                    CreatePart(PrimitiveType.Cube, "Mini_Anvil", root.transform,
                        new Vector3(0f, 0.32f, 0.08f), new Vector3(0.14f, 0.06f, 0.12f), Quaternion.identity, matSteel);
                    // Pivot Cam Wheel
                    CreatePart(PrimitiveType.Cylinder, "Cam_Gear", root.transform,
                        new Vector3(-0.08f, 0.36f, -0.06f), new Vector3(0.10f, 0.025f, 0.10f), Quaternion.Euler(90f, 0f, 0f), matBronze);
                    // Rocker Arm
                    CreatePart(PrimitiveType.Cube, "Hammer_Arm", root.transform,
                        new Vector3(0f, 0.40f, 0f), new Vector3(0.04f, 0.04f, 0.22f), Quaternion.Euler(-18f, 0f, 0f), matWood);
                    // Mini Hammer Head
                    CreatePart(PrimitiveType.Cube, "Hammer_Head", root.transform,
                        new Vector3(0f, 0.37f, 0.08f), new Vector3(0.08f, 0.07f, 0.07f), Quaternion.identity, matDarkIron);
                    break;

                case "G2": // Cũi Đào Quặng (Mining Ore Drill Cart)
                    // Cart Platform
                    CreatePart(PrimitiveType.Cube, "Cart_Base", root.transform,
                        new Vector3(0f, 0.08f, 0f), new Vector3(0.28f, 0.05f, 0.22f), Quaternion.identity, matDarkIron);
                    // 4 Wheels
                    CreatePart(PrimitiveType.Cylinder, "Wheel_FL", root.transform,
                        new Vector3(-0.13f, 0.06f, 0.09f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);
                    CreatePart(PrimitiveType.Cylinder, "Wheel_FR", root.transform,
                        new Vector3(0.13f, 0.06f, 0.09f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);
                    CreatePart(PrimitiveType.Cylinder, "Wheel_BL", root.transform,
                        new Vector3(-0.13f, 0.06f, -0.09f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);
                    CreatePart(PrimitiveType.Cylinder, "Wheel_BR", root.transform,
                        new Vector3(0.13f, 0.06f, -0.09f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.Euler(0f, 0f, 90f), matDarkIron);
                    // Heavy Spiral Drill Bit Mount
                    CreatePart(PrimitiveType.Cylinder, "Drill_Body", root.transform,
                        new Vector3(0f, 0.20f, -0.02f), new Vector3(0.12f, 0.10f, 0.12f), Quaternion.Euler(45f, 0f, 0f), matSteel);
                    // Spiral Drill Tip
                    CreatePart(PrimitiveType.Cylinder, "Drill_Bit", root.transform,
                        new Vector3(0f, 0.28f, 0.07f), new Vector3(0.07f, 0.12f, 0.07f), Quaternion.Euler(45f, 0f, 0f), matSteel);
                    // Raw Ore Rock Pile in Cart
                    CreatePart(PrimitiveType.Cube, "Ore_Nugget_1", root.transform,
                        new Vector3(0.04f, 0.15f, -0.05f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.Euler(15f, 25f, 35f), matCrystal);
                    break;

                case "G3": // Lò Than Tự Đun (Crucible Blast Smelter)
                    // Tripod Iron Legs
                    CreatePart(PrimitiveType.Cylinder, "Leg_1", root.transform,
                        new Vector3(-0.10f, 0.10f, -0.08f), new Vector3(0.03f, 0.10f, 0.03f), Quaternion.Euler(15f, 0f, -15f), matDarkIron);
                    CreatePart(PrimitiveType.Cylinder, "Leg_2", root.transform,
                        new Vector3(0.10f, 0.10f, -0.08f), new Vector3(0.03f, 0.10f, 0.03f), Quaternion.Euler(15f, 0f, 15f), matDarkIron);
                    CreatePart(PrimitiveType.Cylinder, "Leg_3", root.transform,
                        new Vector3(0f, 0.10f, 0.12f), new Vector3(0.03f, 0.10f, 0.03f), Quaternion.Euler(-15f, 0f, 0f), matDarkIron);
                    // Pot Crucible Body
                    CreatePart(PrimitiveType.Cylinder, "Crucible_Pot", root.transform,
                        new Vector3(0f, 0.25f, 0f), new Vector3(0.24f, 0.12f, 0.24f), Quaternion.identity, matDarkIron);
                    // Molten Metal Glowing Surface
                    CreatePart(PrimitiveType.Cylinder, "Molten_Metal", root.transform,
                        new Vector3(0f, 0.35f, 0f), new Vector3(0.21f, 0.02f, 0.21f), Quaternion.identity, matFire);
                    // Chimney Vent Flue
                    CreatePart(PrimitiveType.Cylinder, "Smoke_Vent", root.transform,
                        new Vector3(0f, 0.45f, -0.04f), new Vector3(0.08f, 0.10f, 0.08f), Quaternion.identity, matBronze);
                    break;

                case "G4": // Cối Xay Kim (Steam Piston Crusher)
                    // Base Frame
                    CreatePart(PrimitiveType.Cube, "Engine_Bed", root.transform,
                        new Vector3(0f, 0.06f, 0f), new Vector3(0.32f, 0.05f, 0.26f), Quaternion.identity, matDarkIron);
                    // Large Brass Flywheel Gear
                    CreatePart(PrimitiveType.Cylinder, "Flywheel", root.transform,
                        new Vector3(-0.10f, 0.24f, 0f), new Vector3(0.26f, 0.03f, 0.26f), Quaternion.Euler(0f, 0f, 90f), matBronze);
                    // Steam Cylinder Column
                    CreatePart(PrimitiveType.Cylinder, "Steam_Cylinder", root.transform,
                        new Vector3(0.08f, 0.26f, 0f), new Vector3(0.12f, 0.18f, 0.12f), Quaternion.identity, matBronze);
                    // Piston Rod
                    CreatePart(PrimitiveType.Cylinder, "Piston_Rod", root.transform,
                        new Vector3(0.08f, 0.44f, 0f), new Vector3(0.04f, 0.12f, 0.04f), Quaternion.identity, matSteel);
                    // Pressure Gauge
                    CreatePart(PrimitiveType.Cylinder, "Pressure_Gauge", root.transform,
                        new Vector3(0.08f, 0.36f, 0.08f), new Vector3(0.06f, 0.02f, 0.06f), Quaternion.Euler(90f, 0f, 0f), matBronze);
                    break;

                case "G5": // Robot Thợ Cả (Dwarven Automaton Golem)
                    // Sturdy Legs
                    CreatePart(PrimitiveType.Cube, "Leg_L", root.transform,
                        new Vector3(-0.08f, 0.12f, 0f), new Vector3(0.07f, 0.14f, 0.08f), Quaternion.identity, matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Leg_R", root.transform,
                        new Vector3(0.08f, 0.12f, 0f), new Vector3(0.07f, 0.14f, 0.08f), Quaternion.identity, matDarkIron);
                    // Armored Torso
                    CreatePart(PrimitiveType.Cube, "Torso", root.transform,
                        new Vector3(0f, 0.28f, 0f), new Vector3(0.22f, 0.16f, 0.15f), Quaternion.identity, matDarkIron);
                    // Glowing Magic Core Chest Plate
                    CreatePart(PrimitiveType.Cube, "Core_Heart", root.transform,
                        new Vector3(0f, 0.29f, 0.08f), new Vector3(0.09f, 0.09f, 0.04f), Quaternion.Euler(0f, 0f, 45f), matCrystal);
                    // Robot Head with Glowing Eye Slit
                    CreatePart(PrimitiveType.Cube, "Golem_Head", root.transform,
                        new Vector3(0f, 0.40f, 0f), new Vector3(0.12f, 0.10f, 0.11f), Quaternion.identity, matBronze);
                    CreatePart(PrimitiveType.Cube, "Eye_Visor", root.transform,
                        new Vector3(0f, 0.40f, 0.06f), new Vector3(0.08f, 0.025f, 0.02f), Quaternion.identity, matFire);
                    // Right Arm holding Sledgehammer
                    CreatePart(PrimitiveType.Cube, "Arm_R", root.transform,
                        new Vector3(0.16f, 0.28f, 0.04f), new Vector3(0.06f, 0.16f, 0.06f), Quaternion.Euler(-25f, 0f, 0f), matDarkIron);
                    CreatePart(PrimitiveType.Cube, "Golem_Hammer", root.transform,
                        new Vector3(0.17f, 0.20f, 0.14f), new Vector3(0.07f, 0.12f, 0.07f), Quaternion.identity, matSteel);
                    // Left Arm holding Tongs
                    CreatePart(PrimitiveType.Cube, "Arm_L", root.transform,
                        new Vector3(-0.16f, 0.28f, 0.04f), new Vector3(0.06f, 0.16f, 0.06f), Quaternion.Euler(-25f, 0f, 0f), matDarkIron);
                    break;
            }

            return root;
        }

        /// <summary>
        /// Builds 3D Ore Shelf with shelves and faceted ore crystals.
        /// </summary>
        public static GameObject BuildOreShelf3D(Transform parent, Vector3 localPos, Quaternion localRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("OreShelf_3D");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot;

            Material matWood = mats.ContainsKey("Wood_Stump") ? mats["Wood_Stump"] : mats["Desk"];
            Material matDarkIron = mats.ContainsKey("Dark_Iron") ? mats["Dark_Iron"] : mats["Anvil"];
            Material matCrystal = mats.ContainsKey("Ore_Crystal") ? mats["Ore_Crystal"] : mats["G2"];
            Material matGold = mats.ContainsKey("Bronze_Brass") ? mats["Bronze_Brass"] : mats["G1"];

            // Wooden Frame Uprights
            CreatePart(PrimitiveType.Cube, "Post_L", root.transform,
                new Vector3(-0.16f, 0.16f, 0f), new Vector3(0.035f, 0.32f, 0.18f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Post_R", root.transform,
                new Vector3(0.16f, 0.16f, 0f), new Vector3(0.035f, 0.32f, 0.18f), Quaternion.identity, matWood);

            // Shelves (Bottom & Top)
            CreatePart(PrimitiveType.Cube, "Shelf_Bottom", root.transform,
                new Vector3(0f, 0.06f, 0f), new Vector3(0.34f, 0.03f, 0.18f), Quaternion.identity, matWood);
            CreatePart(PrimitiveType.Cube, "Shelf_Top", root.transform,
                new Vector3(0f, 0.24f, 0f), new Vector3(0.34f, 0.03f, 0.18f), Quaternion.identity, matWood);

            // Ore Crystals on Shelves
            CreatePart(PrimitiveType.Cube, "Crystal_1", root.transform,
                new Vector3(-0.06f, 0.12f, 0f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.Euler(25f, 35f, 15f), matCrystal);
            CreatePart(PrimitiveType.Cube, "Crystal_2", root.transform,
                new Vector3(0.06f, 0.13f, 0.02f), new Vector3(0.09f, 0.09f, 0.09f), Quaternion.Euler(-15f, 45f, 20f), matGold);
            CreatePart(PrimitiveType.Cube, "Crystal_3", root.transform,
                new Vector3(0f, 0.30f, 0f), new Vector3(0.07f, 0.07f, 0.07f), Quaternion.Euler(30f, 15f, 60f), matCrystal);

            return root;
        }

        /// <summary>
        /// Builds 3D Treasure Chest with brass trim and ingots inside.
        /// </summary>
        public static GameObject BuildChest3D(Transform parent, Vector3 localPos, Quaternion localRot, Dictionary<string, Material> mats)
        {
            GameObject root = new GameObject("Chest_3D");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = localRot;

            Material matWood = mats.ContainsKey("Wood_Stump") ? mats["Wood_Stump"] : mats["Desk"];
            Material matDarkIron = mats.ContainsKey("Dark_Iron") ? mats["Dark_Iron"] : mats["Anvil"];
            Material matGold = mats.ContainsKey("Bronze_Brass") ? mats["Bronze_Brass"] : mats["G1"];

            // Chest Body Box
            CreatePart(PrimitiveType.Cube, "Chest_Base", root.transform,
                new Vector3(0f, 0.10f, 0f), new Vector3(0.30f, 0.18f, 0.22f), Quaternion.identity, matWood);
            // Metal Corner Straps
            CreatePart(PrimitiveType.Cube, "Strap_L", root.transform,
                new Vector3(-0.11f, 0.10f, 0f), new Vector3(0.025f, 0.185f, 0.225f), Quaternion.identity, matDarkIron);
            CreatePart(PrimitiveType.Cube, "Strap_R", root.transform,
                new Vector3(0.11f, 0.10f, 0f), new Vector3(0.025f, 0.185f, 0.225f), Quaternion.identity, matDarkIron);
            // Lock Latch
            CreatePart(PrimitiveType.Cube, "Lock_Latch", root.transform,
                new Vector3(0f, 0.14f, 0.115f), new Vector3(0.045f, 0.06f, 0.02f), Quaternion.identity, matGold);
            // Opened Arched Lid (Angled back)
            CreatePart(PrimitiveType.Cube, "Chest_Lid", root.transform,
                new Vector3(0f, 0.24f, -0.05f), new Vector3(0.31f, 0.08f, 0.22f), Quaternion.Euler(-35f, 0f, 0f), matWood);
            // Gold Ingots Inside
            CreatePart(PrimitiveType.Cube, "Gold_Ingot_1", root.transform,
                new Vector3(-0.05f, 0.16f, 0f), new Vector3(0.10f, 0.035f, 0.055f), Quaternion.Euler(0f, 15f, 0f), matGold);
            CreatePart(PrimitiveType.Cube, "Gold_Ingot_2", root.transform,
                new Vector3(0.05f, 0.17f, 0.02f), new Vector3(0.10f, 0.035f, 0.055f), Quaternion.Euler(0f, -20f, 0f), matGold);

            return root;
        }
    }
}
