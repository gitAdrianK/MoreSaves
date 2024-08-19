using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace MoreSaves.Patching
{
    /// <summary>
    /// Patches the EndingManager class. 
    /// Function DeleteSaves to delete the auto save should an ending be achieved.
    /// </summary>
    public class EndingManager
    {
        private static readonly char SEP;

        private static readonly MethodInfo checkWin;
        private static readonly HarmonyMethod deleteSaves;

        static EndingManager()
        {
            SEP = Path.DirectorySeparatorChar;

            Type endingManager = AccessTools.TypeByName("JumpKing.GameManager.MultiEnding.EndingManager");

            checkWin = endingManager.GetMethod("CheckWin");
            deleteSaves = new HarmonyMethod(AccessTools.Method(typeof(EndingManager), nameof(DeleteSaves)));
        }

        public EndingManager(Harmony harmony)
        {
            harmony.Patch(
                checkWin,
                postfix: deleteSaves);
        }

        /// <summary>
        /// Deletes the savefiles in the dll directory when the ending is achieved.
        /// </summary>
        public static void DeleteSaves(bool __result)
        {
            if (!__result || ModEntry.saveName == string.Empty)
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
    }
}
