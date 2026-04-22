using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter
{
    private readonly MapBuilder _mapBuilder;
    private PlaceAssetRenderer[] _placeAssetRenderers;

    private int _prevHoverCellIndex = 0;
    private Renderer _prevRemoveHoverAssetRenderer;
    private const float ORIGIN_ALPHA = 0.3f;
    private const float HIGHLIGHT_ALPHA = 1f;
    
    private HashSet<int> _highlightedCells = new();
    private HashSet<int> _rangeBuffer = new();

    private readonly Color _rangeColor;
    private MaterialPropertyBlock _mpb;
    private MaterialPropertyBlock _assetMpb;
    
    public CellHighlighter(MapBuilder mapBuilder, Color color)
    {
        _mapBuilder = mapBuilder;
        _rangeColor = color;
        
        _placeAssetRenderers = new PlaceAssetRenderer[mapBuilder.Cells.Length];
        for (var i = 0; i < _placeAssetRenderers.Length; i++)
        {
            _placeAssetRenderers[i] = new PlaceAssetRenderer();
        }
    }

    public void RegisterFloorRenderer(int index, GameObject floor)
    {
        var renderer = floor.GetComponentInChildren<Renderer>();
        _placeAssetRenderers[index].FloorRenderer = renderer;
    }

    public void RegisterWallRenderer(int index, int rot, GameObject wall)
    {
        var renderer = wall.GetComponentInChildren<Renderer>();
        _placeAssetRenderers[index].WallRenderer[rot] = renderer;
    }
    
    public void UpdateRangeCellHighlight(int start, int end)
    {
        RecomputeRange(start, end);
        
        foreach (var i in _highlightedCells)
        {
            if (_rangeBuffer.Contains(i))
            {
                continue;
            }

            _mapBuilder.Cells[i].RestoreColor();
            RestoreAssetColor(_placeAssetRenderers[i]);
        }

        foreach (var i in _rangeBuffer)
        {
            if (_highlightedCells.Contains(i))
            {
                continue;
            }

            _mapBuilder.Cells[i].ChangeColor(_rangeColor);
            ChangeAssetColor(_placeAssetRenderers[i]);
        }

        (_highlightedCells, _rangeBuffer) = (_rangeBuffer, _highlightedCells);
    }
    
    public bool UpdateCellHighlight(int index)
    {
        if (index == _prevHoverCellIndex) return false;
        
        var curCell = _mapBuilder.Cells[index];
        curCell.ChangeAlpha(HIGHLIGHT_ALPHA);
        
        _mapBuilder.Cells[_prevHoverCellIndex].ChangeAlpha(ORIGIN_ALPHA);
        
        _prevHoverCellIndex = index;
        return true;
    }

    public void ClearHoverCellHighlight()
    {
        if(_prevHoverCellIndex < _mapBuilder.Cells.Length 
           && _mapBuilder.Cells[_prevHoverCellIndex] != null)
            _mapBuilder.Cells[_prevHoverCellIndex].ChangeAlpha(ORIGIN_ALPHA);
        
        _prevHoverCellIndex = 0;
    }
    
    public void RestoreAllHighlights()
    {
        foreach (var i in _highlightedCells)
        {
            _mapBuilder.Cells[i].RestoreColor();
            RestoreAssetColor(_placeAssetRenderers[i]);
        }
           
        _highlightedCells.Clear();
    }
    
    public void UpdateRemoveHighlight(Renderer objRenderer)
    {
        if (objRenderer == null || objRenderer == _prevRemoveHoverAssetRenderer) return;

        ClearRemoveHighlight();

        _mpb ??= new MaterialPropertyBlock();
        _mpb.SetColor(Cell.BASE_COLOR, Color.red);
        objRenderer.SetPropertyBlock(_mpb);

        _prevRemoveHoverAssetRenderer = objRenderer;
    }
    
    public void ClearRemoveHighlight()
    {
        if (_prevRemoveHoverAssetRenderer == null) return;
        
        _prevRemoveHoverAssetRenderer.SetPropertyBlock(null);
        _prevRemoveHoverAssetRenderer = null;
    }
    
    private void RecomputeRange(int start, int end)
    {
        _rangeBuffer.Clear();

        var start2D = _mapBuilder.Convert1DIndexTo2D(start);
        var end2D   = _mapBuilder.Convert1DIndexTo2D(end);
        var min = Vector2Int.Min(start2D, end2D);
        var max = Vector2Int.Max(start2D, end2D);

        for (var y = min.y; y <= max.y; y++)
        {
            for (var x = min.x; x <= max.x; x++)
            {
                _rangeBuffer.Add(_mapBuilder.Convert2DIndexTo1D(new Vector2Int(x, y)));
            }
        }
    }

    private void ChangeAssetColor(PlaceAssetRenderer asset)
    {
        if(asset.FloorRenderer == null) return;
        
        _assetMpb ??= new MaterialPropertyBlock();
        _assetMpb.SetColor(Cell.BASE_COLOR, _rangeColor);
        asset.FloorRenderer.SetPropertyBlock(_assetMpb);

        for (var i = 0; i < 4; ++i)
        {
            if(asset.WallRenderer[i] == null) continue;
            asset.WallRenderer[i].SetPropertyBlock(_assetMpb);
        }
    }

    private void RestoreAssetColor(PlaceAssetRenderer asset)
    {
        if (asset.FloorRenderer == null) return;
        
        asset.FloorRenderer.SetPropertyBlock(null);
        
        for (var i = 0; i < 4; ++i)
        {
            if(asset.WallRenderer[i] == null) continue;
            asset.WallRenderer[i].SetPropertyBlock(null);
        }

        _assetMpb = null;
    }
}
