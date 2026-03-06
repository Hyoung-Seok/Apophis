using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuilderAssetLoader
{
    private const string ROOT_PATH = "Assets/3_Prefabs/MapBuilder";

    public static List<string> GetAssetCategories()
    {
        return Directory.GetDirectories(ROOT_PATH)
            .Select(Path.GetFileName).ToList();
    }
    
}
