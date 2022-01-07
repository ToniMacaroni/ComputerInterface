using System.Text;
using ComputerInterface.ViewLib;
using HarmonyLib;

namespace ComputerInterface.Views
{
    internal class ModView : ComputerView
    {
        private readonly CIConfig _config;
        private ModListView.ModListItem _plugin;

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

            _plugin = (ModListView.ModListItem) args[0];
            Redraw();
        }

        private void Redraw()
        {
            var builder = new StringBuilder();

            RedrawHeader(builder);
            RedrawSelection(builder);
            DrawNotice(builder);

            Text = builder.ToString();
        }

        private void RedrawHeader(StringBuilder str)
        {
            str.Append("/// ").Append(_plugin.PluginInfo.Metadata.Name).Append(" ").Append(_plugin.PluginInfo.Metadata.Version).Append(" ///").AppendLine();
            str.Append("/// ").Append(_plugin.PluginInfo.Instance.enabled ? "<color=#00ff00>Enabled</color>" : "<color=#ff0000>Disabled</color>").AppendLine();
        }

        private void RedrawSelection(StringBuilder str)
        {
            str.AppendLine();
            str.Append(GetSelectionString(0, "[")).Append("Enable").Append(GetSelectionString(0, "]")).AppendLine();
            str.Append(GetSelectionString(1, "[")).Append("Disable").Append(GetSelectionString(1, "]")).AppendLine();
            str.AppendLine().AppendLine();
        }

        private void DrawNotice(StringBuilder str)
        {
            if (!_plugin.Supported)
            {
                str.BeginCenter().AppendClr("Mod doesn't implement this feature", "ffffff50").EndAlign();
            }
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
                _plugin.PluginInfo.Instance.enabled = true;
                _config.RemoveDisabledMod(_plugin.PluginInfo.Metadata.GUID);
                return;
            }

            if (idx == 1)
            {
                // Disable was pressed
                _plugin.PluginInfo.Instance.enabled = false;
                _config.AddDisabledMod(_plugin.PluginInfo.Metadata.GUID);
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