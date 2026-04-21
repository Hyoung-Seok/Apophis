using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter
{
    private readonly MapBuilder _mapBuilder;

    private int _prevHoverCellIndex = 0;
    private Renderer _prevRemoveHoverAssetRenderer;
    private const float ORIGIN_ALPHA = 0.3f;
    private const float HIGHLIGHT_ALPHA = 1f;
    
    private HashSet<int> _highlightedCells = new();
    private HashSet<int> _rangeBuffer = new();

    private readonly Color _rangeColor;
    private MaterialPropertyBlock _mpb;
    
    public CellHighlighter(MapBuilder mapBuilder, Color color)
    {
        _mapBuilder = mapBuilder;
        _rangeColor = color;
    }
    
    public void UpdateRangeCellHighlight(int start, int end)
    {
        RecomputeRange(start, end);
        
        foreach (var i in _highlightedCells)
        {
            if(_rangeBuffer.Contains(i) == false)
                _mapBuilder.Cells[i].RestoreColor();
        }

        foreach (var i in _rangeBuffer)
        {
            if(_highlightedCells.Contains(i) == false)
                _mapBuilder.Cells[i].ChangeColor(_rangeColor);
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
            _mapBuilder.Cells[i].RestoreColor();
        _highlightedCells.Clear();
    }
    
    public bool UpdateRemoveHighlight(Renderer objRenderer)
    {
        
        if (objRenderer == null || objRenderer == _prevRemoveHoverAssetRenderer) return false;

        ClearRemoveHighlight();

        _mpb ??= new MaterialPropertyBlock();
        _mpb.SetColor(Cell.BASE_COLOR, Color.red);
        objRenderer.SetPropertyBlock(_mpb);

        _prevRemoveHoverAssetRenderer = objRenderer;
        return true;
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
}
