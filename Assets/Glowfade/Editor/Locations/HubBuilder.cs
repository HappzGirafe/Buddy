using UnityEngine;

namespace Glowfade.EditorTools
{
    /// The Star Theatre: the domed hub every corridor leads from.
    /// Contents: six colour-coded bleacher sections (aisle steps, two child-size crawl tunnels, a back passage behind
    /// each section), the dead star projector standing on its lift column over the pit, the railed pit with the only
    /// stairs down to the Trap Room, the six finale spike marks, painted constellations, two dome speakers, the
    /// stage-manager cue desk and a handful of dim blue-green work lights.
    public static class HubBuilder
    {
        // ---------------------------------------------------------------- layout (local metres, degrees clockwise from +Z)
        const float BleacherInner = 8f;
        const float BleacherOuter = 14.5f;
        const int TierCount = 6;
        const float TierRise = 0.4f;
        const float AisleHalfDeg = 8f;           // no tiers within +-8 degrees of a door: flat aisle from door to centre
        const int SegmentsPerSection = 6;        // 44 degrees of tiers per section -> 7.33 degree box segments
        const float AisleStepWidth = 1.4f;
        const float CrawlWidth = 1f;
        const float CrawlHeight = 0.95f;
        const float EdgeTrim = 0.1f;             // shaved off tier faces that border a door aisle or an open crawl slot
        const float MarkRadius = 6f;
        const float PatrolRadius = 5.2f;
        static readonly float[] CrawlAngles = { 79f, 221f };  // segment centres in the Peekins and Digsley sections

        // Section s spans 60s+8 .. 60s+52 degrees. Its finale mark sits in front of it at 60s+30 and belongs to the host
        // whose wing door is just counter-clockwise of it (Digsley, whose wing is under the pit, takes the front-of-house one).
        static readonly string[] Hosts = { "Hesper", "Peekins", "Pinny", "Digsley", "Tapwell", "Deepwick" };
        static readonly string[] HostNames = { "Miss Hesper", "Peekins", "Auntie Pinny", "Digsley Burrowes", "Dr. Tapwell", "Mrs. Deepwick" };
        static readonly string[] DoorNames = { "Lamp-Post Meadow", "Treetop Nursery", "Midnight Kitchen", "Front of House", "Echo Lab", "Deep-Dark Tank" };
        static readonly string[] GlowWords = { "mint", "gold", "blue-white", "pink", "lilac", "cyan" };
        static readonly int[] ChimeOrder = { 6, 1, 2, 4, 3, 5 };
        static readonly Color[] HostGlow =
        {
            new Color(0.55f, 1f, 0.75f),    // Hesper: cold mint eyespot rings
            new Color(1f, 0.78f, 0.25f),    // Peekins: gold eyeshine
            new Color(0.72f, 0.86f, 1f),    // Pinny: blue-white saddle
            new Color(1f, 0.4f, 0.7f),      // Digsley: pink nose ring and spike tape
            new Color(0.82f, 0.68f, 1f),    // Tapwell: lilac fingertip
            new Color(0.25f, 0.9f, 0.95f),  // Deepwick: cyan jaw dots
        };
        static readonly Color[] SeatColours =  // faded, sun-bleached audience-section paint
        {
            new Color(0.2f, 0.25f, 0.16f),   // pistachio
            new Color(0.16f, 0.2f, 0.26f),   // powder blue
            new Color(0.28f, 0.12f, 0.1f),   // tomato
            new Color(0.25f, 0.11f, 0.13f),  // dusty crimson
            new Color(0.27f, 0.21f, 0.1f),   // ochre
            new Color(0.1f, 0.22f, 0.21f),   // sea-glass
        };
        static readonly Color WorkLightColor = new Color(0.35f, 0.85f, 0.75f);

        static Material GlowTape => GB.Glow("Hub_GlowTape", new Color(0.7f, 1f, 0.85f), 1.2f);

        public static void Build(Transform root)
        {
            BuildShell(root);

            var markers = GB.Group(root, "Markers");
            BuildBleachers(root, markers);
            BuildProjector(root);
            BuildPit(root, markers);
            BuildFinaleMarks(root, markers);
            BuildDomeStars(root);
            BuildSpeakers(root, markers);
            BuildCueDesk(root, markers);
            BuildWorkLights(root);
            BuildRingPatrol(markers);
        }

