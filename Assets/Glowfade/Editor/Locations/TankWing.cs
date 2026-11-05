using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Mrs. Deepwick's flooded Deep-Dark Tank (Act VI "Follow a Friendly Light"), the Starhouse's old basement aquarium.
    /// The walkable floor stays at y 0 under a non-colliding water plane at 0.35. Dry concrete viewing galleries at y 1.0
    /// run along the west and north walls (ramp in the south-west, stairs in the north-east), with a camera pier reaching
    /// out over the water. The show's seabed set sits on a round island in the middle with a walk-in diving bell; glass
    /// tanks line the east wall and a dead-end pump room in the north-east holds the breaker.
    /// Deepwick's rule: real recharge lamps hang on brackets 2.2 m up on open walls; every dark dead end (pier tip, tank
    /// slot, stair nook, pump-room corner, inside the bell) is a LureNode where her bulb hangs at 1.7 m pretending to be one.
    public static class TankWing
    {
        const float Deck = 1.0f;        // top of the dry galleries and the pier
        const float IslandTop = 0.5f;   // top of the seabed-set island
        const float WaterY = 0.35f;     // flood surface

        static Material galleryMat, railMat, tapeMat, cyanMat, portholeMat, kelpGlowMat, uvMat, redMat,
            brassMat, feltMat, setTealMat, paintMat, pipeMat;

        public static void Build(Transform root)
        {
            Materials();
            var floor = GB.Mat("Tank_WetTile", new Color(0.09f, 0.13f, 0.14f), 0.7f);
            var walls = GB.Mat("Tank_TiledWall", new Color(0.12f, 0.16f, 0.17f), 0.45f);
            GB.Room(root, "Room", new Vector3(0f, 0f, 10f), new Vector3(22f, 7f, 20f), floor, walls, GB.DarkConcrete, new Doorway(Side.South));

            Flood(GB.Group(root, "Flood"));
            Galleries(GB.Group(root, "Galleries"));
            GalleryProps(GB.Group(root, "GalleryProps"));
            Island(GB.Group(root, "SeabedSet"));
            Tanks(GB.Group(root, "Aquarium"));
            PumpRoom(GB.Group(root, "PumpRoom"));
            StageManagerDesk(GB.Group(root, "StageManagerDesk", new Vector3(5.6f, 0f, 0f)));
            SouthWall(GB.Group(root, "SouthWall"));
            Pipes(GB.Group(root, "Pipework"));
            Patrol(GB.Group(root, "Patrol"));
        }

        static void Materials()
        {
            galleryMat = GB.Mat("Tank_GalleryConcrete", new Color(0.11f, 0.12f, 0.12f), 0.25f);
            railMat = GB.Mat("Tank_RailPaint", new Color(0.16f, 0.22f, 0.2f), 0.35f, 0.6f);
            brassMat = GB.Mat("Tank_Brass", new Color(0.42f, 0.31f, 0.13f), 0.5f, 0.85f);
            feltMat = GB.Mat("Tank_FeltKelp", new Color(0.1f, 0.2f, 0.11f), 0.05f);
            setTealMat = GB.Mat("Tank_SetTeal", new Color(0.1f, 0.27f, 0.26f), 0.12f);
            paintMat = GB.Mat("Tank_FadedCoral", new Color(0.42f, 0.22f, 0.18f), 0.1f);
            pipeMat = GB.Mat("Tank_RustPipe", new Color(0.24f, 0.13f, 0.08f), 0.3f, 0.5f);
            tapeMat = GB.Glow("Tank_GlowTape", new Color(0.15f, 0.85f, 0.8f), 1.2f);
            cyanMat = GB.Glow("Tank_GlowCyan", new Color(0.2f, 0.95f, 1f), 2f);
            portholeMat = GB.Glow("Tank_GlowPorthole", new Color(0.2f, 0.8f, 0.9f), 0.8f);
            kelpGlowMat = GB.Glow("Tank_GlowKelp", new Color(0.15f, 0.7f, 0.75f), 1f);
            uvMat = GB.Glow("Tank_GlowUV", new Color(0.55f, 0.2f, 1f), 2.5f);
            redMat = GB.Glow("Tank_GlowRed", new Color(1f, 0.15f, 0.1f), 1.2f);
        }

        // ---------------------------------------------------------------- flood water

        /// Non-colliding water over everything except a dry landing inside the door (|x| <= 3, z <= 3).
        static void Flood(Transform g)
        {
            float y = WaterY - 0.01f;
            GB.Box(g, "Water_Main", new Vector3(0f, y, 11.5f), new Vector3(22f, 0.02f, 17f), GB.Water, 0f, false);
            GB.Box(g, "Water_SW", new Vector3(-7f, y, 1.5f), new Vector3(8f, 0.02f, 3f), GB.Water, 0f, false);
            GB.Box(g, "Water_SE", new Vector3(7f, y, 1.5f), new Vector3(8f, 0.02f, 3f), GB.Water, 0f, false);
            // the dry landing stands just above the flood (one step up from the corridor)
            GB.Box(g, "DryLanding", new Vector3(0f, 0.19f, 1.5f), new Vector3(6f, 0.38f, 3f), galleryMat);

            // painted-plywood starfish floated off the old set
            GB.Box(g, "Flotsam_Star0", new Vector3(-3.2f, WaterY + 0.01f, 4.4f), new Vector3(0.5f, 0.03f, 0.5f), paintMat, 20f, false);
            GB.Box(g, "Flotsam_Star1", new Vector3(4.6f, WaterY + 0.01f, 13.4f), new Vector3(0.45f, 0.03f, 0.45f), paintMat, 55f, false);
            GB.Box(g, "Flotsam_Star2", new Vector3(-6.4f, WaterY + 0.01f, 15.6f), new Vector3(0.5f, 0.03f, 0.5f), paintMat, 75f, false);

            // floor drain under the water (gap that carries sound down)
            GB.Box(g, "FloorDrain", new Vector3(-5f, 0.01f, 7f), new Vector3(0.8f, 0.02f, 0.8f), GB.Metal, 0f, false);
            GB.Marker(g, MarkerKind.Hatch, new Vector3(-5f, 0f, 7f), 0f, "Floor drain grate under the flood, west of the set: open gap to the drains below");

            GB.Marker(g, MarkerKind.TrailBreaker, new Vector3(-5.5f, 0f, 5f), 0f, "Flood water, west pool: wading through it wipes a scent trail");
            GB.Marker(g, MarkerKind.TrailBreaker, new Vector3(5f, 0f, 15.5f), 0f, "Flood water, north-east pool by the pump room: wading through it wipes a scent trail");
        }

        // ---------------------------------------------------------------- dry galleries

        /// West gallery (x -11..-8.8), north gallery (z 17.6..20) and the camera pier, all solid concrete at y 1.0.
        static void Galleries(Transform g)
        {
            GB.Box(g, "Gallery_West", new Vector3(-9.9f, Deck * 0.5f, 11.75f), new Vector3(2.2f, Deck, 16.5f), galleryMat);
            GB.Box(g, "Gallery_North", new Vector3(-1.8f, Deck * 0.5f, 18.8f), new Vector3(14f, Deck, 2.4f), galleryMat);
            GB.Box(g, "CameraPier", new Vector3(-7.1f, Deck * 0.5f, 11f), new Vector3(3.4f, Deck, 1.6f), galleryMat);

            // south-west access: a 0.25 step out of the water, then a 15 degree ramp up to the west gallery
            GB.Box(g, "Ramp_FootStep", new Vector3(-9.9f, 0.125f, 0.55f), new Vector3(2.2f, 0.25f, 0.3f), galleryMat);
            GB.Ramp(g, "Ramp_West", new Vector3(-9.9f, 0.25f, 0.7f), 0f, 2.8f, 0.75f, 2.2f, galleryMat, 0.25f);
            GB.Box(g, "Ramp_Fill", new Vector3(-9.9f, 0.27f, 2.65f), new Vector3(2.2f, 0.54f, 1.7f), galleryMat);
            // north-east access: four 0.25 steps up to the north gallery
            GB.Stairs(g, "Stairs_NE", new Vector3(4.3f, 0f, 16.4f), 0f, 4, 0.25f, 0.3f, 1.8f, galleryMat);

            Rail(g, "Rail_WestA", new Vector3(-8.83f, Deck, 3.5f), new Vector3(-8.83f, Deck, 10.2f));
            Rail(g, "Rail_WestB", new Vector3(-8.83f, Deck, 11.8f), new Vector3(-8.83f, Deck, 17.6f));
            Rail(g, "Rail_PierS", new Vector3(-8.8f, Deck, 10.23f), new Vector3(-5.43f, Deck, 10.23f));
            Rail(g, "Rail_PierN", new Vector3(-8.8f, Deck, 11.77f), new Vector3(-5.43f, Deck, 11.77f));
            Rail(g, "Rail_PierEnd", new Vector3(-5.43f, Deck, 10.23f), new Vector3(-5.43f, Deck, 11.77f));
            Rail(g, "Rail_North", new Vector3(-8.8f, Deck, 17.63f), new Vector3(3.4f, Deck, 17.63f));
            Rail(g, "Rail_NorthEnd", new Vector3(5.17f, Deck, 17.6f), new Vector3(5.17f, Deck, 20f));

            // cyan glow tape on every drop edge, so the dry route reads in the dark
            Tape(g, "Tape_West", new Vector3(-8.88f, Deck, 10.55f), new Vector3(0.05f, 0.01f, 14.1f));
            Tape(g, "Tape_PierS", new Vector3(-7.1f, Deck, 10.29f), new Vector3(3.4f, 0.01f, 0.05f));
            Tape(g, "Tape_PierN", new Vector3(-7.1f, Deck, 11.71f), new Vector3(3.4f, 0.01f, 0.05f));
            Tape(g, "Tape_PierEnd", new Vector3(-5.49f, Deck, 11f), new Vector3(0.05f, 0.01f, 1.6f));
            Tape(g, "Tape_North", new Vector3(-2.7f, Deck, 17.68f), new Vector3(12.2f, 0.01f, 0.05f));
            Tape(g, "Tape_NorthEnd", new Vector3(5.12f, Deck, 18.8f), new Vector3(0.05f, 0.01f, 2.4f));
            float slope = Mathf.Atan2(0.75f, 2.8f) * Mathf.Rad2Deg;
            GB.Box(g, "Tape_Ramp", new Vector3(-8.85f, 0.631f, 2.1f), new Vector3(0.05f, 0.01f, 2.9f), tapeMat, new Vector3(-slope, 0f, 0f), false);
            for (int i = 0; i < 4; i++)
                GB.Box(g, $"Tape_Step{i}", new Vector3(4.3f, 0.25f * (i + 1) + 0.006f, 16.4f + 0.3f * i + 0.03f), new Vector3(1.8f, 0.01f, 0.05f), tapeMat, 0f, false);
        }

        /// Lockers, sea chest, fuse corner, Tagalong case, the show's camera on the pier, and two real recharge lamps.
        static void GalleryProps(Transform g)
        {
            // diver's equipment lockers against the west wall (walkway beside them stays 1.6 m)
            var lockerMat = GB.Mat("Tank_LockerTeal", new Color(0.14f, 0.26f, 0.25f), 0.35f, 0.4f);
            for (int i = 0; i < 2; i++)
            {
                var l = GB.Group(g, $"DiverLocker{i}", new Vector3(-10.7f, Deck, 13.975f + 0.8f * i), 90f);
                GB.Box(l, "Body", new Vector3(0f, 1f, 0f), new Vector3(0.75f, 2f, 0.6f), lockerMat);
                GB.Box(l, "Vents", new Vector3(0f, 1.65f, 0.305f), new Vector3(0.45f, 0.25f, 0.01f), GB.MatteBlack, 0f, false);
                GB.Marker(l, MarkerKind.HidingSpot, Vector3.zero, 0f, $"Inside diver's equipment locker {i + 1} on the west gallery (vents face the walkway)");
            }
            GB.Sphere(g, "DiveHelmet", new Vector3(-10.7f, Deck + 2.22f, 14.775f), 0.22f, brassMat, false);

            // Mrs. Deepwick's prop sea chest on the north gallery
            var chest = GB.Group(g, "SeaChest", new Vector3(-6f, Deck, 19.6f), 180f); // z 19.25..19.95, leaves 1.59 m of gallery
            GB.Box(chest, "Chest", new Vector3(0f, 0.42f, 0f), new Vector3(1.4f, 0.84f, 0.7f), GB.Wood);
            GB.Box(chest, "Band0", new Vector3(-0.4f, 0.42f, 0f), new Vector3(0.06f, 0.86f, 0.72f), brassMat, 0f, false);
            GB.Box(chest, "Band1", new Vector3(0.4f, 0.42f, 0f), new Vector3(0.06f, 0.86f, 0.72f), brassMat, 0f, false);
            GB.Box(chest, "Hasp", new Vector3(0f, 0.7f, 0.365f), new Vector3(0.1f, 0.14f, 0.03f), brassMat, 0f, false);
            GB.Marker(chest, MarkerKind.HidingSpot, Vector3.zero, 0f, "Inside Mrs. Deepwick's prop sea chest, lid slats face the gallery");

            // far north-west corner: spare-fuse cabinet with the fuse dropped below it
            GB.Box(g, "FuseCabinet", new Vector3(-10f, Deck + 1.5f, 19.93f), new Vector3(0.5f, 0.6f, 0.14f), GB.Metal, 0f, false);
            GB.Marker(g, MarkerKind.FuseSpawn, new Vector3(-10f, Deck, 19f), 0f, "Spare fuse on the gallery floor under the fuse cabinet, far north-west corner");

            // specimen case with the spare Tagalong Twins patches
            var tw = GB.Group(g, "TagalongCase", new Vector3(2f, Deck, 20f), 180f);
            GB.Box(tw, "Stand", new Vector3(0f, 0.45f, 0.35f), new Vector3(0.9f, 0.9f, 0.6f), GB.Wood);
            GB.Box(tw, "Case", new Vector3(0f, 1.15f, 0.35f), new Vector3(0.86f, 0.5f, 0.56f), GB.Glass);
            GB.Ellipsoid(tw, "TwinL", new Vector3(-0.2f, 1.12f, 0.4f), new Vector3(0.24f, 0.15f, 0.08f), setTealMat, Vector3.zero, false);
            GB.Ellipsoid(tw, "TwinR", new Vector3(0.12f, 1.12f, 0.4f), new Vector3(0.24f, 0.15f, 0.08f), setTealMat, Vector3.zero, false);
            GB.Box(tw, "EmptyPin", new Vector3(0.36f, 1.12f, 0.42f), new Vector3(0.015f, 0.015f, 0.06f), brassMat, 0f, false);
            GB.Box(tw, "LabelCard", new Vector3(0f, 0.75f, 0.655f), new Vector3(0.4f, 0.15f, 0.01f), GB.Plywood, 0f, false);
            GB.Marker(tw, MarkerKind.StoryProp, new Vector3(0f, 0f, 0.9f), 0f, "Specimen case of the spare Tagalong Twins patches: two felt fish with stitched-shut eyes, and a third empty pin on the label card");

            // the show's 16 mm camera, still on its tripod at the pier tip, aimed at the seabed set
            var cam = GB.Group(g, "Show16mmCamera", new Vector3(-5.9f, Deck, 10.55f), 90f);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f;
                GB.Box(cam, $"Leg{i}", GB.Polar(a, 0.13f, 0.62f), new Vector3(0.035f, 1.25f, 0.035f), GB.MatteBlack, new Vector3(-12f, a, 0f), false);
            }
            GB.Box(cam, "Body", new Vector3(0f, 1.38f, 0f), new Vector3(0.2f, 0.26f, 0.42f), GB.MatteBlack, 0f, false);
            GB.Cylinder(cam, "Magazine0", new Vector3(0f, 1.62f, -0.08f), 0.13f, 0.06f, GB.MatteBlack, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(cam, "Magazine1", new Vector3(0f, 1.62f, 0.14f), 0.13f, 0.06f, GB.MatteBlack, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(cam, "Lens", new Vector3(0f, 1.4f, 0.3f), 0.05f, 0.16f, GB.Metal, new Vector3(90f, 0f, 0f), false);
            GB.Marker(cam, MarkerKind.StoryProp, Vector3.zero, 0f, "The show's 16 mm camera on its tripod at the end of the pier, still aimed at the seabed set");

            // pier tip: a reel left as bait beside a dark dead end
            GB.Marker(g, MarkerKind.ReelSpawn, new Vector3(-6.9f, Deck, 11.35f), 90f, "Reel near the tip of the camera pier, about 1 m short of a lure node");
            GB.Marker(g, MarkerKind.LureNode, new Vector3(-6.1f, Deck, 11.25f), 270f, "Tip of the camera pier over dark water: dead end, faces back along the pier");

            // real recharge lamps: brackets at 2.2 m on open gallery walls
            WallLamp(g, "RechargeLamp_West", new Vector3(-11f, Deck, 8f), 90f, "Real recharge lamp on the west gallery wall, bracket at 2.2 m");
            WallLamp(g, "RechargeLamp_North", new Vector3(-2f, Deck, 20f), 180f, "Real recharge lamp on the north gallery wall, bracket at 2.2 m");
        }

        // ---------------------------------------------------------------- seabed set (island)

        /// Round matte-black island (r 3.4 step, r 3.0 deck at y 0.5) centred at (0, 11): the loop the player circles
        /// through the water. Deepwick's mark faces the door under a cyan spot; the diving bell's door faces away, north.
        static void Island(Transform g)
        {
            GB.Cylinder(g, "Island_Step", new Vector3(0f, 0.125f, 11f), 3.4f, 0.25f, GB.MatteBlack);
            GB.Cylinder(g, "Island_Deck", new Vector3(0f, IslandTop * 0.5f, 11f), 3f, IslandTop, GB.MatteBlack);
            for (int i = 0; i < 14; i++) // Deepwick's cyan dots painted round the riser
                GB.Sphere(g, $"CyanDot{i:00}", new Vector3(0f, 0.13f, 11f) + GB.Polar(i * (360f / 14f) + 6f, 3.42f), 0.035f, cyanMat, false);

            // her spike mark, lit by the set spot
            Vector3 mark = new Vector3(0.9f, IslandTop, 9f);
            GB.Box(g, "Mark_X0", mark + Vector3.up * 0.006f, new Vector3(0.6f, 0.01f, 0.06f), cyanMat, 45f, false);
            GB.Box(g, "Mark_X1", mark + Vector3.up * 0.006f, new Vector3(0.6f, 0.01f, 0.06f), cyanMat, -45f, false);
            GB.Marker(g, MarkerKind.GloamieSpawn, mark, 180f, "Mrs. Deepwick's spike mark on her seabed set, lure lit, facing the door");
            GB.SpotLight(g, "SetSpot", new Vector3(0.6f, 6.6f, 8.8f), new Vector3(90f, 0f, 0f), new Color(0.25f, 0.75f, 0.85f), 8f, 36f, 1.4f);
            GB.Box(g, "SetSpot_Housing", new Vector3(0.6f, 6.8f, 8.8f), new Vector3(0.35f, 0.3f, 0.35f), GB.MatteBlack, 0f, false);
            GB.Cylinder(g, "Batten", new Vector3(0f, 6.9f, 8.8f), 0.04f, 10f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Box(g, "VelvetBorder_S", new Vector3(0f, 5.8f, 6.8f), new Vector3(9f, 2.4f, 0.05f), GB.Velvet, 0f, false);
            GB.Box(g, "VelvetBorder_N", new Vector3(0f, 5.8f, 15.4f), new Vector3(9f, 2.4f, 0.05f), GB.Velvet, 0f, false);

            // her rocking chair and knitting basket
            var chair = GB.Group(g, "RockingChair", new Vector3(-1.2f, IslandTop, 9.5f), 180f);
            GB.Box(chair, "Seat", new Vector3(0f, 0.45f, 0f), new Vector3(0.55f, 0.06f, 0.5f), GB.Wood);
            GB.Box(chair, "Back", new Vector3(0f, 0.9f, -0.24f), new Vector3(0.55f, 0.85f, 0.06f), GB.Wood);
            GB.Box(chair, "SideL", new Vector3(-0.25f, 0.25f, 0f), new Vector3(0.05f, 0.4f, 0.45f), GB.Wood, 0f, false);
            GB.Box(chair, "SideR", new Vector3(0.25f, 0.25f, 0f), new Vector3(0.05f, 0.4f, 0.45f), GB.Wood, 0f, false);
            GB.Box(chair, "RockerL", new Vector3(-0.25f, 0.04f, 0f), new Vector3(0.05f, 0.08f, 0.85f), GB.Wood, 0f, false);
            GB.Box(chair, "RockerR", new Vector3(0.25f, 0.04f, 0f), new Vector3(0.05f, 0.08f, 0.85f), GB.Wood, 0f, false);
            GB.Box(chair, "Shawl", new Vector3(0f, 1f, -0.2f), new Vector3(0.5f, 0.5f, 0.04f), setTealMat, new Vector3(-12f, 0f, 0f), false);

            Vector3 basket = new Vector3(-2.1f, IslandTop, 9.7f);
            GB.Cylinder(g, "KnittingBasket", basket + Vector3.up * 0.15f, 0.25f, 0.3f, GB.Wood);
            GB.Sphere(g, "YarnBall", basket + new Vector3(0.05f, 0.36f, 0f), 0.1f, setTealMat, false);
            GB.Box(g, "Needles", basket + new Vector3(-0.05f, 0.4f, 0.02f), new Vector3(0.02f, 0.02f, 0.4f), brassMat, new Vector3(-30f, 20f, 0f), false);
            GB.Box(g, "HalfScarf", basket + new Vector3(0.35f, 0.01f, 0.1f), new Vector3(0.16f, 0.02f, 0.6f), setTealMat, 15f, false);
            GB.Marker(g, MarkerKind.StoryProp, basket, 180f, "Deepwick's rocking chair and knitting basket: a half-knitted sea-glass scarf, needles still in; a taped script page reads 'brings her own light to the darkest place of all'");

            // giant plywood clam, half open, with a reel inside
            Vector3 clam = new Vector3(2f, IslandTop, 10.4f);
            GB.Ellipsoid(g, "Clam_Lower", clam + new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.5f, 1f), paintMat);
            GB.Ellipsoid(g, "Clam_Upper", clam + new Vector3(0f, 0.78f, 0.18f), new Vector3(1.2f, 0.3f, 1f), paintMat, new Vector3(50f, 0f, 0f), false);
            GB.Marker(g, MarkerKind.ReelSpawn, clam + Vector3.up * 0.4f, 180f, "Reel inside the giant plywood clam beside Deepwick's mark (shell top 0.9 m, deck walkable around it)");

            // felt-on-plywood kelp flats with Evergleam glints
            KelpFlat(g, "KelpFlat0", new Vector3(-2.4f, IslandTop, 11.8f), 80f, 2.2f);
            KelpFlat(g, "KelpFlat1", new Vector3(2.4f, IslandTop, 12.1f), 100f, 1.8f);
            KelpFlat(g, "KelpFlat2", new Vector3(-1.2f, IslandTop, 13.2f), 25f, 2.6f);

            Bell(GB.Group(g, "DivingBell", new Vector3(0f, IslandTop, 11.6f)));
        }

        /// Walk-in brass diving bell (inner radius about 1.24 m, 2.4 m headroom) with its 1.4 m door facing north.
        static void Bell(Transform b)
        {
            GB.Cylinder(b, "FloorPlate", new Vector3(0f, 0.05f, 0f), 1.42f, 0.1f, brassMat);
            GB.ArcWall(b, "Hull", Vector3.zero, 1.3f, 0.1f, 2.4f, 0.12f, 35f, 325f, brassMat, 29f);
            GB.Cylinder(b, "RoofRing", new Vector3(0f, 2.8f, 0f), 1.36f, 0.6f, brassMat);
            GB.Ellipsoid(b, "Dome", new Vector3(0f, 3.1f, 0f), new Vector3(2.72f, 1.2f, 2.72f), brassMat, Vector3.zero, false);
            GB.Cylinder(b, "LiftEye", new Vector3(0f, 3.8f, 0f), 0.22f, 0.08f, GB.Metal, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(b, "Cable", new Vector3(0f, 5.2f, 0f), 0.03f, 2.6f, GB.Metal, false);
            foreach (float a in new[] { 90f, 180f, 270f })
                GB.Cylinder(b, $"Porthole{a:000}", GB.Polar(a, 1.38f, 1.7f), 0.2f, 0.05f, portholeMat, new Vector3(90f, a, 0f), false);
            GB.Marker(b, MarkerKind.LureNode, new Vector3(0f, 0.1f, -0.6f), 0f, "Inside the walk-in diving bell, back wall: dark dead end facing its north door");
        }

        static void KelpFlat(Transform parent, string name, Vector3 basePos, float yaw, float height)
        {
            var k = GB.Group(parent, name, basePos, yaw);
            GB.Box(k, "Flat", new Vector3(0f, height * 0.5f, 0f), new Vector3(0.7f, height, 0.08f), feltMat);
            GB.Sphere(k, "Glint0", new Vector3(-0.15f, height * 0.55f, 0.05f), 0.04f, cyanMat, false);
            GB.Sphere(k, "Glint1", new Vector3(0.12f, height * 0.8f, 0.05f), 0.035f, cyanMat, false);
        }

        // ---------------------------------------------------------------- aquarium tanks (east wall)

        /// Two big glass tanks on plinths against the east wall, with a 1.5 m dark slot between them,
        /// and a sand filter in the south-east corner.
        static void Tanks(Transform g)
        {
            var plinth = GB.Mat("Tank_PlinthTile", new Color(0.14f, 0.2f, 0.2f), 0.5f);
            var murk = GB.Transparent("Tank_DeepWater", new Color(0.02f, 0.14f, 0.18f, 0.55f));
            Tank(g, "TankA", new Vector3(9.5f, 0f, 5f), 4f, plinth, murk);       // z 3.0..7.0
            Tank(g, "TankB", new Vector3(9.5f, 0f, 10.15f), 3.3f, plinth, murk); // z 8.5..11.8
            GB.Marker(g, MarkerKind.LureNode, new Vector3(10.4f, 0f, 7.75f), 270f, "Dark 1.5 m slot between the two glass tanks, dead end at the east wall");

            var filterMat = GB.Mat("Tank_FilterPaint", new Color(0.18f, 0.24f, 0.2f), 0.3f, 0.3f);
            GB.Cylinder(g, "SandFilter", new Vector3(9.9f, 1.1f, 1.3f), 0.8f, 2.2f, filterMat);
            GB.Cylinder(g, "FilterPipe_Up", new Vector3(9.9f, 2.65f, 1.3f), 0.1f, 0.9f, pipeMat, false);
            GB.Cylinder(g, "FilterPipe_Over", new Vector3(9.9f, 3.1f, 2.6f), 0.1f, 2.6f, pipeMat, new Vector3(90f, 0f, 0f), false);
        }

        /// One tank: 3 m wide (x) by lengthZ, plinth 0.6 high, glass 2.4 high, murky water and seabed inside, cyan glow.
        static void Tank(Transform parent, string name, Vector3 floorCenter, float lengthZ, Material plinth, Material murk)
        {
            const float w = 3f;
            var t = GB.Group(parent, name, floorCenter);
            GB.Box(t, "Plinth", new Vector3(0f, 0.3f, 0f), new Vector3(w, 0.6f, lengthZ), plinth);
            GB.Box(t, "Glass", new Vector3(0f, 1.8f, 0f), new Vector3(w - 0.1f, 2.4f, lengthZ - 0.1f), GB.Glass);
            GB.Box(t, "TankWater", new Vector3(0f, 1.6f, 0f), new Vector3(w - 0.2f, 1.95f, lengthZ - 0.2f), murk, 0f, false);
            GB.Box(t, "Rim", new Vector3(0f, 3.03f, 0f), new Vector3(w, 0.06f, lengthZ), GB.Metal, 0f, false);
            for (int i = 0; i < 4; i++)
            {
                float sx = (i & 1) == 0 ? -1f : 1f, sz = (i & 2) == 0 ? -1f : 1f;
                GB.Box(t, $"Post{i}", new Vector3(sx * (w * 0.5f - 0.04f), 1.8f, sz * (lengthZ * 0.5f - 0.04f)), new Vector3(0.08f, 2.4f, 0.08f), GB.Metal, 0f, false);
            }
            GB.Ellipsoid(t, "Rock0", new Vector3(0.5f, 0.8f, -0.6f), new Vector3(1.1f, 0.5f, 0.9f), GB.DarkConcrete, Vector3.zero, false);
            GB.Ellipsoid(t, "Rock1", new Vector3(-0.6f, 0.75f, 0.7f), new Vector3(0.8f, 0.4f, 0.7f), GB.DarkConcrete, Vector3.zero, false);
            GB.Box(t, "EvergleamKelp0", new Vector3(0.9f, 1.4f, 0.3f), new Vector3(0.12f, 1.6f, 0.04f), kelpGlowMat, 0f, false);
            GB.Box(t, "EvergleamKelp1", new Vector3(-0.2f, 1.2f, -0.9f), new Vector3(0.12f, 1.2f, 0.04f), kelpGlowMat, 25f, false);
            GB.Cylinder(t, "FeedPipe", new Vector3(1.1f, 3.65f, 0f), 0.1f, 1.2f, pipeMat, false);
            GB.PointLight(t, "TankGlow", new Vector3(0f, 2.2f, 0f), new Color(0.2f, 0.8f, 0.9f), 3f, 0.9f);
        }

        // ---------------------------------------------------------------- pump room (north-east)

        /// Interior x 7.15..11, z 13.65..20: one 1.6 m door in its west wall and a 1.0 m crawl gap in its south wall out to
        /// the tank nook. The breaker faces the door, so its crescendo goes off in a dead end unless you take the crawl gap.
        static void PumpRoom(Transform g)
        {
            var wallMat = GB.Mat("Tank_PumpRoomWall", new Color(0.1f, 0.12f, 0.12f), 0.3f);
            GB.Wall(g, "Wall_W", new Vector3(7f, 0f, 13.35f), new Vector3(7f, 0f, 20f), 7f, 0.3f, wallMat, new Opening(1.65f, 1.6f, 0f, 2.4f));
            GB.Wall(g, "Wall_S", new Vector3(7.15f, 0f, 13.5f), new Vector3(11f, 0f, 13.5f), 7f, 0.3f, wallMat, new Opening(2.45f, 1f, 0f, 1f));
            GB.Box(g, "DoorTape_S", new Vector3(6.84f, 1.2f, 14.15f), new Vector3(0.02f, 2.4f, 0.05f), tapeMat, 0f, false);
            GB.Box(g, "DoorTape_N", new Vector3(6.84f, 1.2f, 15.85f), new Vector3(0.02f, 2.4f, 0.05f), tapeMat, 0f, false);
            GB.Marker(g, MarkerKind.Handprint, new Vector3(9.6f, 0f, 13.5f), 180f, "Child-sized crawl gap (1.0 m wide, 1.0 m tall) under the pump-room wall: shortcut from the breaker out to the tank nook");

            var br = GB.Group(g, "BreakerPanel", new Vector3(11f, 0f, 15.6f), 270f);
            GB.Box(br, "Panel", new Vector3(0f, 1.5f, 0.12f), new Vector3(0.9f, 1.3f, 0.24f), GB.Mat("Tank_BreakerGrey", new Color(0.3f, 0.32f, 0.3f), 0.35f, 0.6f));
            GB.Box(br, "Lever", new Vector3(0.3f, 1.45f, 0.3f), new Vector3(0.08f, 0.4f, 0.08f), GB.MatteBlack, 0f, false);
            GB.Sphere(br, "DeadLamp", new Vector3(-0.3f, 2f, 0.26f), 0.04f, redMat, false);
            GB.Box(br, "Conduit", new Vector3(0f, 4.6f, 0.06f), new Vector3(0.1f, 4.8f, 0.1f), GB.Metal, 0f, false);
            GB.Marker(br, MarkerKind.Breaker, new Vector3(0f, 0f, 0.45f), 0f, "Wing breaker on the pump-room east wall, facing the door: the 10 s crescendo goes off in a dead end; leave by the door or the crawl gap");

            var pumpMat = GB.Mat("Tank_PumpGreen", new Color(0.12f, 0.2f, 0.15f), 0.4f, 0.5f);
            GB.Box(g, "Pump_Base", new Vector3(8.6f, 0.15f, 17.2f), new Vector3(1.4f, 0.3f, 1.2f), GB.DarkConcrete);
            GB.Cylinder(g, "Pump_Motor", new Vector3(8.6f, 0.85f, 17.2f), 0.5f, 1.1f, pumpMat, new Vector3(90f, 0f, 0f));
            GB.Cylinder(g, "Pump_Riser", new Vector3(8.6f, 4.2f, 17.55f), 0.18f, 5.6f, pipeMat);
            GB.Cylinder(g, "Pump_Wheel", new Vector3(9.15f, 0.95f, 17.2f), 0.22f, 0.04f, GB.Metal, new Vector3(0f, 0f, 90f), false);

            GB.Box(g, "Bench_Top", new Vector3(8.3f, 0.92f, 19.65f), new Vector3(2f, 0.06f, 0.7f), GB.Wood);
            GB.Box(g, "Bench_LegW", new Vector3(7.33f, 0.445f, 19.65f), new Vector3(0.06f, 0.89f, 0.7f), GB.Wood);
            GB.Box(g, "Bench_LegE", new Vector3(9.27f, 0.445f, 19.65f), new Vector3(0.06f, 0.89f, 0.7f), GB.Wood);
            GB.Box(g, "Bench_Toolbox", new Vector3(8.9f, 1f, 19.7f), new Vector3(0.45f, 0.1f, 0.22f), GB.Metal, 0f, false);
            GB.Marker(g, MarkerKind.BatterySpawn, new Vector3(8f, 0.95f, 19.5f), 180f, "Battery on the pump-room workbench (top 0.95 m)");

            GB.Box(g, "SumpGrate", new Vector3(10f, 0.01f, 17.6f), new Vector3(0.7f, 0.02f, 0.7f), GB.Metal, 0f, false);
            GB.Marker(g, MarkerKind.Hatch, new Vector3(10f, 0f, 17.6f), 0f, "Pump-room sump grate: open gap down to the drains");
            GB.Marker(g, MarkerKind.LureNode, new Vector3(10.35f, 0f, 19.35f), 215f, "Back corner of the pump room behind the pump: dark dead end facing the door");

            // outside the pump room: dark nook between the north gallery's end and the pump-room wall
            GB.Marker(g, MarkerKind.LureNode, new Vector3(6f, 0f, 19.2f), 180f, "Dark nook behind the north-east stairs, dead end against the north wall");
        }

        // ---------------------------------------------------------------- stage-manager desk (south-east)

        /// Desk against the south wall east of the door (group origin at the wall, x 4.6..6.6 in the room), with the
        /// black-light Curtain Call cue lamp.
        static void StageManagerDesk(Transform d)
        {
            GB.Box(d, "Top", new Vector3(0f, 0.87f, 0.4f), new Vector3(2f, 0.06f, 0.8f), GB.Plywood);
            GB.Box(d, "SideL", new Vector3(-0.97f, 0.42f, 0.4f), new Vector3(0.06f, 0.84f, 0.8f), GB.Plywood);
            GB.Box(d, "SideR", new Vector3(0.97f, 0.42f, 0.4f), new Vector3(0.06f, 0.84f, 0.8f), GB.Plywood);
            GB.Box(d, "BackPanel", new Vector3(0f, 0.5f, 0.05f), new Vector3(1.88f, 0.7f, 0.04f), GB.MatteBlack, 0f, false);

            GB.Cylinder(d, "CueLamp_Base", new Vector3(0.55f, 0.92f, 0.35f), 0.08f, 0.04f, GB.MatteBlack, false);
            GB.Box(d, "CueLamp_Neck", new Vector3(0.55f, 1.08f, 0.35f), new Vector3(0.03f, 0.3f, 0.03f), GB.Metal, 0f, false);
            GB.Box(d, "CueLamp_Hood", new Vector3(0.55f, 1.26f, 0.42f), new Vector3(0.2f, 0.08f, 0.18f), GB.MatteBlack, 0f, false);
            GB.Box(d, "CueLamp_Tube", new Vector3(0.55f, 1.21f, 0.42f), new Vector3(0.16f, 0.025f, 0.03f), uvMat, 0f, false);
            GB.PointLight(d, "CueLamp_UV", new Vector3(0.55f, 1.12f, 0.5f), new Color(0.55f, 0.25f, 1f), 3f, 0.8f);

            GB.Box(d, "ScriptBinder", new Vector3(-0.1f, 0.925f, 0.4f), new Vector3(0.32f, 0.05f, 0.25f), setTealMat, 0f, false);
            GB.Box(d, "CueLightBox", new Vector3(0f, 1.6f, 0.04f), new Vector3(0.6f, 0.3f, 0.06f), GB.MatteBlack, 0f, false);
            GB.Sphere(d, "Cue_Standby", new Vector3(-0.15f, 1.6f, 0.08f), 0.035f, redMat, false);
            GB.Sphere(d, "Cue_Go", new Vector3(0.15f, 1.6f, 0.08f), 0.035f, GB.Glow("Tank_GlowGreen", new Color(0.3f, 1f, 0.4f), 1f), false);
            GB.Cylinder(d, "Stool", new Vector3(0.7f, 0.3f, 1.25f), 0.22f, 0.6f, GB.MatteBlack);

            GB.Marker(d, MarkerKind.CueLampDesk, new Vector3(0f, 0f, 1.1f), 0f, "Stage-manager desk with the black-light Curtain Call cue lamp, just inside the door");
            GB.Marker(d, MarkerKind.HidingSpot, new Vector3(0f, 0f, 0.4f), 0f, "Under the stage-manager desk (open front faces the hall)");
            GB.Marker(d, MarkerKind.BatterySpawn, new Vector3(-0.6f, 0.9f, 0.45f), 0f, "Battery on the stage-manager desk (top 0.9 m)");
        }

        // ---------------------------------------------------------------- entrance wall, pipes, patrol

        /// Entrance wall west of the door: a real recharge lamp and the show's sing-along board.
        static void SouthWall(Transform g)
        {
            WallLamp(g, "RechargeLamp_South", new Vector3(-6.5f, 0f, 0f), 0f, "Real recharge lamp on the entrance wall west of the door, bracket at 2.2 m");

            var sb = GB.Group(g, "SongBoard", new Vector3(-3.7f, 0f, 0f), 0f);
            GB.Box(sb, "Board", new Vector3(0f, 2f, 0.04f), new Vector3(2f, 1.2f, 0.06f), setTealMat, 0f, false);
            for (int i = 0; i < 5; i++)
                GB.Sphere(sb, $"NoteDot{i}", new Vector3(-0.8f + 0.3f * i, 2.35f - 0.08f * (i % 2), 0.08f), 0.04f, cyanMat, false);
            GB.Box(sb, "PaintedLantern", new Vector3(0.65f, 1.85f, 0.075f), new Vector3(0.3f, 0.45f, 0.01f), GB.Plywood, 0f, false);
            GB.Marker(sb, MarkerKind.StoryProp, new Vector3(0f, 0f, 0.45f), 0f, "Sing-along board from the show: 'If you're lost in the dark, follow a friendly light', with a painted lantern beside the words");
        }

        /// Rusted aquarium mains along three walls, a crossing over the hall into the pump room, a riser by the ramp,
        /// and the "Deep-Dark Tank" sign above the north gallery as a landmark seen from the door.
        static void Pipes(Transform g)
        {
            GB.Cylinder(g, "Main_North", new Vector3(-2f, 5.6f, 19.7f), 0.22f, 18f, pipeMat, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(g, "Main_West", new Vector3(-10.7f, 5.2f, 10f), 0.18f, 20f, pipeMat, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(g, "Main_East", new Vector3(10.7f, 4.2f, 6.6f), 0.18f, 13.2f, pipeMat, new Vector3(90f, 0f, 0f), false);
            GB.Cylinder(g, "Crossing", new Vector3(-1.2f, 6.3f, 17.55f), 0.2f, 19.6f, pipeMat, new Vector3(0f, 0f, 90f), false);
            GB.Cylinder(g, "Riser_SW", new Vector3(-8.3f, 3.5f, 0.25f), 0.15f, 7f, pipeMat);
            GB.Cylinder(g, "Riser_SW_Valve", new Vector3(-8.3f, 1.3f, 0.45f), 0.2f, 0.04f, GB.Metal, new Vector3(90f, 0f, 0f), false);

            var sign = GB.Group(g, "TankSign", new Vector3(-2f, 4.6f, 20f), 180f);
            GB.Box(sign, "Board", new Vector3(0f, 0f, 0.04f), new Vector3(3.6f, 0.8f, 0.06f), setTealMat, 0f, false);
            for (int i = 0; i < 7; i++)
                GB.Sphere(sign, $"Bulb{i}", new Vector3(-1.5f + 0.5f * i, -0.52f, 0.08f), 0.045f, cyanMat, false);
        }

        static void Patrol(Transform g)
        {
            (Vector3 p, string note)[] points =
            {
                (new Vector3(0f, 0f, 5f), "Flooded floor between the door and the set"),
                (new Vector3(-5f, 0f, 14f), "West pool, north of the camera pier"),
                (new Vector3(0f, 0f, 15.9f), "Behind the set, outside the diving bell's door"),
                (new Vector3(5.6f, 0f, 9.5f), "East pool between the set and the tanks"),
                (new Vector3(-9.9f, Deck, 6f), "West gallery near its recharge lamp"),
                (new Vector3(9f, 0f, 15.2f), "Inside the pump room, between the door and the breaker"),
                (new Vector3(0.5f, Deck, 18.8f), "North gallery, midway"),
                (new Vector3(5.8f, 0f, 3.2f), "South-east floor by the stage-manager desk"),
            };
            foreach (var (p, note) in points)
                GB.Marker(g, MarkerKind.PatrolPoint, p, 0f, note);
        }

        // ---------------------------------------------------------------- helpers

        /// Hand rail along a drop edge: top rail 0.95 m above the deck, posts about every 2.4 m.
        static void Rail(Transform parent, string name, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            d.y = 0f;
            float len = d.magnitude;
            var r = GB.Group(parent, name, a, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg);
            GB.Box(r, "TopRail", new Vector3(0f, 0.95f, len * 0.5f), new Vector3(0.06f, 0.06f, len), railMat);
            int posts = Mathf.Max(2, Mathf.CeilToInt(len / 2.4f) + 1);
            for (int i = 0; i < posts; i++)
                GB.Box(r, $"Post{i}", new Vector3(0f, 0.475f, Mathf.Lerp(0.03f, len - 0.03f, i / (posts - 1f))), new Vector3(0.06f, 0.95f, 0.06f), railMat);
        }

        /// Flat strip of glow tape lying on a surface (onSurface.y is the surface height).
        static void Tape(Transform parent, string name, Vector3 onSurface, Vector3 size)
            => GB.Box(parent, name, onSurface + Vector3.up * (size.y * 0.5f + 0.001f), size, tapeMat, 0f, false);

        /// Real recharge lamp: brass plate and bracket holding a steady warm bulb 2.2 m above the surface below.
        /// wallBase is on the wall face at that surface's height; yaw points out of the wall.
        static void WallLamp(Transform parent, string name, Vector3 wallBase, float yaw, string note)
        {
            var l = GB.Group(parent, name, wallBase, yaw);
            GB.Box(l, "BackPlate", new Vector3(0f, 2.2f, 0.03f), new Vector3(0.26f, 0.36f, 0.06f), brassMat, 0f, false);
            GB.Box(l, "Bracket", new Vector3(0f, 2.33f, 0.17f), new Vector3(0.04f, 0.04f, 0.28f), brassMat, 0f, false);
            GB.Box(l, "ChargeSocket", new Vector3(0f, 1.25f, 0.04f), new Vector3(0.18f, 0.24f, 0.08f), GB.Metal, 0f, false);
            GB.GlowLamp(l, "Lamp", new Vector3(0f, 2.2f, 0.3f), new Color(1f, 0.62f, 0.25f), 0.09f, 3.5f, 1f);
            GB.Marker(l, MarkerKind.RechargeLamp, new Vector3(0f, 0f, 0.45f), 0f, note);
        }
    }
}
