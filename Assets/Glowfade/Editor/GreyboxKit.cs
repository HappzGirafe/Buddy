using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Glowfade.EditorTools
{
    /// A wall of a GB.Room, in the room's local frame: North = +Z, East = +X, South = -Z, West = -X.
    public enum Side { North, East, South, West }

    /// A gap in a GB.Room wall. Offset is measured from the middle of that wall,
    /// along +X for North/South walls and along +Z for East/West walls.
    public struct Doorway
    {
        public Side side;
        public float offset, width, height, sill;

        public Doorway(Side side, float offset = 0f, float width = 3f, float height = 3f, float sill = 0f)
        {
            this.side = side;
            this.offset = offset;
            this.width = width;
            this.height = height;
            this.sill = sill;
        }
    }

    /// A gap in a GB.Wall, measured from the wall's start point. bottom/top are heights above the wall's base.
    public struct Opening
    {
        public float center, width, bottom, top;

        public Opening(float center, float width, float bottom, float top)
        {
            this.center = center;
            this.width = width;
            this.bottom = bottom;
            this.top = top;
        }
    }

    /// Greybox building kit for the Starhouse.
    /// Every position, rotation and size is LOCAL to the given parent. Parents must be unscaled groups
    /// (made with GB.Group or returned by GB.Room/Wall/Stairs) - never parent things under a Box/Cylinder.
    /// Angles: yaw is degrees clockwise from +Z, seen from above.
    public static class GB
    {
        const string MaterialFolder = "Assets/Glowfade/Materials/Greybox";
        const string MeshFolder = "Assets/Glowfade/Meshes";
        const float WallThickness = 0.3f;

        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static int meshCounter;

        /// Called by StarhouseBuilder at the start of a build.
        public static void BeginBuild()
        {
            materials.Clear();
            meshCounter = 0;
        }

        // ---------------------------------------------------------------- materials

        /// Opaque URP Lit material saved as an asset. Prefix names with your location (e.g. "Kitchen_Tiles")
        /// unless you mean to share one of the palette materials below.
        public static Material Mat(string name, Color color, float smoothness = 0.15f, float metallic = 0f)
        {
            var m = GetMaterial(name);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(m);
            return m;
        }

        /// Emissive material for glowing things (Evergleam costumes, cue lamps, bulbs). Use with a light if it should light the room.
        public static Material Glow(string name, Color color, float intensity = 3f)
        {
            var m = GetMaterial(name);
            m.SetColor("_BaseColor", new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, 1f));
            m.SetFloat("_Smoothness", 0.3f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(color.r * intensity, color.g * intensity, color.b * intensity, 1f));
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(m);
            return m;
        }

        /// See-through material (water, glass). color.a is the opacity.
        public static Material Transparent(string name, Color color, float smoothness = 0.9f)
        {
            var m = GetMaterial(name);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(m);
            return m;
        }

        // Shared palette
        public static Material Concrete => Mat("Concrete", new Color(0.30f, 0.29f, 0.28f));
        public static Material DarkConcrete => Mat("DarkConcrete", new Color(0.16f, 0.16f, 0.17f));
        public static Material Velvet => Mat("VelvetBlack", new Color(0.025f, 0.022f, 0.03f), 0.05f);
        public static Material MatteBlack => Mat("MatteBlack", new Color(0.05f, 0.05f, 0.055f), 0.1f);
        public static Material Wood => Mat("Wood", new Color(0.36f, 0.24f, 0.15f), 0.25f);
        public static Material Plywood => Mat("Plywood", new Color(0.55f, 0.45f, 0.32f), 0.2f);
        public static Material Metal => Mat("Metal", new Color(0.45f, 0.46f, 0.48f), 0.55f, 0.8f);
        public static Material Tile => Mat("Tile", new Color(0.62f, 0.64f, 0.6f), 0.5f);
        public static Material Carpet => Mat("Carpet", new Color(0.22f, 0.08f, 0.12f), 0.05f);
        public static Material Cloth => Mat("Cloth", new Color(0.35f, 0.33f, 0.3f), 0.05f);
        public static Material Water => Transparent("Water", new Color(0.05f, 0.18f, 0.22f, 0.65f));
        public static Material Glass => Transparent("Glass", new Color(0.6f, 0.75f, 0.8f, 0.18f));

        static Shader LitShader => Shader.Find("Universal Render Pipeline/Lit");

        static Material GetMaterial(string name)
        {
            if (materials.TryGetValue(name, out var cached) && cached != null) return cached;
            string path = $"{MaterialFolder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                EnsureFolder(MaterialFolder);
                m = new Material(LitShader) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            materials[name] = m;
            return m;
        }

        /// Creates a project folder (and its parents) if needed. Path like "Assets/Glowfade/Scenes".
        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        // ---------------------------------------------------------------- groups and primitives

        /// Empty, unscaled group. Use these to organise and to place sub-frames (e.g. a rotated prop).
        public static Transform Group(Transform parent, string name, Vector3 position = default, float yaw = 0f)
        {
            var go = new GameObject(name);
            Place(go, parent, name, position, Quaternion.Euler(0f, yaw, 0f), Vector3.one);
            return go.transform;
        }

        /// Box centred on `center` with full size `size`, turned by yaw degrees.
        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, float yaw = 0f, bool collider = true)
            => Box(parent, name, center, size, mat, new Vector3(0f, yaw, 0f), collider);

        /// Box with a full euler rotation (x = pitch, y = yaw, z = roll).
        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, Vector3 euler, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Place(go, parent, name, center, Quaternion.Euler(euler), size);
            Finish(go, mat, collider);
            return go;
        }

        /// Upright cylinder centred on `center`. Uses an exact mesh collider so tops are flat to stand on.
        public static GameObject Cylinder(Transform parent, string name, Vector3 center, float radius, float height, Material mat, bool collider = true)
            => Cylinder(parent, name, center, radius, height, mat, Vector3.zero, collider);

        /// Cylinder with a rotation, e.g. euler (90,0,0) lies it along Z, (0,0,90) along X.
        public static GameObject Cylinder(Transform parent, string name, Vector3 center, float radius, float height, Material mat, Vector3 euler, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Place(go, parent, name, center, Quaternion.Euler(euler), new Vector3(radius * 2f, height * 0.5f, radius * 2f));
            Object.DestroyImmediate(go.GetComponent<Collider>());
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            Finish(go, mat, true);
            return go;
        }

        public static GameObject Sphere(Transform parent, string name, Vector3 center, float radius, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Place(go, parent, name, center, Quaternion.identity, Vector3.one * radius * 2f);
            Finish(go, mat, collider);
            return go;
        }

        /// Stretched sphere with full size `size` (e.g. heads, dishes, bellies of props).
        public static GameObject Ellipsoid(Transform parent, string name, Vector3 center, Vector3 size, Material mat, Vector3 euler = default, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Place(go, parent, name, center, Quaternion.Euler(euler), size);
            Finish(go, mat, collider);
            return go;
        }

        // ---------------------------------------------------------------- architecture

        /// Box room. floorCenter is the middle of the floor's top surface; size is the INTERIOR (x width, y height, z depth).
        /// Floor slab, walls (0.3 thick) and ceiling sit outside the interior. Pass null for any part you don't want
        /// (e.g. ceiling = null for an open top, walls = null to build your own). Returns the room group, origin at floorCenter.
        public static Transform Room(Transform parent, string name, Vector3 floorCenter, Vector3 size, Material floor, Material walls, Material ceiling, params Doorway[] doors)
        {
            var g = Group(parent, name, floorCenter);
            const float t = WallThickness;
            float hx = size.x * 0.5f, hz = size.z * 0.5f, h = size.y;
            if (floor != null) Box(g, "Floor", new Vector3(0f, -0.25f, 0f), new Vector3(size.x + 2f * t, 0.5f, size.z + 2f * t), floor);
            if (ceiling != null) Box(g, "Ceiling", new Vector3(0f, h + t * 0.5f, 0f), new Vector3(size.x + 2f * t, t, size.z + 2f * t), ceiling);
            if (walls != null)
            {
                RoomWall(g, "Wall_N", new Vector3(-hx - t, 0f, hz + t * 0.5f), new Vector3(hx + t, 0f, hz + t * 0.5f), hx + t, Side.North, h, walls, doors);
                RoomWall(g, "Wall_S", new Vector3(-hx - t, 0f, -hz - t * 0.5f), new Vector3(hx + t, 0f, -hz - t * 0.5f), hx + t, Side.South, h, walls, doors);
                RoomWall(g, "Wall_E", new Vector3(hx + t * 0.5f, 0f, -hz), new Vector3(hx + t * 0.5f, 0f, hz), hz, Side.East, h, walls, doors);
                RoomWall(g, "Wall_W", new Vector3(-hx - t * 0.5f, 0f, -hz), new Vector3(-hx - t * 0.5f, 0f, hz), hz, Side.West, h, walls, doors);
            }
            return g;
        }

        static void RoomWall(Transform g, string name, Vector3 a, Vector3 b, float halfLength, Side side, float height, Material mat, Doorway[] doors)
        {
            var openings = new List<Opening>();
            if (doors != null)
                foreach (var d in doors)
                    if (d.side == side) openings.Add(new Opening(halfLength + d.offset, d.width, d.sill, d.sill + d.height));
            Wall(g, name, a, b, height, WallThickness, mat, openings.ToArray());
        }

        /// Straight wall from a to b (both at the wall's base, local). The wall is centred on the a-b line.
        /// Openings (doors/windows) are measured along the wall from a.
        public static Transform Wall(Transform parent, string name, Vector3 a, Vector3 b, float height, float thickness, Material mat, params Opening[] openings)
        {
            Vector3 dir = b - a;
            dir.y = 0f;
            float length = dir.magnitude;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            var g = Group(parent, name, a, yaw); // local +Z runs along the wall from a to b

            var sorted = new List<Opening>(openings ?? new Opening[0]);
            sorted.Sort((p, q) => p.center.CompareTo(q.center));
            float cursor = 0f;
            int n = 0;
            foreach (var o in sorted)
            {
                float s = Mathf.Clamp(o.center - o.width * 0.5f, 0f, length);
                float e = Mathf.Clamp(o.center + o.width * 0.5f, 0f, length);
                if (s > cursor) WallPiece(g, $"Seg{n++}", cursor, s, 0f, height, thickness, mat);
                if (o.bottom > 0f) WallPiece(g, $"Sill{n++}", s, e, 0f, o.bottom, thickness, mat);
                if (o.top < height) WallPiece(g, $"Lintel{n++}", s, e, o.top, height, thickness, mat);
                cursor = Mathf.Max(cursor, e);
            }
            if (cursor < length) WallPiece(g, $"Seg{n++}", cursor, length, 0f, height, thickness, mat);
            return g;
        }

        static void WallPiece(Transform g, string name, float z0, float z1, float y0, float y1, float thickness, Material mat)
        {
            if (z1 - z0 < 0.01f || y1 - y0 < 0.01f) return;
            Box(g, name, new Vector3(0f, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f), new Vector3(thickness, y1 - y0, z1 - z0), mat);
        }

        /// Curved wall made of straight segments. `radius` is the wall's centre line; angles are degrees clockwise from +Z.
        /// y0 is the bottom of the wall. Call it several times to leave door or window gaps.
        public static Transform ArcWall(Transform parent, string name, Vector3 center, float radius, float y0, float height, float thickness, float startDeg, float endDeg, Material mat, float segmentDeg = 4f)
        {
            var g = Group(parent, name, center);
            int count = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(endDeg - startDeg) / segmentDeg));
            float step = (endDeg - startDeg) / count;
            for (int i = 0; i < count; i++)
            {
                Vector3 p0 = Polar(startDeg + step * i, radius);
                Vector3 p1 = Polar(startDeg + step * (i + 1), radius);
                Vector3 d = p1 - p0;
                float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                Box(g, $"Seg{i:00}", (p0 + p1) * 0.5f + Vector3.up * (y0 + height * 0.5f), new Vector3(thickness, height, d.magnitude + thickness * 0.5f), mat, yaw);
            }
            return g;
        }

        /// Point on a circle: angle in degrees clockwise from +Z, seen from above.
        public static Vector3 Polar(float deg, float radius, float y = 0f)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a) * radius, y, Mathf.Cos(a) * radius);
        }

        /// Solid stairs. `start` is the bottom of the first riser at the flight's centre line; the flight climbs
        /// in the yaw direction. Keep rise <= 0.3 and run >= 0.28 so the player and AI can use them.
        public static Transform Stairs(Transform parent, string name, Vector3 start, float yaw, int steps, float rise, float run, float width, Material mat)
        {
            var g = Group(parent, name, start, yaw);
            for (int i = 0; i < steps; i++)
            {
                float h = rise * (i + 1);
                Box(g, $"Step{i:00}", new Vector3(0f, h * 0.5f, run * (i + 0.5f)), new Vector3(width, h, run), mat);
            }
            return g;
        }

        /// Sloped slab. `start` is the top surface at the start edge's centre; it runs `run` metres horizontally in the
        /// yaw direction and changes height by `rise` (negative goes down). Keep slopes under 35 degrees.
        public static GameObject Ramp(Transform parent, string name, Vector3 start, float yaw, float run, float rise, float width, Material mat, float thickness = 0.3f)
        {
            float pitch = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float length = Mathf.Sqrt(run * run + rise * rise);
            Quaternion rot = Quaternion.Euler(-pitch, yaw, 0f);
            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 topMiddle = start + forward * (run * 0.5f) + Vector3.up * (rise * 0.5f);
            Vector3 center = topMiddle - rot * Vector3.up * (thickness * 0.5f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Place(go, parent, name, center, rot, new Vector3(width, thickness, length));
            Finish(go, mat, true);
            return go;
        }

        /// Flat round floor whose top surface is at center.y, with an optional square hole |x|,|z| <= holeHalf around the centre.
        public static GameObject DiscFloor(Transform parent, string name, Vector3 center, float radius, float holeHalf, Material mat, int segments = 96)
        {
            segments = Mathf.Max(8, segments / 8 * 8); // keeps the hole's corners on vertices
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float a = 2f * Mathf.PI * s / segments;
                var dir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
                float inner = holeHalf > 0f ? holeHalf / Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.z)) : 0f;
                Vector3 vi = dir * inner, vo = dir * radius;
                verts.Add(vi);
                verts.Add(vo);
                uvs.Add(new Vector2(vi.x, vi.z) * 0.5f);
                uvs.Add(new Vector2(vo.x, vo.z) * 0.5f);
            }
            for (int s = 0; s < segments; s++)
            {
                int i0 = s * 2, o0 = i0 + 1, i1 = i0 + 2, o1 = i0 + 3;
                tris.Add(i0); tris.Add(o0); tris.Add(o1);
                tris.Add(i0); tris.Add(o1); tris.Add(i1);
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return MeshObject(parent, name, center, SaveMesh(mesh), mat, true);
        }

        /// Inward-facing hemisphere (visible from inside), resting on its rim at basePos. No collider.
        public static GameObject Dome(Transform parent, string name, Vector3 basePos, float radius, Material mat, int segments = 64, int rings = 16)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int r = 0; r <= rings; r++)
            {
                float lat = Mathf.PI * 0.5f * r / rings;
                float y = Mathf.Sin(lat), ring = Mathf.Cos(lat);
                for (int s = 0; s <= segments; s++)
                {
                    float lon = 2f * Mathf.PI * s / segments;
                    var dir = new Vector3(Mathf.Sin(lon) * ring, y, Mathf.Cos(lon) * ring);
                    verts.Add(dir * radius);
                    normals.Add(-dir);
                    uvs.Add(new Vector2((float)s / segments, (float)r / rings));
                }
            }
            int w = segments + 1;
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < segments; s++)
                {
                    int i0 = r * w + s, i1 = i0 + 1, i2 = i0 + w, i3 = i2 + 1;
                    tris.Add(i0); tris.Add(i2); tris.Add(i1); // wound to face the centre
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return MeshObject(parent, name, basePos, SaveMesh(mesh), mat, false);
        }

        // ---------------------------------------------------------------- lights

        /// Realtime point light, no shadows. Keep each location to about 8 lights.
        public static Light PointLight(Transform parent, string name, Vector3 position, Color color, float range = 6f, float intensity = 1.5f)
        {
            var go = new GameObject(name);
            Place(go, parent, name, position, Quaternion.identity, Vector3.one);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            l.lightmapBakeType = LightmapBakeType.Realtime;
            return l;
        }

        /// Realtime spot light pointing along its local +Z after `euler` (e.g. (90,0,0) points straight down). No shadows.
        public static Light SpotLight(Transform parent, string name, Vector3 position, Vector3 euler, Color color, float range = 10f, float angle = 50f, float intensity = 3f)
        {
            var go = new GameObject(name);
            Place(go, parent, name, position, Quaternion.Euler(euler), Vector3.one);
            var l = go.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = color;
            l.range = range;
            l.spotAngle = angle;
            l.innerSpotAngle = angle * 0.5f;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            l.lightmapBakeType = LightmapBakeType.Realtime;
            return l;
        }

        /// Small glowing bulb (emissive sphere, no collider) with a point light. lightRange 0 = bulb only, no light.
        public static Transform GlowLamp(Transform parent, string name, Vector3 position, Color color, float bulbRadius = 0.12f, float lightRange = 4f, float lightIntensity = 1.2f)
        {
            var g = Group(parent, name, position);
            Sphere(g, "Bulb", Vector3.zero, bulbRadius, Glow("Glow_" + ColorKey(color), color, 2.5f), false);
            if (lightRange > 0f) PointLight(g, "Light", Vector3.zero, color, lightRange, lightIntensity);
            return g;
        }

        static string ColorKey(Color c) => $"{(int)(c.r * 255):X2}{(int)(c.g * 255):X2}{(int)(c.b * 255):X2}";

        // ---------------------------------------------------------------- gameplay markers

        /// Gameplay placeholder. Put `position` on the surface the thing stands on, yaw facing where it faces.
        /// Reel, battery and fuse markers get a small glowing stand-in so they show up in play mode.
        public static LevelMarker Marker(Transform parent, MarkerKind kind, Vector3 position, float yaw = 0f, string note = "")
        {
            var t = Group(parent, kind.ToString(), position, yaw);
            var m = t.gameObject.AddComponent<LevelMarker>();
            m.kind = kind;
            m.note = note;
            switch (kind)
            {
                case MarkerKind.ReelSpawn:
                    Cylinder(t, "FilmCanister", new Vector3(0f, 0.04f, 0f), 0.19f, 0.07f, Glow("Pickup_Reel", new Color(1f, 0.8f, 0.25f), 1.5f), false);
                    break;
                case MarkerKind.BatterySpawn:
                    Box(t, "Battery", new Vector3(0f, 0.06f, 0f), new Vector3(0.08f, 0.12f, 0.08f), Glow("Pickup_Battery", new Color(0.3f, 1f, 0.4f), 1.5f), collider: false);
                    break;
                case MarkerKind.FuseSpawn:
                    Box(t, "Fuse", new Vector3(0f, 0.04f, 0f), new Vector3(0.06f, 0.06f, 0.16f), Glow("Pickup_Fuse", new Color(1f, 0.45f, 0.1f), 1.5f), collider: false);
                    break;
            }
            return m;
        }

        // ---------------------------------------------------------------- internals

        static void Place(GameObject go, Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = scale;
        }

        static void Finish(GameObject go, Material mat, bool collider)
        {
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            SetStatic(go);
        }

        static void SetStatic(GameObject go) =>
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);

        static GameObject MeshObject(Transform parent, string name, Vector3 position, Mesh mesh, Material mat, bool collider)
        {
            var go = new GameObject(name);
            Place(go, parent, name, position, Quaternion.identity, Vector3.one);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            SetStatic(go);
            return go;
        }

        static Mesh SaveMesh(Mesh mesh)
        {
            EnsureFolder(MeshFolder);
            string path = $"{MeshFolder}/{mesh.name}_{meshCounter++:000}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                return existing;
            }
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
