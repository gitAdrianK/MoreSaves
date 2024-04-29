using BehaviorTree;
using JumpKing;
using MoreSaves.Util;

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
                Game1.instance.contentManager.audio.menu.Select.Play();

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
