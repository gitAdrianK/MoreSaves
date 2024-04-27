using BehaviorTree;
using JumpKing;
using JumpKing.PauseMenu.BT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoreSaves.Models
{
    public class ExplorerTextButton : TextButton
    {
        private SpriteFont font;
        private string text;
        private Color color;

        public ExplorerTextButton(string text, IBTnode child, Color color) : base(text, child, color)
        {
            font = Game1.instance.contentManager.font.MenuFont;
            this.text = text;
            this.color = color;
        }
        public override void Draw(int x, int y, bool selected)
        {
            Game1.spriteBatch.DrawString(font, text, new Vector2(x, y), color);
            Point point = font.MeasureString(text).ToPoint();
            Game1.spriteBatch.Draw(Game1.instance.contentManager.gui.Explore.texture, new Vector2(x + point.X + 2, (float)y + 2f), color);
        }

        public override Point GetSize()
        {
            Vector2 value = font.MeasureString(text);
            Vector2 value2 = new Vector2(Game1.instance.contentManager.gui.Explore.texture.Width + 2, 0f);
            return Vector2.Add(value, value2).ToPoint();
        }
    }
}
