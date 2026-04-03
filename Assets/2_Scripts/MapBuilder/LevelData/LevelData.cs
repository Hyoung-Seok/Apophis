using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public string LevelName => LevelName;
    public List<CellAssetsData> CellAssetData => cellAssetData;
    
    [SerializeField] private string levelName;
    [SerializeField] private List<CellAssetsData> cellAssetData;

}
