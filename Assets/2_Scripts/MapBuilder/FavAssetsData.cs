using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FavAssetsData : ScriptableObject
{
    public IReadOnlyList<string> FavAssetGuids => favAssetGuids;
    [SerializeField] private List<string> favAssetGuids = new();

    public bool ToggleFavorite(string guid)
    {
        if (favAssetGuids.Contains(guid) == true)
        {
            favAssetGuids.Remove(guid);
            EditorUtility.SetDirty(this);
            return false;
        }
        
        favAssetGuids.Add(guid);
        EditorUtility.SetDirty(this);
        return true;
    }
}
