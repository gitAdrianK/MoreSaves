using BehaviorTree;
using HarmonyLib;
using JumpKing;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.MiscSystems.Achievements;
using JumpKing.SaveThread;
using System;
using System.IO;
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
            SaveManager saveManager = SaveManager.instance;
            try
            {
                ModEntry.shouldPrevent = true;
                saveManager.StopSaving();
                // Optimally start gameplay after the reload.
                Type saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");
                Type encryption = AccessTools.TypeByName("FileUtil.Encryption.Encryption");
                Type achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");

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

                saveCombinedSaveFile.Invoke(null, new object[] { SAVES, COMBINED, combinedSaveFile });
                saveEventFlags.Invoke(null, new object[] { SAVES_PERMA, EVENT, eventFlags });
                savePlayerStats.Invoke(null, new object[] { SAVES_PERMA, STATS, playerStats });
                saveInventory.Invoke(null, new object[] { SAVES_PERMA, INVENTORY, inventory });
                saveGeneralSettings.Invoke(null, new object[] { SAVES_PERMA, SETTINGS, generalSettings });

                saveInit.Invoke(null, null);

                object achievementManagerInstance = achievementManager.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                var s = Traverse.Create(achievementManagerInstance).Field("m_snapshot").SetValue(playerStats);

                // TODO: Set flags.

                Game1.instance.contentManager.audio.menu.Select.Play();
                Game1.instance.m_game.UpdateMenu();
                // CONSIDER: Start gameplay.
            }
            catch (Exception e)
            {
                Game1.instance.contentManager.audio.menu.MenuFail.Play();
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
