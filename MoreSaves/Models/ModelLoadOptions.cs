using HarmonyLib;
using JumpKing;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using LanguageJK;
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
        public enum PageOption
        {
            Auto,
            Manual,
        }

        private const string AUTO = ModStrings.AUTO;
        private const string MANUAL = ModStrings.MANUAL;

        /// <summary>
        /// Maximum amount of buttons per page.
        /// </summary>
        private const int AMOUNT = 9;

        /// <summary>
        /// The buttons that the auto page can hold.
        /// </summary>
        private static List<TextButton> autoButtons;

        /// <summary>
        /// The buttons that the manual page can hold.
        /// </summary>
        private static List<TextButton> manualButtons;

        /// <summary>
        /// Reads the auto and manual directories and creates a button for each folder found inside.
        /// </summary>
        public static void SetupButtons()
        {
            char sep = Path.DirectorySeparatorChar;
            string dllDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{sep}";
            string[] autoDirectories = Directory.GetDirectories($"{dllDirectory}{AUTO}{sep}");
            string[] manualDirectories = Directory.GetDirectories($"{dllDirectory}{MANUAL}{sep}");
            SpriteFont menuFontSmall = Game1.instance.contentManager.font.MenuFontSmall;

            autoButtons = new List<TextButton>();
            foreach (string directory in autoDirectories)
            {
                string dir = directory.Split(sep).Last();
                autoButtons.Add(new TextButton(CropName(dir), new NodeLoadSave(AUTO, dir), menuFontSmall));
            }
            manualButtons = new List<TextButton>();
            foreach (string directory in manualDirectories)
            {
                string dir = directory.Split(sep).Last();
                manualButtons.Add(new TextButton(CropName(dir), new NodeLoadSave(MANUAL, dir), menuFontSmall));
            }
        }

        /// <summary>
        /// Crops the name should it be longer than 30 characters as it would cause an overflow.
        /// The name will be cropped at 27 characters and "..." will be inserted at the front to indicate the name having been cropped.
        /// </summary>
        /// <param name="name">The name to be cropped</param>
        /// <returns>The cropped name</returns>
        private static string CropName(string name)
        {
            if (name.Length > 30)
            {
                name = $"...{name.Substring(name.Length - 27)}";
            }
            return name;
        }

        public static MenuSelectorClosePopup CreateLoadOptions(object factory, GuiFormat format, int page, PageOption pageOption)
        {
            List<TextButton> buttons;
            switch (pageOption)
            {
                case PageOption.Auto:
                    buttons = autoButtons;
                    break;
                case PageOption.Manual:
                    buttons = manualButtons;
                    break;
                default:
                    buttons = new List<TextButton>();
                    break;
            }

            if (buttons.Count() == 0)
            {
                MenuSelectorClosePopup emptySelector = new MenuSelectorClosePopup(format);
                emptySelector.AddChild(new TextInfo("No saves to load.", Color.Gray));
                emptySelector.Initialize();
                return emptySelector;
            }

            var _this = Traverse.Create(factory);
            var gui_left = _this.Field("CONTROLS_GUI_FORMAT_LEFT").GetValue<GuiFormat>();

            MenuSelectorClosePopup menuSelector = new MenuSelectorClosePopup(gui_left);

            menuSelector.AddChild(new TextInfo("Load Save!", Color.White));

            int num = 0;
            for (int i = page * AMOUNT; i < page * AMOUNT + AMOUNT; i++)
            {
                if (num == AMOUNT)
                {
                    break;
                }

                if (i < buttons.Count)
                {
                    menuSelector.AddChild(buttons[i]);
                }

                num++;
            }

            if (page > 0)
            {
                menuSelector.AddChild(new TextButton(language.PAGINATION_PREVIOUS, new MenuSelectorBack(menuSelector)));
            }
            if (page * AMOUNT + num < buttons.Count)
            {
                menuSelector.AddChild(new TextButton(language.PAGINATION_NEXT, CreateLoadOptions(factory, format, page + 1, pageOption)));
            }

            menuSelector.Initialize();
            return menuSelector;
        }
    }
}
