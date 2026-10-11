using UnityEngine;

namespace Forge.World
{
    /// <summary>
    /// Animates 3D machines (G1..G5) procedurally in real-time.
    /// Provides mechanical life: reciprocating hammer blows, rotating spiral drills,
    /// bubbling smelting vats, churning steam flywheels & pistons, and animated Golem automaton.
    /// </summary>
    public class MachineAnimator3D : MonoBehaviour
    {
        public enum MachineType { G1_Hammer, G2_Drill, G3_Smelter, G4_SteamEngine, G5_Golem }
        public MachineType machineType = MachineType.G1_Hammer;

        [Header("G1 Mechanical Hammer")]
        public Transform g1HammerArm;
        public float g1StrikeRate = 1.8f; // blows per second

        [Header("G2 Mining Drill")]
        public Transform g2DrillBit;
        public float g2SpinSpeed = 480f; // degrees per second
        public float g2WobbleIntensity = 0.02f;

        [Header("G3 Crucible Smelter")]
        public Transform g3CrucibleVat;
        public Transform g3SteamVent;
        public float g3BubbleSpeed = 3.5f;

        [Header("G4 Steam Engine")]
        public Transform g4Flywheel;
        public Transform g4Piston;
        public float g4EngineRpm = 180f;

        [Header("G5 Automaton Golem")]
        public Transform g5Torso;
        public Transform g5RightArm;
        public Transform g5LeftArm;
        public Transform g5Core;
        public float g5BreatheRate = 1.2f;

        private Vector3 _g4PistonBasePos;
        private Vector3 _g2DrillBasePos;
        private Vector3 _g5TorsoBasePos;
        private float _timeOffset;

        private void Start()
        {
            _timeOffset = Random.Range(0f, 10f);

            // Auto-locate sub-parts if not wired
            if (g1HammerArm == null) g1HammerArm = transform.Find("Hammer_Arm");
            if (g2DrillBit == null) g2DrillBit = transform.Find("Drill_Assembly/Drill_Bit");
            if (g3CrucibleVat == null) g3CrucibleVat = transform.Find("Crucible_Pot");
            if (g3SteamVent == null) g3SteamVent = transform.Find("Steam_Vent_Cap");
            if (g4Flywheel == null) g4Flywheel = transform.Find("Flywheel_Gear");
            if (g4Piston == null) g4Piston = transform.Find("Piston_Assembly/Piston_Rod");
            if (g5Torso == null) g5Torso = transform.Find("Torso");
            if (g5RightArm == null) g5RightArm = transform.Find("Torso/Arm_Right");
            if (g5LeftArm == null) g5LeftArm = transform.Find("Torso/Arm_Left");
            if (g5Core == null) g5Core = transform.Find("Torso/Magic_Core");

            if (g4Piston != null) _g4PistonBasePos = g4Piston.localPosition;
            if (g2DrillBit != null) _g2DrillBasePos = g2DrillBit.localPosition;
            if (g5Torso != null) _g5TorsoBasePos = g5Torso.localPosition;
        }

        private void Update()
        {
            float t = Time.time + _timeOffset;

            switch (machineType)
            {
                case MachineType.G1_Hammer:
                    AnimateG1(t);
                    break;
                case MachineType.G2_Drill:
                    AnimateG2(t);
                    break;
                case MachineType.G3_Smelter:
                    AnimateG3(t);
                    break;
                case MachineType.G4_SteamEngine:
                    AnimateG4(t);
                    break;
                case MachineType.G5_Golem:
                    AnimateG5(t);
                    break;
            }
        }

        private void AnimateG1(float t)
        {
            if (g1HammerArm == null) return;
            // Sawtooth-like hammer stroke: slow lift up, rapid snap down, bounce
            float cycle = (t * g1StrikeRate) % 1.0f;
            float angle;
            if (cycle < 0.70f)
            {
                // Lifting back up
                angle = Mathf.Lerp(0f, -38f, cycle / 0.70f);
            }
            else if (cycle < 0.85f)
            {
                // Rapid strike down
                angle = Mathf.Lerp(-38f, 5f, (cycle - 0.70f) / 0.15f);
            }
            else
            {
                // Small rebound recoil
                angle = Mathf.Lerp(5f, 0f, (cycle - 0.85f) / 0.15f);
            }
            g1HammerArm.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private void AnimateG2(float t)
        {
            if (g2DrillBit != null)
            {
                // Fast drill rotation
                g2DrillBit.Rotate(0f, 0f, g2SpinSpeed * Time.deltaTime, Space.Self);

                // Subtle axial thrust chatter
                float chatter = Mathf.Sin(t * 30f) * g2WobbleIntensity;
                g2DrillBit.localPosition = _g2DrillBasePos + new Vector3(0f, 0f, chatter);
            }
        }

        private void AnimateG3(float t)
        {
            if (g3CrucibleVat != null)
            {
                // Molten metal boiling simmer
                float simmerX = Mathf.Sin(t * g3BubbleSpeed) * 1.5f;
                float simmerZ = Mathf.Cos(t * g3BubbleSpeed * 1.3f) * 1.5f;
                g3CrucibleVat.localRotation = Quaternion.Euler(simmerX, 0f, simmerZ);
            }
            if (g3SteamVent != null)
            {
                // Pressure relief valve chatter
                float ventLift = Mathf.Clamp01(Mathf.Sin(t * 8f) * 0.015f);
                g3SteamVent.localPosition = new Vector3(0f, 0.45f + ventLift, 0f);
            }
        }

        private void AnimateG4(float t)
        {
            float deg = (t * g4EngineRpm * 6f) % 360f;
            if (g4Flywheel != null)
            {
                // Heavy brass flywheel turning
                g4Flywheel.localRotation = Quaternion.Euler(0f, 0f, deg);
            }
            if (g4Piston != null)
            {
                // Harmonic piston reciprocating stroke
                float stroke = Mathf.Sin(deg * Mathf.Deg2Rad) * 0.065f;
                g4Piston.localPosition = _g4PistonBasePos + new Vector3(stroke, 0f, 0f);
            }
        }

        private void AnimateG5(float t)
        {
            // Breathing / heavy mechanical idle
            float breathe = Mathf.Sin(t * g5BreatheRate * Mathf.PI * 2f);
            if (g5Torso != null)
            {
                g5Torso.localPosition = _g5TorsoBasePos + new Vector3(0f, breathe * 0.02f, 0f);
                g5Torso.localRotation = Quaternion.Euler(breathe * 2f, 0f, 0f);
            }
            if (g5RightArm != null)
            {
                // Right arm hammer strike cycle
                float armStrike = Mathf.Sin(t * 2.5f) * 15f;
                g5RightArm.localRotation = Quaternion.Euler(armStrike - 10f, 0f, 0f);
            }
            if (g5LeftArm != null)
            {
                // Left arm tongs steady holding
                float armHold = Mathf.Cos(t * 2.5f) * 5f;
                g5LeftArm.localRotation = Quaternion.Euler(armHold + 5f, 0f, 0f);
            }
            if (g5Core != null)
            {
                // Pulsing magical chest core scale
                float corePulse = 1.0f + Mathf.Sin(t * 4f) * 0.12f;
                g5Core.localScale = new Vector3(corePulse, corePulse, corePulse);
            }
        }
    }
}
