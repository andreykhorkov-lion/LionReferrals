using UnityEngine;

namespace LionReferrals
{
    [CreateAssetMenu(fileName = "TrackerSettings", menuName = "LionStudios/Create tracker settings")]
    public class TrackerSettings : ScriptableObject
    {
        public string URL;
        public string TrackingId;
    }
}