using HarmonyLib;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using System;
using System.Reflection;

namespace MoreSaves.Patching
{
    public class SaveHelper
    {
        private static readonly MethodInfo saveGeneralSettings;
        private static readonly HarmonyMethod saveGeneralSettingsPatch;

        private static readonly MethodInfo saveStats;
        private static readonly HarmonyMethod saveStatsPatch;

        private static readonly MethodInfo saveInventory;
        private static readonly HarmonyMethod saveInventoryPatch;

        static SaveHelper()
        {
            Type saveHelper = AccessTools.TypeByName("JumpKing.SaveThread.SaveHelper");

            MethodInfo save = saveHelper.GetMethod("Save");
            MethodInfo saveEncrypted = saveHelper.GetMethod("SaveEncrypted");

            saveGeneralSettings = save.MakeGenericMethod(typeof(GeneralSettings));
            saveStats = saveEncrypted.MakeGenericMethod(typeof(PlayerStats));
            saveInventory = saveEncrypted.MakeGenericMethod(typeof(Inventory));

            saveGeneralSettingsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveGeneralSettings)));
            saveStatsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveStats)));
            saveInventoryPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveInventory)));
        }

        public SaveHelper(Harmony harmony)
        {
            harmony.Patch(
                saveGeneralSettings,
                postfix: saveGeneralSettingsPatch);

            harmony.Patch(
                saveStats,
                postfix: saveStatsPatch);

            harmony.Patch(
                saveInventory,
                postfix: saveInventoryPatch);
        }

        public static void SaveGeneralSettings(GeneralSettings p_object)
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            XmlWrapper.Serialize(SaveLube.GetGeneralSettings(), ModStrings.AUTO, ModEntry.saveName, ModStrings.SAVES_PERMA);
        }

        public static void SaveStats(string p_file, PlayerStats p_object)
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            Encryption.SavePlayerStats(p_object, p_file, ModStrings.AUTO, ModEntry.saveName, ModStrings.SAVES_PERMA);
        }

        public static void SaveInventory(Inventory p_object)
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            Encryption.SaveInventory(p_object, ModStrings.AUTO, ModEntry.saveName, ModStrings.SAVES_PERMA);
        }
    }
}