        /// Floor with the square projector pit, the round wall with its six door gaps and the booth window, and the dome.
        static void BuildShell(Transform root)
        {
            var shell = GB.Group(root, "Shell");
            GB.DiscFloor(shell, "Floor", Vector3.zero, StarhouseLayout.RingOuter, StarhouseLayout.PitHalf, GB.Mat("Hub_Floor", new Color(0.09f, 0.085f, 0.1f), 0.25f));

            var wallMat = GB.Mat("Hub_Wall", new Color(0.1f, 0.1f, 0.13f), 0.1f);
            float r = StarhouseLayout.RingInner + StarhouseLayout.RingThickness * 0.5f;
            float t = StarhouseLayout.RingThickness, top = StarhouseLayout.RingHeight, doorTop = StarhouseLayout.DoorHeight;
            float[] doors = StarhouseLayout.DoorAngles;
            for (int i = 0; i < doors.Length; i++)
            {
                float d = doors[i];
                bool booth = Mathf.Approximately(d, StarhouseLayout.FrontOfHouse.angle);
                float wide = booth ? StarhouseLayout.BoothWindowHalfAngle : StarhouseLayout.DoorHalfAngle;
                float next = doors[(i + 1) % doors.Length];
                bool nextBooth = Mathf.Approximately(next, StarhouseLayout.FrontOfHouse.angle);
                float from = d + wide;
                float to = next - (nextBooth ? StarhouseLayout.BoothWindowHalfAngle : StarhouseLayout.DoorHalfAngle);
                if (to < from) to += 360f;
                GB.ArcWall(shell, $"Wall_{d:000}_{next:000}", Vector3.zero, r, 0f, top, t, from, to, wallMat);

                float h = StarhouseLayout.DoorHalfAngle;
                if (!booth)
                {
                    GB.ArcWall(shell, $"Lintel_{d:000}", Vector3.zero, r, doorTop, top - doorTop, t, d - h, d + h, wallMat, 2f);
                    continue;
                }
                // front-of-house door with the projection booth window above it
                float lo = StarhouseLayout.BoothWindowBottom, hi = StarhouseLayout.BoothWindowTop;
                GB.ArcWall(shell, "Lintel_Booth", Vector3.zero, r, doorTop, lo - doorTop, t, d - h, d + h, wallMat, 2f);
                GB.ArcWall(shell, "BoothJamb_L", Vector3.zero, r, 0f, lo, t, d - wide, d - h, wallMat, 2f);
                GB.ArcWall(shell, "BoothJamb_R", Vector3.zero, r, 0f, lo, t, d + h, d + wide, wallMat, 2f);
                GB.ArcWall(shell, "BoothHeader", Vector3.zero, r, hi, top - hi, t, d - wide, d + wide, wallMat, 2f);
            }

            GB.Dome(shell, "Dome", new Vector3(0f, top, 0f), StarhouseLayout.DomeRadius, GB.Mat("Hub_Dome", new Color(0.04f, 0.05f, 0.09f), 0.05f));

            // close the gap between the theatre floor and the Trap Room ceiling around the pit
            float p = StarhouseLayout.PitHalf;
            var skirt = GB.Mat("Hub_PitSkirt", new Color(0.08f, 0.08f, 0.09f));
            GB.Box(shell, "PitSkirt_N", new Vector3(0f, -0.4f, p + 0.15f), new Vector3(2f * p + 0.6f, 0.8f, 0.3f), skirt);
            GB.Box(shell, "PitSkirt_S", new Vector3(0f, -0.4f, -p - 0.15f), new Vector3(2f * p + 0.6f, 0.8f, 0.3f), skirt);
            GB.Box(shell, "PitSkirt_E", new Vector3(p + 0.15f, -0.4f, 0f), new Vector3(0.3f, 0.8f, 2f * p), skirt);
            GB.Box(shell, "PitSkirt_W", new Vector3(-p - 0.15f, -0.4f, 0f), new Vector3(0.3f, 0.8f, 2f * p), skirt);
        }

        // ---------------------------------------------------------------- bleachers (no-trap ground for Digsley)

