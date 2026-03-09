using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class PaletteEditorWindow : EditorWindow
{
    public AssetData SelectedAsset { get; private set; }
    
    [SerializeField] private VisualTreeAsset paletteAsset;
    [SerializeField] private VisualTreeAsset paletteItem;
    
    private VisualElement _root;
    private DropdownField _categoryDropdown;
    private VisualElement _assetsContainer;

    public static PaletteEditorWindow ShowWindow()
    {
        var window = GetWindow<PaletteEditorWindow>();
        
        window.titleContent = new GUIContent("Palette Editor");
        window.minSize = window.maxSize = new Vector2(1280, 720);

        return window;
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
        _categoryDropdown.RegisterValueChangedCallback(CreatePaletteItem);
    }

    private void CreatePaletteItem(ChangeEvent<string> evt)
    {
        _assetsContainer.Clear();
        
        var assetData = BuilderAssetLoader.BuilderAssets[_categoryDropdown.value];
        var previewTarget = new List<(GameObject, Button)>();
        
        foreach (var asset in assetData)
        {
            var item = paletteItem.CloneTree();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);
            
            // TODO : 에셋 버튼이 클릭되면 실행할 함수 등록 및 즐겨찾기 기능 구현
            var assetBtn = item.Q<Button>("AssetButton");
            assetBtn.clicked += () => SelectAsset(asset);
            
            item.Q<Label>("AssetName").text = asset.Name;
            
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

    private void SelectAsset(AssetData assetData)
    {
        SelectedAsset = assetData;
    }
}
