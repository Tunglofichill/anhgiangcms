using UnityEngine;

namespace Forge
{
    [CreateAssetMenu(fileName = "Balance", menuName = "Forge/Balance Config")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("Tap & Economy Multipliers")]
        public double tapBase = 1.0;
        public double costMult = 1.15;
        public double outputMult = 1.12;

        [Header("Bonuses")]
        [Tooltip("Extra tap contribution to DPS: tapPower * (0.25 * G1.level)")]
        public double g1TapBonusRate = 0.25;
        [Tooltip("Global multiplier bonus: 1 + 0.05 * G5.level")]
        public double g5GlobalBonusRate = 0.05;

        [Header("Timing")]
        [Tooltip("Maximum offline progress in seconds (6 hours = 21600s)")]
        public float maxOfflineSeconds = 21600f;
        [Tooltip("Fixed tick interval in seconds (0.1s)")]
        public float tickRate = 0.1f;
    }
}
