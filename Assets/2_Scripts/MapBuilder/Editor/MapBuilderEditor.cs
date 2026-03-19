using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;
    
    private MapBuilder _mapBuilder;
    private VisualElement _root;
    
    public override VisualElement CreateInspectorGUI()
    {
        _root = visualTreeAsset.CloneTree();
        _mapBuilder = (MapBuilder)target;

        return _root;
    }
}
