using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Duckov.Utilities;
using Duckov.Options;
using Duckov.UI;
using Unity.VisualScripting;
using System.Reflection;
using UnityEngine.UI;
using ShoulderSurfing;

public class HealthBarCommon {
	public static Dictionary<HealthBar, float> LastTimeToHurt = new Dictionary<HealthBar, float>();
	public static HashSet<HealthBar> AffectedInstances = new HashSet<HealthBar>();
}

[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("UpdatePosition")]
public static class HealthBarUpdatePositionExtender {
	const float HealthBarFadeDelay = 3f;

	static FieldInfo fillField;
	static FieldInfo followFillField;
	static FieldInfo deathIndicatorField;
	static FieldInfo backgroundField;

	public static void Postfix(HealthBar __instance) {
		if (fillField == null) {
			fillField = typeof(HealthBar).GetField("fill", BindingFlags.NonPublic | BindingFlags.Instance);
			followFillField = typeof(HealthBar).GetField("followFill", BindingFlags.NonPublic | BindingFlags.Instance);
			deathIndicatorField = typeof(HealthBar).GetField("background", BindingFlags.NonPublic | BindingFlags.Instance);
			backgroundField = typeof(HealthBar).GetField("deathIndicator", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		// Disable mod in origin game view
		if (ShoulderCamera.shoulderCameraToggled == false) {
			if (HealthBarCommon.AffectedInstances.Count > 0) {
				foreach (HealthBar instance in HealthBarCommon.AffectedInstances) {
					if (instance == null) {
						continue;
					}

					GameObject deathIndicator = (GameObject)deathIndicatorField.GetValue(instance);
					GameObject background = (GameObject)backgroundField.GetValue(instance);
					Image fillImage = (Image)fillField.GetValue(instance);
					Image followFillImage = (Image)followFillField.GetValue(instance);
					deathIndicator.SetActive(true);
					background.SetActive(true);
					fillImage.enabled = true;
					followFillImage.enabled = true;
				}
				HealthBarCommon.AffectedInstances.Clear();
				HealthBarCommon.LastTimeToHurt.Clear();
			}
			return;
		}

		float lastHurtTime = 0f;
		if (HealthBarCommon.LastTimeToHurt.TryGetValue(__instance, out lastHurtTime)) {
			if (Time.time - lastHurtTime > HealthBarFadeDelay) {
				HealthBarCommon.LastTimeToHurt.Remove(__instance);
			}
		}
		bool showHealthBar = __instance.target.Hidden == false
			&& __instance.transform.position.z > 0f
			&& Time.time - lastHurtTime < HealthBarFadeDelay;

		// Reduce update frequency
		GameObject death = (GameObject)deathIndicatorField.GetValue(__instance);
		if (showHealthBar != death.activeSelf) {
			Image fillImage = (Image)fillField.GetValue(__instance);
			Image followFillImage = (Image)followFillField.GetValue(__instance);
			GameObject background = (GameObject)backgroundField.GetValue(__instance);

			fillImage.enabled = showHealthBar;
			followFillImage.enabled = showHealthBar;
			death.SetActive(showHealthBar);
			background.SetActive(showHealthBar);

			if (!showHealthBar) {
				HealthBarCommon.AffectedInstances.Add(__instance);
			}
		}
	}
}


[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("OnTargetDead")]
public static class HealthBarOnTargetDeadExtender {
	public static void Postfix(HealthBar __instance) {
		if (ShoulderCamera.shoulderCameraToggled == false) {
			return;
		}
		HealthBarCommon.LastTimeToHurt.Remove(__instance);
	}
}


[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("OnTargetHurt")]
public static class HealthBarOnTargetHurtExtender {
	public static void Postfix(HealthBar __instance) {
		if (ShoulderCamera.shoulderCameraToggled == false) {
			return;
		}
		HealthBarCommon.LastTimeToHurt[__instance] = Time.time;
	}
}
