using UnityEngine;

[CreateAssetMenu(fileName = "MagazineData", menuName = "Weapon/MagazineData")]
public class MagazineData : ScriptableObject
{
    public ECaliber Caliber => caliber;
    public int Capacity => capacity;
    public string DisplayName => displayName;
    
    [SerializeField] private ECaliber caliber;
    [SerializeField] private int capacity;
    [SerializeField] private string displayName;
}
