using UnityEngine;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TargetManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 5;
    public float columnSpacing = 1.5f;
    public float startX;
    [Header("Spawn Bounds")]
    public float horizontalSpawnMargin = 0.5f; // margin to prevent bullet-trapping at walls

    [Header("Target Scale Range")]
    [Range(0.1f, 1f)] public float targetScale = 0.5f;

    [Header("Vertical Grid Settings")]
    public int numberOfRows = 10;
    public float rowSpacing = 0.75f;
    public float topRowY = 4.5f;

    private float[] gridRowYPositions;


    [Header("Grid Position Offset")]
    public float gridYOffset = 0f; // Shift the entire grid up or down

    [Header("Target Prefabs")]
    public GameObject[] targetPrefabs;

    [Header("Spawn Settings")]
    public int minTargets = 1;
    public int maxTargets = 4;
    public Vector2 area1Min;
    public Vector2 area1Max;
    public float targetRadius = 0.5f; // minimum spacing between targets

    [Header("Game Over Settings")]
    public float gameOverY = -4.5f;  // Adjust to match Area 10’s Y position

    private Dictionary<GameObject, int> targetRowLookup = new Dictionary<GameObject, int>();
    private List<GameObject> activeTargets = new List<GameObject>();

    private void Start()
    {
        startX = -((columns - 1) * columnSpacing) / 2f;

        gridRowYPositions = new float[numberOfRows];
        for (int i = 0; i < numberOfRows; i++)
        {
            gridRowYPositions[i] = topRowY - i * rowSpacing;
        }

    }

    public void SpawnTargetsInArea(int rowIndex)
    {
        if (Camera.main == null) return;
        if (rowIndex < 0 || rowIndex >= gridRowYPositions.Length) return;

        float baseY = gridRowYPositions[rowIndex] + gridYOffset;
        float verticalJitter = Mathf.Min(rowSpacing / 2f - targetRadius, 0.1f);

        float leftBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        float rightBound = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

        float usableWidth = (rightBound - leftBound) - (2f * horizontalSpawnMargin);
        float minX = leftBound + horizontalSpawnMargin;
        float maxX = rightBound - horizontalSpawnMargin;

        int maxFittable = Mathf.Max(3, Mathf.FloorToInt(usableWidth / (targetRadius * 2.5f)));
        int targetCount = Random.Range(3, Mathf.Min(5, maxFittable + 1));

        List<Vector2> spawnPositions = new List<Vector2>();
        int maxAttempts = 300;
        int attempts = 0;

        while (spawnPositions.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(minX + targetRadius, maxX - targetRadius);
            float y = baseY + Random.Range(-verticalJitter, verticalJitter);
            Vector2 candidate = new Vector2(x, y);

            bool overlaps = false;
            foreach (var pos in spawnPositions)
            {
                if (Vector2.Distance(pos, candidate) < targetRadius * 2f)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                spawnPositions.Add(candidate);
            }
        }

        foreach (var pos in spawnPositions)
        {
            CreateTarget(pos, rowIndex);
        }

#if UNITY_EDITOR
        Debug.Log($"[TargetManager] Randomly spawned {spawnPositions.Count} targets in row {rowIndex} after {attempts} attempts");
#endif
    }




    GameObject SpawnTarget(Vector3 spawnPos)
    {
        GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
        float angle = Random.Range(0f, 360f);
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        GameObject newTarget = Instantiate(prefab, spawnPos, rot);
        return newTarget;
    }

    public IEnumerator MoveTargetsDown(float duration = 0.35f, float ease = 2.5f)
    {
        List<GameObject> targets = GetActiveTargets();
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();

        List<GameObject> validTargets = new List<GameObject>(); // <--- new: parallel list

        foreach (GameObject target in targets)
        {
            if (target == null || !targetRowLookup.ContainsKey(target))
                continue;

            int currentRow = targetRowLookup[target];
            int nextRow = currentRow + 1;

            if (nextRow >= gridRowYPositions.Length)
                continue;

            float newY = gridRowYPositions[nextRow] + gridYOffset;
            Vector3 start = target.transform.position;
            Vector3 end = new Vector3(start.x, newY, start.z);

            if (float.IsNaN(newY) || float.IsNaN(start.x) || float.IsNaN(start.y))
                continue; // ❌ Skip if values are corrupted

            validTargets.Add(target);
            startPositions.Add(start);
            endPositions.Add(end);
            targetRowLookup[target] = nextRow;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = 1f - Mathf.Pow(1f - t, ease);

            for (int i = 0; i < startPositions.Count; i++)
            {
                if (validTargets[i] != null)
                {
                    Vector3 interpolated = Vector3.Lerp(startPositions[i], endPositions[i], eased);
                    if (!float.IsNaN(interpolated.x) && !float.IsNaN(interpolated.y) && !float.IsNaN(interpolated.z))
                    {
                        validTargets[i].transform.position = interpolated;
                    }
                }
            }

            yield return null;
        }
    }


    private void ClearDeadTargets()
    {
        activeTargets.RemoveAll(t => t == null);
    }

    public List<GameObject> GetActiveTargets()
    {
        ClearDeadTargets();
        return new List<GameObject>(activeTargets);
    }

    public bool CheckForGameOver()
    {
        foreach (var target in GetActiveTargets())
        {
            if (target != null && target.transform.position.y <= gameOverY)
            {
                return true;
            }
        }
        return false;
    }

    public void ClearAllTargets()
    {
        foreach (GameObject target in GetActiveTargets())
        {
            if (target != null)
                Destroy(target);
        }

        activeTargets.Clear();
        targetRowLookup.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Regenerate gridRowYPositions for editor-time preview
            gridRowYPositions = new float[numberOfRows];
            for (int i = 0; i < numberOfRows; i++)
            {
                gridRowYPositions[i] = topRowY - i * rowSpacing;
            }
        }

        float yMin = gridRowYPositions[gridRowYPositions.Length - 1] + gridYOffset;
        float yMax = gridRowYPositions[0] + gridYOffset;

        // 🔶 Draw horizontal area lines and labels
        for (int i = 0; i < gridRowYPositions.Length; i++)
        {
            float y = gridRowYPositions[i] + gridYOffset;

            // Color-code: red = danger, yellow = middle, green = safe
            if (i >= numberOfRows - 2)
                Gizmos.color = new Color(1f, 0.3f, 0.3f); // red
            else if (i >= numberOfRows - 4)
                Gizmos.color = new Color(1f, 0.8f, 0.2f); // orange-yellow
            else
                Gizmos.color = new Color(0.6f, 1f, 0.6f); // greenish

            Vector3 start = new Vector3(-10f, y, 0f);
            Vector3 end = new Vector3(10f, y, 0f);

            Gizmos.DrawLine(start, end);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(start.x, y + 0.2f, 0f), $"Area {i + 1}");
