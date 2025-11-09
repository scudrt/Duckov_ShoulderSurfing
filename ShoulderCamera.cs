using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using UnityEngine.Rendering;
using Cinemachine.Utility;
using UnityEngine.InputSystem;
using Duckov.Buildings.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

namespace ShoulderSurfing {
	public class ShoulderCamera: MonoBehaviour {
		// Global switch indicating whether we're in shoulder surfing view
		public static bool shoulderCameraToggled = true;
		public static bool shoulderCameraInitalized = false;

		public static bool isMiniGameEnabled = false;
		public static Vector3 ShoulderCameraOffset = new Vector3(1f, 1.25f, -2.8f);

		public static int renderDistance {
			get {
				return __renderDistance;
			}
			set {
				__renderDistance = value;
				if (Instance) {
					Instance.RefreshRenderDistance();
				}
			}
		}
		private static int __renderDistance = 80;

		public static float FOV {
			get {
				return __FOV;
			}
			set {
				__FOV = value;
				if (Instance) {
					Instance.RefreshFOV();
				}
			}
		}
		private static float __FOV= 72f;
		public static float adsFOV = 40f;

		private static bool _isLeftShoulder = false;
		public static bool IsLeftShoulder
		{
			get => _isLeftShoulder;
			set
			{
				_isLeftShoulder = value;
			}
		}
		
		public static KeyCode switchShoulderCameraKey = KeyCode.F7;
		public static KeyCode shoulderSideKeySingle = KeyCode.LeftAlt;
		public static KeyCode shoulderLeftSideKey = KeyCode.Q;
		public static KeyCode shoulderRightSideKey = KeyCode.E;

		public static ShoulderCamera Instance {
			get {
				return __Instance;
			}
			set {
				__Instance = value;
			}
		}
		private static ShoulderCamera __Instance;

		public static void EnableTPSMode(GameObject parentMod) {
			__Instance = parentMod.AddComponent<ShoulderCamera>();
			// OcclusionFadeManager.FadeEnabled = true;
		}

		public static void DisableTPSMode(GameObject parentMod) {
			ShoulderCamera me;
			if (parentMod.TryGetComponent<ShoulderCamera>(out me)) {
				shoulderCameraToggled = false;
				me.OnShoulderCameraDisable();
				Destroy(me);
			}
			__Instance = null;
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

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			if (!shoulderCameraToggled) {
				return;
			}
			DisableAllDOF();
		}

		public void DisableCameraModeDOF() {
			if (!Camera.main) {
				return;
			}

			Transform CMVolumes = Camera.main.transform.Find("__CMVolumes");
			if (CMVolumes) {
				DepthOfField dof;
				var volume = CMVolumes.GetComponent<Volume>();
				if (volume && volume.profile && volume.profile.TryGet<DepthOfField>(out dof)) {
					if (dof) {
						dof.active = false;
					}
				}
			}
		}

		public void DisableAllDOF() {
			// Disable all dof for shoulder camera
			var volumes = FindObjectsOfType<Volume>();
			foreach (var volume in volumes) {
				if (!volume || !volume.profile) {
					continue;
				}

				DepthOfField dof = null;
				if (volume.profile.TryGet<DepthOfField>(out dof) && dof) {
					dof.active = false;
				}
			}
		}

		public void DisableAimOcclusionFade() {
			// Disable aim occlusion fade in shoulder camera mode
			OcclusionFadeManager fadeManager = OcclusionFadeManager.Instance;
			if (fadeManager && fadeManager.aimOcclusionFadeChecker) {
				fadeManager.aimOcclusionFadeChecker.transform.position = Vector3.one * 6666f;
				fadeManager.aimOcclusionFadeChecker.transform.forward = Vector3.up;
				fadeManager.aimOcclusionFadeChecker.gameObject.SetActive(false);
			}
			Shader.SetGlobalVector(aimPosHash, Vector3.one * 9999f);
			Shader.SetGlobalVector(aimViewDirHash, Vector3.up);
		}

