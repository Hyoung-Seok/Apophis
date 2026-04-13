using System.Linq;
using UnityEngine;
using UnityEditor;

public static class LevelDataIO
{
    public const string DEFAULT_PATH = "Assets/7_Data/MapBuilder/LevelData";

    public static ESaveResult Save(MapBuilder mapBuilder, string levelName)
    {
        var path = $"{DEFAULT_PATH}/{levelName}.asset";
        var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        if (data != null)
        {
            var confirm = EditorUtility.DisplayDialog(
                "덮어쓰기 확인", $"'{levelName}'이 이미 존재합니다. 덮어쓰시겠습니까?",
                "덮어쓰기", "취소");
            
            if(!confirm) return ESaveResult.Cancelled;
        }
        else
        {
            data = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(data, path);
        }
        
        data.SetData(mapBuilder, levelName);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        return ESaveResult.Success;
    }

    public static void Load(LevelData data)
    {
        var parent = new GameObject(data.LevelName);

        var floors = CreateFloorAsset(data, parent.transform);
        CreateWallAsset(data, floors, parent.transform);
        CreateFreeObject(data, parent.transform);
    }

    public static void LoadAsEditingMode(MapBuilder mapBuilder, LevelData data)
    {
        mapBuilder.DeleteLevelData();
        mapBuilder.SetGridSetting(data);
        mapBuilder.CreateGrid();
        mapBuilder.SetAssetData(data);

        var floors = CreateFloorAsset(data, mapBuilder.LevelParent);
        CreateWallAsset(data, floors, mapBuilder.LevelParent);
        CreateFreeObject(data, mapBuilder.LevelParent);
    }

    private static GameObject[] CreateFloorAsset(LevelData data, Transform parent)
    {
        var center = Object.FindFirstObjectByType<MapBuilder>().transform.position;

        var size = data.GridSize.x * data.GridSize.y;
        var startX = center.x - (data.GridSize.x - 1) * 0.5f * data.CellSize;
        var startZ = center.z + (data.GridSize.y - 1) * 0.5f * data.CellSize;
        
        var result = new GameObject[size];
        
        for (var y = 0; y < data.GridSize.y; y++)
        {
            for (var x = 0; x < data.GridSize.x; x++)
            {
                var index =  y * data.GridSize.x + x;
                if (string.IsNullOrEmpty(data.CellAssetData[index].FloorPath))
                    continue;
                
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(data.CellAssetData[index].FloorPath);
                var cell = Object.Instantiate(obj, parent);
                var pos = new Vector3(startX + data.CellSize * x, center.y, startZ - data.CellSize * y);
                
                cell.transform.position = pos;
                cell.transform.rotation = Quaternion.Euler(0, (int)data.CellAssetData[index].FloorRot * 90, 0);
                cell.name = $"[{x}, {y}]";
                result[index] = cell;
            }
        }

        return result;
    }

    private static void CreateWallAsset(LevelData data, GameObject[] floor, Transform parent)
    {
        if (floor.Length <= 0) return;
        
        var height = floor.First(x => x != null).GetComponent<Renderer>().bounds.max.y;
        
        for (var i = 0; i < data.CellAssetData.Length; i++)
        {
            if(floor[i] == null) continue;
            var curData = data.CellAssetData[i];

            for (var j = 0; j < 4; j++)
            {
                if(string.IsNullOrEmpty(curData.WallPaths[j])) continue;
                
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(curData.WallPaths[j]);
                var wall = MapBuilderEditor.CreateWallPivot(obj, parent, data.CellSize);
                
                var floorPos = floor[i].transform.position;
                var pos = new Vector3(floorPos.x, height, floorPos.z);
                
                wall.transform.position = pos;
                wall.transform.rotation = Quaternion.Euler(0, j * 90f, 0);
            }
        }
    }

    private static void CreateFreeObject(LevelData data, Transform parent)
    {
        foreach (var freeAsset in data.FreeAssetData)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(freeAsset.AssetPath);
            var obj = Object.Instantiate(prefab, parent);

            obj.transform.position = freeAsset.Position;
            obj.transform.rotation = Quaternion.Euler(0, freeAsset.YRotation, 0);
        }
    }
}

public enum ESaveResult
{
    Success,
    Cancelled
}
