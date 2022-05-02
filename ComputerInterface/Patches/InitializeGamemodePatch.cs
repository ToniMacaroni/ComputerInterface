using HarmonyLib;
using GorillaNetworking;
using UnityEngine;

namespace ComputerInterface.Patches
{
    [HarmonyPatch(typeof(GorillaComputer))]
    [HarmonyPatch("InitializeGameMode")]
    internal class InitializeGamemodePatch
    {
        internal static bool Prefix(GorillaComputer __instance)
        {
            __instance.OnModeSelectButtonPress(PlayerPrefs.GetString("currentGameMode", "INFECTION"));
            return false;
        }
    }
}
