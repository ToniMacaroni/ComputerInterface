using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class ColorSettingView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;
        private readonly UISelectionHandler _columnSelectionHandler;
        private Color _color;

        private string _rString = "255";
        private string _gString = "255";
        private string _bString = "255";

        public ColorSettingView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.MaxIdx = 2;

            _columnSelectionHandler = new UISelectionHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _columnSelectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _color = BaseGameInterface.GetColor();
            SetColor(BaseGameInterface.GetColor());

            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();
            str.BeginColor(_color).Repeat("=", SCREEN_WIDTH).EndColor().AppendLine();
            str.BeginCenter().Append("Change Color").AppendLine();
            str.Append("Values are from 0 - 255").EndAlign().AppendLine();
            str.BeginColor(_color).Repeat("=", SCREEN_WIDTH).EndColor().AppendLines(2);

            str.Append("R: ");
            DrawValue(str, _rString, 0);
            str.AppendLine();

            str.Append("G: ");
            DrawValue(str, _gString, 1);
            str.AppendLine();

            str.Append("B: ");
            DrawValue(str, _bString, 2);
            str.AppendLine();

            Text = str.ToString();
        }

        private void DrawValue(StringBuilder str, string val, int lineNum)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_columnSelectionHandler.CurrentSelectionIndex == i && lineNum == _selectionHandler.CurrentSelectionIndex)
                {
                    str.BeginColor(PrimaryColor).Append(val[i]).EndColor();
                    continue;
                }

                str.Append(val[i]);
            }
        }

        private void SetColor(Color color)
        {
            _color = color;
            _rString = Mathf.RoundToInt(color.r * 255).ToString().PadLeft(3, '0');
            _gString = Mathf.RoundToInt(color.g * 255).ToString().PadLeft(3, '0');
            _bString = Mathf.RoundToInt(color.b * 255).ToString().PadLeft(3, '0');
        }

        private void UpdateColor()
        {
            var r = Mathf.Clamp(int.Parse(_rString), 0, 255);
            var g = Mathf.Clamp(int.Parse(_gString), 0, 255);
            var b = Mathf.Clamp(int.Parse(_bString), 0, 255);

            _rString = r.ToString().PadLeft(3, '0');
            _gString = g.ToString().PadLeft(3, '0');
            _bString = b.ToString().PadLeft(3, '0');

            _color = new Color(r / 255f, g / 255f, b / 255f);
            BaseGameInterface.SetColor(_color);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key) || _columnSelectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            if (key.IsNumberKey())
            {
                var line = _selectionHandler.CurrentSelectionIndex;
                var column = _columnSelectionHandler.CurrentSelectionIndex;
                var numChar = key.ToString().Substring(3)[0];

                switch (line)
                {
                    case 0:
                        SetValOnString(ref _rString, column, numChar);
                        break;
                    case 1:
                        SetValOnString(ref _gString, column, numChar);
                        break;
                    case 2:
                        SetValOnString(ref _bString, column, numChar);
                        break;
                }

                _columnSelectionHandler.MoveSelectionDown();
                UpdateColor();
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

        private void SetValOnString(ref string str, int column, char chr)
        {
            char[] ch = str.ToCharArray();
            ch[column] = chr;
            str = new string(ch);
        }
    }
}
