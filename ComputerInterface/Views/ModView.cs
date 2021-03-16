using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views
{
    internal class ModView : ComputerView
    {
        private readonly CIConfig _config;
        private BepInEx.PluginInfo _plugin;

        private readonly UISelectionHandler _selectionHandler;

        public ModView(CIConfig config)
        {
            _config = config;
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += OnOptionSelected;
            _selectionHandler.MaxIdx = 1;
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
            str.Append("/// ").Append(_plugin.Instance.enabled ? "<color=#00ff00>Enabled</color>" : "<color=#ff0000>Disabled</color>").AppendLine();
        }

        private void RedrawSelection(StringBuilder str)
        {
            str.AppendLine();
            str.Append(GetSelectionString(0, "[")).Append("Enable").Append(GetSelectionString(0, "]")).AppendLine();
            str.Append(GetSelectionString(1, "[")).Append("Disable").Append(GetSelectionString(1, "]")).AppendLine();
            str.AppendLine().AppendLine();
        }

        private string GetSelectionString(int idx, string chararcter)
        {
            return _selectionHandler.CurrentSelectionIndex == idx ? "<color=#ed6540>" + chararcter+ "</color>" : " ";
        }

        private void OnOptionSelected(int idx)
        {
            if (idx == 0)
            {
                // Enable was pressed
                _plugin.Instance.enabled = true;
                _config.RemoveDisabledMod(_plugin.Metadata.GUID);
                return;
            }

            if (idx == 1)
            {
                // Disable was pressed
                _plugin.Instance.enabled = false;
                _config.AddDisabledMod(_plugin.Metadata.GUID);
            }

            Redraw();
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