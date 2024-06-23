using HarmonyLib;
using JumpKing.MiscSystems.Achievements;
using System;
using System.Reflection;

namespace MoreSaves.Patching
{
    public class AchievementManager
    {
        private static readonly Type achievementManager;

        private static readonly Traverse playerStats;
        private static readonly Traverse permaStats;

        static AchievementManager()
        {
            achievementManager = AccessTools.TypeByName("JumpKing.MiscSystems.Achievements.AchievementManager");

            object achievementManagerInstance = achievementManager.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Traverse achievementManagerTraverse = Traverse.Create(achievementManagerInstance);

            playerStats = achievementManagerTraverse.Field("m_snapshot");
            permaStats = achievementManagerTraverse.Field("m_all_time_stats");
        }

        public static PlayerStats GetPlayerStats()
        {
            return playerStats.GetValue<PlayerStats>();
        }

        public static PlayerStats GetPermaStats()
        {
            return permaStats.GetValue<PlayerStats>();
        }
    }
}
