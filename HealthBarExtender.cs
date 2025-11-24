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
using TMPro;
using LeTai.Asset.TranslucentImage;
using System.Xml;
using Unity.Collections;

public class HealthBarCommon {
	public static Dictionary<HealthBar, float> LastTimeToHurt = new Dictionary<HealthBar, float>();
	public static HashSet<HealthBar> AffectedInstances = new HashSet<HealthBar>();

	public static FieldInfo fillField;
	public static FieldInfo followFillField;
	public static FieldInfo deathIndicatorField;
	public static FieldInfo backgroundField;
	public static FieldInfo nameTextField;
	public static FieldInfo levelIconField;
	public static MethodInfo refreshIconMethod; // RefreshCharacterIcon

	public static void InitializeFields() {
		if (fillField != null) {
			return;
		}

		fillField = typeof(HealthBar).GetField("fill", BindingFlags.NonPublic | BindingFlags.Instance);
		followFillField = typeof(HealthBar).GetField("followFill", BindingFlags.NonPublic | BindingFlags.Instance);
		deathIndicatorField = typeof(HealthBar).GetField("background", BindingFlags.NonPublic | BindingFlags.Instance);
		backgroundField = typeof(HealthBar).GetField("deathIndicator", BindingFlags.NonPublic | BindingFlags.Instance);
		nameTextField = typeof(HealthBar).GetField("nameText", BindingFlags.NonPublic | BindingFlags.Instance);
		levelIconField = typeof(HealthBar).GetField("levelIcon", BindingFlags.NonPublic | BindingFlags.Instance);
		refreshIconMethod = typeof(HealthBar).GetMethod("RefreshCharacterIcon", BindingFlags.NonPublic | BindingFlags.Instance);
	}

	public static void ClearAffectedForInstance(HealthBar instance) {
		if (!instance) {
			return;
		}

		LastTimeToHurt.Remove(instance);

		GameObject deathIndicator = (GameObject)deathIndicatorField.GetValue(instance);
		GameObject background = (GameObject)backgroundField.GetValue(instance);
		Image followFillImage = (Image)followFillField.GetValue(instance);
		Image fillImage = (Image)fillField.GetValue(instance);
		deathIndicator.SetActive(true);
		background.SetActive(true);
		followFillImage.enabled = true;
		fillImage.enabled = true;
	}

	public static void ClearAllAffected() {
		if (HealthBarCommon.AffectedInstances.Count > 0) {
			foreach (HealthBar instance in HealthBarCommon.AffectedInstances) {
				ClearAffectedForInstance(instance);
			}
			HealthBarCommon.AffectedInstances.Clear();
			HealthBarCommon.LastTimeToHurt.Clear();
		}
	}
}

[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("UpdatePosition")]
public static class HealthBarUpdatePositionExtender {
	const float HealthBarFadeDelay = 3f;

	public static void Postfix(HealthBar __instance) {
		// Disable mod in origin game view
		if (ShoulderCamera.shoulderCameraInitalized == false) {
			HealthBarCommon.ClearAllAffected();
			return;
		}

		HealthBarCommon.InitializeFields();

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
		GameObject death = (GameObject)HealthBarCommon.deathIndicatorField.GetValue(__instance);
		if (showHealthBar != death.activeSelf) {
			Image fillImage = (Image)HealthBarCommon.fillField.GetValue(__instance);
			Image followFillImage = (Image)HealthBarCommon.followFillField.GetValue(__instance);
			GameObject background = (GameObject)HealthBarCommon.backgroundField.GetValue(__instance);
			// Handle name tag and level icon display
			Image levelIcon = (Image)HealthBarCommon.levelIconField.GetValue(__instance);
			TextMeshProUGUI nameText = (TextMeshProUGUI)HealthBarCommon.nameTextField.GetValue(__instance);

			if (!showHealthBar) {
				HealthBarCommon.AffectedInstances.Add(__instance);

				// Handle name tag and level icon show state
				levelIcon.gameObject.SetActive(false);
				nameText.gameObject.SetActive(false);
			} else {
				// Going to recover health bar state
				HealthBarCommon.refreshIconMethod.Invoke(__instance, new object[] { });
			}

			// Set health bar show state
			fillImage.enabled = showHealthBar;
			followFillImage.enabled = showHealthBar;
			death.SetActive(showHealthBar);
			background.SetActive(showHealthBar);
		}
	}
}


[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("OnTargetDead")]
public static class HealthBarOnTargetDeadExtender {
	public static void Postfix(HealthBar __instance) {
		if (ShoulderCamera.shoulderCameraInitalized == false) {
			return;
		}
		HealthBarCommon.ClearAffectedForInstance(__instance);

		// Clear references for this instance
		Image levelIcon = (Image)HealthBarCommon.levelIconField.GetValue(__instance);
		TextMeshProUGUI nameText = (TextMeshProUGUI)HealthBarCommon.nameTextField.GetValue(__instance);
	}
}


[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("OnTargetHurt")]
public static class HealthBarOnTargetHurtExtender {
	public static void Postfix(HealthBar __instance) {
		if (ShoulderCamera.shoulderCameraInitalized == false) {
			return;
		}
		HealthBarCommon.LastTimeToHurt[__instance] = Time.time;
	}
}

[HarmonyPatch(typeof(HealthBar))]
[HarmonyPatch("CheckInFrame")]
public static class HealthBarCheckInFrameExtender {
	public static bool Prefix(HealthBar __instance, ref bool __result) {
		if (ShoulderCamera.shoulderCameraInitalized == false) {
			return true;
		}

		__result = __instance.target != null;
		return false;
	}
}
