using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using UnityEngine.Rendering;
using Cinemachine.Utility;
using UnityEngine.InputSystem;
using Duckov.Buildings.UI;

namespace ShoulderSurfing {
	public class ShoulderCamera: MonoBehaviour {
		// Global switch indicating whether we're in shoulder surfing view
		public static bool shoulderCameraToggled = false;
		public static bool shoulderCameraInitalized = false;

		public static bool isMiniGameEnabled = false;
		private static float fov = 75f;

		private static bool _isLeftShoulder = false;
		public static bool IsLeftShoulder
		{
			get => _isLeftShoulder;
			set
			{
				_isLeftShoulder = value;
				// if (value) {
				// 	// 左肩机位
				// 	originDefaultFOV = hookCamera.defaultFOV;
				// 	originAdsFOV = hookCamera.adsFOV;
				// 	hookCamera.defaultFOV = 75f;
				// 	hookCamera.adsFOV = 45f;
				// } else {
				// 	// 右肩机位
				// 	hookCamera.defaultFOV = originDefaultFOV;
				// 	hookCamera.adsFOV = originAdsFOV;
				// }
			}
		}
		

		public static void EnableTPSMode(GameObject parentMod) {
			parentMod.AddComponent<ShoulderCamera>();
			// OcclusionFadeManager.FadeEnabled = true;
		}

		public static void DisableTPSMode(GameObject parentMod) {
			ShoulderCamera me;
			if (parentMod.TryGetComponent<ShoulderCamera>(out me)) {
				shoulderCameraToggled = false;
				me.OnShoulderCameraDisable();
				Destroy(me);
			}
		}

		public void RehookCamera() {
			if (hookCamera != null && mainCamera != null) {
				return;
			}

			this.hookCamera = GameCamera.Instance;
			if (hookCamera == null) {
				return;
			}

			if (shoulderCameraToggled) {
				// The camera should be reinitialized after loading maps in shoulder mode
				shoulderCameraInitalized = false;
				OnShoulderCameraEnable();
			}
		}

		public void OnShoulderCameraEnable()
		{
			if (shoulderCameraInitalized)
			{
				return;
			}
			if (hookCamera == null || hookCamera.renderCamera == null)
			{
				return;
			}

			originDefaultFOV = hookCamera.defaultFOV;
			originAdsFOV = hookCamera.adsFOV;
			// Initialize game camera FOV
			hookCamera.defaultFOV = 75f;
			hookCamera.adsFOV = 45f;

			mainCamera = hookCamera.renderCamera; // hookCamera.mainVCam;

			if (hookCamera.mianCameraArm != null)
			{
				hookCamera.mianCameraArm.enabled = false;
			}
			if (hookCamera.brain != null)
			{
				hookCamera.brain.enabled = false;
			}
			if (hookCamera.mainVCam != null)
			{
				hookCamera.mainVCam.enabled = false;
			}

			originFarClip = mainCamera.farClipPlane; // original: 300f
			mainCamera.farClipPlane = 80f;

			originDOF2Active.Clear();
			originMotionBlur2Active.Clear();
			// Removing all depth of field effects in the game
			foreach (Volume volume in FindObjectsOfType<Volume>(true))
			{
				if (volume && volume.profile)
				{
					DepthOfField depthOfField = null;
					MotionBlur motionBlur = null;
					volume.profile.TryGet<DepthOfField>(out depthOfField);
					volume.profile.TryGet<MotionBlur>(out motionBlur);
					if (depthOfField != null)
					{
						originDOF2Active[depthOfField] = depthOfField.active;
						depthOfField.active = false;
					}
					if (motionBlur != null)
					{
						originMotionBlur2Active[motionBlur] = motionBlur.active;
						motionBlur.active = false;
					}
				}
			}

			Debug.Log("Shoulder Camera initialized");

			shoulderCameraInitalized = true;
			// SetOcclusionFadeStatus(false);
		}


