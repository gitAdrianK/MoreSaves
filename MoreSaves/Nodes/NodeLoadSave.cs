using BehaviorTree;
using HarmonyLib;
using JumpKing;
using JumpKing.Level;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using JumpKing.Workshop;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoreSaves.Nodes
{
    /// <summary>
    /// Node to copy in a save into the Jump King folder.
    /// Force closes the game.
    /// </summary>
    public class NodeLoadSave : IBTnode
    {
        private static readonly char SEP = Path.DirectorySeparatorChar;

        private static readonly string SAVES = "Saves";
        private static readonly string SAVES_PERMA = "SavesPerma";
        private static readonly string COMBINED = "combined.sav";
        private static readonly string EVENT = "event_flags.set";
        private static readonly string STATS = "attempt_stats.stat";
        private static readonly string INVENTORY = "inventory.inv";
        private static readonly string SETTINGS = "general_settings.set";

        /// <summary>
        /// Folders that make up the path to the file.
        /// </summary>
        private string[] folders;

        public NodeLoadSave(params string[] folders)
        {
            this.folders = folders;
        }

        protected override BTresult MyRun(TickData p_data)
        {
            JKContentManager contentManager = Game1.instance.contentManager;
            SaveManager saveManager = SaveManager.instance;
            try
            {
                ModEntry.shouldPrevent = true;
                saveManager.StopSaving();

                // Classes and methods.
                Type saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");
                Type encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");
                Type achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");
                Type titleScreen = AccessTools.TypeByName("JumpKing.GameManager.TitleScreen.GameTitleScreen");

                MethodInfo saveWithoutWrite = saveLube.GetMethod("SaveWithoutWrite", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo saveCombinedSaveFile = saveWithoutWrite.MakeGenericMethod(typeof(CombinedSaveFile));
                MethodInfo saveEventFlags = saveWithoutWrite.MakeGenericMethod(typeof(EventFlagsSave));
                MethodInfo savePlayerStats = saveWithoutWrite.MakeGenericMethod(typeof(PlayerStats));
                MethodInfo saveInventory = saveWithoutWrite.MakeGenericMethod(typeof(Inventory));
                MethodInfo saveGeneralSettings = saveWithoutWrite.MakeGenericMethod(typeof(GeneralSettings));

                MethodInfo loadFile = encryption.GetMethod("LoadFile");
                MethodInfo loadCombinedSaveFile = loadFile.MakeGenericMethod(typeof(CombinedSaveFile));
                MethodInfo loadEventFlags = loadFile.MakeGenericMethod(typeof(EventFlagsSave));
                MethodInfo loadPlayerStats = loadFile.MakeGenericMethod(typeof(PlayerStats));
                MethodInfo loadInventory = loadFile.MakeGenericMethod(typeof(Inventory));

                MethodInfo saveInit = saveLube.GetMethod("ProgramStartInitialize");

                object achievementManagerInstance = achievementManager.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

                // Load from dllDirectory
                string directory = ModEntry.dllDirectory;
                foreach (string folder in folders)
                {
                    directory += folder + SEP;
                }

                CombinedSaveFile combinedSaveFile = (CombinedSaveFile)loadCombinedSaveFile.Invoke(null, new object[] { $"{directory}{SEP}{SAVES}{SEP}{COMBINED}" });
                EventFlagsSave eventFlags = (EventFlagsSave)loadEventFlags.Invoke(null, new object[] { $"{directory}{SEP}{SAVES_PERMA}{SEP}{EVENT}" });
                PlayerStats playerStats = (PlayerStats)loadPlayerStats.Invoke(null, new object[] { $"{directory}{SEP}{SAVES_PERMA}{SEP}{STATS}" });
                Inventory inventory = (Inventory)loadInventory.Invoke(null, new object[] { $"{directory}{SEP}{SAVES_PERMA}{SEP}{INVENTORY}" });
                GeneralSettings generalSettings = XmlSerializerHelper.Deserialize<GeneralSettings>($"{directory}{SEP}{SAVES_PERMA}{SEP}{SETTINGS}");

                // Root and level.
                string root;
                Level level = null;

                if (playerStats.steam_level_id == null)
                {
                    root = "Content";
                }
                else
                {
                    level = WorkshopManager.instance.levels.Where(lvl => lvl.ID == playerStats.steam_level_id).Single();
                    root = level.Root;
                }

                // Save and set
                Game1.instance.contentManager.ReinitializeAssets();

                if (root == "Content")
                {
                    contentManager.SetLevel(root);
                }
                else
                {
                    contentManager.SetLevel(root, level);
                }

                saveCombinedSaveFile.Invoke(null, new object[] { SAVES, COMBINED, combinedSaveFile });
                saveEventFlags.Invoke(null, new object[] { SAVES_PERMA, EVENT, eventFlags });
                savePlayerStats.Invoke(null, new object[] { SAVES_PERMA, STATS, playerStats });
                saveInventory.Invoke(null, new object[] { SAVES_PERMA, INVENTORY, inventory });
                saveGeneralSettings.Invoke(null, new object[] { SAVES_PERMA, SETTINGS, generalSettings });
                saveInit.Invoke(null, null);

                Traverse.Create(achievementManagerInstance).Field("m_snapshot").SetValue(playerStats);

                contentManager.LoadAssets(Game1.instance);
                LevelManager.LoadScreens();

                // BUG: Nvm, what doesn't seem to work now are the vanilla maps.
                // Workshop maps seem fine

                contentManager.audio.menu.Select.Play();
                Game1.instance.m_game.UpdateMenu();
                // CONSIDER: Start gameplay.
            }
            catch (Exception e)
            {
                contentManager.audio.menu.MenuFail.Play();
                throw e;
            }
            finally
            {
                saveManager.StartSaving();
                ModEntry.shouldPrevent = false;
            }
            return BTresult.Success;
        }
    }
}
