using BehaviorTree;
using JumpKing;
using MoreSaves.Util;
using System;

namespace MoreSaves.Nodes
{
    /// <summary>
    /// Node that creates a manual save with the name of the map and a slightly modified ISO 8601.
    /// </summary>
    public class NodeCreateManualSave : IBTnode
    {
        protected override BTresult MyRun(TickData p_data)
        {
            if (ModEntry.saveName == string.Empty)
            {
                Game1.instance.contentManager.audio.menu.MenuFail.Play();
                return BTresult.Failure;
            }

            string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string directoryName = $"{ModEntry.saveName}-{date}";

            SaveUtil.CopyOutSaves(ModStrings.MANUAL, directoryName);

            Game1.instance.contentManager.audio.menu.Select.Play();
            return BTresult.Success;
        }
    }
}
