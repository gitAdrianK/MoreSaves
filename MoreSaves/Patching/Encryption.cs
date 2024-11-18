using HarmonyLib;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using System;
using System.IO;
using System.Reflection;

namespace MoreSaves.Patching
{
    public class Encryption
    {
        private static readonly char SEP;

        private static readonly MethodInfo saveCombinedSaveFile;
        private static readonly MethodInfo savePlayerStats;
        private static readonly MethodInfo saveEventFlags;
        private static readonly MethodInfo saveInventory;

        static Encryption()
        {
            SEP = Path.DirectorySeparatorChar;

            Type encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");

            MethodInfo saveFile = encryption.GetMethod("SaveFile");
            saveCombinedSaveFile = saveFile.MakeGenericMethod(typeof(CombinedSaveFile));
            savePlayerStats = saveFile.MakeGenericMethod(typeof(PlayerStats));
            saveEventFlags = saveFile.MakeGenericMethod(typeof(EventFlagsSave));
            saveInventory = saveFile.MakeGenericMethod(typeof(Inventory));
        }

        /// <summary>
        /// Used to save the combined savefile.
        /// Filename is always combined.sav
        /// </summary>
        /// <param name="combinedSave">Combined savefile to save</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SaveCombinedSaveFile(CombinedSaveFile combinedSave, params string[] folders)
        {
            string path = BuildAndCreatePath(folders);
            saveCombinedSaveFile.Invoke(null, new object[] { $"{path}{ModStrings.COMBINED}", combinedSave });
        }

        /// <summary>
        /// Used to save the given player stats.
        /// </summary>
        /// <param name="playerStats">PlayerStats to save</param>
        /// <param name="name">The name of the file it will be saved as</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SavePlayerStats(PlayerStats playerStats, string name, params string[] folders)
        {
            string path = BuildAndCreatePath(folders);
            savePlayerStats.Invoke(null, new object[] { $"{path}{name}", playerStats });
        }

        /// <summary>
        /// Used to save the given event flags.
        /// </summary>
        /// <param name="eventFlags">EventFlagsSave to save</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SaveEventFlags(EventFlagsSave eventFlags, params string[] folders)
        {
            string path = BuildAndCreatePath(folders);
            saveEventFlags.Invoke(null, new object[] { $"{path}{ModStrings.EVENT}", eventFlags });
        }

        /// <summary>
        /// Used to save the given inventory.
        /// </summary>
        /// <param name="inventory">Inventory to save</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SaveInventory(Inventory inventory, params string[] folders)
        {
            string path = BuildAndCreatePath(folders);
            saveInventory.Invoke(null, new object[] { $"{path}{ModStrings.INVENTORY}", inventory });
        }

        /// <summary>
        /// Builds a path from given folders, starting from the path to the dll. If the path doesn't exist creates it.
        /// </summary>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        /// <returns>The path</returns>
        private static string BuildAndCreatePath(params string[] folders)
        {
            string path = ModEntry.dllDirectory;
            foreach (string folder in folders)
            {
                path += folder + SEP;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            return path;
        }
    }
}
