using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    [SerializeField] private VisualTreeAsset mapBuilderAsset;
    
    private VisualElement _root;
    private MapBuilder _mapBuilder;
    private PaletteEditorWindow _paletteEditor;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = mapBuilderAsset.CloneTree();
        _mapBuilder = (MapBuilder)target; 
        
        BindingButton();
        
        return _root;
    }

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button != 0) return;
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (!Physics.Raycast(ray, out var hit, 1000f, _mapBuilder.CellLayer) ||
                        _paletteEditor?.SelectedAsset == null)
                {
                    return;
                }

                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(_paletteEditor.SelectedAsset.Path);
                var obj = (GameObject)PrefabUtility.InstantiatePrefab(asset, _mapBuilder.LevelParent);
                
                obj.transform.position = hit.collider.transform.position;
                Undo.RegisterCreatedObjectUndo(obj, "Place Object");
                
                e.Use();
                break;
            
            default:
                return;
        }
    }

    private void BindingButton()
    {
        var createGrid = _root.Q<Button>("CreateGridBtn");
        var destroyGrid = _root.Q<Button>("DestroyGridBtn");
        var showPalette = _root.Q<Button>("OpenPaletteBtn");
        
        createGrid.clicked += _mapBuilder.CreateGrid;
        destroyGrid.clicked += _mapBuilder.DestroyGrid;
        showPalette.clicked += ShowPaletteEditorWindow;
    }

    private void ShowPaletteEditorWindow()
    {
        _paletteEditor = PaletteEditorWindow.ShowWindow(); 
    }
}
