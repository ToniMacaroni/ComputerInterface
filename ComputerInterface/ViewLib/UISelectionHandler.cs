using System;

namespace ComputerInterface.Views
{
    public class UISelectionHandler
    {
        public event Action<int> OnSelected; 

        public int CurrentSelectionIndex;
        public int Min = 0;
        public int Max;

        private readonly EKeyboardKey _upKey;
        private readonly EKeyboardKey _downKey;
        private readonly EKeyboardKey _selectKey;
        private readonly bool _canSelect;

        public UISelectionHandler(EKeyboardKey upKey, EKeyboardKey downKey, EKeyboardKey selectKey, bool canSelect)
        {
            _upKey = upKey;
            _downKey = downKey;
            _selectKey = selectKey;
            _canSelect = canSelect;
        }

        public bool HandleKeypress(EKeyboardKey key)
        {
            if (key == _upKey)
            {
                MoveSelectionUp();
                return true;
            }

            if (key == _downKey)
            {
                MoveSelectionDown();
                return true;
            }

            if (_canSelect && key == _selectKey)
            {
                OnSelected?.Invoke(CurrentSelectionIndex);
                return true;
            }

            return false;
        }

        private void MoveSelectionUp()
        {
            CurrentSelectionIndex--;
            ClampSelection();
        }

        private void MoveSelectionDown()
        {
            CurrentSelectionIndex++;
            ClampSelection();
        }

        private void ClampSelection()
        {
            if (CurrentSelectionIndex > Max)
            {
                CurrentSelectionIndex = Max;
                return;
            }

            if (CurrentSelectionIndex < Min)
            {
                CurrentSelectionIndex = Min;
            }
        }
    }
}