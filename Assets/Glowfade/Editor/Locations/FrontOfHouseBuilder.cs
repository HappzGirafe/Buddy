using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Front of house of the Wexley Starhouse (greybox).
    /// Root: ground floor at the end of the 180-degree corridor; local +Z points away from the theatre (towards the street).
    /// - Lobby: |x| <= 11, 0 <= z <= 20, ceiling 8 m. Corridor door in the theatre-side wall (z = 0), chained street doors at z = 20.
    ///   Loops: round "Glen Moon" banquette, island ticket booth, concession counter + staff aisle, workshop with two doors.
    /// - Mezzanine (top y 4.5) along the theatre-side wall, reached by a 15-step flight on the +X wall, leads through the wall
    ///   into the projection booth above the corridor (safe room + save point) with the film vault.
    /// - Costume workshop: walled room in the -X / street corner (x -11..-5.5, z 8.6..20), kept dark.
    public static class FrontOfHouseBuilder
    {
        // ------------------------------------------------------------------ layout numbers (root-local metres)
        const float LobbyHalf = 11f, LobbyDepth = 20f, LobbyHeight = 8f, T = 0.3f;
        const float BoothY = StarhouseLayout.BoothFloorY;   // 4.5: booth floor and mezzanine top
        const float BoothHalf = 3.5f;                       // booth zone |x| (outer faces of its side walls)
        const float BoothNorth = -9.9f;                     // booth slab reaches the theatre's round wall here
        const float BoothCeiling = 7.7f;                    // underside of the booth ceiling slab
        const float BoothDoorX = 2.1f;                      // booth door in the theatre-side wall, x 1.3..2.9
        const float VaultDoorX = -0.3f;                     // vault door, x -1.05..0.45
        const float MezzDepth = 2.4f, MezzWest = -2.5f;     // mezzanine z 0..2.4, x -2.5..11
        const float StairX = 10.1f, StairWidth = 1.8f, StairRise = 0.3f, StairRun = 0.4f;
        const int StairSteps = 15;
        const float StairFoot = MezzDepth + StairSteps * StairRun;   // 8.4: the flight climbs towards -Z from here
        const float WorkshopEast = -5.5f, WorkshopNorth = 8.6f, WorkshopHeight = 3.6f;

        // ------------------------------------------------------------------ palette (assigned in MakeMaterials)
        static Material terrazzo, plaster, brass, redVelvet, laminate, cream, paper, bronze, deadBulb, frond, candy;
        static Material boothFloor, boothWall, projectorGrey, vaultSteel, blanket, mirror;
        static Material workshopWall, workshopFloor, dressForm;
        static Material glowWarm, glowStreet, glowMoon, glowRed, glowScreen, glowLamphouse, glowPopcorn, glowTape;
        static Material glowMint, glowGold, glowSaddle, glowLilac, glowAmber, glowCyan, glowPink;
        static Material headHesper, headVinyl, headPeekins, headPinny, headCream, headTapwell, headEar, headDeepwick, headDigsley, headWhite;
        static Material feltGreen, quilt, tweed, knitTeal, cordBlue, crimson;
        static Material[] callSignMats;

        static readonly Color WarmLight = new Color(1f, 0.72f, 0.45f);
        static readonly Color LampLight = new Color(1f, 0.68f, 0.38f);
        // Gloamie glow fingerprints (Cast.md): Hesper mint rings, Peekins gold eyeshine, Pinny blue-white saddle,
        // Tapwell lilac fingertip, Deepwick amber lure + cyan dots, Digsley pink nose / spike tape.
        static readonly Color Mint = new Color(0.45f, 1f, 0.7f);
        static readonly Color Gold = new Color(1f, 0.75f, 0.25f);
        static readonly Color SaddleWhite = new Color(0.75f, 0.88f, 1f);
        static readonly Color Lilac = new Color(0.8f, 0.65f, 1f);
        static readonly Color Amber = new Color(1f, 0.6f, 0.2f);
        static readonly Color Cyan = new Color(0.3f, 0.95f, 1f);
        static readonly Color Pink = new Color(1f, 0.45f, 0.7f);

        public static void Build(Transform root)
        {
            MakeMaterials();
            BuildShell(GB.Group(root, "Shell"));
            BuildMezzanineAndStairs(GB.Group(root, "MezzanineAndStairs"));

            var lobby = GB.Group(root, "Lobby");
            BuildStreetDoors(lobby);
            BuildTicketBooth(lobby);
            BuildConcession(lobby);
            BuildMoonBanquette(lobby);
            BuildLobbyFurniture(lobby);
            BuildBreaker(lobby);

            BuildWorkshop(GB.Group(root, "CostumeWorkshop"));
            BuildBooth(GB.Group(root, "ProjectionBooth"));
        }

        static void MakeMaterials()
        {
            terrazzo = GB.Mat("FOH_Terrazzo", new Color(0.17f, 0.16f, 0.15f), 0.45f);
            plaster = GB.Mat("FOH_Plaster", new Color(0.12f, 0.1f, 0.11f), 0.1f);
            brass = GB.Mat("FOH_Brass", new Color(0.42f, 0.33f, 0.16f), 0.5f, 0.8f);
            redVelvet = GB.Mat("FOH_RedVelvet", new Color(0.22f, 0.03f, 0.05f), 0.05f);
            laminate = GB.Mat("FOH_RedLaminate", new Color(0.33f, 0.12f, 0.1f), 0.35f);
            cream = GB.Mat("FOH_CreamPaint", new Color(0.6f, 0.56f, 0.46f), 0.2f);
            paper = GB.Mat("FOH_FadedPaper", new Color(0.45f, 0.4f, 0.32f), 0.1f);
            bronze = GB.Mat("FOH_Bronze", new Color(0.18f, 0.14f, 0.1f), 0.35f, 0.6f);
            deadBulb = GB.Mat("FOH_DeadBulb", new Color(0.3f, 0.29f, 0.26f), 0.6f);
            frond = GB.Mat("FOH_DeadFrond", new Color(0.2f, 0.17f, 0.09f), 0.1f);
            candy = GB.Mat("FOH_FadedCandy", new Color(0.5f, 0.3f, 0.35f), 0.3f);

            boothFloor = GB.Mat("FOH_Booth_Linoleum", new Color(0.13f, 0.15f, 0.13f), 0.3f);
            boothWall = GB.Mat("FOH_Booth_Wall", new Color(0.2f, 0.18f, 0.16f), 0.1f);
            projectorGrey = GB.Mat("FOH_Booth_ProjectorGrey", new Color(0.24f, 0.26f, 0.25f), 0.35f, 0.5f);
            vaultSteel = GB.Mat("FOH_Booth_VaultSteel", new Color(0.32f, 0.33f, 0.35f), 0.45f, 0.7f);
            blanket = GB.Mat("FOH_Booth_Blanket", new Color(0.3f, 0.21f, 0.13f), 0.05f);
            mirror = GB.Mat("FOH_Workshop_Mirror", new Color(0.35f, 0.37f, 0.4f), 0.95f, 0.6f);

            workshopWall = GB.Mat("FOH_Workshop_Wall", new Color(0.15f, 0.13f, 0.12f), 0.1f);
            workshopFloor = GB.Mat("FOH_Workshop_Boards", new Color(0.24f, 0.19f, 0.14f), 0.2f);
            dressForm = GB.Mat("FOH_Workshop_DressForm", new Color(0.5f, 0.45f, 0.38f), 0.15f);

            glowWarm = GB.Glow("FOH_Glow_Warm", WarmLight, 1.6f);
            glowStreet = GB.Glow("FOH_Glow_Street", new Color(0.5f, 0.6f, 0.85f), 0.35f);
            glowMoon = GB.Glow("FOH_Glow_Moon", new Color(1f, 0.9f, 0.7f), 0.5f);
            glowRed = GB.Glow("FOH_Glow_Red", new Color(1f, 0.12f, 0.08f), 1.5f);
            glowScreen = GB.Glow("FOH_Glow_Screen", new Color(0.55f, 0.65f, 0.75f), 0.5f);
            glowLamphouse = GB.Glow("FOH_Glow_Lamphouse", new Color(1f, 0.55f, 0.25f), 0.8f);
            glowPopcorn = GB.Glow("FOH_Glow_Popcorn", new Color(1f, 0.82f, 0.4f), 0.5f);
            glowTape = GB.Glow("FOH_Glow_SpikeTape", new Color(1f, 0.9f, 0.3f), 0.6f);

            glowMint = GB.Glow("FOH_Glow_Mint", Mint, 1.2f);
            glowGold = GB.Glow("FOH_Glow_Gold", Gold, 1.2f);
            glowSaddle = GB.Glow("FOH_Glow_Saddle", SaddleWhite, 1f);
            glowLilac = GB.Glow("FOH_Glow_Lilac", Lilac, 1.2f);
            glowAmber = GB.Glow("FOH_Glow_Amber", Amber, 1.2f);
            glowCyan = GB.Glow("FOH_Glow_Cyan", Cyan, 1f);
            glowPink = GB.Glow("FOH_Glow_Pink", Pink, 1.2f);
            callSignMats = new[] { glowMint, glowGold, glowSaddle, glowLilac, glowAmber, glowPink };

            headHesper = GB.Mat("FOH_Head_Pistachio", new Color(0.55f, 0.64f, 0.45f), 0.05f);
            headVinyl = GB.Mat("FOH_Head_BlackVinyl", new Color(0.02f, 0.02f, 0.025f), 0.9f);
            headPeekins = GB.Mat("FOH_Head_Oatmeal", new Color(0.6f, 0.5f, 0.36f), 0.05f);
            headPinny = GB.Mat("FOH_Head_InkVelvet", new Color(0.04f, 0.04f, 0.05f), 0.1f);
            headCream = GB.Mat("FOH_Head_QuiltCream", new Color(0.8f, 0.76f, 0.62f), 0.1f);
            headTapwell = GB.Mat("FOH_Head_Grizzle", new Color(0.2f, 0.2f, 0.22f), 0.05f);
            headEar = GB.Mat("FOH_Head_EarLatex", new Color(0.62f, 0.47f, 0.46f), 0.3f);
            headDeepwick = GB.Mat("FOH_Head_NavyGloss", new Color(0.03f, 0.05f, 0.11f), 0.8f);
            headDigsley = GB.Mat("FOH_Head_CrushedVelvet", new Color(0.08f, 0.05f, 0.05f), 0.15f);
            headWhite = GB.Mat("FOH_Head_GoggleWhite", new Color(0.85f, 0.85f, 0.8f), 0.4f);

            feltGreen = GB.Mat("FOH_Felt_CapeGreen", new Color(0.42f, 0.5f, 0.34f), 0.05f);
            quilt = GB.Mat("FOH_Felt_Quilt", new Color(0.72f, 0.68f, 0.55f), 0.05f);
            tweed = GB.Mat("FOH_Felt_Tweed", new Color(0.5f, 0.38f, 0.15f), 0.05f);
            knitTeal = GB.Mat("FOH_Felt_KnitTeal", new Color(0.28f, 0.55f, 0.5f), 0.05f);
            cordBlue = GB.Mat("FOH_Felt_CordBlue", new Color(0.55f, 0.68f, 0.82f), 0.05f);
            crimson = GB.Mat("FOH_Felt_Crimson", new Color(0.4f, 0.08f, 0.1f), 0.05f);
        }

        // ================================================================== shell
        static void BuildShell(Transform g)
        {
            float wx = LobbyHalf + T, h = LobbyHeight, zc = -T * 0.5f;
            GB.Box(g, "Floor", new Vector3(0f, -0.25f, LobbyDepth * 0.5f), new Vector3(2f * wx, 0.5f, LobbyDepth + 2f * T), terrazzo);
            GB.Box(g, "Ceiling", new Vector3(0f, h + T * 0.5f, LobbyDepth * 0.5f), new Vector3(2f * wx, T, LobbyDepth + 2f * T), GB.DarkConcrete);

            // theatre-side wall (z -0.3..0): 3x3 m corridor door at x = 0; the booth slab passes through at y 4.2-4.5 and the
            // booth's own south wall (built in BuildBooth) fills |x| <= 3.5 above that.
            GB.Box(g, "TheatreWall_W", new Vector3(-(wx + BoothHalf) * 0.5f, h * 0.5f, zc), new Vector3(wx - BoothHalf, h, T), plaster);
            GB.Box(g, "TheatreWall_E", new Vector3((wx + BoothHalf) * 0.5f, h * 0.5f, zc), new Vector3(wx - BoothHalf, h, T), plaster);
            GB.Box(g, "CorridorDoor_JambW", new Vector3(-2.5f, 1.5f, zc), new Vector3(2f, 3f, T), plaster);
            GB.Box(g, "CorridorDoor_JambE", new Vector3(2.5f, 1.5f, zc), new Vector3(2f, 3f, T), plaster);
            GB.Box(g, "CorridorDoor_Lintel", new Vector3(0f, 3.6f, zc), new Vector3(2f * BoothHalf, 1.2f, T), plaster);

            GB.Box(g, "StreetWall", new Vector3(0f, h * 0.5f, LobbyDepth + T * 0.5f), new Vector3(2f * wx, h, T), plaster);
            GB.Box(g, "SideWall_W", new Vector3(-LobbyHalf - T * 0.5f, h * 0.5f, LobbyDepth * 0.5f), new Vector3(T, h, LobbyDepth), plaster);
            GB.Box(g, "SideWall_E", new Vector3(LobbyHalf + T * 0.5f, h * 0.5f, LobbyDepth * 0.5f), new Vector3(T, h, LobbyDepth), plaster);

            // floor dressing: a worn runner from the street doors towards the theatre, and the Starhouse's brass compass star
            GB.Box(g, "CarpetRunner", new Vector3(0f, 0.006f, 12.8f), new Vector3(2.2f, 0.012f, 13.6f), GB.Carpet, collider: false);
            for (int i = 0; i < 4; i++)
                GB.Box(g, $"StarInlay{i}", new Vector3(0f, 0.008f, 4f), new Vector3(0.2f, 0.012f, i % 2 == 0 ? 3.4f : 2.2f), brass, i * 45f, false);
        }

        // ================================================================== mezzanine + stairs to the booth
        static void BuildMezzanineAndStairs(Transform g)
        {
            float east = LobbyHalf;
            float stairWest = StairX - StairWidth * 0.5f;   // 9.2: open (west) edge of the flight
            float midX = (MezzWest + east) * 0.5f;

            GB.Box(g, "MezzanineSlab", new Vector3(midX, BoothY - 0.15f, MezzDepth * 0.5f), new Vector3(east - MezzWest, 0.3f, MezzDepth), GB.Carpet);
            GB.Box(g, "Fascia", new Vector3(midX, BoothY - 0.25f, MezzDepth + 0.04f), new Vector3(east - MezzWest, 0.5f, 0.08f), brass, collider: false);

            // balustrade along the open edge (stops at the stair head) and across the west end
            float railLen = stairWest - MezzWest, railX = (MezzWest + stairWest) * 0.5f;
            GB.Box(g, "Balustrade", new Vector3(railX, BoothY + 0.5f, MezzDepth - 0.05f), new Vector3(railLen, 1f, 0.1f), plaster);
            GB.Box(g, "HandRail", new Vector3(railX, BoothY + 1.03f, MezzDepth - 0.05f), new Vector3(railLen, 0.06f, 0.14f), brass);
            GB.Box(g, "Balustrade_WestEnd", new Vector3(MezzWest + 0.05f, BoothY + 0.5f, MezzDepth * 0.5f), new Vector3(0.1f, 1f, MezzDepth), plaster);
            GB.Box(g, "HandRail_WestEnd", new Vector3(MezzWest + 0.05f, BoothY + 1.03f, MezzDepth * 0.5f), new Vector3(0.14f, 0.06f, MezzDepth), brass);

            float[] columnX = { -2.1f, 2.7f, 6.9f };
            for (int i = 0; i < columnX.Length; i++)
                GB.Box(g, $"Column{i}", new Vector3(columnX[i], (BoothY - 0.3f) * 0.5f, MezzDepth - 0.3f), new Vector3(0.4f, BoothY - 0.3f, 0.4f), plaster);
            GB.Box(g, "StairCore", new Vector3(StairX, (BoothY - 0.3f) * 0.5f, MezzDepth * 0.5f), new Vector3(StairWidth, BoothY - 0.3f, MezzDepth), plaster);

            // 15 x 0.3 m risers, 0.4 m treads, climbing towards the theatre wall (-Z) to the mezzanine landing
            GB.Stairs(g, "Stairs_ToBooth", new Vector3(StairX, 0f, StairFoot), 180f, StairSteps, StairRise, StairRun, StairWidth, GB.Carpet);

            float slope = Mathf.Atan2(StairRise, StairRun) * Mathf.Rad2Deg;
            float zLow = StairFoot + 0.2f, zHigh = MezzDepth, zMid = (zLow + zHigh) * 0.5f;
            float noseMid = StairRise + (StairFoot - zMid) * (StairRise / StairRun);   // nosing line height at zMid
            float len = (zLow - zHigh) / Mathf.Cos(slope * Mathf.Deg2Rad);
            var pitch = new Vector3(-slope, 180f, 0f);
            GB.Box(g, "StairBalustrade", new Vector3(stairWest + 0.05f, noseMid + 0.5f, zMid), new Vector3(0.1f, 1f, len), plaster, pitch);
            GB.Box(g, "StairHandRail", new Vector3(stairWest + 0.05f, noseMid + 1.03f, zMid), new Vector3(0.14f, 0.06f, len), brass, pitch, false);

            // faded banner over the corridor door, hung from the mezzanine fascia
            GB.Box(g, "Banner_LongestNight", new Vector3(0f, 3.65f, MezzDepth + 0.1f), new Vector3(3.2f, 0.9f, 0.03f), cream, collider: false);
            GB.Box(g, "Banner_Title", new Vector3(0f, 3.78f, MezzDepth + 0.12f), new Vector3(2.6f, 0.22f, 0.01f), glowMoon, collider: false);
            GB.Box(g, "Banner_Stars", new Vector3(0f, 3.45f, MezzDepth + 0.12f), new Vector3(1.6f, 0.08f, 0.01f), glowMint, collider: false);
        }

        // ================================================================== lobby
        /// Chained street doors (decor) on the far wall, dead marquee bulbs, PlayerStart just inside.
        static void BuildStreetDoors(Transform g)
        {
            float zf = LobbyDepth - 0.07f;   // door leaves stand against the street wall's inner face
            for (int p = 0; p < 2; p++)
            {
                var pair = GB.Group(g, p == 0 ? "StreetDoors_W" : "StreetDoors_E", new Vector3(p == 0 ? -1.7f : 1.7f, 0f, zf));
                GB.Box(pair, "Header", new Vector3(0f, 2.72f, 0f), new Vector3(2.5f, 0.24f, 0.16f), bronze);
                for (int s = 0; s < 2; s++)
                {
                    float lx = s == 0 ? -0.56f : 0.56f;
                    GB.Box(pair, $"Leaf{s}", new Vector3(lx, 1.3f, 0f), new Vector3(1.08f, 2.6f, 0.1f), bronze);
                    GB.Box(pair, $"Glass{s}", new Vector3(lx, 1.7f, -0.06f), new Vector3(0.62f, 1.3f, 0.02f), glowStreet, collider: false);
                    GB.Box(pair, $"PushBar{s}", new Vector3(lx, 1.05f, -0.1f), new Vector3(0.9f, 0.06f, 0.06f), brass, collider: false);
                }
                // heavy chain looped through both push bars, sagging to a padlock
                for (int i = 0; i < 9; i++)
                {
                    float t = i / 8f, u = 2f * t - 1f;
                    var link = new Vector3(Mathf.Lerp(-0.6f, 0.6f, t), 1.05f - 0.14f * (1f - u * u), -0.15f);
                    GB.Box(pair, $"ChainLink{i}", link, new Vector3(0.14f, 0.05f, 0.025f), GB.Metal, new Vector3(i % 2 == 0 ? 0f : 90f, 0f, 0f), false);
                }
                GB.Box(pair, "Padlock", new Vector3(0f, 0.84f, -0.16f), new Vector3(0.12f, 0.16f, 0.06f), GB.Metal, collider: false);
                GB.Box(pair, "Transom", new Vector3(0f, 3.25f, 0.03f), new Vector3(2.3f, 0.7f, 0.02f), glowStreet, collider: false);
            }

            GB.Box(g, "MarqueeStrip", new Vector3(0f, 3.95f, LobbyDepth - 0.05f), new Vector3(6.6f, 0.3f, 0.1f), brass, collider: false);
            for (int i = 0; i < 12; i++)
            {
                bool lit = i == 2 || i == 7 || i == 10;
                GB.Sphere(g, $"MarqueeBulb{i:00}", new Vector3(-3.025f + i * 0.55f, 3.95f, LobbyDepth - 0.13f), 0.06f, lit ? glowWarm : deadBulb, false);
            }
            GB.PointLight(g, "StreetDoors_Light", new Vector3(0f, 3.4f, LobbyDepth - 0.8f), WarmLight, 5f, 0.8f);
            Poster(g, "SalvageNotice", new Vector3(4.3f, 1.6f, LobbyDepth - 0.03f), 180f, 0.6f, 0.8f, cream, glowRed);

            GB.Marker(g, MarkerKind.PlayerStart, new Vector3(0f, 0f, 18.4f), 180f, "Just inside the chained street doors, facing the theatre");
        }

        /// Island ticket booth near the street doors (circle it); hide under the cash desk.
        static void BuildTicketBooth(Transform lobby)
        {
            var b = GB.Group(lobby, "TicketBooth", new Vector3(4.6f, 0f, 15.2f));   // window faces +Z (the street doors)
            GB.Box(b, "CounterFront", new Vector3(0f, 0.5f, 0.92f), new Vector3(3f, 1f, 0.16f), cream);
            GB.Box(b, "CounterTop", new Vector3(0f, 1.025f, 1f), new Vector3(3.2f, 0.05f, 0.45f), laminate);
            GB.Box(b, "Side_W", new Vector3(-1.42f, 1.3f, 0f), new Vector3(0.16f, 2.6f, 2f), cream);
            GB.Box(b, "Side_E", new Vector3(1.42f, 1.3f, 0f), new Vector3(0.16f, 2.6f, 2f), cream);
            GB.Box(b, "Window", new Vector3(0f, 1.7f, 0.92f), new Vector3(2.68f, 1.3f, 0.06f), GB.Glass);
            GB.Box(b, "SpeakHole", new Vector3(0f, 1.42f, 0.96f), new Vector3(0.2f, 0.2f, 0.01f), brass, collider: false);
            GB.Box(b, "Header", new Vector3(0f, 2.45f, 0.92f), new Vector3(3f, 0.2f, 0.16f), cream);
            GB.Box(b, "Roof", new Vector3(0f, 2.6f, 0f), new Vector3(3.3f, 0.1f, 2.3f), laminate);
            GB.Box(b, "Sign", new Vector3(0f, 2.98f, 0.95f), new Vector3(2f, 0.5f, 0.1f), cream, collider: false);
            GB.Box(b, "SignLetters", new Vector3(0f, 2.98f, 1.01f), new Vector3(1.6f, 0.2f, 0.02f), glowWarm, collider: false);
            GB.Box(b, "CashDesk", new Vector3(0f, 0.9f, 0.58f), new Vector3(2.6f, 0.05f, 0.52f), laminate);
            GB.Box(b, "CashRegister", new Vector3(0.7f, 1.05f, 0.6f), new Vector3(0.4f, 0.25f, 0.35f), brass, collider: false);
            GB.Cylinder(b, "Stool", new Vector3(0.6f, 0.35f, -0.15f), 0.18f, 0.7f, redVelvet);
            GB.Box(b, "TicketDispenser", new Vector3(-0.9f, 1.13f, 1.08f), new Vector3(0.28f, 0.16f, 0.22f), GB.Metal, collider: false);
            GB.Cylinder(b, "TicketRoll", new Vector3(-0.9f, 1.25f, 1.08f), 0.07f, 0.22f, laminate, new Vector3(0f, 0f, 90f), false);

            GB.Marker(lobby, MarkerKind.StoryProp, new Vector3(3.7f, 1.05f, 16.28f), 0f,
                "Ticket roll on the booth counter: 21 DEC 1997 - THE LONGEST NIGHT, LIVE SOLSTICE SPECIAL - every seat sold");
            GB.Marker(lobby, MarkerKind.HidingSpot, new Vector3(4.6f, 0f, 15.78f), 180f, "Under the cash desk inside the ticket booth (enter from its open back)");
        }

        /// Concession stand on the +X wall: island front counter with a staff aisle behind it, so it can be circled.
        static void BuildConcession(Transform lobby)
        {
            var c = GB.Group(lobby, "ConcessionStand");
            GB.Box(c, "FrontCounter", new Vector3(8.05f, 0.475f, 13.2f), new Vector3(0.9f, 0.95f, 4.4f), laminate);
            GB.Box(c, "FrontCounterTop", new Vector3(8.05f, 0.975f, 13.2f), new Vector3(1.05f, 0.05f, 4.5f), brass);
            GB.Box(c, "CandyCase", new Vector3(8.05f, 1.2f, 12.3f), new Vector3(0.8f, 0.4f, 1.8f), GB.Glass);
            GB.Box(c, "CandyBoxes", new Vector3(8.05f, 1.06f, 12.3f), new Vector3(0.6f, 0.1f, 1.5f), candy, collider: false);
            GB.Box(c, "Till", new Vector3(8.25f, 1.1f, 14.9f), new Vector3(0.35f, 0.2f, 0.3f), GB.Metal, collider: false);

            GB.Box(c, "BackCounter", new Vector3(10.675f, 0.45f, 13.6f), new Vector3(0.65f, 0.9f, 6.4f), laminate);
            GB.Box(c, "BackCounterTop", new Vector3(10.65f, 0.925f, 13.6f), new Vector3(0.7f, 0.05f, 6.5f), brass);
            GB.Box(c, "PopcornCart", new Vector3(10.62f, 1.35f, 12.4f), new Vector3(0.62f, 0.75f, 0.8f), GB.Glass);
            GB.Box(c, "PopcornGlow", new Vector3(10.62f, 1.1f, 12.4f), new Vector3(0.5f, 0.22f, 0.68f), glowPopcorn, collider: false);
            GB.Box(c, "PopcornRoof", new Vector3(10.62f, 1.76f, 12.4f), new Vector3(0.68f, 0.08f, 0.86f), laminate, collider: false);
            GB.Box(c, "SodaFountain", new Vector3(10.7f, 1.2f, 14.9f), new Vector3(0.5f, 0.5f, 0.9f), GB.Metal);
            for (int i = 0; i < 3; i++)
                GB.Box(c, $"SodaTap{i}", new Vector3(10.4f, 1.25f, 14.6f + i * 0.3f), new Vector3(0.1f, 0.12f, 0.06f), brass, collider: false);
            GB.Box(c, "MenuBoard", new Vector3(10.96f, 2.9f, 13.6f), new Vector3(0.06f, 1.1f, 4.2f), GB.MatteBlack, collider: false);
            for (int i = 0; i < 4; i++)
                GB.Box(c, $"MenuLine{i}", new Vector3(10.92f, 3.25f - i * 0.22f, 13.6f), new Vector3(0.01f, 0.08f, 3.2f - i * 0.4f), i == 0 ? glowWarm : paper, collider: false);
            GB.PointLight(c, "PopcornWarmer_Light", new Vector3(10.2f, 2.1f, 12.6f), new Color(1f, 0.78f, 0.45f), 5f, 0.9f);

            GB.Marker(c, MarkerKind.BatterySpawn, new Vector3(7.75f, 1f, 14.6f), 270f, "On the concession counter by the till");
        }

        /// Round red-velvet banquette under the papier-mache Glen Moon: the lobby's central loop and sightline breaker.
        static void BuildMoonBanquette(Transform lobby)
        {
            var m = GB.Group(lobby, "MoonBanquette", new Vector3(3f, 0f, 9.5f));
            GB.Cylinder(m, "Seat", new Vector3(0f, 0.225f, 0f), 1.9f, 0.45f, redVelvet);
            GB.Cylinder(m, "Backrest", new Vector3(0f, 0.55f, 0f), 1.25f, 1.1f, redVelvet);
            GB.Cylinder(m, "Cap", new Vector3(0f, 1.14f, 0f), 1.32f, 0.08f, brass);
            GB.Cylinder(m, "Column", new Vector3(0f, 2.25f, 0f), 0.14f, 2.3f, brass);
            GB.Sphere(m, "GlenMoon", new Vector3(0f, 4.15f, 0f), 0.85f, glowMoon, false);
            GB.Ellipsoid(m, "Crater0", new Vector3(0.42f, 4.45f, -0.62f), new Vector3(0.32f, 0.26f, 0.1f), paper, new Vector3(-20f, 35f, 0f), false);
            GB.Ellipsoid(m, "Crater1", new Vector3(-0.55f, 3.95f, -0.6f), new Vector3(0.22f, 0.2f, 0.08f), paper, new Vector3(10f, -42f, 0f), false);
            GB.Ellipsoid(m, "Crater2", new Vector3(0.1f, 3.75f, 0.8f), new Vector3(0.26f, 0.2f, 0.08f), paper, new Vector3(15f, 170f, 0f), false);
            GB.PointLight(m, "Moon_Light", new Vector3(0f, 3.1f, 0f), new Color(1f, 0.85f, 0.62f), 7f, 1f);
        }

        static void BuildLobbyFurniture(Transform lobby)
        {
            Bench(lobby, "Bench_TheatreWall", new Vector3(-8f, 0f, 0.35f), 0f, 3.2f);
            Bench(lobby, "Bench_Pair_N", new Vector3(-2.9f, 0f, 5.25f), 180f, 2.4f);
            Bench(lobby, "Bench_Pair_S", new Vector3(-2.9f, 0f, 5.85f), 0f, 2.4f);
            DeadPalm(lobby, "Palm_W", new Vector3(-2.3f, 0f, 0.7f));
            DeadPalm(lobby, "Palm_E", new Vector3(2.3f, 0f, 0.7f));
            // plywood photo standee of Miss Hesper's lamp-post from the show's opening ("Lights low, voices soft")
            var post = GB.Group(lobby, "Standee_LampPost", new Vector3(-4.2f, 0f, 3f), 15f);   // greeting faces the street doors
            GB.Box(post, "Base", new Vector3(0f, 0.03f, 0f), new Vector3(0.7f, 0.06f, 0.7f), GB.Plywood);
            GB.Box(post, "Post", new Vector3(0f, 1.3f, 0f), new Vector3(0.14f, 2.5f, 0.14f), GB.Plywood);
            GB.Box(post, "Lantern", new Vector3(0f, 2.72f, 0f), new Vector3(0.4f, 0.45f, 0.4f), GB.MatteBlack, collider: false);
            GB.Cylinder(post, "GlowRing", new Vector3(0f, 2.72f, 0f), 0.24f, 0.04f, glowMint, false);
            GB.Box(post, "Greeting", new Vector3(0f, 1.75f, 0.1f), new Vector3(1.2f, 0.4f, 0.04f), cream, collider: false);
            GB.Box(post, "GreetingPaint", new Vector3(0f, 1.75f, 0.125f), new Vector3(1f, 0.12f, 0.01f), glowMoon, collider: false);
            Sputnik(lobby, "Chandelier_NW", new Vector3(-7f, 6.2f, 4.2f), 3);
            Sputnik(lobby, "Chandelier_SE", new Vector3(6.2f, 6.2f, 15.4f), -1);

            // "Who's Awake in the Glen?" display board facing the PlayerStart, under its own picture lamp
            var board = GB.Group(lobby, "WhosAwakeBoard", new Vector3(-2.6f, 0f, 15.4f));
            GB.Box(board, "Foot_W", new Vector3(-0.6f, 0.04f, 0f), new Vector3(0.12f, 0.08f, 0.7f), bronze);
            GB.Box(board, "Foot_E", new Vector3(0.6f, 0.04f, 0f), new Vector3(0.12f, 0.08f, 0.7f), bronze);
            GB.Box(board, "Panel", new Vector3(0f, 1.3f, 0f), new Vector3(1.5f, 2.1f, 0.08f), bronze);
            WhosAwakePoster(board, "Poster_WhosAwake", new Vector3(0f, 1.35f, 0.045f), 0f, 1f);
            GB.Box(board, "LampArm", new Vector3(0f, 2.42f, 0.2f), new Vector3(0.04f, 0.04f, 0.4f), brass, collider: false);
            GB.Box(board, "LampHood", new Vector3(0f, 2.42f, 0.4f), new Vector3(0.5f, 0.08f, 0.12f), brass, collider: false);
            GB.SpotLight(board, "PosterLamp_Light", new Vector3(0f, 2.36f, 0.42f), new Vector3(62f, 180f, 0f), WarmLight, 3.5f, 75f, 1.6f);
            GB.Marker(lobby, MarkerKind.StoryProp, new Vector3(-2.6f, 0f, 15.85f), 0f,
                "\"Who's Awake in the Glen?\" poster (lobby): each Gloamie's portrait beside its glockenspiel call-sign");

            // faded wall posters
            Poster(lobby, "Poster_Season1", new Vector3(-5f, 1.8f, 0.03f), 0f, 0.9f, 1.3f, paper, glowMint);
            Poster(lobby, "Poster_CountToTen", new Vector3(-8f, 1.95f, 0.03f), 0f, 0.9f, 1.2f, paper, glowGold);
            Poster(lobby, "Poster_TapAlong", new Vector3(5.6f, 1.8f, 0.03f), 0f, 0.9f, 1.3f, paper, glowLilac);
            Poster(lobby, "Poster_FriendlyLight", new Vector3(-LobbyHalf + 0.03f, 1.8f, 6.8f), 90f, 0.9f, 1.3f, paper, glowAmber);
            Poster(lobby, "Poster_StarShow", new Vector3(LobbyHalf - 0.03f, 5.6f, 5f), 270f, 1.4f, 1.9f, paper, glowCyan);
            Poster(lobby, "Poster_SnackTime", new Vector3(-6f, 1.8f, LobbyDepth - 0.03f), 180f, 0.9f, 1.3f, paper, glowSaddle);
            Poster(lobby, "Poster_CueTheTrap", new Vector3(7.2f, 1.8f, LobbyDepth - 0.03f), 180f, 0.9f, 1.3f, paper, glowPink);
            Poster(lobby, "Sign_CostumeShop", new Vector3(WorkshopEast + T + 0.03f, 2.9f, 10.8f), 90f, 1.3f, 0.35f, cream, null);

            GB.Marker(lobby, MarkerKind.PatrolPoint, new Vector3(-6.5f, 0f, 3.5f), 90f, "Lobby roam: by the theatre-wall bench");
            GB.Marker(lobby, MarkerKind.PatrolPoint, new Vector3(0f, 0f, 12.5f), 180f, "Lobby roam: on the runner, between doors");
            GB.Marker(lobby, MarkerKind.PatrolPoint, new Vector3(6.4f, 0f, 5.5f), 0f, "Lobby roam: foot of the booth stairs");
        }

        /// The building's main breaker on the -X wall, with glow spike tape on the floor in front of it.
        static void BuildBreaker(Transform lobby)
        {
            var b = GB.Group(lobby, "MainBreaker", new Vector3(-LobbyHalf, 0f, 4.2f), 90f);   // faces +X into the lobby
            GB.Box(b, "Cabinet", new Vector3(0f, 1.45f, 0.17f), new Vector3(1.5f, 2.3f, 0.34f), GB.Metal);
            GB.Box(b, "FacePlate", new Vector3(0f, 1.5f, 0.35f), new Vector3(1.3f, 1.9f, 0.02f), bronze, collider: false);
            for (int r = 0; r < 4; r++)
                GB.Box(b, $"BreakerRow{r}", new Vector3(-0.2f, 1f + r * 0.28f, 0.37f), new Vector3(0.7f, 0.16f, 0.04f), GB.MatteBlack, collider: false);
            GB.Box(b, "SwitchPivot", new Vector3(0.45f, 1.6f, 0.4f), new Vector3(0.16f, 0.12f, 0.1f), GB.Metal, collider: false);
            GB.Box(b, "SwitchHandle", new Vector3(0.45f, 1.36f, 0.46f), new Vector3(0.08f, 0.5f, 0.08f), laminate, new Vector3(-25f, 0f, 0f), false);
            GB.Sphere(b, "PilotLamp", new Vector3(-0.45f, 2.3f, 0.37f), 0.045f, glowRed, false);
            GB.Box(b, "Label", new Vector3(0.05f, 2.3f, 0.37f), new Vector3(0.6f, 0.12f, 0.01f), cream, collider: false);
            for (int i = 0; i < 3; i++)
                GB.Box(b, $"Conduit{i}", new Vector3(-0.4f + i * 0.4f, 5.2f, 0.06f), new Vector3(0.07f, 5.5f, 0.07f), GB.Metal, collider: false);
            GB.Box(b, "SpikeTape", new Vector3(0f, 0.006f, 0.85f), new Vector3(1.4f, 0.012f, 0.06f), glowTape, collider: false);

            GB.Marker(lobby, MarkerKind.Breaker, new Vector3(-10.55f, 0f, 4.2f), 90f, "Main breaker: the Starhouse building supply");
        }

        // ================================================================== costume workshop (dark)
        static void BuildWorkshop(Transform w)
        {
            float x0 = -LobbyHalf, x1 = WorkshopEast, z0 = WorkshopNorth, z1 = LobbyDepth, h = WorkshopHeight;
            float ex = x1 + T * 0.5f;   // -5.35: centre line of the wall facing the lobby
            float nz = z0 - T * 0.5f;   // 8.45: centre line of the theatre-side wall
            float eStart = z0 - T;      // 8.3
            GB.Wall(w, "Wall_Lobby", new Vector3(ex, 0f, eStart), new Vector3(ex, 0f, z1), h, T, workshopWall,
                new Opening(10.8f - eStart, 1.6f, 0f, 2.4f),   // main door from the lobby, z 10.0..11.6
                new Opening(18.2f - eStart, 1.2f, 0f, 1f));    // child-sized costume hatch (crawl), z 17.6..18.8
            GB.Wall(w, "Wall_Theatre", new Vector3(x0, 0f, nz), new Vector3(x1 + T, 0f, nz), h, T, workshopWall,
                new Opening(-8.6f - x0, 1.6f, 0f, 2.4f));      // back door to the lobby's north-west corner, x -9.4..-7.8
            GB.Box(w, "Ceiling", new Vector3((x0 + x1 + T) * 0.5f, h + 0.15f, (eStart + z1) * 0.5f), new Vector3(x1 + T - x0, 0.3f, z1 - eStart), GB.DarkConcrete);
            GB.Box(w, "FloorBoards", new Vector3((x0 + x1) * 0.5f, 0.006f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 0.012f, z1 - z0), workshopFloor, collider: false);
            GB.Box(w, "DeadTubeLight", new Vector3(-8.3f, h - 0.06f, 14f), new Vector3(0.25f, 0.08f, 2.4f), deadBulb, collider: false);
            GB.Box(w, "HatchFlap", new Vector3(x1 + T + 0.02f, 0.5f, 18.2f), new Vector3(0.02f, 0.96f, 1.15f), cordBlue, collider: false);

            // spare Gloamie heads on shelving along the -X wall (each Gloamie twice)
            HeadShelf(w, "HeadShelf_N", new Vector3(x0 + 0.28f, 0f, 11.1f), 3.8f, 0);
            HeadShelf(w, "HeadShelf_S", new Vector3(x0 + 0.28f, 0f, 15.3f), 3.8f, 3);

            // costume racks: hiding spots (Tapwell probes "between hanging costumes")
            CostumeRack(w, "CostumeRack_1", new Vector3(-8.4f, 0f, 12.4f), 0f, new[] { feltGreen, tweed, quilt, knitTeal, crimson });
            CostumeRack(w, "CostumeRack_2", new Vector3(-8.4f, 0f, 15.8f), 0f, new[] { knitTeal, crimson, feltGreen, quilt, tweed });
            CostumeRack(w, "CostumeRack_3", new Vector3(-6.6f, 0f, 19.2f), 90f, new[] { quilt, cordBlue, tweed, feltGreen, crimson });
            GB.Marker(w, MarkerKind.HidingSpot, new Vector3(-8.4f, 0f, 12.4f), 90f, "In among the hanging costumes, rack 1");
            GB.Marker(w, MarkerKind.HidingSpot, new Vector3(-8.4f, 0f, 15.8f), 270f, "In among the hanging costumes, rack 2");
            GB.Marker(w, MarkerKind.HidingSpot, new Vector3(-6.6f, 0f, 19.2f), 180f, "In among the hanging costumes, rack 3 (beside the costume hatch)");

            SewingTable(w, "SewingTable_1", new Vector3(-5.95f, 0f, 13.2f));
            SewingTable(w, "SewingTable_2", new Vector3(-5.95f, 0f, 15.6f));
            GB.Marker(w, MarkerKind.BatterySpawn, new Vector3(-6.2f, 0.85f, 13.3f), 270f, "On sewing table 1, beside the machine");

            DressForm(w, "DressForm_1", new Vector3(-10.1f, 0f, 18.3f), 60f, 1f, feltGreen);
            DressForm(w, "DressForm_2", new Vector3(-9.2f, 0f, 19.25f), 20f, 1f, quilt);
            DressForm(w, "DressForm_3", new Vector3(-8.2f, 0f, 18.1f), -30f, 1f, null);
            DressForm(w, "DressForm_Child", new Vector3(-10.1f, 0f, 9.1f), 135f, 0.62f, cordBlue);
            GB.Marker(w, MarkerKind.StoryProp, new Vector3(-10.1f, 0f, 9.1f), 135f,
                "Child-size dress form in a powder-blue corduroy romper, name patch PEEKINS: far too small for an adult performer");

            // performers' sign-in lectern under the make-up mirror, just inside the main door
            var lec = GB.Group(w, "SignInLectern", new Vector3(-6.2f, 0f, 9.3f));   // reading side faces +Z
            GB.Box(lec, "Post", new Vector3(0f, 0.5f, 0f), new Vector3(0.36f, 1f, 0.3f), GB.Wood);
            GB.Box(lec, "Desk", new Vector3(0f, 1.04f, 0.04f), new Vector3(0.56f, 0.05f, 0.42f), GB.Wood, new Vector3(12f, 0f, 0f));
            GB.Box(lec, "SignInSheet", new Vector3(0f, 1.07f, 0.04f), new Vector3(0.22f, 0.01f, 0.3f), cream, new Vector3(12f, 0f, 0f), false);
            GB.Marker(w, MarkerKind.StoryProp, new Vector3(-6.2f, 1.08f, 9.34f), 0f, "sign-in sheet: the line next to PEEKINS is blank");

            var mir = GB.Group(w, "MakeupMirror", new Vector3(-6.6f, 1.65f, z0 + 0.02f));   // on the theatre-side wall, facing +Z
            GB.Box(mir, "Frame", Vector3.zero, new Vector3(1.3f, 1f, 0.04f), GB.Wood, collider: false);
            GB.Box(mir, "Glass", new Vector3(0f, 0f, 0.025f), new Vector3(1.1f, 0.8f, 0.01f), mirror, collider: false);
            for (int i = 0; i < 6; i++)
                GB.Sphere(mir, $"Bulb{i}", new Vector3(i < 3 ? -0.6f : 0.6f, -0.3f + (i % 3) * 0.3f, 0.05f), 0.04f, i == 4 ? glowWarm : deadBulb, false);

            GB.Marker(w, MarkerKind.Handprint, new Vector3(ex, 0f, 18.2f), 90f, "Costume hatch, 1.0 m tall: small UV handprints lead through from the lobby");
        }

        // ================================================================== projection booth (safe room, save point)
        /// Above the corridor: interior x -3.2..3.2, floor y 4.5, ceiling 7.7. Entry passage (x 1.0..3.2) from the mezzanine door,
        /// film vault in the south-west corner, main room to the north with the projector at the theatre's booth window.
        static void BuildBooth(Transform b)
        {
            float y = BoothY, wallH = BoothCeiling - BoothY;   // 3.2
            float zs = -T;                                      // inner face of the booth's south wall
            float north = BoothNorth;                           // side walls and ceiling stop at the booth zone's edge, against the round wall

            GB.Box(b, "FloorSlab", new Vector3(0f, y - 0.15f, BoothNorth * 0.5f), new Vector3(2f * BoothHalf, 0.3f, -BoothNorth), boothFloor);
            GB.Box(b, "SideWall_E", new Vector3(BoothHalf - T * 0.5f, y + wallH * 0.5f, (zs + north) * 0.5f), new Vector3(T, wallH, zs - north), boothWall);
            GB.Box(b, "SideWall_W", new Vector3(-BoothHalf + T * 0.5f, y + wallH * 0.5f, (zs + north) * 0.5f), new Vector3(T, wallH, zs - north), boothWall);
            GB.Box(b, "Ceiling", new Vector3(0f, BoothCeiling + 0.15f, north * 0.5f), new Vector3(2f * BoothHalf, 0.3f, -north), GB.DarkConcrete);
            // south wall = the lobby's theatre-side wall above the slab, with the door from the mezzanine (x 1.3..2.9).
            // The north side is left open: the theatre's round wall closes it, with the booth window at y 5.4-6.9.
            GB.Wall(b, "SouthWall", new Vector3(-BoothHalf, y, -T * 0.5f), new Vector3(BoothHalf, y, -T * 0.5f), LobbyHeight - y, T, boothWall,
                new Opening(BoothDoorX + BoothHalf, 1.6f, 0f, 2.4f));
            Poster(b, "Sign_Projection", new Vector3(BoothDoorX, y + 2.75f, 0.02f), 0f, 1.2f, 0.3f, cream, null);
            GB.Box(b, "SconceBracket", new Vector3(0.7f, y + 2f, 0.07f), new Vector3(0.08f, 0.22f, 0.14f), brass, collider: false);
            GB.GlowLamp(b, "Sconce", new Vector3(0.7f, y + 2.14f, 0.17f), WarmLight, 0.08f, 4.5f, 0.9f);
            GB.Box(b, "ExitSign", new Vector3(BoothDoorX, y + 2.65f, zs - 0.03f), new Vector3(0.5f, 0.18f, 0.05f), glowRed, collider: false);
            GB.Cylinder(b, "FireExtinguisher", new Vector3(3.08f, y + 0.35f, -1.2f), 0.09f, 0.6f, laminate);

            // ---- FILM VAULT: interior x -3.2..0.6, z -3.0..-0.3; round door in its north wall
            var vault = GB.Group(b, "FilmVault");
            GB.Box(vault, "Wall_E", new Vector3(0.8f, y + wallH * 0.5f, -1.85f), new Vector3(0.4f, wallH, 3.1f), vaultSteel);
            GB.Wall(vault, "Wall_N", new Vector3(-3.2f, y, -3.2f), new Vector3(1f, y, -3.2f), wallH, 0.4f, vaultSteel,
                new Opening(VaultDoorX + 3.2f, 1.5f, 0f, 2.2f));
            GB.Box(vault, "Threshold", new Vector3(VaultDoorX, y + 0.01f, -3.2f), new Vector3(1.5f, 0.02f, 0.4f), brass, collider: false);
            // the heavy round door, swung right round and standing open against the vault front
            GB.Cylinder(vault, "RoundDoor", new Vector3(-2.15f, y + 1.06f, -3.6f), 1.04f, 0.32f, vaultSteel, new Vector3(90f, 0f, 0f));
            GB.Box(vault, "Hinge", new Vector3(-1.1f, y + 1.1f, -3.55f), new Vector3(0.1f, 1.4f, 0.3f), GB.Metal);
            GB.Cylinder(vault, "WheelHub", new Vector3(-2.15f, y + 1.06f, -3.81f), 0.12f, 0.1f, brass, new Vector3(90f, 0f, 0f), false);
            GB.Box(vault, "WheelSpoke0", new Vector3(-2.15f, y + 1.06f, -3.85f), new Vector3(0.9f, 0.05f, 0.05f), brass, new Vector3(0f, 0f, 45f), false);
            GB.Box(vault, "WheelSpoke1", new Vector3(-2.15f, y + 1.06f, -3.85f), new Vector3(0.9f, 0.05f, 0.05f), brass, new Vector3(0f, 0f, -45f), false);
            Poster(vault, "Sign_FilmVault", new Vector3(VaultDoorX, y + 2.55f, -3.42f), 180f, 1f, 0.28f, cream, glowRed);
            ReelShelf(vault, "ReelShelf_W", new Vector3(-2.975f, y, -1.85f), 0f, 2.2f);     // along the -X wall, open to +X
            ReelShelf(vault, "ReelShelf_S", new Vector3(-1.35f, y, -0.525f), 90f, 3.7f);    // along the south wall, open to -Z
            GB.Box(vault, "ReelTable", new Vector3(-1.9f, y + 0.425f, -1.7f), new Vector3(0.55f, 0.85f, 0.55f), vaultSteel);
            GB.Box(vault, "ReelTableTop", new Vector3(-1.9f, y + 0.875f, -1.7f), new Vector3(0.65f, 0.05f, 0.65f), brass);
            GB.GlowLamp(vault, "CageLamp", new Vector3(-1f, y + 2.85f, -0.62f), new Color(1f, 0.62f, 0.3f), 0.07f, 3.5f, 0.7f);
            GB.Marker(vault, MarkerKind.ReelSpawn, new Vector3(-1.9f, y + 0.9f, -1.7f), 180f, "Reel 13 — locked in the vault");

            // ---- big 16 mm projector aimed through the theatre's booth window
            var proj = GB.Group(b, "Projector16mm", new Vector3(0f, y, -8f));   // lens points -Z
            GB.Box(proj, "Pedestal", new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 1.4f), projectorGrey);
            GB.Box(proj, "Body", new Vector3(0f, 1.25f, -0.05f), new Vector3(0.62f, 0.7f, 1.1f), projectorGrey);
            GB.Box(proj, "Lamphouse", new Vector3(0.43f, 1.3f, 0.1f), new Vector3(0.26f, 0.6f, 0.5f), projectorGrey);
            GB.Box(proj, "LampVents", new Vector3(0.565f, 1.3f, 0.1f), new Vector3(0.02f, 0.36f, 0.3f), glowLamphouse, collider: false);
            GB.Cylinder(proj, "Chimney", new Vector3(0.43f, 2.4f, 0.1f), 0.1f, 1.6f, GB.Metal, false);
            GB.Cylinder(proj, "Lens", new Vector3(0f, 1.3f, -0.85f), 0.1f, 0.5f, GB.MatteBlack, new Vector3(90f, 0f, 0f), false);
            GB.Box(proj, "FeedArm", new Vector3(0f, 1.85f, -0.5f), new Vector3(0.05f, 0.4f, 0.06f), GB.Metal, collider: false);
            GB.Cylinder(proj, "FeedReel", new Vector3(0f, 2.33f, -0.5f), 0.42f, 0.07f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Box(proj, "TakeUpArm", new Vector3(0f, 1.8f, 0.45f), new Vector3(0.05f, 0.3f, 0.06f), GB.Metal, collider: false);
            GB.Cylinder(proj, "TakeUpReel", new Vector3(0f, 2.2f, 0.45f), 0.38f, 0.07f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            // guard rail in front of the window port (sill is 0.9 m above the booth floor, the theatre floor 4.5 m below)
            GB.Box(b, "PortRail_Top", new Vector3(0f, y + 1.15f, -9.3f), new Vector3(5.5f, 0.06f, 0.06f), brass);
            GB.Box(b, "PortRail_Mid", new Vector3(0f, y + 0.55f, -9.3f), new Vector3(5.5f, 0.06f, 0.06f), brass);
            for (int i = 0; i < 3; i++)
                GB.Box(b, $"PortRailPost{i}", new Vector3(-2.72f + i * 2.72f, y + 0.58f, -9.3f), new Vector3(0.06f, 1.16f, 0.06f), brass);
            GB.Cylinder(b, "CanStack0", new Vector3(1.9f, y + 0.18f, -8.9f), 0.19f, 0.36f, GB.Metal);
            GB.Cylinder(b, "CanStack1", new Vector3(2.35f, y + 0.12f, -8.75f), 0.19f, 0.24f, GB.Metal);

            // ---- the booth camera that shot Reel 13, still on its tripod at the window
            var cam = GB.Group(b, "BoothCamera", new Vector3(-2f, y, -8.5f));
            GB.Cylinder(cam, "TripodColumn", new Vector3(0f, 0.65f, 0f), 0.04f, 1.3f, GB.Metal);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f + 30f;
                GB.Box(cam, $"TripodLeg{i}", GB.Polar(a, 0.22f, 0.55f), new Vector3(0.03f, 1.19f, 0.03f), GB.Metal, new Vector3(-22f, a, 0f), false);
            }
            GB.Box(cam, "CameraBody", new Vector3(0f, 1.42f, 0f), new Vector3(0.22f, 0.3f, 0.42f), GB.MatteBlack);
            GB.Cylinder(cam, "EmptyMagazine", new Vector3(0f, 1.74f, 0.05f), 0.16f, 0.1f, GB.MatteBlack, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(cam, "CameraLens", new Vector3(0f, 1.42f, -0.3f), 0.05f, 0.2f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Sphere(cam, "TallyLamp", new Vector3(0.12f, 1.52f, -0.15f), 0.02f, glowRed, false);
            GB.Marker(b, MarkerKind.StoryProp, new Vector3(-2f, y, -8.5f), 180f,
                "Booth camera on its tripod, still aimed at the bleachers: its 16 mm magazine is empty - Reel 13 was shot from here");

            // ---- rewind bench and booth monitors along the +X wall
            var bench = GB.Group(b, "RewindBench", new Vector3(2.825f, y, -6.2f));   // worked from the -X side
            GB.Box(bench, "Cabinet", new Vector3(0f, 0.45f, 0f), new Vector3(0.75f, 0.9f, 3.2f), GB.Wood);
            GB.Box(bench, "Top", new Vector3(-0.02f, 0.925f, 0f), new Vector3(0.79f, 0.05f, 3.3f), laminate);
            for (int s = 0; s < 2; s++)
            {
                float z = s == 0 ? -1.3f : 1.3f;
                GB.Box(bench, $"RewindPost{s}", new Vector3(-0.15f, 1.12f, z), new Vector3(0.05f, 0.34f, 0.05f), GB.Metal, collider: false);
                GB.Cylinder(bench, $"RewindReel{s}", new Vector3(-0.15f, 1.32f, z), 0.28f, 0.04f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            }
            GB.Box(bench, "Splicer", new Vector3(-0.2f, 0.99f, 0.85f), new Vector3(0.14f, 0.08f, 0.3f), GB.Metal, collider: false);
            for (int i = 0; i < 3; i++)
            {
                float z = -0.6f + i * 0.6f;
                GB.Box(bench, $"Monitor{i}", new Vector3(0.12f, 1.15f, z), new Vector3(0.45f, 0.4f, 0.48f), GB.MatteBlack);
                GB.Box(bench, $"Screen{i}", new Vector3(-0.11f, 1.16f, z), new Vector3(0.01f, 0.28f, 0.36f), glowScreen, collider: false);
            }
            GB.Cylinder(b, "BenchStool", new Vector3(1.95f, y + 0.3f, -6.4f), 0.2f, 0.6f, redVelvet);

            // ---- the warm lamp that makes this the safe room
            GB.Box(b, "PendantCord", new Vector3(0.2f, BoothCeiling - 0.38f, -5.6f), new Vector3(0.02f, 0.76f, 0.02f), GB.MatteBlack, collider: false);
            GB.Cylinder(b, "PendantShade", new Vector3(0.2f, y + 2.4f, -5.6f), 0.22f, 0.18f, brass, false);
            GB.GlowLamp(b, "PendantLamp", new Vector3(0.2f, y + 2.27f, -5.6f), LampLight, 0.07f, 5.5f, 1.3f);

            // ---- the projectionist's cot (save point) with the booth copy of the call-sign poster above its head
            var cot = GB.Group(b, "Cot", new Vector3(-2.8f, y, -5.9f));
            GB.Box(cot, "Frame", new Vector3(0f, 0.2f, 0f), new Vector3(0.74f, 0.4f, 1.9f), GB.Metal); // same footprint as the mattress, so sitting on it always works
            GB.Box(cot, "Mattress", new Vector3(0f, 0.45f, 0f), new Vector3(0.74f, 0.1f, 1.9f), GB.Cloth);
            GB.Box(cot, "Blanket", new Vector3(0.02f, 0.515f, 0.25f), new Vector3(0.78f, 0.03f, 1.2f), blanket, collider: false);
            GB.Ellipsoid(cot, "Pillow", new Vector3(0f, 0.56f, -0.72f), new Vector3(0.5f, 0.12f, 0.32f), GB.Cloth, collider: false);
            GB.Marker(b, MarkerKind.SavePoint, new Vector3(-2.8f, y + 0.5f, -5.9f), 90f, "Projectionist's cot: save point, and where you wake after a catch");

            WhosAwakePoster(b, "Poster_WhosAwake_Booth", new Vector3(-3.17f, y + 1.6f, -7.8f), 90f, 0.8f);
            GB.Marker(b, MarkerKind.StoryProp, new Vector3(-2.95f, y, -7.8f), 90f, "\"Who's Awake in the Glen?\" poster (booth copy), pinned up by the cot");
        }

        // ================================================================== helpers
        /// Framed wall poster; its face looks along the group's +Z (yaw 0 faces +Z). Decor only.
        static void Poster(Transform parent, string name, Vector3 center, float yaw, float w, float h, Material face, Material accent)
        {
            var p = GB.Group(parent, name, center, yaw);
            GB.Box(p, "Frame", Vector3.zero, new Vector3(w + 0.1f, h + 0.1f, 0.03f), brass, collider: false);
            GB.Box(p, "Paper", new Vector3(0f, 0f, 0.02f), new Vector3(w, h, 0.01f), face, collider: false);
            if (accent != null)
                GB.Box(p, "Accent", new Vector3(0f, h * 0.22f, 0.03f), new Vector3(w * 0.6f, h * 0.12f, 0.01f), accent, collider: false);
        }

        /// "Who's Awake in the Glen?": six Gloamie portraits, each beside its call-sign notes in that Gloamie's glow colour.
        static void WhosAwakePoster(Transform parent, string name, Vector3 center, float yaw, float s)
        {
            var p = GB.Group(parent, name, center, yaw);
            GB.Box(p, "Frame", Vector3.zero, new Vector3(1.3f * s, 1.8f * s, 0.03f), brass, collider: false);
            GB.Box(p, "Paper", new Vector3(0f, 0f, 0.02f), new Vector3(1.2f * s, 1.7f * s, 0.01f), cream, collider: false);
            GB.Box(p, "Title", new Vector3(0f, 0.68f * s, 0.03f), new Vector3(1f * s, 0.16f * s, 0.01f), glowMoon, collider: false);
            for (int i = 0; i < 6; i++)
            {
                float row = (0.42f - i * 0.21f) * s;
                GB.Box(p, $"Portrait{i}", new Vector3(-0.38f * s, row, 0.03f), new Vector3(0.15f * s, 0.15f * s, 0.01f), callSignMats[i], collider: false);
                GB.Box(p, $"CallSign{i}", new Vector3(0.12f * s, row, 0.03f), new Vector3((0.36f + 0.06f * (i % 3)) * s, 0.04f * s, 0.01f), callSignMats[i], collider: false);
            }
        }

        /// Upholstered lobby bench; the sitter faces the group's +Z.
        static void Bench(Transform parent, string name, Vector3 pos, float yaw, float length)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Box(g, "Base", new Vector3(0f, 0.19f, 0f), new Vector3(length - 0.2f, 0.38f, 0.36f), GB.MatteBlack);
            GB.Box(g, "Seat", new Vector3(0f, 0.43f, 0f), new Vector3(length, 0.1f, 0.5f), redVelvet);
            GB.Box(g, "Back", new Vector3(0f, 0.78f, -0.23f), new Vector3(length, 0.6f, 0.08f), redVelvet);
        }

        static void DeadPalm(Transform parent, string name, Vector3 pos)
        {
            var g = GB.Group(parent, name, pos);
            GB.Cylinder(g, "Pot", new Vector3(0f, 0.3f, 0f), 0.3f, 0.6f, brass);
            GB.Cylinder(g, "Trunk", new Vector3(0f, 1.05f, 0f), 0.05f, 0.9f, GB.Wood, false);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f + 20f;
                GB.Box(g, $"Frond{i}", GB.Polar(a, 0.3f, 1.42f), new Vector3(0.14f, 0.02f, 0.75f), frond, new Vector3(35f, a, 0f), false);
            }
        }

        /// Dead 1960s starburst chandelier hanging from the lobby ceiling. litBulb < 0 = every bulb dead.
        static void Sputnik(Transform parent, string name, Vector3 pos, int litBulb)
        {
            var g = GB.Group(parent, name, pos);
            float drop = LobbyHeight - pos.y;
            GB.Box(g, "Drop", new Vector3(0f, drop * 0.5f, 0f), new Vector3(0.03f, drop, 0.03f), brass, collider: false);
            GB.Sphere(g, "Hub", Vector3.zero, 0.16f, brass, false);
            for (int i = 0; i < 3; i++)
                GB.Box(g, $"Rod{i}", Vector3.zero, new Vector3(0.03f, 0.03f, 1.4f), brass, i * 60f, false);
            for (int i = 0; i < 6; i++)
                GB.Sphere(g, $"Bulb{i}", GB.Polar(i * 60f, 0.72f), 0.06f, i == litBulb ? glowWarm : deadBulb, false);
        }

        /// Shelving of spare Gloamie heads against the -X wall: runs along Z, open towards +X. Six heads, kinds firstHead.. (mod 6).
        static void HeadShelf(Transform parent, string name, Vector3 pos, float length, int firstHead)
        {
            var g = GB.Group(parent, name, pos);
            const float d = 0.55f, h = 2.4f;
            GB.Box(g, "End0", new Vector3(0f, h * 0.5f, -length * 0.5f), new Vector3(d, h, 0.05f), GB.Plywood);
            GB.Box(g, "End1", new Vector3(0f, h * 0.5f, length * 0.5f), new Vector3(d, h, 0.05f), GB.Plywood);
            GB.Box(g, "Back", new Vector3(-d * 0.5f + 0.02f, h * 0.5f, 0f), new Vector3(0.04f, h, length), GB.Plywood, collider: false);
            GB.Box(g, "Top", new Vector3(0f, h - 0.025f, 0f), new Vector3(d, 0.05f, length), GB.Plywood);
            float[] levels = { 0.12f, 0.85f, 1.6f };
            for (int i = 0; i < levels.Length; i++)
            {
                GB.Box(g, $"Board{i}", new Vector3(0f, levels[i], 0f), new Vector3(d, 0.05f, length), GB.Plywood);
                for (int k = 0; k < 2; k++)
                    Head(g, $"Head{i}{k}", new Vector3(0f, levels[i] + 0.025f, (k == 0 ? -0.25f : 0.25f) * length), firstHead + i * 2 + k);
            }
        }

        /// A spare costume head resting on a shelf at `at` (bottom of the head), facing +X. Decor, no colliders.
        static void Head(Transform p, string name, Vector3 at, int kind)
        {
            switch (kind % 6)
            {
                case 0: // Miss Hesper: fuzzy pistachio dome, two huge glossy black vinyl eyes, no mouth
                    GB.Ellipsoid(p, name, at + new Vector3(0f, 0.29f, 0f), new Vector3(0.5f, 0.56f, 0.5f), headHesper, collider: false);
                    GB.Ellipsoid(p, name + "_EyeL", at + new Vector3(0.19f, 0.36f, -0.11f), new Vector3(0.1f, 0.2f, 0.15f), headVinyl, collider: false);
                    GB.Ellipsoid(p, name + "_EyeR", at + new Vector3(0.19f, 0.36f, 0.11f), new Vector3(0.1f, 0.2f, 0.15f), headVinyl, collider: false);
                    break;
                case 1: // Peekins: small oatmeal ball, white blaze, huge amber eye domes that still flare gold
                    GB.Ellipsoid(p, name, at + new Vector3(0f, 0.2f, 0f), new Vector3(0.42f, 0.4f, 0.44f), headPeekins, collider: false);
                    GB.Ellipsoid(p, name + "_Blaze", at + new Vector3(0.17f, 0.3f, 0f), new Vector3(0.1f, 0.2f, 0.06f), headCream, collider: false);
                    GB.Sphere(p, name + "_EyeL", at + new Vector3(0.16f, 0.22f, -0.1f), 0.08f, glowGold, false);
                    GB.Sphere(p, name + "_EyeR", at + new Vector3(0.16f, 0.22f, 0.1f), 0.08f, glowGold, false);
                    break;
                case 2: // Auntie Pinny: long low ink-black head, short curling snout, white-piped ears
                    GB.Ellipsoid(p, name, at + new Vector3(0.02f, 0.21f, 0f), new Vector3(0.66f, 0.42f, 0.42f), headPinny, collider: false);
                    GB.Ellipsoid(p, name + "_Snout", at + new Vector3(0.38f, 0.16f, 0f), new Vector3(0.2f, 0.13f, 0.14f), headPinny, collider: false);
                    GB.Ellipsoid(p, name + "_Ears", at + new Vector3(-0.1f, 0.43f, 0f), new Vector3(0.1f, 0.13f, 0.44f), headCream, collider: false);
                    break;
                case 3: // Dr. Tapwell: grizzled charcoal head with two huge pink-grey latex ear dishes
                    GB.Ellipsoid(p, name, at + new Vector3(0f, 0.22f, 0f), new Vector3(0.38f, 0.44f, 0.38f), headTapwell, collider: false);
                    GB.Ellipsoid(p, name + "_EarL", at + new Vector3(-0.02f, 0.46f, -0.22f), new Vector3(0.05f, 0.4f, 0.3f), headEar, new Vector3(-25f, 0f, 0f), false);
                    GB.Ellipsoid(p, name + "_EarR", at + new Vector3(-0.02f, 0.46f, 0.22f), new Vector3(0.05f, 0.4f, 0.3f), headEar, new Vector3(25f, 0f, 0f), false);
                    break;
                case 4: // Mrs. Deepwick: big glossy navy dome, amber lure bulb, cyan jaw dots
                    GB.Ellipsoid(p, name, at + new Vector3(0f, 0.31f, 0f), new Vector3(0.62f, 0.6f, 0.62f), headDeepwick, collider: false);
                    GB.Sphere(p, name + "_Lure", at + new Vector3(0.36f, 0.52f, 0f), 0.06f, glowAmber, false);
                    GB.Box(p, name + "_JawDots", at + new Vector3(0.29f, 0.17f, 0f), new Vector3(0.02f, 0.03f, 0.4f), glowCyan, collider: false);
                    break;
                default: // Digsley: crushed-velvet head, pink star-nose ring, painted cartoon goggle eyes
                    GB.Ellipsoid(p, name, at + new Vector3(0f, 0.23f, 0f), new Vector3(0.48f, 0.44f, 0.48f), headDigsley, collider: false);
                    GB.Cylinder(p, name + "_NoseRing", at + new Vector3(0.25f, 0.19f, 0f), 0.09f, 0.05f, glowPink, new Vector3(0f, 0f, 90f), false);
                    GB.Ellipsoid(p, name + "_Goggles", at + new Vector3(0.19f, 0.33f, 0f), new Vector3(0.06f, 0.12f, 0.3f), headWhite, collider: false);
                    break;
            }
        }

        /// Rolling costume rack with its rail along the group's Z. Costumes have no colliders, so you can push in among them to hide.
        static void CostumeRack(Transform parent, string name, Vector3 pos, float yaw, Material[] costumes)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Box(g, "Base", new Vector3(0f, 0.06f, 0f), new Vector3(0.7f, 0.12f, 2.1f), GB.MatteBlack);
            GB.Box(g, "Upright0", new Vector3(0f, 0.99f, -1f), new Vector3(0.06f, 1.74f, 0.06f), GB.Metal);
            GB.Box(g, "Upright1", new Vector3(0f, 0.99f, 1f), new Vector3(0.06f, 1.74f, 0.06f), GB.Metal);
            GB.Box(g, "Rail", new Vector3(0f, 1.88f, 0f), new Vector3(0.04f, 0.04f, 2.06f), GB.Metal, collider: false);
            for (int i = 0; i < costumes.Length; i++)
            {
                float z = -0.8f + 1.6f * i / Mathf.Max(1, costumes.Length - 1);
                float len = costumes[i] == cordBlue ? 0.7f : 1.1f + 0.15f * (i % 3);   // the powder-blue romper is child-sized
                GB.Box(g, $"Costume{i}", new Vector3(0f, 1.84f - len * 0.5f, z), new Vector3(0.55f, len, 0.18f), costumes[i], collider: false);
            }
        }

        /// Sewing table against the workshop's lobby wall; machine on the wall side, its pilot bulb still faintly lit.
        static void SewingTable(Transform parent, string name, Vector3 pos)
        {
            var g = GB.Group(parent, name, pos);
            GB.Box(g, "Top", new Vector3(0f, 0.82f, 0f), new Vector3(0.9f, 0.06f, 1.6f), GB.Wood);
            GB.Box(g, "Leg0", new Vector3(0f, 0.395f, -0.72f), new Vector3(0.8f, 0.79f, 0.06f), GB.Wood);
            GB.Box(g, "Leg1", new Vector3(0f, 0.395f, 0.72f), new Vector3(0.8f, 0.79f, 0.06f), GB.Wood);
            GB.Box(g, "MachineBed", new Vector3(0.1f, 0.9f, -0.2f), new Vector3(0.22f, 0.1f, 0.45f), GB.MatteBlack, collider: false);
            GB.Box(g, "MachineArm", new Vector3(0.17f, 1.05f, -0.2f), new Vector3(0.08f, 0.22f, 0.4f), GB.MatteBlack, collider: false);
            GB.Box(g, "MachineHead", new Vector3(0.1f, 1.12f, -0.38f), new Vector3(0.16f, 0.12f, 0.08f), GB.MatteBlack, collider: false);
            GB.Sphere(g, "PilotBulb", new Vector3(0.02f, 1.13f, -0.43f), 0.025f, glowWarm, false);
            GB.Cylinder(g, "FabricBolt", new Vector3(0.05f, 0.95f, 0.45f), 0.1f, 0.7f, feltGreen, new Vector3(0f, 0f, 90f), false);
        }

        /// Dress form on a pole; s scales it (0.62 = child-size). The torso is solid so you bump into it.
        static void DressForm(Transform parent, string name, Vector3 pos, float yaw, float s, Material outfit)
        {
            var g = GB.Group(parent, name, pos, yaw);
            GB.Cylinder(g, "Foot", new Vector3(0f, 0.03f, 0f), 0.24f * s, 0.06f, GB.MatteBlack);
            GB.Cylinder(g, "Pole", new Vector3(0f, 0.45f * s, 0f), 0.025f, 0.9f * s, GB.Metal, false);
            GB.Ellipsoid(g, "Torso", new Vector3(0f, 1.2f * s, 0f), new Vector3(0.42f, 0.62f, 0.3f) * s, dressForm);
            GB.Sphere(g, "NeckCap", new Vector3(0f, 1.53f * s, 0f), 0.06f * s, GB.Wood, false);
            if (outfit != null)
                GB.Box(g, "Outfit", new Vector3(0f, 1.02f * s, 0f), new Vector3(0.46f, 0.42f, 0.34f) * s, outfit, collider: false);
            if (s < 0.9f)
                GB.Box(g, "NamePatch", new Vector3(0f, 1.12f * s, 0.175f * s), new Vector3(0.1f, 0.05f, 0.01f), cream, collider: false);
        }

        /// Steel shelf of 16 mm film-can stacks in the vault: runs along the group's Z, open towards its +X.
        static void ReelShelf(Transform parent, string name, Vector3 pos, float yaw, float length)
        {
            var g = GB.Group(parent, name, pos, yaw);
            const float d = 0.45f, h = 2.4f;
            GB.Box(g, "End0", new Vector3(0f, h * 0.5f, -length * 0.5f + 0.025f), new Vector3(d, h, 0.05f), vaultSteel);
            GB.Box(g, "End1", new Vector3(0f, h * 0.5f, length * 0.5f - 0.025f), new Vector3(d, h, 0.05f), vaultSteel);
            for (int i = 0; i < 4; i++)
            {
                float level = 0.1f + i * 0.6f;
                GB.Box(g, $"Board{i}", new Vector3(0f, level, 0f), new Vector3(d, 0.05f, length - 0.1f), vaultSteel);
                if (i == 3) continue;   // top board stays bare
                for (int k = 0; k < 3; k++)
                {
                    int cans = 2 + (i + k + name.Length) % 4;
                    GB.Cylinder(g, $"Cans{i}{k}", new Vector3(0f, level + 0.025f + cans * 0.03f, (k - 1) * length * 0.3f), 0.19f, cans * 0.06f, GB.Metal, false);
                }
            }
        }
    }
}
