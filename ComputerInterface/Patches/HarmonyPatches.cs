using HarmonyLib;
using System.Reflection;

namespace ComputerInterface.Patches
{	public class HarmonyPatches
	{
		private static Harmony instance;

		public static bool IsPatched { get; private set; }
		public const string InstanceId = PluginInfo.Id;

		internal static void ApplyHarmonyPatches()
		{
			if (!IsPatched) {
				if (instance == null) {
					instance = new Harmony(InstanceId);
				}

				instance.PatchAll(Assembly.GetExecutingAssembly());
				IsPatched = true;
			}
		}

		internal static void RemoveHarmonyPatches()
		{
			if (instance != null && IsPatched) {
				instance.UnpatchSelf();
				IsPatched = false;
			}
		}
	}
}
