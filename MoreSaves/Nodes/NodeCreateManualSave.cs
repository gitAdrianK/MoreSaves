namespace MoreSaves.Nodes
{
    using System;
    using BehaviorTree;
    using JumpKing;
    using JumpKing.SaveThread;
    using MoreSaves.Patching;

    /// <summary>
    /// Node that creates a manual save with the name of the map and a slightly modified ISO 8601.
    /// </summary>
    public class NodeCreateManualSave : IBTnode
    {

        private const string MANUAL = ModStrings.MANUAL;
        private const string SAVES_PERMA = ModStrings.SAVES_PERMA;

        protected override BTresult MyRun(TickData p_data)
        {
            if (ModEntry.SaveName == string.Empty)
            {
                Game1.instance.contentManager.audio.menu.MenuFail.Play();
                return BTresult.Failure;
            }

            var date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var directoryName = $"{ModEntry.SaveName}-{date}";

            XmlWrapper.Serialize(SaveLube.GetGeneralSettings(), MANUAL, directoryName, SAVES_PERMA);
            Encryption.SaveInventory(InventoryManager.GetInventory(), MANUAL, directoryName, SAVES_PERMA);
            Encryption.SaveEventFlags(EventFlagsSave.Save, MANUAL, directoryName, SAVES_PERMA);
            Encryption.SavePlayerStats(AchievementManager.GetPlayerStats(), ModStrings.STATS, MANUAL, directoryName, SAVES_PERMA);
            Encryption.SavePlayerStats(AchievementManager.GetPermaStats(), ModStrings.PERMANENT, MANUAL, directoryName, SAVES_PERMA);
            Encryption.SaveCombinedSaveFile(SaveLube.GetCombinedSaveFile(), MANUAL, directoryName, ModStrings.SAVES);

            Game1.instance.contentManager.audio.menu.Select.Play();
            return BTresult.Success;
        }
    }
}
