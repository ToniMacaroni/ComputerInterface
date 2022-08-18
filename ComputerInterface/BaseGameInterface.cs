using System;
using System.Collections.Generic;
using BepInEx;
using GorillaLocomotion;
using GorillaNetworking;
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
                InitializeNoobMaterial(r, g, b);
            }
        }
        public static void SetColor(Color color) => SetColor(color.r, color.g, color.b);

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

            if (!GorillaComputer.instance.CheckAutoBanListForName(name)) return;

            PhotonNetwork.LocalPlayer.NickName = name;
            GorillaComputer.instance.offlineVRRigNametagText.text = name;
            GorillaComputer.instance.savedName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();

            /* Player's name is not updating on change */
            if (PhotonNetwork.InRoom)
            {
                GetColor(out var r, out var g, out var b);
                InitializeNoobMaterial(r, g, b);
            }
            /* Player's name is not updating on change */
        }

        public static void InitializeNoobMaterial(Color colour) => _InitializeNoobMaterial(colour);

        public static void InitializeNoobMaterial(float r, float g, float b) => _InitializeNoobMaterial(new Color(r, g, b));

        private static void _InitializeNoobMaterial(Color colour)
        {
            // Why the hell does this game check if you're left handed or not
            if (PhotonNetwork.InRoom)
                GorillaTagger.Instance.myVRRig.photonView.RPC("InitializeNoobMaterial", RpcTarget.All, colour.r, colour.g, colour.b, PlayerPrefs.GetInt("leftHanded", 0) == 1);
            else
                Console.WriteLine("Tried sending InitializeNoobMaterial but I'm not in a room!!");

            //GorillaComputer.instance.check
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
            return (ETurnMode)Enum.Parse(typeof(ETurnMode), turnMode);
        }

        public static void SetInstrumentVolume(int value)
        {
            PlayerPrefs.SetFloat("instrumentVolume", (float)value / 50f);
            PlayerPrefs.Save();
        }

        public static float GetInstrumentVolume()
        {
            float instVolume = PlayerPrefs.GetFloat("instrumentVolume", 0.1f);
            return (instVolume);
        }

        public static void SetItemMode(bool disableParticles)
        {
            PlayerPrefs.SetString("disableParticles", disableParticles ? "TRUE" : "FALSE");
            PlayerPrefs.Save();
            GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
        }

        public static bool GetItemMode()
        {
            string itemMode = PlayerPrefs.GetString("disableParticles");
            if (itemMode.IsNullOrWhiteSpace()) return false;
            return itemMode == "TRUE";
        }

        public static void SetTurnValue(int value)
        {
            if (!CheckForComputer(out var computer)) return;

            computer.SetField("turnValue", value);
            PlayerPrefs.SetInt("turnFactor", value);
            PlayerPrefs.Save();
            GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode(computer.GetField<string>("turnType"), value);
        }

        public static int GetTurnValue()
        {
            return PlayerPrefs.GetInt("turnFactor", 4);
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

        public static void SetGroupMode(EGroup mode)
        {
            if (!CheckForComputer(out var computer)) return;

            computer.groupMapJoin = mode.ToString().ToUpper();
            PlayerPrefs.SetString("groupMapJoin", computer.groupMapJoin);
            PlayerPrefs.Save();
        }

        public static void SetVoiceMode(bool voiceChatOn)
        {
            if (!CheckForComputer(out var computer)) return;
            computer.voiceChatOn = voiceChatOn ? "TRUE" : "FALSE";
            PlayerPrefs.SetString("voiceChatOn", computer.voiceChatOn);
            PlayerPrefs.Save();
        }

        public static bool GetVoiceMode()
        {
            return PlayerPrefs.GetString("voiceChatOn", "TRUE") == "TRUE";
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
                PhotonNetworkController.Instance.friendIDList = new List<string>(computer.friendJoinCollider.playerIDsCurrentlyTouching);
                Debug.Log(computer.networkController.friendIDList);
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
                GorillaNetworkJoinTrigger triggeredTrigger = null;
                switch (computer.groupMapJoin)
                {
                    case "FOREST":
                        triggeredTrigger = computer.forestMapTrigger;
                        break;
                    case "CAVE":
                        triggeredTrigger = computer.caveMapTrigger;
                        break;
                    case "CANYON":
                        triggeredTrigger = computer.canyonMapTrigger;
                        break;
                    case "CITY":
                        triggeredTrigger = computer.cityMapTrigger;
                        break;
                    case "MOUNTAIN":
                        triggeredTrigger = computer.mountainMapTrigger;
                        break;
                    default:
                        triggeredTrigger = computer.forestMapTrigger;
                        Console.WriteLine("The group map to join does not exist");
                        break;
                }
                PhotonNetworkController.Instance.AttemptJoinPublicWithFriends(triggeredTrigger);
            }
        }

        public static void Disconnect()
        {
            PhotonNetworkController.Instance.AttemptDisconnect();
        }

        public static void JoinRoom(string roomId)
        {
            if (!CheckForComputer(out var computer)) return;
            if (string.IsNullOrWhiteSpace(roomId)) return;
            if (!GorillaComputer.instance.CheckAutoBanListForName(roomId)) return;

            computer.networkController.AttemptToJoinSpecificRoom(roomId);
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

        public static void InitGroupState()
        {
            SetGroupMode(GetGroupMode());
        }

        public static void InitVoiceMode()
        {
            SetVoiceMode(GetVoiceMode());
        }

        public static void InitItemMode()
        {
            SetItemMode(GetItemMode());
        }

        public static string InitGameMode(string gamemode = "")
        {
            if (!CheckForComputer(out var computer)) return "";

            string currentGameMode = gamemode.IsNullOrWhiteSpace() ? currentGameMode = PlayerPrefs.GetString("currentGameMode", "INFECTION") : gamemode;
            computer.currentGameMode = currentGameMode;
            computer.OnModeSelectButtonPress(currentGameMode, computer.leftHanded);
            return currentGameMode;
        }

        public static void InitAll()
        {
            InitColorState();
            InitNameState();
            InitTurnState();
            InitMicState();
            InitGroupState();
            InitVoiceMode();
            InitItemMode();

            // The computer will reset custom gamemodes when start is called
            // var gamemode = InitGameMode();


            if (CheckForComputer(out var computer))
            {
                computer.InvokeMethod("Awake");
            }

            // InitGameMode(gamemode);

            //PhotonNetworkController.instance.SetField("pastFirstConnection", true);
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

        public enum EGroup
        {
            Forest,
            Cave,
            Canyon,
            City
        }

        public enum EGameMode
        {
            INFECTION,
            CASUAL,
            HUNT
        }
    }
}
