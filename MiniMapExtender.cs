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

[HarmonyPatch(typeof(MiniMapCompass))]
[HarmonyPatch("SetupRotation")]
public static class MiniMapCompassExtender {
	public const float originMapZRotation = -30f; // I don't know whether it will change

	static FieldInfo arrowField;

	public static bool Prefix(MiniMapCompass __instance) {
        if (arrowField == null) {
			arrowField = typeof(MiniMapCompass).GetField("arrow");
        }
		Transform trans = (Transform)arrowField.GetValue(__instance);
		trans.localRotation = global::UnityEngine.Quaternion.Euler(0f, 0f, originMapZRotation);
		return false;
	}

}

[HarmonyPatch(typeof(MiniMapDisplay))]
[HarmonyPatch("SetupRotation")]
public static class MiniMapDisplayExtender {
	public static bool Prefix(MiniMapCompass __instance) {
		__instance.transform.localRotation = global::UnityEngine.Quaternion.Euler(0f, 0f, MiniMapCompassExtender.originMapZRotation);
		return false;
	}

}