using UnityEngine;

namespace Glowfade.EditorTools
{
    /// Shared numbers for the Wexley Starhouse greybox. World origin = centre of the Star Theatre floor.
    /// Angles are degrees clockwise from +Z (north), seen from above.
    public static class StarhouseLayout
    {
        // Star Theatre (hub)
        public const float RingInner = 16f;          // inner face of the round wall
        public const float RingThickness = 0.4f;
        public const float RingOuter = RingInner + RingThickness;
        public const float RingHeight = 8f;          // the dome starts on top of the wall
        public const float DomeRadius = RingInner + RingThickness * 0.5f;
        public const float DoorHalfAngle = 5.5f;     // half-width of each door gap in the round wall, in degrees
        public const float DoorHeight = 3f;
        public const float PitHalf = 3f;             // square projector pit: |x|,|z| <= PitHalf
        public const float HubSlabBottom = -0.5f;    // underside of the theatre floor = top of the Trap Room ceiling hole edge

        // Projection booth window, cut into the round wall above the front-of-house door
        public const float BoothFloorY = 4.5f;
        public const float BoothWindowBottom = 5.4f;
        public const float BoothWindowTop = 6.9f;
        public const float BoothWindowHalfAngle = 9f;

        // Corridors from the round wall out to each location
        public const float CorridorWidth = 3f;       // interior
        public const float CorridorHeight = 3f;      // interior

        // Locations
        public const float LowerFloorY = -6f;        // Trap Room and Deep-Dark Tank floor level
        public const float WingHalfWidth = 11f;      // wing interior footprint: |x| <= 11
        public const float WingDepth = 20f;          //                          0 <= z <= 20

        public const string MeadowName = "Wing_LampPostMeadow";
        public const string NurseryName = "Wing_TreetopNursery";
        public const string KitchenName = "Wing_MidnightKitchen";
        public const string FrontOfHouseName = "FrontOfHouse";
        public const string EchoLabName = "Wing_EchoLab";
        public const string TankName = "Wing_DeepDarkTank";
        public const string TrapRoomName = "Wing_UnderStageTrapRoom";
        public const string HubName = "Hub_StarTheatre";

        /// A location reached by a corridor from the Star Theatre. Its root sits at the entrance:
        /// on the floor, in the middle of the doorway, with local +Z pointing away from the hub.
        public struct Slot
        {
            public string name;
            public float angle;
            public float radius;
            public float floorY;

            public Slot(string name, float angle, float radius, float floorY)
            {
                this.name = name;
                this.angle = angle;
                this.radius = radius;
                this.floorY = floorY;
            }
        }

        public static readonly Slot Meadow = new Slot(MeadowName, 0f, 26f, 0f);
        public static readonly Slot Nursery = new Slot(NurseryName, 60f, 26f, 0f);
        public static readonly Slot Kitchen = new Slot(KitchenName, 120f, 26f, 0f);
        public static readonly Slot FrontOfHouse = new Slot(FrontOfHouseName, 180f, 26f, 0f);
        public static readonly Slot EchoLab = new Slot(EchoLabName, 240f, 26f, 0f);
        public static readonly Slot Tank = new Slot(TankName, 300f, 30f, LowerFloorY);

        public static readonly Slot[] Slots = { Meadow, Nursery, Kitchen, FrontOfHouse, EchoLab, Tank };

        public static readonly float[] DoorAngles = { 0f, 60f, 120f, 180f, 240f, 300f };

        public static Vector3 RootPosition(Slot slot) => GB.Polar(slot.angle, slot.radius, slot.floorY);

        /// The Trap Room's root: under the theatre's centre, on the lower floor. Interior |x|,|z| <= 13, ceiling at local y 5.5.
        public static readonly Vector3 TrapRoomRoot = new Vector3(0f, LowerFloorY, 0f);
        public const float TrapRoomHalf = 13f;
    }
}
