using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Act I "Lamp-Lighting": Miss Hesper's Lamp-Post Meadow. A black-light night meadow built in an old exhibit hall.
    /// Local frame: root at the entrance doorway, +Z into the wing, interior |x| <= 11, 0 <= z <= 20, 8 m tall.
    ///  - Audience floor (z 0..8.5): 16 mm camera on a dolly track, stage-manager desk, felt hollow log, street lamp.
    ///  - Stage deck (x -7.5..6.5, z 8.5..15.5, top 0.6) with Hesper's lamp-post hill (top 1.2) in the middle,
    ///    reached by front steps, rear steps and an east ramp. Its west edge is a raised knoll (top 1.25) with a
    ///    0.95 m child-sized crawl gap underneath that runs from the audience floor to backstage.
    ///  - East walkway (x 6.5..11) and a backstage strip (z 15.5..20) close the loop around the set.
    /// Hesper's rule: light draws her and her back-facing eyespots see only lit lights, so the space offers lamps to
    /// flip on as decoys, grass flats and velvet legs that break line of sight, and a loop around her lamp hill.
    public static class MeadowWing
    {
        const float DeckTop = 0.6f;
        const float HillTop = 1.2f;
        const float KnollTop = 1.25f;
        const float KnollUnder = 0.95f;

        static readonly Color Mint = new Color(0.55f, 1f, 0.78f);
        static readonly Color MintWhite = new Color(0.8f, 1f, 0.9f);
        static readonly Color WarmBulb = new Color(1f, 0.78f, 0.5f);
        static readonly Color Amber = new Color(1f, 0.62f, 0.28f);
        static readonly Color CueViolet = new Color(0.55f, 0.3f, 1f);

        static Material hallFloor, hallWall, felt, feltDark, mintGlow, mintFaint, iron, pistachio, plum, brass;
        static Material petalLilac, petalCream, deadBulb, crtBody, crtScreen, trapLid, trapCueLens;

        public static void Build(Transform root)
        {
            Materials();
            Shell(root);
            AudienceFloor(root);
            StageDeck(root);
            HesperLampPost(root);
            Knoll(root);
            EastWalkway(root);
            Backstage(root);
            Overhead(root);
        }

        static void Materials()
        {
            hallFloor = GB.Mat("Meadow_HallFloor", new Color(0.085f, 0.085f, 0.095f), 0.2f);
            hallWall = GB.Mat("Meadow_HallWall", new Color(0.055f, 0.06f, 0.07f), 0.1f);
            felt = GB.Mat("Meadow_FeltGrass", new Color(0.12f, 0.22f, 0.13f), 0.05f);
            feltDark = GB.Mat("Meadow_FeltMoss", new Color(0.07f, 0.13f, 0.08f), 0.05f);
            mintGlow = GB.Glow("Meadow_MintTape", Mint, 2f);
            mintFaint = GB.Glow("Meadow_MintFaint", Mint, 0.9f);
            iron = GB.Mat("Meadow_LampIron", new Color(0.11f, 0.13f, 0.11f), 0.35f, 0.6f);
            pistachio = GB.Mat("Meadow_PistachioPaint", new Color(0.42f, 0.5f, 0.34f), 0.2f);
            plum = GB.Mat("Meadow_PlumTrim", new Color(0.28f, 0.1f, 0.22f), 0.15f);
            brass = GB.Mat("Meadow_Brass", new Color(0.5f, 0.4f, 0.16f), 0.5f, 0.7f);
            petalLilac = GB.Mat("Meadow_PetalLilac", new Color(0.5f, 0.42f, 0.58f), 0.1f);
            petalCream = GB.Mat("Meadow_PetalCream", new Color(0.68f, 0.64f, 0.48f), 0.1f);
            deadBulb = GB.Mat("Meadow_DeadBulb", new Color(0.26f, 0.25f, 0.2f), 0.7f);
            crtBody = GB.Mat("Meadow_CRTBody", new Color(0.16f, 0.15f, 0.13f), 0.3f);
            crtScreen = GB.Mat("Meadow_CRTScreen", new Color(0.03f, 0.045f, 0.045f), 0.9f);
            trapLid = GB.Mat("Meadow_TrapLid", new Color(0.09f, 0.08f, 0.07f), 0.2f);
            trapCueLens = GB.Mat("Meadow_TrapCueLens", new Color(0.35f, 0.04f, 0.03f), 0.8f);
        }

        // ---------------------------------------------------------------- hall shell, masking, moon and stars

        static readonly Vector2[] StarSpots =
        {
            new Vector2(-9.5f, 6.8f), new Vector2(-8.2f, 4.2f), new Vector2(-6.8f, 7.4f), new Vector2(-5.9f, 5.3f),
            new Vector2(-1.2f, 7.6f), new Vector2(0.6f, 7.5f), new Vector2(1.4f, 4.6f), new Vector2(2.9f, 6.6f),
            new Vector2(3.6f, 3.6f), new Vector2(4.3f, 5.0f), new Vector2(5.6f, 7.3f), new Vector2(7.0f, 4.4f),
            new Vector2(8.4f, 6.2f), new Vector2(9.6f, 7.6f), new Vector2(-9.8f, 3.4f), new Vector2(9.9f, 4.9f),
        };

        static void Shell(Transform root)
        {
            GB.Room(root, "Room", new Vector3(0f, 0f, 10f), new Vector3(22f, 8f, 20f), hallFloor, hallWall, GB.DarkConcrete, new Doorway(Side.South));

            // Black-velvet masking: a proscenium border over the deck front, and two legs that hide corners from the audience floor.
            GB.Box(root, "ProsceniumBorder", new Vector3(-0.5f, 6.1f, 8.25f), new Vector3(17f, 1.5f, 0.1f), GB.Velvet, 0f, false);
            GB.Box(root, "MaskingLeg_West", new Vector3(-10.3f, 3.5f, 7.3f), new Vector3(1.4f, 7f, 0.12f), GB.Velvet);
            GB.Box(root, "MaskingLeg_East", new Vector3(10.3f, 3.5f, 16f), new Vector3(1.4f, 7f, 0.12f), GB.Velvet);

            // Painted moon flat (a crescent: pale disc half-covered by a velvet disc) and black-light stars on the north wall.
            var moonPaint = GB.Glow("Meadow_MoonPaint", new Color(0.72f, 0.74f, 0.92f), 0.55f);
            GB.Cylinder(root, "MoonFlat", new Vector3(-2.5f, 5.2f, 19.9f), 1.7f, 0.08f, moonPaint, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(root, "MoonShadow", new Vector3(-1.85f, 5.45f, 19.84f), 1.45f, 0.04f, GB.Velvet, new Vector3(90f, 0f, 0f), false);
            var starPaint = GB.Glow("Meadow_StarPaint", new Color(0.82f, 0.86f, 1f), 1.2f);
            var stars = GB.Group(root, "Stars");
            for (int i = 0; i < StarSpots.Length; i++)
                GB.Sphere(stars, $"Star{i:00}", new Vector3(StarSpots[i].x, StarSpots[i].y, 19.95f), i % 3 == 0 ? 0.065f : 0.04f, starPaint, false);
        }

        // ---------------------------------------------------------------- audience floor (z 0..8.5)

        static void AudienceFloor(Transform root)
        {
            var g = GB.Group(root, "AudienceFloor");

            // 16 mm film camera on a dolly track running across the audience side, aimed at the set.
            var track = GB.Group(g, "DollyTrack", new Vector3(-4f, 0f, 5f));
            GB.Box(track, "RailS", new Vector3(0f, 0.025f, -0.4f), new Vector3(11.2f, 0.05f, 0.06f), GB.Metal, 0f, false);
            GB.Box(track, "RailN", new Vector3(0f, 0.025f, 0.4f), new Vector3(11.2f, 0.05f, 0.06f), GB.Metal, 0f, false);
            for (int i = 0; i < 14; i++)
                GB.Box(track, $"Tie{i:00}", new Vector3(-5.2f + 0.8f * i, 0.012f, 0f), new Vector3(0.12f, 0.024f, 1.1f), GB.Wood, 0f, false);
            GB.Box(track, "StopW", new Vector3(-5.65f, 0.06f, 0f), new Vector3(0.1f, 0.12f, 1f), GB.Metal, 0f, false);
            GB.Box(track, "StopE", new Vector3(5.65f, 0.06f, 0f), new Vector3(0.1f, 0.12f, 1f), GB.Metal, 0f, false);

            var camMat = GB.Mat("Meadow_CameraBody", new Color(0.08f, 0.08f, 0.09f), 0.4f, 0.3f);
            var dolly = GB.Group(g, "FilmCamera16mm", new Vector3(-4.5f, 0f, 5f));
            for (int i = 0; i < 4; i++)
                GB.Cylinder(dolly, $"Wheel{i}", new Vector3(i < 2 ? -0.42f : 0.42f, 0.13f, i % 2 == 0 ? -0.4f : 0.4f), 0.08f, 0.06f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Box(dolly, "Platform", new Vector3(0f, 0.27f, 0f), new Vector3(1.1f, 0.14f, 1.3f), camMat);
            GB.Cylinder(dolly, "Pedestal", new Vector3(0f, 0.79f, 0.1f), 0.11f, 0.9f, GB.Metal);
            GB.Box(dolly, "Body", new Vector3(0f, 1.4f, 0.15f), new Vector3(0.26f, 0.32f, 0.5f), camMat);
            GB.Cylinder(dolly, "Lens", new Vector3(0f, 1.38f, 0.52f), 0.07f, 0.24f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(dolly, "MagazineFront", new Vector3(0f, 1.72f, 0.3f), 0.16f, 0.09f, camMat, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(dolly, "MagazineBack", new Vector3(0f, 1.72f, 0f), 0.16f, 0.09f, camMat, new Vector3(0f, 0f, 90f), false);
            GB.Box(dolly, "Viewfinder", new Vector3(-0.17f, 1.45f, -0.15f), new Vector3(0.06f, 0.08f, 0.26f), camMat, 0f, false);
            GB.Box(dolly, "PushBar", new Vector3(0f, 1f, -0.6f), new Vector3(0.7f, 0.04f, 0.04f), GB.Metal, 0f, false);
            for (int s = -1; s <= 1; s += 2)
                GB.Box(dolly, $"PushBarPost{(s < 0 ? "L" : "R")}", new Vector3(0.33f * s, 0.67f, -0.6f), new Vector3(0.04f, 0.66f, 0.04f), GB.Metal, 0f, false);
            GB.Box(dolly, "Slate", new Vector3(0.3f, 0.35f, -0.42f), new Vector3(0.32f, 0.02f, 0.26f), GB.Mat("Meadow_SlateChalk", new Color(0.12f, 0.12f, 0.13f), 0.1f), 0f, false);
            GB.Marker(dolly, MarkerKind.StoryProp, new Vector3(0.3f, 0.34f, -0.42f), 180f,
                "Clapper slate on the camera dolly, chalked 'LONGEST NIGHT - LIVE - 21.12.97 - ACT I LAMP-LIGHTING'. The camera still points at Hesper's lamp.");

            // Felt hollow log: a crawl-in hiding spot on the open audience floor. The round log is only a skin
            // (sunk a little into the floor); what you bump into is an open box shell inside it, 0.9 m wide and
            // 0.92 m high, open at both ends, so you can lie down (Z) and crawl through.
            var logMat = GB.Mat("Meadow_LogFelt", new Color(0.2f, 0.14f, 0.09f), 0.05f);
            var log = GB.Group(g, "HollowLog", new Vector3(5.4f, 0f, 4.4f));
            GB.Cylinder(log, "Log", new Vector3(0f, 0.55f, 0f), 0.75f, 2.4f, logMat, new Vector3(0f, 0f, 90f), false);
            GB.Box(log, "Shell_N", new Vector3(0f, 0.46f, 0.5f), new Vector3(2.4f, 0.92f, 0.1f), logMat);
            GB.Box(log, "Shell_S", new Vector3(0f, 0.46f, -0.5f), new Vector3(2.4f, 0.92f, 0.1f), logMat);
            GB.Box(log, "Shell_Roof", new Vector3(0f, 0.97f, 0f), new Vector3(2.4f, 0.1f, 1.1f), logMat);
            GB.Cylinder(log, "HollowW", new Vector3(-1.21f, 0.5f, 0f), 0.45f, 0.03f, GB.Velvet, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(log, "HollowE", new Vector3(1.21f, 0.5f, 0f), 0.45f, 0.03f, GB.Velvet, new Vector3(0f, 0f, 90f), false);
            GB.Box(log, "MossTuft", new Vector3(0.3f, 1.32f, 0f), new Vector3(0.9f, 0.08f, 0.5f), feltDark, 0f, false);
            GB.Marker(log, MarkerKind.HidingSpot, Vector3.zero, 270f, "Inside the felt hollow log prop (crawl in from the west end)");

            BuildHatch(g, "FloorVent_Audience", new Vector3(2.8f, 0f, 6.8f), 0f, false,
                "Floor vent in front of the deck steps - footsteps on the audience floor carry down to Digsley (Act V)");
            BuildMonitorCart(g, "MonitorCart_Audience", new Vector3(-8.6f, 0f, 2.2f), 60f,
                "Field monitor by the entrance - from Act V it flickers on with Hesper's lamp-post set; kill it within 4 s");
            SetLamp(g, "SetLamp_StreetWest", new Vector3(-7.8f, 0f, 7.4f), 2.6f, 4.5f);
            BuildRechargeLamp(g, "RechargeLamp_West", new Vector3(-10.98f, 0f, 4f), 90f, "Warm wall lamp on a 2.2 m bracket, west wall near the entrance");
            BuildRechargeLamp(g, "RechargeLamp_East", new Vector3(10.98f, 0f, 7.4f), 270f, "Warm wall lamp on a 2.2 m bracket, east wall beside the prompt corner");
            StageManagerDesk(g);

            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(-9.3f, 0f, 7.9f), 0f, "Battery in the dark pocket behind the west masking leg, by the crawl-gap mouth");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-5f, 0f, 2.8f), 0f, "Audience floor west, behind the camera dolly");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(6.6f, 0f, 6.6f), 0f, "Audience floor east, between the hollow log and the deck");
        }

        /// Stage-manager prompt desk against the east wall, facing the set, with the black-light Curtain Call cue lamp.
        static void StageManagerDesk(Transform parent)
        {
            var g = GB.Group(parent, "StageManagerDesk", new Vector3(10.5f, 0f, 4.6f), 270f);
            // knee space 0.88 m high, so you can lie down (Z) and crawl under it to hide
            GB.Box(g, "Top", new Vector3(0f, 0.905f, 0f), new Vector3(1.6f, 0.05f, 0.8f), GB.Wood);
            GB.Box(g, "SideL", new Vector3(-0.77f, 0.44f, 0f), new Vector3(0.06f, 0.88f, 0.78f), GB.Wood);
            GB.Box(g, "SideR", new Vector3(0.77f, 0.44f, 0f), new Vector3(0.06f, 0.88f, 0.78f), GB.Wood);
            GB.Box(g, "ModestyPanel", new Vector3(0f, 0.47f, -0.36f), new Vector3(1.48f, 0.8f, 0.05f), GB.Wood);
            GB.Box(g, "PromptBook", new Vector3(-0.3f, 0.945f, 0.05f), new Vector3(0.32f, 0.04f, 0.42f), GB.Mat("Meadow_PaperCream", new Color(0.62f, 0.58f, 0.46f), 0.05f), 0f, false);
            GB.Box(g, "Intercom", new Vector3(0.15f, 0.985f, -0.28f), new Vector3(0.32f, 0.12f, 0.16f), GB.MatteBlack, 0f, false);
            GB.Box(g, "CueLampHood", new Vector3(0.6f, 1.005f, -0.25f), new Vector3(0.16f, 0.16f, 0.16f), GB.MatteBlack, 0f, false);
            GB.GlowLamp(g, "BlackLightCueLamp", new Vector3(0.6f, 1.135f, -0.25f), CueViolet, 0.06f, 3f, 0.8f);
            GB.Box(g, "CueButton", new Vector3(0.6f, 0.94f, 0.1f), new Vector3(0.08f, 0.03f, 0.08f), GB.Glow("Meadow_CueButton", CueViolet, 1.2f), 0f, false);
            GB.Cylinder(g, "Stool", new Vector3(0.6f, 0.25f, 1.0f), 0.2f, 0.5f, GB.MatteBlack); // pushed aside, clear of the knee space

            GB.Marker(g, MarkerKind.CueLampDesk, new Vector3(0f, 0f, 0.4f), 0f, "Stage-manager prompt desk with the black-light Curtain Call cue lamp");
            GB.Marker(g, MarkerKind.HidingSpot, new Vector3(0f, 0f, -0.05f), 0f, "Under the stage-manager desk, behind the modesty panel");
            GB.Marker(g, MarkerKind.StoryProp, new Vector3(-0.3f, 0.925f, 0.05f), 0f,
                "Prompt book open at the crew log: 'Hesper kept walking into the studio lamps again - always facing the wrong way.'");
        }

        // ---------------------------------------------------------------- stage deck (x -7.5..6.5, z 8.5..15.5, top 0.6)

        static void StageDeck(Transform root)
        {
            var g = GB.Group(root, "StageDeck");
            GB.Box(g, "Deck", new Vector3(-0.5f, DeckTop * 0.5f, 12f), new Vector3(14f, DeckTop, 7f), GB.MatteBlack);
            GB.Box(g, "LampHill", new Vector3(0f, (DeckTop + HillTop) * 0.5f, 12.9f), new Vector3(5f, HillTop - DeckTop, 2.2f), GB.MatteBlack);
            GB.Stairs(g, "HillSteps", new Vector3(0f, DeckTop, 11.1f), 0f, 2, 0.3f, 0.35f, 2f, GB.MatteBlack);

            // Three ways onto the deck: front steps from the audience, rear steps from backstage, a ramp from the east walkway.
            GB.Stairs(g, "FrontSteps", new Vector3(0f, 0f, 7.45f), 0f, 3, 0.2f, 0.35f, 3f, GB.MatteBlack);
            GB.Stairs(g, "RearSteps", new Vector3(-4.5f, 0f, 16.55f), 180f, 3, 0.2f, 0.35f, 2f, GB.MatteBlack);
            GB.Ramp(g, "EastRamp", new Vector3(8.6f, 0f, 12.4f), 270f, 2.1f, DeckTop, 1.8f, GB.MatteBlack);

            // Mint glow tape on step nosings and the deck's front lip.
            for (int i = 0; i < 3; i++)
            {
                GB.Box(g, $"FrontNosing{i}", new Vector3(0f, 0.2f * (i + 1) + 0.004f, 7.47f + 0.35f * i), new Vector3(3f, 0.008f, 0.04f), mintGlow, 0f, false);
                GB.Box(g, $"RearNosing{i}", new Vector3(-4.5f, 0.2f * (i + 1) + 0.004f, 16.53f - 0.35f * i), new Vector3(2f, 0.008f, 0.04f), mintGlow, 0f, false);
            }
            GB.Box(g, "EdgeTapeW", new Vector3(-4.5f, DeckTop + 0.004f, 8.53f), new Vector3(6f, 0.008f, 0.04f), mintGlow, 0f, false);
            GB.Box(g, "EdgeTapeE", new Vector3(4f, DeckTop + 0.004f, 8.53f), new Vector3(5f, 0.008f, 0.04f), mintGlow, 0f, false);

            // Low felt-grass ground rows along the deck's front edge (gap at the front steps).
            GrassFlat(g, "GroundRow_W1", new Vector3(-5.8f, DeckTop, 8.65f), 180f, 2.8f, 0.6f);
            GrassFlat(g, "GroundRow_W2", new Vector3(-3f, DeckTop, 8.65f), 180f, 2.6f, 0.55f);
            GrassFlat(g, "GroundRow_E1", new Vector3(2.9f, DeckTop, 8.65f), 180f, 2.4f, 0.55f);
            GrassFlat(g, "GroundRow_E2", new Vector3(5.25f, DeckTop, 8.65f), 180f, 2.1f, 0.6f);

            // Tall grass flats: line-of-sight breaks around the lamp hill (her eyespots only see lit lights).
            GrassFlat(g, "TallGrass_1", new Vector3(-4.6f, DeckTop, 10.6f), 195f, 2f, 1.6f);
            GrassFlat(g, "TallGrass_2", new Vector3(4f, DeckTop, 10.4f), 160f, 1.8f, 1.4f);
            GrassFlat(g, "TallGrass_3", new Vector3(-5.6f, DeckTop, 13.2f), 170f, 1.8f, 1.7f);
            GrassFlat(g, "TallGrass_4", new Vector3(4.4f, DeckTop, 12.6f), 90f, 1.4f, 1.5f);

            // Cut-out flowers with mint painted eyes.
            Flower(g, "Flower_1", new Vector3(-2.6f, DeckTop, 9.5f), 180f, 1.3f, petalLilac);
            Flower(g, "Flower_2", new Vector3(2.6f, DeckTop, 9.7f), 170f, 1.1f, petalCream);
            Flower(g, "Flower_3", new Vector3(-6.9f, DeckTop, 9.6f), 190f, 1.6f, petalCream);
            Flower(g, "Flower_4", new Vector3(3f, DeckTop, 15f), 200f, 1.4f, petalLilac);
            Flower(g, "Flower_5", new Vector3(-1.8f, HillTop, 12.2f), 180f, 0.8f, petalLilac);
            Flower(g, "Flower_6", new Vector3(6f, DeckTop, 15f), 160f, 1.2f, petalCream);

            // Curtain Call spike marks (faint mint X's) where a bowing Gloamie stops.
            Vector3[] spikes =
            {
                new Vector3(-4f, DeckTop, 9.4f), new Vector3(1.4f, DeckTop, 10.3f), new Vector3(5.6f, DeckTop, 14.3f),
                new Vector3(-3.4f, DeckTop, 15f), new Vector3(-9.2f, KnollTop, 11.2f),
            };
            for (int i = 0; i < spikes.Length; i++)
            {
                GB.Box(g, $"SpikeX{i}a", spikes[i] + Vector3.up * 0.004f, new Vector3(0.3f, 0.008f, 0.05f), mintFaint, 45f, false);
                GB.Box(g, $"SpikeX{i}b", spikes[i] + Vector3.up * 0.004f, new Vector3(0.3f, 0.008f, 0.05f), mintFaint, -45f, false);
            }

            // Set lamps Hesper is drawn to (one lit, one bulb only).
            SetLamp(g, "SetLamp_DeckEast", new Vector3(5.7f, DeckTop, 9.5f), 2f, 4f);
            SetLamp(g, "SetLamp_DeckWest", new Vector3(-6.8f, DeckTop, 15f), 1.8f, 0f);

            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(4.6f, DeckTop, 9.5f), 200f, "Act I reel (unlocks Glowsticks) at the foot of the east set lamp - grabbing it means standing in its light");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-3.6f, DeckTop, 12.4f), 0f, "Deck west of the lamp hill (part of the loop around it)");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-1.5f, DeckTop, 14.75f), 90f, "Deck behind the lamp hill");
        }

        // ---------------------------------------------------------------- Hesper's lamp-post (centrepiece on the hill)

        static void HesperLampPost(Transform root)
        {
            var g = GB.Group(root, "HesperLampPost", new Vector3(0f, HillTop, 13.3f), 180f);
            GB.Cylinder(g, "Plinth", new Vector3(0f, 0.2f, 0f), 0.38f, 0.4f, pistachio);
            GB.Cylinder(g, "PlinthTrim", new Vector3(0f, 0.42f, 0f), 0.3f, 0.05f, plum, false);
            GB.Cylinder(g, "Post", new Vector3(0f, 2.3f, 0f), 0.09f, 3.8f, pistachio);
            GB.Cylinder(g, "CollarLow", new Vector3(0f, 1.6f, 0f), 0.13f, 0.08f, plum, false);
            GB.Cylinder(g, "CollarHigh", new Vector3(0f, 3.2f, 0f), 0.12f, 0.08f, plum, false);
            GB.Box(g, "LadderBar", new Vector3(0f, 3.75f, 0f), new Vector3(0.9f, 0.06f, 0.06f), iron, 0f, false);
            GB.Cylinder(g, "LanternBase", new Vector3(0f, 4.24f, 0f), 0.28f, 0.06f, iron, false);
            GB.Box(g, "LanternGlass", new Vector3(0f, 4.5f, 0f), new Vector3(0.44f, 0.46f, 0.44f), GB.Glass, 0f, false);
            for (int i = 0; i < 4; i++)
                GB.Box(g, $"LanternRib{i}", new Vector3(i < 2 ? -0.22f : 0.22f, 4.5f, i % 2 == 0 ? -0.22f : 0.22f), new Vector3(0.03f, 0.48f, 0.03f), iron, 0f, false);
            GB.Box(g, "LanternRoof", new Vector3(0f, 4.77f, 0f), new Vector3(0.6f, 0.08f, 0.6f), iron, 0f, false);
            GB.Sphere(g, "Finial", new Vector3(0f, 4.88f, 0f), 0.06f, brass, false);
            GB.Cylinder(g, "DimmerWheel", new Vector3(0f, 0.25f, 0.4f), 0.14f, 0.04f, brass, new Vector3(90f, 0f, 0f), false);
            GB.GlowLamp(g, "HesperLamp", new Vector3(0f, 4.5f, 0f), MintWhite, 0.15f, 7f, 1.3f);

            // Her mark: four mint glow-tape eyespot rings around the plinth.
            var rings = GB.Group(root, "HesperEyespotMarks");
            for (int i = 0; i < 4; i++)
            {
                var p = new Vector3(i < 2 ? -0.8f : 0.8f, HillTop, i % 2 == 0 ? 12.6f : 13.75f);
                GB.Cylinder(rings, $"Ring{i}", p + Vector3.up * 0.005f, 0.22f, 0.01f, mintGlow, false);
                GB.Cylinder(rings, $"Pupil{i}", p + Vector3.up * 0.008f, 0.12f, 0.012f, GB.Velvet, false);
            }

            // Bin of popped bulbs dusted with mint scales.
            var bin = GB.Group(root, "BulbBin", new Vector3(1.7f, HillTop, 13.4f), 200f);
            GB.Box(bin, "Crate", new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.4f), GB.Plywood);
            for (int i = 0; i < 3; i++)
                GB.Sphere(bin, $"PoppedBulb{i}", new Vector3(-0.12f + 0.12f * i, 0.43f, 0.05f * (i - 1)), 0.05f, deadBulb, false);
            GB.Box(bin, "MothDust", new Vector3(0f, 0.405f, 0f), new Vector3(0.44f, 0.01f, 0.34f), mintFaint, 0f, false);
            GB.Marker(bin, MarkerKind.StoryProp, Vector3.zero, 0f,
                "Crate of burst studio bulbs, every one dusted with mint-green scales ('tik... tik-tik').");

            GB.Marker(root, MarkerKind.GloamieSpawn, new Vector3(0f, HillTop, 12.5f), 180f,
                "Miss Hesper's opening mark at the foot of her lamp-post: 'Lights low, voices soft.'");
        }

        // ---------------------------------------------------------------- west knoll (top 1.25) over the crawl gap

        static void Knoll(Transform root)
        {
            var g = GB.Group(root, "WestKnoll");
            // Slab x -11..-7.5 (underside 0.95). The fill leaves a 1.2 m wide, 0.95 m tall crawl gap along the west wall.
            GB.Box(g, "KnollSlab", new Vector3(-9.25f, (KnollUnder + KnollTop) * 0.5f, 12f), new Vector3(3.5f, KnollTop - KnollUnder, 7f), GB.MatteBlack);
            GB.Box(g, "KnollFill", new Vector3(-8.65f, KnollUnder * 0.5f, 12f), new Vector3(2.3f, KnollUnder, 7f), GB.MatteBlack);
            GB.Stairs(g, "KnollSteps", new Vector3(-6.6f, DeckTop, 12f), 270f, 3, (KnollTop - DeckTop) / 3f, 0.3f, 1.6f, GB.MatteBlack);

            GrassFlat(g, "KnollRow_S", new Vector3(-9.25f, KnollTop, 8.62f), 180f, 3.3f, 0.6f);
            GrassFlat(g, "KnollRow_N", new Vector3(-9.25f, KnollTop, 15.38f), 0f, 3.3f, 0.6f);
            SetLamp(g, "SetLantern_Knoll", new Vector3(-9.7f, KnollTop, 9.4f), 1.4f, 0f);
            Flower(g, "Flower_7", new Vector3(-8.3f, KnollTop, 14.4f), 150f, 1.3f, petalLilac);

            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(-9f, KnollTop, 12.8f), 90f, "Battery on top of the knoll (reached by the steps from the deck)");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(-10.4f, 0f, 8.3f), 180f, "Crawl gap under the knoll edge (0.95 m tall) - small handprints lead through to backstage");
            GB.Marker(g, MarkerKind.Handprint, new Vector3(-10.4f, 0f, 15.7f), 0f, "Backstage mouth of the knoll crawl gap");
        }

        // ---------------------------------------------------------------- east walkway (x 6.5..11, z 8.5..15.5)

        static void EastWalkway(Transform root)
        {
            var g = GB.Group(root, "EastWalkway");
            BuildMonitorCart(g, "MonitorCart_East", new Vector3(9.7f, 0f, 10.8f), 270f,
                "Field monitor beside the east ramp, screen toward the deck (Act V: switch off within 4 s)");
            SetLamp(g, "SetLantern_East", new Vector3(10f, 0f, 9f), 2f, 0f);

            GB.Box(g, "LightingRoadCase", new Vector3(10.62f, 0.45f, 14.6f), new Vector3(0.65f, 0.9f, 1.2f), GB.Mat("Meadow_RoadCase", new Color(0.1f, 0.1f, 0.11f), 0.3f));
            GB.Box(g, "RoadCaseTape", new Vector3(10.29f, 0.75f, 14.6f), new Vector3(0.01f, 0.04f, 1.1f), mintFaint, 0f, false);
            GB.Marker(g, MarkerKind.FuseSpawn, new Vector3(10.5f, 0.9f, 14.6f), 270f, "Fuse on top of the lighting road case (0.9 m) against the east wall");

            BuildHatch(g, "FloorVent_East", new Vector3(8.2f, 0f, 14.8f), 90f, false,
                "Floor vent on the east walkway between the ramp and backstage (Act V gap to Digsley)");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(8.6f, 0f, 9.4f), 0f, "East walkway, south of the side ramp");
        }

        // ---------------------------------------------------------------- backstage strip (z 15.5..20)

        static void Backstage(Transform root)
        {
            var g = GB.Group(root, "Backstage");

            // Wing breaker panel on the north wall.
            var breaker = GB.Group(g, "BreakerPanel", new Vector3(6.8f, 0f, 19.98f), 180f);
            GB.Box(breaker, "Cabinet", new Vector3(0f, 1.5f, 0.1f), new Vector3(0.9f, 1.3f, 0.2f), GB.Mat("Meadow_BreakerGrey", new Color(0.3f, 0.32f, 0.3f), 0.3f, 0.5f));
            GB.Box(breaker, "Lever", new Vector3(0.3f, 1.45f, 0.23f), new Vector3(0.06f, 0.36f, 0.06f), GB.MatteBlack, 0f, false);
            GB.Box(breaker, "LeverGrip", new Vector3(0.3f, 1.65f, 0.28f), new Vector3(0.14f, 0.05f, 0.06f), GB.Mat("Meadow_GripRed", new Color(0.45f, 0.06f, 0.05f), 0.3f), 0f, false);
            GB.Cylinder(breaker, "Conduit", new Vector3(-0.3f, 4.8f, 0.05f), 0.05f, 5.3f, GB.Metal, false);
            GB.Box(breaker, "GlowArrow", new Vector3(0f, 2.3f, 0.02f), new Vector3(0.5f, 0.06f, 0.01f), mintGlow, 0f, false);
            GB.Marker(g, MarkerKind.Breaker, new Vector3(6.8f, 0f, 19.6f), 180f,
                "Meadow breaker on the back wall - throwing it brings every set lamp up in a loud 10 s crescendo");

            // Costume rack of felt grass and petal costumes.
            var rack = GB.Group(g, "CostumeRack", new Vector3(-7.4f, 0f, 18.75f), 180f);
            GB.Box(rack, "Base", new Vector3(0f, 0.06f, 0f), new Vector3(2f, 0.1f, 0.6f), GB.Metal);
            GB.Box(rack, "UprightW", new Vector3(-0.97f, 1f, 0f), new Vector3(0.05f, 1.9f, 0.05f), GB.Metal);
            GB.Box(rack, "UprightE", new Vector3(0.97f, 1f, 0f), new Vector3(0.05f, 1.9f, 0.05f), GB.Metal);
            GB.Cylinder(rack, "Rail", new Vector3(0f, 1.92f, 0f), 0.025f, 2f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Box(rack, "Costumes", new Vector3(0f, 1.2f, 0f), new Vector3(1.85f, 1.3f, 0.5f), GB.Mat("Meadow_CostumeFelt", new Color(0.2f, 0.26f, 0.16f), 0.05f));
            for (int i = 0; i < 3; i++)
                GB.Ellipsoid(rack, $"PuppeteerHood{i}", new Vector3(-0.6f + 0.6f * i, 2.1f, 0f), new Vector3(0.3f, 0.32f, 0.3f), GB.Velvet, Vector3.zero, false);
            GB.Marker(rack, MarkerKind.HidingSpot, Vector3.zero, 0f, "Between the felt grass costumes on the rack");

            // Crew lockers against the east wall.
            var lockerMat = GB.Mat("Meadow_LockerGreen", new Color(0.17f, 0.23f, 0.19f), 0.3f, 0.4f);
            for (int i = 0; i < 2; i++)
            {
                var locker = GB.Group(g, $"CrewLocker{i}", new Vector3(10.7f, 0f, 18f + 0.95f * i), 270f);
                GB.Box(locker, "Body", new Vector3(0f, 1f, 0f), new Vector3(0.85f, 2f, 0.6f), lockerMat);
                GB.Box(locker, "DoorSeam", new Vector3(0f, 1f, 0.305f), new Vector3(0.02f, 1.8f, 0.01f), GB.MatteBlack, 0f, false);
                GB.Box(locker, "Vents", new Vector3(0.2f, 1.75f, 0.305f), new Vector3(0.3f, 0.14f, 0.01f), GB.MatteBlack, 0f, false);
                if (i == 0) GB.Marker(locker, MarkerKind.HidingSpot, Vector3.zero, 0f, "Inside the crew locker (vent slats face the backstage strip)");
            }

            SetLamp(g, "SpareLamp_Unlit", new Vector3(-10.2f, 0f, 19.2f), 2.2f, 0f, true);
            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(-9.6f, 0f, 17.2f), 45f,
                "Act I reel (unlocks Glowsticks) by the dead spare lamp - long way round the east side, or 7 m through the knoll crawl gap");

            BuildHatch(g, "StageTrapdoor", new Vector3(-1.8f, 0f, 17.6f), 0f, true,
                "Old stage trapdoor behind the deck (Act V: Digsley can surface here); the deck itself is no-trap ground");
            BuildMonitorCart(g, "MonitorCart_Backstage", new Vector3(1.6f, 0f, 18.9f), 180f,
                "Field monitor parked backstage, facing the rear steps (Act V)");

            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(0.4f, 0f, 17.4f), 90f, "Backstage centre, beside the trapdoor");
            GB.Marker(g, MarkerKind.PatrolPoint, new Vector3(-9.4f, 0f, 18.2f), 90f, "Backstage west, by the crawl-gap mouth and the reel");
        }

        // ---------------------------------------------------------------- overhead grid, catwalk, UV wash and fireflies (decor)

        static readonly Vector3[] ParCans =
        {
            new Vector3(-6f, 6.55f, 9.5f), new Vector3(-2f, 6.55f, 9.5f), new Vector3(2f, 6.55f, 9.5f), new Vector3(6f, 6.55f, 9.5f),
            new Vector3(-3f, 6.55f, 12.5f), new Vector3(-4f, 6.55f, 15.5f), new Vector3(0f, 6.55f, 15.5f), new Vector3(4f, 6.55f, 15.5f),
        };

        static readonly Vector3[] Fireflies =
        {
            new Vector3(-5.8f, 1.9f, 11.6f), new Vector3(-3.2f, 2.4f, 10.2f), new Vector3(-1f, 2.9f, 9.4f), new Vector3(1.8f, 2.2f, 10.8f),
            new Vector3(3.9f, 2.7f, 13.8f), new Vector3(5.6f, 1.8f, 11f), new Vector3(-6.4f, 2.6f, 14.2f), new Vector3(-9.2f, 2.4f, 11f),
        };

        static void Overhead(Transform root)
        {
            var g = GB.Group(root, "LightingGrid");
            float[] rows = { 9.5f, 12.5f, 15.5f };
            for (int i = 0; i < rows.Length; i++)
                GB.Cylinder(g, $"PipeX{i}", new Vector3(0f, 6.8f, rows[i]), 0.04f, 18f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(g, "PipeZ_W", new Vector3(-7f, 6.88f, 12.5f), 0.04f, 8f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(g, "PipeZ_E", new Vector3(7f, 6.88f, 12.5f), 0.04f, 8f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            for (int i = 0; i < 4; i++)
                GB.Box(g, $"Hanger{i}", new Vector3(i < 2 ? -7f : 7f, 7.45f, i % 2 == 0 ? 9.5f : 15.5f), new Vector3(0.03f, 1.1f, 0.03f), GB.Metal, 0f, false);
            for (int i = 0; i < ParCans.Length; i++)
                GB.Cylinder(g, $"ParCan{i}", ParCans[i], 0.11f, 0.28f, GB.MatteBlack, false);

            // Catwalk across the hall above the proscenium.
            GB.Box(g, "CatwalkDeck", new Vector3(0f, 6.2f, 7.6f), new Vector3(18f, 0.06f, 0.8f), GB.Metal, 0f, false);
            GB.Box(g, "CatwalkRailS", new Vector3(0f, 7.1f, 7.22f), new Vector3(18f, 0.05f, 0.05f), GB.Metal, 0f, false);
            GB.Box(g, "CatwalkRailN", new Vector3(0f, 7.1f, 7.98f), new Vector3(18f, 0.05f, 0.05f), GB.Metal, 0f, false);
            for (int i = 0; i < 4; i++)
                GB.Box(g, $"CatwalkHanger{i}", new Vector3(-8f + 16f / 3f * i, 7.12f, 7.6f), new Vector3(0.04f, 1.76f, 0.04f), GB.Metal, 0f, false);

            // Faint black-light wash over the lamp hill so the felt and glow tape read.
            GB.SpotLight(g, "UVWash", new Vector3(0f, 6.6f, 9.5f), new Vector3(55f, 0f, 0f), CueViolet, 8f, 70f, 1.2f);

            // Puppet fireflies on invisible black rods.
            var flies = GB.Group(root, "Fireflies");
            for (int i = 0; i < Fireflies.Length; i++)
                GB.Sphere(flies, $"Firefly{i}", Fireflies[i], 0.035f, mintGlow, false);
        }

        // ---------------------------------------------------------------- prop helpers (each builds inside its own group)

        /// Felt grass flat standing on basePos: a solid board with felt blades tipped in mint glow tape, painted face
        /// along local +Z, a light stage brace behind it (local -Z).
        static void GrassFlat(Transform parent, string name, Vector3 basePos, float yaw, float width, float height)
        {
            var g = GB.Group(parent, name, basePos, yaw);
            float boardH = height * 0.55f;
            GB.Box(g, "Board", new Vector3(0f, boardH * 0.5f, 0f), new Vector3(width, boardH, 0.06f), felt);
            GB.Box(g, "Brace", new Vector3(0f, boardH * 0.45f, -0.25f), new Vector3(0.06f, boardH * 0.9f, 0.5f), GB.Plywood, 0f, false);
            int blades = Mathf.Max(3, Mathf.RoundToInt(width / 0.4f));
            for (int i = 0; i < blades; i++)
            {
                float x = -width * 0.5f + width * (i + 0.5f) / blades;
                float h = height * (0.7f + 0.3f * Mathf.Abs(Mathf.Sin(i * 2.3f + width)));
                float tilt = Mathf.Sin(i * 1.7f + height) * 12f;
                var rot = Quaternion.Euler(0f, 0f, tilt);
                Vector3 mid = new Vector3(x, 0f, 0.01f) + rot * new Vector3(0f, h * 0.5f, 0f);
                GB.Box(g, $"Blade{i:00}", mid, new Vector3(0.18f, h, 0.04f), felt, new Vector3(0f, 0f, tilt), false);
                Vector3 tip = new Vector3(x, 0f, 0.035f) + rot * new Vector3(0f, h - 0.05f, 0f);
                GB.Box(g, $"Tip{i:00}", tip, new Vector3(0.12f, 0.08f, 0.02f), mintGlow, new Vector3(0f, 0f, tilt), false);
            }
        }

        /// Giant plywood cut-out flower on a stage weight; its face (petals and a mint painted eye) looks along local +Z.
        static void Flower(Transform parent, string name, Vector3 basePos, float yaw, float height, Material petals)
        {
            var g = GB.Group(parent, name, basePos, yaw);
            GB.Box(g, "Weight", new Vector3(0f, 0.03f, -0.05f), new Vector3(0.3f, 0.06f, 0.3f), GB.MatteBlack, 0f, false);
            GB.Box(g, "Stem", new Vector3(0f, height * 0.5f, 0f), new Vector3(0.08f, height, 0.08f), felt);
            GB.Box(g, "Leaf", new Vector3(0.16f, height * 0.4f, 0.02f), new Vector3(0.32f, 0.12f, 0.03f), felt, new Vector3(0f, 0f, 30f), false);
            float r = 0.22f + height * 0.12f;
            GB.Cylinder(g, "Petals", new Vector3(0f, height + r * 0.6f, 0.03f), r, 0.04f, petals, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(g, "Eye", new Vector3(0f, height + r * 0.6f, 0.065f), r * 0.38f, 0.03f, mintFaint, new Vector3(90f, 0f, 0f), false);
        }

        /// Small painted set lamp-post. lightRange > 0 adds a weak warm light (counts against the light budget),
        /// 0 gives a glowing bulb only, dead = true leaves a burnt-out bulb.
        static void SetLamp(Transform parent, string name, Vector3 basePos, float postHeight, float lightRange, bool dead = false)
        {
            var g = GB.Group(parent, name, basePos);
            GB.Cylinder(g, "Base", new Vector3(0f, 0.08f, 0f), 0.22f, 0.16f, iron);
            GB.Cylinder(g, "Post", new Vector3(0f, 0.16f + postHeight * 0.5f, 0f), 0.05f, postHeight, iron);
            float top = 0.16f + postHeight;
            GB.Box(g, "MintBand", new Vector3(0f, 1f, 0f), new Vector3(0.12f, 0.03f, 0.12f), mintFaint, 0f, false);
            GB.Box(g, "Cage", new Vector3(0f, top + 0.17f, 0f), new Vector3(0.26f, 0.32f, 0.26f), GB.Glass, 0f, false);
            GB.Box(g, "Cap", new Vector3(0f, top + 0.36f, 0f), new Vector3(0.34f, 0.06f, 0.34f), iron, 0f, false);
            if (dead) GB.Sphere(g, "DeadBulb", new Vector3(0f, top + 0.16f, 0f), 0.07f, deadBulb, false);
            else GB.GlowLamp(g, "Bulb", new Vector3(0f, top + 0.16f, 0f), WarmBulb, 0.07f, lightRange, 1f);
        }

        /// Rolling field-monitor cart: CRT on a two-shelf trolley, screen along local +Z. Carries its MonitorCart marker.
        static void BuildMonitorCart(Transform parent, string name, Vector3 pos, float yaw, string note)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Box(g, "LowerShelf", new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.05f, 0.6f), GB.Metal);
            GB.Box(g, "TopShelf", new Vector3(0f, 0.85f, 0f), new Vector3(0.9f, 0.05f, 0.6f), GB.Metal);
            GB.Box(g, "Back", new Vector3(0f, 0.52f, -0.27f), new Vector3(0.9f, 0.62f, 0.05f), GB.Metal);
            for (int i = 0; i < 4; i++)
                GB.Sphere(g, $"Caster{i}", new Vector3(i < 2 ? -0.38f : 0.38f, 0.07f, i % 2 == 0 ? -0.24f : 0.24f), 0.065f, GB.MatteBlack, false);
            GB.Box(g, "Monitor", new Vector3(0f, 1.11f, -0.02f), new Vector3(0.56f, 0.46f, 0.5f), crtBody);
            GB.Box(g, "Screen", new Vector3(0f, 1.12f, 0.235f), new Vector3(0.44f, 0.34f, 0.02f), crtScreen, 0f, false);
            GB.Cylinder(g, "CableReel", new Vector3(0.1f, 0.29f, 0.02f), 0.15f, 0.12f, GB.MatteBlack, false);
            GB.Marker(g, MarkerKind.MonitorCart, Vector3.zero, 0f, note);
        }

        /// Floor vent (grille) or stage trapdoor, flush with the floor, with Digsley's unlit red cue lamp beside it.
        static void BuildHatch(Transform parent, string name, Vector3 pos, float yaw, bool trapdoor, string note)
        {
            var g = GB.Group(parent, name, pos, yaw);
            if (trapdoor)
            {
                GB.Box(g, "Lid", new Vector3(0f, 0.006f, 0f), new Vector3(1.2f, 0.012f, 1.2f), trapLid, 0f, false);
                GB.Box(g, "Hinge", new Vector3(0f, 0.012f, -0.58f), new Vector3(1.1f, 0.012f, 0.04f), GB.Metal, 0f, false);
                GB.Cylinder(g, "RingPull", new Vector3(0f, 0.016f, 0.45f), 0.07f, 0.008f, GB.Metal, false);
            }
            else
            {
                GB.Box(g, "Grille", new Vector3(0f, 0.006f, 0f), new Vector3(1f, 0.012f, 0.6f), GB.Metal, 0f, false);
                for (int i = 0; i < 4; i++)
                    GB.Box(g, $"Slot{i}", new Vector3(-0.36f + 0.24f * i, 0.013f, 0f), new Vector3(0.12f, 0.004f, 0.5f), GB.MatteBlack, 0f, false);
            }
            float side = trapdoor ? 0.85f : 0.75f;
            GB.Box(g, "CueLampHousing", new Vector3(side, 0.05f, 0f), new Vector3(0.12f, 0.1f, 0.12f), GB.MatteBlack, 0f, false);
            GB.Sphere(g, "CueLamp", new Vector3(side, 0.12f, 0f), 0.045f, trapCueLens, false);
            GB.Marker(g, MarkerKind.Hatch, Vector3.zero, 0f, note);
        }

        /// Warm recharge lamp on a 2.2 m wall bracket. wallPoint is the wall face at floor level; yaw faces into the room.
        static void BuildRechargeLamp(Transform parent, string name, Vector3 wallPoint, float yaw, string note)
        {
            var g = GB.Group(parent, name, wallPoint, yaw);
            GB.Box(g, "Backplate", new Vector3(0f, 2.2f, 0.02f), new Vector3(0.22f, 0.32f, 0.04f), brass, 0f, false);
            GB.Box(g, "Bracket", new Vector3(0f, 2.12f, 0.14f), new Vector3(0.05f, 0.05f, 0.24f), brass, 0f, false);
            GB.Box(g, "Shade", new Vector3(0f, 2.33f, 0.27f), new Vector3(0.24f, 0.05f, 0.24f), brass, 0f, false);
            GB.GlowLamp(g, "Bulb", new Vector3(0f, 2.2f, 0.27f), Amber, 0.09f, 3.5f, 0.9f);
            GB.Marker(g, MarkerKind.RechargeLamp, new Vector3(0f, 0f, 0.4f), 0f, note);
        }
    }
}
