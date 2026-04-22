using System.Collections.Generic;
using UnityEditor;
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

    private bool _isDirty;

    public CellHighlighter(MapBuilder mapBuilder, Color color)
    {
        _mapBuilder = mapBuilder;
        _rangeColor = color;

        _placeAssetRenderers = new PlaceAssetRenderer[mapBuilder.Cells.Length];
        for (var i = 0; i < _placeAssetRenderers.Length; i++)
        {
            _placeAssetRenderers[i] = new PlaceAssetRenderer();
        }

        Undo.undoRedoPerformed += OnUndoRedo;
        _mapBuilder.OnLevelDataDeleted += OnUndoRedo;
        _mapBuilder.OnGridCreated += RebuildFromHierarchy;
    }

    public void Dispose()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        _mapBuilder.OnLevelDataDeleted -= OnUndoRedo;
        _mapBuilder.OnGridCreated -= RebuildFromHierarchy;
    }

    public void RegisterFloorRenderer(int index, GameObject floor)
    {
        _placeAssetRenderers[index].FloorRenderer
            = floor.GetComponentsInChildren<Renderer>(true);
    }

    public void RegisterWallRenderer(int index, int rot, GameObject wall)
    {
        _placeAssetRenderers[index].WallRenderer[rot]
            = wall.GetComponentsInChildren<Renderer>(true);
    }

    public void UpdateRangeCellHighlight(int start, int end)
    {
        CheckRefreshCache();
        RecomputeRange(start, end);

        foreach (var i in _highlightedCells)
        {
            if (_rangeBuffer.Contains(i))
            {
                continue;
            }

            _mapBuilder.Cells[i].RestoreColor();
            _placeAssetRenderers[i].ClearBlock();
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
        if (_prevHoverCellIndex < _mapBuilder.Cells.Length
            && _mapBuilder.Cells[_prevHoverCellIndex] != null)
            _mapBuilder.Cells[_prevHoverCellIndex].ChangeAlpha(ORIGIN_ALPHA);

        _prevHoverCellIndex = 0;
    }

    public void RestoreAllHighlights()
    {
        foreach (var i in _highlightedCells)
        {
            _mapBuilder.Cells[i].RestoreColor();
            _placeAssetRenderers[i].ClearBlock();
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

    public void RebuildFromHierarchy()
    {
        Clear();
        _highlightedCells.Clear();

        var parent = _mapBuilder.LevelParent;
        var floorLayer = LayerMask.NameToLayer("Floor");
        var wallLayer = LayerMask.NameToLayer("Wall");

        for (var i = 0; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i);
            var layer = child.gameObject.layer;
            
            if(!MapBuilderUtil.TryParseIndex(child.name, out var x, out var y)) continue;
            if(layer != floorLayer &&  layer != wallLayer) continue;
            
            var index = _mapBuilder.Convert2DIndexTo1D(new Vector2Int(x, y));
            if(index < 0 || index >= _placeAssetRenderers.Length) continue;

            if (layer == floorLayer)
            {
                RegisterFloorRenderer(index, child.gameObject);
            }
            else if (layer == wallLayer)
            {
                var rot = Mathf.RoundToInt(child.transform.eulerAngles.y / 90f) % 4;
                if(rot < 0) rot += 4;
                RegisterWallRenderer(index, rot, child.gameObject);
            }
        }
    }

    private void RecomputeRange(int start, int end)
    {
        _rangeBuffer.Clear();

        var start2D = _mapBuilder.Convert1DIndexTo2D(start);
        var end2D = _mapBuilder.Convert1DIndexTo2D(end);
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
    
    private void Clear()
    {
        _placeAssetRenderers = new PlaceAssetRenderer[_mapBuilder.Cells.Length];

        for (var i = 0; i < _placeAssetRenderers.Length; ++i)
        {
            _placeAssetRenderers[i] = new PlaceAssetRenderer();
        }
    }

    private void ChangeAssetColor(PlaceAssetRenderer asset)
    {
        _assetMpb ??= new MaterialPropertyBlock();
        _assetMpb.SetColor(Cell.BASE_COLOR, _rangeColor);
        asset.ApplyBlock(_assetMpb);
    }

    private void OnUndoRedo() => _isDirty = true;

    private void CheckRefreshCache()
    {
        if (_isDirty == false) return;
        RebuildFromHierarchy();
        _isDirty = false;
    }
}