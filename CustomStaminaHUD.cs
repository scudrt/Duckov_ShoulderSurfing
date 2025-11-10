using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ShoulderSurfing;
using System.Collections;

public class CustomStaminaHUD : MonoBehaviour
{
    private GameObject customCanvas;
    private GameObject barObject;
    private Image fillImage;
    private CanvasGroup canvasGroup;
    private CharacterMainControl characterMainControl;
    private float percent;
    private float targetAlpha;

    // 颜色渐变（参考原版的 glowColor）
    private Gradient glowColor;

    private bool isAlwayShow = false;
    private Coroutine alwayShowCor;
    private void Start()
    {
        CreateCustomUI();
        InitializeGradient();
    }

    public void _UpdateBarPos(Vector2 offset, RectTransform parentRect)
    {
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;
        if (parentRect.rect.width <= 0 || parentRect.rect.height <= 0)
        {
            Debug.LogWarning($"父Canvas尺寸异常: {parentRect.rect.width} x {parentRect.rect.height}");
            return;
        }
        var barRect = barObject.GetComponent<RectTransform>();
        // 计算偏移量：将归一化坐标转换为相对于中心锚点的像素偏移
        // x=0时，偏移为 -parentWidth/2（最左）；x=1时，偏移为 parentWidth/2（最右）
        float offsetX = (offset.x - 0.5f) * parentWidth;
        float offsetY = (offset.y - 0.5f) * parentHeight;
        // 设置anchoredPosition
        barRect.anchoredPosition = new Vector2(offsetX, offsetY);
    }

    public void UpdateBarPos(Vector2 offset)
    {
        SetTempAlwayShow();
        // 获取父RectTransform的实际宽度和高度
        _UpdateBarPos(offset, GameManager.PauseMenu.GetComponent<RectTransform>());
    }

    public void UpdateScale(float scale)
    {
        barObject.transform.localScale = Vector3.one * scale;
        SetTempAlwayShow();
    }

    public void SetTempAlwayShow()
    {
        barObject.transform.SetParent(GameManager.PauseMenu.transform);
        if (alwayShowCor != null)
        {
            StopCoroutine(alwayShowCor);
            alwayShowCor = null;
        }
        alwayShowCor = StartCoroutine(AlwaysShow());
    }

    public IEnumerator AlwaysShow()
    {
        yield return new WaitForSecondsRealtime(1f);
        barObject.transform.SetParent(customCanvas.transform);
    }

    private void CreateCustomUI()
    {
        // 创建 Canvas
        customCanvas = new GameObject("CustomStaminaCanvas");
        Canvas canvas = customCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // 确保在最前面

        // 添加 CanvasScaler 用于适应不同分辨率
        CanvasScaler scaler = customCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2560, 1440);

        // 创建 CanvasGroup 用于控制透明度
        canvasGroup = customCanvas.AddComponent<CanvasGroup>();
        CreateBackgroundCircle();
        CreateBarCircle();
        _UpdateBarPos(StaminaHUDExtender.staminaHUDOffset, canvas.GetComponent<RectTransform>());
        UpdateScale(StaminaHUDExtender.staminaHUDScale);
        // 设置为不销毁，跨场景保持
        DontDestroyOnLoad(customCanvas);

