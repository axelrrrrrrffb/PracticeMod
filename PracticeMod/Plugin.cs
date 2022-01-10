using System.Collections.Generic;
using BepInEx;
using Bepinject;
using Utilla;
using Photon.Pun;
using GorillaLocomotion;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using PracticeMod.Patches;

namespace PracticeMod
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
	[BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
	[BepInDependency("tonimacaroni.computerinterface", "1.4.0")]
	public class Plugin : BaseUnityPlugin
	{
		const int SurvivorMaterialIndex = 0;
		const int InfectedMaterialIndex = 1;

		static bool inPrivate;
		static bool Alone => PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom?.PlayerCount == 1;
		public static bool Allowed => inPrivate && Alone;

		InputDevice rightHand;
		InputDevice leftHand;

		bool previousRPrimary;
		bool previousLPrimary;

		Vector3 teleportPosition;
		float teleportRotation;

		void Awake()
		{
			Zenjector.Install<MainInstaller>().OnProject();
			Events.RoomJoined += OnRoomJoined;
			Events.RoomLeft += OnRoomLeft;
		}

		void OnEnable()
		{
			HarmonyPatches.ApplyHarmonyPatches();
		}

		void OnDisable()
		{
			HarmonyPatches.RemoveHarmonyPatches();
		}

		void Update()
		{
			if (!PhotonNetwork.InRoom) return;
			if (Allowed)
			{
				List<InputDevice> list = new List<InputDevice>();
				InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, list);
				rightHand = list[0];

				list = new List<InputDevice>();
				InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, list);
				leftHand = list[0];

				rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var rPrimary);
				leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out var lPrimary);

				if (rPrimary && !previousRPrimary)
				{
					Teleport();
				}

				if (lPrimary && !previousLPrimary)
				{
					StoreTeleport();
				}

				previousRPrimary = rPrimary;
				previousLPrimary = lPrimary;
			}
		}

		void OnRoomJoined(object sender, Events.RoomJoinedArgs e)
		{
			inPrivate = e.isPrivate;
		}

		void OnRoomLeft(object sender, Events.RoomJoinedArgs e)
		{
			MovementSpeedPatch.ResetSpeed();
			MaterialIndexPatch.ResetMaterial();
		}

		void Teleport()
		{
			if (teleportPosition == Vector3.zero) return;

			PlayerTeleportPatch.TeleportPlayer(teleportPosition, teleportRotation);
		}

		void StoreTeleport()
		{
			Transform t = Player.Instance.bodyCollider.transform;
			teleportPosition = t.position;
			teleportRotation = t.eulerAngles.y;
		}

		internal static void SetInfectedSpeed()
		{
			if (Allowed)
			{
				MovementSpeedPatch.SetSpeed(GorillaGameManager.instance.fastJumpLimit, GorillaGameManager.instance.fastJumpMultiplier);
				MaterialIndexPatch.SetMaterial(InfectedMaterialIndex);
			}
		}

		internal static void SetSurvivorSpeed()
		{
			if (Allowed)
			{
				MovementSpeedPatch.SetSpeed(GorillaGameManager.instance.slowJumpLimit, GorillaGameManager.instance.slowJumpMultiplier);
				MaterialIndexPatch.SetMaterial(SurvivorMaterialIndex);
			}
		}

		internal static void SetSpecificSpeed(int playerCount, int infectedCount, bool infected)
		{
			if (Allowed)
			{
				float percentage = infected ? (playerCount - infectedCount) : (infectedCount - 1) * 0.9f;
				percentage /= (float)(playerCount - 1);

				float fastJumpLimit = GorillaGameManager.instance.fastJumpLimit;
				float slowJumpLimit = GorillaGameManager.instance.slowJumpLimit;
				float fastJumpMultiplier = GorillaGameManager.instance.fastJumpMultiplier;
				float slowJumpMultiplier = GorillaGameManager.instance.slowJumpMultiplier;

				float computedJumpLimit = (fastJumpLimit - slowJumpLimit) * percentage + slowJumpLimit;
				float computedJumpMultiplier = (fastJumpMultiplier - slowJumpMultiplier) * percentage + slowJumpMultiplier;

				MovementSpeedPatch.SetSpeed(computedJumpLimit, computedJumpMultiplier);
			}
		}
	}
}
