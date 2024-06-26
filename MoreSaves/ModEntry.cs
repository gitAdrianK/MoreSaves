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
using System.Text.RegularExpressions;

namespace MoreSaves
{
    [JumpKingMod(ModStrings.MODNAME)]
    public static class ModEntry
    {
        private static readonly char SEP;

        private const string AUTO = ModStrings.AUTO;
        private const string MANUAL = ModStrings.MANUAL;

        public static string dllDirectory;
        public static string exeDirectory;

        public static string saveName;

        public static Harmony harmony;

        public static EndingManager endingManager;
        public static SaveHelper saveHelper;
        public static SaveLube saveLube;

        [MainMenuItemSetting]
        public static TextButton LoadAutoSavefile(object factory, GuiFormat format)
        {
            ModelLoadOptions.SetupButtons();
            return new TextButton("Load Automatic Save",
                ModelLoadOptions.CreateLoadOptions(factory, format, 0, ModelLoadOptions.PageOption.Auto));
        }

        [MainMenuItemSetting]
        public static TextButton LoadManualSavefile(object factory, GuiFormat format)
        {
            ModelLoadOptions.SetupButtons();
            return new TextButton("Load Manual Save",
                ModelLoadOptions.CreateLoadOptions(factory, format, 0, ModelLoadOptions.PageOption.Manual));
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

        static ModEntry()
        {
            SEP = Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Called by Jump King before the level loads
        /// </summary>
        [BeforeLevelLoad]
        public static void BeforeLevelLoad()
        {
            //Debugger.Launch();

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

            endingManager = new EndingManager();
            saveHelper = new SaveHelper();
            saveLube = new SaveLube();
        }

        /// <summary>
        /// Called by Jump King when the Level Starts
        /// </summary>
        [OnLevelStart]
        public static void OnLevelStart()
        {
            saveName = SanitizeName(GetSaveName());

            XmlWrapper.Serialize(SaveLube.GetGeneralSettings(), ModStrings.AUTO, saveName, ModStrings.SAVES_PERMA);
            Encryption.SaveInventory(InventoryManager.GetInventory(), ModStrings.AUTO, saveName, ModStrings.SAVES_PERMA);
            Encryption.SaveEventFlags(EventFlagsSave.Save, AUTO, saveName, ModStrings.SAVES_PERMA);
            Encryption.SavePlayerStats(AchievementManager.GetPlayerStats(), ModStrings.STATS, AUTO, saveName, ModStrings.SAVES_PERMA);
            Encryption.SavePlayerStats(AchievementManager.GetPermaStats(), ModStrings.PERMANENT, AUTO, saveName, ModStrings.SAVES_PERMA);
        }

        /// <summary>
        /// Called by Jump King when the Level Ends
        /// </summary>
        [OnLevelEnd]
        public static void OnLevelEnd()
        {
            saveName = string.Empty;
        }

        private static string GetSaveName()
        {
            JKContentManager contentManager = Game1.instance.contentManager;
            if (contentManager == null)
            {
                // The contentManager doesn't exist when started through WS
                return "Debug";
            }

            if (contentManager.level != null)
            {
                return contentManager.level.Name;
            }

            if (EventFlagsSave.ContainsFlag(StoryEventFlags.StartedNBP))
            {
                return language.GAMETITLESCREEN_NEW_BABE_PLUS;
            }
            if (EventFlagsSave.ContainsFlag(StoryEventFlags.StartedGhost))
            {
                return language.GAMETITLESCREEN_GHOST_OF_THE_BABE;
            }
            return language.GAMETITLESCREEN_NEW_GAME;
        }

        private static string SanitizeName(string name)
        {
            name = name.Trim();
            if (name == string.Empty)
            {
                name = "Save_emptyName";
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '#');
            }
            foreach (char c in Path.GetInvalidPathChars())
            {
                name = name.Replace(c, '#');
            }
            name = Regex.Replace(name, "^\\.\\.$", ". .");
            name = Regex.Replace(name, "^[c|C][o|O][n|N]$", $"Save_{name}");
            name = Regex.Replace(name, "^[p|P][r|R][n|N]$", $"Save_{name}");
            name = Regex.Replace(name, "^[a|A][u|U][x|X]$", $"Save_{name}");
            name = Regex.Replace(name, "^[n|N][u|U][l|L]$", $"Save_{name}");
            name = Regex.Replace(name, "^[c|C][o|O][m|M]\\d$", $"Save_{name}");
            name = Regex.Replace(name, "^[l|L][p|P][t|T]\\d$", $"Save_{name}");

            return name;
        }
    }
}
