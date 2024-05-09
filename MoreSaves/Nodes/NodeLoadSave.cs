using BehaviorTree;
using HarmonyLib;
using JumpKing;
using JumpKing.Level;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using JumpKing.SaveThread.SaveComponents;
using JumpKing.Workshop;
using System;
using System.Collections.Generic;
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
        private static readonly string PERMANENT = "perma_player_stats.stat";
        private static readonly string INVENTORY = "inventory.inv";
        private static readonly string SETTINGS = "general_settings.set";

        private static readonly JKContentManager contentManager;
        private static readonly SaveManager saveManager;

        private static readonly Type saveLube;
        private static readonly Type encryption;
        private static readonly Type achievementManager;
        private static readonly Type skinManager;

        private static readonly MethodInfo setCombinedSave;
        private static readonly MethodInfo setPlayerStats;
        private static readonly MethodInfo setPermanentStats;
        private static readonly MethodInfo setInventory;
        private static readonly MethodInfo setGeneralSettings;

        private static readonly MethodInfo setSkinEnabled;

        private static readonly MethodInfo loadFile;
        private static readonly MethodInfo loadCombinedSaveFile;
        private static readonly MethodInfo loadEventFlags;
        private static readonly MethodInfo loadPlayerStats;
        private static readonly MethodInfo loadInventory;

        private static readonly MethodInfo saveProgramStartInitialize;
        private static readonly MethodInfo saveCombinedSaveFile;

        private static readonly object achievementManagerInstance;
        private static readonly Traverse achievementManagerTraverse;

        private string directory;

        static NodeLoadSave()
        {
            // Classes and methods.
            contentManager = Game1.instance.contentManager;
            saveManager = SaveManager.instance;

            saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");
            encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");
            achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");
            skinManager = AccessTools.TypeByName("JumpKing.Player.Skins.SkinManager");

            setCombinedSave = saveLube.GetMethod("set_CombinedSave");
            setPlayerStats = saveLube.GetMethod("set_PlayerStatsAttemptSnapshot");
            setPermanentStats = saveLube.GetMethod("set_PermanentPlayerStats");
            setInventory = saveLube.GetMethod("set_inventory");
            setGeneralSettings = saveLube.GetMethod("set_generalSettings");

            setSkinEnabled = skinManager.GetMethod("SetSkinEnabled");

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
                PlayerStats permaStats = (PlayerStats)loadPlayerStats.Invoke(null, new object[] { $"{directory}{SEP}{SAVES_PERMA}{SEP}{PERMANENT}" });
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
                setPlayerStats.Invoke(null, new object[] { playerStats });
                setPermanentStats.Invoke(null, new object[] { permaStats });
                setInventory.Invoke(null, new object[] { inventory });
                setGeneralSettings.Invoke(null, new object[] { generalSettings });
                EventFlagsSave.Save = eventFlags;

                List<ItemEquipOptions.ItemOption> options = new List<ItemEquipOptions.ItemOption>(generalSettings.item_options.Save.options);
                foreach (var option in options)
                {
                    setSkinEnabled.Invoke(null, new object[] { option.item, option.equipped });
                }

                saveCombinedSaveFile.Invoke(null, null);
                saveProgramStartInitialize.Invoke(null, null);

                achievementManagerTraverse.Field("m_snapshot").SetValue(playerStats);
                achievementManagerTraverse.Field("m_all_time_stats").SetValue(permaStats);

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
