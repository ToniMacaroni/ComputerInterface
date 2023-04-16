using BepInEx;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Text;

namespace ComputerInterface.Views
{
    internal class DetailsEntry : IComputerModEntry
    {
        public string EntryName => "Details";
        public Type EntryViewType => typeof(DetailsView);
    }

    internal class DetailsView : ComputerView
    {
        private string _name;
        private string _roomCode;
        private int _playerCount;
        private int _playerBans;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateStats();
            Redraw();
        }

        private void UpdateStats()
        {
            _name = BaseGameInterface.GetName();
            _roomCode = BaseGameInterface.GetRoomCode();
            _playerCount = PhotonNetwork.CountOfPlayersInRooms;
            _playerBans = GorillaComputer.instance.GetField<int>("usersBanned");
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLine();

            str.AppendClr("Name: ", "ffffff50")
                .AppendLine()
                .Repeat(" ", 4)
                .Append(_name)
                .AppendLines(2);

            str.AppendClr("Current room:", "ffffff50")
                .AppendLine()
                .Repeat(" ", 4)
                .Append(_roomCode.IsNullOrWhiteSpace() ? "-None-" : _roomCode)
                .AppendLines(2);

            str.AppendClr("Players online:", "ffffff50")
                .AppendLine()
                .Repeat(" ", 4)
                .Append(_playerCount)
                .AppendLines(2);

            str.AppendClr("User bans yesterday:", "ffffff50")
                .AppendLine()
                .Repeat(" ", 4)
                .Append(_playerBans)
                .AppendLines(2);

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }
    }
}