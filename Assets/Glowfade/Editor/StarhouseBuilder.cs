using System;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Glowfade.EditorTools
{
    /// Builds the whole Wexley Starhouse greybox into Assets/Glowfade/Scenes/Starhouse.unity:
    /// every location, the corridors between them, the dark mood, a walkable player and a baked NavMesh.
    public static class StarhouseBuilder
    {
        public const string ScenePath = "Assets/Glowfade/Scenes/Starhouse.unity";
        const string SceneFolder = "Assets/Glowfade/Scenes";
        const string NavMeshPath = "Assets/Glowfade/Scenes/Starhouse_NavMesh.asset";
        const string VolumePath = "Assets/Glowfade/Scenes/Starhouse_Volume.asset";

        [MenuItem("Glowfade/Build Starhouse Greybox")]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            int failures = Build();
            EditorUtility.DisplayDialog("Starhouse", failures == 0
                ? "Starhouse greybox built."
                : $"Built with {failures} failed part(s). See the Console.", "OK");
        }

        /// Entry point for -executeMethod. Exits with 0 when every part built.
        public static void BuildFromCommandLine()
        {
            int failures = 1;
            try { failures = Build(); }
            catch (Exception e) { Debug.LogException(e); }
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        public static int Build()
        {
            GB.BeginBuild();
            GB.EnsureFolder(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var level = new GameObject("Starhouse").transform;
            int failures = 0;

            failures += Part("Star Theatre", () => HubBuilder.Build(GB.Group(level, StarhouseLayout.HubName)));
            failures += Part("Trap Room", () => TrapRoomBuilder.Build(GB.Group(level, StarhouseLayout.TrapRoomName, StarhouseLayout.TrapRoomRoot)));
            foreach (var slot in StarhouseLayout.Slots)
            {
                var s = slot;
                failures += Part(s.name, () => BuilderFor(s.name)(GB.Group(level, s.name, StarhouseLayout.RootPosition(s), s.angle)));
                failures += Part("Corridor to " + s.name, () => Corridor(level, s));
            }

            TagMarkers(level);
            Atmosphere();
            failures += Part("NavMesh", () => BakeNavMesh(level));
            failures += Part("Player", () => SpawnPlayer(level));

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError("[Starhouse] Saving " + ScenePath + " failed (read-only, open in another program, or disk full?)");
                failures++;
            }
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Starhouse] Build finished with {failures} failure(s).");
            return failures;
        }

        static int Part(string label, Action build)
        {
            try
            {
                build();
                Debug.Log($"[Starhouse] Built {label}");
                return 0;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Starhouse] {label} failed: {e}");
                return 1;
            }
        }

        static Action<Transform> BuilderFor(string slotName)
        {
            switch (slotName)
            {
                case StarhouseLayout.MeadowName: return MeadowWing.Build;
                case StarhouseLayout.NurseryName: return NurseryWing.Build;
                case StarhouseLayout.KitchenName: return KitchenWing.Build;
                case StarhouseLayout.FrontOfHouseName: return FrontOfHouseBuilder.Build;
                case StarhouseLayout.EchoLabName: return EchoLabWing.Build;
                case StarhouseLayout.TankName: return TankWing.Build;
                default: throw new ArgumentException("No builder for " + slotName);
            }
        }

        // ---------------------------------------------------------------- corridors

        /// Black-velvet corridor from the theatre's round wall out to a location's entrance.
        /// Locations below the theatre floor are reached by a ramp.
        static void Corridor(Transform level, StarhouseLayout.Slot slot)
        {
            var g = GB.Group(level, "Corridor_" + slot.name.Replace("Wing_", ""), Vector3.zero, slot.angle);
            float w = StarhouseLayout.CorridorWidth, h = StarhouseLayout.CorridorHeight;
            float shellStart = StarhouseLayout.RingInner + 0.05f; // tucks into the round wall's door gap
            // The corridor floor slides under the theatre's round floor, which falls short of r 16.4 at the door jambs.
            float floorStart = StarhouseLayout.RingInner;
            float end = slot.radius - 0.3f;                        // outer face of the location's entrance wall

            if (Mathf.Approximately(slot.floorY, 0f))
            {
                FlatSection(g, floorStart, end + 0.3f, shellStart, end, 0f, w, h);
                return;
            }

            float rampStart = StarhouseLayout.RingOuter + 1.2f, rampEnd = end - 1.5f;
            // The sloped walls and ceiling start further down the slope than the ramp itself,
            // so the flat shell runs on past the top of the ramp to overlap them.
            FlatSection(g, floorStart, rampStart, shellStart, rampStart + 1.7f, 0f, w, h);
            SlopedSection(g, rampStart, rampEnd, 0f, slot.floorY, w, h);
            FlatSection(g, rampEnd, end + 0.3f, rampEnd, end, slot.floorY, w, h);
        }

        static void FlatSection(Transform g, float floorFrom, float floorTo, float shellFrom, float shellTo, float y, float w, float h)
        {
            const float t = 0.3f;
            // 5 mm low, so the theatre floor and the locations' floors win where they overlap it
            GB.Box(g, "Floor", new Vector3(0f, y - 0.255f, (floorFrom + floorTo) * 0.5f), new Vector3(w + 2f * t, 0.5f, floorTo - floorFrom), GB.MatteBlack);
            float length = shellTo - shellFrom, middle = (shellFrom + shellTo) * 0.5f;
            GB.Box(g, "Wall_L", new Vector3(-w * 0.5f - t * 0.5f, y + h * 0.5f, middle), new Vector3(t, h, length), GB.Velvet);
            GB.Box(g, "Wall_R", new Vector3(w * 0.5f + t * 0.5f, y + h * 0.5f, middle), new Vector3(t, h, length), GB.Velvet);
            GB.Box(g, "Ceiling", new Vector3(0f, y + h + t * 0.5f, middle), new Vector3(w + 2f * t, t, length), GB.Velvet);
        }

        static void SlopedSection(Transform g, float from, float to, float y0, float y1, float w, float h)
        {
            const float t = 0.3f;
            float run = to - from, rise = y1 - y0;
            float pitch = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float length = Mathf.Sqrt(run * run + rise * rise);
            var euler = new Vector3(-pitch, 0f, 0f);
            Vector3 up = Quaternion.Euler(euler) * Vector3.up;
            Vector3 middle = new Vector3(0f, (y0 + y1) * 0.5f, (from + to) * 0.5f);
            GB.Box(g, "Ramp", middle - up * 0.25f, new Vector3(w + 2f * t, 0.5f, length + 0.1f), GB.MatteBlack, euler);
            GB.Box(g, "Wall_L", middle + up * (h * 0.5f) + Vector3.left * (w * 0.5f + t * 0.5f), new Vector3(t, h + 0.6f, length + 0.6f), GB.Velvet, euler);
            GB.Box(g, "Wall_R", middle + up * (h * 0.5f) + Vector3.right * (w * 0.5f + t * 0.5f), new Vector3(t, h + 0.6f, length + 0.6f), GB.Velvet, euler);
            // On a downhill ramp, stop the ceiling where its underside meets the lower corridor's ceiling,
            // so it doesn't hang down into the doorway at the bottom.
            Vector3 fwd = Quaternion.Euler(euler) * Vector3.forward;
            float half = (length + 0.6f) * 0.5f, lowEnd = half;
            if (rise < 0f) lowEnd = Mathf.Clamp((y1 + h - middle.y - up.y * h) / fwd.y, -half, half);
            GB.Box(g, "Ceiling", middle + up * (h + t * 0.5f) + fwd * ((lowEnd - half) * 0.5f), new Vector3(w + 2f * t, t, lowEnd + half), GB.Velvet, euler);
        }

        // ---------------------------------------------------------------- scene setup

        static void TagMarkers(Transform level)
        {
            foreach (Transform root in level)
                foreach (var m in root.GetComponentsInChildren<LevelMarker>(true))
                    if (string.IsNullOrEmpty(m.location)) m.location = root.name;
        }

        /// Near-black ambient light, thin fog and a little bloom so Evergleam glows read in the dark.
        static void Atmosphere()
        {
            RenderSettings.skybox = null;
            WorkLights.Apply(new Color(0.035f, 0.035f, 0.05f));
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.03f;
            RenderSettings.fogColor = new Color(0.01f, 0.01f, 0.02f);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;

            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumePath);
                var bloom = profile.Add<Bloom>(true);
                bloom.threshold.Override(0.9f);
                bloom.intensity.Override(1.4f);
                bloom.scatter.Override(0.7f);
                var vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.32f);
                var grain = profile.Add<FilmGrain>(true);
                grain.type.Override(FilmGrainLookup.Medium3);
                grain.intensity.Override(0.35f);
                var tonemapping = profile.Add<Tonemapping>(true);
                tonemapping.mode.Override(TonemappingMode.ACES);
                foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
                EditorUtility.SetDirty(profile);
            }

            var volume = new GameObject("Global Volume").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        static void BakeNavMesh(Transform level)
        {
            var surface = level.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            AssetDatabase.DeleteAsset(NavMeshPath);
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath);
        }

        /// First-person player at the front-of-house PlayerStart marker, with a flashlight and work-light toggle.
        static void SpawnPlayer(Transform level)
        {
            var start = level.GetComponentsInChildren<LevelMarker>(true).FirstOrDefault(m => m.kind == MarkerKind.PlayerStart);
            Vector3 position = start != null ? start.transform.position : StarhouseLayout.RootPosition(StarhouseLayout.FrontOfHouse) + new Vector3(0f, 0f, -4f);
            float yaw = start != null ? start.transform.eulerAngles.y : 0f;

            var player = new GameObject("Player");
            player.transform.SetPositionAndRotation(position + Vector3.up * 0.05f, Quaternion.Euler(0f, yaw, 0f));
            var controller = player.AddComponent<CharacterController>();
            controller.radius = 0.35f;
            controller.stepOffset = 0.4f;
            controller.slopeLimit = 50f;
            controller.minMoveDistance = 0f;

            var cameraGo = new GameObject("Camera") { tag = "MainCamera" };
            cameraGo.transform.SetParent(player.transform, false);
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 120f;
            camera.fieldOfView = 70f;
            cameraGo.AddComponent<AudioListener>();
            var cameraData = cameraGo.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null) cameraData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;

            var flashlightGo = new GameObject("Flashlight");
            flashlightGo.transform.SetParent(cameraGo.transform, false);
            flashlightGo.transform.localPosition = new Vector3(0.18f, -0.15f, 0.1f);
            var beam = flashlightGo.AddComponent<Light>();
            // a wide soft cone, so looking ahead still lights the floor a few metres in front of you
            beam.type = LightType.Spot;
            beam.range = 25f;
            beam.spotAngle = 70f;
            beam.innerSpotAngle = 35f;
            beam.intensity = 100f;
            beam.color = new Color(1f, 0.95f, 0.85f);
            beam.shadows = LightShadows.Soft;
            flashlightGo.AddComponent<Flashlight>();

            var walker = player.AddComponent<FirstPersonController>();
            walker.cameraPivot = cameraGo.transform;
            cameraGo.transform.localPosition = new Vector3(0f, walker.standEye, 0f);
            controller.height = walker.standHeight;
            controller.center = new Vector3(0f, walker.standHeight * 0.5f, 0f);
            player.AddComponent<WorkLights>();
            player.AddComponent<PlayerHud>();
        }

        static void AddToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
