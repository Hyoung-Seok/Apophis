using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Renderer cellRenderer;
    
    private MaterialPropertyBlock _mpb;
    private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
    
    public void ChangeAlpha(float alpha)
    {
        _mpb ??= new MaterialPropertyBlock();
        
        var color = cellRenderer.sharedMaterial.GetColor(BASE_COLOR);
        color.a = alpha;
        
        _mpb.SetColor(BASE_COLOR, color);
        cellRenderer.SetPropertyBlock(_mpb);
    }
}
