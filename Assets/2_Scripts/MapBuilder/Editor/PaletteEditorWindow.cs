using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaletteEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset paletteAsset;
    
    private VisualElement _root;

    public static void ShowWindow()
    {
        var window = GetWindow<PaletteEditorWindow>();
        
        window.titleContent = new GUIContent("Palette Editor");
        window.minSize = window.maxSize = new Vector2(1280, 720);
    }

    public void CreateGUI()
    {
        _root = paletteAsset.CloneTree();
    }
}
