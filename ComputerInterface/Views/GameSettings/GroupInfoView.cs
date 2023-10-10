using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    internal class GroupInfoView : ComputerView
    {
        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            SetText(Redraw);
        }

        public void Redraw(StringBuilder str)
        {
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Group Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);
            str.AppendLine("1. Create/Join a Private room").AppendLine();
            str.AppendLine("2. Select a map in the Group tab").AppendLine();
            str.AppendLine("3. Gather everyone near the computer").AppendLine();
            str.AppendLine("4. Make sure everyone is on the same gamemode").AppendLine();
            str.AppendLine("5. Press the Enter key").AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GroupView>();
                    break;
            }
        }
    }
}