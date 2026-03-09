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

    private int _prevIndex = 0;
    private const float HIGHLIGHT_ALPHA = 1f;
    private const float ORIGIN_ALPHA = 0.3f;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = mapBuilderAsset.CloneTree();
        _mapBuilder = (MapBuilder)target;

        if (EditorWindow.HasOpenInstances<PaletteEditorWindow>() == true)
        {
            _paletteEditor = EditorWindow.GetWindow<PaletteEditorWindow>();   
        }
        
        BindingButton();
        
        return _root;
    }

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var e = Event.current;

        switch (e.type)
        {
            case EventType.MouseMove:
                if (!TryGetHitCell(e, out var rayHit)) return;

                var index = rayHit.transform.GetSiblingIndex();
                if (_prevIndex == index) return;
                
                _mapBuilder.Cells[_prevIndex].SetAlpha(ORIGIN_ALPHA);
                _mapBuilder.Cells[index].SetAlpha(HIGHLIGHT_ALPHA);

                _prevIndex = index;
                SceneView.RepaintAll();
                break;
            
            case EventType.MouseDown:
                if (e.button != 0) return;
                if (!TryGetHitCell(e, out var hit)) return;
                if (_paletteEditor?.SelectedAsset is null) return;
                
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

    private bool TryGetHitCell(Event e, out RaycastHit hit)
    {
        var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        return Physics.Raycast(ray, out hit, 1000f, _mapBuilder.CellLayer);
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
