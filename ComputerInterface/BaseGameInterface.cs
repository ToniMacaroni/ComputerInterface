using System;
using System.Xml.Linq;
using BepInEx;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ComputerInterface
{
    // TODO: Refactor to PlayerModel instance
    public static class BaseGameInterface
    {
        public const int MAX_ROOM_LENGTH = 10;
        public const int MAX_NAME_LENGTH = 12;

        #region Color/Colour settings

        public static void SetColor(float r, float g, float b)
        {
            PlayerPrefs.SetFloat("redValue", Mathf.Clamp(r, 0f, 1f));
            PlayerPrefs.SetFloat("greenValue", Mathf.Clamp(g, 0f, 1f));
            PlayerPrefs.SetFloat("blueValue", Mathf.Clamp(b, 0f, 1f));
            PlayerPrefs.Save();

            GorillaTagger.Instance.UpdateColor(r, g, b);
            InitializeNoobMaterial(r, g, b);
        }
        public static void SetColor(Color color) => SetColor(color.r, color.g, color.b);

        public static void GetColor(out float r, out float g, out float b)
        {
            r = Mathf.Clamp(PlayerPrefs.GetFloat("redValue"), 0f, 1f);
            g = Mathf.Clamp(PlayerPrefs.GetFloat("greenValue"), 0f, 1f);
            b = Mathf.Clamp(PlayerPrefs.GetFloat("blueValue"), 0f, 1f);
        }

        public static Color GetColor()
        {
            GetColor(out var r, out var g, out var b);
            return new Color(r, g, b);
        }

        public static void InitializeNoobMaterial(float r, float g, float b) => InitializeNoobMaterial(new Color(r, g, b));

        public static void InitializeNoobMaterial(Color color) => GorillaTagger.Instance.myVRRig?.photonView.RPC("InitializeNoobMaterial", RpcTarget.All, color.r, color.g, color.b, GorillaComputer.instance?.leftHanded ?? true);

        #endregion

        #region Name settings

        public static string GetName()
        {
            if (CheckForComputer(out var computer)) return computer.savedName.IsNullOrWhiteSpace() ? GorillaComputer.instance.savedName : PhotonNetwork.LocalPlayer.NickName;
            return null;
        }

        public static void SetName(string name, out bool error)
        {
            error = false;
            if (CheckForComputer(out var computer))
            {
                if (!NameAllowed(name))
                {
                    error = true;
                    return;
                }

                computer.offlineVRRigNametagText.text = name;
                computer.savedName = name;
                PlayerPrefs.SetString("playerName", name);
                PlayerPrefs.Save();

                GetColor(out var r, out var g, out var b);
                SetColor(r, g, b);

                return;
            }
            error = true;
        }

        private static bool NameAllowed(string name)
        {
            if (CheckForComputer(out var computer))
            {
                if (name.Length == 0) return false;
                if (string.IsNullOrWhiteSpace(name)) return false;
                if (!computer.CheckAutoBanListForName(name)) return false;
                if (name.Length > MAX_NAME_LENGTH) return false;

                return true;
            }

            return false;
        }

        #endregion

        #region Turn settings

        public static void SetTurnMode(ETurnMode turnMode)
        {
            if (!CheckForComputer(out var computer)) return;

            var turnModeString = turnMode.ToString();
            var turnTypeField = AccessTools.Field(typeof(GorillaComputer), "turnType");
            var turnValueField = AccessTools.Field(typeof(GorillaComputer), "turnValue");
            turnTypeField.SetValue(computer, turnModeString);
            PlayerPrefs.SetString("stickTurning", turnModeString);
            PlayerPrefs.Save();
            GorillaTagger.Instance.GetComponent<GorillaSnapTurn>().ChangeTurnMode(turnModeString, (int)turnValueField.GetValue(computer));
        }

        public static ETurnMode GetTurnMode()
        {
            var turnMode = PlayerPrefs.GetString("stickTurning");
            if (turnMode.IsNullOrWhiteSpace()) return ETurnMode.NONE;
            return (ETurnMode)Enum.Parse(typeof(ETurnMode), turnMode);
        }

        #endregion

        #region Item settings

        public static void SetInstrumentVolume(int value)
        {
            PlayerPrefs.SetFloat("instrumentVolume", value / 50f);
            PlayerPrefs.Save();
        }

        public static float GetInstrumentVolume()
        {
            float instVolume = PlayerPrefs.GetFloat("instrumentVolume", 0.1f);
            return (instVolume);
        }

        public static void SetItemMode(bool disableParticles)
        {
            if (!CheckForComputer(out var computer)) return;

            computer.disableParticles = disableParticles;
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

        #endregion

        #region Turn settings

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

        #endregion

        #region Microphone settings

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

        #endregion

        #region Voice settings

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

        #endregion

        #region Group settings

        public static void JoinGroupMap(int map)
        {
            if (!CheckForComputer(out var computer)) return;

            var allowedMapsToJoin = GetGroupJoinMaps();

            map = Mathf.Min(allowedMapsToJoin.Length - 1, map);

            computer.groupMapJoin = allowedMapsToJoin[map].ToUpper();
            computer.groupMapJoinIndex = map;
            PlayerPrefs.SetString("groupMapJoin", computer.groupMapJoin);
            PlayerPrefs.SetInt("groupMapJoinIndex", computer.groupMapJoinIndex);
            PlayerPrefs.Save();

            computer.OnGroupJoinButtonPress(Mathf.Min(allowedMapsToJoin.Length - 1, map), computer.friendJoinCollider);
        }

        public static string[] GetGroupJoinMaps()
        {
            if (!CheckForComputer(out var computer)) return Array.Empty<string>();
            return computer.allowedMapsToJoin;
        }

        #endregion

        #region Room settings

        public static void Disconnect()
        {
            if (CheckForComputer(out var computer))
            {
                computer.networkController.AttemptDisconnect();
            }
        }

        public static void JoinRoom(string roomId, out bool error)
        {
            error = false;
            if (CheckForComputer(out var computer))
            {
                if (!RoomAllowed(roomId))
                {
                    error = true;
                    return;
                }

                computer.networkController.AttemptToJoinSpecificRoom(roomId);
                return;
            }
            error = true;
        }

        public static bool RoomAllowed(string roomId)
        {
            if (CheckForComputer(out _))
            {
                if (roomId.Length == 0) return false;
                if (string.IsNullOrWhiteSpace(roomId)) return false;
                if (!GorillaComputer.instance.CheckAutoBanListForName(roomId)) return false;
                if (roomId.Length > MAX_ROOM_LENGTH) return false;

                return true;
            }

            return false;
        }

        public static string GetRoomCode()
        {
            if (PhotonNetwork.InRoom) return PhotonNetwork.CurrentRoom.Name;
            return null;
        }

        #endregion

        #region Initialization

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
            SetName(name, out _);
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

            string currentGameMode = gamemode.IsNullOrWhiteSpace() ? PlayerPrefs.GetString("currentGameMode", "INFECTION") : gamemode;
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

        #endregion

        public static bool CheckForComputer(out GorillaComputer computer)
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

        public enum EGameMode
        {
            INFECTION,
            CASUAL,
            HUNT
        }
    }
}
