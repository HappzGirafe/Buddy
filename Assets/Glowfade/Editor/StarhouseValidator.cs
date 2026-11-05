using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Glowfade.EditorTools
{
    /// Checks the built Starhouse: required markers per location, NavMesh reachability from the player start,
    /// geometry kept inside each location's footprint. Also renders review screenshots into Design/Screenshots.
    public static class StarhouseValidator
    {
        static readonly MarkerKind[] MustBeReachable =
        {
            MarkerKind.ReelSpawn, MarkerKind.GloamieSpawn, MarkerKind.SavePoint, MarkerKind.CueLampDesk, MarkerKind.Breaker,
            MarkerKind.BatterySpawn, MarkerKind.FuseSpawn, MarkerKind.RechargeLamp, MarkerKind.PatrolPoint,
        };

        static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        static string ShotFolder => Path.Combine(ProjectRoot, "Design", "Screenshots");

        [MenuItem("Glowfade/Validate Starhouse")]
        public static void ValidateMenu()
        {
            // validating reopens the Starhouse from disk, so offer to save first
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            bool ok = Validate(true, out string report);
            Debug.Log(report);
            EditorUtility.DisplayDialog("Starhouse", ok ? "All checks passed." : "Some checks failed. See the Console.", "OK");
        }

        /// Entry point for -executeMethod. Writes Design/Screenshots/validation.txt and exits 0 when everything passed.
        public static void ValidateFromCommandLine()
        {
            bool ok = false;
            try
            {
                ok = Validate(true, out string report);
                Directory.CreateDirectory(ShotFolder);
                File.WriteAllText(Path.Combine(ShotFolder, "validation.txt"), report);
                Debug.Log(report);
            }
            catch (Exception e) { Debug.LogException(e); }
            EditorApplication.Exit(ok ? 0 : 2);
        }

        public static bool Validate(bool screenshots, out string report)
        {
            EditorSceneManager.OpenScene(StarhouseBuilder.ScenePath, OpenSceneMode.Single);
            var sb = new StringBuilder();
            bool ok = true;
            var level = GameObject.Find("Starhouse").transform;

            var surface = level.GetComponent<NavMeshSurface>();
            if (surface == null || surface.navMeshData == null)
            {
                sb.AppendLine("FAIL no baked NavMesh");
                ok = false;
            }
            else
            {
                surface.RemoveData();
                surface.AddData();
                var tri = NavMesh.CalculateTriangulation();
                sb.AppendLine($"NavMesh: {tri.indices.Length / 3} triangles");
            }

            var markers = level.GetComponentsInChildren<LevelMarker>(true);
            var start = markers.FirstOrDefault(m => m.kind == MarkerKind.PlayerStart);
            Vector3 startOnMesh = Vector3.zero;
            bool haveStart = false;
            if (start != null && NavMesh.SamplePosition(start.transform.position, out var startHit, 2f, NavMesh.AllAreas))
            {
                haveStart = true;
                startOnMesh = startHit.position;
            }
            else
            {
                sb.AppendLine("FAIL PlayerStart missing or not on the NavMesh");
                ok = false;
            }

            var path = new NavMeshPath();
            foreach (Transform root in level)
            {
                int objects = root.GetComponentsInChildren<Transform>(true).Length;
                int lights = root.GetComponentsInChildren<Light>(true).Length;
                var mine = root.GetComponentsInChildren<LevelMarker>(true);
                sb.AppendLine();
                sb.AppendLine($"== {root.name}: {objects} objects, {lights} lights, {mine.Length} markers");
                if (mine.Length > 0)
                    sb.AppendLine("   " + string.Join(", ", mine.GroupBy(m => m.kind).OrderBy(g => g.Key).Select(g => $"{g.Key} x{g.Count()}")));

                foreach (var problem in RequiredMarkerProblems(root.name, mine))
                {
                    sb.AppendLine("   FAIL " + problem);
                    ok = false;
                }

                if (haveStart)
                {
                    foreach (var m in mine.Where(m => MustBeReachable.Contains(m.kind)))
                    {
                        if (!NavMesh.SamplePosition(m.transform.position, out var hit, 1.5f, NavMesh.AllAreas))
                        {
                            sb.AppendLine($"   WARN {m.kind} '{m.note}' at {Local(root, m.transform.position)} is not on the NavMesh");
                            continue;
                        }
                        NavMesh.CalculatePath(startOnMesh, hit.position, NavMesh.AllAreas, path);
                        if (path.status != NavMeshPathStatus.PathComplete)
                        {
                            sb.AppendLine($"   FAIL {m.kind} '{m.note}' at {Local(root, m.transform.position)} cannot be walked to from the PlayerStart");
                            ok = false;
                        }
                    }
                }

                foreach (var outside in OutsideFootprint(root).Take(8))
                    sb.AppendLine("   WARN outside footprint: " + outside);
            }

            if (screenshots)
            {
                if (!FlashlightCheck(sb)) ok = false; // game lighting, so before the work-lit shots
                Screenshots(level, sb);
            }
            report = (ok ? "RESULT: PASS" : "RESULT: FAIL") + Environment.NewLine + sb;
            return ok;
        }

        static string Local(Transform root, Vector3 world)
        {
            Vector3 p = root.InverseTransformPoint(world);
            return $"({p.x:0.0}, {p.y:0.0}, {p.z:0.0})";
        }

        static IEnumerable<string> RequiredMarkerProblems(string rootName, LevelMarker[] markers)
        {
            int Count(MarkerKind k) => markers.Count(m => m.kind == k);
            var need = new List<(MarkerKind kind, int min)>();
            if (rootName.StartsWith("Wing_"))
            {
                need.Add((MarkerKind.ReelSpawn, 2));
                need.Add((MarkerKind.GloamieSpawn, 1));
                need.Add((MarkerKind.PatrolPoint, 3));
                need.Add((MarkerKind.CueLampDesk, 1));
                need.Add((MarkerKind.Breaker, 1));
                need.Add((MarkerKind.FuseSpawn, 1));
                need.Add((MarkerKind.HidingSpot, 3));
                need.Add((MarkerKind.RechargeLamp, 1));
            }
            else if (rootName == StarhouseLayout.FrontOfHouseName)
            {
                need.Add((MarkerKind.PlayerStart, 1));
                need.Add((MarkerKind.SavePoint, 1));
                need.Add((MarkerKind.ReelSpawn, 1));
            }
            else if (rootName == StarhouseLayout.HubName)
            {
                need.Add((MarkerKind.GloamieSpawn, 6));
            }
            foreach (var (kind, min) in need)
                if (Count(kind) < min) yield return $"needs at least {min} {kind}, has {Count(kind)}";
        }

        /// Renderers that poke outside the space a location is allowed to use (see StarhouseLayout).
        static IEnumerable<string> OutsideFootprint(Transform root)
        {
            Func<Vector3, bool> inside;
            const float tol = 0.7f;
            if (root.name.StartsWith("Corridor_") || root.name == "Player" || root.name == "Global Volume") yield break;
            if (root.name == StarhouseLayout.HubName)
                inside = p => new Vector2(p.x, p.z).magnitude <= StarhouseLayout.RingOuter + tol || (Mathf.Abs(p.x) <= StarhouseLayout.PitHalf + tol && Mathf.Abs(p.z) <= StarhouseLayout.PitHalf + tol);
            else if (root.name == StarhouseLayout.TrapRoomName)
                inside = p => Mathf.Abs(p.x) <= StarhouseLayout.TrapRoomHalf + tol && Mathf.Abs(p.z) <= StarhouseLayout.TrapRoomHalf + tol && p.y >= -tol && p.y <= 5.5f + tol;
            else if (root.name == StarhouseLayout.FrontOfHouseName)
                inside = p => (Mathf.Abs(p.x) <= StarhouseLayout.WingHalfWidth + tol && p.z >= -tol && p.z <= StarhouseLayout.WingDepth + tol)
                    || (Mathf.Abs(p.x) <= 3.5f + tol && p.z >= -10f - tol && p.z <= tol && p.y >= 4.2f - tol); // booth above the corridor
            else
                inside = p => Mathf.Abs(p.x) <= StarhouseLayout.WingHalfWidth + tol && p.z >= -tol && p.z <= StarhouseLayout.WingDepth + tol;

            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                // test real vertices so round shapes aren't judged by their bounding boxes
                Vector3[] points = mesh.isReadable && mesh.vertexCount <= 4000 ? mesh.vertices : BoundsCorners(mesh.bounds);
                foreach (var v in points)
                {
                    var p = root.InverseTransformPoint(mf.transform.TransformPoint(v));
                    if (!inside(p))
                    {
                        yield return $"{PathOf(mf.transform, root)} reaches ({p.x:0.0}, {p.y:0.0}, {p.z:0.0})";
                        break;
                    }
                }
            }
        }

        static Vector3[] BoundsCorners(Bounds b)
        {
            var corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
                corners[i] = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            return corners;
        }

        static string PathOf(Transform t, Transform root)
        {
            var names = new List<string>();
            for (var c = t; c != null && c != root; c = c.parent) names.Add(c.name);
            names.Reverse();
            return string.Join("/", names);
        }

        // ---------------------------------------------------------------- flashlight

        /// Renders the player's own view at the start with the game's dark lighting, looking a little down at the
        /// floor ahead, and fails if the flashlight leaves it unreadable. Saves 19_player_view_flashlight.png.
        static bool FlashlightCheck(StringBuilder sb)
        {
            const float MinBeamBrightness = 20f; // mean of 0..255 in the beam's patch of floor
            var player = GameObject.Find("Player");
            var cam = player != null ? player.GetComponentInChildren<Camera>(true) : null;
            if (cam == null)
            {
                sb.AppendLine("   FAIL no player camera for the flashlight check");
                return false;
            }
            var oldRotation = cam.transform.localRotation;
            cam.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            Color32[] pixels;
            const int width = 960, height = 540;
            try
            {
                // the first render of a batch session comes out before post-processing (tone mapping, vignette) is set up,
                // raw and much too bright, so render once to warm up and measure the second
                Shot(cam, "19_player_view_flashlight.png", width, height);
                pixels = Shot(cam, "19_player_view_flashlight.png", width, height);
            }
            finally { cam.transform.localRotation = oldRotation; }

            // the patch of floor the beam lands on: the middle of the frame, a little below centre
            double sum = 0;
            int count = 0;
            for (int y = (int)(height * 0.2f); y < (int)(height * 0.45f); y++)
                for (int x = (int)(width * 0.35f); x < (int)(width * 0.65f); x++)
                {
                    var c = pixels[y * width + x];
                    sum += (c.r + c.g + c.b) / 3.0;
                    count++;
                }
            float brightness = (float)(sum / count);
            bool ok = brightness >= MinBeamBrightness;
            sb.AppendLine();
            sb.AppendLine($"Flashlight at the start, looking 20 degrees down: beam patch brightness {brightness:F0}/255 (needs {MinBeamBrightness:F0})");
            if (!ok) sb.AppendLine("   FAIL the flashlight barely lights the floor in front of the player");
            return ok;
        }

        // ---------------------------------------------------------------- screenshots

        static void Screenshots(Transform level, StringBuilder sb)
        {
            Directory.CreateDirectory(ShotFolder);
            var oldAmbient = RenderSettings.ambientLight;
            var oldProbe = RenderSettings.ambientProbe;
            bool oldFog = RenderSettings.fog;
            var player = GameObject.Find("Player");
            GameObject cameraGo = null, sunGo = null;
            try
            {
                WorkLights.Apply(new Color(0.55f, 0.55f, 0.6f));
                RenderSettings.fog = false;
                if (player != null) player.SetActive(false);

                cameraGo = new GameObject("ShotCamera");
                var cam = cameraGo.AddComponent<Camera>();
                if (cameraGo.GetComponent<UniversalAdditionalCameraData>() == null) cameraGo.AddComponent<UniversalAdditionalCameraData>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
                var renderers = level.GetComponentsInChildren<Renderer>(true);

                // the editor can overwrite the ambient probe while rendering in batch mode, so add a direct work light too
                sunGo = new GameObject("ShotWorkLight");
                var sun = sunGo.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.shadows = LightShadows.None;
                sun.color = Color.white;
                sun.intensity = 1.6f;
                sunGo.transform.rotation = Quaternion.Euler(70f, 30f, 0f);

                // top-down plans: hide everything above the floor being shown
                cam.orthographic = true;
                cam.orthographicSize = 52f;
                cam.nearClipPlane = 1f;
                cam.farClipPlane = 300f;
                cam.transform.SetPositionAndRotation(new Vector3(0f, 120f, 4f), Quaternion.Euler(90f, 0f, 0f));
                WithHidden(renderers, r => r.bounds.min.y > 2.9f, () => Shot(cam, "00_plan_ground.png", 1600, 1600));
                WithHidden(renderers, r => r.bounds.min.y > -3.1f, () => Shot(cam, "01_plan_lower.png", 1600, 1600));

                // a look inside each location, lit along the view like a big headlamp
                cam.orthographic = false;
                cam.fieldOfView = 75f;
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 150f;
                sun.intensity = 1.1f;
                int n = 2;
                foreach (Transform root in level)
                {
                    if (root.name.StartsWith("Corridor_") || root.name == "Player" || root.name == "Global Volume") continue;
                    foreach (var (from, to, tag) in ViewsFor(root))
                    {
                        cam.transform.position = root.TransformPoint(from);
                        cam.transform.rotation = Quaternion.LookRotation(root.TransformPoint(to) - cam.transform.position);
                        sunGo.transform.rotation = cam.transform.rotation * Quaternion.Euler(12f, 20f, 0f);
                        Shot(cam, $"{n++:00}_{root.name}_{tag}.png", 1280, 720);
                    }
                }
            }
            finally
            {
                // put the scene back as it was even if a shot failed, so a later save can't keep the shot setup
                if (sunGo != null) UnityEngine.Object.DestroyImmediate(sunGo);
                if (cameraGo != null) UnityEngine.Object.DestroyImmediate(cameraGo);
                if (player != null) player.SetActive(true);
                RenderSettings.ambientLight = oldAmbient;
                RenderSettings.ambientProbe = oldProbe;
                RenderSettings.fog = oldFog;
            }
            sb.AppendLine();
            sb.AppendLine($"Screenshots written to {ShotFolder}");
        }

        static IEnumerable<(Vector3 from, Vector3 to, string tag)> ViewsFor(Transform root)
        {
            if (root.name == StarhouseLayout.HubName)
            {
                yield return (new Vector3(0f, 1.7f, -15f), new Vector3(0f, 2.5f, 0f), "from_lobby_door");
                yield return (new Vector3(0f, 6.2f, -15.6f), new Vector3(0f, 0f, 2f), "from_booth_window");
                yield return (new Vector3(9f, 4f, 9f), new Vector3(0f, -2f, 0f), "pit");
            }
            else if (root.name == StarhouseLayout.TrapRoomName)
            {
                yield return (new Vector3(-12f, 3.8f, -12f), new Vector3(0f, 0.5f, 0f), "corner");
                // north of the projector's lift column, which fills the middle of the pit
                yield return (new Vector3(0f, 1.7f, 2f), new Vector3(0f, 1.2f, 12f), "from_pit");
            }
            else
            {
                yield return (new Vector3(0f, 1.7f, 0.6f), new Vector3(0f, 1.4f, 12f), "entrance");
                yield return (new Vector3(-10.4f, 4.8f, 0.6f), new Vector3(2f, 0f, 12f), "high_corner");
            }
        }

        static void WithHidden(Renderer[] renderers, Func<Renderer, bool> hide, Action action)
        {
            var hidden = renderers.Where(r => r.enabled && hide(r)).ToList();
            foreach (var r in hidden) r.enabled = false;
            try { action(); }
            finally { foreach (var r in hidden) r.enabled = true; }
        }

        static Color32[] Shot(Camera cam, string fileName, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var request = new RenderPipeline.StandardRequest { destination = rt };
            if (RenderPipeline.SupportsRenderRequest(cam, request)) RenderPipeline.SubmitRenderRequest(cam, request);
            else
            {
                cam.targetTexture = rt;
                cam.Render();
                cam.targetTexture = null;
            }
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            Directory.CreateDirectory(ShotFolder);
            File.WriteAllBytes(Path.Combine(ShotFolder, fileName), tex.EncodeToPNG());
            var pixels = tex.GetPixels32();
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            return pixels;
        }
    }
}
