using HarmonyLib;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using LanguageJK;
using Microsoft.Xna.Framework;
using MoreSaves.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MoreSaves.Models
{
    public class ModelLoadOptions
    {
        private static readonly int AMOUNT = 9;

        private static List<TextButton> buttons;

        public static void SetupButtons()
        {
            char sep = Path.DirectorySeparatorChar;
            string dllDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{sep}";
            string[] autoDirectories = Directory.GetDirectories($"{dllDirectory}auto{sep}");
            string[] manualDirectories = Directory.GetDirectories($"{dllDirectory}manual{sep}");

            buttons = new List<TextButton>();
            foreach (string directory in autoDirectories)
            {
                string dir = directory.Split(sep).Last();
                buttons.Add(new TextButton(dir, new NodeLoadSave("auto", dir)));
            }
            foreach (string directory in manualDirectories)
            {
                string dir = directory.Split(sep).Last();
                buttons.Add(new TextButton(dir, new NodeLoadSave("manual", dir)));
            }
        }

        public static MenuSelectorClosePopup CreateLoadOptions(object factory, GuiFormat format, int page)
        {
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
                menuSelector.AddChild(new TextButton(language.PAGINATION_NEXT, CreateLoadOptions(factory, format, page + 1)));
            }

            menuSelector.Initialize();
            return menuSelector;
        }
    }
}
