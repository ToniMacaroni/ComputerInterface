using HarmonyLib;

namespace ComputerInterface
{
    internal static class ReflectionEx
    {
        public static void InvokeMethod(this object obj, string name, params object[] par)
        {
            var method = AccessTools.Method(obj.GetType(), name);
            method.Invoke(obj, par);
        }
    }
}