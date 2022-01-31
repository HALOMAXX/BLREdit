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
            ImportSystem.UpdateImagesForImportItems(attachments);
            ImportSystem.UpdateImagesForImportItems(avatars);
            ImportSystem.UpdateImagesForImportItems(badges);
            ImportSystem.UpdateImagesForImportItems(emotes);
            ImportSystem.UpdateImagesForImportItems(hangers);
            ImportSystem.UpdateImagesForImportItems(helmets);
            ImportSystem.UpdateImagesForImportItems(lowerBodies);
            ImportSystem.UpdateImagesForImportItems(tactical);
            ImportSystem.UpdateImagesForImportItems(upperBodies);
        }

        public ImportGear() { }
        public ImportGear(ImportGear gear)
        {
            attachments = ImportSystem.CleanItems(gear.attachments, "attachments");
            avatars = ImportSystem.CleanItems(gear.avatars, "avatar");
            badges = ImportSystem.CleanItems(gear.badges, "badge");
            emotes = ImportSystem.CleanItems(gear.emotes, "emote");
            hangers = ImportSystem.CleanItems(gear.hangers, "hanger");
            helmets = ImportSystem.CleanItems(gear.helmets, "helmet");
            lowerBodies = ImportSystem.CleanItems(gear.lowerBodies, "lowerBody");
            tactical = ImportSystem.CleanItems(gear.tactical, "tactical");
            upperBodies = ImportSystem.CleanItems(gear.upperBodies, "upperBody");
        }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
