using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Runtime mesh factory for the domes, bowls, rings and cones that the
    /// spherical chapter needs. Meshes are unit sized (radius 0.5) and scaled by
    /// the visual builder, so one mesh serves every game.
    /// </summary>
    public static class ProceduralMesh
    {
        private static Mesh _dome;
        private static Mesh _bowl;
        private static Mesh _torus;
        private static Mesh _ring;
        private static Mesh _cone;
        private static Mesh _quad;

        public static Mesh Dome => _dome != null ? _dome : (_dome = BuildHemisphere(false, 28, 44));
        public static Mesh Bowl => _bowl != null ? _bowl : (_bowl = BuildHemisphere(true, 28, 44));
        public static Mesh Torus => _torus != null ? _torus : (_torus = BuildTorus(0.5f, 0.055f, 12, 40));
        public static Mesh Ring => _ring != null ? _ring : (_ring = BuildTorus(0.5f, 0.012f, 8, 48));
        public static Mesh Cone => _cone != null ? _cone : (_cone = BuildCone(12));
        public static Mesh Quad => _quad != null ? _quad : (_quad = BuildQuad());

        public static Mesh BuildHemisphere(bool bowl, int latSteps, int lonSteps)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();

            float rOuter = 0.5f;
            float rInner = 0.46f;
            float sign = bowl ? -1f : 1f;

            AddShell(verts, tris, uvs, rOuter, sign, latSteps, lonSteps, 0.002f);
            AddShell(verts, tris, uvs, rInner, -sign, latSteps, lonSteps, -0.002f);
            AddRim(verts, tris, uvs, rOuter, rInner, sign, lonSteps);

            var mesh = new Mesh { name = bowl ? "BowlMesh" : "DomeMesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddShell(List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            float radius, float sign, int latSteps, int lonSteps, float normalBias)
        {
            int start = verts.Count;
            for (int i = 0; i <= latSteps; i++)
            {
                float v = i / (float)latSteps;
                float lat = v * Mathf.PI * 0.5f;
                float y = Mathf.Sin(lat) * radius * sign;
                float ring = Mathf.Cos(lat) * radius;
                for (int j = 0; j <= lonSteps; j++)
                {
                    float u = j / (float)lonSteps;
                    float phi = u * Mathf.PI * 2f;
                    Vector3 p = new Vector3(Mathf.Cos(phi) * ring, y + normalBias, Mathf.Sin(phi) * ring);
                    verts.Add(p);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int i = 0; i < latSteps; i++)
            {
                for (int j = 0; j < lonSteps; j++)
                {
                    int a = start + i * (lonSteps + 1) + j;
                    int b = a + lonSteps + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                    // Double sided: shells are visible from both the play side
                    // and through the mouth of bowls.
                    tris.Add(a); tris.Add(a + 1); tris.Add(b);
                    tris.Add(a + 1); tris.Add(b + 1); tris.Add(b);
                }
            }
        }

        private static void AddRim(List<Vector3> verts, List<int> tris, List<Vector2> uvs,
            float rOuter, float rInner, float sign, int lonSteps)
        {
            int start = verts.Count;
            for (int j = 0; j <= lonSteps; j++)
            {
                float u = j / (float)lonSteps;
                float phi = u * Mathf.PI * 2f;
                float c = Mathf.Cos(phi);
                float s = Mathf.Sin(phi);
                verts.Add(new Vector3(c * rOuter, 0f, s * rOuter));
                verts.Add(new Vector3(c * rInner, 0f, s * rInner));
                verts.Add(new Vector3(c * rOuter, 0.012f * sign, s * rOuter));
                verts.Add(new Vector3(c * rInner, 0.012f * sign, s * rInner));
                uvs.Add(new Vector2(u, 0)); uvs.Add(new Vector2(u, 0.2f));
                uvs.Add(new Vector2(u, 0.4f)); uvs.Add(new Vector2(u, 0.6f));
            }

            for (int j = 0; j < lonSteps; j++)
            {
                int b = start + j * 4;
                int n = b + 4;
                AddQuad(tris, b + 0, b + 1, n + 1, n + 0);
                AddQuad(tris, b + 2, b + 3, n + 3, n + 2);
                AddQuad(tris, b + 0, b + 2, n + 2, n + 0);
                AddQuad(tris, b + 1, b + 3, n + 3, n + 1);
            }
        }

        private static void AddQuad(List<int> tris, int a, int b, int c, int d)
        {
            tris.Add(a); tris.Add(b); tris.Add(c);
            tris.Add(a); tris.Add(c); tris.Add(d);
            tris.Add(a); tris.Add(c); tris.Add(b);
            tris.Add(a); tris.Add(d); tris.Add(c);
        }

        public static Mesh BuildTorus(float majorRadius, float tubeRadius, int radialSteps, int tubularSteps)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();

            for (int i = 0; i <= tubularSteps; i++)
            {
                float u = i / (float)tubularSteps;
                float theta = u * Mathf.PI * 2f;
                Vector3 center = new Vector3(Mathf.Cos(theta) * majorRadius, 0f, Mathf.Sin(theta) * majorRadius);
                Vector3 outward = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
                for (int j = 0; j <= radialSteps; j++)
                {
                    float v = j / (float)radialSteps;
                    float phi = v * Mathf.PI * 2f;
                    Vector3 p = center + outward * (Mathf.Cos(phi) * tubeRadius) + Vector3.up * (Mathf.Sin(phi) * tubeRadius);
                    verts.Add(p);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int i = 0; i < tubularSteps; i++)
            {
                for (int j = 0; j < radialSteps; j++)
                {
                    int a = i * (radialSteps + 1) + j;
                    int b = a + radialSteps + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                    tris.Add(a); tris.Add(a + 1); tris.Add(b);
                    tris.Add(a + 1); tris.Add(b + 1); tris.Add(b);
                }
            }

            var mesh = new Mesh { name = "TorusMesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildCone(int segments)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();

            verts.Add(new Vector3(0f, 0.5f, 0f));
            uvs.Add(new Vector2(0.5f, 1f));
            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float phi = u * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(phi) * 0.5f, -0.5f, Mathf.Sin(phi) * 0.5f));
                uvs.Add(new Vector2(u, 0f));
            }
            for (int i = 1; i <= segments; i++)
            {
                tris.Add(0); tris.Add(i); tris.Add(i + 1);
                tris.Add(0); tris.Add(i + 1); tris.Add(i);
            }

            var mesh = new Mesh { name = "ConeMesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildQuad()
        {
            var mesh = new Mesh { name = "QuadMesh" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f)
            };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildDisc(int segments)
        {
            var verts = new List<Vector3> { Vector3.zero };
            var tris = new List<int>();
            var uvs = new List<Vector2> { new Vector2(0.5f, 0.5f) };
            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float phi = u * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(phi) * 0.5f, 0f, Mathf.Sin(phi) * 0.5f));
                uvs.Add(new Vector2(Mathf.Cos(phi) * 0.5f + 0.5f, Mathf.Sin(phi) * 0.5f + 0.5f));
            }
            for (int i = 1; i <= segments; i++)
            {
                tris.Add(0); tris.Add(i + 1); tris.Add(i);
                tris.Add(0); tris.Add(i); tris.Add(i + 1);
            }
            var mesh = new Mesh { name = "DiscMesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
