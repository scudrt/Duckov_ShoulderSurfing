using HarmonyLib;
using UnityEngine;


[HarmonyPatch(typeof(StaminaHUD))]
[HarmonyPatch("Awake")]
public static class StaminaExtender {
    public static void Postfix(StaminaHUD __instance)
    {
        // 获取Canvas组件
        Canvas canvas = __instance.GetComponent<Canvas>();
        if (canvas == null)
        {
			canvas = __instance.gameObject.AddComponent<Canvas>();
        }

        // 如果Canvas的渲染模式不是ScreenSpaceOverlay，则修改
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // 获取RectTransform
        RectTransform rectTransform = __instance.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = __instance.gameObject.AddComponent<RectTransform>();
        }

        // 设置锚点为底部中间
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);

        // 设置位置
        float yPos = Screen.height / 3f;
        rectTransform.anchoredPosition = new Vector2(0, yPos);
    }
}

