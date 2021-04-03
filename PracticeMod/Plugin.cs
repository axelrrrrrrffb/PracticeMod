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

namespace PracticeMod
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
	[BepInDependency("org.legoandmars.gorillatag.utilla", "1.3.0")]
	[BepInDependency("tonimacaroni.computerinterface", "1.4.0")]
	public class Plugin : BaseUnityPlugin
	{
		static bool inPrivate;
		static bool modEnabled;
		static bool Alone => PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom?.PlayerCount == 1;
		static bool Allowed => Alone && inPrivate;

		static Harmony harmony;

		bool isPatched;

		InputDevice rightHand;
		InputDevice leftHand;

		bool previousRPrimary;
		bool previousLPrimary;

		Vector3 teleportPosition;
		float teleportRotation;

		void Awake()
		{
			Zenjector.Install<MainInstaller>().OnProject();
			Events.RoomJoined += RoomJoined;

			harmony = new Harmony(PluginInfo.GUID);
			if (!modEnabled)
			{
				OnEnable();
			}
		}

		void Update()
		{
			if (!PhotonNetwork.InRoom) return;
			if (Allowed && isPatched)
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

		void OnEnable()
		{
			modEnabled = true;
			ApplyPatches();
		}

		void OnDisable()
		{
			modEnabled = false;
			RemovePatches();
		}

		void ApplyPatches()
		{
			if (isPatched) return;

			harmony.PatchAll(Assembly.GetExecutingAssembly());
			isPatched = true;
		}

		void RemovePatches()
		{
			if (!isPatched) return;

			harmony.PatchAll(Assembly.GetExecutingAssembly());
			isPatched = false;
		}

		void RoomJoined(object sender, Events.RoomJoinedArgs e)
		{
			inPrivate = e.isPrivate;
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

		public static void SetInfectedSpeed()
		{
			if (Allowed)
			{
				Player.Instance.maxJumpSpeed = GorillaTagManager.instance.fastJumpLimit;
				Player.Instance.jumpMultiplier = GorillaTagManager.instance.fastJumpMultiplier;
				SetMat(1);
			}
		}

		public static void SetSurvivorSpeed()
		{
			if (Allowed)
			{
				Player.Instance.maxJumpSpeed = GorillaTagManager.instance.slowJumpLimit;
				Player.Instance.jumpMultiplier = GorillaTagManager.instance.slowJumpMultiplier;
				SetMat(0);
			}
		}

		public static void SetSpecificSpeed(int playerCount, int infectedCount, bool infected)
		{
			if (Allowed)
			{
				float percentage = infected ? (playerCount - infectedCount) : (infectedCount - 1) * 0.9f;
				percentage /= (float)(playerCount - 1);

				float fastJumpMultiplier = GorillaTagManager.instance.fastJumpMultiplier;
				float slowJumpMultiplier = GorillaTagManager.instance.slowJumpMultiplier;
				float fastJumpLimit = GorillaTagManager.instance.fastJumpLimit;
				float slowJumpLimit = GorillaTagManager.instance.slowJumpLimit;

				float computedJumpMultiplier = (fastJumpMultiplier - slowJumpMultiplier) * percentage + slowJumpMultiplier;
				float computedJumpLimit = (fastJumpLimit - slowJumpLimit) * percentage + slowJumpLimit;

				Player.Instance.jumpMultiplier = computedJumpMultiplier;
				Player.Instance.maxJumpSpeed = computedJumpLimit;
			}
		}

		static void SetMat(int index)
		{
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			hashtable.Add("matIndex", index);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
		}
	}
}
