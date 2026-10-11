using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Forge.EditorTools
{
    /// <summary>
    /// Generates high-fidelity native Wavefront .OBJ 3D models with clean vertex normals,
    /// seamless UV coordinates, and dedicated submeshes for each PBR material group.
    /// Eliminates clunky primitive boxes and produces stunning AAA Dwarven aesthetics!
    /// </summary>
    public static class ForgeObjModelGenerator
    {
        private const string MODELS_DIR = "Assets/_Forge/Art/Models";

        private class ObjBuilder
        {
            private List<Vector3> verts = new List<Vector3>();
            private List<Vector3> normals = new List<Vector3>();
            private List<Vector2> uvs = new List<Vector2>();
            private Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
            private string currentGroup = "Default";

            public void SetGroup(string groupName)
            {
                currentGroup = groupName;
                if (!groups.ContainsKey(currentGroup))
                {
                    groups[currentGroup] = new List<string>();
                }
            }

            public void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                int baseIdx = verts.Count + 1; // 1-indexed for OBJ

                verts.Add(p0); normals.Add(normal); uvs.Add(uv0);
                verts.Add(p1); normals.Add(normal); uvs.Add(uv1);
                verts.Add(p2); normals.Add(normal); uvs.Add(uv2);
                verts.Add(p3); normals.Add(normal); uvs.Add(uv3);

                if (!groups.ContainsKey(currentGroup)) groups[currentGroup] = new List<string>();
                // Tri 1: 0, 1, 2
                groups[currentGroup].Add($"f {baseIdx}/{baseIdx}/{baseIdx} {baseIdx + 1}/{baseIdx + 1}/{baseIdx + 1} {baseIdx + 2}/{baseIdx + 2}/{baseIdx + 2}");
                // Tri 2: 0, 2, 3
                groups[currentGroup].Add($"f {baseIdx}/{baseIdx}/{baseIdx} {baseIdx + 2}/{baseIdx + 2}/{baseIdx + 2} {baseIdx + 3}/{baseIdx + 3}/{baseIdx + 3}");
            }

            public void AddBox(Vector3 center, Vector3 size)
            {
                Vector3 h = size * 0.5f;
                // 8 corners
                Vector3 c0 = center + new Vector3(-h.x, -h.y, -h.z);
                Vector3 c1 = center + new Vector3(h.x, -h.y, -h.z);
                Vector3 c2 = center + new Vector3(h.x, h.y, -h.z);
                Vector3 c3 = center + new Vector3(-h.x, h.y, -h.z);
                Vector3 c4 = center + new Vector3(-h.x, -h.y, h.z);
                Vector3 c5 = center + new Vector3(h.x, -h.y, h.z);
                Vector3 c6 = center + new Vector3(h.x, h.y, h.z);
                Vector3 c7 = center + new Vector3(-h.x, h.y, h.z);

                Vector2 uv0 = new Vector2(0, 0); Vector2 uv1 = new Vector2(1, 0);
                Vector2 uv2 = new Vector2(1, 1); Vector2 uv3 = new Vector2(0, 1);

                AddQuad(c0, c3, c2, c1, uv0, uv3, uv2, uv1); // Front (-Z)
                AddQuad(c5, c6, c7, c4, uv0, uv3, uv2, uv1); // Back (+Z)
                AddQuad(c4, c7, c3, c0, uv0, uv3, uv2, uv1); // Left (-X)
                AddQuad(c1, c2, c6, c5, uv0, uv3, uv2, uv1); // Right (+X)
                AddQuad(c3, c7, c6, c2, uv0, uv3, uv2, uv1); // Top (+Y)
                AddQuad(c4, c0, c1, c5, uv0, uv3, uv2, uv1); // Bottom (-Y)
            }

            public void AddBoxOriented(Vector3 center, Vector3 size, Quaternion rot)
            {
                Vector3 h = size * 0.5f;
                Vector3 c0 = center + rot * new Vector3(-h.x, -h.y, -h.z);
                Vector3 c1 = center + rot * new Vector3(h.x, -h.y, -h.z);
                Vector3 c2 = center + rot * new Vector3(h.x, h.y, -h.z);
                Vector3 c3 = center + rot * new Vector3(-h.x, h.y, -h.z);
                Vector3 c4 = center + rot * new Vector3(-h.x, -h.y, h.z);
                Vector3 c5 = center + rot * new Vector3(h.x, -h.y, h.z);
                Vector3 c6 = center + rot * new Vector3(h.x, h.y, h.z);
                Vector3 c7 = center + rot * new Vector3(-h.x, h.y, h.z);

                Vector2 uv0 = new Vector2(0, 0); Vector2 uv1 = new Vector2(1, 0);
                Vector2 uv2 = new Vector2(1, 1); Vector2 uv3 = new Vector2(0, 1);

                AddQuad(c0, c3, c2, c1, uv0, uv3, uv2, uv1); // Front (-Z)
                AddQuad(c5, c6, c7, c4, uv0, uv3, uv2, uv1); // Back (+Z)
                AddQuad(c4, c7, c3, c0, uv0, uv3, uv2, uv1); // Left (-X)
                AddQuad(c1, c2, c6, c5, uv0, uv3, uv2, uv1); // Right (+X)
                AddQuad(c3, c7, c6, c2, uv0, uv3, uv2, uv1); // Top (+Y)
                AddQuad(c4, c0, c1, c5, uv0, uv3, uv2, uv1); // Bottom (-Y)
            }

            public void AddTrapezoidPrism(Vector3 center, Vector3 bottomSize, Vector3 topSize, Quaternion rot)
            {
                Vector3 hb = bottomSize * 0.5f;
                Vector3 ht = topSize * 0.5f;
                float halfH = bottomSize.y * 0.5f;

                Vector3 c0 = center + rot * new Vector3(-hb.x, -halfH, -hb.z);
                Vector3 c1 = center + rot * new Vector3(hb.x, -halfH, -hb.z);
                Vector3 c2 = center + rot * new Vector3(ht.x, halfH, -ht.z);
                Vector3 c3 = center + rot * new Vector3(-ht.x, halfH, -ht.z);

                Vector3 c4 = center + rot * new Vector3(-hb.x, -halfH, hb.z);
                Vector3 c5 = center + rot * new Vector3(hb.x, -halfH, hb.z);
                Vector3 c6 = center + rot * new Vector3(ht.x, halfH, ht.z);
                Vector3 c7 = center + rot * new Vector3(-ht.x, halfH, ht.z);

                Vector2 uv0 = new Vector2(0, 0); Vector2 uv1 = new Vector2(1, 0);
                Vector2 uv2 = new Vector2(1, 1); Vector2 uv3 = new Vector2(0, 1);

                AddQuad(c0, c3, c2, c1, uv0, uv3, uv2, uv1); // Front
                AddQuad(c5, c6, c7, c4, uv0, uv3, uv2, uv1); // Back
                AddQuad(c4, c7, c3, c0, uv0, uv3, uv2, uv1); // Left
                AddQuad(c1, c2, c6, c5, uv0, uv3, uv2, uv1); // Right
                AddQuad(c3, c7, c6, c2, uv0, uv3, uv2, uv1); // Top
                AddQuad(c4, c0, c1, c5, uv0, uv3, uv2, uv1); // Bottom
            }

            public void AddCrystalSpire(Vector3 baseCenter, Vector3 tipPos, float radius, int segments = 6)
            {
                Vector3 dir = (tipPos - baseCenter);
                float len = dir.magnitude;
                if (len < 0.001f) return;
                dir /= len;

                Vector3 u = Mathf.Abs(dir.y) < 0.9f ? Vector3.Cross(dir, Vector3.up).normalized : Vector3.Cross(dir, Vector3.right).normalized;
                Vector3 v = Vector3.Cross(dir, u).normalized;

                Vector3 midCenter = baseCenter + dir * (len * 0.70f);
                float angStep = Mathf.PI * 2f / segments;

                Vector3[] midVerts = new Vector3[segments];
                for (int i = 0; i < segments; i++)
                {
                    float a = i * angStep;
                    midVerts[i] = midCenter + (u * Mathf.Cos(a) + v * Mathf.Sin(a)) * radius;
                }

                for (int i = 0; i < segments; i++)
                {
                    int next = (i + 1) % segments;
                    // Lower facet
                    AddTriangle(baseCenter, midVerts[next], midVerts[i], new Vector2(0.5f, 0f), new Vector2(1f, 0.7f), new Vector2(0f, 0.7f));
                    // Upper facet
                    AddTriangle(midVerts[i], midVerts[next], tipPos, new Vector2(0f, 0.7f), new Vector2(1f, 0.7f), new Vector2(0.5f, 1f));
                }
            }

            public void AddCylinder(Vector3 bottomCenter, float radiusBottom, float radiusTop, float height, int segments = 16)
            {
                float angStep = Mathf.PI * 2f / segments;
                Vector3 topCenter = bottomCenter + new Vector3(0, height, 0);

                for (int i = 0; i < segments; i++)
                {
                    float a0 = i * angStep;
                    float a1 = (i + 1) * angStep;

                    Vector3 b0 = bottomCenter + new Vector3(Mathf.Cos(a0) * radiusBottom, 0, Mathf.Sin(a0) * radiusBottom);
                    Vector3 b1 = bottomCenter + new Vector3(Mathf.Cos(a1) * radiusBottom, 0, Mathf.Sin(a1) * radiusBottom);
                    Vector3 t0 = topCenter + new Vector3(Mathf.Cos(a0) * radiusTop, 0, Mathf.Sin(a0) * radiusTop);
                    Vector3 t1 = topCenter + new Vector3(Mathf.Cos(a1) * radiusTop, 0, Mathf.Sin(a1) * radiusTop);

                    float u0 = (float)i / segments;
                    float u1 = (float)(i + 1) / segments;

                    AddQuad(b0, t0, t1, b1, new Vector2(u0, 0), new Vector2(u0, 1), new Vector2(u1, 1), new Vector2(u1, 0));

                    // Top cap
                    if (radiusTop > 0.001f)
                    {
                        AddTriangle(topCenter, t0, t1, new Vector2(0.5f, 0.5f), new Vector2(Mathf.Cos(a0) * 0.5f + 0.5f, Mathf.Sin(a0) * 0.5f + 0.5f), new Vector2(Mathf.Cos(a1) * 0.5f + 0.5f, Mathf.Sin(a1) * 0.5f + 0.5f));
                    }
                    // Bottom cap
                    if (radiusBottom > 0.001f)
                    {
                        AddTriangle(bottomCenter, b1, b0, new Vector2(0.5f, 0.5f), new Vector2(Mathf.Cos(a1) * 0.5f + 0.5f, Mathf.Sin(a1) * 0.5f + 0.5f), new Vector2(Mathf.Cos(a0) * 0.5f + 0.5f, Mathf.Sin(a0) * 0.5f + 0.5f));
                    }
                }
            }

            public void AddCylinderOriented(Vector3 baseCenter, Vector3 topCenter, float radiusBase, float radiusTop, int segments = 16)
            {
                Vector3 dir = (topCenter - baseCenter);
                float len = dir.magnitude;
                if (len < 0.0001f) return;
                dir /= len;

                Vector3 u = Mathf.Abs(dir.y) < 0.9f ? Vector3.Cross(dir, Vector3.up).normalized : Vector3.Cross(dir, Vector3.right).normalized;
                Vector3 v = Vector3.Cross(dir, u).normalized;

                float angStep = Mathf.PI * 2f / segments;

                for (int i = 0; i < segments; i++)
                {
                    float a0 = i * angStep;
                    float a1 = (i + 1) * angStep;

                    Vector3 rad0 = u * Mathf.Cos(a0) + v * Mathf.Sin(a0);
                    Vector3 rad1 = u * Mathf.Cos(a1) + v * Mathf.Sin(a1);

                    Vector3 b0 = baseCenter + rad0 * radiusBase;
                    Vector3 b1 = baseCenter + rad1 * radiusBase;
                    Vector3 t0 = topCenter + rad0 * radiusTop;
                    Vector3 t1 = topCenter + rad1 * radiusTop;

                    float u0 = (float)i / segments;
                    float u1 = (float)(i + 1) / segments;

                    AddQuad(b0, b1, t1, t0, new Vector2(u0, 0), new Vector2(u1, 0), new Vector2(u1, 1), new Vector2(u0, 1));

                    // Top cap
                    if (radiusTop > 0.001f)
                    {
                        AddTriangle(topCenter, t0, t1, new Vector2(0.5f, 0.5f), new Vector2(Mathf.Cos(a0) * 0.5f + 0.5f, Mathf.Sin(a0) * 0.5f + 0.5f), new Vector2(Mathf.Cos(a1) * 0.5f + 0.5f, Mathf.Sin(a1) * 0.5f + 0.5f));
                    }
                    // Bottom cap
                    if (radiusBase > 0.001f)
                    {
                        AddTriangle(baseCenter, b1, b0, new Vector2(0.5f, 0.5f), new Vector2(Mathf.Cos(a1) * 0.5f + 0.5f, Mathf.Sin(a1) * 0.5f + 0.5f), new Vector2(Mathf.Cos(a0) * 0.5f + 0.5f, Mathf.Sin(a0) * 0.5f + 0.5f));
                    }
                }
            }

            public void AddTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
            {
                Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                int baseIdx = verts.Count + 1;

                verts.Add(p0); normals.Add(normal); uvs.Add(uv0);
                verts.Add(p1); normals.Add(normal); uvs.Add(uv1);
                verts.Add(p2); normals.Add(normal); uvs.Add(uv2);

                if (!groups.ContainsKey(currentGroup)) groups[currentGroup] = new List<string>();
                groups[currentGroup].Add($"f {baseIdx}/{baseIdx}/{baseIdx} {baseIdx + 1}/{baseIdx + 1}/{baseIdx + 1} {baseIdx + 2}/{baseIdx + 2}/{baseIdx + 2}");
            }

            public string ExportToString(string modelName)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"# Legend of the Forge 3D Model: {modelName}");
                sb.AppendLine($"# Sculpted by Truong Giang");

                CultureInfo inv = CultureInfo.InvariantCulture;

                foreach (var v in verts)
                    sb.AppendLine($"v {v.x.ToString("F4", inv)} {v.y.ToString("F4", inv)} {v.z.ToString("F4", inv)}");

                foreach (var vt in uvs)
                    sb.AppendLine($"vt {vt.x.ToString("F4", inv)} {vt.y.ToString("F4", inv)}");

                foreach (var vn in normals)
                    sb.AppendLine($"vn {vn.x.ToString("F4", inv)} {vn.y.ToString("F4", inv)} {vn.z.ToString("F4", inv)}");

                foreach (var grp in groups)
                {
                    sb.AppendLine($"g {grp.Key}");
                    sb.AppendLine($"usemtl {grp.Key}");
                    foreach (var f in grp.Value)
                        sb.AppendLine(f);
                }

                return sb.ToString();
            }
        }

        [MenuItem("Forge/Art/Generate 3D Native Models (.OBJ)", false, 55)]
        public static void GenerateAllObjModels()
        {
            if (!Directory.Exists(MODELS_DIR))
            {
                Directory.CreateDirectory(MODELS_DIR);
                AssetDatabase.Refresh();
            }

            Debug.Log("[ForgeModels] Generating AAA 3D Native OBJ Models...");

            string workbenchObj = BuildWorkbenchObj();
            File.WriteAllText(Path.Combine(MODELS_DIR, "Workbench_Master.obj"), workbenchObj);

            string anvilObj = BuildAnvilObj();
            File.WriteAllText(Path.Combine(MODELS_DIR, "Anvil_Dwarven.obj"), anvilObj);

            string furnaceObj = BuildFurnaceObj();
            File.WriteAllText(Path.Combine(MODELS_DIR, "Furnace_Dwarven.obj"), furnaceObj);

            string hammerObj = BuildHammerObj();
            File.WriteAllText(Path.Combine(MODELS_DIR, "Hammer_Dwarven.obj"), hammerObj);

            string deskMaterialsObj = BuildDeskMaterialsObj();
            File.WriteAllText(Path.Combine(MODELS_DIR, "Desk_Materials.obj"), deskMaterialsObj);

            AssetDatabase.Refresh();
            ConfigureModelImports();
            Debug.Log("[ForgeModels] All 3D OBJ Models successfully created and imported!");
        }

        private static void ConfigureModelImports()
        {
            string[] files = Directory.GetFiles(MODELS_DIR, "*.obj");
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer != null)
                {
                    importer.globalScale = 1.0f;
                    importer.importNormals = ModelImporterNormals.Import;
                    importer.importBlendShapes = false;
                    importer.importAnimation = false;
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.preserveHierarchy = true;
                    importer.SaveAndReimport();
                }
            }
        }

        /// <summary>
        /// Sculpted Dwarven Anvil with 4 clean submesh groups:
        /// 1. Oak_Stump: Realistic tree stump with 24-gon trunk and iron banding
        /// 2. Dark_Iron: London pattern curved anvil body, flared base, waist, horn, heel
        /// 3. Steel_Polished: Mirror-polished bevelled top table face
        /// 4. Hot_Ingot: Glowing tapered forged sword blade blank
        /// </summary>
        public static string BuildAnvilObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Oak_Stump (Gỗ Sồi Cổ Thụ)
            // ==========================================
            b.SetGroup("Wood_Stump");
            // Solid 24-sided Oak tree stump (r=0.35, h=0.48)
            b.AddCylinder(new Vector3(0, 0, 0), 0.35f, 0.33f, 0.48f, 24);

            // ==========================================
            // GROUP 2: Dark_Iron (Thân Đe Rèn Thép Hắc Thiết)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Iron bands around stump
            b.AddCylinder(new Vector3(0, 0.10f, 0), 0.355f, 0.355f, 0.035f, 24);
            b.AddCylinder(new Vector3(0, 0.38f, 0), 0.338f, 0.338f, 0.035f, 24);

            // Flared Base Footing (Chân đe xòe cong 4 góc: 0.48 x 0.07 x 0.30)
            b.AddBox(new Vector3(0, 0.515f, 0), new Vector3(0.48f, 0.07f, 0.30f));
            b.AddBox(new Vector3(0, 0.565f, 0), new Vector3(0.38f, 0.05f, 0.24f));

            // Sculpted Concave Waist (Eo thắt cong thon thả)
            b.AddBox(new Vector3(0.01f, 0.625f, 0), new Vector3(0.24f, 0.08f, 0.18f));
            b.AddBox(new Vector3(0.02f, 0.705f, 0), new Vector3(0.28f, 0.08f, 0.20f));

            // Upper Anvil Body (Thân đe trên)
            b.AddBox(new Vector3(0.03f, 0.775f, 0), new Vector3(0.42f, 0.08f, 0.22f));

            // Tapered Conical Horn extending towards +X in OBJ (which becomes -X / Left in Unity)
            b.AddCylinderOriented(new Vector3(0.16f, 0.79f, 0), new Vector3(0.42f, 0.81f, 0), 0.085f, 0.025f, 16);

            // Square Heel extending towards -X in OBJ (which becomes +X / Right in Unity)
            b.AddBox(new Vector3(-0.28f, 0.79f, 0), new Vector3(0.16f, 0.07f, 0.18f));

            // ==========================================
            // GROUP 3: Steel_Polished (Mặt Đe Thép Mài Sáng Bóng)
            // ==========================================
            b.SetGroup("Steel_Polished");

            // Precision flat table top face plate (0.36 x 0.025 x 0.20)
            b.AddBox(new Vector3(-0.04f, 0.825f, 0), new Vector3(0.36f, 0.025f, 0.20f));

            // Bevelled Step Table between horn and face
            b.AddBox(new Vector3(0.15f, 0.815f, 0), new Vector3(0.05f, 0.015f, 0.16f));

            // ==========================================
            // GROUP 4: Hot_Ingot (Phôi Kiếm Thần Binh Nóng Đỏ)
            // ==========================================
            b.SetGroup("Hot_Ingot");

            // Forged glowing hot sword blade laying across the anvil face
            b.AddBox(new Vector3(0.02f, 0.850f, 0), new Vector3(0.24f, 0.025f, 0.06f));
            b.AddBox(new Vector3(0.14f, 0.852f, 0.01f), new Vector3(0.08f, 0.020f, 0.04f));

            return b.ExportToString("Anvil_Dwarven");
        }

        /// <summary>
        /// Sculpted Dwarven Stone Hearth Furnace with 5 submesh groups:
        /// 1. Stone_Furnace: Stepped plinth, fluted pillars, 7-voussoir curved stone arch, spandrels, cornice, chimney shaft
        /// 2. Dark_Iron: Cast-iron floor grate, keystone, tiered hammered smoke hood, chimney bands & crown
        /// 3. Bronze_Brass: Ornate keystone medallion, hood rivet band studs, pillar brackets
        /// 4. Furnace_Fire: Deep combustion glowing coal bed, burning split logs, rising flame tongues
        /// 5. Wood_Stump: Side bellows with leather pleats, quench bucket with tongs, firewood pile
        /// </summary>
        public static string BuildFurnaceObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Stone_Furnace (Vòm Đá Hoa Cương Cổ)
            // ==========================================
            b.SetGroup("Stone_Furnace");

            // Stepped Masonry Plinth Base Foundation (Bệ móng đá hoa cương 2 bậc vững chắc)
            b.AddBox(new Vector3(0, 0.05f, 0), new Vector3(1.26f, 0.10f, 0.92f));
            b.AddBox(new Vector3(0, 0.14f, -0.02f), new Vector3(1.16f, 0.08f, 0.86f));

            // Left Stone Pillar Jamb (Cột đá trái)
            b.AddBox(new Vector3(-0.38f, 0.50f, 0.00f), new Vector3(0.24f, 0.64f, 0.66f));
            // Left Pillar Impost Capital (Phào chỉ đầu cột trái)
            b.AddBox(new Vector3(-0.38f, 0.84f, -0.02f), new Vector3(0.28f, 0.06f, 0.66f));

            // Right Stone Pillar Jamb (Cột đá phải)
            b.AddBox(new Vector3(0.38f, 0.50f, 0.00f), new Vector3(0.24f, 0.64f, 0.66f));
            // Right Pillar Impost Capital (Phào chỉ đầu cột phải)
            b.AddBox(new Vector3(0.38f, 0.84f, -0.02f), new Vector3(0.28f, 0.06f, 0.66f));

            // Back Masonry Wall (Tường đá sau)
            b.AddBox(new Vector3(0, 0.58f, 0.24f), new Vector3(0.96f, 0.80f, 0.22f));

            // TRUE SEGMENTED RADIATING VOUSSOIR STONE ARCH (Vòm đá cuốn 7 khối voussoir nhô ra mặt tiền)
            // Arch Center: (0, 0.84, -0.28), Radius = 0.27
            float[] archAngles = new float[] { 15f, 38f, 62f, 118f, 142f, 165f }; // 90° is keystone
            float archRadius = 0.27f;
            Vector3 archCenter = new Vector3(0, 0.84f, -0.28f);
            foreach (float angDeg in archAngles)
            {
                float angRad = angDeg * Mathf.Deg2Rad;
                Vector3 vPos = archCenter + new Vector3(Mathf.Cos(angRad) * archRadius, Mathf.Sin(angRad) * archRadius, 0);
                Quaternion vRot = Quaternion.Euler(0, 0, angDeg - 90f);
                b.AddBoxOriented(vPos, new Vector3(0.12f, 0.14f, 0.24f), vRot);
            }

            // Front Spandrel Masonry Wall (Vách đá tam giác ôm vòm cuốn)
            b.AddBox(new Vector3(-0.28f, 1.02f, -0.16f), new Vector3(0.24f, 0.30f, 0.36f));
            b.AddBox(new Vector3(0.28f, 1.02f, -0.16f), new Vector3(0.24f, 0.30f, 0.36f));

            // Heavy Masonry Cornice Mantel Shelf (Gờ mái đá hoa cương nằm ngang nâng nón khói)
            b.AddBox(new Vector3(0, 1.18f, -0.10f), new Vector3(0.92f, 0.08f, 0.48f));

            // Chimney Flue Shaft Stone Core
            b.AddCylinder(new Vector3(0, 1.48f, 0.00f), 0.22f, 0.21f, 0.16f, 16);

            // ==========================================
            // GROUP 2: Dark_Iron (Phụ Kiện Sắt Đen, Vỉ Lò, Nón Hút Khói, Vương Miện)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Hearth Apron Sill Plate & Retaining Fender (Mặt ngưỡng sắt chân lò)
            b.AddBox(new Vector3(0, 0.20f, -0.34f), new Vector3(0.86f, 0.05f, 0.20f));
            b.AddBox(new Vector3(0, 0.25f, -0.26f), new Vector3(0.56f, 0.045f, 0.04f));

            // Cast-iron Floor Grate Bars (Vỉ sắt đúc 5 nan chịu nhiệt)
            for (int i = -2; i <= 2; i++)
            {
                b.AddBox(new Vector3(i * 0.09f, 0.21f, -0.05f), new Vector3(0.035f, 0.035f, 0.38f));
            }

            // Heavy Sculpted Keystone at Top of Arch (Khối đá chốt đỉnh vòm)
            b.AddTrapezoidPrism(new Vector3(0, 1.08f, -0.26f), new Vector3(0.18f, 0.18f, 0.26f), new Vector3(0.24f, 0.18f, 0.26f), Quaternion.identity);

            // Tiered Hammered Smoke Hood (Nón hút khói sắt đúc thu côn chuẩn tỷ lệ camera)
            b.AddCylinder(new Vector3(0, 1.22f, 0.00f), 0.38f, 0.26f, 0.22f, 16);
            b.AddCylinder(new Vector3(0, 1.44f, 0.00f), 0.27f, 0.27f, 0.04f, 16); // Rivet collar band

            // Chimney Flue & Dwarven Crown Cowl (Vương miện tán khói lò rèn đặc trưng)
            b.AddCylinder(new Vector3(0, 1.62f, 0.00f), 0.23f, 0.33f, 0.07f, 16); // Flared crown
            b.AddCylinder(new Vector3(0, 1.69f, 0.00f), 0.35f, 0.32f, 0.03f, 16); // Top cap

            // Tuyere Iron Nozzle from bellows on outer right wall (-X in OBJ = +X in Unity)
            b.AddCylinderOriented(new Vector3(-0.48f, 0.44f, -0.05f), new Vector3(-0.24f, 0.36f, -0.05f), 0.03f, 0.02f, 10);

            // Quench Bucket Iron Hoops & Tongs resting on bucket
            b.AddCylinder(new Vector3(0.50f, 0.12f, -0.18f), 0.165f, 0.165f, 0.025f, 14);
            b.AddCylinder(new Vector3(0.50f, 0.24f, -0.18f), 0.175f, 0.175f, 0.025f, 14);
            b.AddCylinderOriented(new Vector3(0.38f, 0.30f, -0.26f), new Vector3(0.62f, 0.30f, -0.10f), 0.015f, 0.015f, 8);

            // ==========================================
            // GROUP 3: Bronze_Brass (Họa Tiết Đồng Hoàng Gia)
            // ==========================================
            b.SetGroup("Bronze_Brass");

            // Keystone Medallion Boss (Đĩa phù hiệu đồng hoàng gia to bản đường kính 0.20m trên mặt tiền)
            b.AddCylinderOriented(new Vector3(0, 1.08f, -0.37f), new Vector3(0, 1.08f, -0.42f), 0.10f, 0.095f, 16);

            // Left & Right Arch Impost Bronze Accent Plates (Nẹp đồng chân vòm 2 bên)
            b.AddBox(new Vector3(-0.38f, 0.84f, -0.34f), new Vector3(0.18f, 0.04f, 0.04f));
            b.AddBox(new Vector3(0.38f, 0.84f, -0.34f), new Vector3(0.18f, 0.04f, 0.04f));

            // Smokehood Rivet Studs around the collar band (8 đinh tán đồng lấp lánh)
            for (int r = 0; r < 8; r++)
            {
                float ra = r * Mathf.PI * 0.25f;
                b.AddBoxOriented(new Vector3(Mathf.Cos(ra) * 0.28f, 1.46f, Mathf.Sin(ra) * 0.28f),
                    new Vector3(0.025f, 0.025f, 0.025f), Quaternion.Euler(0, -ra * Mathf.Rad2Deg, 0));
            }

            // Decorative Arch Base Anchors
            b.AddBox(new Vector3(-0.26f, 0.88f, -0.33f), new Vector3(0.04f, 0.06f, 0.04f));
            b.AddBox(new Vector3(0.26f, 0.88f, -0.33f), new Vector3(0.04f, 0.06f, 0.04f));

            // ==========================================
            // GROUP 4: Furnace_Fire (Lòng Than Rực Lửa & Củi Đang Cháy)
            // ==========================================
            b.SetGroup("Furnace_Fire");

            // Combustion Core Ember Bed (Lòng than hồng rực rỡ)
            b.AddBox(new Vector3(0, 0.24f, -0.05f), new Vector3(0.52f, 0.07f, 0.36f));

            // Multi-Cluster Irregular Coal Nuggets (5 khối than đá nóng đỏ)
            b.AddBoxOriented(new Vector3(-0.14f, 0.30f, -0.12f), new Vector3(0.16f, 0.09f, 0.16f), Quaternion.Euler(12f, 25f, -10f));
            b.AddBoxOriented(new Vector3(0.13f, 0.31f, -0.10f), new Vector3(0.15f, 0.10f, 0.15f), Quaternion.Euler(-15f, -30f, 12f));
            b.AddBoxOriented(new Vector3(-0.08f, 0.32f, 0.06f), new Vector3(0.14f, 0.09f, 0.14f), Quaternion.Euler(20f, 10f, 5f));
            b.AddBoxOriented(new Vector3(0.11f, 0.31f, 0.08f), new Vector3(0.15f, 0.09f, 0.15f), Quaternion.Euler(-10f, 40f, -8f));
            b.AddBoxOriented(new Vector3(0f, 0.34f, -0.04f), new Vector3(0.22f, 0.11f, 0.20f), Quaternion.Euler(5f, -15f, 10f));

            // Burning Firewood Split Logs laying criss-cross (3 khúc củi chẻ gối nhau cháy âm ỉ)
            b.AddCylinderOriented(new Vector3(-0.16f, 0.29f, -0.16f), new Vector3(0.12f, 0.38f, 0.06f), 0.042f, 0.038f, 8);
            b.AddCylinderOriented(new Vector3(0.15f, 0.30f, -0.14f), new Vector3(-0.11f, 0.40f, 0.04f), 0.040f, 0.036f, 8);
            b.AddCylinderOriented(new Vector3(-0.06f, 0.38f, -0.18f), new Vector3(0.06f, 0.44f, 0.08f), 0.035f, 0.032f, 8);

            // Rising Blazing Flame Spires (3 ngọn lửa 3D vươn cao soi sáng khoang lò)
            b.AddCrystalSpire(new Vector3(0, 0.40f, -0.06f), new Vector3(0, 0.64f, -0.06f), 0.08f, 6);
            b.AddCrystalSpire(new Vector3(-0.09f, 0.38f, -0.08f), new Vector3(-0.07f, 0.56f, -0.08f), 0.05f, 5);
            b.AddCrystalSpire(new Vector3(0.08f, 0.38f, -0.05f), new Vector3(0.07f, 0.57f, -0.05f), 0.05f, 5);

            // ==========================================
            // GROUP 5: Wood_Stump (Bễ Thổi, Xô Nước & Chồng Củi)
            // ==========================================
            b.SetGroup("Wood_Stump");

            // Blacksmith Bellows Assembly on outer right wall (-X in OBJ = +X in Unity)
            b.AddBoxOriented(new Vector3(-0.56f, 0.58f, -0.05f), new Vector3(0.18f, 0.03f, 0.36f), Quaternion.Euler(0, -15f, 8f));
            b.AddBoxOriented(new Vector3(-0.56f, 0.49f, -0.05f), new Vector3(0.16f, 0.15f, 0.32f), Quaternion.Euler(0, -15f, 8f));
            b.AddBoxOriented(new Vector3(-0.56f, 0.40f, -0.05f), new Vector3(0.18f, 0.03f, 0.36f), Quaternion.Euler(0, -15f, 8f));

            // Quench Tub Wooden Staves (Xô gỗ tôi thép bên trái)
            b.AddCylinder(new Vector3(0.50f, 0.08f, -0.18f), 0.15f, 0.17f, 0.20f, 14);

            // Firewood Log Cord Stack (5 khúc củi xếp gọn bên chân bệ)
            b.AddCylinderOriented(new Vector3(-0.42f, 0.05f, -0.15f), new Vector3(-0.42f, 0.05f, -0.40f), 0.045f, 0.045f, 8);
            b.AddCylinderOriented(new Vector3(-0.52f, 0.05f, -0.15f), new Vector3(-0.52f, 0.05f, -0.40f), 0.045f, 0.045f, 8);
            b.AddCylinderOriented(new Vector3(-0.47f, 0.13f, -0.15f), new Vector3(-0.47f, 0.13f, -0.40f), 0.045f, 0.045f, 8);
            b.AddCylinderOriented(new Vector3(0.49f, 0.05f, -0.38f), new Vector3(0.49f, 0.05f, -0.58f), 0.045f, 0.045f, 8);
            b.AddCylinderOriented(new Vector3(0.45f, 0.13f, -0.38f), new Vector3(0.45f, 0.13f, -0.58f), 0.045f, 0.045f, 8);

            return b.ExportToString("Furnace_Dwarven");
        }

        /// <summary>
        /// Master Artisan Dwarven Workbench 3D Model:
        /// 1. Wood_Stump: Thick beveled oak tabletop, vertical apron wall with timber slats, heavy trestle legs, stretcher beam
        /// 2. Dark_Iron: Front edge protective iron bullnose plate, heavy cast-iron corner brackets, leg shoes
        /// 3. Bronze_Brass: Row of 20 forged bronze decorative rivets along edge plate and corner braces
        /// </summary>
        public static string BuildWorkbenchObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Wood_Stump (Gỗ Sồi Hoàng Gia Nghệ Nhân)
            // ==========================================
            b.SetGroup("Wood_Stump");

            // Main Tabletop Slab (4.74 x 0.12 x 1.50) from y=0.28 to y=0.40
            b.AddBox(new Vector3(0, 0.34f, 0), new Vector3(4.74f, 0.12f, 1.50f));

            // Front Edge Chamfer/Bevel Sub-slab (gờ bo viền vát cạnh mặt trước)
            b.AddBox(new Vector3(0, 0.38f, -0.76f), new Vector3(4.74f, 0.04f, 0.03f));

            // Front Vertical Apron (Vách gỗ đứng ghép nan: 4.70 x 0.68 x 0.06 ở z=-0.71)
            b.AddBox(new Vector3(0, -0.06f, -0.71f), new Vector3(4.70f, 0.68f, 0.06f));

            // 11 Vertical Timber Apron Battens (Các nan gỗ nổi khối tạo nhịp điệu kiến trúc Dwarven)
            for (int i = 0; i < 11; i++)
            {
                float x = -2.20f + i * 0.44f;
                b.AddBox(new Vector3(x, -0.06f, -0.745f), new Vector3(0.045f, 0.66f, 0.025f));
            }

            // Lower Apron Horizontal Skirting Molding (Gờ phào chỉ chân vách trước)
            b.AddBox(new Vector3(0, -0.38f, -0.73f), new Vector3(4.72f, 0.05f, 0.05f));

            // Heavy Left Trestle Leg (Cột chân gỗ sồi dày bên trái: 0.28 x 0.68 x 1.25)
            b.AddBox(new Vector3(-1.85f, -0.06f, 0), new Vector3(0.28f, 0.68f, 1.25f));
            // Heavy Right Trestle Leg (Cột chân gỗ sồi dày bên phải: 0.28 x 0.68 x 1.25)
            b.AddBox(new Vector3(1.85f, -0.06f, 0), new Vector3(0.28f, 0.68f, 1.25f));

            // Bottom Stretcher Beam (Thanh xà giằng chân bàn nối 2 trụ)
            b.AddBox(new Vector3(0, -0.30f, 0), new Vector3(3.42f, 0.12f, 0.12f));

            // ==========================================
            // GROUP 2: Dark_Iron (Nẹp Sắt Đúc Hắc Thiết & Đai Bọc)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Full-Length Front Edge Heavy Armor Guard (Thanh ray thép dẹt đúc bảo vệ mép trước bàn)
            b.AddBox(new Vector3(0, 0.385f, -0.765f), new Vector3(4.74f, 0.035f, 0.025f));

            // Corner Iron Angle Brackets (Nẹp góc hắc thiết 2 bên đầu bàn)
            b.AddBox(new Vector3(-2.36f, 0.34f, -0.74f), new Vector3(0.06f, 0.13f, 0.06f));
            b.AddBox(new Vector3(2.36f, 0.34f, -0.74f), new Vector3(0.06f, 0.13f, 0.06f));
            b.AddBox(new Vector3(-2.36f, 0.34f, 0.74f), new Vector3(0.06f, 0.13f, 0.06f));
            b.AddBox(new Vector3(2.36f, 0.34f, 0.74f), new Vector3(0.06f, 0.13f, 0.06f));

            // Iron Leg Foundation Shoes (Đế sắt bọc chân bàn)
            b.AddBox(new Vector3(-1.85f, -0.385f, 0), new Vector3(0.32f, 0.04f, 1.30f));
            b.AddBox(new Vector3(1.85f, -0.385f, 0), new Vector3(0.32f, 0.04f, 1.30f));

            // ==========================================
            // GROUP 3: Bronze_Brass (Đinh Tán Đồng Hoàng Gia)
            // ==========================================
            b.SetGroup("Bronze_Brass");

            // Row of 20 Gleaming Bronze Rivets along front edge armor plate
            for (int r = 0; r < 20; r++)
            {
                float rx = -2.25f + r * 0.237f;
                b.AddBox(new Vector3(rx, 0.385f, -0.78f), new Vector3(0.022f, 0.022f, 0.012f));
            }

            // Decorative Corner Rosettes
            b.AddBox(new Vector3(-2.35f, 0.34f, -0.765f), new Vector3(0.035f, 0.035f, 0.015f));
            b.AddBox(new Vector3(2.35f, 0.34f, -0.765f), new Vector3(0.035f, 0.035f, 0.015f));

            return b.ExportToString("Workbench_Master");
        }

        /// <summary>
        /// Sculpted 3D Workshop Materials & Artisan Props on Workbench (Heroic Scale & High Mirror Polish):
        /// 1. Steel_Polished: Stack of 3 mirror-polished Damascus steel ingots & master finished dagger blade
        /// 2. Bronze_Brass: Stack of 2 gleaming 24K gold bullion ingots & dagger crossguard/pommel & jeweler hammer head
        /// 3. Ore_Crystal: 5-spire faceted raw glowing arcane crystal ore cluster
        /// 4. Dark_Iron: Ore matrix rock base & smithing tongs & jeweler anvilette
        /// 5. Hot_Ingot: Fresh glowing hot billet clamped in smithing tongs
        /// 6. Wood_Stump: Leather jeweler craft mat & leather dagger grip & jeweler hammer handle
        /// </summary>
        /// <summary>
        /// Sculpted Hero Crafting Props with 6 distinct PBR submesh groups:
        /// 1. Steel_Polished: Stack of 3 pyramid-stacked Damascus steel ingots & masterwork dagger double-edged blade
        /// 2. Bronze_Brass: Stack of 2 solid 24K gold bullion ingots & dagger crossguard & pommel
        /// 3. Wood_Stump: Master leather artisan craft mat & dagger leather handle grip
        /// 4. Ore_Crystal: 5 majestic faceted hexagonal arcane crystal spires glowing with radiant cyan magic
        /// 5. Dark_Iron: Matrix rock base & heavy blacksmith smithing tongs
        /// 6. Hot_Ingot: Glowing hot forged billet clamped securely in tongs jaws
        /// </summary>
        public static string BuildDeskMaterialsObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Steel_Polished (Thép Damascus Mài Bóng Gương & Lưỡi Thần Binh)
            // ==========================================
            b.SetGroup("Steel_Polished");

            // Heroic Stack of 3 Beveled Damascus Steel Ingots in Pyramid (Phía sau bên trái)
            // Ingot 1 (Bottom Left)
            b.AddTrapezoidPrism(new Vector3(-0.17f, 0.038f, 0.05f), new Vector3(0.18f, 0.065f, 0.11f), new Vector3(0.15f, 0.065f, 0.09f), Quaternion.Euler(0, 10f, 0));
            // Ingot 2 (Bottom Right)
            b.AddTrapezoidPrism(new Vector3(-0.04f, 0.038f, 0.04f), new Vector3(0.18f, 0.065f, 0.11f), new Vector3(0.15f, 0.065f, 0.09f), Quaternion.Euler(0, -6f, 0));
            // Ingot 3 (Top Pyramid Row)
            b.AddTrapezoidPrism(new Vector3(-0.11f, 0.098f, 0.045f), new Vector3(0.18f, 0.065f, 0.11f), new Vector3(0.15f, 0.065f, 0.09f), Quaternion.Euler(0, 2f, 0));

            // Stamped Fuller Hallmark on top of Ingot 3 (Khắc dấu triện hoàng gia sắc sảo)
            b.AddBoxOriented(new Vector3(-0.11f, 0.132f, 0.045f), new Vector3(0.12f, 0.007f, 0.025f), Quaternion.Euler(0, 2f, 0));

            // Finished Master Dagger Double-Edged Blade (Lưỡi đoản kiếm nằm ngang phía trước)
            b.AddBox(new Vector3(0.06f, 0.030f, -0.11f), new Vector3(0.18f, 0.015f, 0.048f));
            // Dagger Tip Diamond Pyramid (Mũi kiếm kim cương 4 mặt sắc lẹm)
            b.AddTriangle(new Vector3(0.15f, 0.030f, -0.134f), new Vector3(0.24f, 0.030f, -0.11f), new Vector3(0.15f, 0.038f, -0.11f), new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0, 1));
            b.AddTriangle(new Vector3(0.15f, 0.030f, -0.086f), new Vector3(0.15f, 0.038f, -0.11f), new Vector3(0.24f, 0.030f, -0.11f), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0.5f));
            b.AddTriangle(new Vector3(0.15f, 0.030f, -0.134f), new Vector3(0.15f, 0.022f, -0.11f), new Vector3(0.24f, 0.030f, -0.11f), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0.5f));
            b.AddTriangle(new Vector3(0.15f, 0.030f, -0.086f), new Vector3(0.24f, 0.030f, -0.11f), new Vector3(0.15f, 0.022f, -0.11f), new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0, 1));
            // Blade Blood Fuller Channel (Rãnh xẻ máu bóng gương)
            b.AddBox(new Vector3(0.06f, 0.038f, -0.11f), new Vector3(0.14f, 0.006f, 0.012f));

            // ==========================================
            // GROUP 2: Bronze_Brass (Thỏi Vàng 24K Hoàng Gia & Đốc Kiếm Vàng)
            // ==========================================
            b.SetGroup("Bronze_Brass");

            // Stack of 2 Gleaming 24K Gold Bullion Ingots (Phía trước bên trái, so le)
            b.AddTrapezoidPrism(new Vector3(-0.16f, 0.030f, -0.05f), new Vector3(0.18f, 0.058f, 0.10f), new Vector3(0.15f, 0.058f, 0.08f), Quaternion.Euler(0, 16f, 0));
            b.AddTrapezoidPrism(new Vector3(-0.14f, 0.084f, -0.04f), new Vector3(0.18f, 0.058f, 0.10f), new Vector3(0.15f, 0.058f, 0.08f), Quaternion.Euler(0, -12f, 0));

            // Stamped Royal Treasury Seal on Top Gold Ingot
            b.AddBoxOriented(new Vector3(-0.14f, 0.115f, -0.04f), new Vector3(0.11f, 0.007f, 0.035f), Quaternion.Euler(0, -12f, 0));

            // Dagger Crossguard (Đốc kiếm chữ thập bằng vàng chạm trổ)
            b.AddBoxOriented(new Vector3(-0.04f, 0.030f, -0.11f), new Vector3(0.024f, 0.028f, 0.110f), Quaternion.identity);
            // Dagger Pommel (Núm chuôi kiếm bằng vàng đúc)
            b.AddCylinder(new Vector3(-0.15f, 0.017f, -0.11f), 0.024f, 0.024f, 0.026f, 14);

            // ==========================================
            // GROUP 3: Wood_Stump (Tấm Da Nghệ Nhân & Chuôi Kiếm Da Thuộc)
            // ==========================================
            b.SetGroup("Wood_Stump");

            // Large Master Leather Jeweler Craft Mat (Tấm da thuộc trải bàn nghệ nhân màu nâu ấm hoàng gia)
            b.AddBox(new Vector3(0f, 0.007f, 0f), new Vector3(0.64f, 0.014f, 0.42f));

            // Dagger Handle Grip (Chuôi dao quấn da thuộc)
            b.AddCylinderOriented(new Vector3(-0.14f, 0.030f, -0.11f), new Vector3(-0.04f, 0.030f, -0.11f), 0.017f, 0.018f, 12);

            // ==========================================
            // GROUP 4: Ore_Crystal (Cụm 5 Trụ Tinh Thể Pha Lê Quặng Lục Giác Phát Quang Rực Rỡ)
            // ==========================================
            b.SetGroup("Ore_Crystal");

            // 5 Giant Faceted Hexagonal Arcane Crystal Spires (Phía bên phải, vươn cao tỏa ánh sáng Cyan lung linh)
            // Trụ chính (Cột mẹ - vươn cao chọc trời, đa diện 6 cạnh)
            b.AddCrystalSpire(new Vector3(0.16f, 0.025f, 0.04f), new Vector3(0.17f, 0.38f, 0.04f), 0.055f, 6);
            // Trụ 2 (Nghiêng nhẹ sang trái)
            b.AddCrystalSpire(new Vector3(0.10f, 0.025f, 0.08f), new Vector3(0.07f, 0.28f, 0.09f), 0.042f, 6);
            // Trụ 3 (Nghiêng nhẹ sang phải)
            b.AddCrystalSpire(new Vector3(0.22f, 0.025f, 0.02f), new Vector3(0.25f, 0.30f, 0.01f), 0.044f, 6);
            // Trụ 4 (Phía trước, thấp xinh)
            b.AddCrystalSpire(new Vector3(0.14f, 0.025f, -0.04f), new Vector3(0.13f, 0.22f, -0.06f), 0.036f, 6);
            // Trụ 5 (Phía sau)
            b.AddCrystalSpire(new Vector3(0.20f, 0.025f, 0.10f), new Vector3(0.22f, 0.24f, 0.12f), 0.038f, 6);

            // ==========================================
            // GROUP 5: Dark_Iron (Khối Đá Mẹ & Kìm Rèn Thép Hắc Thiết)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Matrix Rock Base underneath crystals (Bệ đá mẹ dark iron nâng cụm tinh thể)
            b.AddBox(new Vector3(0.16f, 0.025f, 0.04f), new Vector3(0.20f, 0.048f, 0.22f));

            // Blacksmith Smithing Tongs (Kìm rèn thép hắc thiết tôi dầu kẹp phôi nóng)
            b.AddCylinder(new Vector3(0.02f, 0.028f, 0.12f), 0.025f, 0.025f, 0.035f, 12);
            b.AddBoxOriented(new Vector3(0.09f, 0.040f, 0.14f), new Vector3(0.12f, 0.024f, 0.020f), Quaternion.Euler(0, 10f, 0));
            b.AddBoxOriented(new Vector3(0.09f, 0.040f, 0.10f), new Vector3(0.12f, 0.024f, 0.020f), Quaternion.Euler(0, -10f, 0));
            b.AddCylinderOriented(new Vector3(0.00f, 0.035f, 0.13f), new Vector3(-0.24f, 0.030f, 0.16f), 0.015f, 0.012f, 10);
            b.AddCylinderOriented(new Vector3(0.00f, 0.035f, 0.11f), new Vector3(-0.23f, 0.030f, 0.06f), 0.015f, 0.012f, 10);

            // ==========================================
            // GROUP 6: Hot_Ingot (Phôi Thép Nóng Đỏ Rực Lửa Kẹp Trong Kìm)
            // ==========================================
            b.SetGroup("Hot_Ingot");

            // Glowing hot billet clamped in tongs jaws (Phôi thép nóng đỏ rực magma)
            b.AddTrapezoidPrism(new Vector3(0.13f, 0.040f, 0.12f), new Vector3(0.12f, 0.040f, 0.055f), new Vector3(0.10f, 0.040f, 0.045f), Quaternion.Euler(0, 5f, 0));

            return b.ExportToString("Desk_Materials");
        }

        /// <summary>
        /// Sculpted 3D Smithing Sledgehammer with 3 clean submesh groups:
        /// 1. Wood_Stump: Ergonomic ash/oak shaft
        /// 2. Dark_Iron: Heavy octagonal hammerhead block with bronze bands
        /// 3. Steel_Polished: Dual bevelled striking face roundels
        /// </summary>
        public static string BuildHammerObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Wood_Stump (Cán Gỗ)
            // ==========================================
            b.SetGroup("Wood_Stump");
            // Shaft from y=0 to y=0.54 (r=0.024)
            b.AddCylinder(new Vector3(0, 0, 0), 0.024f, 0.022f, 0.54f, 12);

            // ==========================================
            // GROUP 2: Dark_Iron (Đầu Búa Bát Giác Hắc Thiết)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Handle Pommel
            b.AddCylinder(new Vector3(0, -0.02f, 0), 0.035f, 0.035f, 0.025f, 12);

            // Reinforced Collar below eye
            b.AddCylinder(new Vector3(0, 0.46f, 0), 0.036f, 0.036f, 0.04f, 12);

            // Octagonal Sledgehead Main Body (Cross length 0.22, height 0.11, depth 0.11)
            b.AddBox(new Vector3(0, 0.54f, 0), new Vector3(0.20f, 0.10f, 0.10f));

            // Eye wedge top
            b.AddBox(new Vector3(0, 0.605f, 0), new Vector3(0.035f, 0.03f, 0.035f));

            // ==========================================
            // GROUP 3: Steel_Polished (Hai Mặt Đập Thép Tôi)
            // ==========================================
            b.SetGroup("Steel_Polished");

            // Striking Face Left (Horizontal cylinder pointing -X)
            b.AddCylinderOriented(new Vector3(-0.09f, 0.54f, 0), new Vector3(-0.13f, 0.54f, 0), 0.052f, 0.052f, 12);

            // Striking Face Right (Horizontal cylinder pointing +X)
            b.AddCylinderOriented(new Vector3(0.09f, 0.54f, 0), new Vector3(0.13f, 0.54f, 0), 0.048f, 0.048f, 12);

            return b.ExportToString("Hammer_Dwarven");
        }
    }
}
