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
            deleteSaves = new HarmonyMethod(AccessTools.Method(typeof(EndingManager), nameof(IsVictory)));
        }

        public EndingManager(Harmony harmony)
        {
            harmony.Patch(
                checkWin,
                postfix: deleteSaves);
        }

        /// <summary>
        /// Notes that a victory has happened.
        /// </summary>
        public static void IsVictory(bool __result)
        {
            if (!__result || ModEntry.saveName == string.Empty)
            {
                return;
            }
            ModEntry.isVictory = true;
        }
    }
}
