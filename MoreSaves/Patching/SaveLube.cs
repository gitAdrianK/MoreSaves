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

        public static void CopySavefile()
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            CopyUtil.CopyOutSaves("auto", ModEntry.saveName);
        }

        public static void DeleteSaves()
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            char sep = Path.DirectorySeparatorChar;
            string directory = $"{ModEntry.dllDirectory}{sep}auto{sep}{ModEntry.saveName}{sep}";
            Directory.Delete(directory, true);
            ModEntry.saveName = string.Empty;
        }
    }
}
