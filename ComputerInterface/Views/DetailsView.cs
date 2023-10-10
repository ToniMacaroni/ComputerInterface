using System;
using System.Text;
using BepInEx;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using GorillaNetworking;
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
            _playerCount = PhotonNetworkController.Instance.TotalUsers();
            _playerBans = GorillaComputer.instance.GetField<int>("usersBanned");
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Details").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
            str.Append("<size=40>Press any key to update page</size>").AppendLines(2);

            str.BeginColor("ffffff50").Append("Name: ").EndColor();
            str.Append($"<size=50>{_name}</size>").AppendLine();
            str.BeginColor("ffffff50").Append("Display Name: ").EndColor();
            str.Append($"<size=50>{GorillaTagger.Instance.offlineVRRig.NormalizeName(true, _name).ToUpper()}</size>").AppendLines(3);

            str.BeginColor("ffffff50").Append("Players Online: ").EndColor();
            str.Append($"<size=50>{_playerCount}</size>").AppendLine();
            str.BeginColor("ffffff50").Append("Users Banned: ").EndColor();
            str.Append($"<size=50>{_playerBans} (Yesterday)</size>").AppendLines(3);

            str.BeginColor("ffffff50").Append("Current Room: ").EndColor();
            str.Append($"<size=50>{(_roomCode.IsNullOrWhiteSpace() ? "-None-" : _roomCode)}</size>").AppendLine();

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
                default:
                    Redraw();
                    break;
            }
        }
    }
}