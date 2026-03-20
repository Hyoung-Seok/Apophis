using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaletteCustomEditor : EditorWindow
{
    public BuilderAssetData CurrentSelectedAsset {get; private set;}
    
    [SerializeField] private VisualTreeAsset paletteUxml;
    [SerializeField] private VisualTreeAsset assetsUxml;

    private static readonly Vector2Int WINDOW_SIZE = new (1280, 720);
    
    private DropdownField _assetsDropdownField;
    private TextField _assetsPath;
    private VisualElement _assetsContainer;
    private VisualElement _favoritesContainer;

    public static void ShowWindow()
    {
        var window = GetWindow<PaletteCustomEditor>();
        
        window.titleContent = new GUIContent("Palette Custom Editor");
        window.minSize = window.maxSize = WINDOW_SIZE;
    }

    public void CreateGUI()
    {
        paletteUxml.CloneTree(rootVisualElement);
        
        BindingElements();
        LoadAssets();
    }

    private void BindingElements()
    {
        _assetsDropdownField = rootVisualElement.Q<DropdownField>("AssetCategory");
        _assetsPath = rootVisualElement.Q<TextField>("AssetPath");
        _assetsContainer = rootVisualElement.Q<VisualElement>("AssetsContainer");
        _favoritesContainer = rootVisualElement.Q<VisualElement>("FavoriteContainer");

        _assetsDropdownField.RegisterValueChangedCallback(AddAssetsInContainer);
    }

    private void LoadAssets()
    {
        if (string.IsNullOrEmpty(_assetsPath.value)) return;
        
        MapBuilderAssetLoader.LoadAllAssets(_assetsPath.value);
        _assetsDropdownField.choices = new List<string>(MapBuilderAssetLoader.BuilderAssetData.Keys);
    }

    private void AddAssetsInContainer(ChangeEvent<string> evt)
    {
        _assetsContainer.Clear();
        
        var category = _assetsDropdownField.value;
        var previewList = new List<(TemplateContainer, GameObject)>();

        foreach (var asset in MapBuilderAssetLoader.BuilderAssetData[category])
        {
            var root = assetsUxml.CloneTree();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);
            
            root.Q<Label>("AssetName").text = asset.Name;
            // TODO : 버튼, Fav버튼 클릭 시 동작 로직 구현
            root.Q<Button>("AssetSelectBtn").clicked += () => OnAssetSelected(asset);
            
            previewList.Add((root, prefab));
            _assetsContainer.Add(root);
        }
        
        rootVisualElement.schedule.Execute(() =>
        {
            for (var i = previewList.Count - 1; i >= 0; i--)
            {
                var img = AssetPreview.GetAssetPreview(previewList[i].Item2);

                if (img == null)
                {
                    continue;
                }
                
                previewList[i].Item1.Q<Button>("AssetSelectBtn").style.backgroundImage = img;
                previewList.RemoveAt(i);
            }
            
        }).Until(() => previewList.Count == 0).Every(100);
    }

    private void OnAssetSelected(BuilderAssetData asset)
    {
        CurrentSelectedAsset = asset;
    }
}
