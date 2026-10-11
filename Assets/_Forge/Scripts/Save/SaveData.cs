using System;
using System.Collections.Generic;

namespace Forge
{
    [Serializable]
    public class GeneratorSaveEntry
    {
        public string id;
        public int level;

        public GeneratorSaveEntry() { }
        public GeneratorSaveEntry(string id, int level)
        {
            this.id = id;
            this.level = level;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public double ore = 0.0;
        public double totalEarned = 0.0;
        public int tapLevel = 1;
        public int hammerLevel = 1;
        public int furnaceLevel = 1;
        public int glovesLevel = 0;
        public int oilLevel = 0;
        public string lastSaveUtc = "";
        public List<GeneratorSaveEntry> generators = new List<GeneratorSaveEntry>();
        public List<string> unlockedMilestones = new List<string>();

        public SaveData()
        {
            lastSaveUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            hammerLevel = 1;
            furnaceLevel = 1;
            glovesLevel = 0;
            oilLevel = 0;
            generators = new List<GeneratorSaveEntry>
            {
                new GeneratorSaveEntry("G1", 0),
                new GeneratorSaveEntry("G2", 0),
                new GeneratorSaveEntry("G3", 0),
                new GeneratorSaveEntry("G4", 0),
                new GeneratorSaveEntry("G5", 0)
            };
            unlockedMilestones = new List<string>();
        }
    }
}
