namespace MoreSaves.Patching
{
    using System.IO;
    using System.Reflection;
    using HarmonyLib;
    using JumpKing.MiscEntities.WorldItems.Inventory;
    using JumpKing.MiscSystems.Achievements;
    using JumpKing.SaveThread;

    public class Encryption
    {
        private static readonly char SEP;

        private static readonly MethodInfo MethodSaveCombinedSaveFile;
        private static readonly MethodInfo MethodSavePlayerStats;
        private static readonly MethodInfo MethodSaveEventFlags;
        private static readonly MethodInfo MethodSaveInventory;

        static Encryption()
        {
            SEP = Path.DirectorySeparatorChar;

            var encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");

            var saveFile = encryption.GetMethod("SaveFile");
            MethodSaveCombinedSaveFile = saveFile.MakeGenericMethod(typeof(CombinedSaveFile));
            MethodSavePlayerStats = saveFile.MakeGenericMethod(typeof(PlayerStats));
            MethodSaveEventFlags = saveFile.MakeGenericMethod(typeof(EventFlagsSave));
            MethodSaveInventory = saveFile.MakeGenericMethod(typeof(Inventory));
        }

        /// <summary>
        /// Used to save the combined savefile.
        /// Filename is always combined.sav
        /// </summary>
        /// <param name="combinedSave">Combined savefile to save</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SaveCombinedSaveFile(CombinedSaveFile combinedSave, params string[] folders)
        {
            var path = BuildAndCreatePath(folders);
            _ = MethodSaveCombinedSaveFile.Invoke(null, new object[] { $"{path}{ModStrings.COMBINED}", combinedSave });
        }

        /// <summary>
        /// Used to save the given player stats.
        /// </summary>
        /// <param name="playerStats">PlayerStats to save</param>
        /// <param name="name">The name of the file it will be saved as</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SavePlayerStats(PlayerStats playerStats, string name, params string[] folders)
        {
            var path = BuildAndCreatePath(folders);
            _ = MethodSavePlayerStats.Invoke(null, new object[] { $"{path}{name}", playerStats });
        }

        /// <summary>
        /// Used to save the given event flags.
        /// </summary>
        /// <param name="eventFlags">EventFlagsSave to save</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SaveEventFlags(EventFlagsSave eventFlags, params string[] folders)
        {
            var path = BuildAndCreatePath(folders);
            _ = MethodSaveEventFlags.Invoke(null, new object[] { $"{path}{ModStrings.EVENT}", eventFlags });
        }

        /// <summary>
        /// Used to save the given inventory.
        /// </summary>
        /// <param name="inventory">Inventory to save</param>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        public static void SaveInventory(Inventory inventory, params string[] folders)
        {
            var path = BuildAndCreatePath(folders);
            _ = MethodSaveInventory.Invoke(null, new object[] { $"{path}{ModStrings.INVENTORY}", inventory });
        }

        /// <summary>
        /// Builds a path from given folders, starting from the path to the dll. If the path doesn't exist creates it.
        /// </summary>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        /// <returns>The path</returns>
        private static string BuildAndCreatePath(params string[] folders)
        {
            var path = ModEntry.DllDirectory;
            foreach (var folder in folders)
            {
                path += folder + SEP;
                if (!Directory.Exists(path))
                {
                    _ = Directory.CreateDirectory(path);
                }
            }
            return path;
        }
    }
}
