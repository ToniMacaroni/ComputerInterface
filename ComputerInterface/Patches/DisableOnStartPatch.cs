using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerInterface.Patches
{
    [HarmonyPatch(typeof(PhotonNetworkController), "DisableOnStart", MethodType.Enumerator)]
    public class DisableOnStartPatch
    {
        public static bool Prefix() => false;
    }
}
