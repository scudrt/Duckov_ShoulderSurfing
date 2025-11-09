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
using UnityEngine.UI;

public static class MiniMapCommon
{
	public static bool isMapRotateWithCamera = false;
	public static GameObject playerArrow;
	public static float mapIndicatorAlpha = 0.5f;

	public static Vector3 GetPlayerMinimapLocalPosition()
	{
		Vector3 vector;
		// MiniMapSettings.TryGetMinimapPosition(LevelManager.Instance.MainCharacter.transform.position, out vector);
		// return LevelManager.Instance.MainCharacter.transform.position - MiniMapSettings.Instance.CombinedCenter;
		// MiniMapView.TryConvertWorldToMinimapPosition(LevelManager.Instance.MainCharacter.transform.position + LevelManager.Instance.MainCharacter.transform.forward * 1f - LevelManager.Instance.MainCharacter.transform.right, out vector);
		MiniMapView.TryConvertWorldToMinimapPosition(LevelManager.Instance.MainCharacter.transform.position + LevelManager.Instance.MainCharacter.transform.forward * 1.08f - LevelManager.Instance.MainCharacter.transform.right * 0.7f, out vector);
		return vector;
		// return MiniMapSettings.Instance.CombinedCenter;
		// return vector;
	}

	public static Vector3 GetPlayerMinimapGlobalPosition(Transform minimapDisplayTrans)
	{
		Vector3 vector;
		MiniMapView.TryConvertWorldToMinimapPosition(LevelManager.Instance.MainCharacter.transform.position, out vector);
		return minimapDisplayTrans.localToWorldMatrix.MultiplyPoint(vector) + new Vector3(0, 28, 0);
	}

	public static Quaternion GetPlayerMinimapRotation()
	{
		// Vector3 to = LevelManager.Instance.GameCamera.renderCamera.transform.up.ProjectOntoPlane(Vector3.up);
		Vector3 to = LevelManager.Instance.MainCharacter.CurrentAimDirection.ProjectOntoPlane(Vector3.up);
		float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
		return Quaternion.Euler(0f, 0f, -currentMapZRotation);
	}

	public static Quaternion GetCameraRotation()
    {
		// Vector3 to = LevelManager.Instance.GameCamera.renderCamera.transform.up.ProjectOntoPlane(Vector3.up);
		Vector3 to = LevelManager.Instance.MainCharacter.CurrentAimDirection.ProjectOntoPlane(Vector3.up);
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
			__instance.transform.localRotation = MiniMapCommon.GetCameraRotation();
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
	public static GameObject currentDisplay;
	public static Image arrowCache;
	public static Image rangeCache;
	public static void Postfix(MiniMapDisplay __instance)
	{
		if(__instance == CustomMinimapManager.Instance.DuplicatedMinimapDisplay)
		{
			return;
		}
		currentDisplay = __instance.gameObject;
		Vector3 pos = __instance.GetComponent<RectTransform>().position;
		if (MiniMapCommon.playerArrow != null)
		{
			// var image = MiniMapCommon.playerArrow.GetComponent<Image>();
			// image.GetComponent<RectTransform>().sizeDelta = new Vector2(0.001f, 0.001f) * image.sprite.texture.width * MiniMapSettings.Instance.PixelSize;
			// MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
			MiniMapCommon.playerArrow.transform.position = MiniMapCommon.GetPlayerMinimapGlobalPosition(currentDisplay.transform);
			// MiniMapCommon.playerArrow.transform.localScale = Vector3.one * 0.7f / __instance.transform.localScale.x;
			MiniMapCommon.playerArrow.transform.SetAsLastSibling();
			MiniMapCommon.playerArrow.transform.localRotation = MiniMapCommon.GetPlayerMinimapRotation();
			arrowCache.color = new Color(1, 1, 1, MiniMapCommon.mapIndicatorAlpha);
			rangeCache.color = new Color(1, 1, 1, 0.5f * MiniMapCommon.mapIndicatorAlpha);

			return;
		}
		// 创建新的GameObject并添加Image组件
		GameObject arrowObject = new GameObject("PlayerImage");
		MiniMapCommon.playerArrow = arrowObject;
		Image arrowImage = arrowObject.AddComponent<Image>();
		arrowCache = arrowImage;
		arrowImage.color = new Color(1, 1, 1, MiniMapCommon.mapIndicatorAlpha);

		GameObject viewRangeGameobject = new GameObject("ViewRangeImage");
		Image rangeImage = viewRangeGameobject.AddComponent<Image>();
		rangeCache = rangeImage;

		arrowImage.sprite = Util.LoadSprite("player.png");
		rangeImage.sprite = Util.LoadSprite("range.png");

		// 设置父对象为MiniMapDisplay实例
		viewRangeGameobject.transform.SetParent(arrowObject.transform, false);
		viewRangeGameobject.transform.localPosition = new Vector3(0, 32, 0);
		rangeImage.rectTransform.pivot = new Vector2(0.5f, 0f);
		rangeImage.color = new Color(1, 1, 1, 0.5f * MiniMapCommon.mapIndicatorAlpha);
		viewRangeGameobject.transform.localScale = Vector3.one * 2f;

		arrowObject.transform.SetParent(__instance.transform, false);
		arrowObject.transform.SetAsLastSibling();
		// MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
		MiniMapCommon.playerArrow.transform.position = MiniMapCommon.GetPlayerMinimapGlobalPosition(currentDisplay.transform);
		MiniMapCommon.playerArrow.transform.localScale = Vector3.one * 0.7f / __instance.transform.localScale.x;
		RectTransform rectTransform = arrowImage.GetComponent<RectTransform>();
		if (rectTransform != null)
		{
			rectTransform.sizeDelta = new Vector2(64, 64);
			// rectTransform.sizeDelta = new Vector2(0.001f, 0.001f) * arrowImage.sprite.texture.width * MiniMapSettings.Instance.PixelSize;
			// 确保锚点和轴心点设置正确
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
	}

}

[HarmonyPatch(typeof(MiniMapView))]
[HarmonyPatch("OnSetZoom")]
public static class MiniMapViewOnSetZoomExtender {
	public static void Postfix(MiniMapView __instance, float scale)
	{
		if (MiniMapCommon.playerArrow != null)
		{
			MiniMapCommon.playerArrow.transform.localScale = Vector3.one * 0.7f / MiniMapDisplaySetupExtender.currentDisplay.transform.localScale.x;
			MiniMapCommon.playerArrow.transform.position = MiniMapCommon.GetPlayerMinimapGlobalPosition(MiniMapDisplaySetupExtender.currentDisplay.transform);
			// MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
		}
	}

}