using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    [SerializeField] private VisualTreeAsset mapBuilderAsset;
    private VisualElement _root;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = mapBuilderAsset.CloneTree();
        return _root;
    }
}
