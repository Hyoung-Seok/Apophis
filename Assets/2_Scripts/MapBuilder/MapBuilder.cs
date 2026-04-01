using System;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    public Cell[] Cells => cells;
    public float CellSize => cellSize;
    public LayerMask CellLayer => cellLayer;
    public LayerMask FloorLayer => floorLayer;
    public Transform LevelParent => levelParent;
    
    [SerializeField] private GameObject cellObj;
    [SerializeField] private LayerMask cellLayer;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform levelParent;

    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellSize;
    [SerializeField] private float cellInterval;

    [SerializeField, HideInInspector] private Cell[] cells;
    [SerializeField, HideInInspector] private CellPropData[] cellPropData;
    [SerializeField, HideInInspector] private GameObject gridParent;
    
    private GameObject _cell;
    private const string CELL_PARENT_NAME = "GridParent";

    public void CreateGrid()
    {
        DestroyGrid();
        InitCellObject();
        
        cells = new Cell[gridSize.x * gridSize.y];
        cellPropData = new CellPropData[cells.Length];

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

    public bool AddPropData(int index, BuilderAssetData assetData, ERot90 rot)
    {
        switch (assetData.Category)
        {
            case "Floor":
                if (!string.IsNullOrEmpty(cellPropData[index].GroundPath))
                {
                    return false;
                }
                
                cellPropData[index].GroundPath = assetData.Path;
                cellPropData[index].GroundRot = rot;
                break;
            
            case "Wall":
                // 만약 해당하는 rot에 이미 path가 등록되어 있다면 리턴
                if (!string.IsNullOrEmpty(cellPropData[index].WallPaths[(int)rot]))
                {
                    return false;
                }

                cellPropData[index].WallPaths[(int)rot] = assetData.Path;
                break;
            
            default:
                return false;
        }

        return true;
    }

    public bool IsCellHasFloor(int index)
    {
        return !string.IsNullOrEmpty(cellPropData[index].GroundPath);
    }

    public (int x, int y) Convert1DIndexTo2D(int index)
    {
        var y = index / gridSize.x;
        var x = index % gridSize.x;
        
        return (x, y);
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
