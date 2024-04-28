using HarmonyLib;
using JumpKing;
using JumpKing.Mods;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using JumpKing.SaveThread;
using LanguageJK;
using Microsoft.Xna.Framework;
using MoreSaves.Models;
using MoreSaves.Nodes;
using MoreSaves.Patching;
using System.IO;
using System.Reflection;

namespace MoreSaves
{
    [JumpKingMod("Zebra.MoreSaves")]
    public static class ModEntry
    {
        public static string saveName;

        public static Harmony harmony;
        public static SlSaveCombinedSaveFile slSaveCombinedSaveFile;
        public static bool isNewRun;

        [MainMenuItemSetting]
        public static TextButton LoadSavefile(object factory, GuiFormat format)
        {
            ModelLoadOptions.SetupButtons();
            return new TextButton("Load", ModelLoadOptions.CreateLoadOptions(factory, format, 0));
        }

        [MainMenuItemSetting]
        public static ExplorerTextButton OpenFolderExplorer(object factory, GuiFormat format)
        {
            return new ExplorerTextButton("Open Save Folder", new NodeOpenFolderExplorer(), Color.Lime);
        }

        [PauseMenuItemSetting]
        public static TextButton CreateManualSave(object factory, GuiFormat format)
        {
            return new TextButton("Create Manual Save", new NodeCreateManualSave());
        }

        /// <summary>
        /// Called by Jump King before the level loads
        /// </summary>
        [BeforeLevelLoad]
        public static void BeforeLevelLoad()
        {
            //Debugger.Launch();
            string dllDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}";
            if (!Directory.Exists($"{dllDirectory}manual"))
            {
                Directory.CreateDirectory($"{dllDirectory}manual");
            }
            if (!Directory.Exists($"{dllDirectory}auto"))
            {
                Directory.CreateDirectory($"{dllDirectory}auto");
            }

            ModelLoadOptions.SetupButtons();

            saveName = string.Empty;

            harmony = new Harmony("Zebra.MoreSaves");
            slSaveCombinedSaveFile = new SlSaveCombinedSaveFile();
        }

        /// <summary>
        /// Called by Jump King when the Level Starts
        /// </summary>
        [OnLevelStart]
        public static void OnLevelStart()
        {
            JKContentManager contentManager = Game1.instance.contentManager;
            if (contentManager.level != null)
            {
                // Surely the WS disallows illegal characters in file names and I don't have to do jack.
                saveName = contentManager.level.Name;
            }
            else
            {
                if (EventFlagsSave.ContainsFlag(StoryEventFlags.StartedNBP))
                {
                    saveName = language.GAMETITLESCREEN_NEW_BABE_PLUS;
                }
                else if (EventFlagsSave.ContainsFlag(StoryEventFlags.StartedGhost))
                {
                    saveName = language.GAMETITLESCREEN_GHOST_OF_THE_BABE;
                }
                else
                {
                    saveName = language.GAMETITLESCREEN_NEW_GAME;
                }
            }
        }

        /// <summary>
        /// Called by Jump King when the Level Ends
        /// </summary>
        [OnLevelEnd]
        public static void OnLevelEnd()
        {
            saveName = string.Empty;
        }
    }
}