        Debug.Log("Custom Circular Stamina HUD created successfully");
    }

    private void CreateBarCircle()
    {
        // 创建 Image 作为耐力条 - 改为圆形填充
        GameObject imageObject = new GameObject("CustomStaminaImage");
        imageObject.transform.SetParent(barObject.transform);
        // 设置位置和大小（屏幕中央）
        fillImage = imageObject.AddComponent<Image>();

        // 设置 Image 属性为圆形填充
        fillImage.type = Image.Type.Filled;
        fillImage.sprite = Util.LoadSprite("CircleGlow.png");
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = false; // 顺时针填充
        // 设置颜色（初始为绿色）
        fillImage.color = Color.green;

        RectTransform rectTransform = fillImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(40, 40); // 圆形，所以宽高相同

    }

    private void CreateBackgroundCircle()
    {
        // 创建背景圆形
        barObject = new GameObject("StaminaBackground");
        barObject.transform.SetParent(customCanvas.transform);
        
        Image backgroundImage = barObject.AddComponent<Image>();
        backgroundImage.sprite = Util.LoadSprite("CircleGlowBK.png");
        
        // 设置背景颜色（半透明灰色）
        backgroundImage.color = new Color(0f, 0f, 0f, 0.8f);
        
        // 设置位置和大小，比前景稍大
        RectTransform bgRect = barObject.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(55, 55); // 比前景圆形大一点
    }
    
    
    private void InitializeGradient()
    {
        // 创建颜色渐变：绿 -> 黄 -> 红
        glowColor = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(Color.red, 0.0f);    // 低耐力：红色
        colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f); // 中等耐力：黄色
        colorKeys[2] = new GradientColorKey(Color.green, 1.0f);   // 满耐力：绿色
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
        
        glowColor.SetKeys(colorKeys, alphaKeys);
    }
    
    private void Update()
    {
        if (!LevelManager.Instance)
        {
            Hide();
            // Debug.Log("LevelManager is null");
            return;
        }

        if (!ShoulderCamera.shoulderCameraToggled)
        {
            if(StaminaHUDExtender.staminaHUD != null)
                StaminaHUDExtender.staminaHUD.SetActive(true);
            StaminaHUDExtender.modObject.SetActive(false);
            Hide();
            return;
        }
        // 获取主角控制器的引用
        if (!characterMainControl)
        {
            characterMainControl = LevelManager.Instance.MainCharacter;

        }

        if (!characterMainControl)
        {
            // Debug.Log("characterMainControl is null" + LevelManager.Instance.MainCharacter);
            return;
        }
        if (isAlwayShow)
        {
            Show();
            fillImage.fillAmount = 1;
            return;
        }

        // 计算耐力百分比
        float currentStamina = GetCurrentStamina();
        float maxStamina = GetMaxStamina();
        
        if (maxStamina <= 0) return;
        
        float newPercent = currentStamina / maxStamina;
        
        // 如果百分比变化，更新UI
        if (!Mathf.Approximately(newPercent, percent))
        {
            percent = newPercent;
            fillImage.fillAmount = percent;
            SetColor();
            
            // 根据原版逻辑设置目标透明度
            if (Mathf.Approximately(percent, 1f))
            {
                targetAlpha = 0f; // 满耐力时隐藏
            }
            else
            {
                targetAlpha = 1f; // 非满耐力时显示
            }
        }
        
        // 更新透明度
        UpdateAlpha(Time.unscaledDeltaTime);
    }
    
    private float GetCurrentStamina()
    {
        return characterMainControl.CurrentStamina;

    }
    
    private float GetMaxStamina()
    {
        return characterMainControl.MaxStamina;
    }
    
    private void SetColor()
    {
        // 根据百分比设置颜色
        Color targetColor = glowColor.Evaluate(percent);
        
        // 调整饱和度（模拟原版的颜色处理）
        float h, s, v;
        Color.RGBToHSV(targetColor, out h, out s, out v);
        s = 0.4f; // 稍微降低饱和度
        v = 1f;
        
        fillImage.color = Color.HSVToRGB(h, s, v);
    }
    
    private void UpdateAlpha(float deltaTime)
    {
        // 平滑过渡透明度
        // Debug.Log("！！！targetAlpha: " + targetAlpha);
        if (!Mathf.Approximately(targetAlpha, canvasGroup.alpha))
        {
            // Debug.Log("targetAlpha: " + targetAlpha);
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, 5f * deltaTime);
        }
    }
    
    // 公共方法，用于在需要时显示/隐藏
    public void Show()
    {
        targetAlpha = 1f;
        canvasGroup.alpha = 1f;
    }
    
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        targetAlpha = 0f;
    }
    
    public void Toggle()
    {
        targetAlpha = (targetAlpha == 0f) ? 1f : 0f;
    }
    
    // 清理资源
    private void OnDestroy()
    {
        if (customCanvas != null)
        {
            Destroy(customCanvas);
        }
    }
}

// Mod 初始化类
[HarmonyPatch(typeof(StaminaHUD))]
[HarmonyPatch("Update")]
public class StaminaHUDExtender
{
    public static GameObject modObject;
    public static GameObject staminaHUD;

    public static Vector2 staminaHUDOffset = new Vector2(0.28f, 0.26f);
    public static float staminaHUDScale = 1f;

    public static void HideCustomStaminaHUD()
    {
        if (modObject)
        {
            modObject.GetComponent<CustomStaminaHUD>().Hide();
        }
    }

    public static void UpdateCustomHUDOffset()
    {
        if (modObject)
        {
            modObject.GetComponent<CustomStaminaHUD>().UpdateBarPos(staminaHUDOffset);
        }
    }
    public static void UpdateCustomHUDScale(float scale)
    {
        if (modObject)
        {
            modObject.transform.localScale = Vector3.one * scale;
            staminaHUDScale = scale;
            modObject.GetComponent<CustomStaminaHUD>().UpdateScale(scale);
        }
    }

    public static bool Prefix(StaminaHUD __instance)
    {
        staminaHUD = __instance.gameObject;
        if (!ShoulderCamera.shoulderCameraToggled)
        {
            if (modObject != null)
                modObject.SetActive(false);
            staminaHUD.SetActive(true);
            return true;
        }
        if (modObject == null)
        {
            Initialize();
        }
        modObject.SetActive(true);
        staminaHUD.SetActive(false);
        return false;
    }

    public static void Unload()
    {
        if (modObject != null)
        {
            GameObject.Destroy(modObject);
        }
    }

    public static void Initialize()
    {
        // 创建 Mod GameObject
        modObject = new GameObject("ShoulderSurfingStaminaHUD");
        modObject.AddComponent<CustomStaminaHUD>();

        // 设置为不销毁
        GameObject.DontDestroyOnLoad(modObject);

        Debug.Log("ShoulderSurfing Mod initialized with custom Stamina HUD");
    }

}