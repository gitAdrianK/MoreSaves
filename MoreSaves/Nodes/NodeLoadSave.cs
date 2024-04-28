using BehaviorTree;
using JumpKing;
using MoreSaves.Util;
using System.Diagnostics;

namespace MoreSaves.Nodes
{
    public class NodeLoadSave : IBTnode
    {
        private string[] directories;

        public NodeLoadSave(params string[] directories)
        {
            this.directories = directories;
        }

        protected override BTresult MyRun(TickData p_data)
        {
            try
            {
                // TODO: Reload the game or whatever it uses to determine map loaded somehow.
                // Optimally start gameplay after the reload.
                CopyUtil.CopyInSaves(directories);
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
