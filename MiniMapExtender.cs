using Cinemachine.Utility;
using Duckov.MiniMaps;
using Duckov.MiniMaps.UI;
using Duckov.Options;
using Duckov.Scenes;
using Duckov.UI;
using HarmonyLib;
using ShoulderSurfing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MiniMapCommon
{
	public static bool isMapRotateWithCamera = false;
	public static bool isMinimapRotateWithCamera = true;
	public static float mapIndicatorAlpha = 0.5f;

	// public static bool IsOfficialSimplePOI(GameObject poiObject)
    // {
	// 	var name = poiObject.name;
    //     return name.StartsWith("MapElement") || name.StartsWith("POI_") || poiObject == LevelManager.Instance.MainCharacter.gameObject;
    // }

	public static Vector3 GetPlayerMinimapGlobalPosition(MiniMapDisplay minimapDisplay)
	{
		Vector3 vector;
		var sceneID = SceneInfoCollection.GetSceneID(SceneManager.GetActiveScene().buildIndex);
		minimapDisplay.TryConvertWorldToMinimap(LevelManager.Instance.MainCharacter.transform.position, sceneID, out vector);
		return minimapDisplay.transform.localToWorldMatrix.MultiplyPoint(vector);
	}

	public static Quaternion GetPlayerMinimapRotation()
	{
		Vector3 to = ShoulderCamera.CameraForward;
        float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
		return Quaternion.Euler(0f, 0f, -currentMapZRotation);
	}

	public static Quaternion GetPlayerMinimapRotationInverse()
	{
		Vector3 to = ShoulderCamera.CameraForward;
        float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
		return Quaternion.Euler(0f, 0f, currentMapZRotation);
	}

}

[HarmonyPatch(typeof(MiniMapCompass))]
[HarmonyPatch("SetupRotation")]
public static class MiniMapCompassExtender {

	public const float originMapZRotation = -30f; // I don't know whether it will change

	static FieldInfo arrowField;

	public static bool Prefix(MiniMapCompass __instance) {
        if (arrowField == null) {
			arrowField = typeof(MiniMapCompass).GetField("arrow", BindingFlags.NonPublic | BindingFlags.Instance);
			if (arrowField == null) {
				Debug.Log("[ShoulderSurfing] 无法获取指南针对象");
			}
		}

		Transform trans = (Transform)arrowField.GetValue(__instance);
		if (MiniMapCommon.isMapRotateWithCamera) {
			trans.localRotation = MiniMapCommon.GetPlayerMinimapRotation();
		} else {
			trans.localRotation = Quaternion.Euler(0f, 0f, originMapZRotation);
		}
		return false;
	}

}

[HarmonyPatch(typeof(MiniMapDisplay))]
[HarmonyPatch("SetupRotation")]
public static class MiniMapDisplayExtender
{
	public static bool Prefix(MiniMapDisplay __instance)
	{
		if (MiniMapCommon.isMapRotateWithCamera)
		{
			__instance.transform.rotation = MiniMapCommon.GetPlayerMinimapRotationInverse();
		}
		else
		{
			__instance.transform.localRotation = Quaternion.Euler(0f, 0f, MiniMapCompassExtender.originMapZRotation);
		}
		return false;
	}
}

[HarmonyPatch(typeof(MiniMapDisplay))]
[HarmonyPatch("Setup")]
public static class MiniMapDisplaySetupExtender
{
	// public static GameObject currentDisplay;
	// public static Image arrowCache;
	// public static Image rangeCache;
	public static void Postfix(MiniMapDisplay __instance)
	{
		PlayerArrow.CreateOrGetPlayerArrow(__instance);
	}

}

// [HarmonyPatch(typeof(MiniMapView))]
// [HarmonyPatch("OnSetZoom")]
// public static class MiniMapViewOnSetZoomExtender
// {
// 	public static void Postfix(MiniMapView __instance, float scale)
// 	{
// 		if (MiniMapCommon.playerArrow != null)
// 		{
// 			MiniMapCommon.playerArrow.transform.localScale = Vector3.one * 0.7f / MiniMapDisplaySetupExtender.currentDisplay.transform.localScale.x;
// 			MiniMapCommon.playerArrow.transform.position = MiniMapCommon.GetPlayerMinimapGlobalPosition(MiniMapDisplaySetupExtender.currentDisplay.transform);
// 			// MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
// 		}
// 	}

// }

[HarmonyPatch(typeof(MiniMapDisplay))]
[HarmonyPatch("HandlePointOfInterest")]
public static class MiniMapDisplayHandlePOIExtender
{
	// static FieldInfo targetField;
	public static bool Prefix(MiniMapDisplay __instance, MonoBehaviour poi)
	{
		if(__instance == CustomMinimapManager.Instance.DuplicatedMinimapDisplay)
		{
			// var poiName = poi.gameObject.name;
			if(poi is SimplePointOfInterest && View.ActiveView == MiniMapView.Instance)
            {
				// Debug.Log($"不初始化poi:{poi.gameObject.name} {poi} {poi.gameObject}");
				return false;
            }
			// Debug.Log($"初始化poi:{poi.gameObject.name} {poi} {poi.gameObject}");
			return true;
		}
		return true;
	}
}

// [HarmonyPatch(typeof(MiniMapDisplay))]
// [HarmonyPatch("ReleasePointOfInterest")]
// public static class MiniMapDisplayReleasePOIExtender
// {
// 	public static void Postfix(MiniMapDisplay __instance, MonoBehaviour poi)
// 	{
// 		poi.gameObject.SetActive(false);
// 	}
// }

// [HarmonyPatch(typeof(PointOfInterestEntry))]
// [HarmonyPatch("UpdatePosition")]
// public static class PointOfInterestEntryExtender {
// 	public static bool Prefix(PointOfInterestEntry __instance)
// 	{
// 		if (__instance.Target == null)
// 			return false;
// 		return true;
//     }
// }