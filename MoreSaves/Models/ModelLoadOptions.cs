using HarmonyLib;
using JumpKing;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreSaves.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoreSaves.Models
{
    public class ModelLoadOptions
    {
        private static List<TextButton> autoButtons;
        private static List<TextButton> manualButtons;
        private static List<TextButton> activeList;

        private static MenuSelector menuSelector;
        private static NodeChangeFolderOptions nodeChangeFolderOptions;
        private static NodeChangePageOptions nodeChangePageOptions;

        public static MenuSelector CreateLoadOptions(object factory, GuiFormat format)
        {
            char sep = Path.DirectorySeparatorChar;
            string dllDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{sep}";
            string[] autoDirectories = Directory.GetDirectories($"{dllDirectory}auto{sep}");
            string[] manualDirectories = Directory.GetDirectories($"{dllDirectory}manual{sep}");
            if (autoDirectories.Length == 0 && manualDirectories.Length == 0)
            {
                MenuSelector emptySelector = new MenuSelector(format);
                emptySelector.AddChild(new TextInfo("No saves to load.", Color.Gray));
                emptySelector.Initialize(true);
                return emptySelector;
            }

            var _this = Traverse.Create(factory);
            var gui_left = _this.Field("CONTROLS_GUI_FORMAT_LEFT").GetValue<GuiFormat>();

            menuSelector = new MenuSelector(gui_left);

            menuSelector.AddChild(new TextInfo("Loading will close JK!", Color.Red));

            nodeChangeFolderOptions = new NodeChangeFolderOptions();
            menuSelector.AddChild(nodeChangeFolderOptions);

            SpriteFont menuFontSmall = Game1.instance.contentManager.font.MenuFontSmall;

            autoButtons = new List<TextButton>();
            foreach (string directory in autoDirectories)
            {
                string dir = directory.Split(Path.DirectorySeparatorChar).Last();
                autoButtons.Add(new TextButton(dir, new NodeLoadSave("auto", dir), menuFontSmall));
            }

            manualButtons = new List<TextButton>();
            foreach (string directory in manualDirectories)
            {
                string dir = directory.Split(Path.DirectorySeparatorChar).Last();
                manualButtons.Add(new TextButton(dir, new NodeLoadSave("manual", dir), menuFontSmall));
            }

            UpdateFolderOption(0);

            menuSelector.Initialize();
            return menuSelector;
        }

        // TODO: Update the menu somehow.

        public static void UpdateFolderOption(int folder)
        {
            if (folder == 0)
            {
                activeList = autoButtons;
            }
            else
            {
                activeList = manualButtons;
            }

            RemovePageSelector();

            nodeChangePageOptions = new NodeChangePageOptions(activeList.Count / 10 + 1);
            menuSelector.AddChild(nodeChangePageOptions);
            UpdatePageOption(0);
            menuSelector.Initialize(false);
        }

        public static void UpdatePageOption(int page)
        {
            if (nodeChangePageOptions == null)
            {
                return;
            }

            RemoveButtons();
            IEnumerable<TextButton> visible = activeList.Skip(9 * page).Take(9);
            foreach (TextButton button in visible)
            {
                menuSelector.AddChild(button);
            }
            // The back button prolly moves after the first and gets removed instead of the last folder
            // Move the back button to the end
            menuSelector.Initialize(false);
        }

        // 1.           |TextInfo
        // 2.           |FolderSelector
        // 3.           |Page Selector
        //              |
        // 4..Count - 2 |Buttons
        //              |
        // Count - 1.   |Return

        private static void RemovePageSelector()
        {
            /*
            if (menuSelector.Children.Count() < 3)
            {
                return;
            }
            RemoveButtons();
            // Buttons are removed, should be second last item.
            Traverse childrenRef = Traverse.Create(menuSelector).Field("m_children");
            List<IBTnode> children = childrenRef.GetValue<IBTnode[]>().ToList();
            children[children.Count - 2].OnDispose();
            children.RemoveAt(children.Count - 2);
            childrenRef.SetValue(children.ToArray());
            */
        }

        private static void RemoveButtons()
        {
            /*
            if (menuSelector.Children.Count() < 4)
            {
                return;
            }
            // First three elements are TextInfo, Folder and Page selector.
            // Last element is the back button.
            Traverse childrenRef = Traverse.Create(menuSelector).Field("m_children");
            List<IBTnode> children = childrenRef.GetValue<IBTnode[]>().ToList();
            while (children.Count > 4)
            {
                children[4].OnDispose();
                children.RemoveAt(4);
            }
            childrenRef.SetValue(children.ToArray());
            */
        }
    }
}
