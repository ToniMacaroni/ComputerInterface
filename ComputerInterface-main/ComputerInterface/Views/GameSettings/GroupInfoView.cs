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
            str.BeginCenter().BeginColor("ffffff50").Append("How to:").EndColor().EndAlign().AppendLine();
            str.AppendLine("1) Create private room").AppendLine();
            str.AppendLine("2) Let your friends join the room").AppendLine();
            str.AppendLine("3) Meet at the computer").AppendLine();
            str.AppendLine("4) Select map and press enter").AppendLine();
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