using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forge
{
    public class MilestoneService : MonoBehaviour
    {
        public event Action<string> OnMilestoneUnlocked;

        private readonly HashSet<string> _unlockedMilestones = new HashSet<string>();

        public void LoadUnlockedMilestones(List<string> milestones)
        {
            _unlockedMilestones.Clear();
            if (milestones != null)
            {
                foreach (var m in milestones)
                {
                    _unlockedMilestones.Add(m);
                }
            }
        }

        public List<string> GetUnlockedList()
        {
            return new List<string>(_unlockedMilestones);
        }

        public bool IsMilestoneUnlocked(string id)
        {
            return _unlockedMilestones.Contains(id);
        }

        public void CheckMilestones(double totalEarned, EconomyService economy)
        {
            if (economy == null) return;

            int g1 = economy.GetGeneratorLevel("G1");
            int g2 = economy.GetGeneratorLevel("G2");
            int g3 = economy.GetGeneratorLevel("G3");
            int g4 = economy.GetGeneratorLevel("G4");
            int g5 = economy.GetGeneratorLevel("G5");

            // M1: totalEarned >= 100
            TryUnlock("M1", totalEarned >= 100.0);

            // M2: G1.level >= 10
            TryUnlock("M2", g1 >= 10);

            // M3: G2.level >= 1
            TryUnlock("M3", g2 >= 1);

            // M4: G3.level >= 10
            TryUnlock("M4", g3 >= 10);

            // M5: G4.level >= 1
            TryUnlock("M5", g4 >= 1);

            // M6: G5.level >= 1
            TryUnlock("M6", g5 >= 1);

            // M7: totalEarned >= 1,000,000
            TryUnlock("M7", totalEarned >= 1000000.0);
        }

        private void TryUnlock(string milestoneId, bool condition)
        {
            if (condition && !_unlockedMilestones.Contains(milestoneId))
            {
                _unlockedMilestones.Add(milestoneId);
                OnMilestoneUnlocked?.Invoke(milestoneId);
                Debug.Log($"[MilestoneService] Unlocked Milestone: {milestoneId}");
            }
        }

        public string GetCurrentRank()
        {
            if (_unlockedMilestones.Contains("M7")) return LocalizationService.Get("RANK_GRANDMASTER");
            if (_unlockedMilestones.Contains("M6")) return LocalizationService.Get("RANK_MASTER");
            if (_unlockedMilestones.Contains("M5")) return LocalizationService.Get("RANK_CHIEF");
            if (_unlockedMilestones.Contains("M4")) return LocalizationService.Get("RANK_SMELTER");
            if (_unlockedMilestones.Contains("M3")) return LocalizationService.Get("RANK_MINER");
            if (_unlockedMilestones.Contains("M2")) return LocalizationService.Get("RANK_NOVICE");
            if (_unlockedMilestones.Contains("M1")) return LocalizationService.Get("RANK_TRAINEE");
            return LocalizationService.Get("RANK_APPRENTICE");
        }
    }
}
