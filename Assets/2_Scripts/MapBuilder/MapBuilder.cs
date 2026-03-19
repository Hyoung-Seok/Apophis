using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    [SerializeField] private GameObject cellObj;
    [SerializeField] private Transform levelParent;

    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellSize;
    [SerializeField] private float cellInterval;

    private GameObject _cell;

    private void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        DestroyGrid();
        InitCellObject();

        var center = transform.position;
        var startX = center.x - (gridSize.x - 1) * 0.5f * cellSize;
        var startZ = center.z + (gridSize.y - 1) * 0.5f * cellSize;

        var gridParent = new GameObject("GridParent").transform;

        for (var y = 0; y < gridSize.y; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                var cell = Instantiate(_cell, gridParent);
                
                cell.transform.position = new Vector3(startX + cellSize * x, center.y, startZ - cellSize * y);
                cell.name = $"[{x},{y}]";
            }
        }
        
        DestroyImmediate(_cell);
    }

    public void DestroyGrid()
    {
        
    }

    private void InitCellObject()
    {
        _cell = Instantiate(cellObj, transform);
        
        var boxCollider = _cell.GetComponent<BoxCollider>();
        var quad = _cell.transform.GetChild(0);
        var modelSize = cellSize - cellInterval;
        
        boxCollider.size = new Vector3(cellSize, cellSize, cellSize);
        quad.localScale = new Vector3(modelSize, modelSize, modelSize);
    }
}
