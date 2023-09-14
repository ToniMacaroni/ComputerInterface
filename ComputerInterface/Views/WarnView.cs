using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    internal class WarnView : ComputerView
    {
        internal static WarnInfo _currentWarn;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _currentWarn = (WarnInfo)args[0]; // No way I'm actually using these arguments
            Redraw();
        }

        public void Redraw()
        {
            StringBuilder str = new StringBuilder();
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Warning").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

            switch (_currentWarn._warnType)
            {
                case WarnType.General:
                    str.AppendLine("Gorilla Tag has thrown a warning:").AppendLine();
                    str.Append(_currentWarn._warnParams[0]);
                    break;
                case WarnType.Outdated:
                    str.AppendLine("You aren't on the latest version of Gorilla Tag, please update your game to continue playing with others.");
                    break;
                case WarnType.NoInternet:
                    str.AppendLine("You aren't connected to an internet connection, please connect to a valid connection to continue playing with others.");
                    break;
                case WarnType.TemporaryBan:
                    str.AppendLine($"You have been temporarily banned. You will not be able to play with others until the ban expires.");
                    str.Append("Reason: ").Append(_currentWarn._warnParams[0]).AppendLine();
                    str.Append("Hours remaining: ").Append(_currentWarn._warnParams[1]);
                    break;
                case WarnType.PermanentBan:
                    str.AppendLine($"You have been permanently banned.");
                    str.Append("Reason: ").Append(_currentWarn._warnParams[0]);
                    break;
            }

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key != EKeyboardKey.Back) return;
            ReturnToMainMenu();
        }
    }

    internal struct WarnInfo
    {
        public WarnType _warnType;
        public object[] _warnParams;
    }

    internal enum WarnType
    {
        General,
        Outdated,
        NoInternet,
        TemporaryBan,
        PermanentBan
    }
}
