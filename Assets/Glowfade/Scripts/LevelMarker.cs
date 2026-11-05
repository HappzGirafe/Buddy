using UnityEngine;

namespace Glowfade
{
    /// What a greybox marker stands for. Gameplay systems will find markers by kind.
    public enum MarkerKind
    {
        PlayerStart,
        SavePoint,
        ReelSpawn,
        BatterySpawn,
        FuseSpawn,
        Breaker,
        CueLampDesk,
        HidingSpot,
        Hatch,
        LureNode,
        RechargeLamp,
        GloamieSpawn,
        PatrolPoint,
        MonitorCart,
        TrailBreaker,
        Handprint,
        StoryProp,
    }

    /// Placeholder for a gameplay object in the greybox level.
    public class LevelMarker : MonoBehaviour
    {
        public MarkerKind kind;
        public string location;
        [TextArea] public string note;

        public static Color ColorFor(MarkerKind kind)
        {
            switch (kind)
            {
                case MarkerKind.PlayerStart: return Color.green;
                case MarkerKind.SavePoint: return new Color(0.4f, 1f, 0.6f);
                case MarkerKind.ReelSpawn: return new Color(1f, 0.85f, 0.2f);
                case MarkerKind.BatterySpawn:
                case MarkerKind.FuseSpawn: return new Color(1f, 0.55f, 0.1f);
                case MarkerKind.Breaker:
                case MarkerKind.CueLampDesk: return new Color(0.7f, 0.3f, 1f);
                case MarkerKind.HidingSpot: return new Color(0.3f, 0.6f, 1f);
                case MarkerKind.Hatch: return new Color(1f, 0.3f, 0.5f);
                case MarkerKind.LureNode:
                case MarkerKind.RechargeLamp: return new Color(1f, 0.7f, 0.3f);
                case MarkerKind.GloamieSpawn: return Color.red;
                case MarkerKind.PatrolPoint: return new Color(1f, 0.4f, 0.4f);
                default: return Color.white;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = ColorFor(kind);
            Vector3 p = transform.position + Vector3.up * 0.3f;
            Gizmos.DrawWireSphere(p, 0.3f);
            Gizmos.DrawLine(p, p + transform.forward * 0.7f);
        }
    }
}
