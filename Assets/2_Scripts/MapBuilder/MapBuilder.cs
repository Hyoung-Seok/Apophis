using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    public Cell[] Cells => cells;
    public Vector2Int GridSize => gridSize;
    public float CellSize => cellSize;
    public float CellInterval => cellInterval;
    public LayerMask CellLayer => cellLayer;
    public LayerMask FloorLayer => floorLayer;
    public Transform LevelParent => levelParent;
    public CellAssetData[] CellAssetsArr => cellAssetArr;
    public List<FreeAssetData> FreeAssetList => freeAssetList;
    
    [SerializeField] private GameObject cellObj;
    [SerializeField] private LayerMask cellLayer;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform levelParent;

    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellSize;
    [SerializeField] private float cellInterval;

    [SerializeField, HideInInspector] private Cell[] cells;
    [SerializeField, HideInInspector] private CellAssetData[] cellAssetArr;
    [SerializeField, HideInInspector] private List<FreeAssetData> freeAssetList = new();
    [SerializeField, HideInInspector] private GameObject gridParent;
    
    private GameObject _cell;
    private const string CELL_PARENT_NAME = "GridParent";

    public void CreateGrid()
    {
        DestroyGrid();
        InitCellObject();
        
        cells = new Cell[gridSize.x * gridSize.y];
        cellAssetArr = new CellAssetData[cells.Length];
        
        for (var i = 0; i < cells.Length; i++)
        {
            cellAssetArr[i] = new CellAssetData();
        }

        var center = transform.position;
        var startX = center.x - (gridSize.x - 1) * 0.5f * cellSize;
        var startZ = center.z + (gridSize.y - 1) * 0.5f * cellSize;

        gridParent = new GameObject(CELL_PARENT_NAME);

        for (var y = 0; y < gridSize.y; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                var cell = Instantiate(_cell, gridParent.transform);
                
                cell.transform.position = new Vector3(startX + cellSize * x, center.y, startZ - cellSize * y);
                cell.name = $"[{x},{y}]";
                
                cells[y * gridSize.x + x] = cell.GetComponent<Cell>();
            }
        }
        
        DestroyImmediate(_cell);
    }

    public void DestroyGrid()
    {
        if (gridParent ==null) return;

        DestroyImmediate(gridParent);
    }
    
    public bool TryAddAssetData(int index, BuilderAssetData assetData, ERot90 rot)
    {
        switch (assetData.Category)
        {
            case "Floor":
                if (!string.IsNullOrEmpty(cellAssetArr[index].FloorPath))
                {
                    return false;
                }
                
                cellAssetArr[index].FloorPath = assetData.Path;
                cellAssetArr[index].FloorRot = rot;
                break;
            
            case "Wall":
                if (!string.IsNullOrEmpty(cellAssetArr[index].WallPaths[(int)rot]))
                {
                    return false;
                }

                cellAssetArr[index].WallPaths[(int)rot] = assetData.Path;
                break;
        }

        return true;
    }

    public void AddFreeAssetData(string path, Vector3 pos, float yRot)
    {
        var assetData = new FreeAssetData()
        {
            AssetPath =  path,
            Position = pos,
            YRotation =  yRot
        };
        
        freeAssetList.Add(assetData);
    }

    public bool IsCellHasFloor(int index)
    {
        return !string.IsNullOrEmpty(cellAssetArr[index].FloorPath);
    }

    public Vector2Int Convert1DIndexTo2D(int index)
    {
        var y = index / gridSize.x;
        var x = index % gridSize.x;
        
        return new Vector2Int(x, y);
    }

    public int Convert2DIndexTo1D(Vector2Int index)
    {
        return index.y * gridSize.x + index.x;
    }
    
    public void DeleteLevelData()
    {
        var groupIndex = Undo.GetCurrentGroup();
        Undo.RegisterCompleteObjectUndo(this, "DeleteLevel");
        
        cellAssetArr = new CellAssetData[cells.Length];
        freeAssetList.Clear();

        for (var i = levelParent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(levelParent.GetChild(i).gameObject);
        }
        Undo.CollapseUndoOperations(groupIndex);
    }
    
    public void SetGridSetting(LevelData levelData)
    {
        gridSize = levelData.GridSize;
        cellSize = levelData.CellSize;
        cellInterval = levelData.CellInterval;
    }

    public void SetAssetData(LevelData levelData)
    {
        for (var i = 0; i < cellAssetArr.Length; i++)
        {
            var cellAsset = new CellAssetData()
            {
                FloorPath = levelData.CellAssetData[i].FloorPath,
                FloorRot = levelData.CellAssetData[i].FloorRot,
                WallPaths = (string[])levelData.CellAssetData[i].WallPaths.Clone()
            };

            cellAssetArr[i] = cellAsset;
        }

        foreach (var data in levelData.FreeAssetData)
        {
            var assetData = new FreeAssetData()
            {
                AssetPath = data.AssetPath,
                Position = data.Position,
                YRotation = data.YRotation
            };

            freeAssetList.Add(assetData);
        }
    }

    private void InitCellObject()
    {
        _cell = Instantiate(cellObj, transform);
        
        var boxCollider = _cell.GetComponent<BoxCollider>();
        var quad = _cell.transform.GetChild(0);
        var modelSize = cellSize - cellInterval;
        
        boxCollider.size = new Vector3(cellSize, 0.2f, cellSize);
        quad.localScale = new Vector3(modelSize, modelSize, modelSize);
    }
}
