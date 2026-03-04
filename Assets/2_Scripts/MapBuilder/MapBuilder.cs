using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellInterval = 0.1f;
    
    public void CreateGrid()
    {
        DestroyGrid();

        if (cellPrefab == null)
        {
            Debug.LogError("셀 프리팹이 할당되지 않았습니다.");
            return;
        }
        
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
                var obj = Instantiate(cell, cellPos, Quaternion.identity,  transform);
                obj.name = $"Cell[{x},{y}]";
            }
        }
        
        DestroyImmediate(cell);
    }

    public void DestroyGrid()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
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
