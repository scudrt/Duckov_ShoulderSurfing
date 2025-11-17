using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Duckov.Sounds;
using ECM2;
using HarmonyLib;
using ShoulderSurfing;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


[HarmonyPatch(typeof(SoundVisualization))]
[HarmonyPatch("RefreshEntryPosition")]
public static class SoundVisualizationExtender {
	const float displayOffset = 320f;
	static Vector2 soundLayoutOffset = new Vector2(0f, 400f);
	public static bool Prefix(SoundVisualization __instance, SoundDisplay e) {
		if (!ShoulderCamera.shoulderCameraInitalized || !GameCamera.Instance.renderCamera || !GameCamera.Instance.target) {
			return true;
		}
		// Override sound visualization for shoulder surfing view

		Transform camTransform = GameCamera.Instance.renderCamera.transform;
		Vector2 camForward = new Vector2(camTransform.forward.x, camTransform.forward.z);
		Vector2 camRight = new Vector2(camTransform.right.x, camTransform.right.z);
		Vector3 soundOffset = e.CurrentSount.pos - GameCamera.Instance.target.transform.position;
		Vector2 displayDir = new Vector2(soundOffset.x, soundOffset.z).normalized;
		displayDir = new Vector2(
			Vector2.Dot(displayDir, camRight),
			Vector2.Dot(displayDir, camForward)
		).normalized;

		/*
		RectTransform layoutCenter = __instance.transform as RectTransform;
		Vector3 pos = e.CurrentSount.pos;
		Vector3 screenPoint = GameCamera.Instance.renderCamera.WorldToScreenPoint(pos);
		bool soundAppearBehind = screenPoint.z < 0f; // Reverse direction for sound appears behind camera

		RectTransformUtility.ScreenPointToLocalPointInRectangle(layoutCenter, screenPoint, null, out var localPoint);
		Vector2 normalized = localPoint.normalized * (soundAppearBehind ? -1f : 1f);
		*/

		e.transform.localPosition = soundLayoutOffset + displayDir * displayOffset;
		e.transform.rotation = Quaternion.FromToRotation(Vector2.up, displayDir);

		return false;
	}
}
