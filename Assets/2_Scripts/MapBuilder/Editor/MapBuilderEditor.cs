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

    private void OnSceneGUI()
    {
        var e = Event.current;
        
        switch (e.type)
        {
            case EventType.MouseMove:
            {
                if (TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit))
                {
                    var index = hit.transform.GetSiblingIndex();

                    if (index != _prevIndex)
                    {
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
                    }
                }
                else
                {
                    if(_curAsset != null) _curAsset.SetActive(false);
                }
                
                break;
            }

            case EventType.MouseDown:
            {
                if (e.button != 0 ||
                    PaletteCustomEditor.Instance?.CurrentSelectedAsset == null) return;

                PlaceObject(e);
                
                break;
            }

            default:
                return;
        }
    }

    private void PlaceObject(Event e)
    {
        if (TryRaycast(e.mousePosition, _mapBuilder.CellLayer, out var hit))
        {
            var pos = hit.transform.position;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PaletteCustomEditor.Instance
                ?.CurrentSelectedAsset.Path);

            var obj = Instantiate(asset, pos, Quaternion.identity, _mapBuilder.LevelParent);
        }
        
        e.Use();
    }

    private bool TryRaycast(Vector2 pos, LayerMask layer, out RaycastHit hit)
    {
        var ray = HandleUtility.GUIPointToWorldRay(pos);
        return Physics.Raycast(ray, out hit, 1000f, layer);
    }

    private void BindingButton()
    {
        _root.Q<Button>("CreateGridBtn").clicked += _mapBuilder.CreateGrid;
        _root.Q<Button>("DestroyGridBtn").clicked += _mapBuilder.DestroyGrid;
        _root.Q<Button>("OpenPaletteBtn").clicked += PaletteCustomEditor.ShowWindow;
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
    }
}
