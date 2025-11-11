using HarmonyLib;
using UnityEngine;
using ShoulderSurfing;

[HarmonyPatch(typeof(CharacterMainControl))]
[HarmonyPatch("StartAction")]
public static class CharacterMainControlStartActionExtender
{
    // 存储补丁状态
    private static bool lastActionResult = false;
    private static CharacterActionBase lastAction = null;
    
    // Postfix 补丁，在原方法执行后运行
    public static void Postfix(CharacterMainControl __instance, CharacterActionBase newAction, bool __result)
    {
        // 保存结果供其他代码使用
        lastActionResult = __result;
        lastAction = newAction;
        
        // 根据返回结果执行自定义逻辑
        if (__result)
        {
            OnActionStartedSuccessfully(__instance, newAction);
        }
    }
    
    // 当动作成功启动时的处理
    private static void OnActionStartedSuccessfully(CharacterMainControl character, CharacterActionBase action)
    {
        // 在这里添加动作成功启动时的自定义逻辑
        // Debug.Log($"动作成功启动: {GetActionName(action)}, 角色: {character.name}");
        if (GetActionName(action) == "CA_Dash")
            return;
        if(!action.CanRun())
            CustomMinimapManager.Hide();
    }
    
    // 获取动作名称的辅助方法
    private static string GetActionName(CharacterActionBase action)
    {
        if (action == null) return "Unknown";
        return action.GetType().Name;
    }
    
    // 公共方法，供其他代码查询最后一次动作执行结果
    public static bool GetLastActionResult()
    {
        return lastActionResult;
    }
    
    public static CharacterActionBase GetLastAction()
    {
        return lastAction;
    }
    
    public static string GetLastActionName()
    {
        return GetActionName(lastAction);
    }
}
