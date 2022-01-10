using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using GorillaLocomotion;

namespace PracticeMod.Patches
{
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("Update", MethodType.Normal)]
	internal class MovementSpeedPatch
	{
		public static bool ShouldOverride { get; private set; }
		public static float OverrideMaxJumpSpeed { get; private set; }
		public static float OverrideJumpMultiplier { get; private set; }

		internal static void Prefix(Player __instance)
		{
			if (ShouldOverride && Plugin.Allowed)
			{
				__instance.maxJumpSpeed = OverrideMaxJumpSpeed;
				__instance.jumpMultiplier = OverrideJumpMultiplier;
			}
		}

		public static void SetSpeed(float maxJumpSpeed, float jumpMultiplier)
		{
			ShouldOverride = true;
			OverrideMaxJumpSpeed = maxJumpSpeed;
			OverrideJumpMultiplier = jumpMultiplier;
		}

		public static void ResetSpeed()
		{
			ShouldOverride = false;
		}
	}
}
