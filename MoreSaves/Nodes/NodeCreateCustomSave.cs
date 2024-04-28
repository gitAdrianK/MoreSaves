using BehaviorTree;
using JumpKing;
using MoreSaves.Util;
using System;

namespace MoreSaves.Nodes
{
    public class NodeCreateCustomSave : IBTnode
    {
        protected override BTresult MyRun(TickData p_data)
        {
            if (ModEntry.saveName == string.Empty)
            {
                Game1.instance.contentManager.audio.menu.MenuFail.Play();
                return BTresult.Failure;
            }

            string date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string directoryName = $"{ModEntry.saveName}-{date}";

            CopyUtil.CopyOutSaves("manual", directoryName);

            Game1.instance.contentManager.audio.menu.Select.Play();
            return BTresult.Success;
        }
    }
}