        /// Six solid 0.4 m tiers between r 8 and 14.5 in each 44-degree section, a flight of 0.2 m aisle half-steps up the
        /// middle of every section, a 1.5 m back passage between the top tier and the round wall (a loop around each
        /// section), and two child-size crawl tunnels from the stage ring under the back tiers to that back passage.
        static void BuildBleachers(Transform root, Transform markers)
        {
            var bleachers = GB.Group(root, "Bleachers");
            var stepMat = GB.Mat("Hub_AisleStep", new Color(0.3f, 0.25f, 0.19f), 0.2f);
            var tape = GlowTape;
            float depth = (BleacherOuter - BleacherInner) / TierCount;       // ~1.08 m per tier
            float seg = (60f - 2f * AisleHalfDeg) / SegmentsPerSection;      // ~7.33 degrees per box
            float halfTan = Mathf.Tan(seg * 0.5f * Mathf.Deg2Rad);

            for (int s = 0; s < 6; s++)
            {
                var section = GB.Group(bleachers, "Section_" + Hosts[s]);
                var seats = GB.Mat("Hub_Seats_" + Hosts[s], SeatColours[s], 0.15f);
                float start = 60f * s + AisleHalfDeg;

                for (int j = 0; j < SegmentsPerSection; j++)
                {
                    float a = start + seg * (j + 0.5f);
                    bool crawl = IsCrawlAngle(a);
                    bool slotCCW = IsCrawlAngle(a - seg), slotCW = IsCrawlAngle(a + seg);
                    for (int i = 0; i < TierCount; i++)
                    {
                        float rIn = BleacherInner + depth * i;
                        float rMid = rIn + depth * 0.5f;
                        float top = TierRise * (i + 1);
                        float len = 2f * (rIn + depth) * halfTan + 0.04f;  // chord at the tier's back edge, small overlap
                        bool openSlot = top <= CrawlHeight + 0.1f;         // tiers the crawl slot runs through open-topped
                        if (!crawl)
                        {
                            // a box is wider than its wedge at its front edge, so shave the face that borders a door aisle
                            // (or an open crawl slot): its front corner then stays outside +-8 degrees / the 1 m slot
                            float cutCCW = (j == 0 || (slotCCW && openSlot)) ? EdgeTrim : 0f;
                            float cutCW = (j == SegmentsPerSection - 1 || (slotCW && openSlot)) ? EdgeTrim : 0f;
                            Block(section, $"Tier{i}_{j}", a, rMid, (cutCCW - cutCW) * 0.5f, 0f, top, len - cutCCW - cutCW, depth, seats);
                            continue;
                        }
                        // crawl tunnel: an open 1 m slot through the two front tiers, roofed at 0.95 m under the back four;
                        // the open-topped side walls are widened outwards to fill in behind the shaved neighbours
                        float side = (len - CrawlWidth) * 0.5f + (openSlot ? EdgeTrim : 0f);
                        if (side >= 0.05f)
                        {
                            float off = (CrawlWidth + side) * 0.5f;
                            Block(section, $"Tier{i}_{j}_L", a, rMid, -off, 0f, top, side, depth, seats);
                            Block(section, $"Tier{i}_{j}_R", a, rMid, off, 0f, top, side, depth, seats);
                        }
                        if (!openSlot)
                            Block(section, $"Tier{i}_{j}_CrawlRoof", a, rMid, 0f, CrawlHeight, top, CrawlWidth + 0.02f, depth, seats);
                    }
                }

                // aisle steps up the middle of the section: a 0.2 m half-step in front of every 0.4 m tier (rise 0.2, run 0.54)
                float mid = 60f * s + 30f;
                for (int i = 0; i < TierCount; i++)
                {
                    float rIn = BleacherInner + depth * i;
                    float top = TierRise * i + TierRise * 0.5f;
                    Block(section, $"AisleStep{i}", mid, rIn - depth * 0.25f, 0f, 0f, top, AisleStepWidth, depth * 0.5f, stepMat);
                    if (i % 2 == 0)
                        GB.Box(section, $"AisleTape{i}", GB.Polar(mid, rIn - depth * 0.5f + 0.05f, top + 0.006f),
                            new Vector3(AisleStepWidth - 0.1f, 0.012f, 0.06f), tape, mid, false);
                }
            }

            // crawl gaps: player-only shortcuts (and hiding holes) from the stage ring to the back passage
            foreach (float a in CrawlAngles)
            {
                string host = HostNames[Mathf.FloorToInt(a / 60f)];
                GB.Marker(markers, MarkerKind.Handprint, GB.Polar(a, BleacherInner + 2f * depth - 0.15f), a + 180f,
                    $"Child-size crawl gap (1.0 m wide, 0.95 m high) under the back four tiers of the {host} section. Runs out to the back passage at r 14.5; adult costumes cannot follow.");
                GB.Marker(markers, MarkerKind.HidingSpot, GB.Polar(a, 12.4f), a + 180f,
                    $"Inside the crawl tunnel under the {host} bleachers. Probe gaps: the open slot toward the stage ring and the back-passage mouth.");
            }

            // Reel 13: the seat you sat in on the Longest Night, and your own small handprints down to the pit stairs
            GB.Marker(markers, MarkerKind.Handprint, GB.Polar(335f, BleacherInner + depth * 3.5f, TierRise * 4f), 155f,
                "Reel 13 trail start: your seat on the Longest Night (4th row, Deepwick section). UV handprints lead down the 330-degree aisle.");
            GB.Marker(markers, MarkerKind.Handprint, GB.Polar(330f, 7.1f), 150f,
                "Reel 13 trail: foot of the 330-degree aisle steps, past Mrs. Deepwick's mark, heading for the pit stairs.");
            GB.Marker(markers, MarkerKind.Handprint, new Vector3(2.15f, 0f, 3.45f), 180f,
                "Reel 13 trail end: head of the pit stairs, where the warm bobbing light led you down into the projector pit.");
        }

        static bool IsCrawlAngle(float a)
        {
            foreach (float c in CrawlAngles)
                if (Mathf.Abs(Mathf.DeltaAngle(a, c)) < 0.5f) return true;
            return false;
        }

        // ---------------------------------------------------------------- helpers

        /// Box in a polar frame: centred rCenter out along `angle`, shifted `offset` sideways (positive = clockwise),
        /// from y0 up to y1, `width` across and `depth` along the radius.
        static void Block(Transform parent, string name, float angle, float rCenter, float offset, float y0, float y1, float width, float depth, Material mat)
        {
            Vector3 radial = GB.Polar(angle, 1f);
            Vector3 across = new Vector3(radial.z, 0f, -radial.x);
            Vector3 c = radial * rCenter + across * offset + Vector3.up * ((y0 + y1) * 0.5f);
            GB.Box(parent, name, c, new Vector3(width, y1 - y0, depth), mat, angle);
        }

