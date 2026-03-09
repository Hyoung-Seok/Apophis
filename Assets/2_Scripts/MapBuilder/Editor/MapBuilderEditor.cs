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
