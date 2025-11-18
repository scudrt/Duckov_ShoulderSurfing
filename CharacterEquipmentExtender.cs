using HarmonyLib;
using ItemStatsSystem.Items;
using ShoulderSurfing;
using System.Reflection;
using UnityEngine;

[HarmonyPatch(typeof(CharacterEquipmentController))]
[HarmonyPatch("ChangeEquipmentModel")]
public static class CharacterEquipmentExtender {
	static FieldInfo? characterControlField = null;
	public static void Postfix(CharacterEquipmentController __instance, ref Transform socket) {
		if (characterControlField == null) {
			characterControlField = typeof(CharacterEquipmentController).GetField("characterMainControl", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		if (ShoulderCamera.shoulderCameraInitalized == false) {
			return;
		}
		// Not the player, ignore it
		if ((CharacterMainControl)characterControlField.GetValue(__instance) != CharacterMainControl.Main) {
			return;
		}

		ShoulderCamera.TrySetTransformCameraFade(socket, true);
	}
}
