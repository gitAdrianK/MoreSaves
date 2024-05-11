using HarmonyLib;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using MoreSaves.Util;
using System;
using System.IO;
using System.Reflection;

namespace MoreSaves.Patching
{
    /// <summary>
    /// Patches the SaveLube class, function SaveCombinedSaveFile to also save at our mod location.
    /// </summary>
    public class SaveLube
    {
        public SaveLube()
        {
            Type saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");

            MethodInfo generic = saveLube.GetMethod("Save");
            MethodInfo saveCombinedSaveFile = generic.MakeGenericMethod(typeof(CombinedSaveFile));
            MethodInfo saveEventFlags = generic.MakeGenericMethod(typeof(EventFlagsSave));
            MethodInfo savePlayerStats = generic.MakeGenericMethod(typeof(PlayerStats));
            MethodInfo saveInventory = generic.MakeGenericMethod(typeof(Inventory));
            MethodInfo saveGeneralSettings = generic.MakeGenericMethod(typeof(GeneralSettings));
            HarmonyMethod savePatch = new HarmonyMethod(AccessTools.Method(typeof(SaveLube), nameof(CopySavefile)));

            ModEntry.harmony.Patch(
                saveCombinedSaveFile,
                postfix: savePatch
            );
            ModEntry.harmony.Patch(
                saveEventFlags,
                postfix: savePatch
            );
            ModEntry.harmony.Patch(
                savePlayerStats,
                postfix: savePatch
            );
            ModEntry.harmony.Patch(
                saveInventory,
                postfix: savePatch
            );
            ModEntry.harmony.Patch(
                saveGeneralSettings,
                postfix: savePatch
            );

            MethodInfo deleteSave = saveLube.GetMethod("DeleteSaves");
            HarmonyMethod deletePatch = new HarmonyMethod(AccessTools.Method(typeof(SaveLube), nameof(DeleteSaves)));

            ModEntry.harmony.Patch(
                deleteSave,
                postfix: deletePatch
            );
        }

        // CONSIDER: It could probably improved that instead of copying ALL files out whenever savelube
        // saves to instead just copy out the files that are being saved. 
        /// <summary>
        /// Copies the required save files out from the games Saves and SavesPerma folder into a folder in
        /// the auto subfolder of the mod dll with the name of the saveName.
        /// </summary>
        public static void CopySavefile()
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            CopyUtil.CopyOutSaves("auto", ModEntry.saveName);
        }

        /// <summary>
        /// Deletes the savefiles in the dll directory when the give up option in selected in game.
        /// </summary>
        public static void DeleteSaves()
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            char sep = Path.DirectorySeparatorChar;
            string directory = $"{ModEntry.dllDirectory}{sep}auto{sep}{ModEntry.saveName}{sep}";
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            ModEntry.saveName = string.Empty;
        }
    }
}
