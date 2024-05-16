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
    [JumpKingMod(ModStrings.MODNAME)]
    public static class ModEntry
    {
        private const string AUTO = ModStrings.AUTO;
        private const string MANUAL = ModStrings.MANUAL;

        public static string dllDirectory;
        public static string exeDirectory;

        public static string saveName;

        public static Harmony harmony;
        public static SaveLube saveLube;
        public static EndingManager endingManager;

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
            // Debugger.Launch();

            dllDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}";
            exeDirectory = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}{Path.DirectorySeparatorChar}";

            if (!Directory.Exists($"{dllDirectory}{MANUAL}"))
            {
                Directory.CreateDirectory($"{dllDirectory}{MANUAL}");
            }
            if (!Directory.Exists($"{dllDirectory}{AUTO}"))
            {
                Directory.CreateDirectory($"{dllDirectory}{AUTO}");
            }

            ModelLoadOptions.SetupButtons();

            saveName = string.Empty;

            harmony = new Harmony(ModStrings.MODNAME);
            saveLube = new SaveLube();
            endingManager = new EndingManager();
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
            saveName = Regex.Replace(saveName, "^\\.\\.$", ". .");
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
