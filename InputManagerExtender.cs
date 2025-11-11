using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Duckov.Options;
using System.Reflection;
using ShoulderSurfing;

public static class InputManagerExtenderCommon {
	public const float DefaultShoulderRecoilMultiplier = 0.36f;
	public static float ShoulderRecoilMultiplier = DefaultShoulderRecoilMultiplier;
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
	public static float totalShakePixels = 0f;
	public static float cameraShakePixels = 0f;

	static bool initialized = false;
	static Vector2 CenterOfScreen;

	static bool isCurrentlyInputActive = false;
	static int aimCheckLayerMask;
	static FieldInfo inputAimPointField;
	static FieldInfo _aimMousePosCacheField;

	// Handling recoil
	static bool needResetRecoil = true;
	static Vector2 targetMousePos = Vector2.zero;

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

		cameraShakePixels = 0f;

		isCurrentlyInputActive = Application.isFocused && InputManager.InputActived && CharacterInputControl.Instance;
		if (!isCurrentlyInputActive) {
			return true;
		}

		CenterOfScreen.x = Screen.width  * 0.5f;
		CenterOfScreen.y = Screen.height * 0.5f;

		bool playerTryControlRecoil = mouseDelta.y < 0f;
		// Make cursor always be on the center of screen
		targetMousePos = CenterOfScreen;

		if (playerTryControlRecoil) {
			// Apply player's recoil control value onto the camera pitch
			totalShakePixels = 0f;

			// Set mouse delta not zero to refresh recoil recover position
			mouseDelta = (targetMousePos - LevelManager.Instance.InputManager.AimScreenPoint)
				* 10f / OptionsManager.MouseSensitivity;
		} else {
			// Set mouse delta zero to keep recoil recover position
			_aimMousePosCacheField.SetValue(__instance, targetMousePos);
			mouseDelta = Vector2.zero;
		}
		return true;
	}

	public static void Postfix(InputManager __instance) {
		if (!isCurrentlyInputActive) {
			return;
		}
		if (!ShoulderCamera.shoulderCameraInitalized) {
			return;
		}

		// Split recoil into camera shake and aim shake
		Vector2 recoilThisFrame = __instance.AimScreenPoint - targetMousePos;
		cameraShakePixels = recoilThisFrame.magnitude * MathF.Sign(recoilThisFrame.y) * InputManagerExtenderCommon.ShoulderRecoilMultiplier;
		totalShakePixels += cameraShakePixels;

		// Preventing recoil recover overhead
		if (totalShakePixels <= 0f) {
			cameraShakePixels = 0f;
			totalShakePixels = 0f;
		}

		// Aim after calculating recoil
		RaycastHit hitinfo;
		Ray ray = LevelManager.Instance.GameCamera.renderCamera.ScreenPointToRay(targetMousePos);
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

[HarmonyPatch(typeof(InputManager))]
[HarmonyPatch("ActiveInput")]
public static class InputManagerActiveInputExtender
{
	private static FieldInfo blockInputSourcesField;
	private static bool isFieldInfoInitialized = false;

	// Postfix 补丁，在原方法执行后运行
	public static void Postfix()
	{
		if (!CustomMinimapManager.isOpen)
			return;
		int count = GetBlockInputSourcesCount(LevelManager.Instance.InputManager);
		// Debug.Log("结束动作");
		if (count <= 0)
			CustomMinimapManager.TryShow();
	}

    private static void InitializeFieldInfo()
    {
        if (isFieldInfoInitialized) return;
        
        try
        {
			blockInputSourcesField = typeof(InputManager).GetField("blockInputSources", BindingFlags.NonPublic | BindingFlags.Instance);
			isFieldInfoInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing field info: {e.Message}");
        }
    }
    
    public static int GetBlockInputSourcesCount(InputManager inputManager)
    {
        if (inputManager == null) return 0;
        
        if (!isFieldInfoInitialized)
        {
            InitializeFieldInfo();
        }
        
        if (blockInputSourcesField == null) return 0;
        
        try
        {
            object blockInputSourcesValue = blockInputSourcesField.GetValue(inputManager);
            
            if (blockInputSourcesValue != null)
            {
                PropertyInfo countProperty = blockInputSourcesValue.GetType().GetProperty("Count");
                if (countProperty != null)
                {
                    return (int)countProperty.GetValue(blockInputSourcesValue);
                }
            }
            
            return 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting blockInputSources count: {e.Message}");
            return 0;
        }
    }
}
