using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaletteCustomEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset paletteUxml;

    private static readonly Vector2Int WINDOW_SIZE = new (1280, 720);
    
    private DropdownField _assetsDropdownField;
    private TextField _assetsPath;

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
    }

    private void LoadAssets()
    {
        if (string.IsNullOrEmpty(_assetsPath.value)) return;
        
        MapBuilderAssetLoader.LoadAllAssets(_assetsPath.value);
        _assetsDropdownField.choices = new List<string>(MapBuilderAssetLoader.BuilderAssetData.Keys);
    }
}