		// 暂时没有用
		public void SetCullingMode(CullMode mode)
		{
			// 更新所有材质
			Renderer[] renderers = FindObjectsOfType<Renderer>();
			foreach (Renderer renderer in renderers)
			{
				Material[] materials = renderer.materials;
				foreach (Material material in materials)
				{
					if (material.HasProperty("_Cull"))
					{
						material.SetInt("_Cull", (int)mode);
					}
				}
			}

			Debug.Log($"剔除模式已设置为: {mode}");
		}

		void SetOcclusionFadeStatus(bool enabled)
        {
			OcclusionFadeManager.FadeEnabled = enabled;
			OcclusionFadeManager.Instance.aimOcclusionFadeChecker.gameObject.SetActive(true);
			OcclusionFadeManager.Instance.characterOcclusionFadeChecker.gameObject.SetActive(true);

			// if (!enabled)
			// {
			// 	SetCullingMode(CullMode.Off);
			// }
            // else
            // {
			// 	SetCullingMode(CullMode.Back);
            // }
        }
		
		void OnShoulderCameraDisable(bool switchToCameraMode = false) {
			if (!shoulderCameraInitalized) {
				return;
			}
			if (hookCamera == null || mainCamera == null) {
				return;
			}

			hookCamera.defaultFOV = originDefaultFOV;
			hookCamera.adsFOV = originAdsFOV;

			mainCamera.farClipPlane = originFarClip;

			if (!switchToCameraMode) {
				if (hookCamera.mainVCam != null) {
					hookCamera.mainVCam.enabled = true;
				}
				if (hookCamera.mianCameraArm != null) {
					hookCamera.mianCameraArm.enabled = true;
				}
			}

			if (hookCamera.brain != null) {
				hookCamera.brain.enabled = true;
			}

			// Recover all DepthOfField and MotionBlur components
			foreach (KeyValuePair<DepthOfField, bool> pair in originDOF2Active) {
				pair.Key.active = pair.Value;
			}
			foreach (KeyValuePair<MotionBlur, bool> pair in originMotionBlur2Active) {
				pair.Key.active = pair.Value;
			}
			originDOF2Active.Clear();
			originMotionBlur2Active.Clear();

			Debug.Log("Shoulder Camera deinitialized");

			shoulderCameraInitalized = false;
			// SetOcclusionFadeStatus(true);
		}


		void UpdateCollidedCameraPosition() {
			// Update camera position by follow target
			Vector3 cameraForward = mainCamera.transform.forward;
			Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
			Vector3 anchorPos = target.transform.position + Vector3.up * 0.5f;

			// v1.0: (1, 1.75, -2.8)
			Vector3 shoulderCameraOffset = cameraRight * 1f + Vector3.up * 1.25f + cameraForward * -2.8f;
			Vector3 shoulderCameraDir = shoulderCameraOffset.normalized;
			float shoulderCameraDistance = shoulderCameraOffset.magnitude;
			Ray shoulderCameraRay = new Ray(anchorPos, shoulderCameraDir);
			RaycastHit hitinfo;
			if (Physics.SphereCast(shoulderCameraRay, 0.1f, out hitinfo, shoulderCameraDistance, cameraCollisionLayerMask)) {
				mainCamera.transform.position = anchorPos + hitinfo.distance * shoulderCameraDir;
			} else {
				// No colliding
				mainCamera.transform.position = anchorPos + shoulderCameraOffset;
			}
		}

		void Start() {
			cameraYaw = 0f;
			cameraPitch = 0f;

			cameraCollisionLayerMask = (1 << LayerMask.NameToLayer("FowBlock"))
				| (1 << LayerMask.NameToLayer("Ground"))
				| (1 << LayerMask.NameToLayer("Wall_FowBlock"));

			mouseDeltaField = typeof(CharacterInputControl).GetField("mouseDelta", BindingFlags.NonPublic | BindingFlags.Instance);

			shoulderCameraToggled = true;
			shoulderCameraInitalized = false;
		}

