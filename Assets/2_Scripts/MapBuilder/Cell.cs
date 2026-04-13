using UnityEngine;

public class Cell : MonoBehaviour
{
    public static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
    [SerializeField] private Renderer cellRenderer;
    
    private MaterialPropertyBlock _mpb;
    
    public void ChangeAlpha(float alpha)
    {
        _mpb ??= new MaterialPropertyBlock();
        
        var color = cellRenderer.sharedMaterial.GetColor(BASE_COLOR);
        color.a = alpha;
        
        _mpb.SetColor(BASE_COLOR, color);
        cellRenderer.SetPropertyBlock(_mpb);
    }
}