        /// Square-section bar from a to b (legs, braces, sloped rails).
        static void Strut(Transform parent, string name, Vector3 a, Vector3 b, float thickness, Material mat, bool collider)
        {
            Vector3 d = b - a;
            Vector3 euler = Quaternion.FromToRotation(Vector3.up, d.normalized).eulerAngles;
            GB.Box(parent, name, (a + b) * 0.5f, new Vector3(thickness, d.magnitude, thickness), mat, euler, collider);
        }

        /// 1 m railing standing on the theatre floor from a to b: posts every ~2.3 m, top and mid rails, glow tape on top.
        static void Railing(Transform parent, string name, Vector3 a, Vector3 b, Material rail, Material tape)
        {
            Vector3 d = b - a;
            d.y = 0f;
            float len = d.magnitude;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            Vector3 mid = (a + b) * 0.5f;
            int posts = Mathf.CeilToInt(len / 2.3f) + 1;
            for (int k = 0; k < posts; k++)
                GB.Box(parent, $"{name}_Post{k}", Vector3.Lerp(a, b, k / (float)(posts - 1)) + Vector3.up * 0.5f, new Vector3(0.08f, 1f, 0.08f), rail);
            GB.Box(parent, name + "_Top", mid + Vector3.up * 0.96f, new Vector3(0.08f, 0.08f, len + 0.08f), rail, yaw);
            GB.Box(parent, name + "_Mid", mid + Vector3.up * 0.5f, new Vector3(0.06f, 0.06f, len), rail, yaw);
            GB.Box(parent, name + "_Tape", mid + Vector3.up * 1.006f, new Vector3(0.05f, 0.012f, len), tape, yaw, false);
        }

        // ---------------------------------------------------------------- the dead star projector

