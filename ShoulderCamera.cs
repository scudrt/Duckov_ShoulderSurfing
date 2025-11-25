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
using ECM2;
using Steamworks;
using Unity.VisualScripting;
using Unity.Collections;
using UnityEngine.ProBuilder;

namespace ShoulderSurfing {
	public class ShoulderCamera: MonoBehaviour {
		// Global switch indicating whether we're in shoulder surfing view
		public static bool shoulderCameraToggled = true;
		public static bool shoulderCameraInitalized = false;

		public static bool invertYAxis = false;

		public static bool isMiniGameEnabled = false;

		public static bool enableAimOcclusionFade = false; // 启用准星透视

		public static Material characterFadeMaterial; //  = Util.LoadMaterial("SodaCharacter");
		public static Shader characterFadeShader;

		public static float shoulderCameraOffsetX {
			get {
				return __shoulderCameraOffsetX;
			} set {
				__shoulderCameraOffsetX = value;
				if (Instance) {
					Instance.UpdateCameraTargetPos();
				}
			}
		}
		public static float shoulderCameraOffsetY {
			get {
				return __shoulderCameraOffsetY;
			}
			set {
				__shoulderCameraOffsetY = value;
				if (Instance) {
					Instance.UpdateCameraTargetPos();
				}
			}
		}
		public static float shoulderCameraOffsetZ {
			get {
				return __shoulderCameraOffsetZ;
			}
			set {
				__shoulderCameraOffsetZ = value;
				if (Instance) {
					Instance.UpdateCameraTargetPos();
				}
			}
		}
		private static float __shoulderCameraOffsetX = 1f;
		private static float __shoulderCameraOffsetY = 1f;
		private static float __shoulderCameraOffsetZ = -2.8f;

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
		public const float minADSFactor = 1.1f;
		public static float currentADSFactor = 1f;

		// Mouse sensitivity parameters
		public static float mouseSensitivityRate = 1f;
		public static float mouseSensitivityRateADS = 1f;

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

		public static Vector3 CameraForward
		{
			get
			{
				if (shoulderCameraInitalized)
				{
					// var tmp = Instance.mainCamera.transform.forward;
					// tmp.y = 0f;
					return new Vector3(Mathf.Sin(Instance.cameraYaw * Mathf.Deg2Rad), 0, Mathf.Cos(Instance.cameraYaw * Mathf.Deg2Rad));
				}
				else
				{
					if (!LevelManager.LevelInited)
					{
						return new Vector3(Mathf.Sin(Instance.cameraYaw * Mathf.Deg2Rad), 0, Mathf.Cos(Instance.cameraYaw * Mathf.Deg2Rad));
					}
					if (LevelManager.Instance.GameCamera == null)
					{
						return new Vector3(Mathf.Sin(Instance.cameraYaw * Mathf.Deg2Rad), 0, Mathf.Cos(Instance.cameraYaw * Mathf.Deg2Rad));
					}
					// return LevelManager.Instance.GameCamera.renderCamera.WorldToScreenPoint(LevelManager.Instance.InputManager.InputAimPoint);
					return LevelManager.Instance.InputManager.InputAimPoint - CharacterMainControl.Main.transform.position;
				}
			}
		}

		public static void SyncMaterialProperties(Material dstMat, Material srcMat) {
			for (int i = 0; i < srcMat.shader.GetPropertyCount(); ++i) {
				int propId = srcMat.shader.GetPropertyNameId(i);
				if (!dstMat.HasProperty(propId)) {
					Debug.Log("no property" + srcMat.shader.GetPropertyName(i));
					continue;
				}
				Debug.Log("sync property" + srcMat.shader.GetPropertyName(i));

				var typeOfProp = srcMat.shader.GetPropertyType(i);
				if (typeOfProp == ShaderPropertyType.Float || typeOfProp == ShaderPropertyType.Range) {
					dstMat.SetFloat(propId, srcMat.GetFloat(propId));
				} else if (typeOfProp == ShaderPropertyType.Int) {
					dstMat.SetInt(propId, srcMat.GetInt(propId));
				} else if (typeOfProp == ShaderPropertyType.Color) {
					dstMat.SetColor(propId, srcMat.GetColor(propId));
				} else if (typeOfProp == ShaderPropertyType.Vector) {
					dstMat.SetVector(propId, srcMat.GetVector(propId));
				} else if (typeOfProp == ShaderPropertyType.Texture) {
					dstMat.SetTexture(propId, srcMat.GetTexture(propId));
					dstMat.SetTextureOffset(propId, srcMat.GetTextureOffset(propId));
					dstMat.SetTextureScale(propId, srcMat.GetTextureScale(propId));
				} else {
					Debug.Log("failed to sync property");
				}
			}
		}

