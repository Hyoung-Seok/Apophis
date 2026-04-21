using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    public static EEditorMode CUR_MODE = EEditorMode.Place;
    private EEditorMode _prevMode = EEditorMode.Place;
    [SerializeField] private VisualTreeAsset mapBuilderUxml;
    
    private MapBuilder _mapBuilder;
    private CellHighlighter _cellHighlighter;
    private VisualElement _root;
    
    private GameObject _selectedObj;
    private GameObject _curWall;
    private string _curCategory;
    private ERot90 _curRot = ERot90.D0;
    
    private const float ROTATION_STEP = 5f;
    private const float YPOS_STEP = 0.1f;

    private int _startCellIndex = -1;
    private int _endCellIndex = -1;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = mapBuilderUxml.CloneTree();
        _mapBuilder = (MapBuilder)target;
        _cellHighlighter = new CellHighlighter(_mapBuilder, Color.green);
        
        BindingButton();

        return _root;
    }

    public static GameObject CreateWallPivot(GameObject wallPrefab, Transform parent, float cellSize)
    {
        var pivot = new GameObject("WallPivot");
        pivot.transform.SetParent(parent);

        var wall = Instantiate(wallPrefab, pivot.transform);
        wall.transform.rotation = Quaternion.identity;
        var bound = wall.GetComponent<Renderer>().bounds;
        
        var wallHeight = bound.size.y;
        var wallDepth = bound.extents.z;
        wall.transform.localPosition 
            = new Vector3(0, wallHeight / 2f, cellSize / 2f + wallDepth);

        return pivot;
    }
    
    private void BindingButton()
    {
        _root.Q<Button>("CreateGridBtn").clicked += _mapBuilder.CreateGrid;
        _root.Q<Button>("DestroyGridBtn").clicked += _mapBuilder.DestroyGrid;
        _root.Q<Button>("OpenPaletteBtn").clicked += PaletteCustomEditor.ShowWindow;
        _root.Q<Button>("OpenLevelDataIOBtn").clicked += LevelDataIOEditor.ShowWindow;
    }

    private void OnSceneGUI()
    {
        var e = Event.current;
        
        if (_prevMode != CUR_MODE)
        {
            _startCellIndex = _endCellIndex = -1;
            _cellHighlighter.RestoreAllHighlights();
            
            if (_prevMode == EEditorMode.Remove)
            {
                _cellHighlighter.ClearRemoveHighlight();
            }
            else
            {
                _cellHighlighter.ClearHoverCellHighlight();
                DestroyPreviewAssets();
            }
            _prevMode = CUR_MODE;
        }
        
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button != 0) return;
                
                if (e.shift && CUR_MODE == EEditorMode.Place
                    && TryGetCellIndex(e.mousePosition, out var startIndex))
                {
                    _endCellIndex = _startCellIndex = startIndex;
                    _cellHighlighter.UpdateRangeCellHighlight(_startCellIndex, _endCellIndex);
                }
                else
                {
                    switch (CUR_MODE)
                    {
                        case EEditorMode.Place when _selectedObj != null:
                            PlaceObject(e);
                            break;
                        
                        case EEditorMode.Remove:
                            RemoveObject(e);
                            break;
                    }
                }
                
                e.Use();
                break;
            
            case EventType.MouseDrag:
                if (_startCellIndex != -1 && TryGetCellIndex(e.mousePosition, out var dragIndex)
                                          && dragIndex != _endCellIndex)
                {
                    _endCellIndex = dragIndex;
                    _cellHighlighter.UpdateRangeCellHighlight(_startCellIndex, _endCellIndex);
                    e.Use();
                }
                break;
            
            case EventType.MouseUp:
                if (_endCellIndex != -1 && e.button == 0)
                {
                    if (_startCellIndex != _endCellIndex)
                    {
                        PlaceObjectByRange();
                    }

                    _startCellIndex = _endCellIndex = -1;
                    _cellHighlighter.RestoreAllHighlights();
                    e.Use();
                }
                break;
            
            case EventType.MouseMove:
                if (CUR_MODE == EEditorMode.Place)
                {
                    OnHoverInPlaceMode(e);
                }
                else
                {
                    if (TryRaycast(e.mousePosition, ~_mapBuilder.CellLayer, out var hit))
                    {
                        var renderer = hit.transform.gameObject.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            _cellHighlighter.UpdateRemoveHighlight(renderer);
                        }
                    }
                    else
                    {
                        _cellHighlighter.ClearRemoveHighlight();
                    }
                    
                    Repaint();
                }

                break;
            
            case EventType.ScrollWheel:
                if (_selectedObj == null) return;
                
                if (IsSnapCellCategory == true)
                {
                    RotateFloorOrWallAsset(e);
                }
                else if (e.control)
                {
                    AdjustFreeAssetHeight(e);
                }
                else
                {
                    RotateFreeAsset(e);
                }
                
                e.Use();
                break;
            
            default:
                return;
        }
    }
    
    private void OnHoverInPlaceMode(Event e)
    {
        if (!TryGetCellIndex(e.mousePosition, out var index))
        {
            _cellHighlighter.ClearHoverCellHighlight();
            if(_selectedObj != null) _selectedObj.SetActive(false);
            return;
        }

        var changed = _cellHighlighter.UpdateCellHighlight(index);
        
        if (IsSnapCellCategory == false && _selectedObj != null)
        {
            UpdateFreeAssetPreview(e);
        }
        else if (changed && TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit))
        {
            SnapPreviewAssetToCell(e, _mapBuilder.Cells[index], hit);
        }
        
        if(changed)
            Repaint();
    }
    
    private void SnapPreviewAssetToCell(Event e, Cell cell, RaycastHit cellHit)
    {
        if (_selectedObj == null) return;
        
        var pos = cellHit.transform.position;

        switch (_curCategory)
        {
            case "Floor":
                _selectedObj.SetActive(true);
                _selectedObj.transform.position = cell.transform.position;
                break;
            
            case "Wall":
                if (TryRaycast(e.mousePosition, _mapBuilder.FloorLayer, out var floorHit) == false)
                {
                    if(_curWall != null)
                        _curWall.SetActive(false);
                    return;
                }
                
                if (_curWall == null)
                {
                    _selectedObj.SetActive(true);
                    _curWall = CreateWallPivot(_selectedObj, _mapBuilder.LevelParent, _mapBuilder.CellSize);
                    _selectedObj.SetActive(false);
                }
                
                _curWall.SetActive(true);
                _curWall.transform.position = new Vector3(pos.x, floorHit.point.y, pos.z);
                _curWall.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
                break;
        }
    }

    private void UpdateFreeAssetPreview(Event e)
    {
        if (TryRaycast(e.mousePosition, _mapBuilder.FloorLayer, out var hit) == false)
        {
            _selectedObj.SetActive(false);
            return;
        }
        
        _selectedObj.SetActive(true);
        var objPos = new Vector3(hit.point.x, _selectedObj.transform.position.y, hit.point.z);
        _selectedObj.transform.position = objPos;
    }

    private void RemoveObject(Event e)
    {
        if (TryRaycast(e.mousePosition, ~_mapBuilder.CellLayer, out var hit) == false) return;
        if (TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var cellHit) == false) return;

        var index = cellHit.transform.GetSiblingIndex();
        var layer = hit.transform.gameObject.layer;
        GameObject desObj = null;

        var groupIndex = Undo.GetCurrentGroup();
        Undo.RegisterCompleteObjectUndo(_mapBuilder, "Remove");

        if (layer == LayerMask.NameToLayer("Floor"))
        {
            _mapBuilder.CellAssetsArr[index].FloorPath = string.Empty;
            _mapBuilder.CellAssetsArr[index].FloorRot = ERot90.D0;
            
            desObj = hit.transform.gameObject;
        }
        else if (layer == LayerMask.NameToLayer("Wall"))
        {
            var pivot = hit.transform.parent;
            var pivotPos = new Vector2(pivot.position.x, pivot.position.z);
            var rot = Mathf.RoundToInt(pivot.eulerAngles.y / 90f) % 4;

            var floorIndex = _mapBuilder.Cells.FirstOrDefault(x =>
            {
                var cellPos = new Vector2(x.transform.position.x, x.transform.position.z);
                return Vector2.Distance(cellPos, pivotPos) < 0.01f;
            })?.transform.GetSiblingIndex();

            if (floorIndex == null)
            {
                Undo.CollapseUndoOperations(groupIndex);
                return;
            }
            _mapBuilder.CellAssetsArr[floorIndex.Value].WallPaths[rot] = string.Empty;

            desObj = pivot.gameObject;
        }
        else
        {
            var target = hit.transform;
            while (target.parent != null && target.parent != _mapBuilder.LevelParent)
            {
                target = target.parent;
            }
            
            var pos = target.transform.position;
            var assetIndex = _mapBuilder.FreeAssetList.FindIndex(d =>
                Vector3.Distance(d.Position, pos) < 0.01f);

            if (assetIndex >= 0)
            {
                _mapBuilder.FreeAssetList.RemoveAt(assetIndex);
                desObj = target.gameObject;
            }
        }

        if (desObj != null)
            Undo.DestroyObjectImmediate(desObj);

        Undo.CollapseUndoOperations(groupIndex);
    }

    private void RemoveObjectByRange(Event e)
    {
        if (SetRangeIndex(e) == false) return;
        
        var start2DIndex = _mapBuilder.Convert1DIndexTo2D(_startCellIndex);
        var end2DIndex = _mapBuilder.Convert1DIndexTo2D(_endCellIndex);

        if (EditorUtility.DisplayDialog("범위 삭제",
                $"{start2DIndex} 부터 {end2DIndex} 까지 배치된 모든 오브젝트를 삭제하시겠습니까?",
                "확인", "취소") == true)
        {
            var groupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(_mapBuilder, "RangeRemove");

            var min = Vector2Int.Min(start2DIndex, end2DIndex);
            var max = Vector2Int.Max(start2DIndex, end2DIndex);

            for (var i = _mapBuilder.LevelParent.childCount - 1; i >= 0; i--)
            {
                var child = _mapBuilder.LevelParent.GetChild(i);

                if (TryParseIndex(child.name, out var x, out var y)
                    && x >= min.x && x <= max.x
                    && y >= min.y && y <= max.y)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                    var index = _mapBuilder.Convert2DIndexTo1D(new Vector2Int(x, y));

                    _mapBuilder.CellAssetsArr[index].FloorPath = string.Empty;
                    _mapBuilder.CellAssetsArr[index].FloorRot = ERot90.D0;
                    _mapBuilder.CellAssetsArr[index].WallPaths = new string[4];
                }
            }
            Undo.CollapseUndoOperations(groupIndex);
        }
    }

    private void PlaceObjectByRange()
    {
        if (!IsSnapCellCategory || _selectedObj == null) return;

        var start2DIndex = _mapBuilder.Convert1DIndexTo2D(_startCellIndex);
        var end2DIndex = _mapBuilder.Convert1DIndexTo2D(_endCellIndex);
        var min = Vector2Int.Min(start2DIndex, end2DIndex);
        var max = Vector2Int.Max(start2DIndex, end2DIndex);
        
        var groupIndex = Undo.GetCurrentGroup();
        Undo.RegisterCompleteObjectUndo(_mapBuilder, "RangePlace");
            
        var assetData = PaletteCustomEditor.Instance.CurrentSelectedAsset;

        for (var y = min.y; y <= max.y; y++)
        {
            for (var x = min.x; x <= max.x; x++)
            {
                var cellIndex = _mapBuilder.Convert2DIndexTo1D(new Vector2Int(x, y));
                var pos = _mapBuilder.Cells[cellIndex].transform.position;
                    
                var placed = _curCategory switch
                {
                    "Floor" => PlaceFloor(cellIndex, assetData, pos),
                    "Wall" => PlaceWall(cellIndex, assetData, pos),
                    _ => null
                };
                    
                if(placed != null)
                    Undo.RegisterCreatedObjectUndo(placed, "RangePlace");
            }
        }
        Undo.CollapseUndoOperations(groupIndex);
    }

    private bool SetRangeIndex(Event e)
    {
        if (TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit) == false) return false;

        var index = hit.transform.GetSiblingIndex();
        if (_startCellIndex == -1)
        {
            _startCellIndex = index;
            return false;
        }
        _endCellIndex = index;
        return true;
    }

    private void PlaceObject(Event e)
    {
        if (!TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit))
        {
            return;
        }
        
        var assetData = PaletteCustomEditor.Instance.CurrentSelectedAsset;
        var index = hit.transform.GetSiblingIndex();
        var pos = hit.transform.position;
        
        var groupIndex = Undo.GetCurrentGroup();
        Undo.RegisterCompleteObjectUndo(_mapBuilder, $"Place_{assetData.Category}");

        var placed = assetData.Category switch
        {
            "Floor" => PlaceFloor(index, assetData, pos),
            "Wall" => PlaceWall(index, assetData, pos),
            _ => PlaceFreeAsset(assetData)
        };

        if (placed == null)
        {
            Undo.RevertAllInCurrentGroup();
            return;
        }
        
        Undo.RegisterCreatedObjectUndo(placed, $"Place_{assetData.Category}");
        Undo.CollapseUndoOperations(groupIndex);
    }

    private GameObject PlaceFloor(int index, BuilderAssetData assetData, Vector3 pos)
    {
        if (_mapBuilder.TryAddAssetData(index, assetData, _curRot) == false)
        {
            return null;
        }
                
        var floorObj = Instantiate(_selectedObj, pos, _selectedObj.transform.rotation, _mapBuilder.LevelParent);
        var index2D =  _mapBuilder.Convert1DIndexTo2D(index);
        floorObj.name = $"{_selectedObj.name}[{index2D.x},{index2D.y}]";

        return floorObj;
    }

    private GameObject PlaceWall(int index, BuilderAssetData assetData, Vector3 pos)
    {
        if (_mapBuilder.IsCellHasFloor(index) == false) return null;
        if (_mapBuilder.TryAddAssetData(index, assetData, _curRot) == false) return null;

        var ts = _curWall.transform;
        var wallObj = Instantiate(_curWall, pos, ts.rotation, _mapBuilder.LevelParent);
        var index2D = _mapBuilder.Convert1DIndexTo2D(index);
        
        wallObj.name = $"{_selectedObj.name}[{index2D.x},{index2D.y}]";
        return wallObj;
    }

    private GameObject PlaceFreeAsset(BuilderAssetData assetData)
    {
        _mapBuilder.AddFreeAssetData(assetData.Path, _selectedObj.transform.position, _selectedObj.transform.eulerAngles.y);
        var obj = Instantiate(_selectedObj, _selectedObj.transform.position, _selectedObj.transform.rotation,
            _mapBuilder.LevelParent);

        return obj;
    }
    
    private void RotateFloorOrWallAsset(Event e)
    {
        var dir = e.delta.y > 0 ? 1 : -1;
        _curRot = (ERot90)(((int)_curRot + dir + 4) % 4);
        
        if (_curCategory == "Floor")
        {
            _selectedObj.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
        }
        else
        {
            if (_curWall != null)
            {
                _curWall.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
            }
        }
    }

    private void RotateFreeAsset(Event e)
    {
        var dir = e.delta.y > 0 ? -ROTATION_STEP : ROTATION_STEP;
        _selectedObj.transform.Rotate(0f, dir, 0f);
    }

    private void AdjustFreeAssetHeight(Event e)
    {
        var dir = e.delta.y > 0 ? -YPOS_STEP : YPOS_STEP;
        var pos = _selectedObj.transform.position;

        pos.y += dir;
        _selectedObj.transform.position = pos;
    }

    private bool TryRaycast(Vector2 pos, LayerMask layer, out RaycastHit hit)
    {
        var ray = HandleUtility.GUIPointToWorldRay(pos);
        return Physics.Raycast(ray, out hit, 1000f, layer);
    }

    private bool TryGetCellIndex(Vector2 pos, out int index)
    {
        if (TryRaycast(pos, _mapBuilder.CellLayer, out var hit) == false)
        {
            index = -1;
            return false;
        }
        
        index = hit.transform.GetSiblingIndex();
        return true;
    }

    private void OnPaletteAssetChanged(BuilderAssetData asset)
    {
        _startCellIndex = _endCellIndex = -1;
        
        if (_selectedObj != null)
        {
            DestroyImmediate(_selectedObj);
            _selectedObj = null;
        }

        if (_curWall != null)
        {
            DestroyImmediate(_curWall);
            _curWall = null;
        }
        
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);
        
        _curCategory = asset.Category;
        _selectedObj = Instantiate(obj, _mapBuilder.LevelParent);
        _selectedObj.SetActive(false);
    }

    private bool TryParseIndex(string name, out int x, out int y)
    {
        x = y = 0;
        
        var start = name.LastIndexOf('[');
        var end = name.LastIndexOf(']');
        if (start == -1 || end == -1) return false;

        var parts = name.Substring(start + 1, end - start - 1).Split(',');
        return parts.Length == 2
               && int.TryParse(parts[0], out x)
               && int.TryParse(parts[1], out y);
    }
    
    private bool IsSnapCellCategory => _curCategory == "Floor" || _curCategory == "Wall";

    private void DestroyPreviewAssets()
    {
        if(_curWall != null) DestroyImmediate(_curWall);
        if(_selectedObj != null) DestroyImmediate(_selectedObj);
    }
    
    private void OnEnable()
    {
        PaletteCustomEditor.OnAssetSelected += OnPaletteAssetChanged;
        
        PaletteCustomEditor.OnDisablePreviewAsset -= DestroyPreviewAssets;
        PaletteCustomEditor.OnDisablePreviewAsset += DestroyPreviewAssets;
    }

    private void OnDisable()
    {
        PaletteCustomEditor.OnAssetSelected -= OnPaletteAssetChanged;

        _startCellIndex = _endCellIndex = -1;

        if (_mapBuilder.Cells != null)
        {
            _cellHighlighter.RestoreAllHighlights();
            _cellHighlighter.ClearHoverCellHighlight();
        }
        
        if (_selectedObj != null)
        {
            DestroyImmediate(_selectedObj);
            _selectedObj = null;
        }

        if (_curWall != null)
        {
            DestroyImmediate(_curWall);
            _curWall = null;
        }
    }
}