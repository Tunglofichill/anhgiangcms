using System.Collections.Generic;
using UnityEngine;

namespace Forge
{
    public class WorldView : MonoBehaviour
    {
        [Header("Machine Objects (Machine_G1..Machine_G5)")]
        public GameObject machineG1;
        public GameObject machineG2;
        public GameObject machineG3;
        public GameObject machineG4;
        public GameObject machineG5;

        [Header("Decorations")]
        public GameObject decorLamp;        // M1
        public GameObject decorOreShelf;    // M3
        public GameObject decorSmokeStack;   // M4
        public GameObject decorSign;         // M5
        public GameObject decorChest;        // M7

        [Header("Effects")]
        public ParticleSystem fxHitSpark;
        public ParticleSystem fxOreFly;
        public ParticleSystem fxSmoke;
        public ParticleSystem fxUnlockBurst;

        private Vector3 _originalG1Scale = Vector3.one;

        private void Awake()
        {
            if (machineG1 != null)
            {
                _originalG1Scale = machineG1.transform.localScale;
            }
        }

        public void PlayHitSpark(Vector3 hitPoint)
        {
            if (SettingsService.Instance != null && !SettingsService.Instance.VFXEnabled) return;

            if (fxHitSpark != null)
            {
                fxHitSpark.transform.position = hitPoint;
                fxHitSpark.Play();
            }

            if (fxOreFly != null)
            {
                fxOreFly.transform.position = hitPoint;
                fxOreFly.Play();
            }
        }

        public void PlayUnlockBurst()
        {
            if (SettingsService.Instance != null && !SettingsService.Instance.VFXEnabled) return;

            if (fxUnlockBurst != null)
            {
                fxUnlockBurst.Play();
            }
        }

        public void Refresh(EconomyService economy, MilestoneService milestones)
        {
            if (economy == null || milestones == null) return;

            // Machine visibility (hidden until level >= 1)
            int g1Level = economy.GetGeneratorLevel("G1");
            int g2Level = economy.GetGeneratorLevel("G2");
            int g3Level = economy.GetGeneratorLevel("G3");
            int g4Level = economy.GetGeneratorLevel("G4");
            int g5Level = economy.GetGeneratorLevel("G5");

            if (machineG1 != null) machineG1.SetActive(g1Level >= 1);
            if (machineG2 != null) machineG2.SetActive(g2Level >= 1);
            if (machineG3 != null) machineG3.SetActive(g3Level >= 1);
            if (machineG4 != null) machineG4.SetActive(g4Level >= 1);
            if (machineG5 != null) machineG5.SetActive(g5Level >= 1);

            // M2: Scale G1 by 1.15
            bool m2 = milestones.IsMilestoneUnlocked("M2");
            if (machineG1 != null)
            {
                machineG1.transform.localScale = m2 ? _originalG1Scale * 1.15f : _originalG1Scale;
            }

            // Decor visibility based on Milestones
            bool m1 = milestones.IsMilestoneUnlocked("M1");
            bool m3 = milestones.IsMilestoneUnlocked("M3");
            bool m4 = milestones.IsMilestoneUnlocked("M4");
            bool m5 = milestones.IsMilestoneUnlocked("M5");
            bool m7 = milestones.IsMilestoneUnlocked("M7");

            if (decorLamp != null) decorLamp.SetActive(m1);
            if (decorOreShelf != null) decorOreShelf.SetActive(m3);
            if (decorSmokeStack != null) decorSmokeStack.SetActive(m4);
            if (decorSign != null) decorSign.SetActive(m5);
            if (decorChest != null) decorChest.SetActive(m7);

            // Smoke particle on M4
            if (fxSmoke != null)
            {
                if (m4 && !fxSmoke.isPlaying) fxSmoke.Play();
                else if (!m4 && fxSmoke.isPlaying) fxSmoke.Stop();
            }
        }
    }
}
