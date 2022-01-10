using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeMod.Patches
{
	[HarmonyPatch(typeof(VRRig))]
	[HarmonyPatch("LateUpdate", MethodType.Normal)]
	internal class MaterialIndexPatch
	{
		static bool shouldOverride;
		static int overrideMaterial;

		internal static void Postfix(ref VRRig __instance)
		{
			if (shouldOverride && Plugin.Allowed)
			{
				__instance.mainSkin.material = __instance.materialsToChangeTo[overrideMaterial];
			}
		}

		public static void SetMaterial(int materialIndex)
		{
			shouldOverride = true;
			overrideMaterial = materialIndex;
		}

		public static void ResetMaterial()
		{
			shouldOverride = false;
		}
	}
}