        /// "Like a giant insect": a drum thorax on a neck, two star globes on a horizontal truss like compound eyes,
        /// a lens head facing the front-of-house door (south), a planet-cage abdomen trailing north, six angled legs.
        /// Everything outside the lift column's footprint is over the pit void or above 2.4 m.
        static void BuildProjector(Transform root)
        {
            var g = GB.Group(root, "StarProjector");
            var steel = GB.Mat("Hub_LiftSteel", new Color(0.14f, 0.14f, 0.15f), 0.3f, 0.6f);
            var hazard = GB.Mat("Hub_HazardPaint", new Color(0.36f, 0.3f, 0.08f), 0.2f);
            var hull = GB.Mat("Hub_ProjectorBody", new Color(0.13f, 0.13f, 0.15f), 0.4f, 0.7f);
            var globe = GB.Mat("Hub_StarGlobe", new Color(0.07f, 0.07f, 0.09f), 0.6f, 0.8f);
            var brass = GB.Mat("Hub_ProjectorBrass", new Color(0.36f, 0.28f, 0.12f), 0.45f, 0.7f);
            var lens = GB.Mat("Hub_DeadLens", new Color(0.02f, 0.03f, 0.04f), 0.95f);

            // lift column: Trap Room floor (y -6) to 0.2 above the theatre floor, |x|,|z| <= 1.25
            GB.Box(g, "LiftColumn", new Vector3(0f, -2.9f, 0f), new Vector3(2.5f, 6.2f, 2.5f), steel);
            GB.Box(g, "LiftColumn_TopBand", new Vector3(0f, 0.08f, 0f), new Vector3(2.54f, 0.14f, 2.54f), hazard, 0f, false);
            GB.Box(g, "LiftColumn_MidBand", new Vector3(0f, -3f, 0f), new Vector3(2.54f, 0.14f, 2.54f), hazard, 0f, false);

            // turntable, neck and the drum body (y 2.4 - 4.2)
            GB.Cylinder(g, "Turntable", new Vector3(0f, 0.35f, 0f), 1.05f, 0.3f, hull);
            GB.Cylinder(g, "Neck", new Vector3(0f, 1.45f, 0f), 0.42f, 1.9f, hull);
            GB.Cylinder(g, "BodyDrum", new Vector3(0f, 3.3f, 0f), 0.8f, 1.8f, hull);
            GB.Cylinder(g, "DrumBand_Low", new Vector3(0f, 2.55f, 0f), 0.83f, 0.08f, brass, false);
            GB.Cylinder(g, "DrumBand_High", new Vector3(0f, 4.05f, 0f), 0.83f, 0.08f, brass, false);

            // horizontal truss through the drum, a star globe at each end (east and west)
            GB.Box(g, "Truss_Upper", new Vector3(0f, 3.58f, 0f), new Vector3(4.9f, 0.12f, 0.12f), hull, 0f, false);
            GB.Box(g, "Truss_Lower", new Vector3(0f, 3.22f, 0f), new Vector3(4.9f, 0.12f, 0.12f), hull, 0f, false);
            for (int side = -1; side <= 1; side += 2)
            {
                string n = side < 0 ? "W" : "E";
                Strut(g, "TrussBrace_" + n + "0", new Vector3(side * 0.9f, 3.22f, 0f), new Vector3(side * 1.5f, 3.58f, 0f), 0.06f, hull, false);
                Strut(g, "TrussBrace_" + n + "1", new Vector3(side * 1.5f, 3.22f, 0f), new Vector3(side * 2.1f, 3.58f, 0f), 0.06f, hull, false);
                GB.Cylinder(g, "GlobeCollar_" + n, new Vector3(side * 1.6f, 3.4f, 0f), 0.24f, 0.4f, brass, new Vector3(0f, 0f, 90f), false);
                GB.Sphere(g, "StarGlobe_" + n, new Vector3(side * 2.6f, 3.4f, 0f), 0.9f, globe);
                GB.Cylinder(g, "GlobeEquator_" + n, new Vector3(side * 2.6f, 3.4f, 0f), 0.94f, 0.06f, brass, false);
            }

            // head facing the front-of-house door, two dead projection lenses, feeler antennae (top ~5.2)
            GB.Ellipsoid(g, "Head", new Vector3(0f, 3.05f, -0.9f), new Vector3(1f, 0.75f, 0.7f), hull);
            GB.Cylinder(g, "Lens_W", new Vector3(-0.22f, 2.95f, -1.25f), 0.13f, 0.3f, lens, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(g, "Lens_E", new Vector3(0.22f, 2.95f, -1.25f), 0.13f, 0.3f, lens, new Vector3(90f, 0f, 0f), false);
            Strut(g, "Antenna_W", new Vector3(-0.3f, 4.15f, -0.4f), new Vector3(-0.85f, 5.2f, -1.3f), 0.04f, brass, false);
            Strut(g, "Antenna_E", new Vector3(0.3f, 4.15f, -0.4f), new Vector3(0.85f, 5.2f, -1.3f), 0.04f, brass, false);

            // abdomen: the planet-projector cage trailing north over the pit (ends at z 2.9, y >= 2.45)
            GB.Cylinder(g, "Abdomen", new Vector3(0f, 2.95f, 1.45f), 0.5f, 1.9f, hull, new Vector3(90f, 0f, 0f), true);
            GB.Cylinder(g, "AbdomenRib_0", new Vector3(0f, 2.95f, 1.05f), 0.54f, 0.07f, brass, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(g, "AbdomenRib_1", new Vector3(0f, 2.95f, 1.85f), 0.54f, 0.07f, brass, new Vector3(90f, 0f, 0f), false);
            GB.Sphere(g, "AbdomenTip", new Vector3(0f, 2.95f, 2.4f), 0.5f, hull, false);

            // six angled legs: hips in the drum, knees high and wide over the pit, feet on the lift column top
            for (int side = -1; side <= 1; side += 2)
                for (int k = 0; k < 3; k++)
                {
                    string n = (side < 0 ? "W" : "E") + k;
                    Vector3 hip, knee, foot;
                    if (k == 1)
                    {
                        // the middle pair folds down under the globes
                        hip = new Vector3(side * 0.62f, 2.6f, 0f);
                        knee = new Vector3(side * 1.7f, 2.05f, 0f);
                        foot = new Vector3(side * 1.15f, 0.2f, 0f);
                    }
                    else
                    {
                        float zs = k == 0 ? -1f : 1f;
                        hip = new Vector3(side * 0.62f, 2.75f, zs * 0.45f);
                        knee = new Vector3(side * 1.85f, 3.3f, zs * 1.55f);
                        foot = new Vector3(side * 1.1f, 0.2f, zs * 1.12f);
                    }
                    Strut(g, "LegUpper_" + n, hip, knee, 0.16f, hull, true);
                    Strut(g, "LegLower_" + n, knee, foot, 0.12f, hull, true);
                }
        }

        // ---------------------------------------------------------------- the pit: stairs down to the Trap Room and railings

        static void BuildPit(Transform root, Transform markers)
        {
            var g = GB.Group(root, "Pit");
            var stairMat = GB.Mat("Hub_PitStairs", new Color(0.17f, 0.17f, 0.18f), 0.3f, 0.5f);
            var railMat = GB.Mat("Hub_Rail", new Color(0.06f, 0.06f, 0.07f), 0.35f, 0.6f);
            var tape = GlowTape;

            // the only way down: 20 x 0.3 m steps from the Trap Room floor, arriving flush at y 0, z 3 (x 1.4 - 2.9)
            GB.Stairs(g, "PitStairs", new Vector3(2.15f, -6f, -2.6f), 0f, 20, 0.3f, 0.28f, 1.5f, stairMat);
            for (int i = 3; i < 20; i += 4)
                GB.Box(g, $"StairTape{i:00}", new Vector3(2.15f, -5.7f + 0.3f * i + 0.006f, -2.6f + 0.28f * i + 0.04f),
                    new Vector3(1.4f, 0.012f, 0.06f), tape, 0f, false);

            // stair side rails in the slots beside the flight: west x 1.31-1.39 (column ends at 1.25, steps start at 1.4),
            // east x 2.91-2.99 (steps end at 2.9, pit edge at 3.0). Posts stand on the Trap Room floor.
            float[] railX = { 1.35f, 2.95f };
            float[] postZ = { -2.55f, -0.5f, 1.5f, 2.95f };
            float slope = 5.95f / 5.55f;  // parallel to the flight (0.3 / 0.28)
            for (int r = 0; r < railX.Length; r++)
            {
                float x = railX[r];
                string n = r == 0 ? "W" : "E";
                Strut(g, $"StairRail{n}_Top", new Vector3(x, -4.75f, -2.6f), new Vector3(x, 1.2f, 2.95f), 0.07f, railMat, true);
                Strut(g, $"StairRail{n}_Mid", new Vector3(x, -5.2f, -2.6f), new Vector3(x, 0.75f, 2.95f), 0.06f, railMat, true);
                for (int k = 0; k < postZ.Length; k++)
                {
                    float topY = -4.75f + (postZ[k] + 2.6f) * slope;
                    GB.Box(g, $"StairRail{n}_Post{k}", new Vector3(x, (topY - 6f) * 0.5f, postZ[k]), new Vector3(0.08f, topY + 6f, 0.08f), railMat);
                }
            }

            // pit-edge railings on the theatre floor, open only over the stair head (x 1.39 - 3.0 on the north edge)
            Railing(g, "Rail_N", new Vector3(-3.1f, 0f, 3.06f), new Vector3(1.35f, 0f, 3.06f), railMat, tape);
            Railing(g, "Rail_E", new Vector3(3.06f, 0f, 3.1f), new Vector3(3.06f, 0f, -3.1f), railMat, tape);
            Railing(g, "Rail_S", new Vector3(3.1f, 0f, -3.06f), new Vector3(-3.1f, 0f, -3.06f), railMat, tape);
            Railing(g, "Rail_W", new Vector3(-3.06f, 0f, -3.1f), new Vector3(-3.06f, 0f, 3.1f), railMat, tape);

            // Digsley's red cue lamp still burns on the stair-head post
            GB.GlowLamp(g, "StairCueLamp", new Vector3(1.35f, 1.27f, 2.95f), new Color(1f, 0.15f, 0.1f), 0.06f, 3f, 0.6f);
            GB.Marker(markers, MarkerKind.Hatch, new Vector3(2.15f, 0f, 2.86f), 180f,
                "Lift pit stair head: a permanent open gap to the Trap Room. Noise on the stage ring carries down to Digsley; the bleachers are no-trap ground.");
        }

        // ---------------------------------------------------------------- finale marks

        /// Six glowing spike-tape X's at r 6 between the doors, each in its Gloamie's glow colour.
        static void BuildFinaleMarks(Transform root, Transform markers)
        {
            var g = GB.Group(root, "FinaleMarks");
            string[] extras =
            {
                " Goes last: abandons the mark to circle the projector lens until sunrise.",
                " Stays curled on this mark as the dawn rises.",
                "",
                " Nearest mark to the cue desk; his own wing is down the pit stairs.",
                "",
                " Her lure stops fooling anyone once the dawn passes its threshold.",
            };
            for (int k = 0; k < 6; k++)
            {
                float a = 30f + 60f * k;
                Vector3 p = GB.Polar(a, MarkRadius);
                var glow = GB.Glow("Hub_Spike_" + Hosts[k], HostGlow[k], 1.6f);
                GB.Box(g, $"SpikeX_{Hosts[k]}_A", p + Vector3.up * 0.004f, new Vector3(0.09f, 0.008f, 0.9f), glow, a + 45f, false);
                GB.Box(g, $"SpikeX_{Hosts[k]}_B", p + Vector3.up * 0.004f, new Vector3(0.09f, 0.008f, 0.9f), glow, a - 45f, false);
                GB.Marker(markers, MarkerKind.GloamieSpawn, p, a + 180f,
                    $"Finale mark: {HostNames[k]} ({GlowWords[k]} spike-tape X) beside the {DoorNames[k]} door. Wakes on chime {ChimeOrder[k]} of 6.{extras[k]}");
            }
        }

        // ---------------------------------------------------------------- dome: painted constellations and speakers

        /// 31 glow-paint stars spread over the dome plus "the Lamp-Post", an invented 7-star winter asterism above the Meadow door.
        static void BuildDomeStars(Transform root)
        {
            var g = GB.Group(root, "DomeStars");
            var bright = GB.Glow("Hub_StarPaint", new Color(0.8f, 1f, 0.92f), 2.2f);
            var faint = GB.Glow("Hub_StarPaintFaint", new Color(0.55f, 0.75f, 0.85f), 1.3f);
            var lampPost = GB.Glow("Hub_StarLampPost", new Color(0.55f, 1f, 0.75f), 2.6f);

            const int count = 31;
            for (int k = 0; k < count; k++)
            {
                float t = (k + 0.5f) / count;
                float lat = Mathf.Asin(0.1f + 0.86f * t) * Mathf.Rad2Deg;  // even spread from ~6 to ~71 degrees up
                float az = 17f + k * 137.508f;                            // golden-angle spiral
                bool big = k % 4 == 0;
                GB.Sphere(g, $"Star{k:00}", DomePoint(az, lat), big ? 0.09f : 0.05f, big ? bright : faint, false);
            }

            Vector2[] asterism =
            {
                new Vector2(0f, 24f), new Vector2(0f, 30f), new Vector2(0f, 36f), new Vector2(0f, 42f),
                new Vector2(-3.5f, 45.5f), new Vector2(3.5f, 45.5f), new Vector2(0f, 48.5f),
            };
            for (int k = 0; k < asterism.Length; k++)
                GB.Sphere(g, $"LampPostStar{k}", DomePoint(asterism[k].x, asterism[k].y), 0.1f, lampPost, false);
        }

        /// Point just inside the dome: azimuth (degrees clockwise from +Z) and latitude above the dome's rim.
        static Vector3 DomePoint(float az, float latDeg)
        {
            float lat = latDeg * Mathf.Deg2Rad;
            return GB.Polar(az, 16.1f * Mathf.Cos(lat), StarhouseLayout.RingHeight + 16.1f * Mathf.Sin(lat));
        }

        /// Two horn speakers hung on the door lintels below the dome (Meadow door and Echo Lab door).
        /// Their mint power lamps are lit although the power is dead.
        static void BuildSpeakers(Transform root, Transform markers)
        {
            var g = GB.Group(root, "DomeSpeakers");
            var hornMat = GB.Mat("Hub_SpeakerHorn", new Color(0.17f, 0.17f, 0.18f), 0.4f, 0.5f);
            var led = GB.Glow("Hub_SpeakerLED", new Color(0.55f, 1f, 0.75f), 2.5f);
            float[] angles = { 0f, 240f };
            string[] names = { "Meadow", "EchoLab" };
            string[] notes =
            {
                "Dome speaker over the Lamp-Post Meadow door: plays the old star-show narration with the power dead (Miss Hesper's voice, never addresses the player).",
                "Dome speaker over the Echo Lab door: second narration source; hums the show's lullaby between constellations.",
            };
            float tilt = 20f * Mathf.Deg2Rad;
            for (int k = 0; k < angles.Length; k++)
            {
                float a = angles[k];
                string n = names[k];
                GB.Box(g, "SpeakerBracket_" + n, GB.Polar(a, 15.69f, 5.96f), new Vector3(0.7f, 0.08f, 0.6f), GB.MatteBlack, a, false);  // back corners at r 15.994
                GB.Box(g, "SpeakerCabinet_" + n, GB.Polar(a, 15.62f, 6.6f), new Vector3(1f, 1.2f, 0.7f), GB.MatteBlack, a, false);
                Vector3 aim = GB.Polar(a + 180f, Mathf.Cos(tilt)) + Vector3.down * Mathf.Sin(tilt);
                GB.Cylinder(g, "SpeakerHorn_" + n, GB.Polar(a, 15.1f, 6.45f), 0.3f, 0.5f, hornMat,
                    Quaternion.FromToRotation(Vector3.up, aim.normalized).eulerAngles, false);
                GB.Sphere(g, "SpeakerLED_" + n, GB.Polar(a, 15.25f, 7.08f), 0.04f, led, false);
                GB.Marker(markers, MarkerKind.StoryProp, GB.Polar(a, 15.62f, 6f), a + 180f, notes[k]);
            }
        }

        // ---------------------------------------------------------------- stage-manager cue desk

        /// Desk against the south pit rail, facing the front-of-house door; the operator stands on its south side
        /// looking up at the projector. Knee space underneath is a hiding spot.
        static void BuildCueDesk(Transform root, Transform markers)
        {
            Vector3 at = new Vector3(-1.3f, 0f, -3.6f);
            var g = GB.Group(root, "CueDesk", at);
            var panel = GB.Mat("Hub_ConsolePanel", new Color(0.11f, 0.11f, 0.12f), 0.3f);
            GB.Box(g, "Pedestal_W", new Vector3(-0.8f, 0.45f, 0f), new Vector3(0.2f, 0.9f, 0.7f), GB.MatteBlack);
            GB.Box(g, "Pedestal_E", new Vector3(0.8f, 0.45f, 0f), new Vector3(0.2f, 0.9f, 0.7f), GB.MatteBlack);
            GB.Box(g, "Top", new Vector3(0f, 0.94f, 0f), new Vector3(1.8f, 0.08f, 0.75f), GB.MatteBlack);
            GB.Box(g, "BackPanel", new Vector3(0f, 0.5f, 0.32f), new Vector3(1.4f, 0.8f, 0.06f), GB.MatteBlack);
            GB.Box(g, "ConsoleFace", new Vector3(0f, 1.08f, 0.17f), new Vector3(1.5f, 0.2f, 0.36f), panel, new Vector3(-18f, 0f, 0f), true);
            GB.Cylinder(g, "CueLampStalk", new Vector3(0.55f, 1.1f, -0.15f), 0.08f, 0.24f, GB.MatteBlack, false);
            GB.GlowLamp(g, "CueLamp_UV", new Vector3(0.55f, 1.29f, -0.15f), new Color(0.5f, 0.25f, 1f), 0.07f, 3f, 0.9f);
            GB.Box(g, "CueButton_Standby", new Vector3(-0.25f, 0.995f, -0.2f), new Vector3(0.1f, 0.03f, 0.1f),
                GB.Glow("Hub_CueStandby", new Color(1f, 0.15f, 0.1f), 1.5f), 0f, false);
            GB.Box(g, "CueButton_Go", new Vector3(0f, 0.995f, -0.2f), new Vector3(0.1f, 0.03f, 0.1f),
                GB.Glow("Hub_CueGo", new Color(0.3f, 1f, 0.45f), 1.2f), 0f, false);

            GB.Marker(markers, MarkerKind.CueLampDesk, at, 180f,
                "Star Theatre stage-manager desk: black-light cue lamp (Curtain Call, 20 m). Operator stands south of it facing the projector; finale console.");
            GB.Marker(markers, MarkerKind.HidingSpot, at + new Vector3(0f, 0f, -0.05f), 180f,
                "Under the cue desk: knee space 1.4 x 0.65 x 0.9 m, open to the south. Probe gaps: front and both pedestal sides.");
        }

        // ---------------------------------------------------------------- work lights (8 lights in the whole hub)

        static void BuildWorkLights(Transform root)
        {
            var g = GB.Group(root, "WorkLights");
            var head = GB.Mat("Hub_WorkLampHead", new Color(0.22f, 0.24f, 0.23f), 0.4f, 0.6f);
            var lensGlow = GB.Glow("Hub_WorkLampLens", WorkLightColor, 2f);
            Vector3 target = new Vector3(0f, 3.3f, 0f);  // the projector's drum

            // two tripod work lights on the stage ring raking the dead projector
            Tripod(g, "Tripod_E", GB.Polar(105f, 7f), target, head, lensGlow);
            Tripod(g, "Tripod_W", GB.Polar(255f, 7f), target, head, lensGlow);

            // caged lamps on the round wall over three back passages (Hesper, Pinny, Tapwell sections); the other three stay dark
            float[] lit = { 30f, 150f, 270f };
            foreach (float a in lit)
            {
                string n = $"{a:000}";
                GB.Box(g, "WallLampBracket_" + n, GB.Polar(a, 15.81f, 2.7f), new Vector3(0.08f, 0.08f, 0.36f), GB.MatteBlack, a, false);
                GB.Cylinder(g, "WallLampCage_" + n, GB.Polar(a, 15.6f, 2.62f), 0.13f, 0.26f, head, false);
                GB.Sphere(g, "WallLampBulb_" + n, GB.Polar(a, 15.6f, 2.5f), 0.07f, lensGlow, false);
                GB.PointLight(g, "WallLampLight_" + n, GB.Polar(a, 15.45f, 2.45f), WorkLightColor, 5.5f, 1f);
            }

            // floor uplight at the pit's south-east corner
            Vector3 up = new Vector3(3.4f, 0.12f, -3.4f);
            GB.Box(g, "Uplight_Housing", up, new Vector3(0.3f, 0.24f, 0.3f), head, 45f, true);
            GB.SpotLight(g, "Uplight", up + Vector3.up * 0.15f, Quaternion.LookRotation(target - up).eulerAngles, WorkLightColor, 8f, 45f, 1.6f);
        }

        static void Tripod(Transform parent, string name, Vector3 basePos, Vector3 target, Material head, Material lensGlow)
        {
            var t = GB.Group(parent, name, basePos);
            Vector3 hub = new Vector3(0f, 1.25f, 0f);
            for (int k = 0; k < 3; k++)
                Strut(t, $"Leg{k}", hub, GB.Polar(30f + 120f * k, 0.5f, 0.01f), 0.035f, GB.MatteBlack, false);  // feet 1 cm up so the slanted end faces stay above the floor
            GB.Box(t, "Pole", new Vector3(0f, 1.05f, 0f), new Vector3(0.06f, 2.1f, 0.06f), GB.MatteBlack);
            Vector3 lampPos = new Vector3(0f, 2.15f, 0f);
            Vector3 fwd = (target - basePos - lampPos).normalized;
            Vector3 aim = Quaternion.LookRotation(fwd).eulerAngles;
            GB.Box(t, "Head", lampPos, new Vector3(0.34f, 0.28f, 0.26f), head, aim, false);
            GB.Box(t, "Lens", lampPos + fwd * 0.135f, new Vector3(0.28f, 0.22f, 0.02f), lensGlow, aim, false);
            GB.SpotLight(t, "Light", lampPos + fwd * 0.2f, aim, WorkLightColor, 8f, 40f, 1.3f);
        }

        // ---------------------------------------------------------------- patrol ring

        static void BuildRingPatrol(Transform markers)
        {
            for (int k = 0; k < 6; k++)
            {
                float a = 60f * k;
                GB.Marker(markers, MarkerKind.PatrolPoint, GB.Polar(a, PatrolRadius), a + 90f,
                    $"Stage ring patrol {k + 1}/6 on the {DoorNames[k]} door line, between the pit rail and the bleachers.");
            }
        }
    }
}
