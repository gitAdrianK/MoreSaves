using JumpKing;
using JumpKing.PauseMenu.BT.Actions;
using MoreSaves.Models;

namespace MoreSaves.Nodes
{
    public class NodeChangeFolderOptions : IOptions
    {
        string[] names = { "Auto", "Manual" };

        public NodeChangeFolderOptions()
            : base(2, 0, EdgeMode.Wrap, Game1.instance.contentManager.font.MenuFontSmall)
        {
        }

        protected override bool CanChange()
        {
            return true;
        }

        protected override string CurrentOptionName()
        {
            return "[WIP] " + names[CurrentOption];
        }

        protected override void OnOptionChange(int option)
        {
            ModelLoadOptions.UpdateFolderOption(option);
        }
    }
}
