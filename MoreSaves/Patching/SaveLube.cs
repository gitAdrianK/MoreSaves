using HarmonyLib;
using JumpKing.SaveThread;
using MoreSaves.Util;
using System;
using System.Reflection;

namespace MoreSaves.Patching
{
    /// <summary>
    /// Patches the SaveLube class. 
    /// Function SaveCombinedSaveFile to also save at our mod location.
    /// Function DeleteSaves to also delete the saves inside the auto folder.
    /// </summary>
    public class SaveLube
    {
        private static readonly MethodInfo saveCombinedSaveFile;
        private static readonly HarmonyMethod savePatch;

        private static readonly MethodInfo deleteSave;
        private static readonly HarmonyMethod deletePatch;

        static SaveLube()
        {
            Type saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");

            MethodInfo generic = saveLube.GetMethod("Save");
            saveCombinedSaveFile = generic.MakeGenericMethod(typeof(CombinedSaveFile));
            savePatch = new HarmonyMethod(AccessTools.Method(typeof(SaveLube), nameof(CopySavefile)));

            deleteSave = saveLube.GetMethod("DeleteSaves");
            deletePatch = new HarmonyMethod(AccessTools.Method(typeof(SaveLube), nameof(DeleteSaves)));
        }

        public SaveLube()
        {
            ModEntry.harmony.Patch(
                saveCombinedSaveFile,
                postfix: savePatch
            );

            ModEntry.harmony.Patch(
                deleteSave,
                postfix: deletePatch
            );
        }

        // CONSIDER: It could probably improved that instead of copying ALL files out whenever savelube
        // saves to instead just copy out the files that are being saved.

        // Problem with this is that certain files are created/saved before the level has loaded, so the saveName is unset.
        // Collecting an item which one would assume saves inventory,
        // instead throws "NotImplementedExceptions". So for now we'll leave it at this.
        // However combined is saved constantly so lets not patch all.

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
            SaveUtil.CopyOutSaves(ModStrings.AUTO, ModEntry.saveName);
        }

        /// <summary>
        /// Deletes the savefiles in the dll directory when the give up option in selected in game.
        /// </summary>
        public static void DeleteSaves()
        {
            SaveUtil.DeleteAutoSaves();
        }
    }
}
