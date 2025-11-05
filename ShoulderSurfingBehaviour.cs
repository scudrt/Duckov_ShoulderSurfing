using HarmonyLib;
using System.Reflection;
using Duckov.UI;
using Duckov.MiniMaps;
using Duckov.MiniMaps.UI;

namespace ShoulderSurfing {

	public class ModBehaviour: Duckov.Modding.ModBehaviour {
		const string MOD_ID = "com.didiv.ShoulderSurfing";
		Harmony harmony = new Harmony(MOD_ID);

		void PatchSingleExtender(Type extenderType, Type ExtenderType, string methodName, BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public) {
			MethodInfo originMethod = extenderType.GetMethod(methodName, bindFlags);
			MethodInfo prefix = ExtenderType.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
			MethodInfo postfix = ExtenderType.GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
			harmony.Patch(
				originMethod,
				prefix == null ? null : new HarmonyMethod(prefix),
				postfix == null ? null : new HarmonyMethod(postfix)
			);
		}

		void UnpatchSingleExtender(Type extenderType, string methodName, BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public) {
			MethodInfo originMethod = extenderType.GetMethod(methodName, bindFlags);
			harmony.Unpatch(originMethod, HarmonyPatchType.All, MOD_ID);
		}

		void ApplyHarmonyExtenders() {
			PatchSingleExtender(typeof(InputManager), typeof(InputManagerExtender), "SetAimInputUsingMouse");
			PatchSingleExtender(typeof(HealthBar), typeof(HealthBarUpdatePositionExtender), "UpdatePosition", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(HealthBar), typeof(HealthBarOnTargetDeadExtender), "OnTargetDead", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(HealthBar), typeof(HealthBarOnTargetHurtExtender), "OnTargetHurt", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(MiniMapCompass), typeof(MiniMapCompassExtender), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(MiniMapDisplay), typeof(MiniMapDisplayExtender), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(MiniMapDisplay), typeof(MiniMapDisplaySetupExtender), "Setup");
			PatchSingleExtender(typeof(MiniMapView), typeof(MiniMapViewOnSetZoomExtender), "OnSetZoom", BindingFlags.Instance | BindingFlags.NonPublic);
			// PatchSingleExtender(typeof(Projectile), typeof(ProjectileExtender), "Init");
		}
		void CancelHarmonyExtender() {
			UnpatchSingleExtender(typeof(InputManager), "SetAimInputUsingMouse");
			UnpatchSingleExtender(typeof(HealthBar), "UpdatePosition", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(HealthBar), "OnTargetDead", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(HealthBar), "OnTargetHurt", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(MiniMapCompass), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(MiniMapDisplay), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(MiniMapDisplay), "Setup");
			UnpatchSingleExtender(typeof(MiniMapView), "OnSetZoom", BindingFlags.Instance | BindingFlags.NonPublic);
			// UnpatchSingleExtender(typeof(Projectile), "Init");
		}
		void Awake() {
		}

		void OnEnable() {
			ApplyHarmonyExtenders();

			ShoulderCamera.EnableTPSMode(this.gameObject);
		}

		void OnDisable() {
			CancelHarmonyExtender();

			ShoulderCamera.DisableTPSMode(this.gameObject);
		}
	}
}
