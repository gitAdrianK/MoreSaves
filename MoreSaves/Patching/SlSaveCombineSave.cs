using HarmonyLib;
using MoreSaves.Util;
using System;
using System.Reflection;

namespace MoreSaves.Patching
{
    /// <summary>
    /// Patches the SaveLube class, function SaveCombinedSaveFile to also save at our mod location.
    /// </summary>
    public class SlSaveCombinedSaveFile
    {
        private static Type saveLube;
        private static MethodInfo original;
        private static HarmonyMethod patch;

        public SlSaveCombinedSaveFile()
        {
            saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");

            original = saveLube.GetMethod("SaveCombinedSaveFile");
            patch = new HarmonyMethod(AccessTools.Method(typeof(SlSaveCombinedSaveFile), nameof(SaveCombinedSaveFile)));

            ModEntry.harmony.Patch(
                original,
                postfix: patch
            );
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
