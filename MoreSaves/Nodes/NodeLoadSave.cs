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
    /// Node to load a save from the mod into Jump King.
    /// All required fields will be set and the JK menu will reload/update.
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

        private static JKContentManager contentManager;
        private static SaveManager saveManager;

        private static Type saveLube;
        private static Type encryption;
        private static Type achievementManager;

        private static MethodInfo setCombinedSave;
        private static MethodInfo setEventFlags;
        private static MethodInfo setPlayerStats;
        private static MethodInfo setInventory;
        private static MethodInfo setGeneralSettings;

        private static MethodInfo loadFile;
        private static MethodInfo loadCombinedSaveFile;
        private static MethodInfo loadEventFlags;
        private static MethodInfo loadPlayerStats;
        private static MethodInfo loadInventory;

        private static MethodInfo saveProgramStartInitialize;
        private static MethodInfo saveCombinedSaveFile;

        private static object achievementManagerInstance;
        private static Traverse achievementManagerTraverse;

        private string directory;

        static NodeLoadSave()
        {
            // Classes and methods.
            contentManager = Game1.instance.contentManager;
            saveManager = SaveManager.instance;

            saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");
            encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");
            achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");

            setCombinedSave = saveLube.GetMethod("set_CombinedSave");
            setEventFlags = saveLube.GetMethod("set_eventFlags");
            setPlayerStats = saveLube.GetMethod("set_PlayerStatsAttemptSnapshot");
            setInventory = saveLube.GetMethod("set_inventory");
            setGeneralSettings = saveLube.GetMethod("set_generalSettings");

            loadFile = encryption.GetMethod("LoadFile");
            loadCombinedSaveFile = loadFile.MakeGenericMethod(typeof(CombinedSaveFile));
            loadEventFlags = loadFile.MakeGenericMethod(typeof(EventFlagsSave));
            loadPlayerStats = loadFile.MakeGenericMethod(typeof(PlayerStats));
            loadInventory = loadFile.MakeGenericMethod(typeof(Inventory));

            saveProgramStartInitialize = saveLube.GetMethod("ProgramStartInitialize");
            saveCombinedSaveFile = saveLube.GetMethod("SaveCombinedSaveFile");

            achievementManagerInstance = achievementManager.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            achievementManagerTraverse = Traverse.Create(achievementManagerInstance);
        }

        public NodeLoadSave(params string[] folders)
        {
            directory = ModEntry.dllDirectory;
            foreach (string folder in folders)
            {
                directory += folder + SEP;
            }
        }

        protected override BTresult MyRun(TickData p_data)
        {
            try
            {
                saveManager.StopSaving();

                // Load from dllDirectory
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
                contentManager.ReinitializeAssets();

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

                achievementManagerTraverse.Field("m_snapshot").SetValue(playerStats);

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
