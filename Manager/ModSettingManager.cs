using Duckov.Modding;
using UnityEngine;
using SodaCraft.Localizations;

namespace ShoulderSurfing
{

    public class ModSettingManager
    {
        public static ModSettingManager Instance;
        public List<AbstractSettingObject> settingObjects = new List<AbstractSettingObject>();

        public bool inited = false;
        public void RegisterConfig()
        {

            if (inited)
            {
                return;
            }
            inited = true;
            Debug.Log("准备添加Modsetting配置项");

            // 根据当前语言设置描述文字
            SystemLanguage[] chineseLanguages = {
                SystemLanguage.Chinese,
                SystemLanguage.ChineseSimplified,
                SystemLanguage.ChineseTraditional
            };

            bool isChinese = chineseLanguages.Contains(LocalizationManager.CurrentLanguage);

            settingObjects = new List<AbstractSettingObject>()
            {
                /*
                new SettingObject<bool>().SetName("ShoulderCameraToggle")
                    .SetDescCN("开关越肩视角")
                    .SetDescEN("Shoulder Camera Toggle")
                    .SetGetValueFunc(() =>
                    {
                        return ShoulderCamera.shoulderCameraToggled;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        ShoulderCamera.shoulderCameraToggled = value;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        // TODO? 因为顺序问题导致的默认为false？
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out bool d1) ? d1 : true;
                        // ShoulderCamera.shoulderCameraToggled = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddToggle(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            thisSettingObject.valueChangeFunc
                        );
                    }),
                */
                new SettingObject<KeyCode>().SetName("ShoulderCameraToggleKey")
                    .SetDescCN("切换视角快捷键")
                    .SetDescEN("Toggle ShoulderCamera Key")
                    .SetGetValueFunc(() =>
                    {
                        return ShoulderCamera.switchShoulderCameraKey;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        ShoulderCamera.switchShoulderCameraKey = value;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : ShoulderCamera.switchShoulderCameraKey;
                        ShoulderCamera.switchShoulderCameraKey = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddKeybinding(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            thisSettingObject.valueChangeFunc
                        );
                    }),
				new SettingObject<KeyCode>().SetName("ShoulderSideKeySingle")
					.SetDescCN("左右肩切换快捷键")
					.SetDescEN("Shoulder Side Switch Key")
					.SetGetValueFunc(() =>
					{
                        return ShoulderCamera.shoulderSideKeySingle;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : ShoulderCamera.shoulderSideKeySingle;
						ShoulderCamera.shoulderSideKeySingle = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.shoulderSideKeySingle = value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddKeybinding(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							thisSettingObject.valueChangeFunc
						);
					}),
				new SettingObject<KeyCode>().SetName("ShoulderLeftSideKey")
					.SetDescCN("左肩切换快捷键")
					.SetDescEN("Shoulder Left Side Key")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.shoulderLeftSideKey;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : ShoulderCamera.shoulderLeftSideKey;
						ShoulderCamera.shoulderLeftSideKey = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.shoulderLeftSideKey = value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddKeybinding(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							thisSettingObject.valueChangeFunc
						);
					}),
				new SettingObject<KeyCode>().SetName("ShoulderRightSideKey")
					.SetDescCN("右肩切换快捷键")
					.SetDescEN("Shoulder Right Side Key")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.shoulderRightSideKey;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : ShoulderCamera.shoulderRightSideKey;
						ShoulderCamera.shoulderRightSideKey = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.shoulderRightSideKey = value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddKeybinding(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							thisSettingObject.valueChangeFunc
						);
					}),
				new SettingObject<int>().SetName("Fov")
					.SetDescCN("视野范围(FOV)")
					.SetDescEN("Field Of View")
					.SetGetValueFunc(() =>
					{
						return (int)ShoulderCamera.FOV;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.FOV = value;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out int d1) ? d1 : ShoulderCamera.FOV;
						ShoulderCamera.FOV = (int)value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddSlider(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							50, 114,
							thisSettingObject.valueChangeFunc
						);
					}),
				new SettingObject<int>().SetName("RenderingDistance")
					.SetDescCN("渲染距离")
					.SetDescEN("Rendering Distance")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.renderDistance;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.renderDistance = value;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out int d1) ? d1 : ShoulderCamera.renderDistance;
						ShoulderCamera.renderDistance = value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddSlider(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							30, 300,
							thisSettingObject.valueChangeFunc
						);
					}),
				new SettingObject<float>().SetName("RecoilMultiplier")
                    .SetDescCN("后坐力大小系数")
                    .SetDescEN("RecoilMultiplier")
                    .SetGetValueFunc(() =>
                    {
                        return InputManagerExtenderCommon.ShoulderRecoilMultiplier;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : InputManagerExtenderCommon.ShoulderRecoilMultiplier;
                        InputManagerExtenderCommon.ShoulderRecoilMultiplier = value;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        InputManagerExtenderCommon.ShoulderRecoilMultiplier = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddSlider(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            new Vector2(0, 1),
                            thisSettingObject.valueChangeFunc,
                            2
                        );
                    }),

                new SettingObject<float>().SetName("MapIndicatorAlpha")
                    .SetDescCN("地图指示器透明度")
                    .SetDescEN("MapIndicatorAlpha")
                    .SetGetValueFunc(() =>
                    {
                        return MiniMapCommon.mapIndicatorAlpha;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : MiniMapCommon.mapIndicatorAlpha;
                        MiniMapCommon.mapIndicatorAlpha = value;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        MiniMapCommon.mapIndicatorAlpha = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddSlider(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            new Vector2(0, 1),
                            thisSettingObject.valueChangeFunc,
                            2
                        );
                    }),

                new SettingObject<float>().SetName("StaminaBarPositionX")
                    .SetDescCN("耐力条X轴位置")
                    .SetDescEN("StaminaBarXAxisPosition")
                    .SetGetValueFunc(() =>
                    {
                        return StaminaHUDExtender.staminaHUDOffset.x;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        StaminaHUDExtender.staminaHUDOffset.x = value;
                        StaminaHUDExtender.UpdateCustomHUDOffset();
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : StaminaHUDExtender.staminaHUDOffset.x;
                        StaminaHUDExtender.staminaHUDOffset.x = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddSlider(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            new Vector2(0, 1),
                            thisSettingObject.valueChangeFunc,
                            3
                        );
                    }),
                new SettingObject<float>().SetName("StaminaBarPositionY")
                    .SetDescCN("耐力条Y轴位置")
                    .SetDescEN("StaminaBarYAxisPosition")
                    .SetValueChangeFunc((value) =>
                    {
                        StaminaHUDExtender.staminaHUDOffset.y = value;
                        StaminaHUDExtender.UpdateCustomHUDOffset();
                    })
                    .SetGetValueFunc(() =>
                    {
                        return StaminaHUDExtender.staminaHUDOffset.y;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : StaminaHUDExtender.staminaHUDOffset.y;
                        StaminaHUDExtender.staminaHUDOffset.y = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddSlider(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            new Vector2(0, 1),
                            thisSettingObject.valueChangeFunc,
                            3
                        );
                    }),

                new SettingObject<float>().SetName("StaminaBarScale")
                    .SetDescCN("耐力条缩放")
                    .SetDescEN("StaminaBarScale")
                    .SetValueChangeFunc((value) =>
                    {
                        StaminaHUDExtender.UpdateCustomHUDScale(value);
                    })
                    .SetGetValueFunc(() =>
                    {
                        return StaminaHUDExtender.staminaHUDScale;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : StaminaHUDExtender.staminaHUDScale;
                        StaminaHUDExtender.staminaHUDScale = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddSlider(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            new Vector2(0, 2),
                            thisSettingObject.valueChangeFunc,
                            2
                        );
                    }),

                new SettingObject<bool>().SetName("MinimapRotationToggle")
                    .SetDescCN("地图旋转开关")
                    .SetDescEN("MinimapRotationToggle")
                    .SetGetValueFunc(() =>
                    {
                        return MiniMapCommon.isMapRotateWithCamera;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        MiniMapCommon.isMapRotateWithCamera = value;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out bool d1) ? d1 : MiniMapCommon.isMapRotateWithCamera;
                        MiniMapCommon.isMapRotateWithCamera = value;
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddToggle(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            thisSettingObject.valueChangeFunc
                        );
                    }),
            };

            try
            {
                foreach (var settingObject in settingObjects)
                {
                    settingObject.Load();
                    settingObject.Register();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{ModBehaviour.MOD_NAME}: 初始化配置失败: {e}");
                // OnDestroy();
            }
        }

        public ModSettingManager()
        {
            if (Instance != null)
            {
                Debug.LogError("ModSettingManager 已实例化");
                return;
            }
            Instance = this;
            // Debug.Log($"{ModBehaviour.MOD_NAME}: ModConfig already available!");
        }


        public void Update()
        {
            if (inited)
                return;
            if (LevelManager.Instance == null)
            {
                return;
            }
            RegisterConfig();
        }
        public void OnDestroy()
        {
            Instance = null;
        }

        public void OnDisable()
        {
            // TODO 取消所有监听
        }
    }
}