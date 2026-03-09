using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuilderAssetLoader
{
    public static Dictionary<string, List<AssetData>> BuilderAssets => _builderAssets;
    
    private static Dictionary<string, List<AssetData>> _builderAssets 
        = new Dictionary<string, List<AssetData>>();
    
    private const string ROOT_PATH = "Assets/3_Prefabs/MapBuilder";
    
    public static List<string> GetAssetCategories()
    {
        if (_builderAssets.Count == 0)
        {
            LoadAllAssets();
        }
        
        return _builderAssets.Keys.ToList();
    }

    private static void LoadAllAssets()
    {
        _builderAssets.Clear();
        
        var categories = Directory.GetDirectories(ROOT_PATH)
            .Select(Path.GetFileName).ToList();

        foreach (var category in categories)
        {
            var guids = AssetDatabase.FindAssets("t:prefab", new[] {Path.Combine(ROOT_PATH, category)});
            _builderAssets[category] = new List<AssetData>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);
                
                _builderAssets[category].Add(new AssetData(name, path));
            }
        }
    }
}
