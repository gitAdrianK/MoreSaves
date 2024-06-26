﻿using BehaviorTree;
using JumpKing;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MoreSaves.Nodes
{
    /// <summary>
    /// Opens the explorer at the mods location.
    /// </summary>
    public class NodeOpenFolderExplorer : IBTnode
    {
        protected override BTresult MyRun(TickData p_data)
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Game1.instance.contentManager.audio.menu.Select.Play();
            return BTresult.Success;
        }
    }
}
