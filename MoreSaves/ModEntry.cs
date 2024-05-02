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
using MoreSaves.Util;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

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

        [PauseMenuItemSetting]
        public static TextButton CreateManualSave(object factory, GuiFormat format)
        {
            return new TextButton("Create Manual Save", new NodeCreateManualSave());
        }

        [MainMenuItemSetting]
        [PauseMenuItemSetting]
        public static ExplorerTextButton OpenFolderExplorer(object factory, GuiFormat format)
        {
            return new ExplorerTextButton("Open Saves Folder", new NodeOpenFolderExplorer(), Color.Lime);
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

            saveName = saveName.Trim();
            if (saveName == string.Empty)
            {
                saveName = "Save_emptyName";
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                saveName = saveName.Replace(c, '#');
            }
            foreach (char c in Path.GetInvalidPathChars())
            {
                saveName = saveName.Replace(c, '#');
            }
            saveName = Regex.Replace(saveName, "^[c|C][o|O][n|N]$", $"Save_{saveName}");
            saveName = Regex.Replace(saveName, "^[p|P][r|R][n|N]$", $"Save_{saveName}");
            saveName = Regex.Replace(saveName, "^[a|A][u|U][x|X]$", $"Save_{saveName}");
            saveName = Regex.Replace(saveName, "^[n|N][u|U][l|L]$", $"Save_{saveName}");
            saveName = Regex.Replace(saveName, "^[c|C][o|O][m|M]\\d$", $"Save_{saveName}");
            saveName = Regex.Replace(saveName, "^[l|L][p|P][t|T]\\d$", $"Save_{saveName}");
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
