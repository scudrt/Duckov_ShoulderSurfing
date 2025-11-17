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
				new SettingObject<bool>().SetName("InvertYAxis")
					.SetDescCN("Y轴反转")
					.SetDescEN("Invert Y Axis")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.invertYAxis;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.invertYAxis = value;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out bool d1) ? d1 : false;
                        ShoulderCamera.invertYAxis = value;
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
				new SettingObject<float>().SetName("ShoulderMouseSensitive")
					.SetDescCN("鼠标灵敏度系数(常规状态)")
					.SetDescEN("Mouse Sensitive Rate(Normal)")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.mouseSensitivityRate;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : ShoulderCamera.mouseSensitivityRate;
						ShoulderCamera.mouseSensitivityRate = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.mouseSensitivityRate = value;
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
				new SettingObject<float>().SetName("ShoulderMouseSensitiveADS")
					.SetDescCN("鼠标灵敏度系数(开镜状态)")
					.SetDescEN("Mouse Sensitive Rate(ADS)")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.mouseSensitivityRateADS;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : ShoulderCamera.mouseSensitivityRateADS;
						ShoulderCamera.mouseSensitivityRateADS = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.mouseSensitivityRateADS = value;
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
				new SettingObject<float>().SetName("CameraOffsetLeftRight")
					.SetDescCN("相机左/右偏移量")
					.SetDescEN("Camera Offset Left/Right")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.shoulderCameraOffsetX;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : ShoulderCamera.shoulderCameraOffsetX;
						ShoulderCamera.shoulderCameraOffsetX = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.shoulderCameraOffsetX = value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddSlider(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							new Vector2(0, 3),
							thisSettingObject.valueChangeFunc,
							2
						);
					}),
				new SettingObject<float>().SetName("CameraOffsetUp")
					.SetDescCN("相机上偏移量")
					.SetDescEN("Camera Offset Up")
					.SetGetValueFunc(() =>
					{
						return ShoulderCamera.shoulderCameraOffsetY;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : ShoulderCamera.shoulderCameraOffsetY;
						ShoulderCamera.shoulderCameraOffsetY = value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.shoulderCameraOffsetY = value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddSlider(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							new Vector2(0, 3),
							thisSettingObject.valueChangeFunc,
							2
						);
					}),
				new SettingObject<float>().SetName("CameraOffsetBackward")
					.SetDescCN("相机后偏移量")
					.SetDescEN("Camera Offset Backward")
					.SetGetValueFunc(() =>
					{
						return -ShoulderCamera.shoulderCameraOffsetZ;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : -ShoulderCamera.shoulderCameraOffsetZ;
						ShoulderCamera.shoulderCameraOffsetZ = -value;
					})
					.SetValueChangeFunc((value) =>
					{
						ShoulderCamera.shoulderCameraOffsetZ = -value;
					})
					.SetRegisterFunc((thisSettingObject) =>
					{
						ModSettingAPI.AddSlider(
							thisSettingObject.name,
							isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
							thisSettingObject.getValueFunc(),
							new Vector2(0, 6),
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
                        StaminaHUDExtender.UpdateCustomHUDOffset(true);
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
                        StaminaHUDExtender.UpdateCustomHUDOffset(true);
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
                        StaminaHUDExtender.UpdateCustomHUDScale(value, true);
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
                // 自定义小地图相关
                new SettingObject<bool>().SetName("CustomMinimapOpen")
                    .SetDescCN("是否启用小地图功能")
                    .SetDescEN("Minimap Open")
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.isEnabled;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.Enable(value);
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out bool d1) ? d1 : CustomMinimapManager.isEnabled;
                        CustomMinimapManager.Enable(value);
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
                new SettingObject<KeyCode>().SetName("CustomMinimapToggle")
                    .SetDescCN("小地图临时开关快捷键")
                    .SetDescEN("Minimap Toggle Key")
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.MinimapToggleKey;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.MinimapToggleKey = value;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : CustomMinimapManager.MinimapToggleKey;
                        CustomMinimapManager.MinimapToggleKey = value;
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
                new SettingObject<float>().SetName("CustomMinimapContainerScale")
                    .SetDescCN("小地图容器缩放")
                    .SetDescEN("Minimap Container Scale")
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.minimapContainerSizeScale;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.SetMinimapContainerScale(value);
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : CustomMinimapManager.minimapContainerSizeScale;
                        CustomMinimapManager.minimapContainerSizeScale = value;
                        CustomMinimapManager.SetMinimapContainerScale(value);
                    })
                    .SetRegisterFunc((thisSettingObject) =>
                    {
                        ModSettingAPI.AddSlider(
                            thisSettingObject.name,
                            isChinese ? thisSettingObject.descCN : thisSettingObject.descEN,
                            thisSettingObject.getValueFunc(),
                            new Vector2(0f, 3f),
                            thisSettingObject.valueChangeFunc,
                            2
                        );
                    }),
                new SettingObject<KeyCode>().SetName("CustomMinimapInnerScaleUp")
                    .SetDescCN("小地图放大快捷键")
                    .SetDescEN("Minimap Scale Up Key")
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.displayZoomUpKey;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.displayZoomUpKey = value;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : CustomMinimapManager.displayZoomUpKey;
                        CustomMinimapManager.displayZoomUpKey = value;
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
                new SettingObject<KeyCode>().SetName("CustomMinimapInnerScaleDown")
                    .SetDescCN("小地图缩小快捷键")
                    .SetDescEN("Minimap Scale Down Key")
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.displayZoomDownKey;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.displayZoomDownKey = value;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out KeyCode d1) ? d1 : CustomMinimapManager.displayZoomDownKey;
                        CustomMinimapManager.displayZoomDownKey = value;
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

                new SettingObject<float>().SetName("CustomMinimapPositionX")
                    .SetDescCN("小地图X轴位置")
                    .SetDescEN("MinimapXAxisPosition")
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.miniMapPositionOffset.x;
                    })
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.miniMapPositionOffset.x = value;
                        CustomMinimapManager.SetMinimapPosition();
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : CustomMinimapManager.miniMapPositionOffset.x;
                        CustomMinimapManager.miniMapPositionOffset.x = value;
                        CustomMinimapManager.SetMinimapPosition();
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
                new SettingObject<float>().SetName("CustomMinimapPositionY")
                    .SetDescCN("小地图Y轴位置")
                    .SetDescEN("MinimapYAxisPosition")
                    .SetValueChangeFunc((value) =>
                    {
                        CustomMinimapManager.miniMapPositionOffset.y = value;
                        CustomMinimapManager.SetMinimapPosition();
                    })
                    .SetGetValueFunc(() =>
                    {
                        return CustomMinimapManager.miniMapPositionOffset.y;
                    })
                    .SetLoadFunc((thisSettingObject) =>
                    {
                        var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out float d1) ? d1 : CustomMinimapManager.miniMapPositionOffset.y;
                        CustomMinimapManager.miniMapPositionOffset.y = value;
                        CustomMinimapManager.SetMinimapPosition();
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
				new SettingObject<bool>().SetName("WallHacking")
					.SetDescCN("穿墙透视(切换地图后生效)")
					.SetDescEN("Wall Hacking(take effects after switch maps)")
					.SetGetValueFunc(() =>
					{
						return CharacterMainControlExtender.enableWallHacking;
					})
					.SetValueChangeFunc((value) =>
					{
						CharacterMainControlExtender.enableWallHacking = value;
					})
					.SetLoadFunc((thisSettingObject) =>
					{
						var value = ModSettingAPI.GetSavedValue(thisSettingObject.GetName(), out bool d1) ? d1 : false;
						CharacterMainControlExtender.enableWallHacking = value;
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
            if (!LevelManager.LevelInited)
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