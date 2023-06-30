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

        public static void SetField(this object obj, string name, object value)
        {
            var field = AccessTools.Field(obj.GetType(), name);
            field.SetValue(obj, value);
        }

        public static T GetField<T>(this object obj, string name)
        {
            var field = AccessTools.Field(obj.GetType(), name);
            return (T)field.GetValue(obj);
        }
    }
}