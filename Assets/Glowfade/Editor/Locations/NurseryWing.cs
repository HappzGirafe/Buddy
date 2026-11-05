using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Peekins's Treetop Nursery - Act II "Count-to-Ten". Root sits at the entrance; local +Z points into the wing.
    /// Peekins moves only while unlit and curls up, or clamps onto a doorframe, railing or post, when lit. So the wing is
    /// built on hard light/dark contrast: a few switchable set lamps make lit islands, everything between them is dark,
    /// and every route passes a frame, rail or post she can clamp onto.
    ///   Stage (x -4.35..11, z 0..16.35): the giant hollow tree with a treehouse deck (y 2.4) wrapped round its trunk,
    ///     a rope bridge to the Nest perch (y 2.4) and a ramp back down: an upper loop above a lower loop round the trunk.
    ///   Crib room (x -11..-4.65, z 6.65..16.35): cradles, a crib loft (y 1.2) with a child-sized crawl tunnel under it.
    ///   Toy corner (x -11..-4.5, z 0..6.35): the Count-to-Ten block pyramid, the toy chest, the toadstool lamp.
    ///   Backstage (z 16.65..20): lockers, costume rack, stage-manager desk with the cue lamp, breaker panel.
    /// Lights (7): crib nightlight, toadstool lamp, treehouse lantern, moon spot, cue lamp, two recharge lamps.
    public static class NurseryWing
    {
        static readonly Color GoldEyeshine = new Color(1f, 0.72f, 0.2f);
        static readonly Color Amber = new Color(1f, 0.62f, 0.26f);

        static readonly Vector3 TrunkCenter = new Vector3(5.8f, 0f, 10.6f);
        const float TrunkRadius = 1.5f;     // centre line of the hollow trunk's wall
        const float TrunkThickness = 0.4f;  // so the trunk is 1.7 m in radius outside and 1.3 m inside
        const float DeckY = 2.4f;           // treehouse deck, rope bridge and Nest perch walking height
        const float LoftY = 1.2f;           // crib loft walking height

        static Material stageFloor, blackFlat, nightBlue, bark, wornBark, leafFelt, cream, powderBlue, oatmeal, rugFelt, ropeNet,
            lockerGreen, toadRed, wood, ply, metal;
        static Material blockRed, blockYellow, blockBlue, blockGreen;
        static Material gold, goldTape, eyeshine, cueUV, trapRed, moonGlow, lampShade;

        public static void Build(Transform root)
        {
            Materials();
            Shell(root);
            Tree(GB.Group(root, "TreeSet"));
            Platforms(GB.Group(root, "Platforms"));
            CribRoom(GB.Group(root, "CribRoom"));
            ToyCorner(GB.Group(root, "ToyCorner"));
            Backstage(GB.Group(root, "Backstage"));
            StageProps(GB.Group(root, "StageProps"));
            Markers(GB.Group(root, "Markers"));
        }

        static void Materials()
        {
            stageFloor = GB.Mat("Nursery_StageFloor", new Color(0.075f, 0.066f, 0.066f), 0.22f);
            blackFlat = GB.Mat("Nursery_BlackFlat", new Color(0.055f, 0.053f, 0.06f), 0.08f);
            nightBlue = GB.Mat("Nursery_NightBlueFlat", new Color(0.11f, 0.14f, 0.21f), 0.08f);
            bark = GB.Mat("Nursery_PaintedBark", new Color(0.24f, 0.16f, 0.1f), 0.12f);
            wornBark = GB.Mat("Nursery_WornBark", new Color(0.45f, 0.36f, 0.27f), 0.3f);
            leafFelt = GB.Mat("Nursery_LeafFelt", new Color(0.11f, 0.19f, 0.1f), 0.04f);
            cream = GB.Mat("Nursery_FadedCream", new Color(0.55f, 0.52f, 0.44f), 0.2f);
            powderBlue = GB.Mat("Nursery_PowderBlueCord", new Color(0.34f, 0.43f, 0.53f), 0.05f);
            oatmeal = GB.Mat("Nursery_OatmealPaint", new Color(0.5f, 0.42f, 0.32f), 0.15f);
            rugFelt = GB.Mat("Nursery_RugFelt", new Color(0.2f, 0.12f, 0.22f), 0.03f);
            ropeNet = GB.Mat("Nursery_RopeNet", new Color(0.3f, 0.26f, 0.19f), 0.05f);
            lockerGreen = GB.Mat("Nursery_LockerGreen", new Color(0.2f, 0.25f, 0.23f), 0.35f, 0.4f);
            toadRed = GB.Mat("Nursery_ToadstoolRed", new Color(0.42f, 0.1f, 0.08f), 0.1f);
            blockRed = GB.Mat("Nursery_BlockRed", new Color(0.45f, 0.13f, 0.11f), 0.1f);
            blockYellow = GB.Mat("Nursery_BlockYellow", new Color(0.55f, 0.44f, 0.14f), 0.1f);
            blockBlue = GB.Mat("Nursery_BlockBlue", new Color(0.13f, 0.21f, 0.42f), 0.1f);
            blockGreen = GB.Mat("Nursery_BlockGreen", new Color(0.16f, 0.34f, 0.16f), 0.1f);
            wood = GB.Wood;
            ply = GB.Plywood;
            metal = GB.Metal;
            gold = GB.Glow("Nursery_GoldGlow", GoldEyeshine, 2.2f);
            goldTape = GB.Glow("Nursery_GoldGlowTape", GoldEyeshine, 1.1f);
            eyeshine = GB.Glow("Nursery_PaintedEyeshine", GoldEyeshine, 3f);
            cueUV = GB.Glow("Nursery_CueLampUV", new Color(0.55f, 0.2f, 1f), 3f);
            trapRed = GB.Glow("Nursery_TrapCueRed", new Color(1f, 0.12f, 0.08f), 2f);
            moonGlow = GB.Glow("Nursery_PaperMoon", new Color(0.9f, 0.88f, 0.72f), 1.3f);
            lampShade = GB.Glow("Nursery_LampShade", new Color(1f, 0.78f, 0.45f), 1.4f);
        }

        // ---------------------------------------------------------------- shell and sub-area walls

        static void Shell(Transform root)
        {
            GB.Room(root, "Room", new Vector3(0f, 0f, 10f), new Vector3(22f, 6f, 20f), stageFloor, GB.Velvet, GB.MatteBlack, new Doorway(Side.South));
            var walls = GB.Group(root, "SetWalls");

            // Upstage wall between the stage and the backstage corridor (z 16.65..20). Openings are measured from x = -11.
            GB.Wall(walls, "Wall_Upstage", new Vector3(-11f, 0f, 16.5f), new Vector3(11f, 0f, 16.5f), 6f, 0.3f, blackFlat,
                new Opening(1.5f, 1.0f, 0f, 0.95f),    // x -9.5: child-sized crawl hole out of the tunnel under the crib loft
                new Opening(5.0f, 1.4f, 0f, 2.4f),     // x -6.0: crib room -> backstage door
                new Opening(7.7f, 1.2f, 1.15f, 1.85f), // x -3.3: prompt window over the stage-manager desk
                new Opening(11.6f, 1.6f, 0f, 2.4f),    // x  0.6: stage-left door
                new Opening(20.0f, 1.6f, 0f, 2.4f));   // x  9.0: stage-right door

            // Crib room: 3.6 m painted flats, open above, so the treetops look down over them.
            GB.Wall(walls, "Wall_CribSouth", new Vector3(-11f, 0f, 6.5f), new Vector3(-4.35f, 0f, 6.5f), 3.6f, 0.3f, nightBlue,
                new Opening(3.0f, 1.6f, 0f, 2.4f));    // x -8.0: toy corner -> crib room
            GB.Wall(walls, "Wall_CribEast", new Vector3(-4.5f, 0f, 6.35f), new Vector3(-4.5f, 0f, 16.35f), 3.6f, 0.3f, nightBlue,
                new Opening(8.85f, 1.6f, 0f, 2.4f));   // z 15.2: crib room -> stage, behind the Nest perch

            // Painted casings on every real door: doorframes Peekins clamps onto when your light catches her in one.
            DoorTrim(walls, "DoorFrame_CribToBackstage", new Vector3(-6.0f, 0f, 16.5f), 90f, 1.4f, 2.4f);
            DoorTrim(walls, "DoorFrame_StageLeft", new Vector3(0.6f, 0f, 16.5f), 90f, 1.6f, 2.4f);
            DoorTrim(walls, "DoorFrame_StageRight", new Vector3(9.0f, 0f, 16.5f), 90f, 1.6f, 2.4f);
            DoorTrim(walls, "DoorFrame_ToysToCrib", new Vector3(-8.0f, 0f, 6.5f), 90f, 1.6f, 2.4f);
            DoorTrim(walls, "DoorFrame_CribToStage", new Vector3(-4.5f, 0f, 15.2f), 0f, 1.6f, 2.4f);
        }

        /// Casing round a door opening. center = middle of the opening on the wall's centre line, at floor level;
        /// wallYaw = the direction the wall runs (90 for a wall along X, 0 for a wall along Z).
        static void DoorTrim(Transform parent, string name, Vector3 center, float wallYaw, float width, float height)
        {
            var g = GB.Group(parent, name, center, wallYaw); // local +Z runs along the wall
            // 1 cm proud of the opening, so the casing hides the wall's cut ends instead of flickering against them
            float off = width * 0.5f + 0.04f;
            GB.Box(g, "Jamb_A", new Vector3(0f, (height + 0.1f) * 0.5f, -off), new Vector3(0.42f, height + 0.1f, 0.1f), cream);
            GB.Box(g, "Jamb_B", new Vector3(0f, (height + 0.1f) * 0.5f, off), new Vector3(0.42f, height + 0.1f, 0.1f), cream);
            GB.Box(g, "Head", new Vector3(0f, height + 0.045f, 0f), new Vector3(0.42f, 0.11f, width + 0.18f), cream);
        }

        // ---------------------------------------------------------------- the giant tree

        /// Hollow trunk (Peekins's Count-to-Ten den, its mouth facing upstage), solid upper trunk, root flares, big branches,
        /// the empty show branch over centre stage, the felt canopy and the treehouse lantern.
        static void Tree(Transform t)
        {
            Vector3 c = TrunkCenter;
            // mouth: 318..42 deg = 1.57 m clear between the inner corners (her spawn exit needs >= 1.5 m for the 0.5 m agent)
            GB.ArcWall(t, "Trunk_Hollow", c, TrunkRadius, 0f, 2.2f, TrunkThickness, 42f, 318f, bark, 14.5f);
            GB.Cylinder(t, "Trunk_Upper", new Vector3(c.x, 4.2f, c.z), TrunkRadius + TrunkThickness * 0.5f, 3.6f, bark);

            float[] roots = { 110f, 170f, 230f, 300f }; // root flares lying radially, kept clear of the hollow's mouth
            foreach (float a in roots)
                GB.Cylinder(t, $"Root_{a:000}", c + GB.Polar(a, 2.0f, 0.2f), 0.2f, 1.0f, bark, new Vector3(90f, a, 0f));

            // Inside the hollow: her curl-up nest, her gold spike mark and a pair of painted eyes on the back wall.
            var den = GB.Group(t, "CountToTenHollow", c);
            GB.Ellipsoid(den, "NestBlanket", new Vector3(0f, 0.05f, -0.65f), new Vector3(1.1f, 0.1f, 0.7f), powderBlue, Vector3.zero, false);
            GB.Box(den, "SpikeMark_A", new Vector3(0f, 0.006f, 0f), new Vector3(0.5f, 0.012f, 0.05f), gold, 45f, false);
            GB.Box(den, "SpikeMark_B", new Vector3(0f, 0.006f, 0f), new Vector3(0.5f, 0.012f, 0.05f), gold, -45f, false);
            GB.Sphere(den, "PaintedEye_L", new Vector3(-0.11f, 0.85f, -1.27f), 0.045f, eyeshine, false);
            GB.Sphere(den, "PaintedEye_R", new Vector3(0.11f, 0.85f, -1.27f), 0.045f, eyeshine, false);

            // The branch Peekins hung from on the show: out over centre stage, right under the moon spot. Empty.
            var show = GB.Group(t, "ShowBranch_Empty", c, 225f);
            Vector3 root = new Vector3(0f, 5.3f, 1.5f);
            Vector3 tip = Branch(show, "Branch", root, 5.0f, -15f, 0.16f, bark);
            Vector3 along = (tip - root) / 5.0f;
            Branch(show, "BaldGripBand", root + along * 4.0f, 0.5f, -15f, 0.175f, wornBark);
            GB.Box(show, "FrayedRope", tip + new Vector3(0f, -0.45f, -0.1f), new Vector3(0.03f, 0.9f, 0.03f), ropeNet, 0f, false);
            GB.Cylinder(show, "RopeLoop", tip + new Vector3(0f, -0.98f, -0.1f), 0.11f, 0.03f, ropeNet, new Vector3(90f, 0f, 0f), false);

            BigBranch(t, "Branch_N", 0f, 5.0f, 3.4f, 10f);
            BigBranch(t, "Branch_E", 90f, 4.9f, 3.0f, 8f);
            BigBranch(t, "Branch_NW", 300f, 5.1f, 3.6f, 6f);
            BigBranch(t, "Branch_SE", 150f, 4.8f, 3.2f, 5f);

            var canopy = GB.Group(t, "Canopy");
            GB.Ellipsoid(canopy, "Crown", new Vector3(c.x, 5.6f, c.z), new Vector3(3.8f, 0.8f, 3.8f), leafFelt, Vector3.zero, false);
            GB.Ellipsoid(canopy, "Leaves_N", new Vector3(5.8f, 5.55f, 15.0f), new Vector3(2.4f, 0.8f, 2.0f), leafFelt, Vector3.zero, false);
            GB.Ellipsoid(canopy, "Leaves_E", new Vector3(10.0f, 5.4f, 10.6f), new Vector3(1.8f, 0.8f, 2.6f), leafFelt, Vector3.zero, false);
            GB.Ellipsoid(canopy, "Leaves_NW", new Vector3(1.6f, 5.45f, 13.0f), new Vector3(2.6f, 0.8f, 2.2f), leafFelt, Vector3.zero, false);
            GB.Ellipsoid(canopy, "Leaves_SE", new Vector3(8.1f, 5.3f, 6.7f), new Vector3(2.4f, 0.8f, 2.2f), leafFelt, Vector3.zero, false);
            GB.Ellipsoid(canopy, "Leaves_SW", new Vector3(4.2f, 5.55f, 9.0f), new Vector3(2.0f, 0.7f, 2.0f), leafFelt, Vector3.zero, false);

            // Evergleam-painted stars hung under the leaves, and two pairs of painted eyes that flare like hers.
            Vector3[] stars =
            {
                new Vector3(4.4f, 4.7f, 12.6f), new Vector3(7.4f, 4.6f, 12.4f), new Vector3(8.9f, 4.5f, 9.0f), new Vector3(3.0f, 4.8f, 13.6f),
                new Vector3(6.8f, 4.9f, 14.6f), new Vector3(9.6f, 4.8f, 11.8f), new Vector3(2.2f, 4.6f, 12.0f), new Vector3(7.6f, 4.5f, 7.0f),
            };
            for (int i = 0; i < stars.Length; i++)
                GB.Sphere(canopy, $"GlowStar{i}", stars[i], 0.06f, gold, false);
            GB.Sphere(canopy, "LeafEyes_A_L", new Vector3(2.3f, 5.05f, 12.4f), 0.04f, eyeshine, false);
            GB.Sphere(canopy, "LeafEyes_A_R", new Vector3(2.5f, 5.05f, 12.4f), 0.04f, eyeshine, false);
            GB.Sphere(canopy, "LeafEyes_B_L", new Vector3(9.4f, 4.95f, 8.9f), 0.04f, eyeshine, false);
            GB.Sphere(canopy, "LeafEyes_B_R", new Vector3(9.4f, 4.95f, 9.1f), 0.04f, eyeshine, false);

            // Switchable set lamp 1/4: paper lantern hung from the SE branch over the stair head (light 1 of 7).
            var lantern = GB.Group(t, "SetLamp_TreehouseLantern", new Vector3(7.3f, 4.5f, 8.0f));
            GB.Box(lantern, "String", new Vector3(0f, 0.33f, 0f), new Vector3(0.02f, 0.22f, 0.02f), ropeNet, 0f, false);
            GB.Ellipsoid(lantern, "PaperLantern", Vector3.zero, new Vector3(0.36f, 0.44f, 0.36f), lampShade, Vector3.zero, false);
            GB.PointLight(lantern, "Light", Vector3.zero, new Color(1f, 0.8f, 0.45f), 4.5f, 1.3f);
        }

        /// Cylinder limb starting at `start` and running along the parent's +Z, tilted up by slopeDeg (negative = down). Returns the tip.
        static Vector3 Branch(Transform parent, string name, Vector3 start, float length, float slopeDeg, float radius, Material mat)
        {
            float s = slopeDeg * Mathf.Deg2Rad;
            var dir = new Vector3(0f, Mathf.Sin(s), Mathf.Cos(s));
            GB.Cylinder(parent, name, start + dir * (length * 0.5f), radius, length, mat, new Vector3(90f - slopeDeg, 0f, 0f), false);
            return start + dir * length;
        }

        static void BigBranch(Transform parent, string name, float yaw, float y, float length, float slopeDeg)
        {
            var g = GB.Group(parent, name, TrunkCenter, yaw);
            Branch(g, "Limb", new Vector3(0f, y, 1.4f), length, slopeDeg, 0.2f, bark);
        }

        // ---------------------------------------------------------------- platforms, bridge and rails

        static void Platforms(Transform p)
        {
            // Treehouse deck wrapped round the trunk: walk on it at 2.4 m, or underneath it (2.2 m clear) round the hollow.
            GB.Box(p, "Deck_Treehouse", new Vector3(6.25f, DeckY - 0.1f, 10.6f), new Vector3(7.5f, 0.2f, 6.6f), ply);
            float[,] deckPosts = { { 2.65f, 7.45f }, { 9.85f, 7.45f }, { 2.65f, 10.6f }, { 9.85f, 10.6f }, { 2.65f, 13.75f }, { 9.85f, 13.75f } };
            for (int i = 0; i < deckPosts.GetLength(0); i++)
                GB.Box(p, $"DeckPost{i}", new Vector3(deckPosts[i, 0], 1.1f, deckPosts[i, 1]), new Vector3(0.25f, 2.2f, 0.25f), wood);

            // 1.6 m wide (x 7.8..9.4): part of the main loop, so >= 1.5 m for the agent; the rail gap at its head is 1.62 m clear.
            var stairs = GB.Stairs(p, "Stairs_Deck", new Vector3(8.6f, 0f, 4.9f), 0f, 8, 0.3f, 0.3f, 1.6f, ply);
            for (int i = 0; i < 8; i++)
                GB.Box(stairs, $"GlowNosing{i}", new Vector3(0f, 0.3f * (i + 1) + 0.006f, 0.3f * i + 0.04f), new Vector3(1.5f, 0.012f, 0.05f), goldTape, 0f, false);

            Rail(p, "DeckRail_S1", new Vector3(2.5f, DeckY, 7.3f), new Vector3(7.75f, DeckY, 7.3f));
            Rail(p, "DeckRail_S2", new Vector3(9.45f, DeckY, 7.3f), new Vector3(10f, DeckY, 7.3f));
            Rail(p, "DeckRail_E", new Vector3(10f, DeckY, 7.3f), new Vector3(10f, DeckY, 13.9f));
            Rail(p, "DeckRail_N", new Vector3(10f, DeckY, 13.9f), new Vector3(2.5f, DeckY, 13.9f));
            Rail(p, "DeckRail_W1", new Vector3(2.5f, DeckY, 7.3f), new Vector3(2.5f, DeckY, 11.5f));
            Rail(p, "DeckRail_W2", new Vector3(2.5f, DeckY, 13.1f), new Vector3(2.5f, DeckY, 13.9f));
            SetDoorframe(p, "SetDoorframe_Treehouse", new Vector3(2.75f, DeckY, 12.3f), 90f, 1.6f);

            // Rope bridge (level, 1.6 m wide, 2.28 m clear underneath) from the deck's west edge to the Nest perch.
            var bridge = GB.Group(p, "RopeBridge");
            GB.Box(bridge, "Deck", new Vector3(0.65f, DeckY - 0.06f, 12.3f), new Vector3(3.9f, 0.12f, 1.6f), ply);
            for (int i = 0; i < 7; i++)
                GB.Box(bridge, $"PlankGap{i}", new Vector3(-0.85f + 0.5f * i, DeckY + 0.004f, 12.3f), new Vector3(0.03f, 0.008f, 1.5f), blackFlat, 0f, false);
            for (int s = -1; s <= 1; s += 2)
            {
                float z = 12.3f + s * 0.8f;
                string side = s < 0 ? "S" : "N";
                GB.Box(bridge, "Net_" + side, new Vector3(0.65f, DeckY + 0.45f, z), new Vector3(3.7f, 0.9f, 0.06f), ropeNet);
                GB.Box(bridge, "HandRope_" + side, new Vector3(0.65f, DeckY + 0.95f, z), new Vector3(3.9f, 0.04f, 0.04f), ropeNet, 0f, false);
                // posts sit just outside the nets' inner faces so the 1.6 m bridge isn't pinched to 1.48 m at the perch
                GB.Box(bridge, "RopePost_" + side, new Vector3(-1.2f, DeckY + 0.6f, z + s * 0.03f), new Vector3(0.12f, 1.2f, 0.12f), wood);
                GB.Box(bridge, "GlowTape_" + side, new Vector3(0.65f, DeckY + 0.006f, z - s * 0.06f), new Vector3(3.7f, 0.012f, 0.04f), goldTape, 0f, false);
            }

            // Nest perch against the crib-room flat; the ramp takes you back down to centre stage.
            GB.Box(p, "Perch_Nest", new Vector3(-2.775f, DeckY - 0.1f, 12.4f), new Vector3(3.15f, 0.2f, 3.6f), ply);
            float[,] perchPosts = { { -4.2f, 10.75f }, { -1.35f, 11.0f }, { -4.2f, 14.05f }, { -1.35f, 14.05f } };
            for (int i = 0; i < perchPosts.GetLength(0); i++)
                GB.Box(p, $"PerchPost{i}", new Vector3(perchPosts[i, 0], 1.1f, perchPosts[i, 1]), new Vector3(0.25f, 2.2f, 0.25f), wood);
            Rail(p, "PerchRail_S", new Vector3(-4.35f, DeckY, 10.6f), new Vector3(-2.8f, DeckY, 10.6f));
            Rail(p, "PerchRail_N", new Vector3(-4.35f, DeckY, 14.2f), new Vector3(-1.2f, DeckY, 14.2f));
            Rail(p, "PerchRail_E1", new Vector3(-1.2f, DeckY, 10.6f), new Vector3(-1.2f, DeckY, 11.5f));
            Rail(p, "PerchRail_E2", new Vector3(-1.2f, DeckY, 13.1f), new Vector3(-1.2f, DeckY, 14.2f));

            // 24.6 degrees; starts on an 11 cm lip so the slab's underside stays above the floor (y >= 0)
            GB.Ramp(p, "Ramp_Perch", new Vector3(-2.0f, 0.11f, 5.6f), 0f, 5.0f, DeckY - 0.11f, 1.6f, ply, 0.12f);
            SlopeRail(p, "RampRail_W", -2.8f, 5.6f, 10.6f, 0.11f, DeckY);
            SlopeRail(p, "RampRail_E", -1.2f, 5.6f, 10.6f, 0.11f, DeckY);
        }

        /// Railing on a platform edge from a to b (both on the walking surface): posts, a top rail Peekins can clamp onto,
        /// and gold glow tape along the edge.
        static void Rail(Transform parent, string name, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float len = new Vector2(d.x, d.z).magnitude;
            var g = GB.Group(parent, name, a, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg); // local +Z runs from a to b
            int n = Mathf.Max(1, Mathf.CeilToInt(len / 1.6f));
            for (int i = 0; i <= n; i++)
                GB.Box(g, $"Post{i}", new Vector3(0f, 0.475f, len * i / n), new Vector3(0.08f, 0.95f, 0.08f), wood);
            GB.Box(g, "TopRail", new Vector3(0f, 0.95f, len * 0.5f), new Vector3(0.07f, 0.07f, len), wood);
            GB.Box(g, "GlowTape", new Vector3(0f, 0.006f, len * 0.5f), new Vector3(0.05f, 0.012f, len), goldTape, 0f, false);
        }

        /// Railing along one edge of a ramp that climbs along +Z from (z0, y0) to (z1, y1) at the given x.
        static void SlopeRail(Transform parent, string name, float x, float z0, float z1, float y0, float y1)
        {
            var g = GB.Group(parent, name);
            float run = z1 - z0, rise = y1 - y0;
            float pitch = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float len = Mathf.Sqrt(run * run + rise * rise);
            for (int i = 0; i <= 3; i++)
            {
                float z = Mathf.Lerp(z0 + 0.1f, z1 - 0.1f, i / 3f);
                float ys = y0 + rise * (z - z0) / run;
                GB.Box(g, $"Post{i}", new Vector3(x, ys + 0.475f, z), new Vector3(0.08f, 0.95f, 0.08f), wood);
            }
            var mid = new Vector3(x, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f);
            GB.Box(g, "TopRail", mid + Vector3.up * 0.95f, new Vector3(0.07f, 0.07f, len), wood, new Vector3(-pitch, 0f, 0f));
            GB.Box(g, "GlowTape", mid + Vector3.up * 0.01f, new Vector3(0.05f, 0.012f, len), goldTape, new Vector3(-pitch, 0f, 0f), false);
        }

        /// Free-standing painted set doorframe (1990s plywood flat with no wall around it). You walk through along local Z.
        /// Peekins clamps around its posts when lit nearby; a pair of eyes is painted on the lintel facing local -Z.
        static void SetDoorframe(Transform parent, string name, Vector3 pos, float yaw, float width)
        {
            var g = GB.Group(parent, name, pos, yaw);
            const float h = 2.3f;
            float off = width * 0.5f + 0.075f;
            GB.Box(g, "Post_L", new Vector3(-off, (h + 0.15f) * 0.5f, 0f), new Vector3(0.15f, h + 0.15f, 0.15f), cream);
            GB.Box(g, "Post_R", new Vector3(off, (h + 0.15f) * 0.5f, 0f), new Vector3(0.15f, h + 0.15f, 0.15f), cream);
            GB.Box(g, "Lintel", new Vector3(0f, h + 0.075f, 0f), new Vector3(width + 0.3f, 0.15f, 0.15f), cream);
            GB.Box(g, "Foot_L", new Vector3(-off, 0.03f, 0f), new Vector3(0.3f, 0.06f, 0.6f), ply);
            GB.Box(g, "Foot_R", new Vector3(off, 0.03f, 0f), new Vector3(0.3f, 0.06f, 0.6f), ply);
            GB.Sphere(g, "PaintedEye_L", new Vector3(-0.12f, h + 0.075f, -0.08f), 0.04f, eyeshine, false);
            GB.Sphere(g, "PaintedEye_R", new Vector3(0.12f, h + 0.075f, -0.08f), 0.04f, eyeshine, false);
        }

        // ---------------------------------------------------------------- crib room

        static void CribRoom(Transform cr)
        {
            // Crib loft in the dark NW corner (walk on it at 1.2 m). Under it runs a 0.95 m crawl tunnel from the gap in its
            // south skirt to the crawl hole in the upstage wall: a child-sized shortcut into the backstage corridor.
            GB.Box(cr, "Loft_Deck", new Vector3(-9.3f, LoftY - 0.1f, 14.175f), new Vector3(3.4f, 0.2f, 4.35f), ply);
            GB.Wall(cr, "LoftSkirt_S", new Vector3(-11f, 0f, 12.05f), new Vector3(-7.6f, 0f, 12.05f), LoftY - 0.2f, 0.1f, ply,
                new Opening(1.5f, 1.0f, 0f, 0.95f)); // crawl gap at x -9.5
            GB.Wall(cr, "LoftSkirt_E", new Vector3(-7.65f, 0f, 12.0f), new Vector3(-7.65f, 0f, 16.35f), LoftY - 0.2f, 0.1f, ply);
            var loftStairs = GB.Stairs(cr, "Stairs_Loft", new Vector3(-6.4f, 0f, 13.2f), 270f, 4, 0.3f, 0.3f, 1.4f, ply);
            for (int i = 0; i < 4; i++)
                GB.Box(loftStairs, $"GlowNosing{i}", new Vector3(0f, 0.3f * (i + 1) + 0.006f, 0.3f * i + 0.04f), new Vector3(1.3f, 0.012f, 0.05f), goldTape, 0f, false);
            Rail(cr, "LoftRail_S", new Vector3(-11f, LoftY, 12.0f), new Vector3(-7.6f, LoftY, 12.0f));
            Rail(cr, "LoftRail_E", new Vector3(-7.6f, LoftY, 13.9f), new Vector3(-7.6f, LoftY, 16.35f));
            GB.Sphere(cr, "CrawlTunnel_PaintedEye_L", new Vector3(-7.72f, 0.45f, 15.0f), 0.035f, eyeshine, false);
            GB.Sphere(cr, "CrawlTunnel_PaintedEye_R", new Vector3(-7.72f, 0.45f, 15.15f), 0.035f, eyeshine, false);

            Crib(cr, "Crib_Peekins", new Vector3(-10.2f, LoftY, 15.6f), 0f, true);
            Crib(cr, "Crib_Spare", new Vector3(-10.2f, LoftY, 13.4f), 0f, false);

            // Hanging mobile over the cribs (decor): stars in gold Evergleam paint and two paper moons.
            var mobile = GB.Group(cr, "HangingMobile", new Vector3(-10.1f, 4.0f, 14.5f));
            GB.Box(mobile, "Cable", new Vector3(0f, 1.0f, 0f), new Vector3(0.02f, 2.0f, 0.02f), GB.MatteBlack, 0f, false);
            GB.Box(mobile, "Arm_A", Vector3.zero, new Vector3(1.1f, 0.03f, 0.03f), wood, 0f, false);
            GB.Box(mobile, "Arm_B", Vector3.zero, new Vector3(0.03f, 0.03f, 1.1f), wood, 0f, false);
            for (int i = 0; i < 4; i++)
            {
                Vector3 end = GB.Polar(i * 90f, 0.53f);
                GB.Box(mobile, $"String{i}", end + new Vector3(0f, -0.25f, 0f), new Vector3(0.01f, 0.5f, 0.01f), GB.Cloth, 0f, false);
                if (i % 2 == 0) GB.Sphere(mobile, $"GlowStar{i}", end + new Vector3(0f, -0.56f, 0f), 0.08f, gold, false);
                else GB.Ellipsoid(mobile, $"PaperMoon{i}", end + new Vector3(0f, -0.58f, 0f), new Vector3(0.05f, 0.22f, 0.22f), moonGlow, Vector3.zero, false);
            }

            // Hiding spot: wardrobe against the west wall, its door swung back along the wall (hinged on the north side).
            Cabinet(cr, "Wardrobe", new Vector3(-10.675f, 0f, 7.9f), 90f, 1.4f, 0.65f, 2.1f, cream, -170f);

            // Changing table against the crib flat (battery on its bare end) and the nightlight beside it.
            var table = GB.Group(cr, "ChangingTable", new Vector3(-4.975f, 0f, 8.0f));
            GB.Box(table, "Top", new Vector3(0f, 0.87f, 0f), new Vector3(0.65f, 0.06f, 1.4f), cream);
            GB.Box(table, "End_N", new Vector3(0f, 0.42f, 0.67f), new Vector3(0.6f, 0.84f, 0.06f), cream);
            GB.Box(table, "End_S", new Vector3(0f, 0.42f, -0.67f), new Vector3(0.6f, 0.84f, 0.06f), cream);
            GB.Box(table, "Shelf", new Vector3(0f, 0.25f, 0f), new Vector3(0.6f, 0.05f, 1.28f), cream);
            GB.Box(table, "Pad", new Vector3(0f, 0.92f, 0.25f), new Vector3(0.5f, 0.04f, 0.7f), powderBlue, 0f, false);

            // Switchable set lamp 2/4: the nightlight (light 2 of 7). It lights the cradles; the loft and corners stay dark.
            var nightlight = GB.Group(cr, "SetLamp_Nightlight", new Vector3(-5.2f, 0f, 10.0f));
            GB.Box(nightlight, "Table", new Vector3(0f, 0.25f, 0f), new Vector3(0.5f, 0.5f, 0.5f), cream);
            GB.Cylinder(nightlight, "Stem", new Vector3(0f, 0.68f, 0f), 0.03f, 0.36f, metal, false);
            GB.Cylinder(nightlight, "Shade", new Vector3(0f, 0.95f, 0f), 0.2f, 0.24f, lampShade, false);
            GB.PointLight(nightlight, "Light", new Vector3(0f, 0.95f, 0f), new Color(1f, 0.74f, 0.42f), 5f, 1.1f);

            Cradle(cr, "Cradle_A", new Vector3(-8.0f, 0f, 9.0f), 0f);
            Cradle(cr, "Cradle_B", new Vector3(-8.0f, 0f, 11.0f), 20f);
            GB.Cylinder(cr, "Rug", new Vector3(-7.6f, 0.01f, 10.0f), 1.5f, 0.02f, rugFelt, false);
            GB.Box(cr, "FloorVent", new Vector3(-6.6f, 0.01f, 7.6f), new Vector3(0.6f, 0.02f, 0.4f), metal, 0f, false);

            // Evergleam stars painted on the walls of the nursery set.
            Vector3[] wallStars =
            {
                new Vector3(-10.98f, 2.6f, 9.0f), new Vector3(-10.98f, 3.1f, 10.4f), new Vector3(-10.98f, 2.8f, 11.6f),
                new Vector3(-9.6f, 2.9f, 6.67f), new Vector3(-7.0f, 3.2f, 6.67f), new Vector3(-5.6f, 2.7f, 6.67f),
                new Vector3(-4.67f, 3.0f, 9.5f), new Vector3(-4.67f, 2.6f, 12.2f),
            };
            for (int i = 0; i < wallStars.Length; i++)
                GB.Sphere(cr, $"PaintedStar{i}", wallStars[i], 0.05f, gold, false);
        }

        /// Child-sized crib, 1.2 x 0.7 m, long side along local X. pos is on the surface it stands on.
        static void Crib(Transform parent, string name, Vector3 pos, float yaw, bool namePatch)
        {
            var g = GB.Group(parent, name, pos, yaw);
            const float L = 1.2f, W = 0.7f, H = 1.0f;
            GB.Box(g, "Base", new Vector3(0f, 0.3f, 0f), new Vector3(L, 0.12f, W), cream);
            GB.Box(g, "Mattress", new Vector3(0f, 0.42f, 0f), new Vector3(L - 0.08f, 0.12f, W - 0.08f), powderBlue, 0f, false);
            GB.Box(g, "Headboard", new Vector3(-L * 0.5f, H * 0.5f, 0f), new Vector3(0.06f, H, W), cream);
            GB.Box(g, "Footboard", new Vector3(L * 0.5f, H * 0.5f, 0f), new Vector3(0.06f, H, W), cream);
            for (int s = -1; s <= 1; s += 2)
            {
                float z = s * (W * 0.5f - 0.03f);
                string side = s < 0 ? "A" : "B";
                GB.Box(g, "TopRail_" + side, new Vector3(0f, H - 0.03f, z), new Vector3(L, 0.06f, 0.06f), cream);
                for (int i = 1; i <= 5; i++)
                    GB.Box(g, $"Bar{side}{i}", new Vector3(-L * 0.5f + L * i / 6f, 0.665f, z), new Vector3(0.025f, 0.61f, 0.025f), cream, 0f, false);
            }
            if (namePatch)
                GB.Box(g, "FeltNamePatch_PEEKINS", new Vector3(L * 0.5f + 0.035f, 0.72f, 0f), new Vector3(0.012f, 0.14f, 0.36f), powderBlue, 0f, false);
        }

        static void Cradle(Transform parent, string name, Vector3 pos, float yaw)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Box(g, "Rocker_A", new Vector3(-0.3f, 0.05f, 0f), new Vector3(0.06f, 0.1f, 1.1f), wood);
            GB.Box(g, "Rocker_B", new Vector3(0.3f, 0.05f, 0f), new Vector3(0.06f, 0.1f, 1.1f), wood);
            GB.Box(g, "Basket", new Vector3(0f, 0.38f, 0f), new Vector3(0.62f, 0.5f, 0.95f), cream);
            GB.Box(g, "Blanket", new Vector3(0f, 0.64f, 0.1f), new Vector3(0.56f, 0.04f, 0.6f), powderBlue, 0f, false);
            GB.Box(g, "Hood", new Vector3(0f, 0.85f, -0.36f), new Vector3(0.62f, 0.45f, 0.25f), powderBlue, new Vector3(-20f, 0f, 0f));
        }

        /// Open-fronted cabinet (wardrobe, locker) you can hide in. Front faces local +Z. The door hangs on the right front edge
        /// (doorYaw > 0) or the left one (doorYaw < 0) and is swung outwards by |doorYaw| degrees. Inside floor is 0.1 m up.
        static void Cabinet(Transform parent, string name, Vector3 pos, float yaw, float w, float d, float h, Material mat, float doorYaw)
        {
            var g = GB.Group(parent, name, pos, yaw);
            const float t = 0.05f;
            GB.Box(g, "Back", new Vector3(0f, h * 0.5f, -d * 0.5f + t * 0.5f), new Vector3(w, h, t), mat);
            GB.Box(g, "Side_L", new Vector3(-w * 0.5f + t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, d), mat);
            GB.Box(g, "Side_R", new Vector3(w * 0.5f - t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, d), mat);
            GB.Box(g, "Top", new Vector3(0f, h - t * 0.5f, 0f), new Vector3(w, t, d), mat);
            GB.Box(g, "Bottom", new Vector3(0f, 0.05f, 0f), new Vector3(w - 2f * t, 0.1f, d - t), mat);
            float side = doorYaw >= 0f ? 1f : -1f;
            var hinge = GB.Group(g, "DoorHinge", new Vector3(side * w * 0.5f, 0f, d * 0.5f), doorYaw);
            GB.Box(hinge, "Door", new Vector3(-side * w * 0.5f, h * 0.5f, 0.025f), new Vector3(w - 0.02f, h - 0.02f, t), mat);
        }

        // ---------------------------------------------------------------- toy corner

        static void ToyCorner(Transform tc)
        {
            SetDoorframe(tc, "SetDoorframe_Storybook", new Vector3(-4.0f, 0f, 3.4f), 90f, 1.4f);
            GB.Box(tc, "Rug", new Vector3(-7.6f, 0.01f, 3.4f), new Vector3(3.0f, 0.02f, 2.6f), rugFelt, 0f, false);

            // The Count-to-Ten pyramid: ten big numbered blocks (4-3-2-1) against the west wall, numbers in gold glow paint.
            Material[] blockMats = { blockRed, blockYellow, blockBlue, blockGreen };
            const float s = 0.55f;
            int n = 0;
            for (int row = 0; row < 4; row++)
                for (int i = 0; i < 4 - row; i++)
                {
                    n++;
                    var pos = new Vector3(-10.6f, s * (row + 0.5f), 2.2f + s * (i + row * 0.5f));
                    GB.Box(tc, $"CountBlock_{n:00}", pos, Vector3.one * (s - 0.02f), blockMats[n % 4]);
                    GB.Box(tc, $"CountBlock_{n:00}_GlowNumber", pos + new Vector3(s * 0.5f, 0f, 0f), new Vector3(0.012f, 0.18f, 0.08f + 0.012f * n), goldTape, 0f, false);
                }

            // Hiding spot: the toy chest (prop trunk) against the south wall, its lid propped open against the wall.
            var chest = GB.Group(tc, "PropTrunk_ToyChest", new Vector3(-9.0f, 0f, 0.45f));
            GB.Box(chest, "Bottom", new Vector3(0f, 0.05f, 0f), new Vector3(1.3f, 0.1f, 0.8f), wood);
            GB.Box(chest, "Front", new Vector3(0f, 0.375f, 0.375f), new Vector3(1.3f, 0.75f, 0.05f), wood);
            GB.Box(chest, "Back", new Vector3(0f, 0.375f, -0.375f), new Vector3(1.3f, 0.75f, 0.05f), wood);
            GB.Box(chest, "Side_W", new Vector3(-0.625f, 0.375f, 0f), new Vector3(0.05f, 0.75f, 0.7f), wood);
            GB.Box(chest, "Side_E", new Vector3(0.625f, 0.375f, 0f), new Vector3(0.05f, 0.75f, 0.7f), wood);
            GB.Box(chest, "Lid_Open", new Vector3(0f, 1.15f, -0.35f), new Vector3(1.3f, 0.05f, 0.8f), blockRed, new Vector3(90f, 0f, 0f));
            GB.Sphere(chest, "PaintedStar_A", new Vector3(-0.3f, 0.45f, 0.405f), 0.04f, gold, false);
            GB.Sphere(chest, "PaintedStar_B", new Vector3(0.3f, 0.55f, 0.405f), 0.04f, gold, false);

            var horse = GB.Group(tc, "RockingHorse", new Vector3(-7.4f, 0f, 1.8f), 90f);
            GB.Box(horse, "Rocker_A", new Vector3(-0.22f, 0.06f, 0f), new Vector3(0.06f, 0.12f, 1.3f), wood);
            GB.Box(horse, "Rocker_B", new Vector3(0.22f, 0.06f, 0f), new Vector3(0.06f, 0.12f, 1.3f), wood);
            GB.Box(horse, "Legs", new Vector3(0f, 0.32f, 0f), new Vector3(0.44f, 0.4f, 0.8f), wood);
            GB.Ellipsoid(horse, "Body", new Vector3(0f, 0.68f, 0f), new Vector3(0.35f, 0.38f, 0.95f), cream, Vector3.zero, false);
            GB.Box(horse, "Neck", new Vector3(0f, 0.95f, 0.38f), new Vector3(0.18f, 0.45f, 0.2f), cream, new Vector3(25f, 0f, 0f));
            GB.Box(horse, "Head", new Vector3(0f, 1.15f, 0.55f), new Vector3(0.18f, 0.2f, 0.38f), cream);
            GB.Sphere(horse, "PaintedEye", new Vector3(0.095f, 1.2f, 0.62f), 0.03f, eyeshine, false);

            // Switchable set lamp 3/4: the toadstool lamp (light 3 of 7). It lights the storybook doorframe; the SW corner stays dark.
            var toad = GB.Group(tc, "SetLamp_Toadstool", new Vector3(-5.6f, 0f, 5.4f));
            GB.Cylinder(toad, "Stem", new Vector3(0f, 0.35f, 0f), 0.14f, 0.7f, cream);
            GB.Ellipsoid(toad, "Cap", new Vector3(0f, 0.8f, 0f), new Vector3(0.9f, 0.38f, 0.9f), toadRed);
            GB.Cylinder(toad, "GlowGills", new Vector3(0f, 0.66f, 0f), 0.38f, 0.03f, lampShade, false);
            for (int i = 0; i < 3; i++)
                GB.Sphere(toad, $"CapSpot{i}", GB.Polar(40f + 120f * i, 0.24f, 0.93f), 0.06f, cream, false);
            GB.PointLight(toad, "Light", new Vector3(0f, 0.55f, 0f), new Color(1f, 0.6f, 0.35f), 4f, 1.0f);

            GB.Box(tc, "LooseBlock_A", new Vector3(-6.3f, 0.2f, 4.6f), Vector3.one * 0.4f, blockBlue, 20f);
            GB.Box(tc, "LooseBlock_B", new Vector3(-8.7f, 0.2f, 4.9f), Vector3.one * 0.4f, blockYellow, -15f);
            GB.Box(tc, "LooseBlock_C", new Vector3(-5.4f, 0.2f, 2.0f), Vector3.one * 0.4f, blockGreen, 40f);
        }

        // ---------------------------------------------------------------- backstage corridor

        static void Backstage(Transform bs)
        {
            // Lockers on the north wall; the fourth stands open and is roomy enough to hide in.
            for (int i = 0; i < 3; i++)
                GB.Box(bs, $"Locker{i}", new Vector3(-10.4f + 0.8f * i, 1.0f, 19.725f), new Vector3(0.78f, 2.0f, 0.55f), lockerGreen);
            Cabinet(bs, "Locker_Open", new Vector3(-8.0f, 0f, 19.725f), 180f, 0.78f, 0.55f, 2.0f, lockerGreen, 105f);
            GB.Ellipsoid(bs, "SpareHead_ChildSize", new Vector3(-10.0f, 2.17f, 19.7f), new Vector3(0.42f, 0.34f, 0.36f), oatmeal, Vector3.zero, false);
            GB.Sphere(bs, "SpareHead_Eye_L", new Vector3(-10.09f, 2.2f, 19.53f), 0.05f, eyeshine, false);
            GB.Sphere(bs, "SpareHead_Eye_R", new Vector3(-9.91f, 2.2f, 19.53f), 0.05f, eyeshine, false);

            // Hiding spot: costume rack of velvet blacks, plus one tiny powder-blue romper.
            var rack = GB.Group(bs, "CostumeRack", new Vector3(-5.7f, 0f, 19.3f));
            GB.Box(rack, "Upright_W", new Vector3(-0.9f, 0.9f, 0f), new Vector3(0.06f, 1.8f, 0.06f), metal);
            GB.Box(rack, "Upright_E", new Vector3(0.9f, 0.9f, 0f), new Vector3(0.06f, 1.8f, 0.06f), metal);
            GB.Box(rack, "Foot_W", new Vector3(-0.9f, 0.04f, 0f), new Vector3(0.08f, 0.08f, 0.6f), metal);
            GB.Box(rack, "Foot_E", new Vector3(0.9f, 0.04f, 0f), new Vector3(0.08f, 0.08f, 0.6f), metal);
            GB.Box(rack, "Rail", new Vector3(0f, 1.78f, 0f), new Vector3(1.86f, 0.05f, 0.05f), metal);
            for (int i = 0; i < 6; i++)
            {
                bool romper = i == 2;
                GB.Box(rack, romper ? "SpareRomper_ChildSize" : $"VelvetBlacks{i}", new Vector3(-0.72f + 0.288f * i, romper ? 1.35f : 1.1f, 0f),
                    new Vector3(0.12f, romper ? 0.75f : 1.3f, 0.5f), romper ? powderBlue : GB.Velvet);
            }

            // Stage-manager desk under the prompt window, with the black-light cue lamp for Curtain Call. Hide under it.
            var desk = GB.Group(bs, "StageManagerDesk", new Vector3(-3.3f, 0f, 17.05f));
            GB.Box(desk, "Top", new Vector3(0f, 0.92f, 0f), new Vector3(1.8f, 0.06f, 0.8f), wood);
            GB.Box(desk, "Side_W", new Vector3(-0.87f, 0.445f, 0f), new Vector3(0.06f, 0.89f, 0.8f), wood);
            GB.Box(desk, "Side_E", new Vector3(0.87f, 0.445f, 0f), new Vector3(0.06f, 0.89f, 0.8f), wood);
            GB.Box(desk, "PromptMonitor", new Vector3(-0.65f, 1.11f, -0.22f), new Vector3(0.36f, 0.32f, 0.3f), GB.MatteBlack, 0f, false);
            GB.Box(desk, "CueLightPanel", new Vector3(-0.1f, 0.99f, 0.05f), new Vector3(0.5f, 0.08f, 0.28f), GB.MatteBlack, 0f, false);
            for (int i = 0; i < 3; i++)
                GB.Sphere(desk, $"CueButton{i}", new Vector3(-0.25f + 0.15f * i, 1.04f, 0.05f), 0.025f, i == 1 ? cueUV : trapRed, false);
            GB.Box(desk, "CueSheet_Clipboard", new Vector3(0.35f, 0.955f, 0.12f), new Vector3(0.25f, 0.01f, 0.33f), cream, 15f, false);
            GB.Box(desk, "CueLamp_Neck", new Vector3(0.75f, 1.15f, -0.25f), new Vector3(0.025f, 0.4f, 0.025f), metal, 0f, false);
            GB.Box(desk, "CueLamp_Head", new Vector3(0.75f, 1.36f, -0.15f), new Vector3(0.12f, 0.07f, 0.2f), cueUV, 0f, false);
            GB.PointLight(desk, "CueLamp_UVLight", new Vector3(0.75f, 1.3f, -0.05f), new Color(0.55f, 0.25f, 1f), 3f, 0.8f); // light 5 of 7

            // The wing's breaker on the north wall. Restoring it is a loud 10 s crescendo with two ways in to watch: the corridor
            // from the west and the stage-right door behind you. The spot to set the lantern down.
            var breaker = GB.Group(bs, "BreakerPanel", new Vector3(7.5f, 0f, 20.0f), 180f); // local +Z points out of the wall
            GB.Box(breaker, "Cabinet", new Vector3(0f, 1.5f, 0.11f), new Vector3(0.9f, 1.2f, 0.22f), metal);
            GB.Box(breaker, "Door_Open", new Vector3(-0.95f, 1.5f, 0.03f), new Vector3(0.88f, 1.15f, 0.03f), metal, 0f, false);
            GB.Box(breaker, "MainLever", new Vector3(0.25f, 1.55f, 0.3f), new Vector3(0.06f, 0.3f, 0.06f), metal, new Vector3(-30f, 0f, 0f), false);
            for (int i = 0; i < 3; i++)
                GB.Sphere(breaker, $"DeadIndicator{i}", new Vector3(-0.25f + 0.12f * i, 1.95f, 0.23f), 0.03f, blockRed, false);
            GB.Box(breaker, "GlowTape_Frame", new Vector3(0f, 2.115f, 0.12f), new Vector3(0.92f, 0.03f, 0.22f), goldTape, 0f, false);
            GB.Box(breaker, "Conduit", new Vector3(0.3f, 3.95f, 0.05f), new Vector3(0.08f, 3.7f, 0.08f), metal, 0f, false);
            GB.Box(breaker, "GlowTape_StandHere", new Vector3(0f, 0.006f, 0.75f), new Vector3(0.9f, 0.012f, 0.05f), goldTape, 0f, false);

            RechargeLamp(bs, "RechargeLamp_Backstage", new Vector3(2.0f, 0f, 20.0f), 180f); // light 6 of 7

            GB.Box(bs, "RoadCase_A", new Vector3(-1.55f, 0.35f, 19.6f), new Vector3(0.9f, 0.7f, 0.6f), GB.MatteBlack);
            GB.Box(bs, "RoadCase_B", new Vector3(-0.6f, 0.35f, 19.6f), new Vector3(0.8f, 0.7f, 0.6f), GB.MatteBlack);
            GB.Box(bs, "RoadCase_Top", new Vector3(-1.2f, 0.95f, 19.62f), new Vector3(0.7f, 0.5f, 0.5f), GB.MatteBlack, 8f);
            GB.Box(bs, "PropTrunk_Spare", new Vector3(3.6f, 0.35f, 17.05f), new Vector3(1.2f, 0.7f, 0.7f), wood);
            GB.Box(bs, "FloorVent", new Vector3(5.0f, 0.01f, 18.3f), new Vector3(0.6f, 0.02f, 0.4f), metal, 0f, false);
            // glow-tape safety marks on the backstage side of each door
            GB.Box(bs, "GlowTape_DoorCrib", new Vector3(-6.0f, 0.006f, 16.72f), new Vector3(1.4f, 0.012f, 0.05f), goldTape, 0f, false);
            GB.Box(bs, "GlowTape_DoorStageLeft", new Vector3(0.6f, 0.006f, 16.72f), new Vector3(1.6f, 0.012f, 0.05f), goldTape, 0f, false);
            GB.Box(bs, "GlowTape_DoorStageRight", new Vector3(9.0f, 0.006f, 16.72f), new Vector3(1.6f, 0.012f, 0.05f), goldTape, 0f, false);
        }

        /// Wall lamp that recharges the flashlight: a steady amber bulb on a bracket at 2.2 m. wallBase is on the wall face
        /// at floor level; yaw points out of the wall.
        static void RechargeLamp(Transform parent, string name, Vector3 wallBase, float yaw)
        {
            var g = GB.Group(parent, name, wallBase, yaw);
            GB.Box(g, "Backplate", new Vector3(0f, 2.25f, 0.02f), new Vector3(0.2f, 0.32f, 0.04f), metal, 0f, false);
            GB.Box(g, "Bracket", new Vector3(0f, 2.36f, 0.16f), new Vector3(0.04f, 0.04f, 0.28f), metal, 0f, false);
            GB.Cylinder(g, "Hood", new Vector3(0f, 2.33f, 0.28f), 0.13f, 0.05f, metal, false);
            GB.GlowLamp(g, "Bulb", new Vector3(0f, 2.2f, 0.28f), Amber, 0.08f, 3.5f, 1.0f);
        }

        // ---------------------------------------------------------------- stage props: doorframe, moon, trapdoor, stand-up

        static void StageProps(Transform sp)
        {
            // The first thing you see from the entrance: a lone painted doorframe standing in the dark at the moon pool's edge.
            SetDoorframe(sp, "SetDoorframe_Bedroom", new Vector3(1.2f, 0f, 4.4f), 0f, 1.4f);

            // Switchable set lamp 4/4: the moon. A paper moon hangs over centre stage; its spot (light 4 of 7) throws a hard pool
            // across the crossing in front of the tree and onto the tip of the empty show branch. Cross it and you are lit.
            var moon = GB.Group(sp, "SetLamp_Moon", new Vector3(0.2f, 4.6f, 5.2f));
            GB.Cylinder(moon, "PaperMoon", Vector3.zero, 0.7f, 0.06f, moonGlow, new Vector3(90f, 0f, 0f), false);
            GB.Box(moon, "Cable", new Vector3(0f, 1.05f, 0f), new Vector3(0.02f, 0.7f, 0.02f), GB.MatteBlack, 0f, false);
            GB.Box(moon, "SpotCan", new Vector3(0.7f, -0.22f, 0.8f), new Vector3(0.18f, 0.22f, 0.18f), GB.MatteBlack, 0f, false);
            GB.Box(moon, "SpotCable", new Vector3(0.7f, 0.645f, 0.8f), new Vector3(0.02f, 1.51f, 0.02f), GB.MatteBlack, 0f, false);
            GB.SpotLight(moon, "MoonSpot", new Vector3(0.7f, -0.35f, 0.8f), new Vector3(90f, 0f, 0f), new Color(0.72f, 0.8f, 1f), 6f, 60f, 2.0f);

            // Stage trapdoor (its gaps carry sound under the stage) with its red cue lamp.
            GB.Box(sp, "Trapdoor", new Vector3(4.4f, 0.006f, 4.0f), new Vector3(1.2f, 0.012f, 1.2f), wood, 0f, false);
            GB.Box(sp, "Trapdoor_Seam", new Vector3(4.4f, 0.014f, 4.0f), new Vector3(0.03f, 0.006f, 1.2f), blackFlat, 0f, false);
            GB.Box(sp, "TrapCueLamp_Post", new Vector3(5.2f, 0.15f, 3.3f), new Vector3(0.1f, 0.3f, 0.1f), GB.MatteBlack, 0f, false);
            GB.Sphere(sp, "TrapCueLamp", new Vector3(5.2f, 0.34f, 3.3f), 0.045f, trapRed, false);

            // "Ready or not!" stand-up of Peekins, arms over her head, in the dark under the deck's east edge.
            var st = GB.Group(sp, "Standee_ReadyOrNot", new Vector3(9.4f, 0f, 12.4f), 225f);
            GB.Box(st, "Base", new Vector3(0f, 0.04f, -0.1f), new Vector3(0.7f, 0.08f, 0.4f), ply);
            GB.Box(st, "Backboard", new Vector3(0f, 0.6f, -0.05f), new Vector3(0.5f, 1.05f, 0.05f), ply); // the stand-up's collider
            GB.Ellipsoid(st, "Romper", new Vector3(0f, 0.42f, 0f), new Vector3(0.62f, 0.66f, 0.05f), powderBlue, Vector3.zero, false);
            GB.Ellipsoid(st, "Head", new Vector3(0f, 0.92f, 0f), new Vector3(0.56f, 0.46f, 0.05f), oatmeal, Vector3.zero, false);
            GB.Box(st, "Arm_L", new Vector3(-0.2f, 1.33f, 0f), new Vector3(0.12f, 0.5f, 0.05f), oatmeal, new Vector3(0f, 0f, 12f));
            GB.Box(st, "Arm_R", new Vector3(0.2f, 1.33f, 0f), new Vector3(0.12f, 0.5f, 0.05f), oatmeal, new Vector3(0f, 0f, -12f));
            GB.Ellipsoid(st, "EyeRing_L", new Vector3(-0.12f, 0.95f, 0.03f), new Vector3(0.17f, 0.17f, 0.02f), bark, Vector3.zero, false);
            GB.Ellipsoid(st, "EyeRing_R", new Vector3(0.12f, 0.95f, 0.03f), new Vector3(0.17f, 0.17f, 0.02f), bark, Vector3.zero, false);
            GB.Sphere(st, "PaintedEye_L", new Vector3(-0.12f, 0.95f, 0.045f), 0.055f, eyeshine, false);
            GB.Sphere(st, "PaintedEye_R", new Vector3(0.12f, 0.95f, 0.045f), 0.055f, eyeshine, false);
            GB.Box(st, "Brace", new Vector3(0f, 0.45f, -0.2f), new Vector3(0.05f, 0.9f, 0.05f), ply, new Vector3(25f, 0f, 0f));

            // Felt "forest floor" round the tree: Peekins creeps silently here; her palms squeak on the bare boards elsewhere.
            GB.Cylinder(sp, "ForestFloorFelt", new Vector3(TrunkCenter.x, 0.01f, TrunkCenter.z), 4.2f, 0.02f, leafFelt, false);
            GB.Box(sp, "LooseBlock_D", new Vector3(6.6f, 0.2f, 3.6f), Vector3.one * 0.4f, blockRed, 10f);
            GB.Box(sp, "LooseBlock_E", new Vector3(7.0f, 0.2f, 3.1f), Vector3.one * 0.4f, blockYellow, -25f);
            GB.Box(sp, "LooseBlock_F", new Vector3(6.8f, 0.6f, 3.35f), Vector3.one * 0.4f, blockBlue, 35f);
            GB.Box(sp, "LooseBlock_G", new Vector3(-1.9f, 0.25f, 13.4f), Vector3.one * 0.5f, blockGreen, 15f);

            RechargeLamp(sp, "RechargeLamp_Stage", new Vector3(11.0f, 0f, 3.2f), 270f); // light 7 of 7
        }

        // ---------------------------------------------------------------- gameplay markers

        static void Markers(Transform mk)
        {
            GB.Marker(mk, MarkerKind.GloamieSpawn, new Vector3(5.8f, 0f, 10.6f), 0f,
                "Peekins: curled on her gold spike mark inside the Count-to-Ten hollow at the foot of the tree; the mouth faces upstage");

            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(0.6f, 0f, 9.0f), 0f, "Centre stage, between the ramp and the treehouse deck");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(4.6f, 0f, 14.9f), 90f, "Behind the tree, outside the hollow's mouth");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(-6.2f, 0f, 11.3f), 270f, "Crib room, between the cradles and the loft stairs");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(-7.2f, 0f, 3.6f), 0f, "Toy corner rug");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(-1.0f, 0f, 18.3f), 90f, "Backstage corridor, west half");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(6.0f, 0f, 18.2f), 270f, "Backstage corridor, breaker approach");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(4.0f, 0f, 3.0f), 0f, "Downstage, beside the trapdoor");
            GB.Marker(mk, MarkerKind.PatrolPoint, new Vector3(8.9f, DeckY, 10.4f), 0f, "Treehouse deck, east side");

            GB.Marker(mk, MarkerKind.ReelSpawn, new Vector3(8.9f, DeckY, 12.9f), 225f, "Reel: treehouse deck, NE corner in the trunk's shadow");
            GB.Marker(mk, MarkerKind.ReelSpawn, new Vector3(-8.4f, LoftY, 15.1f), 90f, "Reel: crib loft, beside Peekins's crib");
            GB.Marker(mk, MarkerKind.FuseSpawn, new Vector3(-9.4f, 0f, 1.8f), 90f, "Fuse: dark SW corner of the toy corner, between the toy chest and the count blocks");
            GB.Marker(mk, MarkerKind.BatterySpawn, new Vector3(-5.0f, 0.9f, 7.55f), 270f, "Battery: on the crib-room changing table (0.9 m)");
            GB.Marker(mk, MarkerKind.BatterySpawn, new Vector3(-3.6f, DeckY, 13.5f), 90f, "Battery: up on the Nest perch, against the crib-room flat");

            GB.Marker(mk, MarkerKind.Breaker, new Vector3(7.5f, 0f, 19.55f), 180f,
                "Wing breaker, backstage north wall: 10 s crescendo; approaches from the corridor (west) and the stage-right door behind you");
            GB.Marker(mk, MarkerKind.CueLampDesk, new Vector3(-3.3f, 0f, 17.75f), 0f,
                "Stage-manager desk under the prompt window; black-light cue lamp for Curtain Call");

            GB.Marker(mk, MarkerKind.HidingSpot, new Vector3(-10.675f, 0.1f, 7.9f), 90f, "Crib-room wardrobe, door ajar");
            GB.Marker(mk, MarkerKind.HidingSpot, new Vector3(-9.0f, 0.1f, 0.45f), 0f, "Toy chest prop trunk, lid open");
            GB.Marker(mk, MarkerKind.HidingSpot, new Vector3(-8.0f, 0.1f, 19.725f), 180f, "Backstage locker, door ajar");
            GB.Marker(mk, MarkerKind.HidingSpot, new Vector3(-5.7f, 0f, 19.3f), 180f, "Costume rack, between the hanging velvet blacks");
            GB.Marker(mk, MarkerKind.HidingSpot, new Vector3(-3.3f, 0f, 17.0f), 0f, "Under the stage-manager desk");

            GB.Marker(mk, MarkerKind.RechargeLamp, new Vector3(2.0f, 0f, 19.55f), 180f, "Amber wall lamp at 2.2 m, backstage north wall (the darker one: best Deepwick swap from Act IV)");
            GB.Marker(mk, MarkerKind.RechargeLamp, new Vector3(10.55f, 0f, 3.2f), 270f, "Amber wall lamp at 2.2 m, stage east wall beside the deck stairs");

            GB.Marker(mk, MarkerKind.Hatch, new Vector3(4.4f, 0f, 4.0f), 0f, "Stage trapdoor with red cue lamp, downstage of the tree");
            GB.Marker(mk, MarkerKind.Hatch, new Vector3(-6.6f, 0f, 7.6f), 0f, "Crib-room floor vent");
            GB.Marker(mk, MarkerKind.Hatch, new Vector3(5.0f, 0f, 18.3f), 0f, "Backstage floor vent");

            GB.Marker(mk, MarkerKind.Handprint, new Vector3(-9.5f, 0f, 11.8f), 180f, "0.95 m crawl gap in the crib loft's skirt: tunnel under the loft");
            GB.Marker(mk, MarkerKind.Handprint, new Vector3(-9.5f, 0f, 16.9f), 0f, "0.95 m crawl hole through the upstage wall, out into backstage");

            GB.Marker(mk, MarkerKind.StoryProp, new Vector3(1.3f, 0f, 6.1f), 225f, "Peekins's show branch: empty, bark rubbed bald where she clung, a frayed rope loop under the moon");
            GB.Marker(mk, MarkerKind.StoryProp, new Vector3(-9.45f, LoftY, 15.6f), 90f, "Peekins's nap crib: felt PEEKINS patch, a child-sized dent in the mattress");
            GB.Marker(mk, MarkerKind.StoryProp, new Vector3(-2.95f, 0.95f, 17.17f), 0f, "Count-to-Ten cue sheet on the SM desk: 'LX up - branch empty AGAIN. Keep her in the dark segments.'");
            GB.Marker(mk, MarkerKind.StoryProp, new Vector3(9.4f, 0f, 12.4f), 225f, "'Ready or not!' plywood stand-up of Peekins, arms up; its painted eyes still catch the light");
        }
    }
}
