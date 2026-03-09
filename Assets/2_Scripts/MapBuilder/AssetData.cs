public class AssetData
{
    public string Name { get; }
    public string Path { get; }

    public AssetData(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
