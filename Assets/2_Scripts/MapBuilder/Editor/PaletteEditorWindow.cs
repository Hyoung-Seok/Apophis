using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class PaletteEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset paletteAsset;
    [SerializeField] private VisualTreeAsset paletteItem;
    
    private VisualElement _root;
    private DropdownField _categoryDropdown;
    private VisualElement _assetsContainer;

    public static void ShowWindow()
    {
        var window = GetWindow<PaletteEditorWindow>();
        
        window.titleContent = new GUIContent("Palette Editor");
        window.minSize = window.maxSize = new Vector2(1280, 720);
    }

    public void CreateGUI()
    {
        _root = paletteAsset.CloneTree();
        rootVisualElement.Add(_root);
        
        InitElements();
    }
    
    private void InitElements()
    {
        _categoryDropdown = _root.Q<DropdownField>("ObjCategoryDropdown");
        _assetsContainer = _root.Q<VisualElement>("AssetContainer");
        
        _categoryDropdown.choices = BuilderAssetLoader.GetAssetCategories();
        _categoryDropdown.RegisterValueChangedCallback(CategoryDropdownValueChangedCallback);
    }

    private void CategoryDropdownValueChangedCallback(ChangeEvent<string> evt)
    {
        _assetsContainer.Clear();
        
        var paths = BuilderAssetLoader.BuilderAssets[_categoryDropdown.value];
        var previewTarget = new List<(GameObject, Button)>();

        foreach (var path in paths)
        {
            var item = paletteItem.CloneTree();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            var assetBtn = item.Q<Button>("AssetButton");
            item.Q<Label>("AssetName").text = prefab.name;
            
            previewTarget.Add((prefab, assetBtn));
            _assetsContainer.Add(item);
        }

        rootVisualElement.schedule.Execute(() =>
        {
            foreach (var (prefabs, btn) in previewTarget)
            {
                var preview = AssetPreview.GetAssetPreview(prefabs);

                if (preview == null)
                {
                   continue;
                }
                
                btn.style.width = 128;
                btn.style.height = 128;
                btn.style.backgroundImage = preview;
            }
        }).Every(200).Until(() => !AssetPreview.IsLoadingAssetPreviews());
    }
}
