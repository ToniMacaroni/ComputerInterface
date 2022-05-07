using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;

/* A Haunted Army#2861 */
/* A Haunted Army#2861 */
/* A Haunted Army#2861 */

namespace ComputerInterface.Patches
{
    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("AttemptToFollowFriendIntoPub", MethodType.Normal)]
    internal class Fix_AttemptToFollowFriendIntoPub
    {
        private static void Prefix(PhotonNetworkController __instance, ref PhotonNetworkController.ConnectionState ___currentState)
        {
            if (!PhotonNetwork.InLobby || ___currentState != PhotonNetworkController.ConnectionState.ConnectedAndWaiting)
                return;
            __instance.AttemptDisconnect();
        }
    }
    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("AttemptToJoinPublicRoom", MethodType.Normal)]
    internal class Fix_AttemptToJoinPublicRoom
    {
        private static void Prefix(PhotonNetworkController __instance, ref PhotonNetworkController.ConnectionState ___currentState)
        {
            if (!PhotonNetwork.InLobby || ___currentState != PhotonNetworkController.ConnectionState.ConnectedAndWaiting)
                return;
            __instance.AttemptDisconnect();
        }
    }
    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("AttemptJoinPublicWithFriends", MethodType.Normal)]
    internal class Fix_AttemptJoinPublicWithFriends
    {
        private static void Prefix(PhotonNetworkController __instance, ref PhotonNetworkController.ConnectionState ___currentState)
        {
            if (!PhotonNetwork.InLobby || ___currentState != PhotonNetworkController.ConnectionState.ConnectedAndWaiting)
                return;
            __instance.AttemptDisconnect();
        }
    }
    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("AttemptToJoinSpecificRoom", MethodType.Normal)]
    internal class Fix_AttemptToJoinSpecificRoom
    {
        private static void Prefix(PhotonNetworkController __instance, ref PhotonNetworkController.ConnectionState ___currentState)
        {
            if (!PhotonNetwork.InLobby || ___currentState != PhotonNetworkController.ConnectionState.ConnectedAndWaiting)
                return;
            __instance.AttemptDisconnect();
        }
    }
}
