# FBX to Prefab ([Download](https://github.com/lemonspurple/blender_to_unity/releases))
Small Add-on / Tool combo to export .fbx with textures and generate a prefab in Unity.

### Setup:

## Install Blender Add-on
1. Download Blender_Export_To_Unity.zip **and do not unpack it.**
2. Open Blender -> Edit -> Preferences (or CTRL + ,)
3. Navigate to Add-ons on the left column. Top right has a small arrow that you click.

![Screenshot 1](https://lemonspurple.github.io/various_graphics/screenshot_1_btu.png)

4. Select Install from disk and navigate to Blender_Export_To_Unity.zip. Click open.
5. Search Unity FBX Exporter to validate it's enabled in Add-ons. Close Preferences.

## Export in Blender
6. Select 'Export FBX + Texture' on the right side of the UI. If it's hidden, left click into the scene and press 'N'.

![Screenshot 2](https://lemonspurple.github.io/various_graphics/screenshot_2_btu.png)

7. Select options
- Path: Specify where data will be exported to.
- Name: Specify name of the fbx + .JSON. If left empty, the 3D objects name is used.
- Only export selected: If deselected, everything will be exported. **This can cause bugs in Unity (i.E. if you accidentally export lights, cameras etc.).**
- Export textures: Saves out all textures if enabled.
- Ignore scale and rotation: Sets model to 1,1,1 in scale and 0,0,0 on rotation.
8. Click export.

## Install Tool in Unity

![Screenshot 3](https://lemonspurple.github.io/various_graphics/screenshot_3_btu.png)

1. Create folder called Editor in lowest hirarchy.
2. Copy FBXPrefabImporter.cs into Editor folder.
3. Open Tools tab.
4. Select FBX Prefab Importer and open the folder you exported your dataset into with the Blender Add-on earlier.
5. Your Prefab was built in the ImportedFBX folder (lowers hirarchy). The textures and materials can be also found there.


## To do:
- Extend shader options. Only works with Universal Render Pipeline/Lit at the moment.
- Allow user to adjust paths for where prefabs, materials and textures land.
- Batch support
- Test on different blender constellations
