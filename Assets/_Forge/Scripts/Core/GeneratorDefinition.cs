using System;
using UnityEngine;

namespace Forge
{
    [CreateAssetMenu(fileName = "Generator_Def", menuName = "Forge/Generator Definition")]
    public class GeneratorDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id; // G1, G2, G3, G4, G5
        public string machineName;
        [TextArea(2, 3)]
        public string description;

        [Header("Economy Values")]
        public double baseCost = 15;
        public double baseOutput = 0.5;

        [Header("Unlock Requirements")]
        public double unlockOre = 0;
        public string unlockGenId = "";
        public int unlockLevel = 0;

        [Header("Visuals")]
        public GameObject prefab;
        public Sprite icon;

        /// <summary>
        /// cost(next) = floor(baseCost * 1.15 ^ currentLevel)
        /// level 0 -> baseCost (to buy level 1)
        /// </summary>
        public double GetCost(int currentLevel, double costMult = 1.15)
        {
            if (currentLevel < 0) currentLevel = 0;
            return Math.Floor(baseCost * Math.Pow(costMult, currentLevel));
        }

        /// <summary>
        /// output(lv) = baseOutput * 1.12 ^ (lv - 1)
        /// lv 0 = 0 (unpurchased)
        /// </summary>
        public double GetOutput(int currentLevel, double outputMult = 1.12)
        {
            if (currentLevel <= 0) return 0.0;
            return baseOutput * Math.Pow(outputMult, currentLevel - 1);
        }
    }
}
