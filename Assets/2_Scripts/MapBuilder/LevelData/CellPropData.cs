using System;
using UnityEngine;

[Serializable]
public class CellPropData
{
    public string FloorPath;
    public ERot90 FloorRot;
    public string[] WallPaths = new string[4];
}

public enum ERot90
{
    D0 = 0,
    D90 = 1,
    D180 = 2,
    D270 = 3
}
