using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public ECaliber Caliber => caliber;
    public List<EFireMode> SupportedMode => supportedMode;
    public float FireDelay => fireDelay;
    public float BaseSpreadAngle => baseSpreadAngle;
    public float MaxSpreadAngle => maxSpreadAngle;
    public float SpreadIncRate => spreadIncRate;
    public float SpreadRecRate => spreadRecRate;
    public float HipMaxSpreadAngle => hipMaxSpreadAngle;
    public float HipSpreadIncRate => hipSpreadIncRate;
    public float HipSpreadRecRate => hipSpreadRecRate;

    [SerializeField] private ECaliber caliber;
    [SerializeField] private List<EFireMode> supportedMode;
    [SerializeField] private float fireDelay;
    
    [Header("Aimed")]
    [SerializeField, Range(1f, 20f)] private float baseSpreadAngle;
    [SerializeField, Range(5f, 50f)] private float maxSpreadAngle;
    [SerializeField, Range(0.01f, 1f)] private float spreadIncRate;
    [SerializeField, Range(1f, 50f)] private float spreadRecRate;
    
    [Header("Hip")]
    [SerializeField, Range(1f, 50f)] private float hipMaxSpreadAngle;
    [SerializeField, Range(0.01f, 1f)] private float hipSpreadIncRate;
    [SerializeField, Range(1f, 50f)] private float hipSpreadRecRate;
    
}
