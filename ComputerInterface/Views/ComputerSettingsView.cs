using System;
using System.Text;
using UnityEngine;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class ComputerSettingsEntry : IComputerModEntry
    {
        public string EntryName => "Computer Display Settings";
        public Type EntryViewType => typeof(ComputerSettingsView);
    }

    public class ComputerSettingsView : ComputerView
    {
        private readonly CustomComputer _computer;
        private readonly UISelectionHandler _rowSelectionHandler;
        private readonly UISelectionHandler _columnSelectionHandler;
        private Color _color;

        public ComputerSettingsView(CustomComputer computer)
        {
            _computer = computer;

            _rowSelectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _rowSelectionHandler.MaxIdx = 2;

            _columnSelectionHandler = new UISelectionHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _columnSelectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _color = _computer.GetBG();
            Redraw();
        }

        public void Redraw()
        {
            Color savedColor = _computer.GetBG();

            var str = new StringBuilder();
            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Computer Display Settings").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).AppendLines(2);

            str.AppendLine(" Background Color:");

			void DrawRow(char name, float color, float savedColor, int col)
            {
                str.AppendClr($"  {name}: ", "ffffff50");
                DrawValue(str, color, col);
                str.AppendClr($"<size=40>  Current: {FormatColor(savedColor)}</size>", "ffffff50").AppendLine();
            }

            DrawRow('R', _color.r, savedColor.r, 0);
            DrawRow('G', _color.g, savedColor.g, 1);
            DrawRow('B', _color.b, savedColor.b, 2);

            str.AppendLines(2)
                .AppendClr(" * Press Enter to update settings.", "ffffff50").AppendLine();

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Enter:
                    _computer.SetBG(_color);
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
                default:
                    if (key.TryParseNumber(out var num))
                    {
                        var line = _rowSelectionHandler.CurrentSelectionIndex;
                        var column = _columnSelectionHandler.MaxIdx - _columnSelectionHandler.CurrentSelectionIndex; // first column is most significant digit

                        switch (line)
                        {
                            case 0:
                                _color.r = SetValOnColor(_color.r, column, num);
                                break;
                            case 1:
                                _color.g = SetValOnColor(_color.g, column, num);
                                break;
                            case 2:
                                _color.b = SetValOnColor(_color.b, column, num);
                                break;
                        }

                        _columnSelectionHandler.MoveSelectionDown();
                        Redraw();
                        break;
                    }
                    if (_rowSelectionHandler.HandleKeypress(key) || _columnSelectionHandler.HandleKeypress(key)) Redraw();
                    break;
            }
        }

        string FormatColor(float color) => Mathf.RoundToInt(color * 255).ToString().PadLeft(3, '0');

        private void DrawValue(StringBuilder str, float val, int lineNum)
        {
            var valStr = FormatColor(val);
            for (int i = 0; i < 3; i++)
            {
                if (_columnSelectionHandler.CurrentSelectionIndex == i && lineNum == _rowSelectionHandler.CurrentSelectionIndex)
                {
                    str.BeginColor(PrimaryColor).Append(valStr[i]).EndColor();
                    continue;
                }

                str.Append(valStr[i]);
            }
        }

        private static float SetValOnColor(float input, int column, int val) => Mathf.Clamp01(SetVal(Mathf.RoundToInt(input * 255), column, val) / 255f);
        private static int SetVal(int input, int column, int val)
        {
            Debug.Log($"input: {input}, column: {column}, val: {val}");
            int powerOfTen = (int)Math.Pow(10, column);
            Debug.Log($"powerOfTen: {powerOfTen}");
            int digitToReplace = input / powerOfTen % 10;
            Debug.Log($"digitToReplace: {digitToReplace}");
            var newValue = input + powerOfTen * (val - digitToReplace);
            Debug.Log($"newValue: {newValue}");
            return newValue;
        }
    }
}
