using JumpKing;
using JumpKing.PauseMenu.BT.Actions;
using MoreSaves.Models;

namespace MoreSaves.Nodes
{
    public class NodeChangePageOptions : IOptions
    {
        public NodeChangePageOptions(int optionsCount)
            : base(optionsCount, 0, EdgeMode.Wrap, Game1.instance.contentManager.font.MenuFontSmall)
        {
        }

        protected override bool CanChange()
        {
            return OptionCount > 1;
        }

        protected override string CurrentOptionName()
        {
            return $"[WIP] Page {CurrentOption + 1}";
        }

        protected override void OnOptionChange(int option)
        {
            ModelLoadOptions.UpdatePageOption(option);
        }
    }
}
