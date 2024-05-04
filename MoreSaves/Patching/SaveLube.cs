using HarmonyLib;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using MoreSaves.Util;
using System;
using System.Reflection;

namespace MoreSaves.Patching
{
    /// <summary>
    /// Patches the SaveLube class, function SaveCombinedSaveFile to also save at our mod location.
    /// </summary>
    public class SaveLube
    {
        private static Type saveLube;

        private static MethodInfo originalSave;
        private static MethodInfo saveCombined;
        private static MethodInfo saveEventFlags;
        private static MethodInfo savePlayerStats;
        private static MethodInfo saveInventory;
        private static MethodInfo saveGeneralSettings;
        private static HarmonyMethod patchSave;

        private static MethodInfo originalSCSF;
        private static HarmonyMethod patchSCSF;

        public SaveLube()
        {
            saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");

            originalSave = saveLube.GetMethod("Save");
            saveCombined = originalSave.MakeGenericMethod(typeof(CombinedSaveFile));
            saveEventFlags = originalSave.MakeGenericMethod(typeof(EventFlagsSave));
            savePlayerStats = originalSave.MakeGenericMethod(typeof(PlayerStats));
            saveInventory = originalSave.MakeGenericMethod(typeof(Inventory));
            saveGeneralSettings = originalSave.MakeGenericMethod(typeof(GeneralSettings));

            patchSave = new HarmonyMethod(AccessTools.Method(typeof(SaveLube), nameof(PreventWriting)));

            ModEntry.harmony.Patch(
                saveCombined,
                prefix: patchSave
            );
            ModEntry.harmony.Patch(
                saveEventFlags,
                prefix: patchSave
            );
            ModEntry.harmony.Patch(
                savePlayerStats,
                prefix: patchSave
            );
            ModEntry.harmony.Patch(
                saveInventory,
                prefix: patchSave
            );
            ModEntry.harmony.Patch(
                saveGeneralSettings,
                prefix: patchSave
            );

            originalSCSF = saveLube.GetMethod("SaveCombinedSaveFile");
            patchSCSF = new HarmonyMethod(AccessTools.Method(typeof(SaveLube), nameof(SaveCombinedSaveFile)));

            ModEntry.harmony.Patch(
                originalSCSF,
                postfix: patchSCSF
            );
        }

        public static bool PreventWriting()
        {
            return !ModEntry.shouldPrevent;
        }

        public static void SaveCombinedSaveFile()
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            CopyUtil.CopyOutSaves("auto", ModEntry.saveName);
        }
    }
}
