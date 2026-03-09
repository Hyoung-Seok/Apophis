using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class MapBuilder : MonoBehaviour
{
    public LayerMask CellLayer => cellLayer;
    public LayerMask GroundLayer => groundLayer;
    public Transform GridParent => gridParent;
    public Transform LevelParent => levelParent;
    public Cell[] Cells => cells;
    
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellInterval = 0.1f;

    [SerializeField, HideInInspector] private Transform gridParent;
    [SerializeField] private Transform levelParent;
    [SerializeField] private LayerMask cellLayer;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Cell[] cells;
    
    public void CreateGrid()
    {
        DestroyGrid();
        
        if (cellPrefab == null)
        {
            Debug.LogError("셀 프리팹이 할당되지 않았습니다.");
            return;
        }
        
        cells = new Cell[gridSize.x * gridSize.y];
        gridParent = new GameObject("GridParent").transform;    
        
        var cell = SetCellProperties();
        
        var startX = cellSize * ((gridSize.x - 1) * 0.5f);
        var startZ = cellSize * ((gridSize.y - 1) * 0.5f);
        
        var pos = transform.position;
        var startPos = new Vector3(pos.x - startX, pos.y, pos.z + startZ);

        for (var y = 0; y < gridSize.y; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                var cellPos = new Vector3(startPos.x + (x * cellSize), startPos.y, startPos.z - (y * cellSize));
                var obj = Instantiate(cell, cellPos, Quaternion.identity, gridParent);
                obj.name = $"Cell[{x},{y}]";

                cells[y * gridSize.y + x] = obj.GetComponent<Cell>();
            }
        }
        
        DestroyImmediate(cell);
    }

    public void DestroyGrid()
    {
        if (gridParent == null)
        {
            return;
        }
        
        DestroyImmediate(gridParent.gameObject);
    }

    private GameObject SetCellProperties()
    {
        var cellObj = Instantiate(cellPrefab, transform);

        var boxCol = cellObj.GetComponent<BoxCollider>();
        var model = cellObj.transform.GetChild(0);
        var modelSize = cellSize - cellInterval;
        
        boxCol.size = new Vector3(cellSize, 0.15f, cellSize);
        model.localScale = new Vector3(modelSize, 0.15f, modelSize);

        return cellObj;
    }
}
