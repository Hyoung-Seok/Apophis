using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Magazine
{
    public MagazineData Data;
    public BulletData LoadedAmmo;
    public int CurrentAmmo;
    
    public ECaliber Caliber => Data.Caliber;
    public int Capacity => Data.Capacity;
    public bool IsEmpty => CurrentAmmo <= 0;
    public bool IsFull => CurrentAmmo >= Data.Capacity;
}
