using UnityEngine;

public class BuilderAssetData
{
    public string Path { get; }
    public string Name { get; }
    
    public BuilderAssetData(string path, string name)
    {
        Path = path;
        Name = name;
    }
}
