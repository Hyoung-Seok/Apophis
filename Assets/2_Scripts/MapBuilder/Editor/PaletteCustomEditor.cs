using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaletteCustomEditor : EditorWindow
{
    public static PaletteCustomEditor Instance { get; private set; }
    public BuilderAssetData CurrentSelectedAsset {get; private set;}
    public static event Action<BuilderAssetData> OnAssetSelected;
    
    [SerializeField] private VisualTreeAsset paletteUxml;
    [SerializeField] private VisualTreeAsset assetsUxml;

    private static readonly Vector2Int WINDOW_SIZE = new (1280, 720);
    
    private DropdownField _assetsDropdownField;
    private TextField _assetsPath;
    private VisualElement _assetsContainer;
    
    private VisualElement _favoritesContainer;
    private const string FAV_DATA_PATH = "Assets/7_Data/MapBuilder/FavAssetData.asset";
    private FavAssetsData _favoritesData;
    
    public static void ShowWindow()
    {
        var window = GetWindow<PaletteCustomEditor>();
        
        window.titleContent = new GUIContent("Palette Custom Editor");
        window.minSize = window.maxSize = WINDOW_SIZE;
    }

    public void CreateGUI()
    {
        paletteUxml.CloneTree(rootVisualElement);
        
        BindElements();
        LoadAssets();
        LoadOrCreateFavData();
        RestoreFavData();
    }

    private void BindElements()
    {
        _assetsDropdownField = rootVisualElement.Q<DropdownField>("AssetCategory");
        _assetsDropdownField.RegisterValueChangedCallback(OnCategoryChanged);
        
        _assetsPath = rootVisualElement.Q<TextField>("AssetPath");
        _assetsContainer = rootVisualElement.Q<VisualElement>("AssetsContainer");
        _favoritesContainer = rootVisualElement.Q<VisualElement>("FavoriteContainer");

        rootVisualElement.Q<Button>("DeleteLevelBtn").clicked += () =>
        {
            EditorApplication.Beep();
            if (EditorUtility.DisplayDialog("레벨 삭제", 
                    "현재 배치된 모든 레벨을 삭제하시겠습니까?", "확인", "취소") == true)
            {
                FindFirstObjectByType<MapBuilder>()?.DeleteLevelData();
            }
        };
    }

    private void LoadAssets()
    {
        if (string.IsNullOrEmpty(_assetsPath.value)) return;
        
        MapBuilderAssetLoader.LoadAllAssets(_assetsPath.value);
        _assetsDropdownField.choices = new List<string>(MapBuilderAssetLoader.BuilderAssetData.Keys);
    }

    private void OnCategoryChanged(ChangeEvent<string> evt)
    {
        _assetsContainer.Clear();
        
        var category = _assetsDropdownField.value;
        var previewList = new List<(TemplateContainer uxml, GameObject prefab)>();

        foreach (var asset in MapBuilderAssetLoader.BuilderAssetData[category])
        {
            var uxml = CreateAssetUxml(asset, previewList);
            _assetsContainer.Add(uxml);
        }
        
        AddLoadPreviewSchedule(previewList);
    }

    private TemplateContainer CreateAssetUxml(BuilderAssetData asset, 
         List<(TemplateContainer uxml, GameObject prefab)> previewList)
    {
        var uxml = assetsUxml.CloneTree();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.Path);

        uxml.name = asset.Guid;
        uxml.Q<Label>("AssetName").text = asset.Name;
        uxml.Q<Button>("AssetSelectBtn").clicked += () => OnAssetButtonClicked(asset);
        uxml.Q<Button>("FavoriteBtn").clicked += () => OnFavButtonSelected(asset);
            
        previewList.Add((uxml, prefab));

        return uxml;
    }

    private void AddLoadPreviewSchedule(List<(TemplateContainer uxml, GameObject prefab)> previewList)
    {
        if (previewList == null || previewList.Count == 0) return;
        
        rootVisualElement.schedule.Execute(() =>
        {
            for (var i = previewList.Count - 1; i >= 0; i--)
            {
                var img = AssetPreview.GetAssetPreview(previewList[i].prefab);

                if (img == null)
                {
                    continue;
                }
                
                previewList[i].uxml.Q<Button>("AssetSelectBtn").style.backgroundImage = img;
                previewList.RemoveAt(i);
            }
            
        }).Until(() => previewList.Count == 0).Every(100);
    }

    private void OnAssetButtonClicked(BuilderAssetData asset)
    {
        CurrentSelectedAsset = asset;
        OnAssetSelected?.Invoke(asset);
    }

    private void OnFavButtonSelected(BuilderAssetData asset)
    {
        if(_favoritesData == null) LoadOrCreateFavData();

        if (!_favoritesData.ToggleFavorite(asset.Guid))
        {
            var element = _favoritesContainer.Q(asset.Guid);
            _favoritesContainer.Remove(element);
        }
        else
        {
            var previewList =  new List<(TemplateContainer uxml, GameObject prefab)>();
            var uxml = CreateAssetUxml(asset, previewList);
            
            ApplyFavStyle(uxml);
            
            _favoritesContainer.Add(uxml);
            AddLoadPreviewSchedule(previewList);
        }
    }

    private void LoadOrCreateFavData()
    {
        _favoritesData = AssetDatabase.LoadAssetAtPath<FavAssetsData>(FAV_DATA_PATH);

        if (_favoritesData == null)
        {
            _favoritesData = CreateInstance<FavAssetsData>();
            AssetDatabase.CreateAsset(_favoritesData, FAV_DATA_PATH);
            AssetDatabase.SaveAssets();
        }
    }

    private void RestoreFavData()
    {
        if (_favoritesData == null || _favoritesData.FavAssetGuids.Count == 0) return;
        
        var prevList = new List<(TemplateContainer uxml, GameObject prefab)>();

        foreach (var guid in _favoritesData.FavAssetGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var assetName = Path.GetFileNameWithoutExtension(path);
            var category = Path.GetFileName(Path.GetDirectoryName(path));
            var assetData = new BuilderAssetData(path, assetName, guid, category);
            
            var uxml  = CreateAssetUxml(assetData, prevList);
            ApplyFavStyle(uxml);
            
            _favoritesContainer.Add(uxml);
        }
        
        AddLoadPreviewSchedule(prevList);
    }

    private void ApplyFavStyle(TemplateContainer uxml)
    {
        var element = uxml.Q<VisualElement>("AssetInfo");
        
        uxml.Q<Label>("AssetName").style.fontSize = 12;
        element.style.width = element.style.height = 125;
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        Instance = null;
    }
}