		public static void TrySetTransformCameraFade(Transform trans, bool enable) {
			if (trans == null) {
				return;
			}

			foreach (var renderer in trans.GetComponentsInChildren<Renderer>()) {
				var originMats = renderer.materials;
				// var originMats = renderer.sharedMaterials;
				if(originMats == null)
					continue;
				for (int i = 0; i < originMats.Length; ++i) {
					var mat = originMats[i];

					// Replace camera fade shader for character and weapon
					if (mat.shader.name != "SodaCraft/SodaCharacter" && mat.shader.name != "SodaCraft/SodaLit") {
						continue;
					}

					bool usingCustomMaterial = mat.HasFloat("_CameraFadeStartDistance");
					if (!usingCustomMaterial) {
						// Create and replace our camera fade material to enable camera fading
						if (enable) {
							var newMat = new Material(characterFadeShader);
							newMat.CopyMatchingPropertiesFromMaterial(mat);
							originMats[i] = newMat;
						} else {
							; // Do nothing while disabling camera fade without our material
						}
					} else {
						mat.SetFloat("_CameraFadeStartDistance", enable ? 1.1f : 0.1f);
						mat.SetFloat("_CameraFadeEndDistance", enable ? 0.8f : 0f);
					}
				}
				renderer.sharedMaterials = originMats;
			}
		}

