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
    private GameObject _curAsset;
    private ERot90 _curRot = ERot90.D0;

    private int _prevIndex = 0;
    private const float ORIGIN_ALPHA = 0.3f;
    private const float HIGLITE_ALPHA = 1f;
    
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
                var category = PaletteCustomEditor.Instance?.CurrentSelectedAsset?.Category;

                if (category == "Floor" || category == "Wall")
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
            curCell.ChangeAlpha(HIGLITE_ALPHA);
                                    
            if (_curAsset != null)
            {
                _curAsset.SetActive(true);
                _curAsset.transform.position = curCell.transform.position;
            }
                                    
            _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
            _prevIndex = index;
                                    
            Repaint();
            return;
        }
        
        if(_curAsset != null) 
            _curAsset.SetActive(false);
    }

    private void PlaceObject(Event e)
    {
        if (!TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit))
        {
            return;
        }
        
        var assetData = PaletteCustomEditor.Instance?.CurrentSelectedAsset;
        if (assetData == null)
        {
            return;
        }

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

                Instantiate(_curAsset, pos, _curAsset.transform.rotation, _mapBuilder.LevelParent);
                break;
            
            case "Wall":
                // 바닥이 없는 경우
                if (_mapBuilder.IsCellHasFloor(index) == false)
                {
                    return;
                }
                
                // TODO : 벽 배치 로직 구현
                break;
        }
        
        e.Use();
    }

    private void RotationFloorOrGroundAsset(Event e)
    {
        var dir = e.delta.y > 0 ? 1 : -1;
        _curRot = (ERot90)(((int)_curRot + dir + 4) % 4);
        
        _curAsset.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
    }

    private bool TryRaycast(Vector2 pos, LayerMask layer, out RaycastHit hit)
    {
        var ray = HandleUtility.GUIPointToWorldRay(pos);
        return Physics.Raycast(ray, out hit, 1000f, layer);
    }

    private void OnPaletteAssetChanged(BuilderAssetData asset)
    {
        if (_curAsset != null)
        {
            DestroyImmediate(_curAsset);
            _curAsset = null;
        }
        
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);
        _curAsset = Instantiate(obj, _mapBuilder.LevelParent);
        _curAsset.SetActive(false);
    }

    private void OnEnable()
    {
        PaletteCustomEditor.OnAssetSelected += OnPaletteAssetChanged;
    }

    private void OnDisable()
    {
        PaletteCustomEditor.OnAssetSelected -= OnPaletteAssetChanged;
        _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
    }
}
