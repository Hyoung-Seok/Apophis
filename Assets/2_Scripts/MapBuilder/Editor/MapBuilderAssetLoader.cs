using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public static class MapBuilderAssetLoader
{
    public static IReadOnlyDictionary<string, List<BuilderAssetData>> BuilderAssetData => _builderAssets;
    private static Dictionary<string, List<BuilderAssetData>> _builderAssets = new();
    
    public static void LoadAllAssets(string path)
    {
        var subFolders = AssetDatabase.GetSubFolders(path);

        if (subFolders.Length == 0) return;
        
        _builderAssets.Clear();

        foreach (var subFolder in subFolders)
        {
            var category = Path.GetFileName(subFolder);
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { subFolder });
            _builderAssets.Add(category, new List<BuilderAssetData>());

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var assetName = Path.GetFileNameWithoutExtension(assetPath);
                
                var assetData = new BuilderAssetData(assetPath, assetName);
                _builderAssets[category].Add(assetData);
            }
        }
    }
}
