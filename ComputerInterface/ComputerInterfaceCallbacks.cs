using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

namespace ComputerInterface
{
    class ComputerInterfaceCallbacks : MonoBehaviourPunCallbacks
    {
        public static Action networkCallback;
        public static Action updateCallback;

        public override void OnJoinedRoom() => networkCallback.Invoke();
        public override void OnCreatedRoom() => networkCallback.Invoke();
        public override void OnLeftRoom() => networkCallback.Invoke();
        public override void OnDisconnected(DisconnectCause cause) => networkCallback.Invoke();
        public override void OnJoinRoomFailed(short returnCode, string message) => networkCallback.Invoke();
        public override void OnJoinRandomFailed(short returnCode, string message) => networkCallback.Invoke();
        public override void OnCreateRoomFailed(short returnCode, string message) => networkCallback.Invoke();
        // public override void OnSchoolFailed(short returnCode, string message) => networkCallback.Invoke();
        public override void OnConnected() => networkCallback.Invoke();
        public override void OnConnectedToMaster() => networkCallback.Invoke();
        void Update() => updateCallback.Invoke();
    }
}
