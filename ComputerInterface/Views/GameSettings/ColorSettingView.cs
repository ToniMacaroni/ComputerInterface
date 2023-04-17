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
        private Color _savedColor;

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
            _savedColor = _color;

            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();
            str.BeginColor(_color).Repeat("=", SCREEN_WIDTH).EndColor().AppendLine();
            str.BeginCenter().Append("Color Tab").AppendLine();
            str.AppendClr("Values are from 0 - 255", "ffffff50").EndAlign().AppendLine();
            str.BeginColor(_color).Repeat("=", SCREEN_WIDTH).EndColor().AppendLines(2);

            str.AppendClr(" R: ", "ffffff50");
            DrawValue(str, _rString, 0);
            str.AppendClr($"<size=40>  Current: {Mathf.RoundToInt(_savedColor.r * 255).ToString().PadLeft(3, '0')}</size>", "ffffff50").AppendLine();

            str.AppendClr(" G: ", "ffffff50");
            DrawValue(str, _gString, 1);
            str.AppendClr($"<size=40>  Current: {Mathf.RoundToInt(_savedColor.g * 255).ToString().PadLeft(3, '0')}</size>", "ffffff50").AppendLine();

            str.AppendClr(" B: ", "ffffff50");
            DrawValue(str, _bString, 2);
            str.AppendClr($"<size=40>  Current: {Mathf.RoundToInt(_savedColor.b * 255).ToString().PadLeft(3, '0')}</size>", "ffffff50").AppendLine();

            str.AppendLines(3)
                .AppendClr(" * Press Enter to update your color.", "ffffff50").AppendLine();

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

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Enter:
                    BaseGameInterface.SetColor(_color);
                    _savedColor = _color;
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ReturnView();
                    break;
                default:
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

                        var r = Mathf.Clamp(int.Parse(_rString), 0, 255);
                        var g = Mathf.Clamp(int.Parse(_gString), 0, 255);
                        var b = Mathf.Clamp(int.Parse(_bString), 0, 255);

                        _rString = r.ToString().PadLeft(3, '0');
                        _gString = g.ToString().PadLeft(3, '0');
                        _bString = b.ToString().PadLeft(3, '0');

                        _color = new Color(r / 255f, g / 255f, b / 255f);
                        _columnSelectionHandler.MoveSelectionDown();
                        Redraw();
                        break;
                    }
                    if (_selectionHandler.HandleKeypress(key) || _columnSelectionHandler.HandleKeypress(key)) Redraw();
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