		void Update() {
			if (LevelManager.Instance == null) {
				return;
			}

			// Check if there are any events interupting shoulder camera view
			bool otherModeInterupted = CameraMode.Active || (BuilderView.Instance && BuilderView.Instance.open) || isMiniGameEnabled;
			if (otherModeInterupted) {
				if (shoulderCameraInitalized) {
					OnShoulderCameraDisable(CameraMode.Active);
				}
				return;
			}

			RehookCamera();

			// View switch
			if (Keyboard.current.f7Key.wasPressedThisFrame
				|| (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.commaKey.wasPressedThisFrame)) {
				shoulderCameraToggled = !shoulderCameraToggled;
			}

			if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.slashKey.wasPressedThisFrame) {
				MiniMapCommon.isMapRotateWithCamera = !MiniMapCommon.isMapRotateWithCamera;
			}
			// Temporary recoil switch
			if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.periodKey.wasPressedThisFrame) {
				InputManagerExtenderCommon.ShoulderRecoilMultiplier = InputManagerExtenderCommon.ShoulderRecoilMultiplier <= 0.0f ? 0.36f : 0.0f;
			}

			if (shoulderCameraToggled && !shoulderCameraInitalized) {
				OnShoulderCameraEnable();
			} else if (!shoulderCameraToggled && shoulderCameraInitalized) {
				OnShoulderCameraDisable();
			}

			if (target == null) {
				target = CharacterMainControl.Main;
			}
			if (inputManager == null) {
				if (LevelManager.Instance != null) {
					inputManager = LevelManager.Instance.InputManager;
				}
			}
		}

		void LateUpdate() {
			if (hookCamera == null || target == null || mainCamera == null) {
				return; // Come in next frame :)
			}
			if (!shoulderCameraToggled || !shoulderCameraInitalized) {
				return;
			}

			// Update camera rotation by player input
			if (this.inputManager) {
				if (InputManager.InputActived && CharacterInputControl.Instance) { // No camera rotation while the game is paused or the inventory is open
					// Update mouse delta to the rotation
					Vector2 currentMouseDelta = (Vector2)mouseDeltaField.GetValue(CharacterInputControl.Instance);

					// Shoulder surfing is more sensitive than the origin
					currentMouseDelta *= global::Duckov.Options.OptionsManager.MouseSensitivity * 0.01f;
					cameraYaw += currentMouseDelta.x;
					cameraPitch = Mathf.Clamp(cameraPitch + currentMouseDelta.y, -70f, 70f);
				}
			}
			// Camera shaked by recoil
			float cameraShakePitchThisFrame = InputManagerExtender.cameraShakePixels * global::Duckov.Options.OptionsManager.MouseSensitivity * 0.01f;
			cameraPitch = Mathf.Clamp(cameraPitch + cameraShakePitchThisFrame, -70f, 70f);

			mainCamera.fieldOfView = hookCamera.mainVCam.m_Lens.FieldOfView;

			mainCamera.transform.rotation = Quaternion.Euler(-cameraPitch, cameraYaw, 0f);

			UpdateCollidedCameraPosition();

			global::System.Action<global::GameCamera, global::CharacterMainControl> onCameraPosUpdate = global::GameCamera.OnCameraPosUpdate;
			if (onCameraPosUpdate != null) {
				onCameraPosUpdate(hookCamera, this.target);
			}
		}

		CharacterMainControl target;
		InputManager inputManager;

		GameCamera hookCamera;
		Camera mainCamera;
		// CinemachineVirtualCamera mainCamera;

		FieldInfo mouseDeltaField;

		int cameraCollisionLayerMask;

		private float cameraPitch = 0f;
		private float cameraYaw = 0f;

		/*---------- Origin paramters of camera ----------*/
		float originDefaultFOV = 0f;
		float originAdsFOV = 0f;
		float originFarClip = 0f;
		Dictionary<DepthOfField, bool> originDOF2Active = new Dictionary<DepthOfField, bool>();
		Dictionary<MotionBlur, bool> originMotionBlur2Active = new Dictionary<MotionBlur, bool>();
	}
}
