using System;
using System.Diagnostics;
using System.Reflection;

namespace MoreSaves.Patching
{
    public class SaveHelper
    {
        private static readonly MethodInfo saveGeneralSettings;
        private static readonly MethodInfo saveCombinedSaveFile;
        private static readonly MethodInfo saveEventFlags;
        private static readonly MethodInfo saveStats;
        private static readonly MethodInfo saveInventory;

        private static readonly HarmonyMethod saveGeneralSettingsPatch;
        private static readonly HarmonyMethod saveCombinedSaveFilePatch;
        private static readonly HarmonyMethod saveEventFlagsPatch;
        private static readonly HarmonyMethod saveStatsPatch;
        private static readonly HarmonyMethod saveInventoryPatch;

        static SaveHelper()
        {
            Type saveHelper = AccessTools.TypeByName("JumpKing.SaveThread.SaveHelper");

            MethodInfo save = saveHelper.GetMethod("Save");
            MethodInfo saveEncrypted = saveHelper.GetMethod("SaveEncrypted");

            saveGeneralSettings = save.MakeGenericMethod(typeof(GeneralSettings));
            saveEventFlags = saveEncrypted.MakeGenericMethod(typeof(EventFlagsSave));
            saveStats = saveEncrypted.MakeGenericMethod(typeof(PlayerStats));
            saveInventory = saveEncrypted.MakeGenericMethod(typeof(Inventory));

            saveGeneralSettingsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveGeneralSettings)));
            saveEventFlagsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveEventFlags)));
            saveStatsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveStats)));
            saveInventoryPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveInventory)));
        }

        public SaveHelper()
        {
            ModEntry.harmony.Patch(
                saveGeneralSettings,
                postfix: saveGeneralSettingsPatch
            );
            ModEntry.harmony.Patch(
                saveEventFlags,
                postfix: saveEventFlagsPatch
            );
            ModEntry.harmony.Patch(
                saveStats,
                postfix: saveStatsPatch
            );
            ModEntry.harmony.Patch(
                saveInventory,
                postfix: saveInventoryPatch
            );
        }

        public static void SaveGeneralSettings(string p_folder, string p_file, GeneralSettings p_object)
        {
            Debugger.Log(1, "", ">A> GeneralSettings: " + p_folder + "/" + p_file + " " + ModEntry.saveName + "\n");
        }

        public static void SaveEventFlags(string p_folder, string p_file, EventFlagsSave p_object)
        {
            Debugger.Log(1, "", ">B> EventFlags: " + p_folder + "/" + p_file + " " + ModEntry.saveName + "\n");
        }

        public static void SaveStats(string p_folder, string p_file, PlayerStats p_object)
        {
            if (p_file == ModStrings.PERMANENT)
            {
                Debugger.Log(1, "", ">C> PermaStats: " + p_folder + "/" + p_file + " " + ModEntry.saveName + "\n");
            }
            else
            {
                Debugger.Log(1, "", ">D> AttemptStats: " + p_folder + "/" + p_file + " " + ModEntry.saveName + "\n");
            }
        }

        public static void SaveInventory(string p_folder, string p_file, Inventory p_object)
        {
            Debugger.Log(1, "", ">E> Inventory: " + p_folder + "/" + p_file + " " + ModEntry.saveName + "\n");
        }
    }
}
