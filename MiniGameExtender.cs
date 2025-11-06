using HarmonyLib;
using Duckov.MiniGames;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.UI;
using ShoulderSurfing;

[HarmonyPatch(typeof(GamingConsole))]
[HarmonyPatch("OnInteractStart")]
public static class MiniGameStartExtender {
	public static void Postfix(GamingConsole __instance) {
		ShoulderCamera.isMiniGameEnabled = __instance.Game != null;
	}
}

[HarmonyPatch(typeof(GamingConsole))]
[HarmonyPatch("OnInteractStop")]
public static class MiniGameEndExtender {
	public static void Postfix(GamingConsole __instance) {
		ShoulderCamera.isMiniGameEnabled = false;
	}
}
