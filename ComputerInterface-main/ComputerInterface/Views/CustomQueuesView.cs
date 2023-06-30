using System;
using System.Text;
using ComputerInterface.ViewLib;
using ComputerInterface.Interfaces;
using UnityEngine;

namespace ComputerInterface.Views
{
    internal class CustomQueuesEntry : IComputerModEntry
    {
        public string EntryName => "Custom Queues";
        public Type EntryViewType => typeof(CustomQueuesView);
    }
    internal class CustomQueuesView : ComputerView
    {
        private readonly UIElementPageHandler<IQueueInfo> _pageHandler;

        public CustomQueuesView()
        {
            _pageHandler = new UIElementPageHandler<IQueueInfo>(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 1;
            _pageHandler.Footer = "<color=#ffffff50><align=\"center\"><       ></align></color>";
            _pageHandler.SetElements(QueueManager.Queues.ToArray());
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateState();
            Redraw();
        }

        public void UpdateState()
        {
            _pageHandler.CurrentPage = QueueManager.Queues.IndexOf(QueueManager.GetQueue());
        }

        public void SetMode()
        {
            QueueManager.SetQueue(QueueManager.Queues[_pageHandler.CurrentPage]);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLines(1);

            DrawOptions(str);

            SetText(str);
        }

        public void DrawOptions(StringBuilder str)
        {
            str.AppendLine(" Current Queue: " + QueueManager.GetQueue().DisplayName);
            str.AppendLines(4);
            str.Append(_pageHandler.GetElementsForPage(_pageHandler.CurrentPage)[0].DisplayName).AppendLine(": ").Append(_pageHandler.GetElementsForPage(_pageHandler.CurrentPage)[0].Description).AppendLine();
            _pageHandler.AppendFooter(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if(key == EKeyboardKey.Enter)
            {
                SetMode();
                Redraw();
            }

            if (key == EKeyboardKey.Left && _pageHandler.CurrentPage == 0)
            {
                _pageHandler.CurrentPage = _pageHandler.MaxPage + 1;
            }

            if (key == EKeyboardKey.Right && _pageHandler.CurrentPage == _pageHandler.MaxPage)
            {
                _pageHandler.CurrentPage = -1;
            }

            if (_pageHandler.HandleKeyPress(key))
            {
                Redraw();
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}
