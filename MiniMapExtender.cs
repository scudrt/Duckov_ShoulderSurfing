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

public static class MiniMapCommon {
	public static bool isMapRotateWithCamera = false;
	public static GameObject playerArrow;
	public static Vector3 GetPlayerMinimapPosition()
	{
		Vector3 vector;
		// MiniMapSettings.TryGetMinimapPosition(LevelManager.Instance.MainCharacter.transform.position, out vector);
		// return LevelManager.Instance.MainCharacter.transform.position - MiniMapSettings.Instance.CombinedCenter;
		MiniMapView.TryConvertWorldToMinimapPosition(LevelManager.Instance.MainCharacter.transform.position, out vector);
		return vector;
		// return MiniMapSettings.Instance.CombinedCenter;
		// return vector;
	}
	
	public static Quaternion GetPlayerMinimapRotation()
    {
		// Vector3 to = LevelManager.Instance.GameCamera.renderCamera.transform.up.ProjectOntoPlane(Vector3.up);
		Vector3 to = LevelManager.Instance.MainCharacter.CurrentAimDirection.ProjectOntoPlane(Vector3.up);
		float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
		return Quaternion.Euler(0f, 0f, -currentMapZRotation);
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
			__instance.transform.localRotation = MiniMapCommon.GetPlayerMinimapRotation();
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
	public static void Postfix(MiniMapDisplay __instance)
	{

		Vector3 pos = __instance.GetComponent<RectTransform>().position;
		if (MiniMapCommon.playerArrow != null)
		{
			// var image = MiniMapCommon.playerArrow.GetComponent<Image>();
			// image.GetComponent<RectTransform>().sizeDelta = new Vector2(0.001f, 0.001f) * image.sprite.texture.width * MiniMapSettings.Instance.PixelSize;
			MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
			MiniMapCommon.playerArrow.transform.SetAsLastSibling();
			MiniMapCommon.playerArrow.transform.rotation = MiniMapCommon.GetPlayerMinimapRotation();
			return;
		}
		// 创建新的GameObject并添加Image组件
		GameObject arrowObject = new GameObject("ArrowImage");
		MiniMapCommon.playerArrow = arrowObject;
		Image arrowImage = arrowObject.AddComponent<Image>();

		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		string path = Path.Combine(directoryName, "textures");
		string text = Path.Combine(path, "arrow_1.png");
		bool flag2 = File.Exists(text);
		if (flag2)
		{
			byte[] data = File.ReadAllBytes(text);
			Texture2D texture2D = new Texture2D(2, 2);
			bool flag3 = texture2D.LoadImage(data);
			if (flag3)
			{
				arrowImage.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f);
			}
		}
		// 设置父对象为MiniMapDisplay实例
		arrowObject.transform.SetParent(__instance.transform, false);
		arrowObject.transform.SetAsLastSibling();
		MiniMapCommon.playerArrow.transform.localPosition = MiniMapCommon.GetPlayerMinimapPosition();
		// arrowObject.transform.position = MiniMapCommon.GetPlayerMinimapPosition();
		RectTransform rectTransform = arrowImage.GetComponent<RectTransform>();
		if (rectTransform != null)
		{
			rectTransform.sizeDelta = new Vector2(5, 5);
			// rectTransform.sizeDelta = new Vector2(0.001f, 0.001f) * arrowImage.sprite.texture.width * MiniMapSettings.Instance.PixelSize;
			// 确保锚点和轴心点设置正确
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0f);
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
			var scaleInvert = 1f / scale;
			scaleInvert = Mathf.Clamp(scaleInvert, 1f, 2f);
			MiniMapCommon.playerArrow.transform.localScale = Vector3.one * scaleInvert;
		}
	}

}