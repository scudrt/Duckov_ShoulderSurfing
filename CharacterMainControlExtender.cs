using HarmonyLib;
using ShoulderSurfing;
using UnityEngine;

public static class CharacterMainControlCommon {
	public static bool enableWallHacking = false; // 是否启用透视
	public static Shader? showBackShader = null;
	public static bool syncWithCameraDirection = false; // If false, the character direction will not sync with camera direction until shoot or aim

	public static float lastTimeCharacterTrigger = 0f;
	public static float aimDurationAfterTrigger = 2.5f; // seconds
}

[HarmonyPatch(typeof(CharacterMainControl))]
[HarmonyPatch("SetCharacterModel")]
public static class CharacterSetModelExtender {
	public static void Postfix(CharacterMainControl __instance) {
		if (CharacterMainControlCommon.showBackShader == null) {
			CharacterMainControlCommon.showBackShader = Shader.Find("CharacterShowBack");
		}
		if (CharacterMainControlCommon.enableWallHacking) {
			return;
		}

		foreach (var renderer in __instance.characterModel.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			foreach (var mat in renderer.materials) {
				if (!mat || mat.shader != CharacterMainControlCommon.showBackShader) {
					continue;
				}
				mat.renderQueue = 0;
				return; // Target material found, return directly
			}
		}
	}
}

[HarmonyPatch(typeof(CharacterMainControl))]
[HarmonyPatch("IsAiming")]
public static class CharacterIsAimingExtender {
	
	public static bool Prefix(CharacterMainControl __instance, ref bool __result) {
		if (!ShoulderCamera.shoulderCameraInitalized) {
			return true;
		}
		if (CharacterMainControlCommon.syncWithCameraDirection) {
			return true;
		}
		if (__instance != CharacterMainControl.Main) {
			return true;
		}

		if (__instance.Running) {
			// No aiming while character is running
			__result = false;
			// Clear character trigger state for smooth aim state transition
			CharacterMainControlCommon.lastTimeCharacterTrigger = 0f;
		} else {
			bool isPlayerTriggeredRecently = (Time.unscaledTime - CharacterMainControlCommon.lastTimeCharacterTrigger) <= CharacterMainControlCommon.aimDurationAfterTrigger;
			__result = __instance.IsInAdsInput || isPlayerTriggeredRecently;
		}

		return false;
	}
}

[HarmonyPatch(typeof(CharacterMainControl))]
[HarmonyPatch("Trigger")]
public static class CharacterTriggerExtender {

	public static bool Prefix(CharacterMainControl __instance, ref bool trigger) {
		if (!ShoulderCamera.shoulderCameraInitalized) {
			return true;
		}
		if (CharacterMainControlCommon.syncWithCameraDirection) {
			return true;
		}

		// Refresh trigger time stamp for free camera
		if (trigger && __instance == CharacterMainControl.Main) {
			CharacterMainControlCommon.lastTimeCharacterTrigger = Time.unscaledTime;
		}

		return true;
	}
}

[HarmonyPatch(typeof(CharacterMainControl))]
[HarmonyPatch("get_CharacterTurnSpeed")]
public static class CharacterTurnSpeedGetterExtender {

	public static bool Prefix(CharacterMainControl __instance, ref float __result) {
		if (!ShoulderCamera.shoulderCameraInitalized || CharacterMainControlCommon.syncWithCameraDirection) {
			return true;
		}
		if (__instance != CharacterMainControl.Main) {
			return true;
		}

		__result = __instance.CharacterAimTurnSpeed;
		return false;
	}
}

