using System;
using UnityEngine;

public class PlayerViewProvider : MonoBehaviour
{
    [SerializeField] private float viewRadius = 3f;
    [SerializeField] private float viewConeAngle = 60f;
    [SerializeField] private float viewConeRange = 8f;
    
    private static readonly int PlayerPosID =  Shader.PropertyToID("_PlayerPos");
    private static readonly int PlayerForwardID = Shader.PropertyToID("_PlayerForward");
    private static readonly int ViewRadiusID = Shader.PropertyToID("_ViewRadius");
    private static readonly int ConeAngleID = Shader.PropertyToID("_ViewConeAngle");
    private static readonly int ConeRange = Shader.PropertyToID("_ViewConeRange");

    private void LateUpdate()
    {
        Shader.SetGlobalVector(PlayerPosID, transform.position);
        Shader.SetGlobalVector(PlayerForwardID, transform.forward);
        
        Shader.SetGlobalFloat(ViewRadiusID, viewRadius);
        Shader.SetGlobalFloat(ConeAngleID, viewConeAngle);
        Shader.SetGlobalFloat(ConeRange, viewConeRange);
    }

    #if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        Shader.SetGlobalVector(PlayerPosID, Vector3.zero);
        Shader.SetGlobalVector(PlayerForwardID, Vector3.zero);
        
        Shader.SetGlobalFloat(ViewRadiusID, viewRadius);
        Shader.SetGlobalFloat(ConeAngleID, viewConeAngle);
        Shader.SetGlobalFloat(ConeRange, viewConeRange);
    }
    #endif
}
