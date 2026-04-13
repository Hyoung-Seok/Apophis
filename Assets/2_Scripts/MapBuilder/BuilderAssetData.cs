using UnityEngine;

public class BuilderAssetData
{
    public string Path { get; }
    public string Name { get; }
    public string Guid { get; }
    public string Category { get; }
    
    public BuilderAssetData(string path, string name, string guid, string category)
    {
        Path = path;
        Name = name;
        Guid = guid;
        Category = category;
    }
}