#endif
        }

        // 🟦 Draw vertical column lines
        Gizmos.color = new Color(0.2f, 0.8f, 1f); // light blue
        for (int i = 0; i < columns; i++)
        {
            float x = startX + i * columnSpacing;
            Vector3 top = new Vector3(x, yMax, 0f);
            Vector3 bottom = new Vector3(x, yMin, 0f);

            Gizmos.DrawLine(top, bottom);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(x - 0.2f, yMax + 0.2f, 0f), $"Col {i + 1}");
#endif
        }
    }

    public bool WillMoveIntoGameOverZone()
    {
        foreach (var target in GetActiveTargets())
        {
            if (target == null || !targetRowLookup.ContainsKey(target))
                continue;

            int currentRow = targetRowLookup[target];
            int nextRow = currentRow + 1;

            if (nextRow >= numberOfRows)
                return true; // Will move into Area 10 (or beyond)
        }

        return false;
    }

    private void CreateTarget(Vector2 pos, int rowIndex)
    {
        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
        GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
        GameObject newTarget = Instantiate(prefab, pos, rot);
        newTarget.transform.localScale = Vector3.one * targetScale;

        activeTargets.Add(newTarget);
        targetRowLookup[newTarget] = rowIndex;
    }

    private bool IsPositionFree(Vector2 pos)
    {
        foreach (var target in activeTargets)
        {
            if (target == null) continue;
            if (Vector2.Distance(target.transform.position, pos) < targetRadius * 2f)
                return false;
        }
        return true;
    }


}
