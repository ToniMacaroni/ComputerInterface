using System;
using BepInEx;
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
        public const int MAX_ROOM_LENGTH = 10;
        public const int MAX_NAME_LENGTH = 12;

        #region Color/Colour settings

        public static void SetColor(float r, float g, float b)
        {
            PlayerPrefs.SetFloat("redValue", Mathf.Clamp(r, 0f, 1f));
            PlayerPrefs.SetFloat("greenValue", Mathf.Clamp(g, 0f, 1f));
            PlayerPrefs.SetFloat("blueValue", Mathf.Clamp(b, 0f, 1f));

            GorillaTagger.Instance.UpdateColor(r, g, b);
            PlayerPrefs.Save();

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
        public static void InitializeNoobMaterial(Color color)
        {
            if (CheckForComputer(out var computer) && PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.All, color.r, color.g, color.b, computer.leftHanded);
        }

        #endregion

        #region Name settings

        public static string GetName()
        {
            if (CheckForComputer(out var computer)) return computer.savedName;
            return null;
        }

        public enum WordCheckResult
        {
            Allowed,
            Empty,
            Blank,
            NotAllowed,
            TooLong,
            ComputerNotFound
        }

        public static string WordCheckResultToMessage(WordCheckResult result)
        {
            return result switch
            {
                WordCheckResult.Allowed => "Input is allowed",
                WordCheckResult.Empty => "Input is empty",
                WordCheckResult.Blank => "Input is blank",
                WordCheckResult.NotAllowed => "Input is not allowed",
                WordCheckResult.TooLong => "Input is too long",
                WordCheckResult.ComputerNotFound => "Computer not found",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }

        private static WordCheckResult WordAllowed(string word)
        {
            if (word.Length == 0) return WordCheckResult.Empty;
            if (string.IsNullOrWhiteSpace(word)) return WordCheckResult.Blank;
            if (!CheckForComputer(out var computer)) return WordCheckResult.ComputerNotFound;
            if (!computer.CheckAutoBanListForName(word)) return WordCheckResult.NotAllowed;
            if (word.Length > MAX_NAME_LENGTH) return WordCheckResult.TooLong;
            return WordCheckResult.Allowed;
        }

        public static WordCheckResult SetName(string name)
        {
            if (!CheckForComputer(out var computer))
            {
                return WordCheckResult.ComputerNotFound;
            }

			var wordAllowed = WordAllowed(name);
            if (wordAllowed == WordCheckResult.Allowed)
            {
                name = name.Replace(" ", "");
                computer.currentName = name;
                PhotonNetwork.LocalPlayer.NickName = name;
                computer.offlineVRRigNametagText.text = name;
                computer.savedName = name;
                PlayerPrefs.SetString("playerName", name);
                PlayerPrefs.Save();

                GetColor(out var r, out var g, out var b);
                InitializeNoobMaterial(r, g, b);
            }

            return wordAllowed;
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
            => PhotonNetworkController.Instance.AttemptDisconnect();

        public static WordCheckResult JoinRoom(string roomId)
        {
            if (!CheckForComputer(out var computer))
            {
                return WordCheckResult.ComputerNotFound;
            }

            var roomAllowed = WordAllowed(roomId);

            if (roomAllowed == WordCheckResult.Allowed)
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomId);

            return roomAllowed;
        }

        public static string GetRoomCode()
        {
            if (PhotonNetwork.InRoom) return PhotonNetwork.CurrentRoom.Name;
            return null;
        }

        #endregion

        #region Support settings

        public static bool displaySupportTab;

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

        public static void InitSupportMode()
        {
            displaySupportTab = false;
        }

        public static void InitAll()
        {
            InitColorState();
            InitNameState();
            InitTurnState();
            InitMicState();
            InitVoiceMode();
            InitItemMode();
            InitSupportMode();

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
