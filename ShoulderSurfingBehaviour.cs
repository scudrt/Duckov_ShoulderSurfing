using HarmonyLib;
using System.Reflection;
using Duckov.UI;
using Duckov.MiniMaps;
using Duckov.MiniMaps.UI;
using Duckov.MiniGames;
using UnityEngine;
using Duckov.Modding;

namespace ShoulderSurfing {

	public class ModBehaviour: Duckov.Modding.ModBehaviour {
		const string MOD_ID = "com.didiv.ShoulderSurfing";
		
		public static string MOD_NAME = "Shoulder Surfing";

		Harmony harmony = new Harmony(MOD_ID);
		public static ModBehaviour Instance;

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
			PatchSingleExtender(typeof(HealthBar), typeof(HealthBarCheckInFrameExtender), "CheckInFrame", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(MiniMapCompass), typeof(MiniMapCompassExtender), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(MiniMapDisplay), typeof(MiniMapDisplayExtender), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(MiniMapDisplay), typeof(MiniMapDisplaySetupExtender), "Setup");
			PatchSingleExtender(typeof(MiniMapView), typeof(MiniMapViewOnSetZoomExtender), "OnSetZoom", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(StaminaHUD), typeof(StaminaHUDExtender), "Update", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(GamingConsole), typeof(MiniGameStartExtender), "OnInteractStart", BindingFlags.Instance | BindingFlags.NonPublic);
			PatchSingleExtender(typeof(GamingConsole), typeof(MiniGameEndExtender), "OnInteractStop", BindingFlags.Instance | BindingFlags.NonPublic);
			// PatchSingleExtender(typeof(Projectile), typeof(ProjectileExtender), "Init");
		}
		void CancelHarmonyExtender() {
			UnpatchSingleExtender(typeof(InputManager), "SetAimInputUsingMouse");
			UnpatchSingleExtender(typeof(HealthBar), "UpdatePosition", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(HealthBar), "OnTargetDead", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(HealthBar), "OnTargetHurt", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(HealthBar), "CheckInFrame", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(MiniMapCompass), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(MiniMapDisplay), "SetupRotation", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(MiniMapDisplay), "Setup");
			UnpatchSingleExtender(typeof(MiniMapView), "OnSetZoom", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(StaminaHUD), "Update", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(GamingConsole), "OnInteractStart", BindingFlags.Instance | BindingFlags.NonPublic);
			UnpatchSingleExtender(typeof(GamingConsole), "OnInteractStop", BindingFlags.Instance | BindingFlags.NonPublic);
			// UnpatchSingleExtender(typeof(Projectile), "Init");
		}
		void Awake()
        {
			if (Instance != null)
			{
				Debug.LogError("ModBehaviour 已实例化");
				return;
			}
			Instance = this;
        }

		void OnEnable() {
			ApplyHarmonyExtenders();
			ShoulderCamera.EnableTPSMode(this.gameObject);
            ModManager.OnModActivated += ModManager_OnModActivated;
            ModManager.OnModWillBeDeactivated += ModManager_OnModWillBeDeactivated;
		}

		void Start()
		{
		}

		void OnDisable()
		{
			CancelHarmonyExtender();
			StaminaHUDExtender.Unload();
			ShoulderCamera.DisableTPSMode(this.gameObject);
            ModManager.OnModActivated -= ModManager_OnModActivated;
            ModManager.OnModWillBeDeactivated -= ModManager_OnModWillBeDeactivated;
		}

		//下面两个函数需要实现，实现后的效果是：ModSetting和mod之间不需要启动顺序，两者无论谁先启动都能正常添加设置
		private void ModManager_OnModActivated(ModInfo arg1, Duckov.Modding.ModBehaviour arg2)
		{
			//(触发时机:此mod在ModSetting之前启用)检查启用的mod是否是ModSetting,是进行初始化
			if (arg1.name != ModSettingAPI.MOD_NAME || !ModSettingAPI.Init(info)) return;
				new ModSettingManager();
		}

		private void ModManager_OnModWillBeDeactivated(ModInfo arg1, Duckov.Modding.ModBehaviour arg2)
		{
			if (arg1.name != ModSettingAPI.MOD_NAME || !ModSettingAPI.Init(info)) return;
			//禁用ModSetting的时候移除监听
			if (ModSettingManager.Instance != null)
				ModSettingManager.Instance.OnDisable();
		}

		protected override void OnAfterSetup()
		{
			//(触发时机:此mod在ModSetting之后启用)此mod，Setup后,尝试进行初始化
			if (ModSettingAPI.Init(info))
				new ModSettingManager();
		}
		
		void Update()
        {
            if (ModSettingManager.Instance != null)
                ModSettingManager.Instance.Update();
        }
	}
}
