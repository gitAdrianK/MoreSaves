using HarmonyLib;
using JumpKing.MiscSystems.Achievements;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoreSaves.Util
{
    public class SaveUtil
    {
        private const string SAVES = ModStrings.SAVES;
        private const string SAVES_PERMA = ModStrings.SAVES_PERMA;
        private const string CONTENT = ModStrings.CONTENT;

        private static readonly char SEP;

        private static readonly string CONTENT_SAVES;
        private static readonly string CONTENT_SAVES_PERMA;

        private static readonly string[] WHITELIST = {
            ModStrings.COMBINED,
            ModStrings.EVENT,
            ModStrings.SETTINGS,
            ModStrings.INVENTORY,
        };

        private static readonly Type encryption;
        private static readonly Type achievementManager;

        private static readonly MethodInfo saveFile;
        private static readonly MethodInfo savePlayerStats;

        private static readonly Traverse playerStats;
        private static readonly Traverse permaStats;

        static SaveUtil()
        {
            SEP = Path.DirectorySeparatorChar;
            CONTENT_SAVES = $"{CONTENT}{SEP}{SAVES}{SEP}";
            CONTENT_SAVES_PERMA = $"{CONTENT}{SEP}{SAVES_PERMA}{SEP}";

            encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");
            achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");

            saveFile = encryption.GetMethod("SaveFile");
            savePlayerStats = saveFile.MakeGenericMethod(typeof(PlayerStats));

            object achievementManagerInstance = achievementManager.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Traverse achievementManagerTraverse = Traverse.Create(achievementManagerInstance);
            playerStats = achievementManagerTraverse.Field("m_snapshot");
            permaStats = achievementManagerTraverse.Field("m_all_time_stats");
        }

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

            savePlayerStats.Invoke(null, new object[] { $"{intoFolder}{SAVES_PERMA}{SEP}{ModStrings.STATS}", playerStats.GetValue() });
            savePlayerStats.Invoke(null, new object[] { $"{intoFolder}{SAVES_PERMA}{SEP}{ModStrings.PERMANENT}", permaStats.GetValue() });
        }

        /// <summary>
        /// Deletes the auto savefiles of the currently loaded map.
        /// </summary>
        public static void DeleteAutoSaves()
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            string directory = $"{ModEntry.dllDirectory}{SEP}{ModStrings.AUTO}{SEP}{ModEntry.saveName}{SEP}";
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            ModEntry.saveName = string.Empty;
        }
    }
}
