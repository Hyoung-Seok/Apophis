using System;
using UnityEngine;

public class PlayerViewProvider : MonoBehaviour
{
    [SerializeField] private float viewRadius = 3f;
    
    private static readonly int PlayerPosID =  Shader.PropertyToID("_PlayerPos");
    private static readonly int ViewRadiusID = Shader.PropertyToID("_ViewRadius");

    private void LateUpdate()
    {
        Shader.SetGlobalVector(PlayerPosID, transform.position);
        Shader.SetGlobalFloat(ViewRadiusID, viewRadius);
    }
}
