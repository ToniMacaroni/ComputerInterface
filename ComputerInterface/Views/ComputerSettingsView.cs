using System;
using System.Text;
using UnityEngine;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Monitors;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ComputerInterface.Views
{
    public class ComputerSettingsEntry : IComputerModEntry
    {
        public string EntryName => "Computer Settings";
        public Type EntryViewType => typeof(ComputerSettingsView);
    }

    public class ComputerSettingsView : ComputerView
    {
        private readonly CustomComputer _computer;
        private readonly UISelectionHandler _rowSelectionHandler;
        private readonly UISelectionHandler _columnSelectionHandler;
        private Color _color;

        private Dictionary<int, MonitorType> _monitorIndexDict;
        private MonitorType _currentMonitor;
        private bool _isUpdatingMonitors;

        public ComputerSettingsView(CustomComputer computer)
        {
            _computer = computer;

            _rowSelectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _rowSelectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _rowSelectionHandler.MaxIdx = 4;

            _columnSelectionHandler = new UISelectionHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _columnSelectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _color = _computer.GetBG();
            _currentMonitor = _computer.MonitorType;

            if (_monitorIndexDict == null)
            {
                _monitorIndexDict = new Dictionary<int, MonitorType>();
                var monitorTypes = (MonitorType[])Enum.GetValues(typeof(MonitorType));
                for (int i = 0; i < monitorTypes.Length; i++)
                {
                    var currentType = monitorTypes[i];
                    _monitorIndexDict.Add(i + 3, currentType);
                }
            }

            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();
            if (_isUpdatingMonitors)
            {
                str.AppendLine().BeginCenter().Append("Updating Monitor").EndAlign();
                Text = str.ToString();
                return;
            }

            Color savedColor = _computer.GetBG();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Computer Settings").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

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

            str.AppendLine().AppendLine(" Monitor Type:");
            var monitorEnumerator = _monitorIndexDict.GetEnumerator();
            while (monitorEnumerator.MoveNext())
            {
                var monitorPair = monitorEnumerator.Current;
                str.AppendLine(_rowSelectionHandler.GetIndicatedText(monitorPair.Key, monitorPair.Value.ToString()));
            }

            Text = str.ToString();
        }

        public async void ProcessSave()
        {
            if (_monitorIndexDict.TryGetValue(_rowSelectionHandler.CurrentSelectionIndex, out var _selectedMonitor) && _selectedMonitor != _currentMonitor && _rowSelectionHandler.CurrentSelectionIndex > 2)
            {
                _isUpdatingMonitors = true;
                Redraw();

                // Change the current monitor with a new one based on which monitor we're selecting
                await _computer.SetMonitorType(_selectedMonitor);
                await Task.Delay(300);

                _currentMonitor = _selectedMonitor;
                _isUpdatingMonitors = false;
            }

            _computer.SetBG(_color);
            Redraw();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_isUpdatingMonitors) return;
            switch (key)
            {
                case EKeyboardKey.Enter:
                    ProcessSave();
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
