using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ComputerInterface
{
    // TODO: Refactor to PlayerModel instance
    public static class BaseGameInterface
    {
        public static void SetColor(float r, float g, float b)
        {
            PlayerPrefs.SetFloat("redValue", r);
            PlayerPrefs.SetFloat("greenValue", g);
            PlayerPrefs.SetFloat("blueValue", b);
            GorillaTagger.Instance.UpdateColor(r, g, b);
            PlayerPrefs.Save();
            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.photonView.RPC("InitializeNoobMaterial", RpcTarget.All, r, g, b);
            }
        }

        public static void SetName(string name)
        {
            if (GorillaComputer.instance == null) return;

            PhotonNetwork.LocalPlayer.NickName = name; 
            GorillaComputer.instance.offlineVRRigNametagText.text = name;
            GorillaComputer.instance.savedName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();
        }

        public static void Disconnect()
        {
            if (GorillaComputer.instance == null) return;

            if (PhotonNetwork.InRoom)
            {
                GorillaComputer.instance.networkController.attemptingToConnect = false;
                GorillaScoreboardSpawner[] componentsInChildren = PhotonNetworkController.instance.currentGorillaParent.GetComponentsInChildren<GorillaScoreboardSpawner>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].OnLeftRoom();
                }
                foreach (SkinnedMeshRenderer skinnedMeshRenderer in GorillaComputer.instance.networkController.offlineVRRig)
                {
                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.enabled = true;
                    }
                }
                foreach (GorillaLevelScreen gorillaLevelScreen in GorillaComputer.instance.levelScreens)
                {
                    gorillaLevelScreen.UpdateText(gorillaLevelScreen.startingText, true);
                }
                Player.Instance.maxJumpSpeed = 6.5f;
                Player.Instance.jumpMultiplier = 1.1f;
                PhotonNetwork.Disconnect();
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public static void JoinRoom(string roomId)
        {
            if (GorillaComputer.instance == null) return;
            if (string.IsNullOrWhiteSpace(roomId)) return;

            var networkController = GorillaComputer.instance.networkController;

            networkController.currentGameType = "privatetag";
            networkController.customRoomID = roomId;
            networkController.isPrivate = true;

            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name != roomId)
            {
                GorillaScoreboardSpawner[] componentsInChildren = PhotonNetworkController.instance.currentGorillaParent.GetComponentsInChildren<GorillaScoreboardSpawner>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].OnLeftRoom();
                }
                networkController.attemptingToConnect = true;
                foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in networkController.offlineVRRig)
                {
                    if (skinnedMeshRenderer2 != null)
                    {
                        skinnedMeshRenderer2.enabled = true;
                    }
                }
                PhotonNetwork.Disconnect();
                Player.Instance.maxJumpSpeed = 6.5f;
                Player.Instance.jumpMultiplier = 1.1f;
                return;
            }

            if (!PhotonNetwork.InRoom && !networkController.attemptingToConnect)
            {
                networkController.attemptingToConnect = true;
                networkController.AttemptToConnectToRoom();
                Debug.Log("attempting to connect");
            }
        }

        public static void InitColorState()
        {
            GorillaTagger.Instance.UpdateColor(
                PlayerPrefs.GetFloat("redValue", 0f),
                PlayerPrefs.GetFloat("greenValue", 0f),
                PlayerPrefs.GetFloat("blueValue", 0f));
        }

        public static void InitNameState()
        {
            var name = PlayerPrefs.GetString("playerName", "gorilla");
            SetName(name);
        }

        public static void InitTurnState()
        {
            var gorillaTurn = GorillaTagger.Instance.GetComponent<GorillaSnapTurn>();
            var defaultValue = Application.platform == RuntimePlatform.Android ? "NONE" : "SNAP";
            var turnType = PlayerPrefs.GetString("stickTurning", defaultValue);
            var turnValue = PlayerPrefs.GetInt("turnFactor", 4);
            gorillaTurn.ChangeTurnMode(turnType, turnValue);
        }

        public static void InitAll()
        {
            InitColorState();
            InitNameState();
            InitTurnState();
        }
    }
}