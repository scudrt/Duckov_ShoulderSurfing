using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Duckov.Utilities;
using Duckov.Options;
using System.Reflection;
using Unity.Properties;
using Unity.VisualScripting;
using System.ComponentModel;
using UnityEngine.UIElements;
using ShoulderSurfing;

public static class InputManagerExtenderCommon {
	public static FieldInfo getField(string fieldName) {
		return typeof(InputManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
	}

	public static MethodInfo getMethod(string methodName) {
		return typeof(InputManager).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
	}
}

[HarmonyPatch(typeof(InputManager))]
[HarmonyPatch("SetAimInputUsingMouse")]
public static class InputManagerExtender {
	static int aimCheckLayerMask;
	static FieldInfo inputAimPointField;

	static bool initialized = false;
	static Vector2 CenterOfScreen;

	public static bool Prefix(ref Vector2 mouseDelta) {
		if (!initialized) {
			CenterOfScreen.x = Screen.width * 0.5f;
			CenterOfScreen.y = Screen.height * 0.5f;

			aimCheckLayerMask = (1 << LayerMask.NameToLayer("FowBlock"))
				| (1 << LayerMask.NameToLayer("Ground"))
				| (1 << LayerMask.NameToLayer("Wall_FowBlock"))
				| (1 << LayerMask.NameToLayer("Door"))
				| (1 << LayerMask.NameToLayer("HeadCollider"))
				// | (1 << LayerMask.NameToLayer("HalfObsticle"))

				// | (1 << LayerMask.NameToLayer("VisualOcclusion"))
				| (1 << LayerMask.NameToLayer("Character"))
				| (1 << LayerMask.NameToLayer("DamageReceiver"));

			inputAimPointField = InputManagerExtenderCommon.getField("inputAimPoint");

			initialized = true;
		}
		if (!ShoulderCamera.shoulderCameraToggled || CameraMode.Active) {
			return true;
		}

		// Make cursor always be on the center of screen
		mouseDelta = (CenterOfScreen - LevelManager.Instance.InputManager.AimScreenPoint) * 10f / OptionsManager.MouseSensitivity;
		return true;
	}

	public static void Postfix(InputManager __instance) {
		if (!ShoulderCamera.shoulderCameraToggled || CameraMode.Active) {
			return;
		}

		RaycastHit hitinfo;
		Ray ray = LevelManager.Instance.GameCamera.renderCamera.ScreenPointToRay(CenterOfScreen);
		ItemAgent_Gun gun = __instance.characterMainControl.GetGun();

		// Hit position of aimming
		Vector3 hitpos;

		// Use ground hit position as default
		Plane plane = new Plane(Vector3.up, Vector3.up * __instance.characterMainControl.transform.position.y);
		float d = 0f;
		plane.Raycast(ray, out d);
		if (d > 0f) {
			hitpos = ray.GetPoint(d);
		} else {
			hitpos = ray.GetPoint(50f);
		}

		ray.origin += ray.direction * 5f;
		// Calculate hit pos for other damagable items
		if (gun && Physics.Raycast(ray, out hitinfo, gun.BulletDistance * 2f, aimCheckLayerMask)) {
			hitpos = hitinfo.point;
		}

		inputAimPointField.SetValue(__instance, hitpos);
		__instance.characterMainControl.SetAimPoint(hitpos);
	}
}
