using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
    [SerializeField] private Renderer cellRenderer;
    
    private MaterialPropertyBlock _mpb;
    private Color _originColor;
    private Color _curRgb;
    private float _currentAlpha;


    public void Init()
    {
        if (cellRenderer == null) return;
        _originColor  = cellRenderer.sharedMaterial.GetColor(BASE_COLOR);
        _curRgb       = _originColor;
        _currentAlpha = _originColor.a;
    }

    public void ChangeAlpha(float alpha)
    {
        _currentAlpha = alpha;
        Apply();
    }

    public void ChangeColor(Color color)
    {
        _curRgb = color;
        Apply();
    }

    public void RestoreColor()
    {
        _curRgb = _originColor;
        Apply();
    }

    private void Apply()
    {
        if (cellRenderer == null) return;
        _mpb ??= new MaterialPropertyBlock();

        var c = _curRgb;
        c.a = _currentAlpha;
        
        _mpb.SetColor(BASE_COLOR, c);
        cellRenderer.SetPropertyBlock(_mpb);
    }
}
