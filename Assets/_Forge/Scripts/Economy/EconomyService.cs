using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forge
{
    public class EconomyService : MonoBehaviour
    {
        [Header("Configuration")]
        public BalanceConfig balanceConfig;
        public List<GeneratorDefinition> generatorDefinitions = new List<GeneratorDefinition>();

        private readonly Dictionary<string, int> _generatorLevels = new Dictionary<string, int>();

        public void Initialize(BalanceConfig config, List<GeneratorDefinition> definitions)
        {
            balanceConfig = config;
            generatorDefinitions = definitions;
            _generatorLevels.Clear();

            foreach (var def in generatorDefinitions)
            {
                if (def != null && !string.IsNullOrEmpty(def.id))
                {
                    _generatorLevels[def.id] = 0;
                }
            }
        }

        public void SetGeneratorLevel(string id, int level)
        {
            if (_generatorLevels.ContainsKey(id))
            {
                _generatorLevels[id] = Mathf.Max(0, level);
            }
        }

        public int GetGeneratorLevel(string id)
        {
            return _generatorLevels.TryGetValue(id, out int level) ? level : 0;
        }

        public GeneratorDefinition GetDefinition(string id)
        {
            return generatorDefinitions.Find(d => d != null && d.id == id);
        }

        public double CalculateGlobalMultiplier()
        {
            int g5Level = GetGeneratorLevel("G5");
            double bonusRate = balanceConfig != null ? balanceConfig.g5GlobalBonusRate : 0.05;
            return 1.0 + (bonusRate * g5Level);
        }

        public double CalculateFinalTapPower()
        {
            double tapBase = balanceConfig != null ? balanceConfig.tapBase : 1.0;
            return tapBase * CalculateGlobalMultiplier();
        }

        public double CalculateFinalDps()
        {
            double sumOutput = 0.0;
            double outputMult = balanceConfig != null ? balanceConfig.outputMult : 1.12;

            foreach (var def in generatorDefinitions)
            {
                if (def == null) continue;
                int level = GetGeneratorLevel(def.id);
                sumOutput += def.GetOutput(level, outputMult);
            }

            int g1Level = GetGeneratorLevel("G1");
            double tapBase = balanceConfig != null ? balanceConfig.tapBase : 1.0;
            double g1TapBonusRate = balanceConfig != null ? balanceConfig.g1TapBonusRate : 0.25;
            double tapBonusDps = tapBase * (g1TapBonusRate * g1Level);

            double dps = sumOutput + tapBonusDps;
            double globalMult = CalculateGlobalMultiplier();

            return dps * globalMult;
        }

        public double GetGeneratorCost(string id)
        {
            var def = GetDefinition(id);
            if (def == null) return double.MaxValue;

            int currentLevel = GetGeneratorLevel(id);
            double costMult = balanceConfig != null ? balanceConfig.costMult : 1.15;
            return def.GetCost(currentLevel, costMult);
        }

        public double GetGeneratorCurrentOutput(string id)
        {
            var def = GetDefinition(id);
            if (def == null) return 0.0;

            int currentLevel = GetGeneratorLevel(id);
            double outputMult = balanceConfig != null ? balanceConfig.outputMult : 1.12;
            double rawOutput = def.GetOutput(currentLevel, outputMult);
            return rawOutput * CalculateGlobalMultiplier();
        }

        public bool IsGeneratorUnlocked(string id, double totalEarned)
        {
            var def = GetDefinition(id);
            if (def == null) return false;

            // G1 is always unlocked
            if (id == "G1") return true;

            // Check total ore requirement
            if (def.unlockOre > 0 && totalEarned < def.unlockOre) return false;

            // Check prerequisite generator level
            if (!string.IsNullOrEmpty(def.unlockGenId) && def.unlockLevel > 0)
            {
                int prereqLevel = GetGeneratorLevel(def.unlockGenId);
                if (prereqLevel < def.unlockLevel) return false;
            }

            return true;
        }

        public bool CanAfford(string id, double currentOre)
        {
            double cost = GetGeneratorCost(id);
            return currentOre >= cost;
        }

        public Dictionary<string, int> GetAllLevels()
        {
            return new Dictionary<string, int>(_generatorLevels);
        }
    }
}
