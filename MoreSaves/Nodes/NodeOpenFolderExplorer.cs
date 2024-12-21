namespace MoreSaves.Nodes
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using BehaviorTree;
    using JumpKing;

    /// <summary>
    /// Opens the explorer at the mods location.
    /// </summary>
    public class NodeOpenFolderExplorer : IBTnode
    {
        protected override BTresult MyRun(TickData p_data)
        {
            _ = Process.Start("explorer.exe", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Game1.instance.contentManager.audio.menu.Select.Play();
            return BTresult.Success;
        }
    }
}
