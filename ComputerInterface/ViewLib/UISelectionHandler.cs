using System;

namespace ComputerInterface.ViewLib
{
    public class UISelectionHandler
    {
        public event Action<int> OnSelected; 

        public int CurrentSelectionIndex;

        /// <summary>
        /// Min 0 indexed item
        /// This can stay on 0 
        /// </summary>
        public int Min = 0;

        /// <summary>
        /// Max 0 indexed item
        /// e.g. If you have two items this should be 1
        /// </summary>
        public int Max { get; set; }

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

        public UISelectionHandler(EKeyboardKey upKey, EKeyboardKey downKey) : this(upKey, downKey, EKeyboardKey.Enter, false)
        {
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

        public void MoveSelectionUp()
        {
            CurrentSelectionIndex--;
            ClampSelection();
        }

        public void MoveSelectionDown()
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