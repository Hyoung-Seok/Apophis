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
    private TextField _levelNameInputField;
    private Button _saveLevelBtn;
    
    private GameObject _selectedObj;
    private string _curCategory;
    private WallPlaceData _wallPlaceData;
    private ERot90 _curRot = ERot90.D0;

    private int _prevIndex = 0;
    private const float ORIGIN_ALPHA = 0.3f;
    private const float HIGHLIGHT_ALPHA = 1f;
    private const float ROTATION_STEP = 5f;
    private const float YPOS_STEP = 0.1f;
    
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

        _saveLevelBtn = _root.Q<Button>("SaveLevelBtn");
        _saveLevelBtn.clicked += OnClickSaveLevelDataBtn;
        _saveLevelBtn.SetEnabled(false);
        _root.Q<Button>("DeleteLevel").clicked += OnClickDeleteLevelDataBtn;

        _levelNameInputField = _root.Q<TextField>("LevelNameInputField");
        _levelNameInputField.RegisterValueChangedCallback(OnLevelNameInputFieldChanged);
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
                                    
        _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
        _prevIndex = index;
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
                _selectedObj.SetActive(false);
                if (TryRaycast(e.mousePosition, _mapBuilder.FloorLayer, out var floorHit) == false)
                {
                    _wallPlaceData?.Wall.SetActive(false);
                    return;
                }
                
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

        var ts = _wallPlaceData.Pivot.transform;
        var wallObj = Instantiate(_wallPlaceData.Pivot, ts.position, ts.rotation, _mapBuilder.LevelParent);
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
            if (_wallPlaceData != null)
            {
                _wallPlaceData.Pivot.transform.rotation = Quaternion.Euler(0, (int)_curRot * 90f, 0);
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

    private void OnClickSaveLevelDataBtn()
    {
        var levelName = _levelNameInputField.text;
        EditorApplication.Beep();
        
        if (levelName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
        {
            EditorUtility.DisplayDialog("저장 실패", "파일명에 사용할 수 없는 문자가 포함되어 있습니다.", "확인");
            return;
        }

        var success = LevelDataIO.Save(_mapBuilder, levelName);
        var msg = success ? $"'{levelName}' 저장 성공" : $"'{levelName}' 저장 실패";
        EditorUtility.DisplayDialog("저장",msg, "확인");
    }
    
    private void OnClickDeleteLevelDataBtn()
    {
        EditorApplication.Beep();
        if (EditorUtility.DisplayDialog("레벨 삭제", 
                "현재 배치된 모든 레벨을 삭제하시겠습니까?", "확인", "취소") == true)
        {
            _mapBuilder.DeleteLevelData();
        }
    }
    
    private bool IsSnapCellCategory => _curCategory == "Floor" || _curCategory == "Wall";

    private void OnLevelNameInputFieldChanged(ChangeEvent<string> evt)
    {
        var isEmpty = string.IsNullOrEmpty(_levelNameInputField.text);
        _saveLevelBtn.SetEnabled(!isEmpty);
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