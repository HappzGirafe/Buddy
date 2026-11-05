using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Digsley's Under-Stage Trap Room, directly beneath the Star Theatre. Root is on the lower floor under the theatre's centre.
    /// Act V "Cue the Trap!": a floor of trapdoors and vents (Digsley hears only through those gaps), a raised no-trap
    /// circuit (catwalk ring, prompt balcony, stuck stage lift), the felt Burrow set piece to loop around and crawl through,
    /// a quiet flat store for Tapwell's hide-and-seek, and the pump-corner breaker ringed by trapdoors.
    public static class TrapRoomBuilder
    {
        const float H = 5.2f;          // ceiling underside
        const float CwY = 2.4f;        // catwalk / balcony deck top
        const float CwIn = 11.3f;      // catwalk inner edge (deck runs from here to the wall at 13)
        const float PostLine = 11.36f; // catwalk posts and rails

        static readonly Color Pink = new Color(1f, 0.35f, 0.7f);
        static readonly Color LampAmber = new Color(1f, 0.72f, 0.38f);

        static Material mGrate, mSteel, mRail, mHatch, mFelt, mFeltDark, mCrimson, mChalk, mFlat, mHazard, mPump, mLocker, mBlueprint, mChain, mBrass;
        static Material mPink, mGoggle, mCueRed, mCueGreen, mUV, mAmber, mScreen;

        public static void Build(Transform root)
        {
            BuildShell(root);
            Materials();
            var g = GB.Group(root, "TrapRoom");
            BuildCatwalks(g);
            BuildBalcony(g);
            BuildLift(g);
            BuildBurrow(g);
            BuildHatches(g);
            BuildWinchGallery(g);
            BuildPumpCorner(g);
            BuildFlatStore(g);
            BuildCrateCorner(g);
            BuildStructure(g);
            BuildMarks(g);
            BuildMarkers(g);
        }

        /// Walls, floor, and a ceiling with the projector pit's square hole in it.
        static void BuildShell(Transform root)
        {
            float half = StarhouseLayout.TrapRoomHalf;
            float height = -StarhouseLayout.LowerFloorY + StarhouseLayout.HubSlabBottom - 0.3f; // ceiling slab ends at the theatre floor's underside
            var room = GB.Room(root, "Shell", Vector3.zero, new Vector3(2f * half, height, 2f * half), GB.DarkConcrete, GB.Concrete, null);

            float p = StarhouseLayout.PitHalf, outer = half + 0.3f, y = height + 0.15f;
            var ceiling = GB.Mat("Trap_Ceiling", new Color(0.12f, 0.1f, 0.09f));
            float band = outer - p;
            GB.Box(room, "Ceiling_N", new Vector3(0f, y, p + band * 0.5f), new Vector3(2f * outer, 0.3f, band), ceiling);
            GB.Box(room, "Ceiling_S", new Vector3(0f, y, -p - band * 0.5f), new Vector3(2f * outer, 0.3f, band), ceiling);
            GB.Box(room, "Ceiling_E", new Vector3(p + band * 0.5f, y, 0f), new Vector3(band, 0.3f, 2f * p), ceiling);
            GB.Box(room, "Ceiling_W", new Vector3(-p - band * 0.5f, y, 0f), new Vector3(band, 0.3f, 2f * p), ceiling);
        }

        static void Materials()
        {
            mGrate = GB.Mat("Trap_CatwalkGrate", new Color(0.11f, 0.11f, 0.12f), 0.35f, 0.6f);
            mSteel = GB.Mat("Trap_Steel", new Color(0.2f, 0.21f, 0.23f), 0.45f, 0.75f);
            mRail = GB.Mat("Trap_RailCrimson", new Color(0.32f, 0.07f, 0.08f), 0.3f, 0.3f);
            mHatch = GB.Mat("Trap_HatchPlate", new Color(0.19f, 0.14f, 0.1f), 0.2f, 0.2f);
            mFelt = GB.Mat("Trap_BurrowFelt", new Color(0.24f, 0.15f, 0.1f), 0.05f);
            mFeltDark = GB.Mat("Trap_BurrowFeltDark", new Color(0.12f, 0.08f, 0.06f), 0.05f);
            mCrimson = GB.Mat("Trap_FadedCrimson", new Color(0.4f, 0.11f, 0.13f), 0.15f);
            mChalk = GB.Mat("Trap_ChalkWhite", new Color(0.72f, 0.7f, 0.66f), 0.1f);
            mFlat = GB.Mat("Trap_FlatNightPaint", new Color(0.09f, 0.11f, 0.2f), 0.1f);
            mHazard = GB.Mat("Trap_HazardPaint", new Color(0.5f, 0.42f, 0.12f), 0.2f);
            mPump = GB.Mat("Trap_PumpGreen", new Color(0.13f, 0.19f, 0.15f), 0.35f, 0.4f);
            mLocker = GB.Mat("Trap_LockerGrey", new Color(0.22f, 0.25f, 0.24f), 0.3f, 0.5f);
            mBlueprint = GB.Mat("Trap_Blueprint", new Color(0.2f, 0.3f, 0.5f), 0.1f);
            mChain = GB.Mat("Trap_Chain", new Color(0.3f, 0.29f, 0.27f), 0.5f, 0.85f);
            mBrass = GB.Mat("Trap_Brass", new Color(0.55f, 0.42f, 0.18f), 0.6f, 0.8f);
            mPink = GB.Glow("Trap_SpikeTapePink", new Color(1f, 0.32f, 0.68f), 2.2f);
            mGoggle = GB.Glow("Trap_GoggleEyes", new Color(1f, 0.78f, 0.9f), 1.8f);
            mCueRed = GB.Glow("Trap_CueLampRed", new Color(1f, 0.07f, 0.04f), 2.5f);
            mCueGreen = GB.Glow("Trap_CueLampGreen", new Color(0.2f, 1f, 0.3f), 1.2f);
            mUV = GB.Glow("Trap_BlackLightCue", new Color(0.55f, 0.2f, 1f), 3f);
            mAmber = GB.Glow("Trap_BreakerAmber", new Color(1f, 0.55f, 0.15f), 2f);
            mScreen = GB.Glow("Trap_MonitorMint", new Color(0.45f, 1f, 0.75f), 1.2f);
        }

        // ---------------------------------------------------------------- reusable props

        /// Steel post from the floor up through the deck edge to rail height.
        static void Post(Transform p, string name, float x, float z)
        {
            float h = CwY + 1.05f;
            GB.Box(p, name, new Vector3(x, h * 0.5f, z), new Vector3(0.12f, h, 0.12f), mSteel);
        }

        /// Top and mid rail between two points at deck level.
        static void Rail(Transform p, string name, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            d.y = 0f;
            float len = d.magnitude;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            Vector3 mid = (a + b) * 0.5f;
            GB.Box(p, name + "_Top", new Vector3(mid.x, a.y + 1f, mid.z), new Vector3(0.07f, 0.07f, len), mRail, yaw);
            GB.Box(p, name + "_Mid", new Vector3(mid.x, a.y + 0.5f, mid.z), new Vector3(0.07f, 0.07f, len), mRail, yaw);
        }

        /// Slightly raised trapdoor plate (hinges on local -Z) with its small red cue lamp beside the front-right corner.
        static Transform Trapdoor(Transform parent, string name, Vector3 pos, float yaw, float size, string note)
        {
            var g = GB.Group(parent, name, pos, yaw);
            float h = size * 0.5f;
            GB.Box(g, "Plate", new Vector3(0f, 0.03f, 0f), new Vector3(size, 0.06f, size), mHatch);
            GB.Box(g, "Hinge_L", new Vector3(-h * 0.5f, 0.066f, -h + 0.07f), new Vector3(0.32f, 0.012f, 0.07f), mSteel, collider: false);
            GB.Box(g, "Hinge_R", new Vector3(h * 0.5f, 0.066f, -h + 0.07f), new Vector3(0.32f, 0.012f, 0.07f), mSteel, collider: false);
            GB.Box(g, "TrapNumber", new Vector3(-h * 0.45f, 0.063f, h - 0.16f), new Vector3(0.3f, 0.006f, 0.1f), mChalk, collider: false);
            GB.Box(g, "RingPull", new Vector3(h * 0.4f, 0.068f, h - 0.16f), new Vector3(0.14f, 0.016f, 0.05f), mSteel, collider: false);
            Vector3 lamp = new Vector3(h + 0.22f, 0f, h - 0.12f);
            GB.Box(g, "CueLamp_Stand", lamp + new Vector3(0f, 0.2f, 0f), new Vector3(0.07f, 0.4f, 0.07f), GB.MatteBlack, collider: false);
            GB.Sphere(g, "CueLamp_Red", lamp + new Vector3(0f, 0.45f, 0f), 0.06f, mCueRed, false);
            GB.Marker(g, MarkerKind.Hatch, new Vector3(0f, 0.06f, 0f), 0f, note);
            return g;
        }

        /// Flush floor vent grille. It carries sound down to Digsley but he cannot come up through it.
        static Transform Vent(Transform parent, string name, Vector3 pos, float yaw, string note, bool tentacles)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Box(g, "Hole", new Vector3(0f, 0.004f, 0f), new Vector3(1.1f, 0.008f, 0.6f), GB.Velvet, collider: false);
            GB.Box(g, "Frame_N", new Vector3(0f, 0.015f, 0.32f), new Vector3(1.2f, 0.03f, 0.06f), mSteel, collider: false);
            GB.Box(g, "Frame_S", new Vector3(0f, 0.015f, -0.32f), new Vector3(1.2f, 0.03f, 0.06f), mSteel, collider: false);
            for (int i = 0; i < 5; i++)
                GB.Box(g, $"Bar{i}", new Vector3(-0.5f + 0.25f * i, 0.016f, 0f), new Vector3(0.05f, 0.03f, 0.6f), mSteel, collider: false);
            if (tentacles)
                for (int i = 0; i < 4; i++)
                {
                    float x = -0.375f + 0.25f * i;
                    float side = i % 2 == 0 ? 1f : -1f;
                    GB.Ellipsoid(g, $"TentacleTip{i}", new Vector3(x, 0.09f, 0.06f * side), new Vector3(0.05f, 0.18f, 0.05f), mPink, new Vector3(12f * side, 0f, (i - 1.5f) * 8f), false); // tilted tip bottoms stay above y 0
                }
            GB.Marker(g, MarkerKind.Hatch, Vector3.zero, 0f, note);
            return g;
        }

        /// Glowing pink spike-tape X: a performer's mark (where Gloamies stop for a Curtain Call bow). pos.y = the surface.
        static void SpikeX(Transform p, string name, Vector3 pos, float yaw = 0f)
        {
            Vector3 c = pos + Vector3.up * 0.004f;
            GB.Box(p, name + "_a", c, new Vector3(0.05f, 0.006f, 0.45f), mPink, yaw + 45f, false);
            GB.Box(p, name + "_b", c, new Vector3(0.05f, 0.006f, 0.45f), mPink, yaw - 45f, false);
        }

        /// A-frame flat cart: two painted set flats leaning together over a low cart. Crouch inside it to hide.
        static Transform AFrame(Transform parent, string name, Vector3 pos, float yaw)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Box(g, "Cart", new Vector3(0f, 0.075f, 0f), new Vector3(1.7f, 0.15f, 3f), GB.Wood);
            const float lean = 14f, len = 2.9f, b = 0.8f;
            float s = Mathf.Sin(lean * Mathf.Deg2Rad), c = Mathf.Cos(lean * Mathf.Deg2Rad);
            float cx = b - s * len * 0.5f, cy = 0.15f + c * len * 0.5f;
            GB.Box(g, "Flat_R", new Vector3(cx, cy, 0f), new Vector3(0.06f, len, 3f), mFlat, new Vector3(0f, 0f, lean));
            GB.Box(g, "Flat_L", new Vector3(-cx, cy, 0f), new Vector3(0.06f, len, 3f), mFlat, new Vector3(0f, 0f, -lean));
            GB.Box(g, "Ridge", new Vector3(0f, 0.15f + c * len, 0f), new Vector3(0.32f, 0.12f, 3.1f), GB.Wood, collider: false);
            GB.Box(g, "CartNumber", new Vector3(0.856f, 0.075f, 0.9f), new Vector3(0.01f, 0.08f, 0.6f), mChalk, collider: false);
            return g;
        }

        /// Chain hanging from a ceiling block, ending in a hook or a sandbag counterweight. All of it stays above 2.3 m.
        static void HangingChain(Transform p, string name, float x, float z, float bottom, bool sandbag)
        {
            var g = GB.Group(p, name, new Vector3(x, 0f, z));
            GB.Box(g, "Block", new Vector3(0f, 5.05f, 0f), new Vector3(0.16f, 0.22f, 0.12f), mSteel, collider: false);
            GB.Box(g, "Chain", new Vector3(0f, (4.94f + bottom) * 0.5f, 0f), new Vector3(0.035f, 4.94f - bottom, 0.035f), mChain, collider: false);
            if (sandbag) GB.Box(g, "Sandbag", new Vector3(0f, bottom - 0.2f, 0f), new Vector3(0.28f, 0.4f, 0.28f), GB.Cloth, collider: false);
            else GB.Box(g, "Hook", new Vector3(0f, bottom - 0.07f, 0f), new Vector3(0.05f, 0.14f, 0.03f), mSteel, collider: false);
        }

        // ---------------------------------------------------------------- the no-trap circuit

        /// Catwalk ring along all four walls at 2.4 m (walkable floor continues underneath, 2.25 m clear).
        static void BuildCatwalks(Transform parent)
        {
            var g = GB.Group(parent, "CatwalkRing");
            float t = 0.15f, yc = CwY - t * 0.5f, w = 13f - CwIn, mid = (13f + CwIn) * 0.5f, inner = 2f * CwIn;
            GB.Box(g, "Deck_N", new Vector3(0f, yc, mid), new Vector3(26f, t, w), mGrate);
            GB.Box(g, "Deck_S", new Vector3(0f, yc, -mid), new Vector3(26f, t, w), mGrate);
            GB.Box(g, "Deck_E", new Vector3(mid, yc, 0f), new Vector3(w, t, inner), mGrate);
            GB.Box(g, "Deck_W", new Vector3(-mid, yc, 0f), new Vector3(w, t, inner), mGrate);

            float L = PostLine;
            Post(g, "Post_NE", L, L);
            Post(g, "Post_NW", -L, L);
            Post(g, "Post_SE", L, -L);
            Post(g, "Post_SW", -L, -L);
            float[] along = { -6.9f, -2.3f, 2.3f, 6.9f };
            foreach (float s in along)
            {
                Post(g, $"Post_E_{s:0.0}", L, s);
                Post(g, $"Post_W_{s:0.0}", -L, s);
                Post(g, $"Post_S_{s:0.0}", s, -L);
                if (Mathf.Abs(s) > 3f) Post(g, $"Post_N_{s:0.0}", s, L); // the prompt balcony takes the middle of the north run
            }

            // rails, with gaps at the prompt balcony, the lift stair, the west stair and one broken section (a loud drop-down)
            Rail(g, "Rail_N1", new Vector3(-L, CwY, L), new Vector3(-3.2f, CwY, L));
            Rail(g, "Rail_N2", new Vector3(3.2f, CwY, L), new Vector3(L, CwY, L));
            Rail(g, "Rail_E1", new Vector3(L, CwY, -L), new Vector3(L, CwY, -0.8f));
            Rail(g, "Rail_E2", new Vector3(L, CwY, 0.8f), new Vector3(L, CwY, L));
            Rail(g, "Rail_S1", new Vector3(-L, CwY, -L), new Vector3(-0.6f, CwY, -L));
            Rail(g, "Rail_S2", new Vector3(0.6f, CwY, -L), new Vector3(L, CwY, -L));
            Rail(g, "Rail_W1", new Vector3(-L, CwY, -L), new Vector3(-L, CwY, -5.4f));
            Rail(g, "Rail_W2", new Vector3(-L, CwY, -3.8f), new Vector3(-L, CwY, L));

            // west stair: floor at x -8.9 up to the west deck at x -11.3 (z -5.4..-3.8)
            GB.Stairs(g, "Stair_W", new Vector3(-(CwIn - 2.4f), 0f, -4.6f), 270f, 8, 0.3f, 0.3f, 1.6f, mGrate);

            // black velvet masking on the walls above the catwalk
            GB.Box(g, "Masking_S", new Vector3(0f, 3.7f, -12.97f), new Vector3(25f, 2.6f, 0.04f), GB.Velvet, collider: false);
            GB.Box(g, "Masking_E", new Vector3(12.97f, 3.7f, 0f), new Vector3(0.04f, 2.6f, 25f), GB.Velvet, collider: false);
            GB.Box(g, "Masking_W", new Vector3(-12.97f, 3.7f, 0f), new Vector3(0.04f, 2.6f, 25f), GB.Velvet, collider: false);
        }

        /// Prompt balcony off the north catwalk, overlooking the pit, with the stage-manager desk. Crew bench underneath.
        static void BuildBalcony(Transform parent)
        {
            var g = GB.Group(parent, "PromptBalcony");
            GB.Box(g, "Deck", new Vector3(0f, CwY - 0.075f, 10.1f), new Vector3(6.4f, 0.15f, 2.4f), mGrate); // x -3.2..3.2, z 8.9..11.3
            Post(g, "Post_W", -1.54f, 8.96f);
            Post(g, "Post_E", 3.14f, 8.96f);
            Rail(g, "Rail_Front", new Vector3(-1.6f, CwY, 8.96f), new Vector3(3.2f, CwY, 8.96f));
            Rail(g, "Rail_SideW", new Vector3(-3.17f, CwY, 8.9f), new Vector3(-3.17f, CwY, 11.3f));
            Rail(g, "Rail_SideE", new Vector3(3.17f, CwY, 8.96f), new Vector3(3.17f, CwY, 11.3f));
            // x -3.2..-1.6, z 6.5..8.9; landing between the west side rail and Post_W is 1.53 m clear
            GB.Stairs(g, "Stair_Balcony", new Vector3(-2.4f, 0f, 6.5f), 0f, 8, 0.3f, 0.3f, 1.6f, mGrate);

            // stage-manager (prompt) desk, facing south over the trap floor; knee space open to the north
            var desk = GB.Group(g, "PromptDesk", new Vector3(0.6f, CwY, 10f));
            GB.Box(desk, "Top", new Vector3(0f, 1.07f, 0f), new Vector3(2f, 0.06f, 0.8f), GB.Wood);
            GB.Box(desk, "Front", new Vector3(0f, 0.52f, -0.37f), new Vector3(2f, 1.04f, 0.06f), GB.MatteBlack);
            GB.Box(desk, "Side_W", new Vector3(-0.97f, 0.52f, 0f), new Vector3(0.06f, 1.04f, 0.8f), GB.MatteBlack);
            GB.Box(desk, "Side_E", new Vector3(0.97f, 0.52f, 0f), new Vector3(0.06f, 1.04f, 0.8f), GB.MatteBlack);
            GB.Box(desk, "CuePanel", new Vector3(0f, 1.25f, -0.33f), new Vector3(1.4f, 0.3f, 0.06f), GB.MatteBlack, collider: false);
            for (int i = 0; i < 6; i++)
                GB.Box(desk, $"CueDot{i}", new Vector3(-0.5f + 0.2f * i, 1.25f, -0.29f), new Vector3(0.07f, 0.07f, 0.02f), i % 2 == 0 ? mCueRed : mCueGreen, collider: false);
            GB.Box(desk, "BlackLight_Neck", new Vector3(0.8f, 1.27f, 0.1f), new Vector3(0.03f, 0.34f, 0.03f), mSteel, collider: false);
            GB.Sphere(desk, "BlackLight_Bulb", new Vector3(0.8f, 1.48f, 0.1f), 0.07f, mUV, false);
            GB.PointLight(desk, "BlackLight_Glow", new Vector3(0.8f, 1.55f, 0.1f), new Color(0.55f, 0.25f, 1f), 3.5f, 0.8f);
            GB.Cylinder(desk, "StageBell", new Vector3(-0.7f, 1.13f, 0.1f), 0.07f, 0.06f, mBrass, false);
            GB.Sphere(desk, "StageBell_Dome", new Vector3(-0.7f, 1.18f, 0.1f), 0.055f, mBrass, false);
            GB.Box(desk, "Clipboard", new Vector3(-0.4f, 1.108f, 0.1f), new Vector3(0.24f, 0.015f, 0.32f), GB.Plywood, collider: false);
            GB.Box(desk, "CueSheet", new Vector3(-0.4f, 1.118f, 0.1f), new Vector3(0.2f, 0.004f, 0.26f), mChalk, collider: false);
            GB.Marker(g, MarkerKind.CueLampDesk, new Vector3(0.6f, CwY, 10.6f), 0f, "Prompt desk with the black-light cue lamp; balcony is no-trap ground");
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(0.6f, CwY, 10f), 0f, "Under the prompt desk (1.04 m knee space, open to the north)");
            GB.Marker(g, MarkerKind.StoryProp, new Vector3(0.2f, CwY + 1.1f, 10.1f), 0f, "Cue the Trap! cue sheet, 21 Dec 1997: trap cues ticked off, a last cue pencilled in: PIT - GO");

            // crew bench under the balcony (floor level, 2.25 m ceiling)
            GB.Box(g, "CrewBench", new Vector3(0f, 0.45f, 12.6f), new Vector3(2.4f, 0.9f, 0.8f), GB.Wood);
            GB.Cylinder(g, "SashCordCoil", new Vector3(-0.1f, 0.94f, 12.5f), 0.15f, 0.08f, GB.Cloth, false);
            GB.Box(g, "Blueprint", new Vector3(-0.6f, 1.55f, 12.985f), new Vector3(1.2f, 0.8f, 0.03f), mBlueprint, collider: false);
            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(0.5f, 0.9f, 12.55f), 180f, "Crew bench under the prompt balcony");
            GB.Marker(g, MarkerKind.StoryProp, new Vector3(-0.6f, 0.9f, 12.55f), 180f, "Trap-room blueprint: it shows six trapdoors; the floor has eight");
        }

        /// A stage lift stuck at 1.2 m: no-trap platform on hydraulic rams, a child-size crawl space under it, stairs up from
        /// the pit side and on up to the east catwalk.
        static void BuildLift(Transform parent)
        {
            var g = GB.Group(parent, "StuckStageLift");
            GB.Box(g, "Deck", new Vector3(8.65f, 1.1f, 0f), new Vector3(5.3f, 0.2f, 4f), GB.Wood); // x 6..11.3, z -2..2, underside 1.0
            GB.Box(g, "HazardEdge_N", new Vector3(8.65f, 1.205f, 1.94f), new Vector3(5.3f, 0.01f, 0.12f), mHazard, collider: false);
            GB.Box(g, "HazardEdge_S", new Vector3(8.65f, 1.205f, -1.94f), new Vector3(5.3f, 0.01f, 0.12f), mHazard, collider: false);
            GB.Box(g, "HazardEdge_W", new Vector3(6.06f, 1.205f, 0f), new Vector3(0.12f, 0.01f, 4f), mHazard, collider: false);
            GB.Cylinder(g, "MainRam", new Vector3(8.65f, 0.5f, 0f), 0.4f, 1f, mSteel);
            float[] rx = { 6.4f, 10.9f };
            float[] rz = { -1.6f, 1.6f };
            int n = 0;
            foreach (float x in rx)
                foreach (float z in rz)
                    GB.Cylinder(g, $"Ram{n++}", new Vector3(x, 0.5f, z), 0.16f, 1f, mSteel);
            GB.Box(g, "Guide_SW", new Vector3(5.88f, H * 0.5f, -2.12f), new Vector3(0.24f, H, 0.24f), mSteel);
            GB.Box(g, "Guide_NW", new Vector3(5.88f, H * 0.5f, 2.12f), new Vector3(0.24f, H, 0.24f), mSteel);
            GB.Stairs(g, "Stair_Lift", new Vector3(4.8f, 0f, 0f), 90f, 4, 0.3f, 0.3f, 1.6f, mGrate);            // floor -> deck, x 4.8..6.0
            GB.Stairs(g, "Stair_LiftToCatwalk", new Vector3(10.1f, 1.2f, 0f), 90f, 4, 0.3f, 0.3f, 1.6f, mGrate); // deck -> east catwalk, x 10.1..11.3
            GB.Marker(g, MarkerKind.Handprint, new Vector3(8.2f, 0f, -2.3f), 180f, "Child-size crawl under the stuck lift (1.0 m clear), south mouth");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(8.2f, 0f, 2.3f), 0f, "Child-size crawl under the stuck lift (1.0 m clear), north mouth");
        }

        /// Digsley's Burrow: a round felt-and-plywood show set you can circle, with child-size crawl doors north and south.
        /// The north door wears his star-nose ring and painted goggle eyes, facing the pit.
        static void BuildBurrow(Transform parent)
        {
            var g = GB.Group(parent, "Burrow", new Vector3(0f, 0f, -7.5f));
            const float r = 2.2f, wallH = 2f, th = 0.15f, door = 15f, crawl = 1f;
            GB.ArcWall(g, "Wall_E", Vector3.zero, r, 0f, wallH, th, door, 180f - door, mFelt, 15f);
            GB.ArcWall(g, "Wall_W", Vector3.zero, r, 0f, wallH, th, 180f + door, 360f - door, mFelt, 15f);
            GB.ArcWall(g, "Lintel_N", Vector3.zero, r, crawl, wallH - crawl, th, -door, door, mFelt, 15f);
            GB.ArcWall(g, "Lintel_S", Vector3.zero, r, crawl, wallH - crawl, th, 180f - door, 180f + door, mFelt, 15f);
            GB.Cylinder(g, "Roof", new Vector3(0f, wallH + 0.06f, 0f), r + 0.15f, 0.12f, mFeltDark);
            GB.Cylinder(g, "Mound1", new Vector3(0f, 2.29f, 0f), 1.8f, 0.34f, mFelt, false);
            GB.Cylinder(g, "Mound2", new Vector3(0f, 2.61f, 0f), 1.2f, 0.3f, mFelt, false);
            GB.Cylinder(g, "Mound3", new Vector3(0f, 2.87f, 0f), 0.55f, 0.22f, mFeltDark, false);
            GB.Box(g, "PopUpTrap", new Vector3(0f, 2.99f, 0f), new Vector3(0.7f, 0.04f, 0.7f), mCrimson, collider: false);

            // star-nose ring of pink tentacles around the north crawl door
            for (int i = 0; i <= 12; i++)
            {
                float a = (-120f + 20f * i) * Mathf.Deg2Rad;
                float x = 0.85f * Mathf.Sin(a), y = 0.55f + 0.85f * Mathf.Cos(a);
                float z = Mathf.Sqrt(2.31f * 2.31f - x * x);
                float yaw = Mathf.Asin(x / 2.31f) * Mathf.Rad2Deg;
                GB.Ellipsoid(g, $"Tentacle{i:00}", new Vector3(x, y, z), new Vector3(0.09f, 0.09f, 0.24f), mPink, new Vector3(0f, yaw, 0f), false);
            }
            // painted cartoon goggle eyes above the door
            for (int s = -1; s <= 1; s += 2)
            {
                string side = s < 0 ? "L" : "R";
                float x = 0.48f * s;
                GB.Cylinder(g, "GoggleRim" + side, new Vector3(x, 1.62f, 2.25f), 0.36f, 0.05f, GB.MatteBlack, new Vector3(90f, 0f, 0f), false);
                GB.Cylinder(g, "GoggleEye" + side, new Vector3(x, 1.62f, 2.285f), 0.29f, 0.04f, mGoggle, new Vector3(90f, 0f, 0f), false);
                GB.Cylinder(g, "GogglePupil" + side, new Vector3(x - 0.06f * s, 1.57f, 2.31f), 0.11f, 0.03f, GB.MatteBlack, new Vector3(90f, 0f, 0f), false);
            }
            GB.Box(g, "Sign", new Vector3(0f, 2.34f, 2.2f), new Vector3(1.8f, 0.36f, 0.05f), mCrimson, collider: false);
            GB.Box(g, "Sign_Letters", new Vector3(0f, 2.34f, 2.23f), new Vector3(1.4f, 0.09f, 0.01f), mChalk, collider: false);
            GB.PointLight(g, "NoseGlow", new Vector3(0f, 1.2f, 2.8f), Pink, 4.5f, 1f);

            // inside (reached only by crawling): a felt bedroll
            GB.Box(g, "Bedroll", new Vector3(-1f, 0.06f, 0.2f), new Vector3(0.7f, 0.12f, 1.6f), GB.Cloth, collider: false);
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(-1f, 0f, 0f), 90f, "Inside the Burrow: crawl-in only, roomy (4 m across) - Tapwell can only probe from outside");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(0f, 0f, 2.5f), 0f, "Child-size crawl door (1.0 m) through the Burrow, north mouth");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(0f, 0f, -2.5f), 180f, "Child-size crawl door (1.0 m) through the Burrow, south mouth");
        }

        /// Eight trapdoors (one is a lift panel) and three vents: the hidden Hatch graph Digsley travels and listens through.
        static void BuildHatches(Transform parent)
        {
            var g = GB.Group(parent, "Hatches");
            Trapdoor(g, "Trap1", new Vector3(-3.7f, 0f, -8.2f), 90f, 1.4f, "Trap 1: west side of the Burrow loop");
            Trapdoor(g, "Trap2", new Vector3(4.6f, 0f, -9.4f), 0f, 1.4f, "Trap 2: east side of the Burrow loop");
            Trapdoor(g, "Trap3", new Vector3(-6f, 0f, 0.5f), 90f, 1.4f, "Trap 3: west arm, between the pit and the flat store");
            Trapdoor(g, "Trap4", new Vector3(9.2f, 0f, -5.4f), 0f, 1.4f, "Trap 4: beside the lamp pier");
            var t5 = Trapdoor(g, "Trap5", new Vector3(8f, 0f, 5.8f), 180f, 1.4f, "Trap 5: winch gallery, under the clanking chains");
            GB.Marker(t5, MarkerKind.GloamieSpawn, new Vector3(0f, 0.06f, 0f), 0f, "Digsley starts under Trap 5");
            Trapdoor(g, "LiftPanel6", new Vector3(1.8f, 0f, 7.2f), 180f, 1.8f, "Lift panel 6: stage-lift plate in front of the prompt balcony");
            Trapdoor(g, "Trap7", new Vector3(-9f, 0f, -9.6f), 90f, 1.4f, "Trap 7: crate corner");
            Trapdoor(g, "Trap8", new Vector3(5.6f, 0f, -12.1f), 0f, 1.4f, "Trap 8: under the south catwalk, 3 m from the breaker - wedge it before throwing the breaker");
            Vent(g, "Vent1", new Vector3(10.4f, 0f, 7.6f), 0f, "Floor vent 1 (winch gallery): carries sound, no eruption", false);
            Vent(g, "Vent2", new Vector3(4.4f, 0f, 10.6f), 0f, "Floor vent 2 (behind the winch bed): carries sound, no eruption", false);
            Vent(g, "Vent3", new Vector3(0f, 0f, -11f), 0f, "Floor vent 3 behind the Burrow: a pink tentacle tip feels up through it", true);
        }

        /// Trap winches on a timber bed: drums, chains up to ceiling pulleys and along the ceiling toward the pit.
        static void BuildWinchGallery(Transform parent)
        {
            var g = GB.Group(parent, "WinchGallery");
            GB.Box(g, "WinchBed", new Vector3(7.2f, 0.15f, 9.6f), new Vector3(6f, 0.3f, 0.9f), GB.Wood); // x 4.2..10.2, z 9.15..10.05
            float[] ux = { 4.35f, 6.25f, 8.15f, 10.05f };
            for (int i = 0; i < ux.Length; i++)
            {
                GB.Box(g, $"Upright{i}", new Vector3(ux[i], 0.95f, 9.6f), new Vector3(0.15f, 1.3f, 0.9f), GB.Wood);
                GB.Box(g, $"SpikeTape{i}", new Vector3(ux[i], 1.25f, 9.6f), new Vector3(0.17f, 0.05f, 0.92f), mPink, collider: false);
            }
            float[] dx = { 5.3f, 7.2f, 9.1f };
            for (int i = 0; i < dx.Length; i++)
            {
                float x = dx[i];
                GB.Cylinder(g, $"Drum{i}", new Vector3(x, 1.15f, 9.6f), 0.32f, 1.6f, mSteel, new Vector3(0f, 0f, 90f));
                GB.Box(g, $"Crank{i}", new Vector3(x + 0.55f, 1.15f, 9.1f), new Vector3(0.05f, 0.05f, 0.4f), mSteel, collider: false);
                GB.Box(g, $"LiftChain{i}", new Vector3(x, 3.045f, 9.6f), new Vector3(0.04f, 3.15f, 0.04f), mChain, collider: false);
                GB.Cylinder(g, $"Pulley{i}", new Vector3(x, 4.62f, 9.6f), 0.2f, 0.07f, mSteel, new Vector3(0f, 0f, 90f), false);
                GB.Box(g, $"RunChain{i}", new Vector3(x, 4.75f, 6.8f), new Vector3(0.04f, 0.04f, 5.6f), mChain, collider: false);
                GB.Cylinder(g, $"HeadPulley{i}", new Vector3(x, 4.62f, 4f), 0.16f, 0.06f, mSteel, new Vector3(0f, 0f, 90f), false);
            }
            GB.PointLight(g, "WinchGlow", new Vector3(7.2f, 2.6f, 8.2f), Pink, 5f, 0.8f);

            GB.Box(g, "ToolCart", new Vector3(4.7f, 0.42f, 7.6f), new Vector3(0.9f, 0.84f, 0.6f), GB.MatteBlack);
            GB.Cylinder(g, "GaffRoll", new Vector3(4.42f, 0.875f, 7.5f), 0.07f, 0.07f, GB.Cloth, false);
            GB.Marker(g, MarkerKind.FuseSpawn, new Vector3(4.75f, 0.84f, 7.65f), 270f, "On the winch tool cart (0.84 m); Trap 5 and lift panel 6 in walk range");

            var mc = GB.Group(g, "FieldMonitorCart", new Vector3(10.2f, 0f, 4f), 270f);
            GB.Box(mc, "Cart", new Vector3(0f, 0.4f, 0f), new Vector3(0.7f, 0.8f, 0.6f), GB.MatteBlack);
            GB.Box(mc, "CRT", new Vector3(0f, 1.03f, -0.02f), new Vector3(0.55f, 0.46f, 0.5f), GB.MatteBlack);
            GB.Box(mc, "Screen", new Vector3(0f, 1.05f, 0.24f), new Vector3(0.42f, 0.32f, 0.02f), mScreen, collider: false);
            GB.Marker(mc, MarkerKind.MonitorCart, Vector3.zero, 0f, "Hesper's field monitor (Act V): switch it off within 4 s");
        }

        /// SE corner under the catwalks: the humming lift pump, the main breaker, a crew locker and the fallen-sandbag reel.
        static void BuildPumpCorner(Transform parent)
        {
            var g = GB.Group(parent, "PumpCorner");
            GB.Box(g, "HydraulicPump", new Vector3(11.6f, 0.7f, -12.2f), new Vector3(2.4f, 1.4f, 1.2f), mPump); // x 10.4..12.8, z -12.8..-11.6
            GB.Cylinder(g, "PumpMotor", new Vector3(11.5f, 1.72f, -12.2f), 0.32f, 1f, mPump, new Vector3(0f, 0f, 90f));
            GB.Box(g, "PumpRunLamp", new Vector3(10.8f, 1.15f, -11.59f), new Vector3(0.12f, 0.08f, 0.02f), mCueGreen, collider: false);
            GB.Cylinder(g, "Accumulator1", new Vector3(12.5f, 1f, -10.4f), 0.3f, 2f, mPump);
            GB.Cylinder(g, "Accumulator2", new Vector3(12.5f, 1f, -9.6f), 0.3f, 2f, mPump);
            GB.Box(g, "Pipe_Wall", new Vector3(12.86f, 1.9f, -6.8f), new Vector3(0.12f, 0.12f, 9.6f), mSteel, collider: false);
            GB.Box(g, "Pipe_Drop", new Vector3(12.86f, 0.95f, -2f), new Vector3(0.12f, 1.9f, 0.12f), mSteel, collider: false);

            GB.Box(g, "Breaker_Panel", new Vector3(8.6f, 1.5f, -12.88f), new Vector3(0.9f, 1.2f, 0.24f), mSteel);
            GB.Box(g, "Breaker_Stripe", new Vector3(8.6f, 2.16f, -12.88f), new Vector3(0.9f, 0.1f, 0.24f), mHazard, collider: false);
            GB.Box(g, "Breaker_Lever", new Vector3(8.92f, 1.45f, -12.72f), new Vector3(0.07f, 0.4f, 0.07f), mCrimson, new Vector3(-25f, 0f, 0f), false);
            GB.Box(g, "Breaker_Lamp", new Vector3(8.35f, 1.9f, -12.755f), new Vector3(0.1f, 0.1f, 0.02f), mAmber, collider: false);
            GB.PointLight(g, "Breaker_Glow", new Vector3(8.6f, 2.05f, -12.3f), new Color(1f, 0.5f, 0.15f), 3f, 0.8f);
            GB.Marker(g, MarkerKind.Breaker, new Vector3(8.6f, 0f, -12.45f), 0f, "Main breaker under the south catwalk: Traps 8, 2 and 4 hear its crescendo - wedge them first, escape up the lift stairs");

            var lk = GB.Group(g, "CrewLocker", new Vector3(9.8f, 0f, -12.6f)); // x 9.2..10.4, z -13..-12.2
            GB.Box(lk, "Back", new Vector3(0f, 1.05f, -0.375f), new Vector3(1.2f, 2.1f, 0.05f), mLocker);
            GB.Box(lk, "Side_W", new Vector3(-0.575f, 1.05f, 0f), new Vector3(0.05f, 2.1f, 0.8f), mLocker);
            GB.Box(lk, "Side_E", new Vector3(0.575f, 1.05f, 0f), new Vector3(0.05f, 2.1f, 0.8f), mLocker);
            GB.Box(lk, "Top", new Vector3(0f, 2.075f, 0f), new Vector3(1.2f, 0.05f, 0.8f), mLocker);
            GB.Box(lk, "Door", new Vector3(-0.575f, 1f, 0.7f), new Vector3(0.06f, 1.95f, 0.6f), mLocker);
            GB.Marker(lk, MarkerKind.HidingSpot, Vector3.zero, 0f, "Double crew locker beside the humming pump (Tapwell gets one fewer probe here)");

            GB.Box(g, "FallenSandbag1", new Vector3(10.3f, 0.15f, -8.7f), new Vector3(0.6f, 0.3f, 0.35f), GB.Cloth, 20f);
            GB.Box(g, "FallenSandbag2", new Vector3(10.2f, 0.42f, -8.6f), new Vector3(0.55f, 0.25f, 0.33f), GB.Cloth, -10f);
            GB.Box(g, "SnappedRope", new Vector3(9.9f, 0.01f, -8.1f), new Vector3(0.03f, 0.02f, 0.9f), GB.Cloth, 35f, false);
        }

        /// NW quarter: A-frame carts of stored set flats and a promo flat. No hatch within 6 m: Digsley can't hear you here,
        /// so this is Tapwell's hide-and-seek ground.
        static void BuildFlatStore(Transform parent)
        {
            var g = GB.Group(parent, "FlatStore");
            var f1 = AFrame(g, "FlatCart1", new Vector3(-9.4f, 0f, 7.2f), 0f);   // x -10.25..-8.55, z 5.7..8.7
            GB.Marker(f1, MarkerKind.HidingSpot, new Vector3(0f, 0.15f, 0.3f), 0f, "Inside flat cart 1 (crouch between the leaning flats); quiet floor");
            var f2 = AFrame(g, "FlatCart2", new Vector3(-9.4f, 0f, 1.4f), 0f);   // x -10.25..-8.55, z -0.1..2.9
            GB.Marker(f2, MarkerKind.HidingSpot, new Vector3(0f, 0.15f, -0.3f), 180f, "Inside flat cart 2; Trap 3 is 3.5 m away, stay crouched");
            AFrame(g, "FlatCart3", new Vector3(-6f, 0f, 10.4f), 90f);           // x -7.5..-4.5, z 9.55..11.25

            // promo flat leaning on the NW pier, Digsley's star nose and goggle eyes painted in Evergleam
            const float lean = 8f;
            var promo = new Vector3(-7.14f, 1.49f, 6.5f);
            GB.Box(g, "PromoFlat", promo, new Vector3(0.05f, 3f, 2f), GB.Plywood, new Vector3(0f, 0f, -lean));
            Vector3 up = new Vector3(Mathf.Sin(lean * Mathf.Deg2Rad), Mathf.Cos(lean * Mathf.Deg2Rad), 0f);
            Vector3 nx = new Vector3(Mathf.Cos(lean * Mathf.Deg2Rad), -Mathf.Sin(lean * Mathf.Deg2Rad), 0f);
            Vector3 face = promo - nx * 0.05f;
            for (int i = 0; i < 10; i++)
            {
                float a = i * 36f * Mathf.Deg2Rad;
                GB.Sphere(g, $"PromoNose{i}", face + up * (0.1f + 0.55f * Mathf.Cos(a)) + Vector3.forward * (0.55f * Mathf.Sin(a)), 0.07f, mPink, false);
            }
            GB.Sphere(g, "PromoEye_L", face + up * 1f + Vector3.forward * 0.38f, 0.13f, mGoggle, false);
            GB.Sphere(g, "PromoEye_R", face + up * 1f - Vector3.forward * 0.38f, 0.13f, mGoggle, false);
            GB.PointLight(g, "PromoGlow", new Vector3(-8.2f, 1.6f, 6.5f), Pink, 4f, 0.7f);
        }

        /// SW corner: road cases, a prop trunk and the scenic paint table (no-trap ground for anyone who climbs on it).
        static void BuildCrateCorner(Transform parent)
        {
            var g = GB.Group(parent, "CrateCorner");
            GB.Box(g, "RoadCase1", new Vector3(-6.8f, 0.45f, -10.4f), new Vector3(1.2f, 0.9f, 0.8f), GB.MatteBlack);
            GB.Box(g, "RoadCase2", new Vector3(-6.7f, 1.25f, -10.4f), new Vector3(1f, 0.7f, 0.7f), GB.MatteBlack);
            GB.Box(g, "RoadCase_Stencil", new Vector3(-6.8f, 0.6f, -9.995f), new Vector3(0.6f, 0.12f, 0.01f), mChalk, collider: false);
            GB.Box(g, "PropTrunk", new Vector3(-5.2f, 0.35f, -10.7f), new Vector3(1.1f, 0.7f, 0.6f), mCrimson);

            GB.Box(g, "PaintTable_Top", new Vector3(-8.2f, 0.86f, -7.6f), new Vector3(2.4f, 0.08f, 1f), GB.Plywood); // x -9.4..-7.0, z -8.1..-7.1
            float[] lx = { -9.3f, -7.1f };
            float[] lz = { -7.18f, -8.02f };
            int n = 0;
            foreach (float x in lx)
                foreach (float z in lz)
                    GB.Box(g, $"PaintTable_Leg{n++}", new Vector3(x, 0.41f, z), new Vector3(0.08f, 0.82f, 0.08f), GB.Wood);
            GB.Cylinder(g, "PaintCan1", new Vector3(-8.9f, 0.97f, -7.4f), 0.09f, 0.14f, GB.Metal, false);
            GB.Cylinder(g, "PaintCan2", new Vector3(-8.6f, 0.97f, -7.85f), 0.09f, 0.14f, GB.Metal, false);
            GB.Cylinder(g, "EvergleamCan", new Vector3(-8.25f, 0.97f, -7.6f), 0.09f, 0.14f, GB.Metal, false);
            GB.Cylinder(g, "EvergleamCan_Lid", new Vector3(-8.25f, 1.045f, -7.6f), 0.085f, 0.01f, mPink, false);
            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(-7.7f, 0.9f, -7.3f), 90f, "On the scenic paint table (0.9 m); Traps 1 and 7 in walk range of the floor around it");
        }

        /// Concrete piers, ceiling beams, hanging chains and the two recharge lamps.
        static void BuildStructure(Transform parent)
        {
            var g = GB.Group(parent, "Structure");
            GB.Box(g, "Pier_NW", new Vector3(-6.5f, H * 0.5f, 6.5f), new Vector3(0.8f, H, 0.8f), GB.Concrete);
            GB.Box(g, "Pier_SE", new Vector3(6.5f, H * 0.5f, -6.5f), new Vector3(0.8f, H, 0.8f), GB.Concrete);
            GB.Box(g, "Beam_N", new Vector3(0f, 5f, 6.5f), new Vector3(26f, 0.4f, 0.3f), GB.DarkConcrete, collider: false);
            GB.Box(g, "Beam_S", new Vector3(0f, 5f, -6.5f), new Vector3(26f, 0.4f, 0.3f), GB.DarkConcrete, collider: false);
            GB.Box(g, "Beam_E", new Vector3(6.5f, 5f, 0f), new Vector3(0.3f, 0.4f, 26f), GB.DarkConcrete, collider: false);
            GB.Box(g, "Beam_W", new Vector3(-6.5f, 5f, 0f), new Vector3(0.3f, 0.4f, 26f), GB.DarkConcrete, collider: false);

            HangingChain(g, "Chain_Trap1", -4.6f, -7.2f, 2.8f, true);
            HangingChain(g, "Chain_Trap2", 5.2f, -10.4f, 2.6f, false);
            HangingChain(g, "Chain_Trap4", 9.9f, -4.4f, 2.75f, true);
            HangingChain(g, "Chain_Trap5", 8.4f, 7f, 2.7f, false);
            HangingChain(g, "Chain_Panel6", 2.8f, 7.6f, 2.9f, true);
            HangingChain(g, "Chain_Trap7", -8.4f, -10.4f, 2.6f, false);

            // real recharge lamps sit on a bracket 2.2 m above the floor they serve
            GB.Box(g, "LampPlate_Pier", new Vector3(6.5f, 2.2f, -6.915f), new Vector3(0.18f, 0.25f, 0.03f), GB.Metal, collider: false);
            GB.Box(g, "LampBracket_Pier", new Vector3(6.5f, 2.2f, -7.06f), new Vector3(0.05f, 0.05f, 0.3f), GB.Metal, collider: false);
            GB.GlowLamp(g, "RechargeLamp_Pier", new Vector3(6.5f, 2.2f, -7.3f), LampAmber, 0.11f, 4f, 1.2f);
            GB.Marker(g, MarkerKind.RechargeLamp, new Vector3(6.5f, 0f, -7.4f), 180f, "Recharge lamp on the SE pier (2.2 m bracket), inside Trap 4's walk range");
            GB.Box(g, "LampPlate_Catwalk", new Vector3(7.5f, CwY + 2.2f, 12.985f), new Vector3(0.18f, 0.25f, 0.03f), GB.Metal, collider: false);
            GB.Box(g, "LampBracket_Catwalk", new Vector3(7.5f, CwY + 2.2f, 12.84f), new Vector3(0.05f, 0.05f, 0.3f), GB.Metal, collider: false);
            GB.GlowLamp(g, "RechargeLamp_Catwalk", new Vector3(7.5f, CwY + 2.2f, 12.6f), LampAmber, 0.11f, 4f, 1.2f);
            GB.Marker(g, MarkerKind.RechargeLamp, new Vector3(7.5f, CwY, 12.4f), 180f, "Recharge lamp on the north catwalk wall: no-trap ground");
        }

        /// Pink spike-tape X's: the marks Gloamies stop on for a Curtain Call bow.
        static void BuildMarks(Transform parent)
        {
            var g = GB.Group(parent, "SpikeMarks");
            SpikeX(g, "Mark_BurrowW", new Vector3(-1.3f, 0f, -4.5f));
            SpikeX(g, "Mark_BurrowE", new Vector3(1.3f, 0f, -4.5f));
            SpikeX(g, "Mark_Lift1", new Vector3(7.6f, 1.2f, 0.9f));
            SpikeX(g, "Mark_Lift2", new Vector3(9.4f, 1.2f, -1.1f));
            SpikeX(g, "Mark_WestStairTop", new Vector3(-12.15f, CwY, -6.4f));
            SpikeX(g, "Mark_Balcony", new Vector3(2.4f, CwY, 10.7f));
            SpikeX(g, "Mark_EastArm", new Vector3(5.2f, 0f, 2.9f));
            SpikeX(g, "Mark_FlatStore", new Vector3(-7.7f, 0f, 4.6f));
            SpikeX(g, "Mark_SE", new Vector3(4.4f, 0f, -5f));
            SpikeX(g, "Mark_SW", new Vector3(-6.4f, 0f, -6.2f));
            SpikeX(g, "Mark_North", new Vector3(0.6f, 0f, 4.6f));
        }

        static void BuildMarkers(Transform parent)
        {
            var g = GB.Group(parent, "Markers");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(3.9f, 0f, -5.9f), 180f, "Burrow loop, east side");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-5f, 0f, -5.4f), 90f, "Burrow loop, west side");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-6.6f, 0f, 2.6f), 0f, "West arm, by Trap 3");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-7.45f, 0f, 8.5f), 90f, "Flat store: quiet floor (Tapwell's ground)");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-0.8f, 0f, 5.4f), 90f, "North arm, below the prompt balcony");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(5.6f, 0f, 4.4f), 0f, "Winch gallery approach");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(8.2f, 0f, -9.6f), 270f, "Pump corner");
            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(-6f, 0f, 12f), 180f, "Reel behind flat cart 3, under the north catwalk: quiet floor, Tapwell's ground");
            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(9.4f, 0f, -8.6f), 270f, "Reel by the fallen sandbags: Traps 4 and 2 in walk range");
            GB.Box(g, "BoomMic", new Vector3(-0.8f, 0.02f, -4.8f), new Vector3(0.03f, 0.03f, 0.36f), GB.MatteBlack, 30f, false);
            GB.Sphere(g, "BoomMic_Foam", new Vector3(-0.71f, 0.04f, -4.644f), 0.04f, GB.MatteBlack, false);
            GB.Marker(g, MarkerKind.StoryProp, new Vector3(-0.8f, 0f, -4.8f), 30f, "Digsley's snapped-off headset boom mic at the Burrow door");
        }
    }
}
