using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public string LevelName => levelName;
    public CellAssetData[] CellAssetData => cellAssetData;
    public List<FreeAssetData> FreeAssetData => freeAssetData;
    public Vector2Int GridSize => gridSize;
    public float CellSize => cellSize;
    public float CellInterval => cellInterval;

    [SerializeField] private string levelName;
    [SerializeField] private CellAssetData[] cellAssetData;
    [SerializeField] private List<FreeAssetData> freeAssetData;
    
    [SerializeField, HideInInspector] private Vector2Int gridSize;
    [SerializeField, HideInInspector] private float cellSize;
    [SerializeField, HideInInspector] private float cellInterval;

    public void SetData(MapBuilder mapBuilder, string name)
    {
        levelName = name;
        
        gridSize = mapBuilder.GridSize;
        cellSize = mapBuilder.CellSize;
        cellInterval = mapBuilder.CellInterval;

        var srcCell = mapBuilder.CellAssetsArr;
        cellAssetData = new CellAssetData[srcCell.Length];

        for (var i = 0; i < srcCell.Length; i++)
        {
            var src = srcCell[i];

            cellAssetData[i] = new CellAssetData()
            {
                FloorPath = src.FloorPath,
                FloorRot = src.FloorRot,
                WallPaths = (string[])src.WallPaths.Clone()
            };
        }
        
        freeAssetData = new List<FreeAssetData>();
        foreach (var freeAsset in mapBuilder.FreeAssetList)
        {
            freeAssetData.Add(new FreeAssetData()
            {
                AssetPath = freeAsset.AssetPath,
                Position = freeAsset.Position,
                YRotation = freeAsset.YRotation,
            });
        }
    }
}
