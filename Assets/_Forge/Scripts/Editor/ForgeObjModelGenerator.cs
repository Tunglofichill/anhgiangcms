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
                    // Lower facet: outward facing normal
                    AddTriangle(baseCenter, midVerts[i], midVerts[next], new Vector2(0.5f, 0f), new Vector2(0f, 0.7f), new Vector2(1f, 0.7f));
                    // Upper facet: outward facing normal
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

            public void AddVoussoirArch(Vector3 center, float innerRadius, float outerRadius, float depth, float startAngle = 0f, float endAngle = 180f, int stonesCount = 9)
            {
                float totalAng = endAngle - startAngle;
                float angPerStone = totalAng / stonesCount;
                float thickness = outerRadius - innerRadius;
                float midR = (innerRadius + outerRadius) * 0.5f;

                for (int i = 0; i < stonesCount; i++)
                {
                    float a0 = startAngle + i * angPerStone;
                    float a1 = a0 + angPerStone;
                    float midA = (a0 + a1) * 0.5f;

                    float radMid = midA * Mathf.Deg2Rad;
                    Vector3 stoneCenter = center + new Vector3(Mathf.Cos(radMid) * midR, Mathf.Sin(radMid) * midR, 0f);

                    float halfAngRad = (angPerStone * 0.5f) * Mathf.Deg2Rad;
                    float wIn = 2f * innerRadius * Mathf.Sin(halfAngRad);
                    float wOut = 2f * outerRadius * Mathf.Sin(halfAngRad);

                    Quaternion rot = Quaternion.Euler(0f, 0f, midA - 90f);
                    Vector3 botSize = new Vector3(wIn * 0.96f, thickness, depth);
                    Vector3 topSize = new Vector3(wOut * 0.96f, thickness, depth);

                    AddTrapezoidPrism(stoneCenter, botSize, topSize, rot);
                }
            }

            public void AddArchPortal(Vector3 center, float innerRadius, float outerRadius, float depth, float startAngle = 0f, float endAngle = 180f, int segments = 16)
            {
                float angStep = (endAngle - startAngle) / segments;
                float halfDepth = depth * 0.5f;

                for (int i = 0; i < segments; i++)
                {
                    float a0 = (startAngle + i * angStep) * Mathf.Deg2Rad;
                    float a1 = (startAngle + (i + 1) * angStep) * Mathf.Deg2Rad;

                    Vector3 inF0 = center + new Vector3(Mathf.Cos(a0) * innerRadius, Mathf.Sin(a0) * innerRadius, -halfDepth);
                    Vector3 inF1 = center + new Vector3(Mathf.Cos(a1) * innerRadius, Mathf.Sin(a1) * innerRadius, -halfDepth);
                    Vector3 outF0 = center + new Vector3(Mathf.Cos(a0) * outerRadius, Mathf.Sin(a0) * outerRadius, -halfDepth);
                    Vector3 outF1 = center + new Vector3(Mathf.Cos(a1) * outerRadius, Mathf.Sin(a1) * outerRadius, -halfDepth);

                    Vector3 inB0 = center + new Vector3(Mathf.Cos(a0) * innerRadius, Mathf.Sin(a0) * innerRadius, halfDepth);
                    Vector3 inB1 = center + new Vector3(Mathf.Cos(a1) * innerRadius, Mathf.Sin(a1) * innerRadius, halfDepth);
                    Vector3 outB0 = center + new Vector3(Mathf.Cos(a0) * outerRadius, Mathf.Sin(a0) * outerRadius, halfDepth);
                    Vector3 outB1 = center + new Vector3(Mathf.Cos(a1) * outerRadius, Mathf.Sin(a1) * outerRadius, halfDepth);

                    Vector2 uv0 = new Vector2(0, 0); Vector2 uv1 = new Vector2(1, 0);
                    Vector2 uv2 = new Vector2(1, 1); Vector2 uv3 = new Vector2(0, 1);

                    // Front face (-Z, facing camera outward)
                    AddQuad(inF0, inF1, outF1, outF0, uv0, uv1, uv2, uv3);
                    // Back face (+Z, facing outward)
                    AddQuad(outB0, outB1, inB1, inB0, uv0, uv1, uv2, uv3);
                    // Inner soffit face (intrados, facing center)
                    AddQuad(inF1, inF0, inB0, inB1, uv0, uv1, uv2, uv3);
                    // Outer extrados face (facing upward/outward)
                    AddQuad(outF0, outF1, outB1, outB0, uv0, uv1, uv2, uv3);
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
                    sb.AppendLine($"o {grp.Key}");
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
        /// Sculpted Dwarven Grand Stone Hearth Furnace with 5 clean submesh groups:
        /// 1. Stone_Furnace: Monumental basalt foundation, monolithic fluted pillars,
        ///    continuous smooth 16-segment curved stone arch portal, cornice mantel shelf, tall chimney flue
        /// 2. Dark_Iron: Hearth apron fender, cast-iron ember grate, keystone wedge, tapered hammered smoke hood, chimney crown
        /// 3. Bronze_Brass: Ornate keystone medallion boss, smoke hood rivet band studs, pillar impost trims
        /// 4. Furnace_Fire: Deep combustion chamber ember bed, incandescent glowing coal nuggets, blazing flame spires
        /// 5. Wood_Stump: Streamlined blacksmith side bellows with leather pleats tucked neatly on the right wall
        /// <summary>
        /// MASTERPIECE DWARVEN ROYAL BASALT & HEAVY IRON GRAND BLAST FORGE (Đại Lò Rèn Đá Núi Lửa Hoàng Gia):
        /// Monumental architecture rising from workshop ground floor (y = 0.00) to flue crown (y = 2.95m):
        /// 1. Stone_Furnace: Massive volcanic basalt foundation plinth from ground up to desk level,
        ///    chiseled bastion pillars, smooth 9-voussoir Romanesque curved stone arch portal, and mantel shelf.
        /// 2. Dark_Iron: Cast-iron hearth ember grate, apron retaining lip, tapered pyramidal hammered smoke hood,
        ///    tall flue chimney stack, Dwarven flared cowl, and bellows air tuyere nozzle.
        /// 3. Bronze_Brass: High-relief 3D Royal Hammer Guild medallion boss, arch impost trims, riveted hood collar band,
        ///    flue trim, and heavy corner foundation armor plates.
        /// 4. Furnace_Fire: Deep combustion chamber ember bed, 8 incandescent magma coal nuggets, burning logs,
        ///    glowing crucible billet, and 3 blazing 3D flame spires.
        /// 5. Wood_Stump: Authentic blacksmith side bellows tucked along right bastion.
        /// </summary>
        public static string BuildFurnaceObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Stone_Furnace (Khối Kiến Trúc Đá Bazan Núi Lửa Đồ Sộ Hoàng Gia)
            // ==========================================
            b.SetGroup("Stone_Furnace");

            // 1. Monumental Stepped Basalt Foundation Plinth from ground (y=0) to hearth sill (y=0.82)
            b.AddBox(new Vector3(0, 0.16f, 0.05f), new Vector3(1.50f, 0.32f, 1.25f)); // Tier 1 Ground Foundation
            b.AddBox(new Vector3(0, 0.46f, 0.02f), new Vector3(1.36f, 0.30f, 1.12f)); // Tier 2 Mid Plinth
            b.AddBox(new Vector3(0, 0.72f, 0.00f), new Vector3(1.24f, 0.22f, 1.02f)); // Tier 3 Hearth Base Sub-slab

            // Front Workstation Hearth Sill Lip (Mặt phiến đá bazan nhô ra phía trước cửa lò ngang tầm mặt bàn)
            b.AddBox(new Vector3(0, 0.82f, -0.42f), new Vector3(0.96f, 0.08f, 0.30f));

            // 2. Massive Volcanic Stone Column Bastions (Hai cột trụ đá bazan nguyên khối vững như thành lũy)
            // Left Column Bastion (x = -0.42)
            b.AddBox(new Vector3(-0.42f, 0.94f, -0.06f), new Vector3(0.28f, 0.16f, 0.82f)); // Socle plinth
            b.AddBox(new Vector3(-0.42f, 1.28f, -0.06f), new Vector3(0.24f, 0.52f, 0.78f)); // Shaft pillar
            b.AddBox(new Vector3(-0.42f, 1.58f, -0.06f), new Vector3(0.28f, 0.08f, 0.82f)); // Capital impost

            // Right Column Bastion (x = +0.42)
            b.AddBox(new Vector3(0.42f, 0.94f, -0.06f), new Vector3(0.28f, 0.16f, 0.82f)); // Socle plinth
            b.AddBox(new Vector3(0.42f, 1.28f, -0.06f), new Vector3(0.24f, 0.52f, 0.78f)); // Shaft pillar
            b.AddBox(new Vector3(0.42f, 1.58f, -0.06f), new Vector3(0.28f, 0.08f, 0.82f)); // Capital impost

            // 3. Thick Refractory Rear Basalt Wall (Vách đá bazan chịu nhiệt dày che kín phía sau)
            b.AddBox(new Vector3(0, 1.28f, 0.34f), new Vector3(1.08f, 0.82f, 0.28f));

            // 4. THE 9-VOUSSOIR ROMANESQUE STONE ARCH (Cổng vòm đá bán nguyệt 9 viên đá nêm tuyệt mỹ)
            // Center = (0, 1.58, -0.06), Inner Radius = 0.30, Outer Radius = 0.48, Depth = 0.78
            b.AddVoussoirArch(new Vector3(0, 1.58f, -0.06f), 0.30f, 0.48f, 0.78f, 0f, 180f, 9);

            // Keystone Wedge (Viên đá khóa đỉnh vòm nhô cao uy nghi)
            b.AddTrapezoidPrism(new Vector3(0, 2.02f, -0.06f), new Vector3(0.22f, 0.18f, 0.82f), new Vector3(0.28f, 0.18f, 0.82f), Quaternion.identity);

            // Spandrels / Haunches Infill (Khối đá vai vòm ôm 2 bên vòm)
            b.AddBox(new Vector3(-0.40f, 1.82f, -0.06f), new Vector3(0.22f, 0.44f, 0.78f));
            b.AddBox(new Vector3(0.40f, 1.82f, -0.06f), new Vector3(0.22f, 0.44f, 0.78f));
            b.AddBox(new Vector3(0, 2.04f, -0.06f), new Vector3(0.60f, 0.08f, 0.78f));

            // 5. Heavy Chiseled Stone Cornice Mantel Shelf (Gờ phào mái đá nâng đỡ nón khói)
            b.AddBox(new Vector3(0, 2.12f, -0.04f), new Vector3(1.18f, 0.06f, 0.90f));
            b.AddBox(new Vector3(0, 2.16f, -0.04f), new Vector3(1.10f, 0.04f, 0.84f));

            // ==========================================
            // GROUP 2: Dark_Iron (Nón Hút Khói, Ống Khói & Chi Tiết Sắt Đúc Hắc Thiết)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // 1. Cast-iron Combustion Floor Grate (5 thanh vỉ sắt đúc chịu nhiệt trong buồng than)
            for (int i = -2; i <= 2; i++)
            {
                b.AddBox(new Vector3(i * 0.10f, 0.88f, 0.00f), new Vector3(0.04f, 0.035f, 0.52f));
            }

            // Front Retaining Iron Flange (Thanh sắt nẹp viền bảo vệ mặt ngưỡng lò)
            b.AddBox(new Vector3(0, 0.89f, -0.55f), new Vector3(0.96f, 0.032f, 0.042f));

            // 2. TAPERED PYRAMIDAL HAMMERED SMOKE HOOD (Nón hút khói sắt rèn thu côn mềm mại)
            b.AddTrapezoidPrism(new Vector3(0, 2.34f, -0.02f), new Vector3(1.04f, 0.32f, 0.80f), new Vector3(0.52f, 0.32f, 0.48f), Quaternion.identity);

            // 3. FLUE SHAFT STACK & CROWN COWL (Cổ ống khói tròn vươn cao thanh thoát)
            b.AddCylinder(new Vector3(0, 2.50f, -0.02f), 0.24f, 0.22f, 0.35f, 24);
            b.AddCylinder(new Vector3(0, 2.70f, -0.02f), 0.25f, 0.25f, 0.05f, 24); // Mid reinforcing band
            b.AddCylinder(new Vector3(0, 2.85f, -0.02f), 0.22f, 0.32f, 0.10f, 24); // Flared Crown
            b.AddCylinder(new Vector3(0, 2.95f, -0.02f), 0.34f, 0.28f, 0.04f, 24); // Cap cowl

            // Blast Air Tuyere Nozzle from Bellows
            b.AddCylinderOriented(new Vector3(-0.52f, 1.10f, 0.02f), new Vector3(-0.26f, 1.02f, 0.02f), 0.040f, 0.024f, 12);

            // ==========================================
            // GROUP 3: Bronze_Brass (Phù Hiệu Búa Hoàng Gia 3D Nổi Khối, Nẹp Vàng Đồng & Đinh Tán)
            // ==========================================
            b.SetGroup("Bronze_Brass");

            // High-Relief 3D Royal Hammer Guild Medallion (Huy hiệu Song Búa Hoàng Gia mạ vàng 24K nhô hẳn ra ngoài vòm đá)
            b.AddCylinderOriented(new Vector3(0, 2.02f, -0.47f), new Vector3(0, 2.02f, -0.54f), 0.17f, 0.16f, 24); // Huy hiệu tròn nổi 3D
            b.AddCylinderOriented(new Vector3(0, 2.02f, -0.54f), new Vector3(0, 2.02f, -0.56f), 0.165f, 0.155f, 24); // Vành nẹp viền tròn đúc gờ nổi
            // Song búa hoàng gia bắt chéo nhau đúc vàng nổi 3D
            b.AddBoxOriented(new Vector3(0, 2.02f, -0.575f), new Vector3(0.032f, 0.24f, 0.032f), Quaternion.Euler(0, 0, 45f)); // Cán búa 1
            b.AddBoxOriented(new Vector3(0, 2.02f, -0.575f), new Vector3(0.032f, 0.24f, 0.032f), Quaternion.Euler(0, 0, -45f)); // Cán búa 2
            b.AddBoxOriented(new Vector3(0.080f, 2.100f, -0.590f), new Vector3(0.090f, 0.060f, 0.045f), Quaternion.Euler(0, 0, 45f)); // Đầu búa 1
            b.AddBoxOriented(new Vector3(-0.080f, 2.100f, -0.590f), new Vector3(0.090f, 0.060f, 0.045f), Quaternion.Euler(0, 0, -45f)); // Đầu búa 2

            // Left & Right Arch Impost Bronze Trims
            b.AddBox(new Vector3(-0.42f, 1.58f, -0.47f), new Vector3(0.26f, 0.042f, 0.062f));
            b.AddBox(new Vector3(0.42f, 1.58f, -0.47f), new Vector3(0.26f, 0.042f, 0.062f));

            // Smoke Hood Lower & Mid Bronze Collar Bands
            b.AddBox(new Vector3(0, 2.19f, -0.43f), new Vector3(1.06f, 0.035f, 0.035f));
            b.AddBox(new Vector3(-0.53f, 2.19f, -0.02f), new Vector3(0.035f, 0.035f, 0.82f));
            b.AddBox(new Vector3(0.53f, 2.19f, -0.02f), new Vector3(0.035f, 0.035f, 0.82f));
            b.AddBox(new Vector3(0, 2.35f, -0.27f), new Vector3(0.78f, 0.030f, 0.030f)); // Mid accent band

            // 16 Bronze Rivet Studs around Smoke Hood Collar
            for (int r = 0; r < 16; r++)
            {
                float ra = r * Mathf.PI * 2f / 16f;
                b.AddBoxOriented(new Vector3(Mathf.Cos(ra) * 0.28f, 2.48f, Mathf.Sin(ra) * 0.25f),
                    new Vector3(0.028f, 0.028f, 0.028f), Quaternion.Euler(0, -ra * Mathf.Rad2Deg, 0));
            }

            // Foundation Corner Armor Brackets at ground level
            b.AddBox(new Vector3(-0.68f, 0.16f, -0.52f), new Vector3(0.08f, 0.16f, 0.08f));
            b.AddBox(new Vector3(0.68f, 0.16f, -0.52f), new Vector3(0.08f, 0.16f, 0.08f));

            // ==========================================
            // GROUP 4: Furnace_Fire (Trái Tim Lò Rèn - Lòng Than Hồng & Lửa Magma Rực Rỡ)
            // ==========================================
            b.SetGroup("Furnace_Fire");

            // Deep Combustion Core Ember Bed (Lòng than hồng nằm sâu trong buồng lò ngang tầm thềm)
            b.AddBox(new Vector3(0, 0.90f, 0.04f), new Vector3(0.62f, 0.08f, 0.54f));

            // Multi-Cluster Irregular Coal Nuggets (8 khối than đá magma nóng đỏ gối lên nhau)
            b.AddBoxOriented(new Vector3(-0.16f, 0.95f, -0.04f), new Vector3(0.16f, 0.10f, 0.16f), Quaternion.Euler(12f, 25f, -10f));
            b.AddBoxOriented(new Vector3(0.15f, 0.96f, -0.02f), new Vector3(0.15f, 0.10f, 0.15f), Quaternion.Euler(-15f, -30f, 12f));
            b.AddBoxOriented(new Vector3(-0.08f, 0.98f, 0.12f), new Vector3(0.15f, 0.10f, 0.15f), Quaternion.Euler(20f, 10f, 5f));
            b.AddBoxOriented(new Vector3(0.11f, 0.97f, 0.14f), new Vector3(0.15f, 0.10f, 0.15f), Quaternion.Euler(-10f, 40f, -8f));
            b.AddBoxOriented(new Vector3(0f, 0.99f, 0.04f), new Vector3(0.22f, 0.11f, 0.20f), Quaternion.Euler(5f, -15f, 10f));
            b.AddBoxOriented(new Vector3(-0.18f, 0.94f, 0.07f), new Vector3(0.13f, 0.09f, 0.13f), Quaternion.Euler(8f, 18f, -6f));
            b.AddBoxOriented(new Vector3(0.18f, 0.94f, 0.06f), new Vector3(0.13f, 0.09f, 0.13f), Quaternion.Euler(-12f, -15f, 14f));
            b.AddBoxOriented(new Vector3(0f, 0.96f, -0.09f), new Vector3(0.15f, 0.09f, 0.13f), Quaternion.Euler(6f, -10f, 4f));

            // Burning Oak Split Logs (2 khúc củi sồi cổ thụ cháy đỏ rực bắc chéo trong buồng đốt)
            b.AddCylinderOriented(new Vector3(-0.17f, 0.94f, -0.08f), new Vector3(0.14f, 1.05f, 0.11f), 0.045f, 0.038f, 8);
            b.AddCylinderOriented(new Vector3(0.16f, 0.95f, -0.06f), new Vector3(-0.13f, 1.06f, 0.09f), 0.042f, 0.038f, 8);

            // Rising Blazing Flame Spires (3 ngọn lửa 3D vuốt nhọn rực sáng vươn cao trong buồng lò)
            b.AddCrystalSpire(new Vector3(0, 1.02f, 0.04f), new Vector3(0, 1.42f, 0.04f), 0.095f, 6);
            b.AddCrystalSpire(new Vector3(-0.12f, 1.00f, 0.02f), new Vector3(-0.10f, 1.32f, 0.02f), 0.065f, 5);
            b.AddCrystalSpire(new Vector3(0.11f, 1.00f, 0.05f), new Vector3(0.10f, 1.33f, 0.05f), 0.065f, 5);

            // ==========================================
            // GROUP 5: Hot_Ingot (Phôi Thép Nung Đỏ Magma Đang Ủ Nhiệt Trong Lò Rèn)
            // ==========================================
            b.SetGroup("Hot_Ingot");

            // Red-hot glowing sword blank resting on embers and extending slightly onto hearth sill
            b.AddTrapezoidPrism(new Vector3(0f, 0.98f, -0.20f), new Vector3(0.14f, 0.050f, 0.30f), new Vector3(0.12f, 0.050f, 0.28f), Quaternion.identity);

            // ==========================================
            // GROUP 6: Wood_Stump (Bễ Thổi Da & Gỗ Nép Gọn Sườn Phải)
            // ==========================================
            b.SetGroup("Wood_Stump");

            // Blacksmith Bellows Assembly on outer wall (-X in OBJ = +X in Unity)
            b.AddBoxOriented(new Vector3(-0.58f, 1.25f, 0.02f), new Vector3(0.14f, 0.030f, 0.36f), Quaternion.Euler(0, -10f, 6f));
            b.AddBoxOriented(new Vector3(-0.58f, 1.15f, 0.02f), new Vector3(0.12f, 0.16f, 0.32f), Quaternion.Euler(0, -10f, 6f));
            b.AddBoxOriented(new Vector3(-0.58f, 1.05f, 0.02f), new Vector3(0.14f, 0.030f, 0.36f), Quaternion.Euler(0, -10f, 6f));

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
            // Note: Unity FBX/OBJ importer inverts X axis (Unity_X = -OBJ_X).
            // We want Workbench in Unity from X = -2.40m to X = +1.00m (Center = -0.70m, Length = 3.40m).
            // Therefore, in OBJ, Center is at X = +0.70m, spanning OBJ X = -1.00m to +2.40m.
            // ==========================================
            b.SetGroup("Wood_Stump");

            // Main Tabletop Slab (3.40 x 0.12 x 1.50)
            b.AddBox(new Vector3(0.70f, 0.34f, 0), new Vector3(3.40f, 0.12f, 1.50f));

            // Front Edge Chamfer/Bevel Sub-slab (gờ bo viền vát cạnh mặt trước)
            b.AddBox(new Vector3(0.70f, 0.38f, -0.76f), new Vector3(3.40f, 0.04f, 0.03f));

            // Front Vertical Apron (Vách gỗ đứng ghép nan: 3.38 x 0.68 x 0.06 ở z=-0.71)
            b.AddBox(new Vector3(0.70f, -0.06f, -0.71f), new Vector3(3.38f, 0.68f, 0.06f));

            // 11 Vertical Timber Apron Battens (Các nan gỗ nổi khối tạo nhịp điệu kiến trúc Dwarven)
            for (int i = 0; i < 11; i++)
            {
                float x = -0.75f + i * 0.29f;
                b.AddBox(new Vector3(x, -0.06f, -0.745f), new Vector3(0.045f, 0.66f, 0.025f));
            }

            // Lower Apron Horizontal Skirting Molding (Gờ phào chỉ chân vách trước)
            b.AddBox(new Vector3(0.70f, -0.38f, -0.73f), new Vector3(3.38f, 0.05f, 0.05f));

            // Heavy Left Trestle Leg (Unity X = -1.80m => OBJ X = +1.80m)
            b.AddBox(new Vector3(1.80f, -0.06f, 0), new Vector3(0.28f, 0.68f, 1.25f));
            // Heavy Right Trestle Leg (Unity X = +0.50m => OBJ X = -0.50m)
            b.AddBox(new Vector3(-0.50f, -0.06f, 0), new Vector3(0.28f, 0.68f, 1.25f));

            // Bottom Stretcher Beam (Thanh xà giằng chân bàn nối 2 trụ, OBJ Center = +0.65m)
            b.AddBox(new Vector3(0.65f, -0.30f, 0), new Vector3(2.30f, 0.12f, 0.12f));

            // ==========================================
            // GROUP 2: Dark_Iron (Nẹp Sắt Đúc Hắc Thiết & Đai Bọc)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Full-Length Front Edge Heavy Armor Guard (Thanh ray thép dẹt đúc bảo vệ mép trước bàn)
            b.AddBox(new Vector3(0.70f, 0.385f, -0.765f), new Vector3(3.40f, 0.035f, 0.025f));

            // Corner Iron Angle Brackets (Nẹp góc hắc thiết 2 bên đầu bàn)
            b.AddBox(new Vector3(2.39f, 0.34f, -0.74f), new Vector3(0.06f, 0.13f, 0.06f)); // Left corner in Unity
            b.AddBox(new Vector3(-0.99f, 0.34f, -0.74f), new Vector3(0.06f, 0.13f, 0.06f)); // Right corner in Unity
            b.AddBox(new Vector3(2.39f, 0.34f, 0.74f), new Vector3(0.06f, 0.13f, 0.06f));
            b.AddBox(new Vector3(-0.99f, 0.34f, 0.74f), new Vector3(0.06f, 0.13f, 0.06f));

            // Iron Leg Foundation Shoes (Đế sắt bọc chân bàn)
            b.AddBox(new Vector3(1.80f, -0.385f, 0), new Vector3(0.32f, 0.04f, 1.30f));
            b.AddBox(new Vector3(-0.50f, -0.385f, 0), new Vector3(0.32f, 0.04f, 1.30f));

            // ==========================================
            // GROUP 3: Bronze_Brass (Đinh Tán Đồng Hoàng Gia)
            // ==========================================
            b.SetGroup("Bronze_Brass");

            // Row of 15 Gleaming Bronze Rivets along front edge armor plate
            for (int r = 0; r < 15; r++)
            {
                float rx = -0.90f + r * 0.228f;
                b.AddBox(new Vector3(rx, 0.385f, -0.78f), new Vector3(0.022f, 0.022f, 0.012f));
            }

            // Decorative Corner Rosettes
            b.AddBox(new Vector3(2.38f, 0.34f, -0.765f), new Vector3(0.035f, 0.035f, 0.015f));
            b.AddBox(new Vector3(-0.98f, 0.34f, -0.765f), new Vector3(0.035f, 0.035f, 0.015f));

            return b.ExportToString("Workbench_Master");
        }

        /// <summary>
        /// Sculpted 3D Workshop Materials & Artisan Props on Workbench (Heroic Scale & High Mirror Polish):
        /// <summary>
        /// Sculpted 3D Polished Workshop Materials & Crafted Relics on Jeweler Leather Mat (Artisan Spaced Layout):
        /// 1. Leather_Mat: Master hand-stitched jeweler leather mat with beveled borders
        /// 2. Ore_Crystal: 5 faceted hexagonal cyan arcane mana crystal spires on dark matrix rock
        /// 3. Steel_Polished: Pyramid of 3 mirror-polished Damascus steel ingots with stamped hallmarks, and master dagger blade
        /// 4. Bronze_Brass: Stack of 2 gleaming 24K gold bullion ingots, dagger crossguard & pommel
        /// 5. Dark_Iron: Matrix rock base, dagger display rests, and blacksmith tongs
        /// 6. Hot_Ingot: Glowing hot forged billet clamped in tongs
        /// </summary>
        /// <summary>
        /// MASTER ARTISAN SPACED WORKBENCH PROPS (4 Cụm Trưng Bày Nghệ Nhân Thoáng Đãng):
        /// <summary>
        /// MASTER ARTISAN SPACED WORKBENCH PROPS (4 Cụm Trưng Bày Nghệ Nhân Thoáng Đãng - Không Dính Nhau):
        /// 1. Cluster 1 (Left, near Anvil, Unity X = -0.45m / OBJ X = +0.45m):
        ///    - Stone_Furnace: Volcanic basalt rock plinth
        ///    - Ore_Crystal: 5 majestic radiant cyan arcane mana crystal spires
        ///    - Bronze_Brass: 4 corner reinforcement brackets
        /// 2. Cluster 2 (Center, Unity X = -0.05m / OBJ X = +0.05m):
        ///    - Wood_Stump: Master crafted oak sword display stand elevating dagger 8cm off desk
        ///    - Steel_Polished: Dragon Dagger blade & 2 folded Damascus steel ingots
        ///    - Bronze_Brass: Dragon crossguard & pommel
        ///    - Leather_Mat: Burgundy leather grip wrap
        /// 3. Cluster 3 (Mid-Right, Unity X = +0.32m / OBJ X = -0.32m):
        ///    - Bronze_Brass: Pyramid of 3 large 24K pure gold bullion bars with Royal Treasury seal
        /// 4. Cluster 4 (Far-Right, near Furnace, Unity X = +0.65m / OBJ X = -0.65m):
        ///    - Dark_Iron: Blacksmith articulated smith tongs
        ///    - Hot_Ingot: Glowing red-hot magma forged billet clamped in jaws
        /// </summary>
        public static string BuildDeskMaterialsObj()
        {
            ObjBuilder b = new ObjBuilder();

            // ==========================================
            // GROUP 1: Stone_Furnace (Bệ Đá Mẹ Cụm Pha Lê - OBJ X = +0.65m / Unity X = -0.65m)
            // ==========================================
            b.SetGroup("Stone_Furnace");

            // Rugged Volcanic Basalt Geode Matrix Rock Base (Bệ đá bazan mẹ nâng cụm pha lê)
            b.AddBox(new Vector3(0.65f, 0.020f, 0.00f), new Vector3(0.28f, 0.040f, 0.26f));

            // ==========================================
            // GROUP 2: Ore_Crystal (Cụm 5 Cột Pha Lê Arcane Ngọc Bích - OBJ X = +0.65m / Unity X = -0.65m)
            // ==========================================
            b.SetGroup("Ore_Crystal");

            // Central mother crystal spire (Towering, vibrant cyan, height = 0.44m, r = 0.060m)
            b.AddCrystalSpire(new Vector3(0.65f, 0.038f, 0.00f), new Vector3(0.65f, 0.44f, 0.00f), 0.060f, 6);
            // 4 satellite crystal spires radiating outwards
            b.AddCrystalSpire(new Vector3(0.72f, 0.038f, -0.03f), new Vector3(0.76f, 0.32f, -0.04f), 0.040f, 6);
            b.AddCrystalSpire(new Vector3(0.58f, 0.038f, 0.03f), new Vector3(0.54f, 0.34f, 0.04f), 0.042f, 6);
            b.AddCrystalSpire(new Vector3(0.67f, 0.038f, 0.07f), new Vector3(0.69f, 0.29f, 0.09f), 0.038f, 6);
            b.AddCrystalSpire(new Vector3(0.60f, 0.038f, -0.07f), new Vector3(0.57f, 0.27f, -0.08f), 0.035f, 6);

            // 3 Small crystal ore shards around matrix base
            b.AddBoxOriented(new Vector3(0.53f, 0.012f, -0.09f), new Vector3(0.035f, 0.026f, 0.035f), Quaternion.Euler(15f, 30f, -20f));
            b.AddBoxOriented(new Vector3(0.77f, 0.012f, 0.08f), new Vector3(0.032f, 0.024f, 0.032f), Quaternion.Euler(-10f, 45f, 15f));
            b.AddBoxOriented(new Vector3(0.70f, 0.012f, -0.10f), new Vector3(0.030f, 0.022f, 0.030f), Quaternion.Euler(25f, -15f, 10f));

            // ==========================================
            // GROUP 3: Wood_Stump (Giá Đỡ Thần Binh Gỗ Sồi Nghệ Nhân - OBJ X = +0.15m / Unity X = -0.15m)
            // ==========================================
            b.SetGroup("Wood_Stump");

            // Master Crafted Oak Sword Display Stand (Nâng đoản kiếm lên cao khỏi mặt bàn 10cm, nổi khối 3D)
            // Base plinth
            b.AddBox(new Vector3(0.15f, 0.018f, -0.06f), new Vector3(0.40f, 0.036f, 0.14f));
            // Twin V-notched upright support pillars
            b.AddBox(new Vector3(0.02f, 0.065f, -0.06f), new Vector3(0.036f, 0.090f, 0.080f)); // Left pillar
            b.AddBox(new Vector3(0.28f, 0.065f, -0.06f), new Vector3(0.036f, 0.090f, 0.080f)); // Right pillar

            // ==========================================
            // GROUP 4: Leather_Mat (Chuôi Kiếm Quấn Da Thuộc Burgundy - OBJ X = +0.15m / Unity X = -0.15m)
            // ==========================================
            b.SetGroup("Leather_Mat");

            // Dagger Leather Handle Grip (Chuôi kiếm quấn da thuộc đỏ burgundy ở y = 0.105m)
            b.AddCylinderOriented(new Vector3(0.30f, 0.105f, -0.06f), new Vector3(0.17f, 0.105f, -0.06f), 0.017f, 0.018f, 12);

            // ==========================================
            // GROUP 5: Steel_Polished (Đoản Kiếm Thần Binh & Thép Damascus - OBJ X = +0.15m / Unity X = -0.15m)
            // ==========================================
            b.SetGroup("Steel_Polished");

            // 1. Mastercraft Dragon Dagger Blade (Lưỡi kiếm Damascus cuộn sóng nâng cao y = 0.105m)
            b.AddBox(new Vector3(0.05f, 0.105f, -0.06f), new Vector3(0.22f, 0.015f, 0.050f));
            // Blade Blood Fuller Channel
            b.AddBox(new Vector3(0.06f, 0.113f, -0.06f), new Vector3(0.16f, 0.005f, 0.012f));
            // Dagger Tip Diamond Pyramid (Mũi dao kim cương 4 mặt vát nhọn)
            b.AddTriangle(new Vector3(-0.060f, 0.105f, -0.085f), new Vector3(-0.160f, 0.105f, -0.060f), new Vector3(-0.060f, 0.113f, -0.060f), new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0, 1));
            b.AddTriangle(new Vector3(-0.060f, 0.105f, -0.035f), new Vector3(-0.060f, 0.113f, -0.060f), new Vector3(-0.160f, 0.105f, -0.060f), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0.5f));
            b.AddTriangle(new Vector3(-0.060f, 0.105f, -0.085f), new Vector3(-0.060f, 0.097f, -0.060f), new Vector3(-0.160f, 0.105f, -0.060f), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0.5f));
            b.AddTriangle(new Vector3(-0.060f, 0.105f, -0.035f), new Vector3(-0.160f, 0.105f, -0.060f), new Vector3(-0.060f, 0.097f, -0.060f), new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0, 1));

            // 2. Stack of 2 Folded Damascus Steel Billets (2 thỏi thép Damascus gối so le sau giá kiếm)
            b.AddTrapezoidPrism(new Vector3(0.15f, 0.026f, 0.09f), new Vector3(0.18f, 0.046f, 0.085f), new Vector3(0.16f, 0.046f, 0.070f), Quaternion.identity);
            b.AddTrapezoidPrism(new Vector3(0.15f, 0.070f, 0.09f), new Vector3(0.16f, 0.042f, 0.075f), new Vector3(0.14f, 0.042f, 0.060f), Quaternion.identity);
            // Stamped Fuller Hallmark on top Damascus billet
            b.AddBox(new Vector3(0.15f, 0.093f, 0.09f), new Vector3(0.10f, 0.006f, 0.024f));

            // ==========================================
            // GROUP 6: Bronze_Brass (Tháp 3 Thỏi Vàng 24K Đồ Sộ & Đốc Kiếm - OBJ X = -0.30m / Unity X = +0.30m)
            // ==========================================
            b.SetGroup("Bronze_Brass");

            // 1. Pyramid of 3 Heavy 24K Gold Bullion Bars (Tháp vàng 24K đồ sộ độc lập hoàn toàn ở X = -0.30m)
            // Ingot 1 (Bottom Right: 20 x 9 x 5 cm)
            b.AddTrapezoidPrism(new Vector3(-0.24f, 0.028f, -0.01f), new Vector3(0.19f, 0.050f, 0.092f), new Vector3(0.16f, 0.050f, 0.074f), Quaternion.identity);
            // Ingot 2 (Bottom Left, aesthetic 6 deg tilt)
            b.AddTrapezoidPrism(new Vector3(-0.36f, 0.028f, -0.07f), new Vector3(0.19f, 0.050f, 0.092f), new Vector3(0.16f, 0.050f, 0.074f), Quaternion.Euler(0, 6f, 0));
            // Ingot 3 (Top Crown, centered across the two lower bars)
            b.AddTrapezoidPrism(new Vector3(-0.30f, 0.076f, -0.04f), new Vector3(0.19f, 0.050f, 0.092f), new Vector3(0.16f, 0.050f, 0.074f), Quaternion.Euler(0, -3f, 0));
            // Deep Stamped Royal Treasury Hallmark Seal on top of gold bar
            b.AddBoxOriented(new Vector3(-0.30f, 0.103f, -0.04f), new Vector3(0.12f, 0.006f, 0.038f), Quaternion.Euler(0, -3f, 0));

            // 2. Dragon Dagger Crossguard & Pommel on elevated stand (y = 0.105m)
            b.AddBox(new Vector3(0.165f, 0.105f, -0.06f), new Vector3(0.022f, 0.036f, 0.110f)); // Crossguard
            b.AddCylinder(new Vector3(0.315f, 0.092f, -0.06f), 0.024f, 0.024f, 0.028f, 12); // Pommel

            // 3. Four corner bronze brackets on arcane stone plinth (X = +0.65m)
            b.AddBox(new Vector3(0.51f, 0.020f, -0.13f), new Vector3(0.035f, 0.044f, 0.035f));
            b.AddBox(new Vector3(0.79f, 0.020f, -0.13f), new Vector3(0.035f, 0.044f, 0.035f));
            b.AddBox(new Vector3(0.51f, 0.020f, 0.13f), new Vector3(0.035f, 0.044f, 0.035f));
            b.AddBox(new Vector3(0.79f, 0.020f, 0.13f), new Vector3(0.035f, 0.044f, 0.035f));

            // ==========================================
            // GROUP 7: Dark_Iron (Kìm Rèn Nghệ Nhân Hắc Thiết - OBJ X = -0.76m / Unity X = +0.76m)
            // ==========================================
            b.SetGroup("Dark_Iron");

            // Master Blacksmith Smithing Tongs (Kìm rèn cỡ lớn nép gần mép bàn hướng về lò rèn, OBJ X = -0.76m)
            // Pivot round joint
            b.AddCylinder(new Vector3(-0.76f, 0.030f, -0.02f), 0.025f, 0.025f, 0.035f, 12);
            // Twin long forged handles resting on desk
            b.AddCylinderOriented(new Vector3(-0.75f, 0.034f, -0.01f), new Vector3(-0.71f, 0.030f, 0.24f), 0.013f, 0.010f, 8);
            b.AddCylinderOriented(new Vector3(-0.77f, 0.034f, -0.01f), new Vector3(-0.81f, 0.030f, 0.24f), 0.013f, 0.010f, 8);
            // Heavy clamping jaws extending toward front
            b.AddBoxOriented(new Vector3(-0.755f, 0.038f, -0.09f), new Vector3(0.018f, 0.024f, 0.110f), Quaternion.Euler(0, 4f, 0));
            b.AddBoxOriented(new Vector3(-0.765f, 0.038f, -0.09f), new Vector3(0.018f, 0.024f, 0.110f), Quaternion.Euler(0, -4f, 0));

            // ==========================================
            // GROUP 8: Hot_Ingot (Phôi Sắt Nung Đỏ Magma Kẹp Trong Kìm)
            // ==========================================
            b.SetGroup("Hot_Ingot");

            // Glowing hot forged iron billet clamped tightly in tongs jaws
            b.AddTrapezoidPrism(new Vector3(-0.76f, 0.040f, -0.14f), new Vector3(0.100f, 0.050f, 0.070f), new Vector3(0.086f, 0.050f, 0.056f), Quaternion.identity);

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
