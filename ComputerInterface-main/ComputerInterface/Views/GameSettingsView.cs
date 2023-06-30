﻿using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views.GameSettings;

namespace ComputerInterface.Views
{
    public class GameSettingsEntry : IComputerModEntry
    {
        public string EntryName => "Game Settings";
        public Type EntryViewType => typeof(GameSettingsView);
    }

    public class GameSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;
        private readonly List<Tuple<string, Type>> _gameSettingsViews;

        private GameSettingsView()
        {
            _gameSettingsViews = new List<Tuple<string, Type>>
            {
                new Tuple<string, Type>("Room   ", typeof(JoinRoomView)),
                new Tuple<string, Type>("Name   ", typeof(NameSettingView)),
                new Tuple<string, Type>("Color  ", typeof(ColorSettingView)),
                new Tuple<string, Type>("Turn   ", typeof(TurnSettingView)),
                new Tuple<string, Type>("Mic    ", typeof(MicSettingsView)),
                new Tuple<string, Type>("Queue  ", typeof(CustomQueuesView)),
                new Tuple<string, Type>("Group  ", typeof(GroupView)),
                new Tuple<string, Type>("Voice  ", typeof(VoiceSettingsView)),
                new Tuple<string, Type>("Items  ", typeof(ItemSettingsView)),
                new Tuple<string, Type>("Credits", typeof(CreditsView)),
                new Tuple<string, Type>("Support", typeof(SupportView)),
            };

            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ItemSelected;
            _selectionHandler.MaxIdx = _gameSettingsViews.Count - 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();
            str.AppendLine();
            for (var i = 0; i < _gameSettingsViews.Count; i++)
            {
                var pair = _gameSettingsViews[i];
                str.Repeat(" ", _gameSettingsViews.Count - 1).Append(GetSelector(i)).Append(pair.Item1).AppendLine();
            }

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }

        private string GetSelector(int idx)
        {
            return idx == _selectionHandler.CurrentSelectionIndex ? $"<color=#{PrimaryColor}>> </color>" : "  ";
        }

        private void ItemSelected(int idx)
        {
            ShowView(_gameSettingsViews[_selectionHandler.CurrentSelectionIndex].Item2);
        }
    }
}