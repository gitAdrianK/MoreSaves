using BehaviorTree;
using JumpKing;
using JumpKing.SaveThread;
using MoreSaves.Patching;
using System;

namespace MoreSaves.Nodes
{
    /// <summary>
    /// Node that creates a manual save with the name of the map and a slightly modified ISO 8601.
    /// </summary>
    public class NodeCreateManualSave : IBTnode
    {

        const string MANUAL = ModStrings.MANUAL;
        const string SAVES = ModStrings.SAVES;
        const string SAVES_PERMA = ModStrings.SAVES_PERMA;

        protected override BTresult MyRun(TickData p_data)
        {
            if (ModEntry.saveName == string.Empty)
            {
                Game1.instance.contentManager.audio.menu.MenuFail.Play();
                return BTresult.Failure;
            }

            string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string directoryName = $"{ModEntry.saveName}-{date}";

            XmlWrapper.Serialize(SaveLube.GetGeneralSettings(), MANUAL, directoryName, ModStrings.SAVES_PERMA);
            Encryption.SaveInventory(InventoryManager.GetInventory(), MANUAL, directoryName, ModStrings.SAVES_PERMA);
            Encryption.SaveEventFlags(EventFlagsSave.Save, MANUAL, directoryName, ModStrings.SAVES_PERMA);
            Encryption.SavePlayerStats(AchievementManager.GetPlayerStats(), ModStrings.STATS, MANUAL, directoryName, ModStrings.SAVES_PERMA);
            Encryption.SavePlayerStats(AchievementManager.GetPermaStats(), ModStrings.PERMANENT, MANUAL, directoryName, ModStrings.SAVES_PERMA);

            Game1.instance.contentManager.audio.menu.Select.Play();
            return BTresult.Success;
        }
    }
}
