## Simple Addon / Editor combo to export FBX from Blender with texture and generate into a prefab in Unity3D directly.

## How to use:

### Blender:
1. Open Blender
2. Select Edit -> Preferences (or use CTRL + somewhere in the scene)
3. Select Addons
4. Click the arrow on the top right and select "Install from disk..."
![Screenshot1](https://lemonspurple.github.io/various_graphics/screenshot_1_btu.png)
5. Select the .zip (Don't unpack it)
6. Search for Unity FBX Exporter to check if it is enabled.
7. Close Preferences
8. On right side of the Scene below Items, Tools, View, you should see Export FBX + Texture. If not, press N.
![Screenshot2](https://lemonspurple.github.io/various_graphics/screenshot_2_btu.png)
9. Select a Path where you want to extract the FBX to, give it a name (otherwise the selected model name is used)
10. Check only export selected (If weird stuff occurs it's probabyl because it's unchecked)
11. Export textures if wished
12. Ignore scale and rotation sets Scale to 1,1,1 and rotation to 0,0,0 before export.
13. Click export

### Unity
1. Create folder named Editor in lowest hirarchy
2. Copy and paste FbxPrefabImporter.cs into it
3. In the top row, you should now see a tab called Tools
4. Click FBX Prefab Importer
![Screenshot3](https://lemonspurple.github.io/various_graphics/screenshot_3_btu.png)
5. Select folder
6. Upon opening your assets are imported into "ImportedFBX" in the base hirarchy level and a prefab is created. The .JSON is deleted in the Unity project.

Stuff to do:
- Add functionality for multiple fbx imports
- Add flexiblity to set custom paths for generating
- Write options for alternative shaders (Only Universal Render Pipeline/Lit is supported as of now)
