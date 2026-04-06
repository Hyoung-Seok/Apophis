using UnityEngine;
using UnityEditor;

public static class LevelDataIO
{
    private const string DEFAULT_PATH = "Assets/7_Data/MapBuilder/LevelData";

    public static bool Save(MapBuilder mapBuilder, string levelName)
    {
        var path = $"{DEFAULT_PATH}/{levelName}.asset";
        var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        if (data != null)
        {
            if (EditorUtility.DisplayDialog(
                    "덮어쓰기 확인", $"'{levelName}'이 이미 존재합니다. 덮어쓰시겠습니까?",
                    "덮어쓰기", "취소") == false)
            {
                return false;
            }
        }
        else
        {
            data = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(data, path);
        }
        
        data.SetData(mapBuilder, levelName);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        return true;
    }
}
