using System;
using System.Collections.Generic;
using BepInEx;
using GorillaLocomotion;
using HarmonyLib;
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
        public static void SetColor(Color color)
        {
            SetColor(color.r, color.g, color.b);
        }

        public static void GetColor(out float r, out float g, out float b)
        {
            r = PlayerPrefs.GetFloat("redValue");
            g = PlayerPrefs.GetFloat("greenValue");
            b = PlayerPrefs.GetFloat("blueValue");
        }

        public static Color GetColor()
        {
            GetColor(out var r, out var g, out var b);
            return new Color(r, g, b);
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

        public static string GetName()
        {
            return PhotonNetwork.LocalPlayer.NickName;
        }

        public static void SetTurnMode(ETurnMode turnMode)
        {
            if (GorillaComputer.instance == null) return;

            var turnModeString = turnMode.ToString();
            var turnTypeField = AccessTools.Field(typeof(GorillaComputer), "turnType");
            var turnValueField = AccessTools.Field(typeof(GorillaComputer), "turnValue");
            turnTypeField.SetValue(GorillaComputer.instance, turnModeString);
            PlayerPrefs.SetString("stickTurning", turnModeString);
            PlayerPrefs.Save();
            GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode(turnModeString, (int)turnValueField.GetValue(GorillaComputer.instance));
        }

        public static ETurnMode GetTurnMode()
        {
            var turnMode = PlayerPrefs.GetString("stickTurning");
            if (turnMode.IsNullOrWhiteSpace()) return ETurnMode.NONE;
            return (ETurnMode) Enum.Parse(typeof(ETurnMode), turnMode);
        }

        public static void SetPttMode(EPTTMode mode)
        {
            if (!CheckForComputer(out var computer)) return;

            var modeString = mode switch
            {
                EPTTMode.AllChat => "ALL CHAT",
                EPTTMode.PushToTalk => "PUSH TO TALK",
                EPTTMode.PushToMute => "PUSH TO MUTE",
                _ => throw new ArgumentOutOfRangeException()
            };

            computer.pttType = modeString;
            PlayerPrefs.SetString("pttType", modeString);
            PlayerPrefs.Save();
        }

        public static EPTTMode GetPttMode()
        {
            var modeString = PlayerPrefs.GetString("pttType", "ALL CHAT");
            return modeString switch
            {
                "ALL CHAT" => EPTTMode.AllChat,
                "PUSH TO TALK" => EPTTMode.PushToTalk,
                "PUSH TO MUTE" => EPTTMode.PushToMute,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static void SetQueueMode(EQueueMode mode)
        {
            if (!CheckForComputer(out var computer)) return;

            computer.currentQueue = mode.ToString().ToUpper();
            PlayerPrefs.SetString("currentQueue", computer.currentQueue);
            PlayerPrefs.Save();
        }

        public static EQueueMode GetQueueMode()
        {
            var modeString = PlayerPrefs.GetString("currentQueue", "DEFAULT");
            return (EQueueMode) Enum.Parse(typeof(EQueueMode), modeString, true);
        }

        public static void SetGroupMode(EGroup mode)
        {
            if (!CheckForComputer(out var computer)) return;

            computer.groupMapJoin = mode.ToString().ToUpper();
            PlayerPrefs.SetString("groupMapJoin", computer.groupMapJoin);
            PlayerPrefs.Save();
        }

        public static EGroup GetGroupMode()
        {
            var modeString = PlayerPrefs.GetString("groupMapJoin", "FOREST");
            return (EGroup)Enum.Parse(typeof(EGroup), modeString, true);
        }

        public static void JoinAsGroup()
        {
            if (!CheckForComputer(out var computer)) return;

            if (PhotonNetwork.InRoom && !PhotonNetwork.CurrentRoom.IsVisible)
            {
                computer.networkController.joinWithFriends = true;
                computer.networkController.friendIDList = new List<string>(computer.friendJoinCollider.playerIDsCurrentlyTouching);
                Debug.Log(computer.networkController.friendIDList);
                computer.networkController.currentGameType = "";
                foreach (string message in computer.networkController.friendIDList)
                {
                    Debug.Log(message);
                }
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                {
                    if (computer.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
                    {
                        Debug.Log("sending player! " + player.UserId);
                        GorillaTagManager.instance.photonView.RPC("JoinPubWithFreinds", player, Array.Empty<object>());
                    }
                }
                PhotonNetwork.SendAllOutgoingCommands();
                if (computer.groupMapJoin == "FOREST")
                {
                    computer.forestMapTrigger.ComputerJoin();
                }
                else if (computer.groupMapJoin == "CAVE")
                {
                    computer.caveMapTrigger.ComputerJoin();
                }
            }
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

        public static void JoinRoom(string roomId, string gameType, bool isPrivate)
        {
            if (GorillaComputer.instance == null) return;
            if (string.IsNullOrWhiteSpace(roomId)) return;

            var networkController = GorillaComputer.instance.networkController;

            networkController.currentGameType = gameType;
            networkController.customRoomID = roomId;
            networkController.isPrivate = isPrivate;

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

        public static void JoinRoom(string roomId)
        {
            JoinRoom(roomId, "privatetag", true);
        }

        public static string GetRoomCode()
        {
            if (PhotonNetwork.InRoom) return PhotonNetwork.CurrentRoom.Name;
            return null;
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

        public static void InitMicState()
        {
            SetPttMode(GetPttMode());
        }

        public static void InitQueueState()
        {
            SetQueueMode(GetQueueMode());
        }

        public static void InitGroupState()
        {
            SetGroupMode(GetGroupMode());
        }

        public static void InitAll()
        {
            InitColorState();
            InitNameState();
            InitTurnState();
            InitMicState();
            InitQueueState();
            InitGroupState();
        }

        private static bool CheckForComputer(out GorillaComputer computer)
        {
            if (GorillaComputer.instance == null)
            {
                computer = null;
                return false;
            }

            computer = GorillaComputer.instance;
            return true;
        }

        public enum ETurnMode
        {
            SNAP,
            SMOOTH,
            NONE
        }

        public enum EPTTMode
        {
            AllChat = 0,
            PushToTalk = 1,
            PushToMute = 2
        }

        public enum EQueueMode
        {
            Default,
            Competitive
        }

        public enum EGroup
        {
            Forest,
            Cave
        }
    }
}