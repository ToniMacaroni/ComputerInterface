using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using Zenject;

namespace ComputerInterface.RoomBrowser
{
    public class RoomExplorer : MonoBehaviourPunCallbacks, IInitializable
    {
        public HashSet<RoomInfo> Rooms = new HashSet<RoomInfo>(new RoomComparer());

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var roomInfo in roomList)
            { 
                var added = Rooms.Add(roomInfo);
                //if(added) Debug.LogWarning("Added room "+roomInfo.Name);
            }
        }

        public async void Initialize()
        {
            while (!PhotonNetwork.IsConnectedAndReady)
            {
                await Task.Delay(500);
            }

            PhotonNetwork.JoinLobby();
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