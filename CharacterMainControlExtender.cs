using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(CharacterMainControl))]
[HarmonyPatch("SetCharacterModel")]
public static class CharacterMainControlExtender {
	public static bool enableWallHacking = false; // 是否启用透视
	public static Shader? showBackShader = null;
	public static void Postfix(CharacterMainControl __instance) {
		if (showBackShader == null) {
			showBackShader = Shader.Find("CharacterShowBack");
		}
		if (enableWallHacking) {
			return;
		}

		foreach (var renderer in __instance.characterModel.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			foreach (var mat in renderer.materials) {
				if (!mat || mat.shader != showBackShader) {
					continue;
				}
				mat.renderQueue = 0;
				return; // Target material found, return directly
			}
		}
	}
}
