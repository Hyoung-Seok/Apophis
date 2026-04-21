using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter
{
    private readonly MapBuilder _mapBuilder;
    
    private HashSet<int> _highlightedCells = new();
    private HashSet<int> _rangeBuffer = new();

    private readonly Color _rangeColor;
    
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
    
    public void RestoreAllHighlights()
    {
        foreach (var i in _highlightedCells)
            _mapBuilder.Cells[i].RestoreColor();
        _highlightedCells.Clear();
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
