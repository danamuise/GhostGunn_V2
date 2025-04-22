using UnityEngine;
using System.Collections.Generic;

public class GridTargetSpawner : MonoBehaviour
{
    [Header("Target Prefabs (Assigned in Inspector)")]
    public List<GameObject> targetPrefabs;

    [Header("Grid Reference")]
    public TargetGridManager grid;

    [Header("Spawning Rules")]
    public int minTargetsPerRow = 2;
    public int maxTargetsPerRow = 4;
    [Range(0.1f, 1.5f)] public float targetScale = 0.5f;

    [Header("Target Offset Randomization")]
    [Range(0f, 0.5f)] public float positionOffsetRange = 55f;
    [Range(0f, 15f)] public float rotationOffsetRange = 55f;
    private int lastEmptyColumn = -1;
    private Transform targetParent;

    private void Awake()
    {
        GameObject parentObj = GameObject.Find("Targets");
        if (parentObj != null)
        {
            targetParent = parentObj.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ 'Targets' GameObject not found — created one dynamically.");
            targetParent = new GameObject("Targets").transform;
        }
    }

    public void SpawnTargetsInArea(int areaIndex)
    {
        if (targetPrefabs == null || targetPrefabs.Count == 0)
        {
            Debug.LogError("❌ No target prefabs assigned in GridTargetSpawner.");
            return;
        }

        int columns = grid.GetColumnCount();
        int targetsToSpawn = Random.Range(minTargetsPerRow, maxTargetsPerRow + 1);

        // List of column indices
        List<int> columnIndices = new List<int>();
        for (int i = 0; i < columns; i++)
            columnIndices.Add(i);

        // Choose a new empty column, ensuring it's different from the last one
        List<int> possibleEmpty = new List<int>(columnIndices);
        if (lastEmptyColumn != -1) possibleEmpty.Remove(lastEmptyColumn);

        int emptyCol = possibleEmpty[Random.Range(0, possibleEmpty.Count)];
        lastEmptyColumn = emptyCol;

        // Remove the empty column from the spawn pool
        columnIndices.Remove(emptyCol);

        // Shuffle and pick up to maxAllowed targets
        Shuffle(columnIndices);
        int maxAllowed = Mathf.Min(targetsToSpawn, columns - 1); // ensure 1 empty space

        for (int i = 0; i < maxAllowed; i++)
        {
            int col = columnIndices[i];
            Vector2 basePos = grid.GetWorldPosition(col, areaIndex);

            // Offsets
            Vector2 offset = new Vector2(
                Random.Range(-positionOffsetRange, positionOffsetRange),
                Random.Range(-positionOffsetRange, positionOffsetRange)
            );
            float zRotation = Random.Range(-rotationOffsetRange, rotationOffsetRange);

            Vector2 spawnPos = basePos + offset;
            Quaternion spawnRot = Quaternion.Euler(0f, 0f, zRotation);

            GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Count)];
            GameObject newTarget = Instantiate(prefab, spawnPos, spawnRot, targetParent);
            newTarget.transform.localScale = Vector3.one * targetScale;

            TargetBehavior tb = newTarget.GetComponent<TargetBehavior>();
            if (tb != null)
            {
                tb.SetHealth(Random.Range(1, 6));
                tb.SetOffsetAndRotation(offset, zRotation);
                tb.AnimateToPosition(basePos, 0.5f, fromEndzone: true);
            }

            grid.MarkCellOccupied(col, areaIndex, true);
        }
    }


    public void AdvanceAllTargetsAndSpawnNew(int moveCount)
    {
        int rows = grid.GetRowCount();
        int cols = grid.GetColumnCount();

        Dictionary<(int col, int row), GameObject> targetsToMove = new();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid.IsCellOccupied(col, row))
                {
                    Vector2 pos = grid.GetWorldPosition(col, row);
                    Collider2D hit = Physics2D.OverlapPoint(pos);
                    if (hit != null && hit.CompareTag("Target"))
                        targetsToMove.Add((col, row), hit.gameObject);
                }
            }
        }

        grid.ClearGrid();

        foreach (var kvp in targetsToMove)
        {
            int col = kvp.Key.col;
            int row = kvp.Key.row;
            GameObject target = kvp.Value;

            int newRow = row + 1;
            if (newRow >= rows)
            {
                Object.Destroy(target); // 💥 explosion later
                continue;
            }

            Vector2 newBasePos = grid.GetWorldPosition(col, newRow);
            TargetBehavior tb = target.GetComponent<TargetBehavior>();
            if (tb != null)
                tb.AnimateToPosition(newBasePos, 0.5f, fromEndzone: false);

            grid.MarkCellOccupied(col, newRow, true);
        }

        SpawnTargetsInArea(0);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }
}
