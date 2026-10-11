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

        // Tool and Forge Upgrades
        public int HammerLevel { get; set; } = 1;
        public int FurnaceLevel { get; set; } = 1;
        public int GlovesLevel { get; set; } = 0;
        public int OilLevel { get; set; } = 0;

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

        public int GetUpgradeLevel(string upgradeId)
        {
            switch (upgradeId)
            {
                case "UP_HAMMER": return HammerLevel;
                case "UP_FURNACE": return FurnaceLevel;
                case "UP_GLOVES": return GlovesLevel;
                case "UP_OIL": return OilLevel;
                default: return 0;
            }
        }

        public void SetUpgradeLevel(string upgradeId, int level)
        {
            switch (upgradeId)
            {
                case "UP_HAMMER": HammerLevel = Mathf.Max(1, level); break;
                case "UP_FURNACE": FurnaceLevel = Mathf.Max(1, level); break;
                case "UP_GLOVES": GlovesLevel = Mathf.Clamp(level, 0, 10); break;
                case "UP_OIL": OilLevel = Mathf.Clamp(level, 0, 10); break;
            }
        }

        public double GetUpgradeCost(string upgradeId)
        {
            switch (upgradeId)
            {
                case "UP_HAMMER":
                    return Math.Floor(20.0 * Math.Pow(1.40, Mathf.Max(1, HammerLevel) - 1));
                case "UP_FURNACE":
                    return Math.Floor(80.0 * Math.Pow(1.55, Mathf.Max(1, FurnaceLevel) - 1));
                case "UP_GLOVES":
                    if (GlovesLevel >= 10) return double.MaxValue;
                    return Math.Floor(200.0 * Math.Pow(2.10, GlovesLevel));
                case "UP_OIL":
                    if (OilLevel >= 10) return double.MaxValue;
                    return Math.Floor(400.0 * Math.Pow(1.90, OilLevel));
                default:
                    return double.MaxValue;
            }
        }

        public bool CanAffordUpgrade(string upgradeId, double currentOre)
        {
            if (IsUpgradeMaxLevel(upgradeId)) return false;
            return currentOre >= GetUpgradeCost(upgradeId);
        }

        public bool IsUpgradeMaxLevel(string upgradeId)
        {
            if (upgradeId == "UP_GLOVES") return GlovesLevel >= 10;
            if (upgradeId == "UP_OIL") return OilLevel >= 10;
            return false;
        }

        public GeneratorDefinition GetDefinition(string id)
        {
            return generatorDefinitions.Find(d => d != null && d.id == id);
        }

        public double CalculateGlobalMultiplier()
        {
            int g5Level = GetGeneratorLevel("G5");
            double bonusRate = balanceConfig != null ? balanceConfig.g5GlobalBonusRate : 0.05;
            double g5Bonus = bonusRate * g5Level;

            // Lò nung tăng +15% tổng sản lượng xưởng rèn mỗi cấp
            double furnaceBonus = (Mathf.Max(1, FurnaceLevel) - 1) * 0.15;

            return 1.0 + g5Bonus + furnaceBonus;
        }

        public double CalculateFinalTapPower(out bool isCrit)
        {
            isCrit = false;
            double tapBase = balanceConfig != null ? balanceConfig.tapBase : 1.0;

            // Búa rèn tăng tiến theo cấp
            int hLv = Mathf.Max(1, HammerLevel);
            double hammerBonus = (hLv - 1) * 2.0 + (hLv / 5) * 5.0;

            double tapPower = (tapBase + hammerBonus) * CalculateGlobalMultiplier();

            // Găng tay da rồng: Tỉ lệ Bạo Kích x5 quặng
            if (GlovesLevel > 0)
            {
                float critChance = Mathf.Clamp(GlovesLevel * 0.05f, 0f, 0.50f);
                if (UnityEngine.Random.value < critChance)
                {
                    isCrit = true;
                    tapPower *= 5.0;
                }
            }

            return tapPower;
        }

        public double CalculateFinalTapPower()
        {
            return CalculateFinalTapPower(out _);
        }

        public double CalculateFinalDps()
        {
            double sumOutput = 0.0;
            double outputMult = balanceConfig != null ? balanceConfig.outputMult : 1.12;

            // Dầu tôi kim loại tăng +15% DPS máy móc mỗi cấp
            double oilBonusMult = 1.0 + (Mathf.Max(0, OilLevel) * 0.15);

            foreach (var def in generatorDefinitions)
            {
                if (def == null) continue;
                int level = GetGeneratorLevel(def.id);
                sumOutput += def.GetOutput(level, outputMult) * oilBonusMult;
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