		public void RehookCamera()
		{
			if (hookCamera != null && mainCamera != null) {
				return;
			}
			if (LevelManager.LevelInited == false || CharacterMainControl.Main == null) {
				return;
			}

			this.hookCamera = GameCamera.Instance;
			if (hookCamera == null || hookCamera.renderCamera == null) {
				return;
			}

			if (target == null) {
				target = CharacterMainControl.Main;
			}

			if (shoulderCameraToggled)
			{
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
			if (enableAimOcclusionFade) {
				return;
			}
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

		public void UpdateADSFov() {
			if (hookCamera == null) {
				return;
			}
			// Update ADS factor from gun
			ItemAgent_Gun gun = target ? target.GetGun() : null;
			float targetADSFactor = minADSFactor;
			if (gun) {
				targetADSFactor = Mathf.Max(minADSFactor, gun.ADSAimDistanceFactor);
			}
			if (targetADSFactor == currentADSFactor) {
				return;
			}

			currentADSFactor = targetADSFactor;
			// Recalculate FOV by new ADS factor
			hookCamera.adsFOV = MathF.Asin(MathF.Sin(FOV * Mathf.Deg2Rad) / targetADSFactor) * Mathf.Rad2Deg;
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

		public void TrySetCharacterFadeEnable(bool enable) {
			// Search all renderers of the target for occlusion hidding
			Transform characterTrans = target.transform.Find("ModelRoot/0_CharacterModel_Custom_Template(Clone)");
			TrySetTransformCameraFade(characterTrans, enable);
		}

		public void OnShoulderCameraEnable() {
			if (shoulderCameraInitalized) {
				return;
			}
			if (hookCamera == null || hookCamera.renderCamera == null) {
				return;
			}

			originDefaultFOV = hookCamera.defaultFOV;
			originAdsFOV = hookCamera.adsFOV;
			// Initialize game camera FOV
			hookCamera.defaultFOV = ShoulderCamera.FOV;

			mainCamera = hookCamera.renderCamera; // hookCamera.mainVCam;
			currentADSFactor = 1;

			if (hookCamera.mianCameraArm != null) {
				hookCamera.mianCameraArm.enabled = false;
			}
			if (hookCamera.brain != null) {
				hookCamera.brain.enabled = false;
			}
			if (hookCamera.mainVCam != null) {
				hookCamera.mainVCam.enabled = false;
			}

			originFarClip = mainCamera.farClipPlane; // original: 300f
			mainCamera.farClipPlane = ShoulderCamera.renderDistance;

			TrySetCharacterFadeEnable(true);

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

			currentADSFactor = 1;
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

			TrySetCharacterFadeEnable(false);

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
		}

		void UpdateCameraTargetPos() {
			targetCameraOffset.x = __shoulderCameraOffsetX * (IsLeftShoulder ? -1f : 1f);
			targetCameraOffset.y = __shoulderCameraOffsetY;
			targetCameraOffset.z = __shoulderCameraOffsetZ;
		}

		void UpdateCollidedCameraPosition() {
			// Update camera position by follow target
			Vector3 cameraForward = mainCamera.transform.forward;
			Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
			Vector3 anchorPos = target.transform.position + anchorOffset;

			UpdateCameraTargetPos();

			// Updating camera position with smooth lerp
			// v1.0: (1, 1.75, -2.8)
			// v1.1: (1, 1.25, -2.8)
			// v1.2: (1, 1.00, -2.8)
			Vector3 cameraMoveOffset = targetCameraOffset - currentCameraOffset;
			float camPosDeltaThisFrame = Time.unscaledDeltaTime * cameraSideSmoothSpeed;
			if (cameraMoveOffset.magnitude > camPosDeltaThisFrame) {
				currentCameraOffset += cameraMoveOffset.normalized * camPosDeltaThisFrame;
			} else {
				currentCameraOffset = targetCameraOffset;
			}
			Vector3 camOffset = cameraRight * currentCameraOffset.x
										+ Vector3.up * currentCameraOffset.y
										+ cameraForward * currentCameraOffset.z;
			Vector3 camDir = camOffset.normalized;
			float targetCamDistance = camOffset.magnitude;

			// Handle camera collision
			Ray shoulderCameraRay = new Ray(anchorPos, camDir);
			RaycastHit hitinfo;
			if (Physics.SphereCast(shoulderCameraRay, 0.15f, out hitinfo, targetCamDistance, cameraCollisionLayerMask)) {
				targetCamDistance = hitinfo.distance;
			}

			// Camera smooth arm
			float cameraSmoothArmDelta = cameraSmoothArmSpeed * Time.deltaTime;
			if (true/* enableSmmothCamera */) {
				float cameraDistanceGap = targetCamDistance - currentCameraDistance;
				if (Mathf.Abs(cameraDistanceGap) > cameraSmoothArmDelta) {
					currentCameraDistance += cameraSmoothArmDelta * MathF.Sign(cameraDistanceGap);
				} else {
					currentCameraDistance = targetCamDistance;
				}
			} else {
				// Set new camera position directly
				currentCameraDistance = targetCamDistance;
			}
			mainCamera.transform.position = anchorPos + camDir * currentCameraDistance;
		}

		void Start() {
			cameraYaw = 0f;
			cameraPitch = 0f;

			cameraCollisionLayerMask = (1 << LayerMask.NameToLayer("FowBlock"))
				| (1 << LayerMask.NameToLayer("Ground"))
				| (1 << LayerMask.NameToLayer("Wall_FowBlock"));

			mouseDeltaField = typeof(CharacterInputControl).GetField("mouseDelta", BindingFlags.NonPublic | BindingFlags.Instance);

			characterFadeShader = Util.LoadShader("SodaCharacter");

			shoulderCameraToggled = true;
			shoulderCameraInitalized = false;
		}

		void Update() {
			if (!LevelManager.LevelInited) {
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
			
			/*
			// Test code here
			if (Keyboard.current.vKey.wasPressedThisFrame) {
			}
			*/

			// Update and handle player keyboard input state
			isInputActiveLastFrame = isInputActiveForCamera;
			isInputActiveForCamera = Application.isFocused && InputManager.InputActived && CharacterInputControl.Instance;
			if (isInputActiveForCamera) {
				HandleShoulderSideSwitch();

				// View switch
				if (Input.GetKeyDown(switchShoulderCameraKey) || (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.commaKey.wasPressedThisFrame)) {
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
			}

			if (shoulderCameraToggled && !shoulderCameraInitalized) {
				OnShoulderCameraEnable();
			} else if (!shoulderCameraToggled && shoulderCameraInitalized) {
				OnShoulderCameraDisable();
			}
		}

		void LateUpdate() {
			if (!shoulderCameraToggled || !shoulderCameraInitalized) {
				return;
			}
			if (hookCamera == null || target == null || mainCamera == null) {
				return; // Come in next frame :)
			}

			// Refresh ADS fov when aimming
			float __mouseSensitivityRate = mouseSensitivityRate; /* / 1.0f; */
			if (target.IsInAdsInput) {
				UpdateADSFov();
				__mouseSensitivityRate = mouseSensitivityRateADS / currentADSFactor;
			}

			// Update camera rotation by player input
			if (isInputActiveForCamera) { // No camera rotation while the game is paused or the inventory is open
				// Freeze input and camera rotation for one frame after input is active
				if (isInputActiveLastFrame) {
					// Update mouse delta to the rotation
					Vector2 currentMouseDelta = (Vector2)mouseDeltaField.GetValue(CharacterInputControl.Instance);
					if (invertYAxis) { // Apply invert Y axis
						currentMouseDelta.y = -currentMouseDelta.y;
					}
					// Shoulder surfing is more sensitive than the origin, so we use 0.01
					currentMouseDelta *= global::Duckov.Options.OptionsManager.MouseSensitivity * 0.01f * __mouseSensitivityRate;

					// Camera shaked by recoil
					float cameraShakePitchThisFrame = InputManagerExtender.cameraShakePixels * global::Duckov.Options.OptionsManager.MouseSensitivity * 0.01f;

					cameraYaw += currentMouseDelta.x;
					cameraPitch = Mathf.Clamp(cameraPitch + cameraShakePitchThisFrame + currentMouseDelta.y, -70f, 70f);
				}
			}

			mainCamera.fieldOfView = hookCamera.mainVCam.m_Lens.FieldOfView;

			mainCamera.transform.rotation = Quaternion.Euler(-cameraPitch, cameraYaw, 0f);

			UpdateCollidedCameraPosition();

			DisableAimOcclusionFade();

			global::System.Action<global::GameCamera, global::CharacterMainControl> onCameraPosUpdate = global::GameCamera.OnCameraPosUpdate;
			if (onCameraPosUpdate != null) {
				// onCameraPosUpdate(hookCamera, this.target);
			}
		}

		int aimPosHash = Shader.PropertyToID("OC_AimPos");
		int aimViewDirHash = Shader.PropertyToID("OC_AimViewDir");
		CharacterMainControl? target;

		GameCamera hookCamera;
		public Camera mainCamera { get; private set; }
		// CinemachineVirtualCamera mainCamera;

		FieldInfo mouseDeltaField;

		// Camera position lerp while switching shoulder side
		const float cameraSideSmoothSpeed = 5f;
		const float cameraSmoothArmSpeed = 8.5f;
		float currentCameraDistance = 3f;
		Vector3 currentCameraOffset;
		Vector3 targetCameraOffset;
		Vector3 anchorOffset = Vector3.up * 0.75f;

		int cameraCollisionLayerMask;
		bool isInputActiveLastFrame = false;
		bool isInputActiveForCamera = false;

		private float cameraPitch = 0f;
		private float cameraYaw = 0f;

		/*---------- Origin paramters of camera ----------*/
		float originDefaultFOV = 0f;
		float originAdsFOV = 0f;
		float originFarClip = 0f;
	}
}
