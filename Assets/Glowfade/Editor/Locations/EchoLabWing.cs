using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Act IV "Tap-Along": Dr. Tapwell's Echo Lab, the Starhouse's old Sound and Hearing exhibit hall.
    /// Tapwell hunts hiding spots, so the wing is built around choice. It has nine hiding spots in four zones.
    /// Doors act as chokepoints and carry his UV chalk tallies. Two child-sized crawl holes are routes he
    /// cannot follow through, and a loop runs round the Giant Ear in the middle of the hall.
    ///
    /// Plan (local metres, entrance at the origin, +Z into the wing):
    ///   Front of hall   z 0..7, full width: whisper-dish pair A (x = +-10.35, z 4.2) facing across the room,
    ///                   locker bank (south-east), stage-manager desk with the cue lamp (south-west).
    ///   Main hall       |x| < 6.4, z 7..20: Giant Ear island (0, 10.5), oscilloscope bench, specimen cupboard,
    ///                   whisper-dish pair B (x = +-5.75, z 13.6), Tap-Along stage (z 15.6..20, deck 0.45 m).
    ///   Specimen Store  x -11..-6.6, z 7..20: specimen cases, humming tone generator, dissection bench.
    ///                   Doors at (-8, 7) and (-6.5, 17), crawl hole at (-6.5, 11).
    ///   Wardrobe Bay    x 6.6..11, z 7..20: costume racks, prop trunk, prop table, breaker, the tapping wall.
    ///                   Doors at (6.5, 9) and (6.5, 17), crawl hole at (8.6, 7).
    public static class EchoLabWing
    {
        const float FlatHeight = 3.6f;   // partition set flats; the 1.4 m above them lets glow and sound spill over
        const float DoorWidth = 1.6f;
        const float DoorTop = 2.4f;
        const float StageTop = 0.45f;

        static readonly Color Lilac = new Color(0.78f, 0.6f, 1f);        // Tapwell's fingertip glow
        static readonly Color LampAmber = new Color(1f, 0.68f, 0.32f);   // recharge lamps
        static readonly Color UV = new Color(0.45f, 0.2f, 1f);           // black-light cue lamp

        static Material lino, wallBlack, stageDeck, foam, foamWedge, dishPaint, dishBowl, plinth, earPink, earDark,
            lockerPaint, lockerDoor, cabinetWood, cabinetDoor, benchTop, rackGrey, trunkCanvas, strap, chalkboard, chalk,
            card, gaffer, ochre, felt, labCoat, furCharcoal, brass, jarCloudy, leverRed;
        static Material lilacGlow, lilacTape, uvGlow, jarGlow, redCue, amberDial, scopeGlow, cueGo;

        public static void Build(Transform root)
        {
            Materials();
            Shell(root);
            FrontOfHall(root);
            MainHall(root);
            TapAlongSet(root);
            SpecimenStore(root);
            WardrobeBay(root);
            Markers(root);
        }

        static void Materials()
        {
            lino = GB.Mat("EchoLab_Lino", new Color(0.085f, 0.08f, 0.095f), 0.3f);
            wallBlack = GB.Mat("EchoLab_WallBlack", new Color(0.06f, 0.055f, 0.07f), 0.08f);
            stageDeck = GB.Mat("EchoLab_StageDeck", new Color(0.04f, 0.038f, 0.045f), 0.12f);
            foam = GB.Mat("EchoLab_Foam", new Color(0.13f, 0.12f, 0.15f), 0.02f);
            foamWedge = GB.Mat("EchoLab_FoamWedge", new Color(0.08f, 0.075f, 0.095f), 0.02f);
            dishPaint = GB.Mat("EchoLab_DishPaint", new Color(0.58f, 0.56f, 0.5f), 0.3f);
            dishBowl = GB.Mat("EchoLab_DishBowl", new Color(0.2f, 0.19f, 0.22f), 0.35f);
            plinth = GB.Mat("EchoLab_PlinthPaint", new Color(0.18f, 0.12f, 0.24f), 0.2f);
            earPink = GB.Mat("EchoLab_EarLatex", new Color(0.62f, 0.46f, 0.46f), 0.3f);
            earDark = GB.Mat("EchoLab_EarInner", new Color(0.36f, 0.22f, 0.25f), 0.3f);
            lockerPaint = GB.Mat("EchoLab_LockerGreen", new Color(0.2f, 0.25f, 0.23f), 0.3f, 0.3f);
            lockerDoor = GB.Mat("EchoLab_LockerDoor", new Color(0.24f, 0.29f, 0.27f), 0.35f, 0.3f);
            cabinetWood = GB.Mat("EchoLab_CabinetWood", new Color(0.24f, 0.16f, 0.1f), 0.3f);
            cabinetDoor = GB.Mat("EchoLab_CabinetDoor", new Color(0.3f, 0.2f, 0.12f), 0.3f);
            benchTop = GB.Mat("EchoLab_BenchTop", new Color(0.08f, 0.08f, 0.085f), 0.45f);
            rackGrey = GB.Mat("EchoLab_RackGrey", new Color(0.2f, 0.21f, 0.22f), 0.3f, 0.4f);
            trunkCanvas = GB.Mat("EchoLab_TrunkCanvas", new Color(0.16f, 0.2f, 0.26f), 0.15f);
            strap = GB.Mat("EchoLab_TrunkStrap", new Color(0.3f, 0.2f, 0.12f), 0.2f);
            chalkboard = GB.Mat("EchoLab_Chalkboard", new Color(0.05f, 0.09f, 0.07f), 0.05f);
            chalk = GB.Mat("EchoLab_Chalk", new Color(0.78f, 0.78f, 0.74f), 0.05f);
            card = GB.Mat("EchoLab_Card", new Color(0.82f, 0.78f, 0.66f), 0.1f);
            gaffer = GB.Mat("EchoLab_SilverGaffer", new Color(0.62f, 0.62f, 0.64f), 0.45f, 0.4f);
            ochre = GB.Mat("EchoLab_OchreTweed", new Color(0.55f, 0.41f, 0.14f), 0.1f);
            felt = GB.Mat("EchoLab_FeltPlum", new Color(0.22f, 0.13f, 0.28f), 0.03f);
            labCoat = GB.Mat("EchoLab_LabCoat", new Color(0.62f, 0.62f, 0.58f), 0.05f);
            furCharcoal = GB.Mat("EchoLab_FurCharcoal", new Color(0.16f, 0.16f, 0.17f), 0.03f);
            brass = GB.Mat("EchoLab_Brass", new Color(0.55f, 0.42f, 0.18f), 0.5f, 0.8f);
            jarCloudy = GB.Mat("EchoLab_JarCloudy", new Color(0.42f, 0.34f, 0.18f), 0.6f);
            leverRed = GB.Mat("EchoLab_LeverRed", new Color(0.55f, 0.08f, 0.06f), 0.3f);

            lilacGlow = GB.Glow("EchoLab_LilacDot", Lilac, 3f);
            lilacTape = GB.Glow("EchoLab_LilacTape", Lilac, 1.2f);
            uvGlow = GB.Glow("EchoLab_UVTube", UV, 3f);
            jarGlow = GB.Glow("EchoLab_JarGlow", new Color(0.85f, 0.62f, 1f), 1.4f);
            redCue = GB.Glow("EchoLab_RedCue", new Color(1f, 0.12f, 0.08f), 2.5f);
            amberDial = GB.Glow("EchoLab_AmberDial", new Color(1f, 0.6f, 0.2f), 2f);
            scopeGlow = GB.Glow("EchoLab_ScopeScreen", new Color(0.55f, 1f, 0.7f), 1.2f);
            cueGo = GB.Glow("EchoLab_CueGo", new Color(0.3f, 1f, 0.4f), 2f);
        }

        // ------------------------------------------------------------------ shell and partitions

        static void Shell(Transform root)
        {
            GB.Room(root, "Room", new Vector3(0f, 0f, 10f), new Vector3(22f, 5f, 20f), lino, wallBlack, GB.MatteBlack, new Doorway(Side.South));

            var parts = GB.Group(root, "Partitions");
            GB.Wall(parts, "SpecimenStore_FrontWall", new Vector3(-11f, 0f, 7f), new Vector3(-6.4f, 0f, 7f), FlatHeight, 0.2f, GB.MatteBlack,
                new Opening(3f, DoorWidth, 0f, DoorTop));      // door at x -8
            GB.Wall(parts, "SpecimenStore_SideWall", new Vector3(-6.5f, 0f, 7f), new Vector3(-6.5f, 0f, 20f), FlatHeight, 0.2f, GB.MatteBlack,
                new Opening(4f, 1f, 0f, 1f),                   // child-sized crawl hole at z 11
                new Opening(10f, DoorWidth, 0f, DoorTop));     // door at z 17
            GB.Wall(parts, "WardrobeBay_FrontWall", new Vector3(6.4f, 0f, 7f), new Vector3(11f, 0f, 7f), FlatHeight, 0.2f, GB.MatteBlack,
                new Opening(2.2f, 1f, 0f, 1f));                // child-sized crawl hole at x 8.6
            GB.Wall(parts, "WardrobeBay_SideWall", new Vector3(6.5f, 0f, 7f), new Vector3(6.5f, 0f, 20f), FlatHeight, 0.2f, GB.MatteBlack,
                new Opening(2f, DoorWidth, 0f, DoorTop),       // door at z 9
                new Opening(10f, DoorWidth, 0f, DoorTop));     // door at z 17

            // Plywood door frames. Tapwell's UV-only chalk tallies go on the plates on their hall side.
            DoorFrame(parts, "DoorFrame_StoreFront", new Vector3(-8f, 0f, 7f), 0f, -1f);
            DoorFrame(parts, "DoorFrame_StoreBack", new Vector3(-6.5f, 0f, 17f), 90f, 1f);
            DoorFrame(parts, "DoorFrame_WardrobeFront", new Vector3(6.5f, 0f, 9f), 90f, -1f);
            DoorFrame(parts, "DoorFrame_WardrobeBack", new Vector3(6.5f, 0f, 17f), 90f, -1f);
            CrawlFrame(parts, "CrawlHole_Store", new Vector3(-6.5f, 0f, 11f), 90f);
            CrawlFrame(parts, "CrawlHole_Wardrobe", new Vector3(8.6f, 0f, 7f), 0f);

            // faded exhibit signage over the entrance, seen when you turn back towards the corridor
            var sign = GB.Group(root, "Signage");
            GB.Box(sign, "EchoLabSign", new Vector3(0f, 3.75f, 0.05f), new Vector3(3.6f, 0.7f, 0.1f), GB.Plywood, collider: false);
            float[] letters = { -1.3f, -0.85f, -0.4f, 0.35f, 0.8f, 1.25f };   // "ECHO LAB", a few letters long gone
            for (int i = 0; i < letters.Length; i++)
                GB.Box(sign, "Letter" + i, new Vector3(letters[i], 3.75f, 0.105f), new Vector3(0.3f, 0.4f, 0.01f), lilacTape, collider: false);
            GB.Ellipsoid(sign, "LogoEarL", new Vector3(-2.15f, 3.85f, 0.08f), new Vector3(0.5f, 0.8f, 0.08f), earPink, new Vector3(0f, 0f, 15f), false);
            GB.Ellipsoid(sign, "LogoEarR", new Vector3(2.15f, 3.85f, 0.08f), new Vector3(0.5f, 0.8f, 0.08f), earPink, new Vector3(0f, 0f, -15f), false);
            GB.Box(sign, "OldBanner_SoundAndHearing", new Vector3(-6.5f, 4.2f, 0.03f), new Vector3(5f, 0.45f, 0.04f), dishPaint, collider: false);
        }

        // ------------------------------------------------------------------ front of hall (z 0..7)

        static void FrontOfHall(Transform root)
        {
            var front = GB.Group(root, "FrontOfHall");

            // whisper-dish pair A: facing each other across the whole hall; lilac tape marks the whisper line. Each dish is
            // backed onto its side wall (base touching it, 0.62 m behind the face) so there is no nook behind the dish that the
            // player fits into but Tapwell's 1 m-wide agent cannot reach.
            Dish(front, "WhisperDish_A_West", new Vector3(-10.35f, 0f, 4.2f), 90f);
            Dish(front, "WhisperDish_A_East", new Vector3(10.35f, 0f, 4.2f), -90f);
            GlowTapeLine(front, "WhisperLine_A", -8.7f, 8.7f, 4.2f);

            // locker bank on the south wall: lockers 2 and 4 are hiding spots, locker 3 is already taped shut
            for (int i = 0; i < 4; i++)
                Locker(front, "Locker" + (i + 1), new Vector3(4.4f + 0.8f * i, 0f, 0.275f), 0f, i == 1 || i == 3, i == 2);

            StageManagerDesk(front);
            FloorVent(front, "FloorVent_Front", new Vector3(3.4f, 0f, 4.9f), 0f);
        }

        /// Freestanding prompt desk spanning x -6.6..-4.6, z 1.6..2.3, top 0.8 m. The operator stands on its south side
        /// (local +Z) and looks north over it into the hall.
        static void StageManagerDesk(Transform parent)
        {
            var g = GB.Group(parent, "StageManagerDesk", new Vector3(-5.6f, 0f, 1.95f), 180f);
            GB.Box(g, "Top", new Vector3(0f, 0.77f, 0f), new Vector3(2f, 0.06f, 0.7f), GB.Wood);
            GB.Box(g, "PedestalL", new Vector3(-0.78f, 0.37f, 0f), new Vector3(0.44f, 0.74f, 0.66f), GB.Plywood);
            GB.Box(g, "PedestalR", new Vector3(0.78f, 0.37f, 0f), new Vector3(0.44f, 0.74f, 0.66f), GB.Plywood);
            GB.Box(g, "HallPanel", new Vector3(0f, 0.39f, -0.31f), new Vector3(1.12f, 0.7f, 0.06f), GB.Plywood);
            GB.Box(g, "CueConsole", new Vector3(0f, 0.9f, -0.18f), new Vector3(1.2f, 0.2f, 0.26f), GB.MatteBlack, new Vector3(15f, 0f, 0f), false);
            Material[] buttons = { amberDial, cueGo, uvGlow };   // standby, go, Curtain Call
            for (int i = 0; i < buttons.Length; i++)
                GB.Box(g, "CueButton" + i, new Vector3(-0.3f + 0.3f * i, 1.005f, -0.152f), new Vector3(0.08f, 0.02f, 0.08f), buttons[i], new Vector3(15f, 0f, 0f), false);
            // the black-light cue lamp on its gooseneck
            GB.Cylinder(g, "CueLampNeck", new Vector3(0.65f, 0.98f, -0.15f), 0.012f, 0.36f, GB.Metal, false);
            GB.Box(g, "CueLampHood", new Vector3(0.65f, 1.19f, -0.05f), new Vector3(0.12f, 0.06f, 0.36f), GB.MatteBlack, collider: false);
            GB.Box(g, "CueLampTube", new Vector3(0.65f, 1.155f, -0.05f), new Vector3(0.04f, 0.025f, 0.3f), uvGlow, collider: false);
            GB.PointLight(g, "CueLampUV", new Vector3(0.65f, 1.05f, 0.05f), UV, 3f, 0.8f);
            GB.Box(g, "PromptScript", new Vector3(-0.2f, 0.815f, 0.15f), new Vector3(0.32f, 0.03f, 0.24f), card, collider: false);
        }

        // ------------------------------------------------------------------ main hall (|x| < 6.4, z 7..15.6)

        static void MainHall(Transform root)
        {
            var hall = GB.Group(root, "MainHall");
            GiantEar(hall);

            // whisper-dish pair B faces across the hall just in front of the stage, backed onto the partitions (no nook behind)
            Dish(hall, "WhisperDish_B_West", new Vector3(-5.75f, 0f, 13.6f), 90f);
            Dish(hall, "WhisperDish_B_East", new Vector3(5.75f, 0f, 13.6f), -90f);
            GlowTapeLine(hall, "WhisperLine_B", -3.8f, 3.8f, 13.6f);

            LabBench(hall, "OscilloscopeBench", new Vector3(-4.2f, 0f, 9.1f), 90f, 2.2f, true);   // x -4.6..-3.8, z 8..10.2
            SpecimenCabinet(hall, "SpecimenCupboard_Hall", new Vector3(6.1f, 0f, 11.2f), -90f, 1.2f, true);
            FloorVent(hall, "FloorVent_Hall", new Vector3(2.8f, 0f, 12.4f), 0f);
        }

        /// Walk-round exhibit in the middle of the hall: a giant painted ear on a round plinth (radius 1.6) with at least
        /// 2.2 m of floor all round it. This is the loop the player circles to break Tapwell's line of sight. The plinth is
        /// 0.8 m tall, above the agent's 0.75 m step, so Tapwell has to go round it as well.
        static void GiantEar(Transform parent)
        {
            var g = GB.Group(parent, "GiantEar", new Vector3(0f, 0f, 10.5f));
            GB.Cylinder(g, "Plinth", new Vector3(0f, 0.4f, 0f), 1.6f, 0.8f, plinth);
            GB.Cylinder(g, "PlinthFelt", new Vector3(0f, 0.82f, 0f), 1.5f, 0.04f, felt, false);
            for (int i = 0; i < 8; i++)
                GB.Sphere(g, "RimDot" + i, GB.Polar(i * 45f + 22.5f, 1.6f, 0.7f), 0.03f, lilacTape, false);
            // the ear faces the entrance (-Z). EarMount and EarCore are the colliders; the ellipsoids are visual only.
            GB.Cylinder(g, "EarMount", new Vector3(0f, 1.1f, 0f), 0.25f, 0.6f, GB.Metal);
            GB.Box(g, "EarCore", new Vector3(0f, 2.25f, 0f), new Vector3(1f, 1.9f, 0.3f), earPink);
            GB.Ellipsoid(g, "Helix", new Vector3(0f, 2.35f, 0f), new Vector3(2f, 3f, 0.7f), earPink, Vector3.zero, false);
            GB.Ellipsoid(g, "Concha", new Vector3(0.1f, 2.15f, -0.22f), new Vector3(1.2f, 1.7f, 0.4f), earDark, Vector3.zero, false);
            GB.Ellipsoid(g, "Lobe", new Vector3(-0.15f, 1.25f, -0.05f), new Vector3(0.8f, 0.75f, 0.45f), earPink, Vector3.zero, false);
            GB.Cylinder(g, "Canal", new Vector3(0.15f, 2.05f, -0.38f), 0.2f, 0.12f, GB.MatteBlack, new Vector3(90f, 0f, 0f), false);
            TapDot(g, "FingertipDot", new Vector3(0.15f, 2.05f, -0.46f), 0.05f);
            // "HEAR THE NIGHT" lectern with its listen button
            GB.Box(g, "Lectern", new Vector3(0f, 0.5f, -2.05f), new Vector3(0.5f, 1f, 0.3f), GB.Plywood);
            GB.Box(g, "LecternTop", new Vector3(0f, 1.04f, -2.1f), new Vector3(0.6f, 0.04f, 0.42f), card, new Vector3(-25f, 0f, 0f), false);
            TapDot(g, "ListenButton", new Vector3(0.18f, 1.045f, -2.2f), 0.035f);
            GB.PointLight(g, "EarGlow", new Vector3(0f, 2.1f, -1f), Lilac, 4.5f, 0.8f);
        }

        // ------------------------------------------------------------------ Tap-Along set (x -4..4, z 15.6..20)

        static void TapAlongSet(Transform root)
        {
            var set = GB.Group(root, "TapAlongSet");
            GB.Box(set, "StageDeck", new Vector3(0f, StageTop * 0.5f, 17.8f), new Vector3(8f, StageTop, 4.4f), stageDeck);
            GB.Box(set, "StageSkirt", new Vector3(0f, 0.2f, 15.585f), new Vector3(8f, 0.36f, 0.03f), felt, collider: false);
            GB.Box(set, "EdgeTape_Front", new Vector3(0f, StageTop + 0.005f, 15.66f), new Vector3(8f, 0.01f, 0.05f), lilacTape, collider: false);
            GB.Box(set, "EdgeTape_West", new Vector3(-3.97f, StageTop + 0.005f, 17.8f), new Vector3(0.05f, 0.01f, 4.4f), lilacTape, collider: false);
            GB.Box(set, "EdgeTape_East", new Vector3(3.97f, StageTop + 0.005f, 17.8f), new Vector3(0.05f, 0.01f, 4.4f), lilacTape, collider: false);
            GB.Stairs(set, "FrontSteps", new Vector3(0f, 0f, 14.55f), 0f, 3, 0.15f, 0.35f, 2.4f, stageDeck);    // z 14.55..15.6
            GB.Stairs(set, "SideSteps", new Vector3(-5.05f, 0f, 18.9f), 90f, 3, 0.15f, 0.35f, 1.4f, stageDeck);  // from the Store's back door

            // black velvet legs and border frame the set like the old studio
            GB.Box(set, "VelvetLeg_West", new Vector3(-3.75f, 2.65f, 15.85f), new Vector3(0.5f, 4.4f, 0.12f), GB.Velvet);
            GB.Box(set, "VelvetLeg_East", new Vector3(3.75f, 2.65f, 15.85f), new Vector3(0.5f, 4.4f, 0.12f), GB.Velvet);
            GB.Box(set, "VelvetBorder", new Vector3(0f, 4.6f, 15.8f), new Vector3(8f, 0.7f, 0.1f), GB.Velvet, collider: false);

            // chalkboard: "WHO'S AWAKE?", the Tap-Along rhythm, and an arrow to the mark of his fingertip
            GB.Box(set, "ChalkboardFrame", new Vector3(-2.1f, 2.25f, 19.975f), new Vector3(3.16f, 1.66f, 0.05f), GB.Wood, collider: false);
            GB.Box(set, "Chalkboard", new Vector3(-2.1f, 2.25f, 19.945f), new Vector3(3f, 1.5f, 0.03f), chalkboard, collider: false);
            GB.Box(set, "ChalkTray", new Vector3(-2.1f, 1.48f, 19.9f), new Vector3(3f, 0.04f, 0.1f), GB.Wood, collider: false);
            GB.Box(set, "Chalk_WhosAwake", new Vector3(-2.3f, 2.75f, 19.928f), new Vector3(2.2f, 0.06f, 0.004f), chalk, collider: false);
            float[] beats = { -3.2f, -2.55f, -2.3f, -1.65f };   // tap ... tap-tap ... tap
            for (int i = 0; i < beats.Length; i++)
                GB.Box(set, "Chalk_Beat" + i, new Vector3(beats[i], 2.35f, 19.928f), new Vector3(0.14f, 0.14f, 0.004f), chalk, collider: false);
            GB.Box(set, "Chalk_Arrow", new Vector3(-2.4f, 1.9f, 19.928f), new Vector3(1.2f, 0.04f, 0.004f), chalk, new Vector3(0f, 0f, 10f), false);
            TapDot(set, "Chalk_FingertipDot", new Vector3(-1.75f, 1.98f, 19.92f));
            // the pointer he picked "who's awake" with, leaning by the board
            GB.Box(set, "WhosAwakePointer", new Vector3(-0.45f, 1.04f, 19.85f), new Vector3(0.03f, 1.2f, 0.03f), GB.Wood, new Vector3(0f, 0f, -12f), false);
            TapDot(set, "PointerTip", new Vector3(-0.325f, 1.64f, 19.85f));

            // the Tap-Along rhythm board: knock pads, with lilac dots on the beats kids copied on their bedroom walls
            var rb = GB.Group(set, "RhythmBoard", new Vector3(2f, StageTop, 19.5f), 180f);
            GB.Box(rb, "Board", new Vector3(0f, 1.4f, 0f), new Vector3(3.2f, 1.9f, 0.08f), GB.Plywood);
            GB.Box(rb, "LegL", new Vector3(-1.45f, 0.225f, 0f), new Vector3(0.1f, 0.45f, 0.1f), GB.Wood);
            GB.Box(rb, "LegR", new Vector3(1.45f, 0.225f, 0f), new Vector3(0.1f, 0.45f, 0.1f), GB.Wood);
            GB.Box(rb, "FootL", new Vector3(-1.45f, 0.02f, 0f), new Vector3(0.12f, 0.04f, 0.5f), GB.Wood, collider: false);
            GB.Box(rb, "FootR", new Vector3(1.45f, 0.02f, 0f), new Vector3(0.12f, 0.04f, 0.5f), GB.Wood, collider: false);
            GB.Box(rb, "Banner_TapAlong", new Vector3(0f, 2.2f, 0.05f), new Vector3(2.6f, 0.2f, 0.02f), ochre, collider: false);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 5; c++)
                    GB.Box(rb, $"Pad{r}{c}", new Vector3(-1.2f + 0.6f * c, 0.8f + 0.45f * r, 0.07f), new Vector3(0.38f, 0.32f, 0.06f),
                        (r + c) % 2 == 0 ? GB.Wood : ochre, collider: false);
            int[,] beatPads = { { 2, 0 }, { 2, 2 }, { 2, 3 }, { 1, 4 }, { 0, 1 } };   // {row, column}
            for (int i = 0; i < beatPads.GetLength(0); i++)
                TapDot(rb, "BeatDot" + i, new Vector3(-1.2f + 0.6f * beatPads[i, 1], 0.8f + 0.45f * beatPads[i, 0], 0.115f), 0.035f);

            // Tapwell's mark: a lilac spike-tape X in front of the board (his GloamieSpawn)
            GB.Box(set, "TapwellMark_A", new Vector3(1.6f, StageTop + 0.006f, 17.9f), new Vector3(0.5f, 0.012f, 0.05f), lilacTape, 45f, false);
            GB.Box(set, "TapwellMark_B", new Vector3(1.6f, StageTop + 0.006f, 17.9f), new Vector3(0.5f, 0.012f, 0.05f), lilacTape, -45f, false);

            PropTrunk(set, "StageTrunk", new Vector3(3.2f, StageTop, 16.7f), -90f, true);   // x 2.78..3.63, z 16..17.4
            for (int i = 0; i < 6; i++)
                FoamPanel(set, "Foam_North" + i, new Vector3(-5f + 2f * i, 3.75f, 20f), 180f);
            GB.SpotLight(set, "StageWash", new Vector3(0f, 4.6f, 15.2f), new Vector3(55f, 0f, 0f), Lilac, 8f, 75f, 1.6f);
        }

        // ------------------------------------------------------------------ Specimen Store (x -11..-6.6, z 7..20)

        static void SpecimenStore(Transform root)
        {
            var west = GB.Group(root, "SpecimenStore");
            // specimen cases along the west wall, facing east into the room
            SpecimenCabinet(west, "SpecimenCase_1", new Vector3(-10.7f, 0f, 8.2f), 90f, 1f, false);
            SpecimenCabinet(west, "SpecimenCase_2", new Vector3(-10.7f, 0f, 9.3f), 90f, 1f, false);
            ToneGenerator(west, new Vector3(-10.65f, 0f, 14.4f), 90f);                                   // z 13.4..15.4
            SpecimenCabinet(west, "SpecimenCupboard_ByHum", new Vector3(-10.7f, 0f, 16.2f), 90f, 1.2f, true); // z 15.6..16.8
            SpecimenCabinet(west, "SpecimenCase_3", new Vector3(-10.7f, 0f, 17.5f), 90f, 1f, false);
            GB.Box(west, "SampleCrate", new Vector3(-10.5f, 0.4f, 19.4f), new Vector3(0.6f, 0.8f, 0.6f), GB.Plywood);

            // dissection bench 1.3 m off the partition (x -8.7..-7.9, z 12.3..15.3), its open back facing the gap:
            // crawl in from there and hide behind the front panel
            LabBench(west, "DissectionBench", new Vector3(-8.3f, 0f, 13.8f), -90f, 3f, true);
            // story prop: Tapwell's seal kit, for the spots he closes for the night
            GB.Cylinder(west, "SealKit_GafferRoll", new Vector3(-8.45f, 0.97f, 15.05f), 0.07f, 0.05f, gaffer, false);
            GB.Box(west, "SealKit_CardStack", new Vector3(-8.15f, 0.95f, 14.98f), new Vector3(0.15f, 0.02f, 0.1f), card, collider: false);
            GB.Box(west, "SealKit_Card", new Vector3(-8.1f, 1.01f, 15.18f), new Vector3(0.16f, 0.1f, 0.005f), card, new Vector3(-15f, 0f, 0f), false);

            WallLamp(west, "RechargeLamp_Store", new Vector3(-11f, 0f, 11.5f), 90f);
            float[] foamZ = { 8.75f, 11.5f, 14.4f, 17.5f };
            for (int i = 0; i < foamZ.Length; i++)
                FoamPanel(west, "Foam_West" + i, new Vector3(-11f, 3.1f, foamZ[i]), 90f);
            // a trail of fingertip marks at head height where he has tapped his way along the wall
            float[] tapZ = { 10.25f, 10.8f, 12.55f, 13.1f };
            for (int i = 0; i < tapZ.Length; i++)
                TapDot(west, "TapMark" + i, new Vector3(-10.98f, 1.75f + 0.05f * (i % 2), tapZ[i]));
            GB.PointLight(west, "SpecimenGlow", new Vector3(-9.9f, 1.5f, 8.75f), new Color(0.85f, 0.6f, 1f), 4f, 0.7f);
        }

        /// Bank of oscillator cabinets that still drones. A hiding spot beside it costs Tapwell one probe.
        static void ToneGenerator(Transform parent, Vector3 floorPos, float yaw)
        {
            var g = GB.Group(parent, "ToneGenerator_Humming", floorPos, yaw);
            GB.Box(g, "Rack", new Vector3(0f, 0.95f, 0f), new Vector3(2f, 1.9f, 0.7f), rackGrey);
            for (int i = 0; i < 3; i++)
            {
                float x = -0.66f + 0.66f * i;
                GB.Box(g, "Panel" + i, new Vector3(x, 1.25f, 0.36f), new Vector3(0.6f, 0.9f, 0.02f), GB.MatteBlack, collider: false);
                GB.Box(g, "Meter" + i, new Vector3(x, 1.12f, 0.375f), new Vector3(0.3f, 0.14f, 0.01f), scopeGlow, collider: false);
                GB.Sphere(g, "DialA" + i, new Vector3(x - 0.15f, 1.45f, 0.38f), 0.04f, amberDial, false);
                GB.Sphere(g, "DialB" + i, new Vector3(x + 0.15f, 1.45f, 0.38f), 0.04f, amberDial, false);
            }
            GB.Box(g, "VentGrille", new Vector3(0f, 0.35f, 0.36f), new Vector3(1.8f, 0.3f, 0.02f), GB.MatteBlack, collider: false);
            GB.Cylinder(g, "CableRun", new Vector3(0.6f, 3f, -0.25f), 0.05f, 2.2f, GB.MatteBlack, false);
        }

        // ------------------------------------------------------------------ Wardrobe Bay (x 6.6..11, z 7..20)

        static void WardrobeBay(Transform root)
        {
            var east = GB.Group(root, "WardrobeBay");
            // costume racks on the east wall (x 10.15..10.75): lab coats, tweed, spare charcoal fur. Hide between the costumes.
            CostumeRack(east, "CostumeRack_Hide", new Vector3(10.45f, 0f, 10.4f), -90f, 2f, new[] { labCoat, ochre, furCharcoal, felt, labCoat }, true);
            CostumeRack(east, "CostumeRack_Spares", new Vector3(10.45f, 0f, 12.9f), -90f, 2f, new[] { furCharcoal, labCoat, ochre, furCharcoal }, false);
            TapDot(east, "TapMark_Rack", new Vector3(10.17f, 1.6f, 10.05f));
            PropTrunk(east, "PropTrunk_Wardrobe", new Vector3(7.05f, 0f, 12.6f), 90f, true);   // x 6.63..7.48, z 11.9..13.3

            // prop table (x 6.6..7.4, z 14.2..15.6, top 0.85) with spare dish ears; a reel sits on it
            GB.Box(east, "PropTable_Top", new Vector3(7f, 0.82f, 14.9f), new Vector3(0.8f, 0.06f, 1.4f), GB.Plywood);
            GB.Box(east, "PropTable_TrestleA", new Vector3(7f, 0.395f, 14.35f), new Vector3(0.7f, 0.79f, 0.06f), GB.Plywood);
            GB.Box(east, "PropTable_TrestleB", new Vector3(7f, 0.395f, 15.45f), new Vector3(0.7f, 0.79f, 0.06f), GB.Plywood);
            GB.Ellipsoid(east, "SpareEar_L", new Vector3(6.85f, 0.9f, 14.45f), new Vector3(0.45f, 0.1f, 0.58f), earPink, new Vector3(0f, 20f, 0f), false);
            GB.Ellipsoid(east, "SpareEar_R", new Vector3(6.95f, 0.96f, 14.75f), new Vector3(0.45f, 0.1f, 0.58f), earPink, new Vector3(10f, -25f, 0f), false);

            BreakerPanel(east, new Vector3(11f, 0f, 15.2f), -90f);
            TappingWall(east, new Vector3(11f, 0f, 18.2f), -90f);
            WallLamp(east, "RechargeLamp_Wardrobe", new Vector3(8.4f, 0f, 20f), 180f);
            FloorVent(east, "FloorVent_Wardrobe", new Vector3(8.8f, 0f, 15.8f), 90f);
            float[] foamZ = { 10.4f, 12.9f, 17.2f };
            for (int i = 0; i < foamZ.Length; i++)
                FoamPanel(east, "Foam_East" + i, new Vector3(11f, 3.1f, foamZ[i]), -90f);
            GB.PointLight(east, "WardrobeGlow", new Vector3(9f, 3f, 11.6f), new Color(0.7f, 0.62f, 1f), 5f, 0.6f);
        }

        /// The wing's breaker. Restoring it is a loud 10-second crescendo, so it sits near both Wardrobe doors and
        /// the crawl hole, with the costume rack and the trunk as places to hide afterwards.
        static void BreakerPanel(Transform parent, Vector3 wallFloorPoint, float yaw)
        {
            var g = GB.Group(parent, "BreakerPanel", wallFloorPoint, yaw);
            GB.Box(g, "Cabinet", new Vector3(0f, 1.6f, 0.08f), new Vector3(0.7f, 1f, 0.16f), rackGrey);
            GB.Box(g, "Door", new Vector3(0f, 1.6f, 0.165f), new Vector3(0.64f, 0.94f, 0.01f), lockerDoor, collider: false);
            GB.Box(g, "MainLever", new Vector3(0.24f, 1.55f, 0.2f), new Vector3(0.05f, 0.3f, 0.06f), leverRed, collider: false);
            GB.Box(g, "FuseSocket", new Vector3(-0.15f, 1.35f, 0.175f), new Vector3(0.14f, 0.2f, 0.01f), GB.MatteBlack, collider: false);
            GB.Sphere(g, "PilotLamp", new Vector3(-0.22f, 1.95f, 0.18f), 0.03f, redCue, false);
            GB.Box(g, "HazardStripe", new Vector3(0f, 2.2f, 0.015f), new Vector3(0.9f, 0.08f, 0.03f), ochre, collider: false);
            GB.Cylinder(g, "Conduit", new Vector3(0f, 3.5f, 0.05f), 0.04f, 2.8f, GB.Metal, false);
        }

        /// Story prop: the patch of studio wall he tapped on in the late reels, where something inside tapped back.
        static void TappingWall(Transform parent, Vector3 wallFloorPoint, float yaw)
        {
            var g = GB.Group(parent, "TappingWall", wallFloorPoint, yaw);
            GB.Box(g, "KnuckleWornPatch", new Vector3(0f, 1.6f, 0.01f), new Vector3(0.7f, 0.7f, 0.02f), dishBowl, collider: false);
            GB.Box(g, "ChalkRing_Top", new Vector3(0f, 2.05f, 0.022f), new Vector3(1f, 0.03f, 0.005f), chalk, collider: false);
            GB.Box(g, "ChalkRing_Bottom", new Vector3(0f, 1.15f, 0.022f), new Vector3(1f, 0.03f, 0.005f), chalk, collider: false);
            GB.Box(g, "ChalkRing_Left", new Vector3(-0.5f, 1.6f, 0.022f), new Vector3(0.03f, 0.9f, 0.005f), chalk, collider: false);
            GB.Box(g, "ChalkRing_Right", new Vector3(0.5f, 1.6f, 0.022f), new Vector3(0.03f, 0.9f, 0.005f), chalk, collider: false);
            GB.Cylinder(g, "StethoscopeBell", new Vector3(0.1f, 1.55f, 0.04f), 0.05f, 0.04f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Box(g, "StethoscopeTube", new Vector3(0.2f, 1.25f, 0.035f), new Vector3(0.02f, 0.6f, 0.02f), GB.MatteBlack, new Vector3(0f, 0f, 15f), false);
            GB.Box(g, "GafferStrip", new Vector3(0.1f, 1.62f, 0.065f), new Vector3(0.22f, 0.05f, 0.01f), gaffer, collider: false);
            TapDot(g, "AnswerDot", new Vector3(-0.15f, 1.78f, 0.03f), 0.03f);
        }

        // ------------------------------------------------------------------ gameplay markers

        static void Markers(Transform root)
        {
            var m = GB.Group(root, "Markers");

            GB.Marker(m, MarkerKind.GloamieSpawn, new Vector3(1.6f, StageTop, 17.9f), 180f,
                "Dr. Tapwell: his spike-tape X on the Tap-Along stage, in front of the rhythm board");

            // patrol nodes on every route: front of hall, Specimen Store, whisper line B, stage, Wardrobe Bay
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(-5f, 0f, 5.4f), 0f, "Front of hall, west, on whisper line A");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(5f, 0f, 5.4f), 0f, "Front of hall, east, by the lockers");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(-8.8f, 0f, 9.2f), 0f, "Specimen Store, south end");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(-8.9f, 0f, 17.6f), 180f, "Specimen Store, north end");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(-2.4f, 0f, 14.1f), 90f, "Main hall, whisper line B in front of the stage");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(1f, StageTop, 17f), 180f, "On the Tap-Along stage");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(8.8f, 0f, 12.6f), 180f, "Wardrobe Bay, between the trunk and the racks");
            GB.Marker(m, MarkerKind.PatrolPoint, new Vector3(3.4f, 0f, 9f), 270f, "Main hall east, by the Wardrobe front door");

            GB.Marker(m, MarkerKind.ReelSpawn, new Vector3(-9.6f, 0f, 19.3f), 135f, "Reel on the Specimen Store floor beside the sample crate (dead-end corner)");
            GB.Marker(m, MarkerKind.ReelSpawn, new Vector3(7.25f, 0.85f, 15.25f), 90f, "Reel on the Wardrobe Bay prop table (top 0.85 m) by the spare ears");
            GB.Marker(m, MarkerKind.FuseSpawn, new Vector3(-3.95f, 0.9f, 8.6f), 90f, "Fuse on the oscilloscope bench (top 0.9 m), hall side");
            GB.Marker(m, MarkerKind.BatterySpawn, new Vector3(-4.85f, 0.8f, 1.75f), 180f, "Battery on the stage-manager desk (top 0.8 m)");
            GB.Marker(m, MarkerKind.BatterySpawn, new Vector3(9.6f, 0f, 8.4f), 0f, "Battery just inside the Wardrobe Bay crawl hole");

            GB.Marker(m, MarkerKind.Breaker, new Vector3(10.6f, 0f, 15.2f), -90f,
                "Wing breaker on the Wardrobe Bay east wall: 10 s crescendo. Exits: doors at z 9 and z 17, crawl hole at (8.6, 7)");
            GB.Marker(m, MarkerKind.CueLampDesk, new Vector3(-5.6f, 0f, 1.15f), 180f,
                "Stage-manager desk with the black-light cue lamp (Curtain Call); operator stands here facing north");

            // nine hiding spots in four zones, so the player always has a next choice as Tapwell seals them
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(5.2f, 0f, 0.275f), 0f, "Locker 2, front of hall; probes come through the door vents");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(6.8f, 0f, 0.275f), 0f, "Locker 4, front of hall (locker 3 beside it is already taped shut)");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(-4.2f, 0f, 9.1f), 90f, "Under the oscilloscope bench, behind its front panel (main hall)");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(6.1f, 0f, 11.2f), -90f, "Tall specimen cupboard on the main hall east wall");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(3.2f, StageTop, 16.7f), -90f, "Prop trunk on the Tap-Along stage, slats facing the stage");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(-8.3f, 0f, 13.8f), -90f, "Under the Specimen Store dissection bench, behind its front panel");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(-10.7f, 0f, 16.2f), 90f,
                "Specimen cupboard beside the humming tone generator: one probe fewer");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(7.05f, 0f, 12.6f), 90f, "Prop trunk against the Wardrobe Bay partition");
            GB.Marker(m, MarkerKind.HidingSpot, new Vector3(10.45f, 0f, 10.4f), -90f, "Between the hanging lab coats and tweed on the costume rack");

            GB.Marker(m, MarkerKind.RechargeLamp, new Vector3(-10.6f, 0f, 11.5f), 90f, "Amber wall lamp on a bracket at 2.2 m (Specimen Store)");
            GB.Marker(m, MarkerKind.RechargeLamp, new Vector3(8.4f, 0f, 19.6f), 180f,
                "Amber wall lamp at 2.2 m in the Wardrobe Bay dead end: suggested slot for Deepwick's Act IV impostor");

            GB.Marker(m, MarkerKind.Hatch, new Vector3(3.4f, 0f, 4.9f), 0f, "Floor vent grille, front of hall (wedge to quiet the desk and lockers)");
            GB.Marker(m, MarkerKind.Hatch, new Vector3(2.8f, 0f, 12.4f), 0f, "Floor vent grille beside the Giant Ear loop");
            GB.Marker(m, MarkerKind.Hatch, new Vector3(8.8f, 0f, 15.8f), 90f, "Floor vent grille by the breaker; wedge it before the crescendo");

            GB.Marker(m, MarkerKind.Handprint, new Vector3(-6.5f, 0f, 11f), 90f, "Crawl hole 1.0 x 1.0 m: Specimen Store to main hall");
            GB.Marker(m, MarkerKind.Handprint, new Vector3(8.6f, 0f, 7f), 0f, "Crawl hole 1.0 x 1.0 m: front of hall to Wardrobe Bay");

            GB.Marker(m, MarkerKind.StoryProp, new Vector3(-7.15f, 0.9f, 15.05f), -90f,
                "Tapwell's seal kit: silver gaffer roll and DO NOT DISTURB - EXPERIMENT IN PROGRESS cards");
            GB.Marker(m, MarkerKind.StoryProp, new Vector3(10.6f, 0f, 18.2f), -90f,
                "The tapping wall: chalk rings and a taped stethoscope where, on the late reels, something tapped back");
            GB.Marker(m, MarkerKind.StoryProp, new Vector3(-2.1f, StageTop, 19.5f), 180f,
                "Chalkboard: WHO'S AWAKE?, the Tap-Along rhythm, and the pointer he picked children with");
        }

        // ------------------------------------------------------------------ prop helpers (each builds in its own yawed group)

        /// Whisper dish on a stand, facing local +Z. A flattened ellipsoid gives the convex back; the flat face disc is
        /// the collider, because an ellipsoid's sphere collider would be a 2 m ball. The listening ring sits at the focus.
        static void Dish(Transform parent, string name, Vector3 floorPos, float yaw)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            const float cy = 1.75f;   // dish centre height
            GB.Cylinder(g, "Base", new Vector3(0f, 0.04f, -0.15f), 0.5f, 0.08f, GB.Metal);
            GB.Cylinder(g, "Post", new Vector3(0f, 0.65f, -0.15f), 0.06f, 1.14f, GB.Metal);
            GB.Box(g, "Yoke", new Vector3(0f, 1.25f, -0.22f), new Vector3(0.14f, 0.14f, 0.3f), GB.Metal);
            GB.Ellipsoid(g, "Shell", new Vector3(0f, cy, -0.2f), new Vector3(2.1f, 2.1f, 0.42f), dishPaint, Vector3.zero, false);
            GB.Cylinder(g, "Face", new Vector3(0f, cy, 0f), 1f, 0.06f, dishPaint, new Vector3(90f, 0f, 0f));
            GB.Cylinder(g, "Bowl", new Vector3(0f, cy, 0.04f), 0.82f, 0.02f, dishBowl, new Vector3(90f, 0f, 0f), false);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                GB.Sphere(g, "RimDot" + i, new Vector3(Mathf.Sin(a) * 0.91f, cy + Mathf.Cos(a) * 0.91f, 0.05f), 0.035f, lilacGlow, false);
            }
            // the listening ring at the focus, with its WHISPER HERE plaque
            GB.Cylinder(g, "FocusFoot", new Vector3(0f, 0.015f, 0.95f), 0.14f, 0.03f, GB.Metal, false);
            GB.Cylinder(g, "FocusPole", new Vector3(0f, 0.78f, 0.95f), 0.015f, 1.5f, GB.Metal, false);
            GB.Cylinder(g, "FocusRing", new Vector3(0f, 1.6f, 0.95f), 0.09f, 0.015f, lilacTape, new Vector3(90f, 0f, 0f), false);
            GB.Box(g, "Plaque", new Vector3(0f, 1.15f, 0.97f), new Vector3(0.28f, 0.18f, 0.02f), card, collider: false);
        }

        /// Dashed lilac glow tape on the floor along x at depth z: the whisper line between a pair of dishes.
        static void GlowTapeLine(Transform parent, string name, float x0, float x1, float z)
        {
            var g = GB.Group(parent, name);
            int n = Mathf.Max(1, Mathf.RoundToInt((x1 - x0) / 1.4f));
            for (int i = 0; i <= n; i++)
                GB.Box(g, "Dash" + i, new Vector3(Mathf.Lerp(x0, x1, (float)i / n), 0.005f, z), new Vector3(0.8f, 0.01f, 0.06f), lilacTape, collider: false);
        }

        /// Steel locker, 0.78 x 0.55 x 2.0 m, door facing local +Z. A hiding locker has a lilac fingertip mark by its
        /// vents. A sealed one wears his gaffer-tape X and DO NOT DISTURB card.
        static void Locker(Transform parent, string name, Vector3 floorPos, float yaw, bool hidingSpot, bool sealedShut)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            GB.Box(g, "Body", new Vector3(0f, 1f, 0f), new Vector3(0.78f, 2f, 0.55f), lockerPaint);
            GB.Box(g, "Door", new Vector3(0f, 1f, 0.285f), new Vector3(0.7f, 1.86f, 0.02f), lockerDoor, collider: false);
            for (int i = 0; i < 3; i++)
                GB.Box(g, "Vent" + i, new Vector3(0f, 1.72f - 0.07f * i, 0.3f), new Vector3(0.42f, 0.025f, 0.012f), GB.MatteBlack, collider: false);
            GB.Box(g, "Handle", new Vector3(0.26f, 1.05f, 0.31f), new Vector3(0.03f, 0.16f, 0.03f), GB.Metal, collider: false);
            if (hidingSpot) TapDot(g, "TapMark", new Vector3(-0.24f, 1.62f, 0.305f));
            if (sealedShut)
            {
                GB.Box(g, "GafferX_A", new Vector3(0f, 1.1f, 0.305f), new Vector3(0.06f, 1.2f, 0.012f), gaffer, new Vector3(0f, 0f, 30f), false);
                GB.Box(g, "GafferX_B", new Vector3(0f, 1.1f, 0.305f), new Vector3(0.06f, 1.2f, 0.012f), gaffer, new Vector3(0f, 0f, -30f), false);
                GB.Box(g, "DoNotDisturbCard", new Vector3(0f, 1.35f, 0.315f), new Vector3(0.24f, 0.14f, 0.008f), card, collider: false);
            }
        }

        /// Specimen cabinet facing local +Z, 0.6 m deep, 2.3 m tall. As a hiding spot it is a tall double-door cupboard
        /// with louvres the long finger can come through. Otherwise it is a glass display case of glowing jars.
        static void SpecimenCabinet(Transform parent, string name, Vector3 floorPos, float yaw, float width, bool hidingSpot)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            float hw = width * 0.5f;
            if (hidingSpot)
            {
                GB.Box(g, "Body", new Vector3(0f, 1.1f, 0f), new Vector3(width, 2.2f, 0.6f), cabinetWood);
                GB.Box(g, "DoorL", new Vector3(-hw * 0.5f, 1.1f, 0.31f), new Vector3(hw - 0.04f, 2.04f, 0.02f), cabinetDoor, collider: false);
                GB.Box(g, "DoorR", new Vector3(hw * 0.5f, 1.1f, 0.31f), new Vector3(hw - 0.04f, 2.04f, 0.02f), cabinetDoor, collider: false);
                float[] louvreY = { 1.78f, 1.7f, 1.62f, 0.42f, 0.34f };
                for (int i = 0; i < louvreY.Length; i++)
                    GB.Box(g, "Louvre" + i, new Vector3(0f, louvreY[i], 0.325f), new Vector3(width - 0.24f, 0.025f, 0.012f), GB.MatteBlack, collider: false);
                GB.Box(g, "HandleL", new Vector3(-0.05f, 1.1f, 0.33f), new Vector3(0.025f, 0.14f, 0.025f), brass, collider: false);
                GB.Box(g, "HandleR", new Vector3(0.05f, 1.1f, 0.33f), new Vector3(0.025f, 0.14f, 0.025f), brass, collider: false);
                GB.Cylinder(g, "TopJarA", new Vector3(-hw * 0.5f, 2.32f, 0f), 0.08f, 0.24f, jarGlow, false);
                GB.Cylinder(g, "TopJarB", new Vector3(hw * 0.4f, 2.3f, 0f), 0.07f, 0.2f, jarCloudy, false);
                TapDot(g, "TapMark", new Vector3(hw * 0.5f, 1.86f, 0.33f));
                return;
            }
            GB.Box(g, "Base", new Vector3(0f, 0.45f, 0f), new Vector3(width, 0.9f, 0.6f), cabinetWood);
            GB.Box(g, "Back", new Vector3(0f, 1.55f, -0.27f), new Vector3(width, 1.3f, 0.06f), cabinetWood);
            GB.Box(g, "Top", new Vector3(0f, 2.25f, 0f), new Vector3(width + 0.04f, 0.1f, 0.62f), cabinetWood);
            GB.Box(g, "SideL", new Vector3(-hw + 0.03f, 1.55f, 0f), new Vector3(0.06f, 1.3f, 0.6f), cabinetWood);
            GB.Box(g, "SideR", new Vector3(hw - 0.03f, 1.55f, 0f), new Vector3(0.06f, 1.3f, 0.6f), cabinetWood);
            GB.Box(g, "Glass", new Vector3(0f, 1.55f, 0.29f), new Vector3(width - 0.08f, 1.3f, 0.015f), GB.Glass, collider: false);
            GB.Box(g, "Shelf", new Vector3(0f, 1.5f, -0.02f), new Vector3(width - 0.08f, 0.025f, 0.48f), cabinetWood, collider: false);
            GB.Cylinder(g, "Jar0", new Vector3(-hw * 0.45f, 1.03f, 0f), 0.09f, 0.26f, jarGlow, false);
            GB.Cylinder(g, "Jar1", new Vector3(hw * 0.35f, 1f, 0.02f), 0.07f, 0.2f, jarCloudy, false);
            GB.Cylinder(g, "Jar2", new Vector3(-hw * 0.2f, 1.625f, 0f), 0.07f, 0.22f, jarCloudy, false);
            GB.Cylinder(g, "Jar3", new Vector3(hw * 0.45f, 1.605f, 0f), 0.06f, 0.18f, jarGlow, false);
        }

        /// Lab bench along local X: black resin top at 0.9 m, front panel on local +Z. As a hiding spot the player tucks
        /// in under the top behind the panel, and the finger probes through the gap below it.
        static void LabBench(Transform parent, string name, Vector3 floorPos, float yaw, float length, bool hidingSpot)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            float hl = length * 0.5f;
            // knee space 0.88 m high, so you can lie down (Z) and crawl under it from the open back
            GB.Box(g, "Top", new Vector3(0f, 0.91f, 0f), new Vector3(length, 0.06f, 0.8f), benchTop);
            GB.Box(g, "EndL", new Vector3(-hl + 0.05f, 0.44f, 0f), new Vector3(0.1f, 0.88f, 0.74f), cabinetWood);
            GB.Box(g, "EndR", new Vector3(hl - 0.05f, 0.44f, 0f), new Vector3(0.1f, 0.88f, 0.74f), cabinetWood);
            GB.Box(g, "FrontPanel", new Vector3(0f, 0.53f, 0.33f), new Vector3(length - 0.2f, 0.6f, 0.06f), cabinetWood);
            GB.Box(g, "Oscilloscope", new Vector3(-hl + 0.5f, 1.11f, -0.1f), new Vector3(0.42f, 0.34f, 0.4f), rackGrey, collider: false);
            GB.Box(g, "ScopeScreen", new Vector3(-hl + 0.5f, 1.13f, 0.105f), new Vector3(0.22f, 0.16f, 0.01f), scopeGlow, collider: false);
            GB.Box(g, "ResonanceBox", new Vector3(hl - 0.55f, 1.01f, 0f), new Vector3(0.3f, 0.14f, 0.16f), GB.Wood, collider: false);
            GB.Box(g, "TuningFork", new Vector3(hl - 0.55f, 1.18f, 0f), new Vector3(0.05f, 0.2f, 0.015f), GB.Metal, collider: false);
            if (hidingSpot) TapDot(g, "TapMark", new Vector3(hl * 0.4f, 0.32f, 0.375f));
        }

        /// Strapped prop trunk, 1.4 x 0.85 x 0.8 m, with its slats (where the finger comes in) facing local +Z.
        static void PropTrunk(Transform parent, string name, Vector3 floorPos, float yaw, bool hidingSpot)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            GB.Box(g, "Body", new Vector3(0f, 0.36f, 0f), new Vector3(1.4f, 0.72f, 0.85f), trunkCanvas);
            GB.Box(g, "Lid", new Vector3(0f, 0.76f, 0f), new Vector3(1.44f, 0.08f, 0.89f), strap);
            GB.Box(g, "StrapL", new Vector3(-0.45f, 0.41f, 0f), new Vector3(0.07f, 0.82f, 0.9f), strap, collider: false);
            GB.Box(g, "StrapR", new Vector3(0.45f, 0.41f, 0f), new Vector3(0.07f, 0.82f, 0.9f), strap, collider: false);
            GB.Box(g, "SlatHigh", new Vector3(0f, 0.55f, 0.428f), new Vector3(1.1f, 0.025f, 0.01f), GB.MatteBlack, collider: false);
            GB.Box(g, "SlatLow", new Vector3(0f, 0.3f, 0.428f), new Vector3(1.1f, 0.025f, 0.01f), GB.MatteBlack, collider: false);
            GB.Box(g, "Stencil_GloamingGlenProps", new Vector3(0f, 0.43f, 0.43f), new Vector3(0.5f, 0.08f, 0.005f), ochre, collider: false);
            if (hidingSpot) TapDot(g, "TapMark", new Vector3(0.3f, 0.6f, 0.44f), 0.022f);
        }

        /// Costume rail along local X. The garments hang 0.6..1.76 m and are the solid part you bump into.
        static void CostumeRack(Transform parent, string name, Vector3 floorPos, float yaw, float length, Material[] garments, bool spareEars)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            float hl = length * 0.5f;
            GB.Box(g, "FootL", new Vector3(-hl, 0.025f, 0f), new Vector3(0.06f, 0.05f, 0.6f), GB.Metal, collider: false);
            GB.Box(g, "FootR", new Vector3(hl, 0.025f, 0f), new Vector3(0.06f, 0.05f, 0.6f), GB.Metal, collider: false);
            GB.Cylinder(g, "PostL", new Vector3(-hl, 0.95f, 0f), 0.02f, 1.8f, GB.Metal, false);
            GB.Cylinder(g, "PostR", new Vector3(hl, 0.95f, 0f), 0.02f, 1.8f, GB.Metal, false);
            GB.Cylinder(g, "Rail", new Vector3(0f, 1.83f, 0f), 0.018f, length, GB.Metal, new Vector3(0f, 0f, 90f), false);
            float step = (length - 0.2f) / garments.Length;
            for (int i = 0; i < garments.Length; i++)
            {
                float drop = i % 2 == 0 ? 1.15f : 1f;
                GB.Box(g, "Garment" + i, new Vector3(-hl + 0.1f + step * (i + 0.5f), 1.76f - drop * 0.5f, 0f), new Vector3(step * 0.85f, drop, 0.52f), garments[i]);
            }
            if (spareEars)   // a pair of spare latex ears hung over the rail
            {
                GB.Ellipsoid(g, "SpareEarL", new Vector3(-0.35f, 2.05f, 0f), new Vector3(0.42f, 0.55f, 0.08f), earPink, new Vector3(0f, 0f, 12f), false);
                GB.Ellipsoid(g, "SpareEarR", new Vector3(0.3f, 2.05f, 0f), new Vector3(0.42f, 0.55f, 0.08f), earPink, new Vector3(0f, 0f, -12f), false);
            }
        }

        /// Acoustic foam tile, 1 x 1 m. wallPoint is on the wall surface at the tile's centre; yaw faces out of the wall.
        static void FoamPanel(Transform parent, string name, Vector3 wallPoint, float yaw)
        {
            var g = GB.Group(parent, name, wallPoint, yaw);
            GB.Box(g, "Pad", new Vector3(0f, 0f, 0.03f), new Vector3(1f, 1f, 0.06f), foam, collider: false);
            GB.Box(g, "WedgeUpper", new Vector3(0f, 0.22f, 0.07f), new Vector3(0.96f, 0.14f, 0.14f), foamWedge, new Vector3(45f, 0f, 0f), false);
            GB.Box(g, "WedgeLower", new Vector3(0f, -0.22f, 0.07f), new Vector3(0.96f, 0.14f, 0.14f), foamWedge, new Vector3(45f, 0f, 0f), false);
        }

        /// Flush floor vent grille (1 x 0.6 m, no collider) with Digsley's red cue lamp beside it. Its gaps carry sound below.
        static void FloorVent(Transform parent, string name, Vector3 floorPos, float yaw)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            GB.Box(g, "Grille", new Vector3(0f, 0.01f, 0f), new Vector3(1f, 0.02f, 0.6f), GB.Metal, collider: false);
            for (int i = 0; i < 4; i++)
                GB.Box(g, "Slot" + i, new Vector3(0f, 0.016f, -0.18f + 0.12f * i), new Vector3(0.84f, 0.012f, 0.06f), GB.MatteBlack, collider: false);
            GB.Box(g, "CueLampHousing", new Vector3(0.62f, 0.03f, 0f), new Vector3(0.1f, 0.06f, 0.1f), GB.MatteBlack, collider: false);
            GB.Sphere(g, "RedCueLamp", new Vector3(0.62f, 0.08f, 0f), 0.035f, redCue, false);
        }

        /// Plywood frame round a 1.6 x 2.4 m door. floorPos is the doorway's floor centre; local X runs along the wall.
        /// The black plate on the hall side (tallySide +1 = local +Z, -1 = local -Z) is where Tapwell's UV-only chalk
        /// tally shows once he has memorised the room.
        static void DoorFrame(Transform parent, string name, Vector3 floorPos, float yaw, float tallySide)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            // 1 cm proud of the opening, so the frame hides the partition's cut ends instead of flickering against them
            float px = DoorWidth * 0.5f + 0.05f;
            GB.Box(g, "PostL", new Vector3(-px, DoorTop * 0.5f, 0f), new Vector3(0.12f, DoorTop, 0.3f), GB.Plywood);
            GB.Box(g, "PostR", new Vector3(px, DoorTop * 0.5f, 0f), new Vector3(0.12f, DoorTop, 0.3f), GB.Plywood);
            GB.Box(g, "Header", new Vector3(0f, DoorTop + 0.05f, 0f), new Vector3(DoorWidth + 0.24f, 0.12f, 0.3f), GB.Plywood);
            GB.Box(g, "TallyPlate", new Vector3(-px, 1.65f, 0.155f * tallySide), new Vector3(0.1f, 0.5f, 0.01f), GB.MatteBlack, collider: false);
        }

        /// Plywood trim round a child-sized 1.0 x 1.0 m crawl hole (a Handprint shortcut).
        static void CrawlFrame(Transform parent, string name, Vector3 floorPos, float yaw)
        {
            var g = GB.Group(parent, name, floorPos, yaw);
            GB.Box(g, "PostL", new Vector3(-0.53f, 0.5f, 0f), new Vector3(0.08f, 1f, 0.26f), GB.Plywood);
            GB.Box(g, "PostR", new Vector3(0.53f, 0.5f, 0f), new Vector3(0.08f, 1f, 0.26f), GB.Plywood);
            GB.Box(g, "Header", new Vector3(0f, 1.03f, 0f), new Vector3(1.16f, 0.08f, 0.26f), GB.Plywood);
        }

        /// Recharge lamp: an amber bulb on a brass bracket at 2.2 m, steady and silent (Deepwick's fake hangs at 1.7 m
        /// and sways). wallFloorPoint is on the floor at the wall face; yaw faces out of the wall.
        static void WallLamp(Transform parent, string name, Vector3 wallFloorPoint, float yaw)
        {
            var g = GB.Group(parent, name, wallFloorPoint, yaw);
            GB.Box(g, "Backplate", new Vector3(0f, 2.2f, 0.02f), new Vector3(0.2f, 0.32f, 0.04f), brass, collider: false);
            GB.Box(g, "Bracket", new Vector3(0f, 2.3f, 0.16f), new Vector3(0.04f, 0.04f, 0.28f), brass, collider: false);
            GB.Box(g, "Hood", new Vector3(0f, 2.33f, 0.3f), new Vector3(0.22f, 0.04f, 0.22f), brass, collider: false);
            GB.GlowLamp(g, "Lamp", new Vector3(0f, 2.2f, 0.3f), LampAmber, 0.09f, 4f, 1f);
        }

        /// One lilac fingertip mark: Tapwell's glow, left where he has tapped or probed.
        static void TapDot(Transform parent, string name, Vector3 position, float radius = 0.025f)
            => GB.Sphere(parent, name, position, radius, lilacGlow, false);
    }
}
