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
	static bool initialized = false;
	static Vector2 CenterOfScreen;

	static bool isCurrentlyInputActive = false;
	static int aimCheckLayerMask;
	static FieldInfo inputAimPointField;
	static FieldInfo _aimMousePosCacheField;

	// Handling recoil
	static bool needResetRecoil = true;
	static float verticalRecoil = 0.0f;
	static Vector2 recoilOffsetAccum = Vector2.zero;
	static Vector2 targetMousePos = Vector2.zero;
	static float maxVerticalRecoil;
	static float RandomGlitchRange;

	public static bool Prefix(InputManager __instance, ref Vector2 mouseDelta) {
		if (!initialized) {
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
			_aimMousePosCacheField = InputManagerExtenderCommon.getField("_aimMousePosCache");

			initialized = true;
		}
		if (!ShoulderCamera.shoulderCameraInitalized) {
			return true;
		}

		isCurrentlyInputActive = Application.isFocused && InputManager.InputActived && CharacterInputControl.Instance;
		if (!isCurrentlyInputActive) {
			needResetRecoil = true;
			verticalRecoil = 0f;
			return true;
		}
		if (needResetRecoil && isCurrentlyInputActive) {
			needResetRecoil = false;
			verticalRecoil = 0f;
			recoilOffsetAccum = Vector2.zero;
		}

		CenterOfScreen.x = Screen.width  * 0.5f;
		CenterOfScreen.y = Screen.height * 0.5f;
		maxVerticalRecoil = Screen.height * 0.75f;
		RandomGlitchRange = Screen.height * 0.05f;

		// Make cursor always be on the center of screen
		targetMousePos = CenterOfScreen + Vector2.up * verticalRecoil;
		_aimMousePosCacheField.SetValue(__instance, targetMousePos);
		mouseDelta = Vector2.zero;
		/* origin code
		mouseDelta = (targetMousePos - LevelManager.Instance.InputManager.AimScreenPoint)
			* 10f / OptionsManager.MouseSensitivity;
		*/
		return true;
	}

	public static void Postfix(InputManager __instance) {
		if (!ShoulderCamera.shoulderCameraInitalized) {
			return;
		}
		if (!isCurrentlyInputActive) {
			return;
		}

		Vector2 offset = __instance.AimScreenPoint - targetMousePos;
		recoilOffsetAccum += offset;
		recoilOffsetAccum = Vector2.Max(Vector2.zero, recoilOffsetAccum);
		float sqrLenOfOffset = recoilOffsetAccum.sqrMagnitude;
		if (sqrLenOfOffset <= 1f) {
			recoilOffsetAccum = Vector2.zero;
			verticalRecoil = 0f;
		} else {
			// Limit the max vertical recoil offset
			verticalRecoil = Mathf.Min(maxVerticalRecoil, Mathf.Sqrt(sqrLenOfOffset));
			// random glitch
			verticalRecoil += UnityEngine.Random.Range(-RandomGlitchRange, RandomGlitchRange);
			recoilOffsetAccum = recoilOffsetAccum.normalized * verticalRecoil;
		}

		RaycastHit hitinfo;
		Ray ray = LevelManager.Instance.GameCamera.renderCamera.ScreenPointToRay(CenterOfScreen + Vector2.up * verticalRecoil);
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
