using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views.GameSettings;

using Photon.Pun;

using UnityEngine;

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

        private GameSettingsUpdate updater;

        private GameSettingsView()
        {
            _gameSettingsViews = new List<Tuple<string, Type>>
            {
                new Tuple<string, Type>("Room", typeof(JoinRoomView)),
                new Tuple<string, Type>("Color", typeof(ColorSettingView)),
                new Tuple<string, Type>("Name", typeof(NameSettingView)),
                new Tuple<string, Type>("Turn Mode", typeof(TurnSettingView)),
                new Tuple<string, Type>("Mic Mode", typeof(MicSettingsView)),
                new Tuple<string, Type>("Voice Mode", typeof(VoiceSettingsView)),
                new Tuple<string, Type>("Item Mode", typeof(ItemSettingsView)),
                new Tuple<string, Type>("Credits", typeof(CreditsView)),
                new Tuple<string, Type>("Group", typeof(GroupView))
            };

            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ItemSelected;
            _selectionHandler.MaxIdx = _gameSettingsViews.Count - 1;

            if(updater == null)
            {
                GameObject obj = new GameObject();
                obj.name = "PlayerCountCallbacks";
                updater = obj.AddComponent<GameSettingsUpdate>();
                updater.view = this;
            }
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();
            str.AppendLines(2);
            for (var i = 0; i < _gameSettingsViews.Count; i++)
            {
                var pair = _gameSettingsViews[i];
                str.Repeat(" ", 8).Append(GetSelector(i)).Append(pair.Item1).AppendLine();
            }

            str.AppendLines(2);
            if(PhotonNetwork.IsConnected)
                str.AppendClr($"Players online: {PhotonNetwork.CountOfPlayers}", "ffffff50").EndAlign().AppendLine();

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
            return idx == _selectionHandler.CurrentSelectionIndex ? "<color=#ed6540>> </color>" : "  ";
        }

        private void ItemSelected(int idx)
        {
            ShowView(_gameSettingsViews[_selectionHandler.CurrentSelectionIndex].Item2);
        }
    }

    class GameSettingsUpdate : MonoBehaviour
    {
        public GameSettingsView view;
        private int last;

        private void Update()
        {
            if(PhotonNetwork.CountOfPlayers != last)
            {
                view.Redraw();
                last = PhotonNetwork.CountOfPlayers;
            }
        }
    }
}
