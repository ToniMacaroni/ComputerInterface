using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.RoomBrowser
{
    internal class InfoView : ComputerView
    {
        private readonly UITextPageHandler _pageHandler;

        public InfoView()
        {
            _pageHandler = new UITextPageHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 7;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            if (args == null || args.Length == 0 || args[0] == null)
            {
                SetText(str =>
                {
                    str.BeginCenter().Append("Error").EndAlign();
                });
                return;
            }

            if (args is string[] lines)
            {
                _pageHandler.SetLines(lines);
            }
            else if (args[0] is string text)
            {
                _pageHandler.SetText(text);
            }

            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLine();
            DrawContent(str);

            SetText(str);
        }

        public void DrawContent(StringBuilder str)
        {
            var lines = _pageHandler.GetLinesForCurrentPage();

            foreach (var line in lines)
            {
                str.Append(line).AppendLine();
            }

            str.AppendLine();
            _pageHandler.AppendFooter(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_pageHandler.HandleKeyPress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnView();
                    break;
            }
        }
    }
}