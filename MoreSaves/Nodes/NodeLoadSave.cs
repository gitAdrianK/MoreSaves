using BehaviorTree;
using JumpKing;
using MoreSaves.Util;
using System.Diagnostics;

namespace MoreSaves.Nodes
{
    /// <summary>
    /// Node to copy in a save into the Jump King folder.
    /// Force closes the game.
    /// </summary>
    public class NodeLoadSave : IBTnode
    {
        /// <summary>
        /// Folder that make up the path to the file.
        /// </summary>
        private string[] folder;

        public NodeLoadSave(params string[] folder)
        {
            this.folder = folder;
        }

        protected override BTresult MyRun(TickData p_data)
        {
            try
            {
                // TODO: Reload the game or whatever it uses to determine map loaded somehow.
                // Optimally start gameplay after the reload.
                CopyUtil.CopyInSaves(folder);
                // XXX: Not even closing, there were some problems, just killing the process.
                //Game1.instance.Exit();
                Game1.instance.contentManager.audio.menu.Select.Play();
                Process.GetCurrentProcess().Kill();
                return BTresult.Success;
            }
            catch
            {
                Game1.instance.contentManager.audio.menu.MenuFail.Play();
                return BTresult.Failure;
            }
        }
    }
}
