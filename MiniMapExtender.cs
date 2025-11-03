using Cinemachine.Utility;
using Duckov.MiniMaps;
using Duckov.MiniMaps.UI;
using Duckov.Options;
using Duckov.UI;
using HarmonyLib;
using ShoulderSurfing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class MiniMapCommon {
	public static bool isMapRotateWithCamera = false;
}

[HarmonyPatch(typeof(MiniMapCompass))]
[HarmonyPatch("SetupRotation")]
public static class MiniMapCompassExtender {
	public const float originMapZRotation = -30f; // I don't know whether it will change

	static FieldInfo arrowField;

	public static bool Prefix(MiniMapCompass __instance) {
        if (arrowField == null) {
			arrowField = typeof(MiniMapCompass).GetField("arrow", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		Transform trans = (Transform)arrowField.GetValue(__instance);
		if (MiniMapCommon.isMapRotateWithCamera) {
			Vector3 to = LevelManager.Instance.GameCamera.renderCamera.transform.up.ProjectOntoPlane(Vector3.up);
			float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
			trans.localRotation = Quaternion.Euler(0f, 0f, currentMapZRotation);
		} else {
			trans.localRotation = Quaternion.Euler(0f, 0f, originMapZRotation);
		}

		return false;
	}

}

[HarmonyPatch(typeof(MiniMapDisplay))]
[HarmonyPatch("SetupRotation")]
public static class MiniMapDisplayExtender {
	public static bool Prefix(MiniMapDisplay __instance) {
		if (MiniMapCommon.isMapRotateWithCamera) {
			Vector3 to = LevelManager.Instance.GameCamera.renderCamera.transform.up.ProjectOntoPlane(Vector3.up);
			float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
			__instance.transform.localRotation = Quaternion.Euler(0f, 0f, currentMapZRotation);
		} else {
			__instance.transform.localRotation = Quaternion.Euler(0f, 0f, MiniMapCompassExtender.originMapZRotation);
		}

		return false;
	}

}