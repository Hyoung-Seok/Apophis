using UnityEngine;

public class PlaceAssetRenderer
{
    public Renderer[] FloorRenderer;
    public Renderer[][] WallRenderer = new Renderer[4][];

    public void ApplyBlock(MaterialPropertyBlock mpb)
    {
        if (FloorRenderer != null)
        {
            foreach (var floor in FloorRenderer)
            {
                if (floor == null) continue;
                floor.SetPropertyBlock(mpb);
            }
        }

        for (var i = 0; i < 4; ++i)
        {
            if (WallRenderer[i] == null) continue;
            foreach (var wall in WallRenderer[i])
            {
                if (wall == null) continue;
                wall.SetPropertyBlock(mpb);
            }
        }
    }

    public void ClearBlock() => ApplyBlock(null);
}
