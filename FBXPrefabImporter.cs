#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

public class FBXPrefabImporter : EditorWindow
{
    [System.Serializable] public class Root { public Settings settings; public FbxObject[] objects; }
    [System.Serializable] public class Settings { public bool export_textures; public bool ignore_transform; }

    [System.Serializable] public class FbxObject
    {
        public string name;
        public float[] location;
        public float[] rotation;
        public float[] scale;
        public MaterialData[] materials;
    }

    [System.Serializable]
    public class MaterialData
    {
        public string name;
        public TextureMap textures;
    }

    [System.Serializable]
    public class TextureMap
    {
        public string base_color;
        public string normal;
        public string roughness;
        public string metallic;
        public string emission;
        public string alpha;
        public string displacement;
        public string ambient_occlusion;
    }

    private string selectedFolder = "";
    private Texture2D bannerTexture;
    private const string bannerURL = "https://lemonspurple.github.io/images/exporter_banner.png";

    [MenuItem("Tools/FBX Prefab Importer")]
    public static void ShowWindow()
    {
        GetWindow<FBXPrefabImporter>("FBX Prefab Importer");
    }

    private void OnEnable()
    {
        LoadBannerImage(bannerURL);
    }

    private void LoadBannerImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        var async = www.SendWebRequest();

        void CheckProgress()
        {
            if (!async.isDone) return;

            EditorApplication.update -= CheckProgress;

            if (www.result == UnityWebRequest.Result.Success)
            {
                bannerTexture = DownloadHandlerTexture.GetContent(www);
                Repaint();
            }
            else
            {
                Debug.LogWarning("Banner load failed: " + www.error);
            }
        }

        EditorApplication.update += CheckProgress;
    }

    private void OnGUI()
    {
        if (bannerTexture)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(bannerTexture, GUILayout.Width(200), GUILayout.Height(35));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("Import FBX with JSON and Textures", EditorStyles.boldLabel);

        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Choose FBX Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                selectedFolder = path;
                ImportFromFolder(selectedFolder);
            }
        }

        if (!string.IsNullOrEmpty(selectedFolder))
        {
            GUILayout.Label("Selected Folder:", EditorStyles.label);
            GUILayout.TextField(selectedFolder);
        }
    }

    private void ImportFromFolder(string folderPath)
    {
        string assetSubDir = "ImportedFBX";
        string targetFolderInAssets = Path.Combine("Assets", assetSubDir);

        if (!AssetDatabase.IsValidFolder(targetFolderInAssets))
        {
            AssetDatabase.CreateFolder("Assets", assetSubDir);
        }

        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx");

        foreach (string fbxFile in fbxFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(fbxFile);
            string jsonPath = Path.Combine(folderPath, fileName + ".json");

            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning($"No JSON found for {fileName}");
                continue;
            }

            string destFbxPath = Path.Combine(targetFolderInAssets, fileName + ".fbx");
            string destJsonPath = Path.Combine(targetFolderInAssets, fileName + ".json");

            File.Copy(fbxFile, destFbxPath, true);
            File.Copy(jsonPath, destJsonPath, true);

            string[] textureFiles = Directory.GetFiles(folderPath, "*.png");
            foreach (string tex in textureFiles)
            {
                string fileNameTex = Path.GetFileName(tex);
                string destTex = Path.Combine(targetFolderInAssets, fileNameTex);
                File.Copy(tex, destTex, true);

                string relTexPath = Path.Combine(targetFolderInAssets, fileNameTex).Replace("\\", "/");
                AssetDatabase.ImportAsset(relTexPath, ImportAssetOptions.ForceUpdate);

                if (fileNameTex.ToLower().Contains("normal"))
                {
                    TextureImporter importer = AssetImporter.GetAtPath(relTexPath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        importer.SaveAndReimport();
                    }
                }
            }

            AssetDatabase.Refresh();

            string relativePath = targetFolderInAssets + "/";
            string prefabPath = relativePath + fileName + ".prefab";

            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath + fileName + ".fbx");
            if (fbxAsset == null)
            {
                Debug.LogWarning($"FBX not found in Assets: {relativePath + fileName}.fbx");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);

            string json = File.ReadAllText(destJsonPath);
            Root root = JsonUtility.FromJson<Root>(json);
            if (root.objects.Length == 0) continue;

            var obj = root.objects[0];
            Transform t = instance.transform;

            t.localPosition = ToVector3(obj.location);
            t.localEulerAngles = ToVector3(obj.rotation);
            t.localScale = ToVector3(obj.scale);

            List<Material> generatedMaterials = new List<Material>();

            if (obj.materials.Length > 0)
            {
                foreach (var matData in obj.materials)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null)
                    {
                        Debug.LogError("URP Lit shader not found. Ensure URP is active.");
                        continue;
                    }

                    Material mat = new Material(shader);
                    mat.name = matData.name;

                    if (matData.textures != null)
                    {
                        if (!string.IsNullOrEmpty(matData.textures.base_color))
                        {
                            string texPath = relativePath + matData.textures.base_color;
                            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                            if (texture) mat.SetTexture("_BaseMap", texture);
                        }

                        if (!string.IsNullOrEmpty(matData.textures.normal))
                        {
                            string texPath = relativePath + matData.textures.normal;
                            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                            if (texture) mat.SetTexture("_BumpMap", texture);
                        }

                        if (!string.IsNullOrEmpty(matData.textures.roughness))
                        {
                            string texPath = relativePath + matData.textures.roughness;
                            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                            if (texture) mat.SetTexture("_SmoothnessTextureChannel", texture);
                        }

                        if (!string.IsNullOrEmpty(matData.textures.metallic))
                        {
                            mat.SetFloat("_Metallic", 1f);
                        }

                        if (!string.IsNullOrEmpty(matData.textures.emission))
                        {
                            string texPath = relativePath + matData.textures.emission;
                            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                            if (texture)
                            {
                                mat.EnableKeyword("_EMISSION");
                                mat.SetTexture("_EmissionMap", texture);
                            }
                        }

                        if (!string.IsNullOrEmpty(matData.textures.alpha))
                        {
                            mat.SetFloat("_Surface", 1);
                            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            mat.SetInt("_ZWrite", 0);
                            mat.DisableKeyword("_ALPHATEST_ON");
                            mat.EnableKeyword("_ALPHABLEND_ON");
                            mat.renderQueue = 3000;
                        }
                    }

                    string matAssetPath = relativePath + mat.name + ".mat";
                    AssetDatabase.CreateAsset(mat, matAssetPath);
                    generatedMaterials.Add(mat);
                }

                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.sharedMaterials = generatedMaterials.ToArray();
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            DestroyImmediate(instance);

            if (File.Exists(destJsonPath))
            {
                AssetDatabase.DeleteAsset(destJsonPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Created prefab: {prefabPath} :DDDDD");
        }
    }

    private Vector3 ToVector3(float[] arr)
    {
        if (arr.Length != 3) return Vector3.zero;
        return new Vector3(arr[0], arr[1], arr[2]);
    }
}
#endif
