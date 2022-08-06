using Photon.Pun;
using Photon.Realtime;

namespace ComputerInterface.Views.GameSettings
{
    class JoinRoomViewCallbacks : MonoBehaviourPunCallbacks
    {
        public JoinRoomView view;

        public override void OnJoinedRoom() => view.Redraw();
        public override void OnCreatedRoom() => view.Redraw();
        public override void OnLeftRoom() => view.Redraw();
        public override void OnDisconnected(DisconnectCause cause) => view.Redraw();
        public override void OnJoinRoomFailed(short returnCode, string message) => view.Redraw();
        public override void OnJoinRandomFailed(short returnCode, string message) => view.Redraw();
        public override void OnCreateRoomFailed(short returnCode, string message) => view.Redraw();
        // public override void OnSchoolFailed(short returnCode, string message) => view.Redraw();
        public override void OnConnected() => view.Redraw();
        public override void OnConnectedToMaster() => view.Redraw();
    }
}
