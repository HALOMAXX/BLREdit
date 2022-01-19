using System.Collections.Generic;

namespace BLREdit
{
    public class ImportWeapons
    {
        public ImportItem[] depot { get; set; }
        public ImportItem[] primary { get; set; }
        public ImportItem[] secondary { get; set; }

        internal void AssignWikiStats(WikiStats[] stats)
        {
            ImportSystem.AssignWikiStatsTo(depot, stats);
            ImportSystem.AssignWikiStatsTo(primary, stats);
            ImportSystem.AssignWikiStatsTo(secondary, stats);
        }

        public WikiStats[] GetWikiStats()
        {
            List<WikiStats> stats = new List<WikiStats>();
            stats.AddRange(ImportSystem.GetWikiStats(depot));
            stats.AddRange(ImportSystem.GetWikiStats(primary));
            stats.AddRange(ImportSystem.GetWikiStats(secondary));
            return stats.ToArray();
        }

        public void UpdateImages()
        {
            ImportSystem.UpdateImagesForImportItems(depot, "Depot");
            ImportSystem.UpdateImagesForImportItems(primary, "Primary");
            ImportSystem.UpdateImagesForImportItems(secondary, "Secondary");
        }

        public ImportWeapons() { }
        public ImportWeapons(ImportWeapons weapons)
        {
            depot = ImportSystem.CleanItems(weapons.depot, "Depot");
            primary = ImportSystem.CleanItems(weapons.primary, "Primary");
            secondary = ImportSystem.CleanItems(weapons.secondary, "Secondary");
        }

        public override string ToString()
        {
            string TextWall = "{";

            TextWall += "Depot:\n";
            foreach (ImportItem item in depot)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nPrimaries:\n";
            foreach (ImportItem item in primary)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nSecondaries:\n";
            foreach (ImportItem item in secondary)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            return TextWall + "}";
        }
    }
}
