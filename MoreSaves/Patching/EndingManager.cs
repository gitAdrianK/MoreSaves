using HarmonyLib;
using MoreSaves.Util;
using System;
using System.Reflection;

namespace MoreSaves.Patching
{
    /// <summary>
    /// Patches the EndingManager class. 
    /// Function DeleteSaves to delete the auto save should an ending be achieved.
    /// </summary>
    public class EndingManager
    {
        private static readonly Type endingManager;

        private static readonly MethodInfo checkWin;
        private static readonly HarmonyMethod deleteSaves;

        static EndingManager()
        {
            endingManager = AccessTools.TypeByName("JumpKing.GameManager.MultiEnding.EndingManager");

            checkWin = endingManager.GetMethod("CheckWin");
            deleteSaves = new HarmonyMethod(AccessTools.Method(typeof(EndingManager), nameof(DeleteSaves)));
        }

        public EndingManager()
        {
            ModEntry.harmony.Patch(
                checkWin,
                postfix: deleteSaves
            );
        }

        /// <summary>
        /// Deletes the savefiles in the dll directory when the ending is achieved.
        /// </summary>
        public static void DeleteSaves(bool __result)
        {
            if (!__result)
            {
                return;
            }
            SaveUtil.DeleteAutoSaves();
        }
    }
}
