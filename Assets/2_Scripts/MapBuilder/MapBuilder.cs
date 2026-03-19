using System;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    public Cell[] Cells => cells;
    public LayerMask CellLayer => cellLayer;
    
    [SerializeField] private GameObject cellObj;
    [SerializeField] private LayerMask cellLayer;
    [SerializeField] private Transform levelParent;

    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellSize;
    [SerializeField] private float cellInterval;

    [SerializeField, HideInInspector] private Cell[] cells;
    [SerializeField, HideInInspector] private GameObject gridParent;
    
    private GameObject _cell;
    private const string CELL_PARENT_NAME = "GridParent";

    public void CreateGrid()
    {
        DestroyGrid();
        InitCellObject();
        
        cells = new Cell[gridSize.x * gridSize.y];

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
