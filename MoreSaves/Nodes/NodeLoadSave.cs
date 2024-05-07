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
                saveManager.StopSaving();

                // Classes and methods.
                Type saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");
                Type encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");
                Type achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");

                MethodInfo setCombinedSave = saveLube.GetMethod("set_CombinedSave");
                MethodInfo setEventFlags = saveLube.GetMethod("set_eventFlags");
                MethodInfo setPlayerStats = saveLube.GetMethod("set_PlayerStatsAttemptSnapshot");
                MethodInfo setInventory = saveLube.GetMethod("set_inventory");
                MethodInfo setGeneralSettings = saveLube.GetMethod("set_generalSettings");

                MethodInfo loadFile = encryption.GetMethod("LoadFile");
                MethodInfo loadCombinedSaveFile = loadFile.MakeGenericMethod(typeof(CombinedSaveFile));
                MethodInfo loadEventFlags = loadFile.MakeGenericMethod(typeof(EventFlagsSave));
                MethodInfo loadPlayerStats = loadFile.MakeGenericMethod(typeof(PlayerStats));
                MethodInfo loadInventory = loadFile.MakeGenericMethod(typeof(Inventory));

                MethodInfo saveProgramStartInitialize = saveLube.GetMethod("ProgramStartInitialize");
                MethodInfo saveCombinedSaveFile = saveLube.GetMethod("SaveCombinedSaveFile");

                object achievementManagerInstance = achievementManager.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

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
                    level = WorkshopManager.instance.levels.First(lvl => lvl.ID == playerStats.steam_level_id);
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

                setCombinedSave.Invoke(null, new object[] { combinedSaveFile });
                // This is probably not needed with me setting the event flags via EventFlagsSave.Save too.
                setEventFlags.Invoke(null, new object[] { eventFlags });
                setPlayerStats.Invoke(null, new object[] { playerStats });
                setInventory.Invoke(null, new object[] { inventory });
                setGeneralSettings.Invoke(null, new object[] { generalSettings });

                saveCombinedSaveFile.Invoke(null, null);
                saveProgramStartInitialize.Invoke(null, null);

                Traverse.Create(achievementManagerInstance).Field("m_snapshot").SetValue(playerStats);

                EventFlagsSave.Save = eventFlags;

                contentManager.LoadAssets(Game1.instance);
                LevelManager.LoadScreens();

                contentManager.audio.menu.Select.Play();
                Game1.instance.m_game.UpdateMenu();
            }
            catch (Exception e)
            {
                contentManager.audio.menu.MenuFail.Play();
                throw e;
            }
            finally
            {
                saveManager.StartSaving();
            }
            return BTresult.Success;
        }
    }
}
