using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using UnityEngine;
using Zenject;

namespace ComputerInterface.RoomBrowser
{
    public class CIRoomManager : MonoBehaviourPunCallbacks, IInitializable
    {
        public HashSet<RoomInfo> Rooms = new HashSet<RoomInfo>(new RoomComparer());

        public TypedLobby TypedLobby { get; private set; }

        private Action _masterConnectionInvokeList;

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var roomInfo in roomList)
            {
                if (!Rooms.Add(roomInfo))
                {
                    Rooms.Remove(roomInfo);
                    Rooms.Add(roomInfo);
                }
            }
        }

        public override void OnConnectedToMaster()
        {
            if (_masterConnectionInvokeList == null) return;

            foreach (var action in _masterConnectionInvokeList.GetInvocationList().Cast<Action>())
            {
                try
                {
                    action();
                }
                catch (Exception) { }
            }

            _masterConnectionInvokeList = null;
        }

        public void RegisterConnectedToMasterCallback(Action action)
        {
            if (action == null) return;
            _masterConnectionInvokeList += action;
        }

        public void CreateRoom(string roomId, string description)
        {
            // ==== Set up base game stuff ====
            #region Set up base game stuff

            var networkController = GorillaComputer.instance.networkController;

            networkController.currentGameType = 
                networkController.currentGameType.IsNullOrWhiteSpace()
                    ? "privatetag"
                    : networkController.currentGameType;

            networkController.customRoomID = roomId;
            networkController.isPrivate = false;
            networkController.attemptingToConnect = false;

            PhotonNetwork.LocalPlayer.NickName = GorillaComputer.instance.savedName;
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                networkController.playFabAuthenticator.SetDisplayName(PhotonNetwork.LocalPlayer.NickName);
            }

            PhotonNetwork.AutomaticallySyncScene = true;

            #endregion

            var props = new RoomProps();

            props.SetDescription(description);
            props.SetGameMode(networkController.currentGameType);

            var mods = Chainloader.PluginInfos.Values.Select(x => x.Metadata.Name);
            props.SetMods(mods);

            var options = new RoomOptions();
            options.IsOpen = true;
            options.IsVisible = true;
            options.CustomRoomProperties = props.GetHashTable();
            options.MaxPlayers = 10;

            options.PublishUserId = true;

            options.CustomRoomPropertiesForLobby = props.GetKeys();

            PhotonNetwork.CreateRoom(roomId, options, TypedLobby);
        }

        public async void Initialize()
        {
            while (!PhotonNetwork.IsConnectedAndReady)
            {
                await Task.Delay(500);
            }

            TypedLobby = new TypedLobby("RoomBrowserLobby", LobbyType.Default);
            PhotonNetwork.JoinLobby(TypedLobby);
        }

        internal class RoomComparer : IEqualityComparer<RoomInfo>
        {
            public bool Equals(RoomInfo x, RoomInfo y)
            {
                return x?.Name == y?.Name;
            }

            public int GetHashCode(RoomInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}