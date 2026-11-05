using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Pinny's Midnight Kitchen (Act III "Snack-Time"). An oversized cartoon kitchen set built inside an old exhibit hall.
    /// South strip (z 0..4): black studio floor with the stage-manager desk, apron rack, crew lockers and a 16 mm camera.
    /// The set (z 4..20): checkerboard floor; central island with a child-sized crawl tunnel; free-standing long counter
    /// plus wall counter (galley loop); huge dining table (raised ground, Little Helper step); giant stove, Pinny's
    /// labelled shelf and giant fridge on the back wall; walk-in pantry (two doors + crawl gap, breaker inside);
    /// wet scullery corner. Mop spills and the sprinkler test valve break Pinny's scent trail.
    /// All coordinates are local to root: x across the wing, +z away from the theatre, floor at y = 0.
    public static class KitchenWing
    {
        static Material mFloorInk, mCheckerCream, mWallpaper, mGingham, mTableCloth, mRedPaint, mCream, mCounter;
        static Material mFridge, mFridgeInner, mPantryFlat, mStep, mLocker, mSteel, mBucket, mPipeRed, mPaper;
        static Material mSack, mJar, mCrust, mTrunk, mBreaker, mCamera, mNight, mApron, mCalf, mCalfStripe;
        static Material mGlowTape, mPilot, mFridgeGlow, mLabelGlow, mOvenGlow, mMoon, mUV, mCueRed, mCueUV;
        static Material[] mFelt;

        static readonly Color PinnyGlow = new Color(0.72f, 0.86f, 1f);   // her floating blue-white saddle
        static readonly Color LampAmber = new Color(1f, 0.68f, 0.32f);   // recharge lamps

        public static void Build(Transform root)
        {
            Materials();
            GB.Room(root, "Room", new Vector3(0f, 0f, 10f), new Vector3(22f, 5f, 20f), mFloorInk, GB.MatteBlack, GB.Velvet, new Doorway(Side.South));

            SetDressing(GB.Group(root, "SetShell"));
            StudioFront(GB.Group(root, "StudioFront"));
            DiningArea(GB.Group(root, "DiningArea"));
            Pantry(GB.Group(root, "Pantry"));
            PinnysShelf(GB.Group(root, "PinnysShelf"));
            Stove(GB.Group(root, "Stove"));
            Fridge(GB.Group(root, "Fridge"));
            Island(GB.Group(root, "Island"));
            Counters(GB.Group(root, "Counters"));
            Scullery(GB.Group(root, "Scullery"));
            WaterAndTraps(GB.Group(root, "WaterAndTraps"));
            Calves(GB.Group(root, "PlushCalves"));
            Markers(GB.Group(root, "Markers"));
        }

        static void Materials()
        {
            mFloorInk = GB.Mat("Kitchen_FloorInk", new Color(0.045f, 0.045f, 0.05f), 0.35f);
            mCheckerCream = GB.Mat("Kitchen_CheckerCream", new Color(0.56f, 0.53f, 0.45f), 0.3f);
            mWallpaper = GB.Mat("Kitchen_Wallpaper", new Color(0.5f, 0.45f, 0.31f), 0.1f);
            mGingham = GB.Mat("Kitchen_Gingham", new Color(0.5f, 0.13f, 0.1f), 0.1f);
            mTableCloth = GB.Mat("Kitchen_TableCloth", new Color(0.58f, 0.36f, 0.32f), 0.05f);
            mRedPaint = GB.Mat("Kitchen_RedPaint", new Color(0.4f, 0.11f, 0.08f), 0.25f);
            mCream = GB.Mat("Kitchen_CreamEnamel", new Color(0.6f, 0.57f, 0.49f), 0.45f);
            mCounter = GB.Mat("Kitchen_Countertop", new Color(0.55f, 0.52f, 0.44f), 0.3f);
            mFridge = GB.Mat("Kitchen_FridgeMint", new Color(0.4f, 0.52f, 0.47f), 0.4f);
            mFridgeInner = GB.Mat("Kitchen_FridgeInner", new Color(0.65f, 0.68f, 0.68f), 0.3f);
            mPantryFlat = GB.Mat("Kitchen_PantryFlat", new Color(0.44f, 0.4f, 0.27f), 0.1f);
            mStep = GB.Mat("Kitchen_HelperStep", new Color(0.55f, 0.42f, 0.12f), 0.2f);
            mLocker = GB.Mat("Kitchen_Locker", new Color(0.24f, 0.28f, 0.27f), 0.4f, 0.3f);
            mSteel = GB.Mat("Kitchen_SinkSteel", new Color(0.22f, 0.23f, 0.24f), 0.6f, 0.7f);
            mBucket = GB.Mat("Kitchen_BucketYellow", new Color(0.62f, 0.47f, 0.07f), 0.4f);
            mPipeRed = GB.Mat("Kitchen_PipeRed", new Color(0.55f, 0.07f, 0.05f), 0.35f);
            mPaper = GB.Mat("Kitchen_Paper", new Color(0.72f, 0.7f, 0.64f), 0.1f);
            mSack = GB.Mat("Kitchen_Burlap", new Color(0.42f, 0.35f, 0.24f), 0.05f);
            mJar = GB.Mat("Kitchen_JarAmber", new Color(0.42f, 0.26f, 0.08f), 0.7f);
            mCrust = GB.Mat("Kitchen_Crust", new Color(0.55f, 0.36f, 0.16f), 0.2f);
            mTrunk = GB.Mat("Kitchen_Trunk", new Color(0.14f, 0.11f, 0.09f), 0.2f);
            mBreaker = GB.Mat("Kitchen_BreakerGrey", new Color(0.3f, 0.31f, 0.32f), 0.4f, 0.5f);
            mCamera = GB.Mat("Kitchen_CameraBody", new Color(0.1f, 0.1f, 0.11f), 0.35f);
            mNight = GB.Mat("Kitchen_NightPane", new Color(0.02f, 0.03f, 0.07f), 0.6f);
            mApron = GB.Mat("Kitchen_Apron", new Color(0.62f, 0.55f, 0.38f), 0.05f);
            mCalf = GB.Mat("Kitchen_CalfPlush", new Color(0.3f, 0.16f, 0.09f), 0.05f);
            mCalfStripe = GB.Mat("Kitchen_CalfStripe", new Color(0.68f, 0.63f, 0.52f), 0.05f);
            mFelt = new[]
            {
                GB.Mat("Kitchen_FeltRed", new Color(0.55f, 0.12f, 0.1f), 0.05f),
                GB.Mat("Kitchen_FeltBlue", new Color(0.18f, 0.3f, 0.55f), 0.05f),
                GB.Mat("Kitchen_FeltYellow", new Color(0.62f, 0.5f, 0.12f), 0.05f),
                GB.Mat("Kitchen_FeltGreen", new Color(0.2f, 0.42f, 0.22f), 0.05f),
                GB.Mat("Kitchen_FeltPink", new Color(0.62f, 0.34f, 0.42f), 0.05f),
            };

            // Evergleam / black-light glows, mostly in Pinny's blue-white
            mGlowTape = GB.Glow("Kitchen_GlowTape", PinnyGlow, 1.6f);
            mPilot = GB.Glow("Kitchen_PilotFlame", new Color(0.55f, 0.75f, 1f), 3f);
            mFridgeGlow = GB.Glow("Kitchen_FridgeLight", new Color(0.8f, 0.9f, 1f), 1.4f);
            mLabelGlow = GB.Glow("Kitchen_LabelGlow", new Color(0.85f, 0.92f, 1f), 0.9f);
            mOvenGlow = GB.Glow("Kitchen_OvenGlow", new Color(0.55f, 0.72f, 1f), 0.8f);
            mMoon = GB.Glow("Kitchen_MoonGlow", new Color(0.82f, 0.9f, 1f), 1.2f);
            mUV = GB.Glow("Kitchen_UVHandprint", new Color(0.6f, 0.45f, 1f), 0.9f);
            mCueRed = GB.Glow("Kitchen_CueRed", new Color(1f, 0.12f, 0.08f), 2f);
            mCueUV = GB.Glow("Kitchen_CueUV", new Color(0.55f, 0.3f, 1f), 2.5f);
        }

        // ---------------------------------------------------------------- small helpers

        /// Crossed spike-tape mark lying on a surface (surface.y = the top of that surface).
        static void SpikeX(Transform p, string name, Vector3 surface, float length)
        {
            GB.Box(p, name + "_A", surface + new Vector3(0f, 0.004f, 0f), new Vector3(0.06f, 0.008f, length), mGlowTape, 45f, false);
            GB.Box(p, name + "_B", surface + new Vector3(0f, 0.005f, 0f), new Vector3(0.06f, 0.008f, length), mGlowTape, -45f, false);
        }

        /// One of the mailed-in lost things, standing on a shelf whose top is at surface.y.
        static void LostThing(Transform p, string name, Vector3 surface, int kind, int colour)
        {
            Material m = mFelt[colour % mFelt.Length];
            switch (kind % 5)
            {
                case 0: GB.Box(p, name + "_Mitten", surface + new Vector3(0f, 0.12f, 0f), new Vector3(0.16f, 0.24f, 0.07f), m, 8f, false); break;
                case 1: GB.Cylinder(p, name + "_Case", surface + new Vector3(0f, 0.06f, 0f), 0.07f, 0.12f, m, false); break;
                case 2: GB.Sphere(p, name + "_Ball", surface + new Vector3(0f, 0.09f, 0f), 0.09f, m, false); break;
                case 3: GB.Box(p, name + "_Lunchbox", surface + new Vector3(0f, 0.08f, 0f), new Vector3(0.24f, 0.16f, 0.1f), m, 0f, false); break;
                default: GB.Box(p, name + "_Shoe", surface + new Vector3(0f, 0.05f, 0f), new Vector3(0.12f, 0.1f, 0.26f), m, 0f, false); break;
            }
        }

        /// Small glowing handwritten label card facing -Z.
        static void TagCard(Transform p, string name, Vector3 c) =>
            GB.Box(p, name, c, new Vector3(0.14f, 0.05f, 0.01f), mLabelGlow, 0f, false);

        /// Oversized cartoon chair, seat top at 0.6 m, facing local +Z (backrest at local -Z).
        static void Chair(Transform p, string name, Vector3 pos, float yaw)
        {
            var c = GB.Group(p, name, pos, yaw);
            GB.Box(c, "Seat", new Vector3(0f, 0.55f, 0f), new Vector3(1.2f, 0.1f, 1.2f), mRedPaint);
            GB.Box(c, "Back", new Vector3(0f, 1.3f, -0.55f), new Vector3(1.2f, 1.4f, 0.1f), mRedPaint);
            GB.Box(c, "BackRail", new Vector3(0f, 2.03f, -0.55f), new Vector3(1.3f, 0.08f, 0.14f), mCream, 0f, false);
            for (int i = 0; i < 4; i++)
                GB.Box(c, $"Leg_{i}", new Vector3(i % 2 == 0 ? -0.5f : 0.5f, 0.25f, i < 2 ? -0.5f : 0.5f), new Vector3(0.12f, 0.5f, 0.12f), mRedPaint);
        }

        /// Yellow mop bucket with a wringer and a leaning mop.
        static void MopBucket(Transform p, string name, Vector3 pos, float yaw)
        {
            var b = GB.Group(p, name, pos, yaw);
            GB.Cylinder(b, "Bucket", new Vector3(0f, 0.25f, 0f), 0.3f, 0.5f, mBucket);
            GB.Box(b, "Wringer", new Vector3(0f, 0.56f, 0f), new Vector3(0.5f, 0.12f, 0.26f), mBucket, 0f, false);
            GB.Cylinder(b, "MopHandle", new Vector3(0.08f, 1.05f, 0f), 0.025f, 1.6f, GB.Wood, new Vector3(0f, 0f, -8f), false);
        }

        /// Striped plush tapir calf lying on the floor (kicking one squeaks and pulls Pinny over).
        static void Calf(Transform p, string name, Vector3 pos, float yaw)
        {
            var g = GB.Group(p, name, pos, yaw);
            GB.Ellipsoid(g, "Body", new Vector3(0f, 0.2f, 0f), new Vector3(0.42f, 0.36f, 0.75f), mCalf, Vector3.zero, false);
            GB.Ellipsoid(g, "Head", new Vector3(0f, 0.22f, 0.46f), new Vector3(0.26f, 0.24f, 0.32f), mCalf, Vector3.zero, false);
            GB.Ellipsoid(g, "StripeL", new Vector3(0.17f, 0.25f, 0f), new Vector3(0.07f, 0.06f, 0.6f), mCalfStripe, Vector3.zero, false);
            GB.Ellipsoid(g, "StripeR", new Vector3(-0.17f, 0.25f, 0f), new Vector3(0.07f, 0.06f, 0.6f), mCalfStripe, Vector3.zero, false);
        }

        // ---------------------------------------------------------------- set shell

        /// Checkerboard set floor, painted wallpaper flats (open above, like a TV set), a painted night window,
        /// a wall rack of mailed-in lost things, the velvet teaser and the dead lighting grid.
        static void SetDressing(Transform g)
        {
            // 2 m cream squares laid on the ink-black floor over the whole set (z 4..19.8), except inside the pantry
            var floor = GB.Group(g, "Checkerboard");
            for (int i = 0; i < 11; i++)
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 != 0) continue;
                    float x0 = -11f + 2f * i, x1 = x0 + 2f;
                    float z0 = 4f + 2f * j, z1 = Mathf.Min(z0 + 2f, 19.8f);
                    bool pantryColumn = x1 < -4.9f;
                    if (pantryColumn && z0 >= 13.5f) continue;
                    if (pantryColumn) z1 = Mathf.Min(z1, 13.35f);
                    GB.Box(floor, $"Cream_{i:00}_{j}", new Vector3((x0 + x1) * 0.5f, 0.005f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 0.01f, z1 - z0), mCheckerCream, 0f, false);
                }
            GB.Box(g, "SetEdgeTape", new Vector3(0f, 0.014f, 4f), new Vector3(22f, 0.008f, 0.06f), mGlowTape, 0f, false);

            // wallpaper flats 3.4 m high standing just in front of the black hall walls
            GB.Box(g, "Flat_Back", new Vector3(3f, 1.7f, 19.85f), new Vector3(15.7f, 3.4f, 0.1f), mWallpaper);
            GB.Box(g, "Flat_East", new Vector3(10.9f, 1.7f, 11.9f), new Vector3(0.1f, 3.4f, 15.8f), mWallpaper);
            GB.Box(g, "Flat_West", new Vector3(-10.9f, 1.7f, 8.675f), new Vector3(0.1f, 3.4f, 9.35f), mWallpaper);
            GB.Box(g, "GinghamBorder_Back", new Vector3(3f, 3.075f, 19.79f), new Vector3(15.7f, 0.25f, 0.02f), mGingham, 0f, false);
            GB.Box(g, "GinghamBorder_East", new Vector3(10.84f, 3.075f, 11.9f), new Vector3(0.02f, 0.25f, 15.8f), mGingham, 0f, false);
            GB.Box(g, "GinghamBorder_West", new Vector3(-10.84f, 3.075f, 8.675f), new Vector3(0.02f, 0.25f, 9.35f), mGingham, 0f, false);
            GB.Box(g, "GlowEdge_Back", new Vector3(3f, 3.415f, 19.85f), new Vector3(15.7f, 0.03f, 0.12f), mGlowTape, 0f, false);
            GB.Box(g, "GlowEdge_East", new Vector3(10.9f, 3.415f, 11.9f), new Vector3(0.12f, 0.03f, 15.8f), mGlowTape, 0f, false);
            GB.Box(g, "GlowEdge_West", new Vector3(-10.9f, 3.415f, 8.675f), new Vector3(0.12f, 0.03f, 9.35f), mGlowTape, 0f, false);

            // cartoon night window painted on the west flat, with a black-light crescent moon
            var win = GB.Group(g, "PaintedWindow");
            GB.Box(win, "Pane", new Vector3(-10.835f, 2.15f, 6.6f), new Vector3(0.02f, 1.3f, 2f), mNight, 0f, false);
            GB.Box(win, "FrameTop", new Vector3(-10.82f, 2.83f, 6.6f), new Vector3(0.05f, 0.08f, 2.16f), mCream, 0f, false);
            GB.Box(win, "Sill", new Vector3(-10.79f, 1.47f, 6.6f), new Vector3(0.12f, 0.07f, 2.3f), mCream, 0f, false);
            GB.Box(win, "FrameS", new Vector3(-10.82f, 2.15f, 5.56f), new Vector3(0.05f, 1.42f, 0.08f), mCream, 0f, false);
            GB.Box(win, "FrameN", new Vector3(-10.82f, 2.15f, 7.64f), new Vector3(0.05f, 1.42f, 0.08f), mCream, 0f, false);
            GB.Box(win, "MullionV", new Vector3(-10.82f, 2.15f, 6.6f), new Vector3(0.04f, 1.3f, 0.05f), mCream, 0f, false);
            GB.Box(win, "MullionH", new Vector3(-10.815f, 2.15f, 6.6f), new Vector3(0.04f, 0.05f, 2f), mCream, 0f, false);
            GB.Cylinder(win, "Moon", new Vector3(-10.82f, 2.5f, 7.05f), 0.22f, 0.02f, mMoon, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(win, "MoonShadow", new Vector3(-10.8f, 2.55f, 7.15f), 0.2f, 0.02f, mNight, new Vector3(0f, 0f, 90f), false);

            // wall rack lined with mailed-in lost things, each row labelled (decor, above head-bump height)
            var rack = GB.Group(g, "LostThingsWallRack");
            for (int b = 0; b < 2; b++)
            {
                float y = b == 0 ? 1.6f : 2.3f;
                GB.Box(rack, $"Board_{b}", new Vector3(-10.7f, y - 0.02f, 10.4f), new Vector3(0.3f, 0.04f, 3.6f), GB.Wood, 0f, false);
                GB.Box(rack, $"LabelRow_{b}", new Vector3(-10.548f, y - 0.02f, 10.4f), new Vector3(0.01f, 0.035f, 3.4f), mLabelGlow, 0f, false);
                for (int k = 0; k < 5; k++)
                    LostThing(rack, $"Thing_{b}_{k}", new Vector3(-10.68f, y, 8.9f + 0.75f * k), k + 2 * b, k + b);
            }
            GB.Box(rack, "BracketS", new Vector3(-10.7f, 1.95f, 8.62f), new Vector3(0.3f, 0.74f, 0.04f), GB.Wood, 0f, false);
            GB.Box(rack, "BracketN", new Vector3(-10.7f, 1.95f, 12.18f), new Vector3(0.3f, 0.74f, 0.04f), GB.Wood, 0f, false);

            // black velvet teaser framing the set, and the dead studio lighting grid (all above reach, no colliders)
            GB.Box(g, "Teaser_Velvet", new Vector3(0f, 4.7f, 4f), new Vector3(22f, 0.6f, 0.08f), GB.Velvet, 0f, false);
            float[] pipeZ = { 7f, 12.5f, 17f };
            for (int i = 0; i < pipeZ.Length; i++)
                GB.Cylinder(g, $"GridPipe_{i}", new Vector3(0f, 4.65f, pipeZ[i]), 0.035f, 21.6f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            Vector3[] cans = { new Vector3(-6f, 4.38f, 7f), new Vector3(3.5f, 4.38f, 7f), new Vector3(-2.5f, 4.38f, 12.5f), new Vector3(7.5f, 4.38f, 17f) };
            for (int i = 0; i < cans.Length; i++)
                GB.Cylinder(g, $"DeadStudioLamp_{i}", cans[i], 0.18f, 0.4f, mCamera, new Vector3(30f, 0f, 0f), false);
        }

        // ---------------------------------------------------------------- studio floor (z 0..4)

        static void StudioFront(Transform g)
        {
            // stage-manager's prompt desk facing the set; black-light cue lamp for Curtain Call; knee space = hiding spot
            var desk = GB.Group(g, "StageManagerDesk");
            GB.Box(desk, "Top", new Vector3(-7.4f, 1.07f, 2.25f), new Vector3(2.4f, 0.06f, 0.9f), GB.MatteBlack);
            GB.Box(desk, "SideW", new Vector3(-8.55f, 0.52f, 2.25f), new Vector3(0.1f, 1.04f, 0.9f), GB.MatteBlack);
            GB.Box(desk, "SideE", new Vector3(-6.25f, 0.52f, 2.25f), new Vector3(0.1f, 1.04f, 0.9f), GB.MatteBlack);
            GB.Box(desk, "FrontPanel", new Vector3(-7.4f, 0.62f, 2.67f), new Vector3(2.2f, 0.84f, 0.06f), GB.MatteBlack);
            GB.Box(desk, "CueLampBase", new Vector3(-8.1f, 1.14f, 2.5f), new Vector3(0.5f, 0.08f, 0.2f), GB.Metal, 0f, false);
            GB.Cylinder(desk, "BlackLightTube", new Vector3(-8.1f, 1.24f, 2.5f), 0.03f, 0.45f, mCueUV, new Vector3(0f, 0f, 90f), false);
            GB.PointLight(desk, "CueLampLight", new Vector3(-8.1f, 1.4f, 2.35f), new Color(0.55f, 0.3f, 1f), 3f, 0.8f);
            GB.Cylinder(desk, "GoButton", new Vector3(-6.75f, 1.12f, 2.45f), 0.06f, 0.04f, mCueRed, false);
            GB.Box(desk, "RunningOrderClipboard", new Vector3(-7.5f, 1.108f, 2.15f), new Vector3(0.32f, 0.015f, 0.42f), mPaper, 10f, false);
            GB.Box(desk, "Headset", new Vector3(-7f, 1.14f, 2.1f), new Vector3(0.2f, 0.08f, 0.18f), mCamera, -20f, false);

            // costume rack of spare Snack-Time aprons against the west wall (hide between them)
            var rack = GB.Group(g, "ApronRack");
            GB.Box(rack, "FootS", new Vector3(-10.35f, 0.04f, 0.6f), new Vector3(0.7f, 0.08f, 0.08f), GB.Metal);
            GB.Box(rack, "FootN", new Vector3(-10.35f, 0.04f, 2.8f), new Vector3(0.7f, 0.08f, 0.08f), GB.Metal);
            GB.Box(rack, "PostS", new Vector3(-10.35f, 1f, 0.6f), new Vector3(0.06f, 1.9f, 0.06f), GB.Metal);
            GB.Box(rack, "PostN", new Vector3(-10.35f, 1f, 2.8f), new Vector3(0.06f, 1.9f, 0.06f), GB.Metal);
            GB.Cylinder(rack, "Rail", new Vector3(-10.35f, 1.95f, 1.7f), 0.025f, 2.3f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            for (int i = 0; i < 5; i++)
                GB.Box(rack, $"Apron_{i}", new Vector3(-10.35f, 1.2f, 0.85f + 0.42f * i), new Vector3(0.55f, 1.4f, 0.04f), i % 2 == 0 ? mApron : mGingham, 0f, false);

            // crew lockers against the east wall, with a bench
            var lockers = GB.Group(g, "CrewLockers");
            for (int i = 0; i < 3; i++)
            {
                float z = 1.2f + 0.8f * i;
                GB.Box(lockers, $"Locker_{i}", new Vector3(10.7f, 1.05f, z), new Vector3(0.6f, 2.1f, 0.78f), mLocker);
                GB.Box(lockers, $"Vent_{i}", new Vector3(10.395f, 1.78f, z), new Vector3(0.01f, 0.22f, 0.4f), GB.MatteBlack, 0f, false);
            }
            GB.Box(lockers, "NameCard_PINNY", new Vector3(10.395f, 1.5f, 2f), new Vector3(0.01f, 0.08f, 0.26f), mPaper, 0f, false);
            GB.Box(lockers, "BenchTop", new Vector3(9.7f, 0.43f, 2f), new Vector3(0.4f, 0.06f, 2f), GB.Wood);
            GB.Box(lockers, "BenchLegS", new Vector3(9.7f, 0.2f, 1.2f), new Vector3(0.3f, 0.4f, 0.06f), GB.Wood);
            GB.Box(lockers, "BenchLegN", new Vector3(9.7f, 0.2f, 2.8f), new Vector3(0.3f, 0.4f, 0.06f), GB.Wood);

            // amber recharge lamp on the south wall, east of the entrance, bracket at 2.2 m
            var lamp = GB.Group(g, "RechargeLamp_South");
            GB.Box(lamp, "Backplate", new Vector3(5.5f, 2.2f, 0.02f), new Vector3(0.25f, 0.35f, 0.04f), GB.Metal, 0f, false);
            GB.Box(lamp, "Bracket", new Vector3(5.5f, 2.2f, 0.15f), new Vector3(0.08f, 0.08f, 0.22f), GB.Metal, 0f, false);
            GB.GlowLamp(lamp, "Lamp", new Vector3(5.5f, 2.2f, 0.32f), LampAmber, 0.12f, 4f, 1.2f);

            // 16 mm film camera on a pedestal, aimed at the island (the show was shot on film)
            var cam = GB.Group(g, "FilmCamera16mm", new Vector3(4.4f, 0f, 3f), -20f);
            GB.Cylinder(cam, "PedestalBase", new Vector3(0f, 0.12f, 0f), 0.45f, 0.24f, mCamera);
            GB.Cylinder(cam, "Column", new Vector3(0f, 0.75f, 0f), 0.1f, 1.1f, GB.Metal);
            GB.Box(cam, "Body", new Vector3(0f, 1.45f, 0.05f), new Vector3(0.32f, 0.36f, 0.6f), mCamera);
            GB.Cylinder(cam, "Lens", new Vector3(0f, 1.45f, 0.47f), 0.09f, 0.25f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(cam, "MagazineFront", new Vector3(0f, 1.78f, 0.14f), 0.14f, 0.08f, mCamera, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(cam, "MagazineBack", new Vector3(0f, 1.78f, -0.16f), 0.14f, 0.08f, mCamera, new Vector3(0f, 0f, 90f), false);
            GB.Box(cam, "PanBar", new Vector3(0.1f, 1.35f, -0.5f), new Vector3(0.04f, 0.04f, 0.5f), GB.Metal, 0f, false);
            GB.Sphere(cam, "TallyLight", new Vector3(0f, 1.66f, 0.32f), 0.025f, mCueRed, false);
        }

        // ---------------------------------------------------------------- dining area (west)

        /// Huge dining table: its 1.2 m top is raised safe ground (no hatch within 8 m), the 1.0 m space underneath is a
        /// child-sized crawl. Climb on by the Little Helper step (4 x 0.3 m treads) or a chair; the pendant makes it a lit refuge.
        static void DiningArea(Transform g)
        {
            var table = GB.Group(g, "DiningTable");
            GB.Box(table, "Top", new Vector3(-7.3f, 1.1f, 8.6f), new Vector3(2.6f, 0.2f, 5.2f), GB.Wood);
            GB.Box(table, "Cloth", new Vector3(-7.3f, 1.205f, 8.6f), new Vector3(2.64f, 0.01f, 5.24f), mTableCloth, 0f, false);
            GB.Box(table, "ClothDropW", new Vector3(-8.63f, 1.105f, 8.6f), new Vector3(0.02f, 0.21f, 5.24f), mTableCloth, 0f, false);
            GB.Box(table, "ClothDropE", new Vector3(-5.97f, 1.105f, 8.6f), new Vector3(0.02f, 0.21f, 5.24f), mTableCloth, 0f, false);
            GB.Box(table, "ClothDropN", new Vector3(-7.3f, 1.105f, 11.23f), new Vector3(2.64f, 0.21f, 0.02f), mTableCloth, 0f, false);
            for (int i = 0; i < 6; i++)
                GB.Box(table, $"Leg_{i}", new Vector3(i % 2 == 0 ? -8.3f : -6.3f, 0.5f, 6.3f + 2.3f * (i / 2)), new Vector3(0.3f, 1f, 0.3f), GB.Wood);

            // full table width (2.6 m) so the NavMesh strip survives ledge erosion on the two upper treads
            var step = GB.Stairs(table, "LittleHelperStep", new Vector3(-7.3f, 0f, 4.6f), 0f, 4, 0.3f, 0.35f, 2.6f, mStep);
            for (int i = 0; i < 4; i++)
                GB.Box(step, $"GlowNose_{i}", new Vector3(0f, 0.3f * (i + 1) + 0.006f, 0.35f * i + 0.04f), new Vector3(2.6f, 0.012f, 0.06f), mGlowTape, 0f, false);
            // side-on against the table's east edge, seats facing the open floor (a seat under the table top can't be sat on)
            Chair(table, "Chair_East", new Vector3(-5.4f, 0f, 7.2f), 0f);
            Chair(table, "Chair_North", new Vector3(-5.4f, 0f, 10.6f), 0f);

            GB.Cylinder(table, "ReelPlate", new Vector3(-7.3f, 1.215f, 9.3f), 0.5f, 0.03f, mPaper, false);
            GB.Cylinder(table, "Plate", new Vector3(-7.3f, 1.215f, 7f), 0.5f, 0.03f, mPaper, false);
            GB.Box(table, "GiantFork", new Vector3(-6.55f, 1.215f, 7f), new Vector3(0.1f, 0.012f, 0.95f), GB.Metal, 0f, false);
            GB.Box(table, "GiantKnife", new Vector3(-8.05f, 1.215f, 7f), new Vector3(0.1f, 0.012f, 0.95f), GB.Metal, 0f, false);
            GB.Cylinder(table, "GiantCup", new Vector3(-6.6f, 1.385f, 10.2f), 0.18f, 0.35f, mCream, false);
            GB.Box(table, "UVHand_0", new Vector3(-8.75f, 0.016f, 9.45f), new Vector3(0.11f, 0.01f, 0.15f), mUV, 80f, false);
            GB.Box(table, "UVHand_1", new Vector3(-8.5f, 0.016f, 9.8f), new Vector3(0.11f, 0.01f, 0.15f), mUV, 100f, false);

            var pendant = GB.Group(g, "PendantLamp");
            GB.Cylinder(pendant, "Cord", new Vector3(-7.3f, 4.3f, 8.6f), 0.015f, 1.4f, GB.MatteBlack, false);
            GB.Cylinder(pendant, "Shade", new Vector3(-7.3f, 3.42f, 8.6f), 0.55f, 0.35f, mGingham, false);
            GB.GlowLamp(pendant, "Bulb", new Vector3(-7.3f, 3.2f, 8.6f), new Color(1f, 0.8f, 0.55f), 0.14f, 6f, 0.9f);
        }

        // ---------------------------------------------------------------- walk-in pantry (north-west)

        /// Walk-in pantry, x -11..-5, z 13.5..20: 3.4 m flats, open above. South door, east door and a child-sized crawl gap
        /// give three ways out of the breaker corner. Wall shelving and a free-standing centre shelf to circle.
        static void Pantry(Transform g)
        {
            GB.Wall(g, "Wall_S", new Vector3(-11f, 0f, 13.5f), new Vector3(-4.85f, 0f, 13.5f), 3.4f, 0.3f, mPantryFlat,
                new Opening(1f, 1f, 0f, 1f),          // crawl gap x -10.5..-9.5, 1.0 m high
                new Opening(4.4f, 1.6f, 0f, 2.4f));   // south door x -7.4..-5.8
            GB.Wall(g, "Wall_E", new Vector3(-5f, 0f, 13.65f), new Vector3(-5f, 0f, 20f), 3.4f, 0.3f, mPantryFlat,
                new Opening(3.35f, 1.6f, 0f, 2.4f));  // east door z 16.2..17.8
            GB.Box(g, "PlankFloor", new Vector3(-8.075f, 0.005f, 16.825f), new Vector3(5.85f, 0.01f, 6.35f), GB.Wood, 0f, false);
            GB.Box(g, "DoorGlow_S", new Vector3(-6.6f, 2.42f, 13.34f), new Vector3(1.6f, 0.03f, 0.02f), mGlowTape, 0f, false);
            GB.Box(g, "DoorGlow_E", new Vector3(-4.84f, 2.42f, 17f), new Vector3(0.02f, 0.03f, 1.6f), mGlowTape, 0f, false);

            var shelves = GB.Group(g, "WallShelving");
            GB.Box(shelves, "EndS", new Vector3(-10.65f, 1.4f, 14.45f), new Vector3(0.7f, 2.8f, 0.1f), GB.Wood);
            GB.Box(shelves, "EndN", new Vector3(-10.65f, 1.4f, 19.85f), new Vector3(0.7f, 2.8f, 0.1f), GB.Wood);
            GB.Box(shelves, "Plinth", new Vector3(-10.65f, 0.15f, 17.15f), new Vector3(0.7f, 0.3f, 5.3f), GB.Wood);
            float[] boardY = { 0.9f, 1.5f, 2.1f, 2.7f };
            for (int b = 0; b < boardY.Length; b++)
                GB.Box(shelves, $"Board_{b}", new Vector3(-10.65f, boardY[b], 17.15f), new Vector3(0.7f, 0.05f, 5.3f), GB.Wood);
            for (int b = 0; b < 3; b++)
                for (int k = 0; k < 4; k++)
                {
                    var s = new Vector3(-10.6f, boardY[b] + 0.025f, 15f + 1.3f * k + 0.25f * b);
                    if ((k + b) % 2 == 0) GB.Cylinder(shelves, $"Jar_{b}_{k}", s + new Vector3(0f, 0.17f, 0f), 0.13f, 0.34f, mJar, false);
                    else GB.Cylinder(shelves, $"Can_{b}_{k}", s + new Vector3(0f, 0.13f, 0f), 0.11f, 0.26f, GB.Metal, false);
                }
            GB.Ellipsoid(shelves, "FlourSack_0", new Vector3(-10.6f, 0.55f, 15.4f), new Vector3(0.55f, 0.5f, 0.8f), mSack, Vector3.zero, false);
            GB.Ellipsoid(shelves, "FlourSack_1", new Vector3(-10.6f, 0.55f, 17.8f), new Vector3(0.55f, 0.5f, 0.8f), mSack, Vector3.zero, false);

            var centre = GB.Group(g, "CentreShelf");
            GB.Box(centre, "Plinth", new Vector3(-8f, 0.15f, 16.5f), new Vector3(1f, 0.3f, 2.4f), GB.Wood);
            GB.Box(centre, "Spine", new Vector3(-8f, 1.35f, 16.5f), new Vector3(0.1f, 2.1f, 2.4f), GB.Wood);
            GB.Box(centre, "EndS", new Vector3(-8f, 1.2f, 15.35f), new Vector3(1f, 2.4f, 0.1f), GB.Wood);
            GB.Box(centre, "EndN", new Vector3(-8f, 1.2f, 17.65f), new Vector3(1f, 2.4f, 0.1f), GB.Wood);
            float[] centreY = { 0.9f, 1.6f, 2.3f };
            for (int b = 0; b < centreY.Length; b++)
                GB.Box(centre, $"Board_{b}", new Vector3(-8f, centreY[b], 16.5f), new Vector3(1f, 0.05f, 2.2f), GB.Wood);
            for (int b = 0; b < 2; b++)
                for (int side = 0; side < 2; side++)
                    for (int k = 0; k < 2; k++)
                    {
                        var s = new Vector3(side == 0 ? -8.3f : -7.7f, centreY[b] + 0.025f, 15.9f + 1.2f * k);
                        GB.Cylinder(centre, $"Tin_{b}_{side}_{k}", s + new Vector3(0f, 0.15f, 0f), 0.12f, 0.3f, (b + side + k) % 2 == 0 ? mJar : mCrust, false);
                    }

            // the wing's breaker panel, on the hall's back wall inside the pantry
            var breaker = GB.Group(g, "BreakerPanel");
            GB.Box(breaker, "Panel", new Vector3(-9.2f, 1.6f, 19.89f), new Vector3(0.9f, 1.1f, 0.22f), mBreaker);
            GB.Box(breaker, "ThrowLever", new Vector3(-8.82f, 1.55f, 19.74f), new Vector3(0.07f, 0.4f, 0.08f), mPipeRed, 0f, false);
            GB.Sphere(breaker, "DeadIndicator", new Vector3(-9.45f, 2f, 19.77f), 0.035f, mCueRed, false);
            GB.Box(breaker, "Stencil_KitchenWing", new Vector3(-9.2f, 1.22f, 19.77f), new Vector3(0.6f, 0.1f, 0.01f), mPaper, 0f, false);
            GB.Cylinder(breaker, "Conduit", new Vector3(-9.2f, 3.6f, 19.93f), 0.04f, 2.8f, GB.Metal, false);

            var trunk = GB.Group(g, "PropTrunk");
            GB.Box(trunk, "Body", new Vector3(-5.95f, 0.4f, 19.5f), new Vector3(1.4f, 0.8f, 0.8f), mTrunk);
            GB.Box(trunk, "Lid", new Vector3(-5.95f, 0.83f, 19.5f), new Vector3(1.44f, 0.06f, 0.84f), mTrunk);
            GB.Box(trunk, "StrapW", new Vector3(-6.45f, 0.435f, 19.5f), new Vector3(0.06f, 0.87f, 0.86f), GB.Metal, 0f, false);
            GB.Box(trunk, "StrapE", new Vector3(-5.45f, 0.435f, 19.5f), new Vector3(0.06f, 0.87f, 0.86f), GB.Metal, 0f, false);
            GB.Box(trunk, "Stencil_SnackTimeProps", new Vector3(-5.95f, 0.5f, 19.065f), new Vector3(0.7f, 0.16f, 0.01f), mPaper, 0f, false);

            var mail = GB.Group(g, "CrumbJarMailSacks");
            GB.Ellipsoid(mail, "Sack_0", new Vector3(-5.6f, 0.4f, 14.35f), new Vector3(0.6f, 0.8f, 0.6f), mSack);
            GB.Ellipsoid(mail, "Sack_1", new Vector3(-5.6f, 0.36f, 15f), new Vector3(0.55f, 0.72f, 0.55f), mSack);
            for (int i = 0; i < 5; i++)
                GB.Box(mail, $"Letter_{i}", new Vector3(-6.05f - 0.13f * (i % 3), 0.016f, 14.2f + 0.28f * i), new Vector3(0.24f, 0.008f, 0.15f), mPaper, 25f * i - 30f, false);

            GB.Cylinder(g, "BulbCord", new Vector3(-8f, 4f, 18.6f), 0.012f, 2f, GB.MatteBlack, false);
            GB.GlowLamp(g, "PantryBulb", new Vector3(-8f, 2.95f, 18.6f), new Color(1f, 0.86f, 0.6f), 0.1f, 4.5f, 0.7f);
            GB.Box(g, "UVHand_0", new Vector3(-10.2f, 0.025f, 13.05f), new Vector3(0.11f, 0.01f, 0.15f), mUV, 15f, false);
            GB.Box(g, "UVHand_1", new Vector3(-9.8f, 0.025f, 12.8f), new Vector3(0.11f, 0.01f, 0.15f), mUV, -10f, false);
        }

        // ---------------------------------------------------------------- back wall: Pinny's shelf, stove, fridge

        /// Pinny's kitchen shelf (hutch), x -4.5..-2.1 against the back flat: neat rows of mailed-in lost things,
        /// each with a glowing handwritten label. Caught players' items turn up here.
        static void PinnysShelf(Transform g)
        {
            GB.Box(g, "BaseCabinet", new Vector3(-3.3f, 0.44f, 19.45f), new Vector3(2.4f, 0.88f, 0.7f), mRedPaint);
            GB.Box(g, "BaseTop", new Vector3(-3.3f, 0.9f, 19.42f), new Vector3(2.5f, 0.04f, 0.76f), mCounter);
            GB.Box(g, "HutchBack", new Vector3(-3.3f, 1.97f, 19.77f), new Vector3(2.4f, 2.1f, 0.06f), mRedPaint);
            GB.Box(g, "HutchSideW", new Vector3(-4.47f, 1.97f, 19.55f), new Vector3(0.06f, 2.1f, 0.5f), mRedPaint);
            GB.Box(g, "HutchSideE", new Vector3(-2.13f, 1.97f, 19.55f), new Vector3(0.06f, 2.1f, 0.5f), mRedPaint);
            GB.Box(g, "Crown", new Vector3(-3.3f, 3.07f, 19.5f), new Vector3(2.5f, 0.1f, 0.6f), mRedPaint, 0f, false);
            GB.Box(g, "SignBoard", new Vector3(-3.3f, 3.3f, 19.72f), new Vector3(1.6f, 0.34f, 0.04f), mCream, 0f, false);
            GB.Box(g, "SignLettering", new Vector3(-3.3f, 3.3f, 19.695f), new Vector3(1.3f, 0.12f, 0.01f), mGlowTape, 0f, false);
            float[] shelfY = { 1.45f, 2f, 2.55f };
            for (int s = 0; s < shelfY.Length; s++)
            {
                GB.Box(g, $"Shelf_{s}", new Vector3(-3.3f, shelfY[s], 19.55f), new Vector3(2.28f, 0.04f, 0.5f), GB.Wood, 0f, false);
                for (int k = 0; k < 6; k++)
                {
                    float x = -4.15f + 0.34f * k;
                    LostThing(g, $"Thing_{s}_{k}", new Vector3(x, shelfY[s] + 0.02f, 19.55f), k + s, 2 * k + s);
                    TagCard(g, $"Label_{s}_{k}", new Vector3(x, shelfY[s], 19.295f));
                }
            }
            for (int k = 0; k < 3; k++)
            {
                float x = -4.15f + 0.4f * k;
                LostThing(g, $"Thing_Counter_{k}", new Vector3(x, 0.92f, 19.45f), k == 2 ? 3 : k, k + 3);
                TagCard(g, $"Label_Counter_{k}", new Vector3(x, 0.9f, 19.035f));
            }
            GB.PointLight(g, "ShelfGlow", new Vector3(-3.3f, 2.7f, 18.5f), PinnyGlow, 4f, 0.9f);
        }

        /// Giant cartoon stove, x -1.8..1.8, 1.4 m high, with blue pilot flames, a stockpot and a big cream hood.
        static void Stove(Transform g)
        {
            GB.Box(g, "Body", new Vector3(0f, 0.7f, 19f), new Vector3(3.6f, 1.4f, 1.6f), mCream);
            GB.Box(g, "Cooktop", new Vector3(0f, 1.41f, 19f), new Vector3(3.6f, 0.02f, 1.6f), GB.MatteBlack, 0f, false);
            GB.Box(g, "OvenDoor", new Vector3(0f, 0.68f, 18.17f), new Vector3(2.6f, 0.86f, 0.06f), mCream, 0f, false);
            GB.Box(g, "OvenWindow", new Vector3(0f, 0.72f, 18.135f), new Vector3(1.6f, 0.4f, 0.01f), mOvenGlow, 0f, false);
            GB.Cylinder(g, "OvenHandle", new Vector3(0f, 1.18f, 18.08f), 0.035f, 2.4f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            for (int i = 0; i < 4; i++)
                GB.Cylinder(g, $"Knob_{i}", new Vector3(-1.35f + 0.9f * i, 1.3f, 18.16f), 0.07f, 0.08f, GB.MatteBlack, new Vector3(90f, 0f, 0f), false);
            Vector3[] burners = { new Vector3(-0.85f, 1.43f, 18.6f), new Vector3(0.85f, 1.43f, 18.6f), new Vector3(-0.85f, 1.43f, 19.4f), new Vector3(0.85f, 1.43f, 19.4f) };
            for (int i = 0; i < burners.Length; i++)
                GB.Cylinder(g, $"Burner_{i}", burners[i], 0.32f, 0.02f, GB.Metal, false);
            GB.Cylinder(g, "PilotFlame_FrontLeft", new Vector3(-0.85f, 1.45f, 18.6f), 0.2f, 0.02f, mPilot, false);
            GB.Cylinder(g, "PilotFlame_BackRight", new Vector3(0.85f, 1.45f, 19.4f), 0.2f, 0.02f, mPilot, false);
            GB.Cylinder(g, "StockPot", new Vector3(-0.85f, 1.75f, 19.4f), 0.42f, 0.62f, GB.Metal, false);
            GB.Box(g, "GiantSpoon", new Vector3(-0.6f, 2.4f, 19.3f), new Vector3(0.09f, 1.2f, 0.05f), GB.Wood, new Vector3(0f, 0f, -20f), false);
            GB.Cylinder(g, "FryingPan", new Vector3(0.85f, 1.48f, 18.6f), 0.36f, 0.08f, GB.MatteBlack, false);
            GB.Box(g, "PanHandle", new Vector3(0.85f, 1.5f, 18.05f), new Vector3(0.08f, 0.05f, 0.5f), GB.MatteBlack, 0f, false);
            GB.Box(g, "Backsplash", new Vector3(0f, 2.15f, 19.785f), new Vector3(3.8f, 1.5f, 0.03f), GB.Tile, 0f, false);
            GB.Box(g, "Hood", new Vector3(0f, 3.3f, 19.15f), new Vector3(4f, 0.6f, 1.3f), mCream, 0f, false);
            GB.Box(g, "HoodChimney", new Vector3(0f, 4.3f, 19.5f), new Vector3(1.4f, 1.4f, 0.6f), mCream, 0f, false);
            GB.Box(g, "HoodGlowTrim", new Vector3(0f, 3f, 18.49f), new Vector3(4f, 0.04f, 0.03f), mGlowTape, 0f, false);
            GB.Box(g, "CooktopGlowEdge", new Vector3(0f, 1.42f, 18.19f), new Vector3(3.6f, 0.03f, 0.03f), mGlowTape, 0f, false);
            GB.PointLight(g, "PilotLight", new Vector3(0f, 1.8f, 18.4f), new Color(0.65f, 0.82f, 1f), 5f, 1.2f);
        }

        /// Giant 4.2 m mint fridge, x 3.0..6.4. Its left door hangs wide open: cold blue-white light spills out and
        /// a reel sits in the crisper (cold storage). Kids' drawings on the freezer door.
        static void Fridge(Transform g)
        {
            GB.Box(g, "Back", new Vector3(4.7f, 2.1f, 19.725f), new Vector3(3.4f, 4.2f, 0.15f), mFridge);
            GB.Box(g, "SideW", new Vector3(3.075f, 2.1f, 18.925f), new Vector3(0.15f, 4.2f, 1.45f), mFridge);
            GB.Box(g, "SideE", new Vector3(6.325f, 2.1f, 18.925f), new Vector3(0.15f, 4.2f, 1.45f), mFridge);
            GB.Box(g, "Top", new Vector3(4.7f, 4.125f, 18.925f), new Vector3(3.1f, 0.15f, 1.45f), mFridge);
            GB.Box(g, "CrisperPlinth", new Vector3(4.7f, 0.275f, 18.925f), new Vector3(3.1f, 0.55f, 1.45f), mFridge);
            GB.Box(g, "FreezerFloor", new Vector3(4.7f, 3.025f, 18.925f), new Vector3(3.1f, 0.15f, 1.45f), mFridge);
            GB.Box(g, "Partition", new Vector3(4.7f, 1.75f, 18.925f), new Vector3(0.15f, 2.4f, 1.45f), mFridge);
            GB.Box(g, "FreezerDoor", new Vector3(4.7f, 3.575f, 18.15f), new Vector3(3.4f, 0.95f, 0.1f), mFridge);
            GB.Box(g, "RightDoor", new Vector3(5.55f, 1.75f, 18.15f), new Vector3(1.7f, 2.4f, 0.1f), mFridge);
            GB.Box(g, "RightHandle", new Vector3(4.85f, 1.75f, 18.07f), new Vector3(0.05f, 0.9f, 0.05f), GB.Metal, 0f, false);
            GB.Box(g, "FreezerHandle", new Vector3(4.7f, 3.25f, 18.07f), new Vector3(1.2f, 0.05f, 0.05f), GB.Metal, 0f, false);

            // left door swung 110 degrees open on its west hinge; free edge ends near (2.4, 16.55)
            var door = GB.Group(g, "LeftDoor_Open", new Vector3(3f, 0f, 18.15f), 110f);
            GB.Box(door, "Door", new Vector3(0.85f, 1.75f, 0f), new Vector3(1.7f, 2.4f, 0.1f), mFridge);
            GB.Box(door, "Bin_0", new Vector3(0.85f, 1.2f, 0.14f), new Vector3(1.4f, 0.18f, 0.18f), mFridgeInner, 0f, false);
            GB.Box(door, "Bin_1", new Vector3(0.85f, 2f, 0.14f), new Vector3(1.4f, 0.18f, 0.18f), mFridgeInner, 0f, false);
            GB.Cylinder(door, "Bottle", new Vector3(0.5f, 2.29f, 0.14f), 0.07f, 0.4f, mFelt[3], false);
            GB.Box(door, "Handle", new Vector3(1.55f, 1.75f, -0.08f), new Vector3(0.05f, 0.9f, 0.05f), GB.Metal, 0f, false);

            // lit interior of the open half (x 3.15..4.62)
            GB.Box(g, "LightPanel", new Vector3(3.89f, 1.75f, 19.635f), new Vector3(1.4f, 2.3f, 0.02f), mFridgeGlow, 0f, false);
            GB.Box(g, "GlassShelf_0", new Vector3(3.89f, 1.35f, 18.925f), new Vector3(1.45f, 0.03f, 1.4f), mFridgeInner, 0f, false);
            GB.Box(g, "GlassShelf_1", new Vector3(3.89f, 2.15f, 18.925f), new Vector3(1.45f, 0.03f, 1.4f), mFridgeInner, 0f, false);
            GB.Cylinder(g, "MilkBottle", new Vector3(3.55f, 1.615f, 19.15f), 0.13f, 0.5f, mPaper, false);
            GB.Cylinder(g, "Pie", new Vector3(4.2f, 1.415f, 19.2f), 0.26f, 0.1f, mCrust, false);
            GB.Ellipsoid(g, "JellyMould", new Vector3(4.2f, 2.305f, 19f), new Vector3(0.45f, 0.28f, 0.45f), mFelt[0], Vector3.zero, false);

            for (int i = 0; i < 3; i++)
                GB.Box(g, $"KidsDrawing_{i}", new Vector3(3.6f + 0.95f * i, 3.55f, 18.095f), new Vector3(0.45f, 0.55f, 0.01f), i == 1 ? mFelt[2] : mPaper, new Vector3(0f, 0f, 4f - 4f * i), false);
            GB.Box(g, "GlowEdge_E", new Vector3(6.41f, 2.1f, 18.1f), new Vector3(0.03f, 4.2f, 0.03f), mGlowTape, 0f, false);
            GB.Box(g, "GlowEdge_Top", new Vector3(4.7f, 4.21f, 18.1f), new Vector3(3.4f, 0.03f, 0.03f), mGlowTape, 0f, false);
            GB.PointLight(g, "FridgeLight", new Vector3(3.9f, 1.9f, 18.7f), new Color(0.78f, 0.9f, 1f), 4.5f, 1f);
        }

        // ---------------------------------------------------------------- island, counters, scullery

        /// Central island x -3..3, z 9.8..12.4, 1.2 m high: the main loop. A 1.0 x 1.0 m child-sized tunnel runs through it
        /// at x -2..-1 (player only). Pinny's spike mark is behind it, facing the camera like a cooking-show host.
        static void Island(Transform g)
        {
            GB.Box(g, "BlockW", new Vector3(-2.45f, 0.5f, 11.1f), new Vector3(0.9f, 1f, 2.4f), mRedPaint);
            GB.Box(g, "BlockE", new Vector3(0.95f, 0.5f, 11.1f), new Vector3(3.9f, 1f, 2.4f), mRedPaint);
            GB.Box(g, "ButcherTop", new Vector3(0f, 1.1f, 11.1f), new Vector3(6f, 0.2f, 2.6f), GB.Wood);
            GB.Box(g, "GlowEdge_S", new Vector3(0f, 1.2f, 9.79f), new Vector3(6f, 0.03f, 0.03f), mGlowTape, 0f, false);
            GB.Box(g, "GlowEdge_N", new Vector3(0f, 1.2f, 12.41f), new Vector3(6f, 0.03f, 0.03f), mGlowTape, 0f, false);
            GB.Box(g, "TunnelMouthUV", new Vector3(-1.5f, 0.98f, 9.88f), new Vector3(1f, 0.03f, 0.02f), mUV, 0f, false);
            GB.Box(g, "UVHand_0", new Vector3(-1.7f, 0.016f, 10.05f), new Vector3(0.11f, 0.01f, 0.15f), mUV, 12f, false);
            GB.Box(g, "UVHand_1", new Vector3(-1.32f, 0.016f, 10.35f), new Vector3(0.11f, 0.01f, 0.15f), mUV, -8f, false);

            GB.Ellipsoid(g, "MixingBowl", new Vector3(1.6f, 1.35f, 11f), new Vector3(1f, 0.5f, 1f), mCream, Vector3.zero, false);
            GB.Cylinder(g, "RollingPin", new Vector3(0.1f, 1.29f, 11.7f), 0.09f, 1.2f, GB.Wood, new Vector3(0f, 0f, 90f), false);
            GB.Box(g, "CuttingBoard", new Vector3(-0.5f, 1.215f, 10.8f), new Vector3(1f, 0.03f, 0.6f), GB.Plywood, 0f, false);
            GB.Cylinder(g, "GiantCookie", new Vector3(-2.3f, 1.23f, 11.3f), 0.3f, 0.06f, mCrust, false);
            GB.Cylinder(g, "CrumbJar", new Vector3(2.5f, 1.5f, 11.75f), 0.25f, 0.6f, GB.Metal, false);

            // pot rack hanging over the island, pots' bottoms at 2.39 m
            GB.Cylinder(g, "PotRackBar", new Vector3(0.5f, 2.9f, 11.1f), 0.04f, 4f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(g, "RackChainW", new Vector3(-1.3f, 3.95f, 11.1f), 0.015f, 2.1f, GB.Metal, false);
            GB.Cylinder(g, "RackChainE", new Vector3(2.3f, 3.95f, 11.1f), 0.015f, 2.1f, GB.Metal, false);
            for (int i = 0; i < 3; i++)
                GB.Cylinder(g, $"HangingPot_{i}", new Vector3(-0.6f + 1.1f * i, 2.69f, 11.1f), 0.22f, 0.34f, GB.Metal, false);

            SpikeX(g, "PinnyMark", new Vector3(0.8f, 0.01f, 13.4f), 0.7f);
        }

        /// Free-standing long counter (x 5.2..6.6, z 6..13) and the wall counter with the giant sink (x 9.6..10.85, z 5..13):
        /// a 3 m galley between them makes a second loop. The plate rack blocks sightlines at head height.
        static void Counters(Transform g)
        {
            GB.Box(g, "LongCounter", new Vector3(5.9f, 0.54f, 9.5f), new Vector3(1.3f, 1.08f, 6.9f), mRedPaint);
            GB.Box(g, "LongCounterTop", new Vector3(5.9f, 1.14f, 9.5f), new Vector3(1.4f, 0.12f, 7f), mCounter);
            GB.Box(g, "PlateRack", new Vector3(5.9f, 1.75f, 9.6f), new Vector3(0.08f, 1.1f, 2.4f), GB.Wood);
            for (int i = 0; i < 4; i++)
                GB.Cylinder(g, $"RackPlate_{i}", new Vector3(5.66f, 1.62f, 8.85f + 0.5f * i), 0.38f, 0.04f, mPaper, new Vector3(0f, 0f, 90f), false);
            GB.Box(g, "LongGlowW", new Vector3(5.19f, 1.2f, 9.5f), new Vector3(0.03f, 0.03f, 7f), mGlowTape, 0f, false);
            GB.Box(g, "LongGlowE", new Vector3(6.61f, 1.2f, 9.5f), new Vector3(0.03f, 0.03f, 7f), mGlowTape, 0f, false);

            GB.Box(g, "WallCounter", new Vector3(10.25f, 0.54f, 9f), new Vector3(1.2f, 1.08f, 8f), mRedPaint);
            GB.Box(g, "WallCounterTop", new Vector3(10.225f, 1.14f, 9f), new Vector3(1.25f, 0.12f, 8f), mCounter);
            GB.Box(g, "SinkBasin", new Vector3(10.2f, 1.205f, 9f), new Vector3(0.85f, 0.012f, 1.4f), mSteel, 0f, false);
            GB.Cylinder(g, "FaucetRiser", new Vector3(10.65f, 1.65f, 9f), 0.05f, 0.9f, GB.Metal, false);
            GB.Cylinder(g, "FaucetSpout", new Vector3(10.4f, 2.07f, 9f), 0.045f, 0.55f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Box(g, "SinkDoorS", new Vector3(9.63f, 0.55f, 8.6f), new Vector3(0.03f, 0.88f, 0.76f), mCream, 0f, false);
            GB.Box(g, "SinkDoorN", new Vector3(9.63f, 0.55f, 9.4f), new Vector3(0.03f, 0.88f, 0.76f), mCream, 0f, false);
            GB.Box(g, "SinkKnobS", new Vector3(9.605f, 0.78f, 8.9f), new Vector3(0.03f, 0.16f, 0.04f), GB.Metal, 0f, false);
            GB.Box(g, "SinkKnobN", new Vector3(9.605f, 0.78f, 9.1f), new Vector3(0.03f, 0.16f, 0.04f), GB.Metal, 0f, false);
            GB.Box(g, "UpperCabinets", new Vector3(10.6f, 2.9f, 9f), new Vector3(0.5f, 0.8f, 8f), mCream, 0f, false);
            for (int i = 0; i < 3; i++)
                GB.Box(g, $"CabinetSeam_{i}", new Vector3(10.345f, 2.9f, 7f + 2f * i), new Vector3(0.01f, 0.76f, 0.03f), GB.MatteBlack, 0f, false);
            GB.Box(g, "WallCounterGlow", new Vector3(9.59f, 1.2f, 9f), new Vector3(0.03f, 0.03f, 8f), mGlowTape, 0f, false);
        }

        /// Scullery corner (x 6.6..10.85, z 13..19.8): mop sink, utility shelf with the fuse, floor drain, second recharge lamp.
        static void Scullery(Transform g)
        {
            GB.Box(g, "MopSink", new Vector3(10.3f, 0.45f, 17.6f), new Vector3(1.1f, 0.9f, 2.4f), mSteel);
            GB.Box(g, "MopSinkWater", new Vector3(10.3f, 0.906f, 17.6f), new Vector3(0.9f, 0.012f, 2.2f), GB.Water, 0f, false);
            GB.Cylinder(g, "MopSinkTap", new Vector3(10.72f, 1.15f, 17.6f), 0.04f, 0.5f, GB.Metal, false);

            GB.Box(g, "UtilityShelfW", new Vector3(6.78f, 0.45f, 19.475f), new Vector3(0.06f, 0.9f, 0.55f), GB.Metal);
            GB.Box(g, "UtilityShelfE", new Vector3(7.72f, 0.45f, 19.475f), new Vector3(0.06f, 0.9f, 0.55f), GB.Metal);
            GB.Box(g, "UtilityShelfLow", new Vector3(7.25f, 0.1f, 19.475f), new Vector3(1f, 0.04f, 0.55f), GB.Metal, 0f, false);
            GB.Box(g, "UtilityShelfTop", new Vector3(7.25f, 0.875f, 19.475f), new Vector3(1f, 0.05f, 0.55f), GB.Metal);
            GB.Box(g, "DishCrate", new Vector3(7.25f, 0.32f, 19.475f), new Vector3(0.6f, 0.4f, 0.45f), GB.Plywood, 0f, false);

            GB.Box(g, "DrainGrate", new Vector3(8.2f, 0.008f, 18.5f), new Vector3(0.7f, 0.016f, 0.7f), GB.MatteBlack, 0f, false);
            for (int i = 0; i < 3; i++)
                GB.Box(g, $"DrainSlat_{i}", new Vector3(8.2f, 0.02f, 18.3f + 0.2f * i), new Vector3(0.6f, 0.01f, 0.05f), GB.Metal, 0f, false);

            var lamp = GB.Group(g, "RechargeLamp_Scullery");
            GB.Box(lamp, "Backplate", new Vector3(9f, 2.2f, 19.78f), new Vector3(0.25f, 0.35f, 0.04f), GB.Metal, 0f, false);
            GB.Box(lamp, "Bracket", new Vector3(9f, 2.2f, 19.65f), new Vector3(0.08f, 0.08f, 0.22f), GB.Metal, 0f, false);
            GB.GlowLamp(lamp, "Lamp", new Vector3(9f, 2.2f, 19.48f), LampAmber, 0.12f, 4f, 1.2f);
        }

        /// Trail breakers (water), and the old stage trapdoor.
        static void WaterAndTraps(Transform g)
        {
            // TB1: scullery mop buckets and their spill
            GB.Box(g, "Spill_Scullery", new Vector3(8.6f, 0.015f, 17f), new Vector3(2.2f, 0.01f, 2.4f), GB.Water, 0f, false);
            MopBucket(g, "MopBucket_A", new Vector3(8.75f, 0f, 17.45f), 10f);
            GB.Cylinder(g, "MopBucket_B_Tipped", new Vector3(9.3f, 0.28f, 16.25f), 0.28f, 0.5f, mBucket, new Vector3(0f, 0f, 90f), true);

            // TB2: sprinkler test valve on the west wall; the aisle it soaks between table and pantry
            GB.Cylinder(g, "SprinklerRiser", new Vector3(-10.72f, 2.5f, 12.7f), 0.06f, 5f, mPipeRed, true);
            GB.Cylinder(g, "TestValveWheel", new Vector3(-10.6f, 1.3f, 12.7f), 0.18f, 0.04f, mPipeRed, new Vector3(0f, 0f, 90f), false);
            GB.Box(g, "TestValveSign", new Vector3(-10.835f, 1.3f, 13.05f), new Vector3(0.01f, 0.3f, 0.42f), mPaper, 0f, false);
            GB.Cylinder(g, "SprinklerBranch", new Vector3(-10.1f, 4.5f, 12.7f), 0.04f, 1.25f, mPipeRed, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(g, "SprinklerHead", new Vector3(-9.5f, 4.42f, 12.7f), 0.07f, 0.12f, GB.Metal, false);
            GB.Box(g, "Soak_DiningAisle", new Vector3(-9.6f, 0.015f, 12.35f), new Vector3(2.4f, 0.01f, 1.9f), GB.Water, 0f, false);

            // TB3: mop bucket and wet-floor sign at the island / fridge junction
            GB.Box(g, "Spill_Fridge", new Vector3(4.3f, 0.015f, 14.6f), new Vector3(2f, 0.01f, 2f), GB.Water, 0f, false);
            MopBucket(g, "MopBucket_C", new Vector3(4.95f, 0f, 15.25f), -20f);
            var sign = GB.Group(g, "WetFloorSign", new Vector3(3.6f, 0f, 14f), 30f);
            GB.Box(sign, "PanelA", new Vector3(0f, 0.45f, 0.12f), new Vector3(0.4f, 0.9f, 0.02f), mBucket, new Vector3(-12f, 0f, 0f), false);
            GB.Box(sign, "PanelB", new Vector3(0f, 0.45f, -0.12f), new Vector3(0.4f, 0.9f, 0.02f), mBucket, new Vector3(12f, 0f, 0f), false);

            // old stage trapdoor in the set floor (Digsley hears through its gap) with its red cue lamp
            GB.Box(g, "Trapdoor", new Vector3(2.4f, 0.01f, 6.6f), new Vector3(1.2f, 0.02f, 1.2f), GB.Wood, 0f, false);
            SpikeX(g, "TrapdoorX", new Vector3(2.4f, 0.02f, 6.6f), 1.2f);
            GB.Cylinder(g, "TrapCueLampPost", new Vector3(3.2f, 0.2f, 7.35f), 0.04f, 0.4f, GB.MatteBlack, false);
            GB.Sphere(g, "TrapCueLamp", new Vector3(3.2f, 0.43f, 7.35f), 0.06f, mCueRed, false);
        }

        /// Striped plush tapir calves left lying on routes; kicking one squeaks and pulls Pinny straight over.
        static void Calves(Transform g)
        {
            Calf(g, "PlushCalf_Galley", new Vector3(7.9f, 0f, 6.8f), 70f);
            Calf(g, "PlushCalf_Dining", new Vector3(-9.8f, 0f, 6.2f), 160f);
            Calf(g, "PlushCalf_Studio", new Vector3(6.7f, 0f, 3.9f), -30f);
        }

        // ---------------------------------------------------------------- gameplay markers

        static void Markers(Transform g)
        {
            GB.Marker(g, MarkerKind.GloamieSpawn, new Vector3(0.8f, 0f, 13.4f), 180f, "Auntie Pinny's spike mark behind the island, facing camera; starts in Tidy");

            Vector3[] patrol =
            {
                new Vector3(0f, 0f, 16.2f),
                new Vector3(-3.3f, 0f, 17.3f),
                new Vector3(-6.4f, 0f, 16.6f),
                new Vector3(-4.5f, 0f, 8.6f),
                new Vector3(8.1f, 0f, 9.6f),
                new Vector3(8f, 0f, 14.8f),
                new Vector3(-4f, 0f, 3f),
            };
            string[] patrolNotes =
            {
                "Tidy: stirring the stockpot at the stove",
                "Tidy: straightening the labels on her shelf",
                "Tidy: counting jars in the pantry",
                "Tidy: laying places at the dining table",
                "Tidy: rinsing at the giant sink (galley)",
                "Tidy: wringing mops in the scullery",
                "Tidy: coiling cables on the studio floor",
            };
            for (int i = 0; i < patrol.Length; i++)
                GB.Marker(g, MarkerKind.PatrolPoint, patrol[i], 0f, patrolNotes[i]);

            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(3.75f, 0.55f, 18.6f), 180f, "Reel in the open giant fridge's crisper (cold storage), 0.55 m, reach in from the floor");
            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(-7.3f, 1.23f, 9.3f), 180f, "Reel served on a giant dinner plate on the table top; climb the Little Helper step");
            GB.Marker(g, MarkerKind.FuseSpawn, new Vector3(7.25f, 0.9f, 19.45f), 180f, "Fuse on the scullery utility shelf, 0.9 m");
            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(9.7f, 0.46f, 2.5f), -90f, "Battery on the crew-locker bench");
            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(-2.6f, 0.92f, 19.3f), 180f, "Battery 'tidied' onto Pinny's shelf counter (0.92 m), labelled");

            GB.Marker(g, MarkerKind.Breaker, new Vector3(-9.2f, 0f, 19.25f), 180f, "Wing breaker on the pantry's back wall (needs the fuse); loud 10 s crescendo; exits: south door, east door, crawl gap");
            GB.Marker(g, MarkerKind.CueLampDesk, new Vector3(-7.4f, 0f, 1.15f), 180f, "Stage-manager desk with the black-light cue lamp (Curtain Call)");

            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(-7.4f, 0f, 2.25f), 180f, "Under the stage-manager desk (low gap under its front panel)");
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(-10.35f, 0f, 1.7f), 90f, "Between the spare aprons on the costume rack");
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(10.7f, 0f, 2f), -90f, "Inside the middle crew locker (name card: PINNY)");
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(10.25f, 0f, 9f), -90f, "Cupboard under the giant sink");
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(-5.95f, 0f, 19.5f), 180f, "Inside the SNACK-TIME PROPS trunk in the pantry");

            GB.Marker(g, MarkerKind.RechargeLamp, new Vector3(5.5f, 0f, 0.75f), 0f, "Amber wall lamp, 2.2 m bracket, beside the entrance");
            GB.Marker(g, MarkerKind.RechargeLamp, new Vector3(9f, 0f, 19.05f), 180f, "Amber wall lamp, 2.2 m bracket, scullery dead end (Deepwick's pick from Act IV)");

            GB.Marker(g, MarkerKind.Hatch, new Vector3(8.2f, 0f, 18.5f), 0f, "Scullery floor drain; its gap carries noise down to Digsley");
            GB.Marker(g, MarkerKind.Hatch, new Vector3(2.4f, 0f, 6.6f), 0f, "Old stage trapdoor with red cue lamp; 8.4 m from the table");

            GB.Marker(g, MarkerKind.Handprint, new Vector3(-1.5f, 0f, 9.5f), 180f, "Crawl tunnel through the island, 1.0 x 1.0 m; Pinny must go round");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(-8.9f, 0f, 9.6f), -90f, "Crawl space under the dining table, 1.0 m clearance");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(-10f, 0f, 13.05f), 180f, "Crawl gap under the pantry's south wall, 1.0 x 1.0 m; player-only exit");

            GB.Marker(g, MarkerKind.StoryProp, new Vector3(-3.3f, 0f, 18.5f), 180f, "Pinny's shelf — caught items end up here, labelled");
            GB.Marker(g, MarkerKind.StoryProp, new Vector3(-7.5f, 1.1f, 2.15f), 180f, "Running orders: Tidy-Up Time grows from 2 min (1989) to 11 min (1997)");
            GB.Marker(g, MarkerKind.StoryProp, new Vector3(-5.95f, 0f, 14.7f), -90f, "Crumb Jar mail sacks: 'send Auntie Pinny anything you've lost', some postmarked after 1997");

            GB.Marker(g, MarkerKind.TrailBreaker, new Vector3(8.35f, 0f, 16.6f), 0f, "Mop-bucket spill: wade through to break Pinny's scent trail");
            GB.Marker(g, MarkerKind.TrailBreaker, new Vector3(-9.6f, 0f, 12.35f), 90f, "Sprinkler test valve (red riser): opening it soaks this aisle; wading breaks the trail");
            GB.Marker(g, MarkerKind.TrailBreaker, new Vector3(4.1f, 0f, 14.4f), 0f, "Mop bucket and spill by the fridge: wade through to break the trail");
        }
    }
}
