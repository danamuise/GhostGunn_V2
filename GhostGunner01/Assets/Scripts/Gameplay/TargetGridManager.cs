using UnityEngine;


public class TargetGridManager : MonoBehaviour
{
    [Header("Grid Dimensions")]
    public int rows = 12;
    public int columns = 5;

    [Header("Cell Settings")]
    public float cellWidth = 1.2f;
    public float cellHeight = 0.9f;

    [Header("Grid Offset")]
    public Vector2 gridOrigin = new Vector2(0f, 4.5f); // top-center of Area 1

    [Header("Debug")]
    public bool showGridGizmos = true;

    private bool[,] gridOccupied;

    public void InitializeGrid()
    {
        gridOccupied = new bool[columns, rows];
        ClearGrid();
    }

    public void ClearGrid()
    {
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                gridOccupied[col, row] = false;
            }
        }
    }

    public void MarkCellOccupied(int col, int row, bool occupied)
    {
        if (col < 0 || col >= columns || row < 0 || row >= rows) return;
        gridOccupied[col, row] = occupied;
    }

    public bool IsCellOccupied(int col, int row)
    {
        if (col < 0 || col >= columns || row < 0 || row >= rows) return true;
        return gridOccupied[col, row];
    }

    public Vector2 GetWorldPosition(int col, int row)
    {
        float startX = -(columns - 1) * 0.5f * cellWidth;
        float x = startX + col * cellWidth;
        float y = gridOrigin.y - row * cellHeight;
        return new Vector2(x, y);
    }

    public int GetColumnCount() => columns;
    public int GetRowCount() => rows;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGridGizmos || !Application.isPlaying) return;

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                Vector2 pos = GetWorldPosition(col, row);
                bool occupied = gridOccupied != null && gridOccupied[col, row];

                Gizmos.color = occupied ? new Color(1f, 0.5f, 0.5f, 0.75f) : new Color(0.3f, 0.9f, 1f, 0.5f);
                Gizmos.DrawWireCube(pos, new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 0));
            }
        }
    }
#endif
}
