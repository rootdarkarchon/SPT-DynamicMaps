using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildDynamicMapsCompositeBundle
{
    public static void Build()
    {
        var outputDir = Path.GetFullPath("AssetBundles");
        Directory.CreateDirectory(outputDir);

        const string shaderPath = "Assets/DynamicMaps/DynamicMapsPremultipliedUI.shader";
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        if (shader == null)
        {
            Debug.LogError($"Could not load DynamicMaps composite shader: {shaderPath}");
            EditorApplication.Exit(1);
            return;
        }

        var build = new AssetBundleBuild
        {
            assetBundleName = "dynamicmaps-composite",
            assetNames = new[] { shaderPath }
        };

        var manifest = BuildPipeline.BuildAssetBundles(
            outputDir,
            new[] { build },
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            Debug.LogError("BuildPipeline.BuildAssetBundles returned null.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Built DynamicMaps composite bundle to: {outputDir}");
    }
}