		public void EnableAimOcclusionFade() {
			// Disable aim occlusion fade in shoulder camera mode
			if (OcclusionFadeManager.Instance && OcclusionFadeManager.Instance.aimOcclusionFadeChecker) {
				OcclusionFadeManager.Instance.aimOcclusionFadeChecker.gameObject.SetActive(true);
			}
		}

		public void RefreshFOV() {
			if (!shoulderCameraInitalized) {
				return;
			}
			if (!hookCamera) {
				return;
			}

			hookCamera.defaultFOV = ShoulderCamera.FOV;
		}

		public void RefreshRenderDistance() {
			if (!shoulderCameraInitalized) {
				return;
			}
			if (!hookCamera || !mainCamera) {
				return;
			}

			mainCamera.farClipPlane = renderDistance;
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
			hookCamera.defaultFOV = ShoulderCamera.FOV;
			hookCamera.adsFOV = ShoulderCamera.adsFOV;

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
			mainCamera.farClipPlane = ShoulderCamera.renderDistance;

			DisableAllDOF();
			DisableCameraModeDOF();
			SceneManager.sceneLoaded += OnSceneLoaded;
			CameraMode.OnCameraModeDeactivated += DisableCameraModeDOF;

			isInputActiveLastFrame = false;

			Debug.Log("Shoulder Camera initialized");

			shoulderCameraInitalized = true;
			// SetOcclusionFadeStatus(false);
		}

		public void OnShoulderCameraDisable(bool switchToCameraMode = false) {
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

			SceneManager.sceneLoaded -= OnSceneLoaded;
			CameraMode.OnCameraModeDeactivated -= DisableCameraModeDOF;

			isInputActiveLastFrame = false;

			Debug.Log("Shoulder Camera deinitialized");

			shoulderCameraInitalized = false;
			// SetOcclusionFadeStatus(true);
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
		
		void HandleShoulderSideSwitch() {
			// Updating shoulder side by player's input
			if (Input.GetKeyDown(shoulderSideKeySingle)) {
				IsLeftShoulder = !IsLeftShoulder;
			}
			if (Input.GetKeyDown(shoulderLeftSideKey)) {
				IsLeftShoulder = true;
			} else if (Input.GetKeyDown(shoulderRightSideKey)) {
				IsLeftShoulder = false;
			}
			targetShoulderCameraOffset.x = Math.Abs(targetShoulderCameraOffset.x) * (IsLeftShoulder ? -1f : 1f);
		}

		void UpdateCollidedCameraPosition() {
			// Update camera position by follow target
			Vector3 cameraForward = mainCamera.transform.forward;
			Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
			Vector3 anchorPos = target.transform.position + anchorOffset;

			// Updating camera position with smooth lerp
			// v1.0: (1, 1.75, -2.8)
			// v1.1: (1, 1.25, -2.8)
			Vector3 cameraMoveOffset = targetShoulderCameraOffset - currentShoulderCameraOffset;
			float camPosDeltaThisFrame = Time.deltaTime * cameraLerpSpeed;
			if (cameraMoveOffset.magnitude > camPosDeltaThisFrame) {
				currentShoulderCameraOffset += cameraMoveOffset.normalized * camPosDeltaThisFrame;
			} else {
				currentShoulderCameraOffset = targetShoulderCameraOffset;
			}
			Vector3 camOffset = cameraRight * currentShoulderCameraOffset.x
										+ Vector3.up * currentShoulderCameraOffset.y
										+ cameraForward * currentShoulderCameraOffset.z;
			Vector3 camDir = camOffset.normalized;
			float camDistance = camOffset.magnitude;

			// Handle camera collision
			Ray shoulderCameraRay = new Ray(anchorPos, camDir);
			RaycastHit hitinfo;
			if (Physics.SphereCast(shoulderCameraRay, 0.15f, out hitinfo, camDistance, cameraCollisionLayerMask)) {
				mainCamera.transform.position = anchorPos + hitinfo.distance * camDir;
			} else {
				// No colliding
				mainCamera.transform.position = anchorPos + camOffset;
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
					OnShoulderCameraDisable(true);
				}
				return;
			}

			RehookCamera();

			HandleShoulderSideSwitch();

			// View switch
			if (Keyboard.current.f7Key.wasPressedThisFrame
				|| (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.commaKey.wasPressedThisFrame) || Input.GetKeyDown(switchShoulderCameraKey)) {
				shoulderCameraToggled = !shoulderCameraToggled;
			}

			// Map 
			if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.slashKey.wasPressedThisFrame) {
				MiniMapCommon.isMapRotateWithCamera = !MiniMapCommon.isMapRotateWithCamera;
			}
			// Temporary recoil switch
			if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.periodKey.wasPressedThisFrame) {
				InputManagerExtenderCommon.ShoulderRecoilMultiplier = InputManagerExtenderCommon.ShoulderRecoilMultiplier <= 0f ? InputManagerExtenderCommon.DefaultShoulderRecoilMultiplier : 0f;
			}

