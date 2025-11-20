# Mod Introduction
Escape From Tarkov Third Person Over-the-Shoulder View Mod
Transforms Escape From Tarkov into an over-the-shoulder shooter game

# Feature List
1. Converts the game camera to a third-person over-the-shoulder perspective, allowing adjustment of camera offset, FOV, render distance, and other parameters

2. Supports 360-degree free aiming, Y-axis inversion, and shoulder switching

3. Features smooth camera collision handling and character fade-out to prevent occlusion, providing better visual experience

4. UI display optimized for over-the-shoulder view, including directional sound indicators, dynamic health bars, stamina bars, and透视开关 effects

5. Over-the-shoulder sensitivity is linked to the original game sensitivity with adjustable coefficients

6. Supports different scope magnification levels that correspond to original scope and weapon settings

7. Implements recoil mechanics derived from the original game, currently supporting vertical recoil only

8. Simple minimap based on original game maps, with adjustable position and size, and ability to rotate with player perspective

9. Supports custom hotkeys for toggling perspective, minimap, shoulder switching, and other operations

10. Additional minor features available in MOD settings (requires MODSetting as prerequisite)

# How to Build
1. Clone this source code

2. Refer to the official Tarkov documentation to set up the MOD compilation environment (or open the project folder in VS Code with .NET extension enabled, which will prompt and help you set up the required environment)

3. Modify the .csproj file by changing the DuckovPath entry to your local game directory path

4. Build using Release configuration (when opening the project in VSCode, you need to manually run the "dotnet build --configuration Release" command or use the tasks.json file for automatic building)

5. After successful build, the compiled Over-the-Shoulder MOD will automatically appear in the Mods folder of your game directory. Launch the game and you can find this MOD in the MODS menu

6. This MOD's settings functionality depends on the MODSetting mod. Without it, the MOD settings feature will not be available