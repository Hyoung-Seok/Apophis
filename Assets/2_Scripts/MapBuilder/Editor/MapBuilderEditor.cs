using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    [SerializeField] private VisualTreeAsset mapBuilderUxml;
    
    private MapBuilder _mapBuilder;
    private VisualElement _root;
    
    private GameObject _selectedObj;
    private string _curCategory;
    private WallPlaceData _wallPlaceData;
    private ERot90 _curRot = ERot90.D0;

    private int _prevIndex = 0;
    private const float ORIGIN_ALPHA = 0.3f;
    private const float HIGHLIGHT_ALPHA = 1f;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = mapBuilderUxml.CloneTree();
        _mapBuilder = (MapBuilder)target;
        
        BindingButton();

        return _root;
    }
    
    private void BindingButton()
    {
        _root.Q<Button>("CreateGridBtn").clicked += _mapBuilder.CreateGrid;
        _root.Q<Button>("DestroyGridBtn").clicked += _mapBuilder.DestroyGrid;
        _root.Q<Button>("OpenPaletteBtn").clicked += PaletteCustomEditor.ShowWindow;
    }

    private void OnSceneGUI()
    {
        var e = Event.current;
        
        switch (e.type)
        {
            case EventType.MouseMove:
                UpdateCellHighlight(e);
                break;
            
            case EventType.MouseDown:
                if (e.button != 0 ||
                    PaletteCustomEditor.Instance?.CurrentSelectedAsset == null) return;

                PlaceObject(e);
                break;
            
            case EventType.ScrollWheel:
                if (_curCategory == "Floor" || _curCategory == "Wall")
                {
                    RotationFloorOrGroundAsset(e);
                    e.Use();
                }
                break;
            
            default:
                return;
        }
    }

    private void UpdateCellHighlight(Event e)
    {
        if (TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit))
        {
            var index = hit.transform.GetSiblingIndex();

            if (index == _prevIndex)
            {
                return;
            }

            var curCell = _mapBuilder.Cells[index];
            curCell.ChangeAlpha(HIGHLIGHT_ALPHA);
                                    
            SnapPreviewAssetToCell(e, curCell, hit);
                                    
            _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
            _prevIndex = index;
                                    
            Repaint();
            return;
        }
        
        if(_selectedObj != null) 
            _selectedObj.SetActive(false);
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
                _selectedObj.SetActive(false);
                if(TryRaycast(e.mousePosition, _mapBuilder.FloorLayer, out var floorHit) == false) return;
                
                if (_wallPlaceData == null)
                {
                    _wallPlaceData = new WallPlaceData();
                    
                    // 1. 피봇 오브젝트 생성
                    _wallPlaceData.Pivot = new GameObject("WallPivot");
                    _wallPlaceData.Pivot.transform.parent = _mapBuilder.LevelParent;
                    
                    // 2. 벽 생성
                    _wallPlaceData.Wall = Instantiate(_selectedObj, _wallPlaceData.Pivot.transform);
                    _wallPlaceData.Wall.transform.rotation = Quaternion.identity;
                    var bound= _wallPlaceData.Wall.GetComponentInChildren<Renderer>().bounds;
                    
                    // 3. 오프셋 계산
                    var wallHeight = bound.size.y;
                    var wallDepth = bound.extents.z;
                    _wallPlaceData.Wall.transform.localPosition 
                        = new Vector3(0, wallHeight / 2f, _mapBuilder.CellSize / 2f + wallDepth);
                }
                
                _wallPlaceData.Wall.SetActive(true);
                _wallPlaceData.Pivot.transform.position = new Vector3(pos.x, floorHit.point.y, pos.z);
                _wallPlaceData.Pivot.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
                break;
        }
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
        
        switch (assetData.Category)
        {
            case "Floor":
                // 이미 배치된 바닥이 있는 경우
                if (_mapBuilder.AddPropData(index, assetData, _curRot) == false)
                {
                    break;
                }

                Instantiate(_selectedObj, pos, _selectedObj.transform.rotation, _mapBuilder.LevelParent);
                break;
            
            case "Wall":
                if (_mapBuilder.IsCellHasFloor(index) == false) break;
                if (_mapBuilder.AddPropData(index, assetData, _curRot) == false) break;

                var ts = _wallPlaceData.Pivot.transform;
                Instantiate(_wallPlaceData.Pivot, ts.position, ts.rotation, _mapBuilder.LevelParent);
                break;
        }
        
        e.Use();
    }
    

    private void RotationFloorOrGroundAsset(Event e)
    {
        var dir = e.delta.y > 0 ? 1 : -1;
        _curRot = (ERot90)(((int)_curRot + dir + 4) % 4);
        
        if (_curCategory == "Floor")
        {
            _selectedObj.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
        }
        else
        {
            if (_wallPlaceData != null)
            {
                _wallPlaceData.Pivot.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
            }
        }
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

        if (_wallPlaceData != null)
        {
            DestroyImmediate(_wallPlaceData.Pivot);
            _wallPlaceData = null;
        }
        
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);
        
        _curCategory = asset.Category;
        _selectedObj = Instantiate(obj, _mapBuilder.LevelParent);
        _selectedObj.SetActive(false);
    }

    private void OnEnable()
    {
        PaletteCustomEditor.OnAssetSelected += OnPaletteAssetChanged;
    }

    private void OnDisable()
    {
        PaletteCustomEditor.OnAssetSelected -= OnPaletteAssetChanged;
        _mapBuilder.Cells[_prevIndex]?.ChangeAlpha(ORIGIN_ALPHA);
        
        if (_selectedObj != null)
        {
            DestroyImmediate(_selectedObj);
            _selectedObj = null;
        }

        if (_wallPlaceData != null)
        {
            DestroyImmediate(_wallPlaceData.Pivot);
            _wallPlaceData = null;
        }
    }
}

public class WallPlaceData
{
    public GameObject Pivot;
    public GameObject Wall;
}