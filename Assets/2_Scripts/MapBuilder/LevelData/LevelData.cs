using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public string LevelName => LevelName;
    public List<CellPropData> LevelPropData => levelPropData;
    
    [SerializeField] private string levelName;
    [SerializeField] private List<CellPropData> levelPropData;

}
