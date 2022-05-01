using System.Text;
using ComputerInterface.ViewLib;
using ComputerInterface.Interfaces;
using ComputerInterface.Queues;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    internal class QueueSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private readonly IQueueInfo[] defaultQueues = { new DefaultQueue(), new CompetitiveQueue(), new MinigamesQueue() };

        public QueueSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator("", $"<color=#{PrimaryColor}> <</color>", "", "");
            _selectionHandler.MaxIdx = defaultQueues.Length - 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateState();
            Redraw();
        }

        public void UpdateState()
        {
            _selectionHandler.CurrentSelectionIndex = 0;
        }

        public void SetMode()
        {
            QueueManager.SetQueue(defaultQueues[_selectionHandler.CurrentSelectionIndex]);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLines(5);

            DrawOptions(str);

            SetText(str);
        }

        public void DrawOptions(StringBuilder str)
        {
            int i = 0;
            foreach(IQueueInfo queue in defaultQueues)
            {
                str.AppendLine(_selectionHandler.GetIndicatedText(i, queue.DisplayName));
                i++;
            }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                SetMode();
                Redraw();
                return;
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