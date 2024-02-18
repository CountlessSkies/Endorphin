using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class FBXMaterialProcess : AssetPostprocessor
{
    public string pathWithoutFileName;
    void OnPreprocessModel()
    {
        ModelImporter importer = (ModelImporter)assetImporter;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
    }

    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        if (IsMetallicOrRoughness())
        {
            importer.isReadable = true;
        }
        else if (IsNormal())
        {
            importer.textureType = TextureImporterType.NormalMap;
        }
        else
        {
            return;
        }
    }


    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string str in importedAssets)
        {
            if (str.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                // Load the FBX asset
                GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(str);

                if (fbxObject != null)
                {
                    // Keep track of processed materials to avoid duplicate processing
                    HashSet<Material> processedMaterials = new();

                    // Process the FBX object and its materials
                    foreach (MeshRenderer renderer in fbxObject.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        // Access the materials of each mesh renderer
                        Material[] materials = renderer.sharedMaterials;

                        foreach (Material material in materials)
                        {
                            // Add the material to the set of processed materials
                            processedMaterials.Add(material);
                        }
                    }

                    foreach (Material material in processedMaterials)
                    {
                        // Process each material as needed
                        Texture2D baseMap = material.GetTexture("_BaseMap") as Texture2D;
                        string baseMapPath = AssetDatabase.GetAssetPath(baseMap);
                        string metallicMapPath = baseMapPath.Replace("basecolor", "metallic").Replace("BaseColor", "Metallic").Replace("diffuse", "metallic").Replace("Diffuse", "Metallic");
                        string roughnessMapPath = baseMapPath.Replace("basecolor", "roughness").Replace("BaseColor", "Roughness").Replace("diffuse", "roughness").Replace("Diffuse", "Roughness");

                        if (baseMapPath.IndexOf("BaseColor", StringComparison.OrdinalIgnoreCase) != -1 || baseMapPath.IndexOf("Diffuse", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            Texture2D metallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicMapPath);
                            Texture2D roughnessMap = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessMapPath);

                            // Debug.Log(metallicMap);
                            // Debug.Log(roughnessMap);

                            if (metallicMap == null)
                            {
                                metallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicMapPath.Replace("jpg", "png"));
                                if (metallicMap == null)
                                {
                                    metallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicMapPath.Replace("png", "jpg"));
                                }
                            }
                            if (roughnessMap == null)
                            {
                                roughnessMap = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessMapPath.Replace("jpg", "png"));
                                if (roughnessMap == null)
                                {
                                    roughnessMap = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessMapPath.Replace("png", "jpg"));
                                }
                            }

                            if (metallicMap != null && roughnessMap != null)
                            {
                                CombineMetallicRoughness(metallicMap, roughnessMap, out string combinedPath);
                                Texture2D newMetallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(combinedPath);

                                material.SetTexture("_MetallicGlossMap", newMetallicMap);
                                material.SetFloat("_Smoothness", 1.0f);
                                material.SetFloat("_Cull", 0);
                            }
                        }
                    }
                }
            }
        }

        static void CombineMetallicRoughness(Texture2D metallicMap, Texture2D roughnessMap, out string combinedPath)
        {
            // Pixel processing
            Color32[] roughnessPixels = roughnessMap.GetPixels32();
            for (int i = 0; i < roughnessPixels.Length; i++)
            {
                roughnessPixels[i].r = (byte)(255 - roughnessPixels[i].r);
                roughnessPixels[i].g = (byte)(255 - roughnessPixels[i].g);
                roughnessPixels[i].b = (byte)(255 - roughnessPixels[i].b);
            }

            Color32[] metallicPixels = metallicMap.GetPixels32();
            for (int i = 0; i < metallicPixels.Length; i++)
            {
                metallicPixels[i].a = roughnessPixels[i].r;
            }

            Texture2D MetallicRoughnessCombined = new(metallicMap.width, metallicMap.height, TextureFormat.ARGB32, false);
            MetallicRoughnessCombined.SetPixels32(metallicPixels);

            byte[] imageTexture = MetallicRoughnessCombined.EncodeToPNG();

            string pathWithoutFileName = Path.GetDirectoryName(AssetDatabase.GetAssetPath(metallicMap));
            string baseFileName = metallicMap.name.Replace("Metallic", "MetallicSmoothness").Replace("metallic", "MetallicSmoothness");
            string extension = Path.GetExtension(AssetDatabase.GetAssetPath(metallicMap));

            combinedPath = string.Format("{0}/{1}{2}", pathWithoutFileName, baseFileName, extension);

            File.WriteAllBytes(combinedPath, imageTexture);
            AssetDatabase.ImportAsset(combinedPath);
        }
    }


    private bool IsMetallicOrRoughness()
    {
        string fileName = Path.GetFileNameWithoutExtension(assetPath);

        return fileName.Contains("Metallic") || fileName.Contains("metallic") || fileName.Contains("Roughness") || fileName.Contains("roughness");
    }


    private bool IsNormal()
    {
        string fileName = Path.GetFileNameWithoutExtension(assetPath);

        return fileName.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) != -1;
    }
}