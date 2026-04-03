using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public string LevelName => LevelName;
    public List<CellAssetsData> CellAssetData => cellAssetData;
    public List<FreeAssetData> FreeAssetData => freeAssetData;
    
    [SerializeField] private string levelName;
    [SerializeField] private List<CellAssetsData> cellAssetData;
    [SerializeField] private List<FreeAssetData> freeAssetData;

}
