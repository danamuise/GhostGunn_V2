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

        List<int> columnIndices = new List<int>();
        for (int i = 0; i < columns; i++)
            columnIndices.Add(i);

        Shuffle(columnIndices);

        int maxAllowed = Mathf.Min(targetsToSpawn, columns - 1); // ensure 1 empty space

        for (int i = 0; i < maxAllowed; i++)
        {
            int col = columnIndices[i];
            Vector2 spawnPos = grid.GetWorldPosition(col, areaIndex);

            GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Count)];
            GameObject newTarget = Instantiate(prefab, spawnPos, Quaternion.identity, targetParent);
            newTarget.transform.localScale = Vector3.one * targetScale;

            TargetBehavior tb = newTarget.GetComponent<TargetBehavior>();
            if (tb != null)
            {
                tb.SetHealth(Random.Range(1, 6));
                tb.AnimateToPosition(spawnPos, 0.5f, fromEndzone: true);
            }

            grid.MarkCellOccupied(col, areaIndex, true);
        }
    }

    public void AdvanceAllTargetsAndSpawnNew(int moveCount)
    {
        int rows = grid.GetRowCount();
        int cols = grid.GetColumnCount();

        // Step 1: Cache existing targets by grid position
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

        // Step 2: Clear the grid (we’ll rebuild it as we go)
        grid.ClearGrid();

        // Step 3: Move targets downward
        foreach (var kvp in targetsToMove)
        {
            int col = kvp.Key.col;
            int row = kvp.Key.row;
            GameObject target = kvp.Value;

            int newRow = row + 1;

            if (newRow >= rows)
            {
                Object.Destroy(target); // Optional: add explosion effect later
                continue;
            }

            Vector2 newPos = grid.GetWorldPosition(col, newRow);
            TargetBehavior tb = target.GetComponent<TargetBehavior>();
            if (tb != null)
                tb.AnimateToPosition(newPos, 0.5f, fromEndzone: false);

            grid.MarkCellOccupied(col, newRow, true);
        }

        // Step 4: Spawn new targets into Area 1
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
