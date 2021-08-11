using System;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using UnityEngine;

namespace ComputerInterface
{
    public class MOTDHandler : MonoBehaviourPunCallbacks
    {
        void Awake()
        {
            Debug.Log("Computer Interface: MOTDHandler::Awake");
        }
        private static void OnReturnCurrentVersion(ExecuteCloudScriptResult result)
        {
            Debug.Log("Computer Interface got MOTD answer!");
            JsonObject functionResult = (JsonObject)result.FunctionResult;
            object obj;
            GorillaComputer.instance.motdText.text = !functionResult.TryGetValue("MOTD", out obj) || !PhotonNetworkController.instance.gameVersion.Contains("live") ? (!functionResult.TryGetValue("MOTDBeta", out obj) || !PhotonNetworkController.instance.gameVersion.Contains("beta") ? "UNABLE TO LOAD MOTD" : (string)obj) : (string)obj;
        }
        private static void OnErrorShared(PlayFabError error)
        {
            Debug.LogError("Computer Interface failed at MOTD loading!");
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("Computer Interface trying to catch MOTD...");
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "ReturnCurrentVersion"
            }, new Action<ExecuteCloudScriptResult>(OnReturnCurrentVersion), new Action<PlayFabError>(OnErrorShared));
        }
    }
}