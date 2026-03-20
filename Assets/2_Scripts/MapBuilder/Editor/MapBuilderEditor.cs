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
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out var hit, 1000f, _mapBuilder.CellLayer))
                {
                    var index = hit.transform.GetSiblingIndex();

                    if (index != _prevIndex)
                    {
                        _mapBuilder.Cells[index].ChangeAlpha(HIGLITE_ALPHA);
                        _mapBuilder.Cells[_prevIndex].ChangeAlpha(ORIGIN_ALPHA);
                        
                        _prevIndex = index;
                    }
                }
                
                Repaint();
                break;
            
            default:
                return;
        }
    }

    private void BindingButton()
    {
        _root.Q<Button>("CreateGridBtn").clicked += _mapBuilder.CreateGrid;
        _root.Q<Button>("DestroyGridBtn").clicked += _mapBuilder.DestroyGrid;
        _root.Q<Button>("OpenPaletteBtn").clicked += PaletteCustomEditor.ShowWindow;
    }
}
