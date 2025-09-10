using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Auto-sizing square cells to fit the the targeted RectTransfrom using the Grid Layout Group.

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGrid : MonoBehaviour
{
    [Header("Target Area")]
    public RectTransform targetArea;

    [Header("Grid")]
    [Min(1)] public int rows = 2;
    [Min(1)] public int cols = 2;

    public Vector2 spacing = new Vector2(8, 8);
    public Vector2 padding = new Vector2(8, 8);

    private GridLayoutGroup _grid;

    private void Awake()
    {
        _grid = GetComponent<GridLayoutGroup>();
        Apply(rows, cols);
    }

    public void Apply(int r, int c)
    {
        rows = Mathf.Max(1, r);
        cols = Mathf.Max(1, c);
        Recalc();
    }

    private void OnRectTransformDimensionsChange()
    {
        Recalc();
    }

    private void OnValidate()
    {
        if (_grid == null)
        {
            _grid = GetComponent<GridLayoutGroup>();
        }

        Recalc();
    }
    private void Recalc()
    {
        if (targetArea == null || rows <= 0 || cols <= 0 || _grid == null)
        {
            return;
        }

        Vector2 area = targetArea.rect.size;

        float totalWidth = padding.x * 2f + spacing.x * (cols - 1);
        float totalHeight = padding.y * 2f + spacing.y * (rows - 1);

        float cellWidth = (area.x - totalWidth) / cols;
        float cellHeight = (area.y - totalHeight) / rows;
        float cell = Mathf.Floor(Mathf.Min(cellWidth, cellHeight)); //The square cells

        _grid.cellSize = new Vector2(cell, cell);
        _grid.spacing = spacing;
        _grid.padding.left = _grid.padding.right = Mathf.RoundToInt(padding.x);
        _grid.padding.top = _grid.padding.bottom = Mathf.RoundToInt(padding.y);
    }

}
