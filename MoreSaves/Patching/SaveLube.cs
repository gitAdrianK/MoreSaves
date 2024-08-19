using HarmonyLib;
using JumpKing.SaveThread;
using System;
using System.IO;
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
        private static readonly char SEP;

        private static readonly Traverse combinedSavefile;
        private static readonly Traverse generalSettings;

        private static readonly MethodInfo saveCombinedSaveFile;
        private static readonly HarmonyMethod saveCombinedPatch;

        private static readonly MethodInfo deleteSave;
        private static readonly HarmonyMethod deletePatch;

        static SaveLube()
        {
            SEP = Path.DirectorySeparatorChar;

            Type saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");

            combinedSavefile = Traverse.Create(saveLube).Property("CombinedSave");
            generalSettings = Traverse.Create(saveLube).Property("generalSettings");

            MethodInfo genericSave = saveLube.GetMethod("Save");
            saveCombinedSaveFile = genericSave.MakeGenericMethod(typeof(CombinedSaveFile));
            saveCombinedPatch = new HarmonyMethod(typeof(SaveLube).GetMethod(nameof(SaveCombinedSaveFile)));

            deleteSave = saveLube.GetMethod("DeleteSaves");
            deletePatch = new HarmonyMethod(typeof(SaveLube).GetMethod(nameof(DeleteSaves)));
        }

        public SaveLube(Harmony harmony)
        {
            harmony.Patch(
                saveCombinedSaveFile,
                postfix: saveCombinedPatch
            );

            harmony.Patch(
                deleteSave,
                postfix: deletePatch
            );
        }

        public static void SaveCombinedSaveFile(CombinedSaveFile p_object)
        {
            if (ModEntry.saveName == string.Empty)
            {
                return;
            }
            Encryption.SaveCombinedSaveFile(p_object, ModStrings.AUTO, ModEntry.saveName, ModStrings.SAVES);
            Encryption.SavePlayerStats(AchievementManager.GetPermaStats(), ModStrings.PERMANENT, ModStrings.AUTO, ModEntry.saveName, ModStrings.SAVES_PERMA);
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
            string directory = $"{ModEntry.dllDirectory}{SEP}{ModStrings.AUTO}{SEP}{ModEntry.saveName}{SEP}";
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            ModEntry.saveName = string.Empty;
        }

        public static CombinedSaveFile GetCombinedSaveFile()
        {
            return combinedSavefile.GetValue<CombinedSaveFile>();
        }

        public static GeneralSettings GetGeneralSettings()
        {
            return generalSettings.GetValue<GeneralSettings>();
        }
    }
}
