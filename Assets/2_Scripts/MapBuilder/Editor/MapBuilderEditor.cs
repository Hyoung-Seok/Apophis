using System;
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
    private VisualElement _root;
    
    private GameObject _selectedObj;
    private GameObject _curWall;
    private string _curCategory;
    private ERot90 _curRot = ERot90.D0;

    private int _prevIndex = 0;
    private const float ORIGIN_ALPHA = 0.3f;
    private const float HIGHLIGHT_ALPHA = 1f;
    private const float ROTATION_STEP = 5f;
    private const float YPOS_STEP = 0.1f;

    private Renderer _prevHoverAssetRenderer;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = mapBuilderUxml.CloneTree();
        _mapBuilder = (MapBuilder)target;
        
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
            if (_prevMode == EEditorMode.Remove)
            {
                ClearRemoveHighlight();
            }
            else
            {
                _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
                _prevIndex = 0;
                
                DestroyPreviewAssets();
            }
            _prevMode = CUR_MODE;
        }
        
        switch (e.type)
        {
            case EventType.MouseMove:
                if(CUR_MODE == EEditorMode.Place)
                    UpdateCellHighlight(e);
                else
                    UpdateRemoveHighlight(e);
                break;
            
            case EventType.MouseDown when CUR_MODE == EEditorMode.Place:
                    if (e.button != 0 ||
                        PaletteCustomEditor.Instance?.CurrentSelectedAsset == null) return;

                    PlaceObject(e);
                break;
            
            case EventType.MouseDown when CUR_MODE == EEditorMode.Remove:
                RemoveObject(e);
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

    private void UpdateCellHighlight(Event e)
    {
        if (TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit) == false)
        {
            if(_selectedObj != null) _selectedObj.SetActive(false);
            return;
        }

        if (IsSnapCellCategory == false && _selectedObj != null)
        {
            UpdateFreeAssetPreview(e);
            Repaint();
        }

        var index = hit.transform.GetSiblingIndex();
        if (index == _prevIndex) return;
        
        var curCell = _mapBuilder.Cells[index];
        curCell.ChangeAlpha(HIGHLIGHT_ALPHA);
        SnapPreviewAssetToCell(e, curCell, hit);
                                    
        if (_prevIndex < _mapBuilder.Cells.Length)
            _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
        _prevIndex = index;
        Repaint();
    }

    private void UpdateRemoveHighlight(Event e)
    {
        if (TryRaycast(e.mousePosition, ~_mapBuilder.CellLayer, out var hit) == false)
        {
            ClearRemoveHighlight();
            return;
        }

        var renderer = hit.transform.GetComponent<Renderer>();
        if (renderer == null || renderer == _prevHoverAssetRenderer) return;

        ClearRemoveHighlight();

        var mpb = new MaterialPropertyBlock();
        mpb.SetColor(Cell.BASE_COLOR, Color.red);
        renderer.SetPropertyBlock(mpb);

        _prevHoverAssetRenderer = renderer;
        Repaint();
    }

    private void ClearRemoveHighlight()
    {
        if (_prevHoverAssetRenderer == null) return;

        var mpb = new MaterialPropertyBlock();
        mpb.SetColor(Cell.BASE_COLOR, Color.white);
        _prevHoverAssetRenderer.SetPropertyBlock(mpb);
        _prevHoverAssetRenderer = null;
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
        
        e.Use();
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
            "Wall" => PlaceWall(index, assetData),
            _ => PlaceFreeAsset(assetData)
        };

        if (placed != null)
        {
            Undo.RegisterCreatedObjectUndo(placed, $"Place_{assetData.Category}");
            Undo.CollapseUndoOperations(groupIndex);
        }
        
        e.Use();
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

    private GameObject PlaceWall(int index, BuilderAssetData assetData)
    {
        if (_mapBuilder.IsCellHasFloor(index) == false) return null;
        if (_mapBuilder.TryAddAssetData(index, assetData, _curRot) == false) return null;

        var ts = _curWall.transform;
        var wallObj = Instantiate(_curWall, ts.position, ts.rotation, _mapBuilder.LevelParent);
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

    private void OnPaletteAssetChanged(BuilderAssetData asset)
    {
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
        if (_mapBuilder.Cells != null && _prevIndex < _mapBuilder.Cells.Length
            && _mapBuilder.Cells[_prevIndex] != null)
            _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
        
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