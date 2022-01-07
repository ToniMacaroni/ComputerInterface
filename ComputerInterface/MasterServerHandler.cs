using GorillaNetworking;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;
using UnityEngine;

namespace ComputerInterface
{
    public class MasterServerHandler : MonoBehaviourPunCallbacks
    {
        void Awake()
        {}
        private static void OnReturnCurrentVersion(ExecuteCloudScriptResult result)
        {
            JsonObject functionResult = (JsonObject)result.FunctionResult;
            object obj;
            GorillaComputer.instance.motdText.text = !functionResult.TryGetValue("MOTD", out obj) || !PhotonNetworkController.instance.gameVersion.Contains("live") ? (!functionResult.TryGetValue("MOTDBeta", out obj) || !PhotonNetworkController.instance.gameVersion.Contains("beta") ? "UNABLE TO LOAD MOTD" : (string)obj) : (string)obj;
        }
        private static void OnErrorShared(PlayFabError error)
        {
            Debug.LogError("Computer Interface failed at MOTD loading!");
        }
        private void OnGetTimeSuccess(GetTimeResult result)
        {
            GorillaComputer.instance.startupMillis = result.Time.Ticks / 10000L - (long)(Time.realtimeSinceStartup * 1000.0);
        }
        private void OnGetTimeFailure(PlayFabError error)
        {
            Debug.LogError("There was a problem getting the time. Error: " + error.GenerateErrorReport() + ". Trying again.");
            PlayFabClientAPI.GetTime(new GetTimeRequest(), new Action<GetTimeResult>(OnGetTimeSuccess), new Action<PlayFabError>(OnGetTimeFailure));
        }
        public override void OnConnectedToMaster()
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "ReturnCurrentVersion"
            }, new Action<ExecuteCloudScriptResult>(OnReturnCurrentVersion), new Action<PlayFabError>(OnErrorShared));
            PlayFabClientAPI.GetTime(new GetTimeRequest(), new Action<GetTimeResult>(OnGetTimeSuccess), new Action<PlayFabError>(OnGetTimeFailure));
        }
    }
}