			if (shoulderCameraToggled && !shoulderCameraInitalized) {
				OnShoulderCameraEnable();
			} else if (!shoulderCameraToggled && shoulderCameraInitalized) {
				OnShoulderCameraDisable();
			}

			if (target == null) {
				target = CharacterMainControl.Main;
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
			if (Application.isFocused && InputManager.InputActived && CharacterInputControl.Instance) { // No camera rotation while the game is paused or the inventory is open
				if (!isInputActiveLastFrame) {
					// Freeze input and camera rotation for one frame after input is active
				} else {
					// Update mouse delta to the rotation
					Vector2 currentMouseDelta = (Vector2)mouseDeltaField.GetValue(CharacterInputControl.Instance);
					// Shoulder surfing is more sensitive than the origin, so we use 0.01
					currentMouseDelta *= global::Duckov.Options.OptionsManager.MouseSensitivity * 0.01f;

					// Camera shaked by recoil
					float cameraShakePitchThisFrame = InputManagerExtender.cameraShakePixels * global::Duckov.Options.OptionsManager.MouseSensitivity * 0.01f;

					cameraYaw += currentMouseDelta.x;
					cameraPitch = Mathf.Clamp(cameraPitch + cameraShakePitchThisFrame + currentMouseDelta.y, -70f, 70f);
				}

				isInputActiveLastFrame = true;
			} else {
				isInputActiveLastFrame = false;
			}

			mainCamera.fieldOfView = hookCamera.mainVCam.m_Lens.FieldOfView;

			mainCamera.transform.rotation = Quaternion.Euler(-cameraPitch, cameraYaw, 0f);

			UpdateCollidedCameraPosition();

			DisableAimOcclusionFade();

			global::System.Action<global::GameCamera, global::CharacterMainControl> onCameraPosUpdate = global::GameCamera.OnCameraPosUpdate;
			if (onCameraPosUpdate != null) {
				onCameraPosUpdate(hookCamera, this.target);
			}
		}

		int aimPosHash = Shader.PropertyToID("OC_AimPos");
		int aimViewDirHash = Shader.PropertyToID("OC_AimViewDir");
		CharacterMainControl target;

		GameCamera hookCamera;
		Camera mainCamera;
		// CinemachineVirtualCamera mainCamera;

		FieldInfo mouseDeltaField;

		// Camera position lerp while switching shoulder side
		const float cameraLerpSpeed = 5f;
		Vector3 currentShoulderCameraOffset;
		Vector3 targetShoulderCameraOffset = new Vector3(1f, 1f, -2.8f);
		Vector3 anchorOffset = Vector3.up * 0.75f;

		int cameraCollisionLayerMask;
		bool isInputActiveLastFrame = false;

		private float cameraPitch = 0f;
		private float cameraYaw = 0f;

		/*---------- Origin paramters of camera ----------*/
		float originDefaultFOV = 0f;
		float originAdsFOV = 0f;
		float originFarClip = 0f;
	}
}
