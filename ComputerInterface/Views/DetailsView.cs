using System;
using System.Text;
using System.Xml.Schema;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using Photon.Pun;

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

        public DetailsView()
        {
            
        }

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
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLine();
            str.AppendClr("Name: ", "ffffff50").AppendLine().Repeat(" ", 4).Append(_name).AppendLine().AppendLine();
            str.AppendClr("Current Room   : ", "ffffff50").AppendLine().Repeat(" ", 4).Append(_roomCode).AppendLine().AppendLine();
            str.AppendClr("Players Online : ", "ffffff50").AppendLine().Repeat(" ", 4).Append(_playerCount).AppendLine();

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