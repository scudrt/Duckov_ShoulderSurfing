using System.Collections;
using System.Reflection;
using Duckov.MiniMaps.UI;
using Duckov.UI.MainMenu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShoulderSurfing
{
    public class CustomMinimapManager
    {
        public static bool isEnabled = false; // 设置是否启用
        public static bool isToggled = false; // 快捷键开关状态
        public static Vector2 miniMapSize = new Vector2(200, 200);
        public static float minimapContainerSizeScale = 1f;
        public static KeyCode displayZoomUpKey = KeyCode.Equals;
        public static KeyCode displayZoomDownKey = KeyCode.Minus;
        public static KeyCode MinimapToggleKey = KeyCode.Alpha9;

        public static float displayZoomScale = 2f;
        public static float displayZoomGap = 1f;
        public static Vector2 displayZoomRange = new Vector2(0.1f, 30);
        public static Vector2 miniMapPositionOffset = new Vector2(0.85f, 0.1f); // 左上角位置

        public static CustomMinimapManager Instance;

        [Header("MiniMap Settings")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        [Header("Player Reference")]
        public Transform playerTransform; // 玩家Transform，用于位置同步

        private GameObject customCanvas;
        private GameObject miniMapContainer;
        public GameObject miniMapScaleContainer;
        private GameObject duplicatedMinimapObject;

        public MiniMapDisplay DuplicatedMinimapDisplay
        {
            get
            {
                return duplicatedMinimapDisplay;
            }
        }
        private MiniMapDisplay duplicatedMinimapDisplay;
        private RectTransform miniMapRect;
        private RectTransform minimapViewportRect;

        private Coroutine settingCor;
        private Coroutine initMapCor;

        public static void CheckToggleKey()
        {
            if (!(isEnabled && Instance != null && Instance.duplicatedMinimapObject != null))
                return;
            if (Input.GetKeyDown(MinimapToggleKey))
            {
                if (Instance.customCanvas != null) {
                    isToggled = !isToggled;
					Instance.customCanvas.SetActive(isToggled);
                }
            }
        }
        
        public static void Enable(bool enable)
        {
            isEnabled = enable;
            isToggled = enable;
            if (isEnabled)
            {
                Instance._InitializeMiniMap();
                Instance.customCanvas.SetActive(true);
            }
            else
            {
                Instance.customCanvas.SetActive(false);
                Instance.ClearMap();
            }
        }

        public static void TryShow()
        {
            if (Instance == null)
                return;
            if (Instance.customCanvas != null && isEnabled)
            {
                Instance.customCanvas.SetActive(true);
            }
        }

        public static void Hide()
        {
            if (Instance == null)
                return;
            if (Instance.customCanvas != null)
            {
                Instance.customCanvas.SetActive(false);
            }
        }

        public void StartSetting()
        {
            miniMapContainer.transform.SetParent(GameManager.PauseMenu.transform);
            if (settingCor != null)
            {
                ModBehaviour.Instance.StopCoroutine(settingCor);
                settingCor = null;
            }
            settingCor = ModBehaviour.Instance.StartCoroutine(FinishSetting());
        }

        public IEnumerator FinishSetting()
        {
            yield return new WaitForSecondsRealtime(1f);
            miniMapContainer.transform.SetParent(customCanvas.transform);
        }

        public static bool IsInitialized()
        {
            return isEnabled && Instance != null && Instance.duplicatedMinimapObject != null;
        }

        public static bool HasMap()
        {
            // Debug.Log($"has map????:{Instance.duplicatedMinimapObject} {isEnabled && Instance != null && Instance.duplicatedMinimapObject != null && Instance.customCanvas.activeInHierarchy}");
            return isEnabled && Instance != null && Instance.duplicatedMinimapObject != null && Instance.customCanvas.activeInHierarchy;
        }

        public static void DisplayZoom(int symbol)
        {
            if (!HasMap())
                return;
            displayZoomScale += symbol * displayZoomGap;
            displayZoomScale = Mathf.Clamp(displayZoomScale, displayZoomRange.x, displayZoomRange.y);
            Instance.UpdateDisplayZoom();
        }

        private void UpdateDisplayZoom() {
            duplicatedMinimapDisplay.transform.localScale = Vector3.one * displayZoomScale;
            // CallDisplayMethod("FitContent");
        }

        public static void SetMinimapContainerScale(float scale)
        {
            minimapContainerSizeScale = scale;
            // Debug.Log($"设置小地图scale {minimapContainerSizeScale}");
            Instance._SetMinimapContainerScale();
            if (!HasMap())
                return;
            Instance.StartSetting();
        }

        private void _SetMinimapContainerScale(){
            // Debug.Log($"minimapContainerSizeScale: {minimapContainerSizeScale} {miniMapSize * minimapContainerSizeScale}");
            miniMapContainer.transform.localScale = Vector3.one * minimapContainerSizeScale;
        }

        public static void SetMinimapPosition()
        {
            SetMinimapPosition(miniMapPositionOffset);
        }

        private static void SetMinimapPosition(Vector2 offset)
        {
            miniMapPositionOffset = offset;
            // Debug.Log($"设置小地图位置 {miniMapPositionOffset}");
            Instance._SetMinimapPosition();
            if (!HasMap())
                return;
            Instance.StartSetting();
        }

        private void _SetMinimapPosition()
        {
            // var parentRect = customCanvas.GetComponent<RectTransform>();
            var parentRect = GameManager.PauseMenu.GetComponent<RectTransform>();
            float parentWidth = parentRect.rect.width;
            float parentHeight = parentRect.rect.height;
            if (parentRect.rect.width <= 0 || parentRect.rect.height <= 0)
            {
                Debug.LogWarning($"父Canvas尺寸异常: {parentRect.rect.width} x {parentRect.rect.height}");
                return;
            }
            // 计算偏移量：将归一化坐标转换为相对于中心锚点的像素偏移
            // x=0时，偏移为 -parentWidth/2（最左）；x=1时，偏移为 parentWidth/2（最右）
            float offsetX = (miniMapPositionOffset.x - 0.5f) * parentWidth;
            float offsetY = (miniMapPositionOffset.y - 0.5f) * parentHeight;
            // Debug.Log($"设置小地图位置 {miniMapPositionOffset} 偏移量 {offsetX} {offsetY} {parentWidth}x{parentHeight}");
            miniMapRect.anchoredPosition = new Vector2(offsetX, offsetY);
        }

        public CustomMinimapManager()
        {
            if (Instance != null)
            {
                Debug.LogError("MinimapManager 已实例化");
                return;
            }
            Instance = this;
            CreateMiniMapContainer();
			SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Destroy()
        {
            Debug.Log($"destroy minimap container");
            GameObject.Destroy(miniMapContainer);
			SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (LevelManager.Instance == null || !isEnabled)
            {
                customCanvas.SetActive(false);
                return;
            }
            Debug.Log($"初始化场景 {scene} {mode}");
            if (initMapCor != null)
                ModBehaviour.Instance.StopCoroutine(initMapCor);
            ClearMap();
            customCanvas.SetActive(false);
            initMapCor = ModBehaviour.Instance.StartCoroutine(InitializeMiniMap());
        }

        public void ClearMap()
        {
            GameObject.Destroy(duplicatedMinimapObject);
            duplicatedMinimapObject = null;
        }

        void HandleUIBlockState() {
            bool minimapIsOn = IsInitialized();
			bool inputActive = Application.isFocused && InputManager.InputActived && CharacterInputControl.Instance;
			if (!inputActive) {
				// Hide minimap when UI is on
				Hide();
			} else {
				// Recover minimap state when shoulder camera is under control
				if (minimapIsOn && isToggled) {
					TryShow();
				} else if (minimapIsOn && !isToggled) {
					Hide();
				}
			}
		}

        public void Update() {
            if (!LevelManager.LevelInited) {
                return;
            }

            HandleUIBlockState();
            bool minimapIsOn = HasMap();
			if (!minimapIsOn) {
                return;
            }

            // Debug.Log($"??:{duplicatedMinimapObject} {duplicatedMinimapObject != null} {minimapIsOn} {Instance} {Instance.customCanvas.activeInHierarchy}");
            // Update minimap if active
            if (Input.GetKeyDown(displayZoomDownKey))
            {
                DisplayZoom(-1);
            }
            else if (Input.GetKeyDown(displayZoomUpKey))
            {
                DisplayZoom(1);
            }

            // 每帧更新小地图位置
            CallDisplayMethod("SetupRotation");
            UpdateMiniMapRotation();
            UpdateMiniMapPosition();
        }

        public IEnumerator InitializeMiniMap()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Debug.Log("初始化小地图");
            _InitializeMiniMap();
            customCanvas.SetActive(true);
        }

        public void _InitializeMiniMap()
        {
            if (DuplicateMinimapDisplay())
            {
                // 3. 设置复制的地图对象
                SetupDuplicatedMinimap();

                Debug.Log("MiniMap initialized successfully!");
            }
            else
            {
                Debug.LogError("Failed to initialize MiniMap!");
            }
        }

        private void CreateMiniMapContainer()
        {
            // 创建 Canvas
            customCanvas = new GameObject("CustomMinimapCanvas");
            var targetCanvas = customCanvas.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            targetCanvas.sortingOrder = 0; // 确保在最前面

            // 添加 CanvasScaler 用于适应不同分辨率
            CanvasScaler scaler = customCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560, 1440);

            // // 创建 CanvasGroup 用于控制透明度
            customCanvas.AddComponent<CanvasGroup>();
            // 创建小地图UI容器
            miniMapContainer = new GameObject("MiniMapContainer");
            miniMapRect = miniMapContainer.AddComponent<RectTransform>();
            miniMapContainer.transform.SetParent(customCanvas.transform);

            // 设置位置和大小（左上角）
            miniMapRect.anchorMin = new Vector2(0.5f, 0.5f);
            miniMapRect.anchorMax = new Vector2(0.5f, 0.5f);
            miniMapRect.pivot = new Vector2(0.5f, 0.5f);
            miniMapRect.sizeDelta = miniMapSize;
            miniMapRect.anchoredPosition = Vector2.zero;
            _SetMinimapPosition();
            _SetMinimapContainerScale();

            // 添加背景
            Image background = miniMapContainer.AddComponent<Image>();
            background.color = backgroundColor;
            background.raycastTarget = false;

            // 创建遮罩区域
            GameObject viewportObject = new GameObject("MiniMapViewport");
            minimapViewportRect = viewportObject.AddComponent<RectTransform>();

            miniMapScaleContainer = new GameObject("test");
            var scaleRect = miniMapScaleContainer.AddComponent<RectTransform>();
            scaleRect.localScale = Vector3.one * 0.5f;
            scaleRect.SetParent(minimapViewportRect);

            viewportObject.AddComponent<Image>().color = Color.clear; // 透明背景
            RectMask2D rectMask = viewportObject.AddComponent<RectMask2D>();

            // 设置遮罩为容器的子对象，并填满容器
            minimapViewportRect.SetParent(miniMapRect);
            minimapViewportRect.anchorMin = Vector2.zero;
            minimapViewportRect.anchorMax = Vector2.one;
            minimapViewportRect.offsetMin = Vector2.zero;
            minimapViewportRect.offsetMax = Vector2.zero;
            customCanvas.SetActive(false);
            GameObject.DontDestroyOnLoad(customCanvas);
            Debug.Log($"create minimap container finished");
        }

        private bool DuplicateMinimapDisplay()
        {
            try
            {
                // 使用反射获取MinimapView单例
                Type minimapViewType = typeof(MiniMapView);
                if (minimapViewType == null)
                {
                    Debug.LogError("MinimapView type not found!");
                    return false;
                }
                MiniMapView minimapView = MiniMapView.Instance;

                // 获取minimapdisplay字段（private）
                FieldInfo minimapDisplayField = minimapViewType.GetField("display",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (minimapDisplayField == null)
                {
                    // 尝试属性
                    PropertyInfo minimapDisplayProperty = minimapViewType.GetProperty("display",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (minimapDisplayProperty == null)
                    {
                        Debug.LogError("display field/property not found!");
                        return false;
                    }

                    object minimapDisplayObj = minimapDisplayProperty.GetValue(minimapView);
                    if (minimapDisplayObj is MiniMapDisplay originalDisplay)
                    {
                        return DuplicateGameObject(originalDisplay);
                    }
                }
                else
                {
                    object minimapDisplayObj = minimapDisplayField.GetValue(minimapView);
                    if (minimapDisplayObj is MiniMapDisplay originalDisplay)
                    {
                        return DuplicateGameObject(originalDisplay);
                    }
                }
                Debug.Log($"duplicate minimap display finished");

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error duplicating minimapdisplay: {e.Message}");
                return false;
            }
        }

        private bool DuplicateGameObject(MiniMapDisplay originalDisplay)
        {
            if (originalDisplay == null)
            {
                Debug.LogError("Original minimapdisplay is null!");
                return false;
            }

            // 复制整个GameObject
            GameObject originalGameObject = originalDisplay.gameObject;
            duplicatedMinimapObject = GameObject.Instantiate(originalGameObject);

            // 获取复制后的MinimapDisplay组件
            duplicatedMinimapDisplay = (MiniMapDisplay)duplicatedMinimapObject.GetComponent(originalDisplay.GetType());
            
            if (duplicatedMinimapDisplay == null)
            {
                Debug.LogError("Failed to get duplicated MinimapDisplay component!");
                GameObject.Destroy(duplicatedMinimapObject);
                return false;
            }

            // 重命名
            duplicatedMinimapObject.name = "MiniMap_Duplicate";
            return true;
        }

        private void SetupDuplicatedMinimap()
        {
            duplicatedMinimapObject.transform.SetParent(miniMapScaleContainer.transform);
            RectTransform rectTransform = duplicatedMinimapObject.GetComponent<RectTransform>();
            CallDisplayMethod("AutoSetup");
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localScale = Vector3.one;
            }

            duplicatedMinimapObject.transform.localPosition = Vector3.zero;
            duplicatedMinimapDisplay.transform.localRotation = Quaternion.identity;
            UpdateDisplayZoom();
            // 禁用可能干扰的交互组件
            // DisableInterferenceComponents();
        }

        private void CallDisplayMethod(string methodName)
        {
            if (duplicatedMinimapDisplay == null)
            {
                Debug.LogError($"Cannot call {methodName} - duplicatedMinimapDisplay is null!");
                return;
            }

            try
            {
                // 使用反射调用AutoSetup方法
                Type minimapDisplayType = duplicatedMinimapDisplay.GetType();
                MethodInfo autoSetupMethod = minimapDisplayType.GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (autoSetupMethod != null)
                {
                    autoSetupMethod.Invoke(duplicatedMinimapDisplay, null);
                    // Debug.Log($"{methodName} method called successfully on duplicated minimap.");
                }
                else
                {
                    Debug.LogWarning($"{methodName} method not found in MinimapDisplay type. This might be expected if the method doesn't exist.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calling {methodName} method: {e.Message}");
            }
        }

        // private void DisableInterferenceComponents()
        // {
        //     // 禁用可能干扰小地图显示的组件
        //     MonoBehaviour[] components = duplicatedMinimapObject.GetComponentsInChildren<MonoBehaviour>();
        //     foreach (var component in components)
        //     {
        //         // 禁用全屏相关的组件或事件处理
        //         if (component is IScrollHandler || component is IEventSystemHandler)
        //         {
        //             // 可以根据需要选择性禁用
        //             // component.enabled = false;
        //         }

        //         // 可以根据组件类型名称禁用特定组件
        //         string componentType = component.GetType().Name;
        //         if (componentType.Contains("Fullscreen") || componentType.Contains("FullMap"))
        //         {
        //             component.enabled = false;
        //         }
        //     }
        // }

        private void UpdateMiniMapRotation()
        {
            if (MiniMapCommon.isMapRotateWithCamera) {
                Vector3 to = ShoulderCamera.CameraForward;
                float currentMapZRotation = Vector3.SignedAngle(Vector3.forward, to, Vector3.up);
                duplicatedMinimapDisplay.transform.rotation = Quaternion.Euler(0, 0, currentMapZRotation);
            } else {
                duplicatedMinimapDisplay.transform.rotation = Quaternion.Euler(0f, 0f, MiniMapCompassExtender.originMapZRotation);
            }

        }

        private void UpdateMiniMapPosition()
        {
            CharacterMainControl main = CharacterMainControl.Main;
            if (main == null)
            {
                return;
            }
            Vector3 minimapPos;
            if (!duplicatedMinimapDisplay.TryConvertWorldToMinimap(main.transform.position, SceneInfoCollection.GetSceneID(SceneManager.GetActiveScene().buildIndex), out minimapPos))
            {
                return;
            }
            RectTransform rectTransform = duplicatedMinimapDisplay.transform as RectTransform;
            if (rectTransform == null)
            {
                return;
            }
            Vector3 b = rectTransform.localToWorldMatrix.MultiplyPoint(minimapPos);
            Vector3 b2 = (rectTransform.parent as RectTransform).position - b;
            rectTransform.position += b2;
        }

    }
}