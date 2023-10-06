using System;
using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    internal class WarnView : ComputerView
    {
        internal static IWarning _currentWarn;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _currentWarn = (IWarning)args[0]; // No way I'm actually using these arguments
            Redraw();
        }

        public void Redraw()
        {
            StringBuilder str = new StringBuilder();
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Warning").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

            str.AppendLine(_currentWarn.getWarningMessage());

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Back)
            {
                ReturnToMainMenu();
            }
        }

        internal interface IWarning
        {
            String getWarningMessage();
        }

        public class GeneralWarning : IWarning
        {
            private String message;

            public GeneralWarning(String message)
            {
                this.message = message;
            }

            public String getWarningMessage() => message;
        }

        public class OutdatedWarning : IWarning
        {
            public String getWarningMessage() => "You aren't on the latest version of Gorilla Tag, please update your game to continue playing with others.";
        }

        public class NoInternetWarning : IWarning
        {
            public String getWarningMessage() => "You aren't connected to an internet connection, please connect to a valid connection to continue playing with others.";
        }

        public class TemporaryBanWarning : IWarning
        {
            private String reason;
            private int hoursRemaining;

            public TemporaryBanWarning(String reason, int hoursRemaining)
            {
                this.reason = reason;
                this.hoursRemaining = hoursRemaining;
            }

            public String getWarningMessage() => $"You have been temporarily banned. You will not be able to play with others until the ban expires.\nReason: {reason}\nHours remaining: {hoursRemaining}";
        }

        public class PermanentBanWarning : IWarning
        {
            private String reason;

            public PermanentBanWarning(String reason)
            {
                this.reason = reason;
            }

            public String getWarningMessage() => $"You have been permanently banned.\nReason: {reason}";
        }
    }
}
