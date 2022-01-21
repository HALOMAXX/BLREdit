using System.Collections.Generic;

namespace BLREdit
{
    public class ImportGear
    {
        public ImportItem[] attachments { get; set; }
        public ImportItem[] avatars { get; set; }
        public ImportItem[] badges { get; set; }
        public object[] crosshairs { get; set; }
        public ImportItem[] emotes { get; set; }
        public ImportItem[] hangers { get; set; }
        public ImportItem[] helmets { get; set; }
        public ImportItem[] lowerBodies { get; set; }
        public ImportItem[] tactical { get; set; }
        public ImportItem[] upperBodies { get; set; }

        internal void AssignWikiStats(WikiStats[] stats)
        {
            ImportSystem.AssignWikiStatsTo(attachments, stats);
            ImportSystem.AssignWikiStatsTo(avatars, stats);
            ImportSystem.AssignWikiStatsTo(badges, stats);
            ImportSystem.AssignWikiStatsTo(emotes, stats);
            ImportSystem.AssignWikiStatsTo(hangers, stats);
            ImportSystem.AssignWikiStatsTo(helmets, stats);
            ImportSystem.AssignWikiStatsTo(lowerBodies, stats);
            ImportSystem.AssignWikiStatsTo(tactical, stats);
            ImportSystem.AssignWikiStatsTo(upperBodies, stats);
        }

        public WikiStats[] GetWikiStats()
        {
            List<WikiStats> stats = new List<WikiStats>();
            stats.AddRange(ImportSystem.GetWikiStats(attachments));
            stats.AddRange(ImportSystem.GetWikiStats(avatars));
            stats.AddRange(ImportSystem.GetWikiStats(badges));
            stats.AddRange(ImportSystem.GetWikiStats(emotes));
            stats.AddRange(ImportSystem.GetWikiStats(hangers));
            stats.AddRange(ImportSystem.GetWikiStats(helmets));
            stats.AddRange(ImportSystem.GetWikiStats(lowerBodies));
            stats.AddRange(ImportSystem.GetWikiStats(tactical));
            stats.AddRange(ImportSystem.GetWikiStats(upperBodies));
            return stats.ToArray();
        }

        public void UpdateImages()
        {
            ImportSystem.UpdateImagesForImportItems(attachments, "Attachments");
            ImportSystem.UpdateImagesForImportItems(avatars, "Avatars");
            ImportSystem.UpdateImagesForImportItems(badges, "Badges");
            ImportSystem.UpdateImagesForImportItems(emotes, "Emotes");
            ImportSystem.UpdateImagesForImportItems(hangers, "Hangers");
            ImportSystem.UpdateImagesForImportItems(helmets, "Helmets");
            ImportSystem.UpdateImagesForImportItems(lowerBodies, "LowerBodies");
            ImportSystem.UpdateImagesForImportItems(tactical, "Tactical");
            ImportSystem.UpdateImagesForImportItems(upperBodies, "UpperBodies");
        }

        public ImportGear() { }
        public ImportGear(ImportGear gear)
        {
            attachments = ImportSystem.CleanItems(gear.attachments, "Attachments");
            avatars = ImportSystem.CleanItems(gear.avatars, "Avatars");
            badges = ImportSystem.CleanItems(gear.badges, "Badges");
            emotes = ImportSystem.CleanItems(gear.emotes, "Emotes");
            hangers = ImportSystem.CleanItems(gear.hangers, "Hangers");
            helmets = ImportSystem.CleanItems(gear.helmets, "Helmets");
            lowerBodies = ImportSystem.CleanItems(gear.lowerBodies, "LowerBodies");
            tactical = ImportSystem.CleanItems(gear.tactical, "Tactical");
            upperBodies = ImportSystem.CleanItems(gear.upperBodies, "UpperBodies");
        }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
