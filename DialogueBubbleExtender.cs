using Duckov.UI;
using Duckov.UI.DialogueBubbles;
using HarmonyLib;
using ShoulderSurfing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;

[HarmonyPatch(typeof(DialogueBubble))]
[HarmonyPatch("UpdatePosition")]
public class DialogueBubbleExtender {
	static FieldInfo textField;
	public static void Postfix(DialogueBubble __instance) {
		if (textField == null) {
			textField = typeof(DialogueBubble).GetField("text");
		}
		if (ShoulderCamera.shoulderCameraToggled == false) {
			return;
		}

		TextMeshProUGUI text = (TextMeshProUGUI)textField.GetValue(__instance);
		bool shouldShowBubble = __instance.transform.position.z > 0f;
		text.enabled = shouldShowBubble;
	}
}

