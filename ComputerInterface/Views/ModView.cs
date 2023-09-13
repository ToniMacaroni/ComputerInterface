using System.Text;
using ComputerInterface.ViewLib;

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
            var pluginInfo = _plugin.PluginInfo;
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append($"{pluginInfo.Metadata.Name} ({(_plugin.PluginInfo.Instance.enabled ? "<color=#00ff00>Enabled</color>" : "<color=#ff0000>Disabled</color>")})").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
            str.Append($"<size=40>{pluginInfo.Metadata.GUID}, v{pluginInfo.Metadata.Version}</size>").AppendLines(2);
        }

        private void RedrawSelection(StringBuilder str)
        {
            str.AppendLine();
            str.Append(GetSelectionString(0, "[")).Append("<color=#7Cff7C>Enabled</color>").Append(GetSelectionString(0, "]")).AppendLine();
            str.Append(GetSelectionString(1, "[")).Append("<color=#ff7C7C>Disabled</color>").Append(GetSelectionString(1, "]")).AppendLine();
            str.AppendLine().AppendLine();
        }

        private void DrawNotice(StringBuilder str)
        {
            if (!_plugin.Supported)
            {
                str.BeginCenter().AppendClr("This mod doesn't implement the Enable/Disable feature.", "50ff5050").EndAlign();
                return;
            }

            str.Append("1. Select an option, either Enable or Disable").AppendLines(2);
            str.Append("2. Press Enter, the mod will be toggled accordingly");
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