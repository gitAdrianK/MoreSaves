namespace MoreSaves.Patching
{
    using System.Reflection;
    using HarmonyLib;
    using JumpKing.MiscEntities.WorldItems.Inventory;
    using JumpKing.MiscSystems.Achievements;
    using JumpKing.SaveThread;

    public class SaveHelper
    {
        private static readonly MethodInfo MethodSaveGeneralSettings;
        private static readonly HarmonyMethod MethodSaveGeneralSettingsPatch;

        private static readonly MethodInfo MethodSaveStats;
        private static readonly HarmonyMethod MethodSaveStatsPatch;

        private static readonly MethodInfo MethodSaveInventory;
        private static readonly HarmonyMethod MethodSaveInventoryPatch;

        static SaveHelper()
        {
            var saveHelper = AccessTools.TypeByName("JumpKing.SaveThread.SaveHelper");

            var save = saveHelper.GetMethod("Save");
            var saveEncrypted = saveHelper.GetMethod("SaveEncrypted");

            MethodSaveGeneralSettings = save.MakeGenericMethod(typeof(GeneralSettings));
            MethodSaveStats = saveEncrypted.MakeGenericMethod(typeof(PlayerStats));
            MethodSaveInventory = saveEncrypted.MakeGenericMethod(typeof(Inventory));

            MethodSaveGeneralSettingsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveGeneralSettings)));
            MethodSaveStatsPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveStats)));
            MethodSaveInventoryPatch = new HarmonyMethod(AccessTools.Method(typeof(SaveHelper), nameof(SaveInventory)));
        }

        public SaveHelper(Harmony harmony)
        {
            _ = harmony.Patch(
                MethodSaveGeneralSettings,
                postfix: MethodSaveGeneralSettingsPatch);

            _ = harmony.Patch(
                MethodSaveStats,
                postfix: MethodSaveStatsPatch);

            _ = harmony.Patch(
                MethodSaveInventory,
                postfix: MethodSaveInventoryPatch);
        }


        public static void SaveGeneralSettings()
        {
            if (ModEntry.SaveName == string.Empty)
            {
                return;
            }
            XmlWrapper.Serialize(SaveLube.GetGeneralSettings(), ModStrings.AUTO, ModEntry.SaveName, ModStrings.SAVES_PERMA);
        }

        public static void SaveStats(string p_file, PlayerStats p_object)
        {
            if (ModEntry.SaveName == string.Empty)
            {
                return;
            }
            Encryption.SavePlayerStats(p_object, p_file, ModStrings.AUTO, ModEntry.SaveName, ModStrings.SAVES_PERMA);
        }

        public static void SaveInventory(Inventory p_object)
        {
            if (ModEntry.SaveName == string.Empty)
            {
                return;
            }
            Encryption.SaveInventory(p_object, ModStrings.AUTO, ModEntry.SaveName, ModStrings.SAVES_PERMA);
        }
    }
}
