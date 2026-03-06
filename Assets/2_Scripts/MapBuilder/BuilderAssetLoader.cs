using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuilderAssetLoader
{
    public static Dictionary<string, List<string>> BuilderAssets => _builderAssets;
    
    private static Dictionary<string, List<string>> _builderAssets 
        = new Dictionary<string, List<string>>();
    
    private const string ROOT_PATH = "Assets/3_Prefabs/MapBuilder";

    public static void LoadAllAssets()
    {
        _builderAssets.Clear();
        
        var categories = Directory.GetDirectories(ROOT_PATH)
            .Select(Path.GetFileName).ToList();

        foreach (var category in categories)
        {
            var guids = AssetDatabase.FindAssets("t:prefab", new[] {ROOT_PATH + "/" +  category});
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            _builderAssets[category] = paths;
        }
    }

    public static List<string> GetAssetCategories()
    {
        if (_builderAssets.Count == 0)
        {
            LoadAllAssets();
        }
        
        return _builderAssets.Keys.ToList();
    }
    
}
