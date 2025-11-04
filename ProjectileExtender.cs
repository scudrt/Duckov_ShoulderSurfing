using Duckov.MiniMaps;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[HarmonyPatch(typeof(Projectile))]
[HarmonyPatch("Init")]
public static class ProjectileExtender {
	public static bool Prefix(Projectile __instance, ref ProjectileContext _context) {
		Debug.Log("projectile forward and context direction:" + __instance.transform.forward + " " + _context.direction);
		return true;
	}
}
