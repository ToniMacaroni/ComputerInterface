using System.Runtime.InteropServices;
using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine.Playables;

namespace ComputerInterface.Views
{
    internal class ModView : ComputerView
    {
        private BepInEx.PluginInfo _plugin;

        private readonly UISelectionHandler _selectionHandler;

        public ModView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter, false);
            _selectionHandler.Max = 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            if (args == null || args.Length==0) return;

            _plugin = (BepInEx.PluginInfo) args[0];
            Redraw();
        }

        private void Redraw()
        {
            var builder = new StringBuilder();
            RedrawHeader(builder);
            RedrawSelection(builder);
            Text = builder.ToString();
        }

        private void RedrawHeader(StringBuilder str)
        {
            str.Append("/// ").Append(_plugin.Metadata.Name).Append(" ").Append(_plugin.Metadata.Version).Append(" ///").AppendLine();
        }

        private void RedrawSelection(StringBuilder str)
        {
            str.AppendLine();
            str.Append(GetSelectionString(0, "[")).Append("Enable").Append(GetSelectionString(0, "]")).AppendLine();
            str.Append(GetSelectionString(1, "[")).Append("Disable").Append(GetSelectionString(1, "]")).AppendLine();
            str.AppendLine().AppendLine();
            str.Append("<color=#ffffff60>Toggling of mods currently not implemented</color>").AppendLine();
        }

        private string GetSelectionString(int idx, string chararcter)
        {
            return _selectionHandler.CurrentSelectionIndex == idx ? "<color=#ed6540>" + chararcter+ "</color>" : " ";
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            if (key == EKeyboardKey.Back)
            {
                ReturnView();
            }
        }
    }
}