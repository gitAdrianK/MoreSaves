﻿using System.IO;
using System.Linq;

namespace MoreSaves.Util
{
    public class CopyUtil
    {
        private static readonly char SEP = Path.DirectorySeparatorChar;

        private static readonly string SAVES = "Saves";
        private static readonly string SAVES_PERMA = "SavesPerma";

        private static readonly string CONTENT_SAVES = $"Content{SEP}{SAVES}{SEP}";
        private static readonly string CONTENT_SAVES_PERMA = $"Content{SEP}{SAVES_PERMA}{SEP}";

        private static readonly string[] WHITELIST = {
            "combined.sav",
            "attempt_stats.stat",
            "event_flags.set",
            "general_settings.set",
            "inventory.inv",
        };

        /// <summary>
        /// Copys the save files out from the Jump King directory.
        /// </summary>
        /// <param name="folders">The folder they are supposed to be copied into. Where every string is a subfolder.</param>
        public static void CopyOutSaves(params string[] folders)
        {
            string from = ModEntry.exeDirectory;

            string into = ModEntry.dllDirectory;
            string intoFolder = into;
            foreach (string folder in folders)
            {
                intoFolder += $"{folder}{SEP}";
            }

            if (!Directory.Exists(intoFolder))
            {
                Directory.CreateDirectory(intoFolder);
            }
            if (!Directory.Exists($"{intoFolder}{SEP}{SAVES}"))
            {
                Directory.CreateDirectory($"{intoFolder}{SEP}{SAVES}");
            }
            if (!Directory.Exists($"{intoFolder}{SEP}{SAVES_PERMA}"))
            {
                Directory.CreateDirectory($"{intoFolder}{SEP}{SAVES_PERMA}");
            }

            foreach (string filePath in Directory.GetFiles($"{from}{CONTENT_SAVES}"))
            {
                string file = filePath.Split(SEP).Last();
                if (!WHITELIST.Contains(file))
                {
                    continue;
                }
                File.Copy(
                    filePath,
                    $"{intoFolder}{SAVES}{SEP}{file}",
                    true
                );
            }
            foreach (string filePath in Directory.GetFiles($"{from}{CONTENT_SAVES_PERMA}"))
            {
                string file = filePath.Split(SEP).Last();
                if (!WHITELIST.Contains(file))
                {
                    continue;
                }
                File.Copy(
                    filePath,
                    $"{intoFolder}{SAVES_PERMA}{SEP}{file}",
                    true
                );
            }
        }
    }
}
