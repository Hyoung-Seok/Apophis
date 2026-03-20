using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaletteCustomEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset paletteUxml;

    private static readonly Vector2Int WINDOW_SIZE = new (1280, 720);

    public static void ShowWindow()
    {
        var window = GetWindow<PaletteCustomEditor>();
        
        window.titleContent = new GUIContent("Palette Custom Editor");
        window.minSize = window.maxSize = WINDOW_SIZE;
    }

    public void CreateGUI()
    {
        paletteUxml.CloneTree(rootVisualElement);
    }
}
