using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
    public Transform targetsParent; // Drag TargetsParent GO here in the Inspector

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

    private HashSet<Vector2Int> recentPowerUpPositions = new HashSet<Vector2Int>();
    private const int maxRecentPositions = 5; // Track last 5 placements to avoid repetition

    private void Start()
    {

        //FindObjectOfType<TargetManager>()?.SpawnInitialRow();
    }

    public void SpawnTargetsInArea(int rowIndex, int moveCount)
    {
        Debug.Log($"📦 SpawnTargetsInArea({rowIndex}) called at {Time.time:F2} seconds");

        if (rowIndex == 0)
        {
            Debug.LogWarning($"🚨 Area 1 spawn triggered from: {System.Environment.StackTrace}");
        }

        if (Camera.main == null) return;
        if (rowIndex < 0 || rowIndex >= gridRowYPositions.Length) return;

        float baseY = gridRowYPositions[rowIndex] + gridYOffset;
        float verticalJitter = Mathf.Min(rowSpacing / 2f - targetRadius, 0.1f);

        float leftBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        float rightBound = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

        float usableWidth = (rightBound - leftBound) - (2f * horizontalSpawnMargin);
        float minX = leftBound + horizontalSpawnMargin;
        float maxX = rightBound - horizontalSpawnMargin;
   
        int maxFittable = Mathf.Max(4, Mathf.FloorToInt(usableWidth / (targetRadius * 2.0f)));

        Debug.LogWarning($"🚨 MAX FITTABLE***********************: "+ maxFittable);
        int targetCount = Random.Range(3, maxFittable + 1);

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
                if (Vector2.Distance(pos, candidate) < targetRadius * 1.6f) // 👈 This is your overlap logic
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
            CreateTarget(pos, rowIndex, moveCount);
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

        GameObject newTarget = Instantiate(prefab, spawnPos, rot, targetsParent);
        return newTarget;
    }

    public IEnumerator MoveTargetsDown(float duration = 0.35f, float ease = 2.5f)
    {
        List<GameObject> targets = GetActiveTargets();
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();
        List<GameObject> validTargets = new List<GameObject>();

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
                continue;

            validTargets.Add(target);
            startPositions.Add(start);
            endPositions.Add(end);
            targetRowLookup[target] = nextRow;
        }

        // Power-up movement setup
        List<Transform> powerUps = new List<Transform>();
        List<Vector3> puStartPositions = new List<Vector3>();
        List<Vector3> puEndPositions = new List<Vector3>();
        /*
                if (powerUpParent != null)
                {
                    foreach (Transform pu in powerUpParent)
                    {
                        if (pu == null) continue;

                        Vector3 start = pu.position;
                        Vector3 end = new Vector3(start.x, start.y - rowSpacing, start.z);

                        powerUps.Add(pu);
                        puStartPositions.Add(start);
                        puEndPositions.Add(end);
                    }
                }
        */
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

            for (int i = 0; i < powerUps.Count; i++)
            {
                if (powerUps[i] != null)
                {
                    Vector3 puInterpolated = Vector3.Lerp(puStartPositions[i], puEndPositions[i], eased);
                    powerUps[i].position = puInterpolated;
                }
            }

            yield return null;
        }

        // Final snap to end positions (optional)
        for (int i = 0; i < powerUps.Count; i++)
        {
            if (powerUps[i] != null)
                powerUps[i].position = puEndPositions[i];
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

    private void CreateTarget(Vector2 pos, int rowIndex, int moveCount)
    {
        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
        GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];

        Vector2 spawnAbovePos = new Vector2(pos.x, 5.35f);
        GameObject newTarget = Instantiate(prefab, spawnAbovePos, rot, targetsParent);
        newTarget.transform.localScale = Vector3.one * targetScale;

        TargetBehavior behavior = newTarget.GetComponent<TargetBehavior>();
        if (behavior != null)
        {
            behavior.SetHealth(GetHealthForMove(moveCount));
            //Debug.Log($"🎯 Target spawned with health {behavior.SetHealth} on move {moveCount}");
            behavior.AnimateToPosition(pos);
        }

        activeTargets.Add(newTarget);
        targetRowLookup[newTarget] = rowIndex;
    }


    private bool IsPositionFree(Vector2 pos)
    {
        float checkRadius = targetRadius * 1.4f;

        foreach (var target in activeTargets)
        {
            if (target == null) continue;

            float dist = Vector2.Distance(target.transform.position, pos);
            if (dist < checkRadius)
            {
                // Optional: add debug to see what’s too close
                Debug.Log($"<color=yellow>⚠️ PU too close to {target.name} at {target.transform.position} (dist={dist:F2})</color>");
                return false;
            }
        }

        return true;
    }

    public void InitializeGrid()
    {
        gridRowYPositions = new float[numberOfRows];
        for (int i = 0; i < numberOfRows; i++)
        {
            gridRowYPositions[i] = topRowY - i * rowSpacing;
        }

        startX = -((columns - 1) * columnSpacing) / 2f;
    }

    private int GetHealthForMove(int move)
    {
        if (move <= 3) return 1;
        if (move <= 6) return Random.Range(3, 6);
        if (move <= 10) return Random.Range(5, 11);
        if (move <= 20) return Random.Range(5, 31);
        if (move <= 30) return Random.Range(10, 101);
        return Random.Range(10, 201);
    }


    public Vector2? GetAvailablePowerUpPosition(int rowIndex)
    {
        List<GameObject> rowTargets = GetActiveTargets().FindAll(t =>
            targetRowLookup.ContainsKey(t) && targetRowLookup[t] == rowIndex);

        if (rowTargets.Count < 2)
        {
            Debug.Log($"<color=orange>🟠 Not enough targets to test for PU spacing in row {rowIndex}.</color>");
            return null;
        }

        // Shuffle to randomize which pair we test
        ShuffleList(rowTargets);

        float rowY = gridRowYPositions[rowIndex] + gridYOffset;

        for (int i = 0; i < rowTargets.Count - 1; i++)
        {
            Vector3 a = rowTargets[i].transform.position;
            Vector3 b = rowTargets[i + 1].transform.position;
            float spacing = Mathf.Abs(b.x - a.x);

            Debug.Log($"<color=teal>🔍 Pair {i}: {a.x:F2} → {b.x:F2} | spacing = {spacing:F2}</color>");

            if (spacing > targetRadius * 2.4f) // Use slightly more aggressive threshold
            {
                float midpointX = (a.x + b.x) / 2f;
                float verticalJitter = Random.Range(-0.05f, 0.05f);
                Vector2 finalPos = new Vector2(midpointX, rowY + verticalJitter);

                float checkRadius = targetRadius * 0.9f; // Detection zone radius
                Collider2D hit = Physics2D.OverlapCircle(finalPos, checkRadius);

                if (hit == null || !hit.CompareTag("Target"))
                {
                    Debug.DrawLine(
                        new Vector3(finalPos.x - 0.2f, finalPos.y, 0f),
                        new Vector3(finalPos.x + 0.2f, finalPos.y, 0f),
                        Color.green, 2f);

                    Debug.Log($"<color=cyan>✅ PU midpoint clear at {finalPos} (X={midpointX:F2})</color>");
                    return finalPos;
                }
                else
                {
                    Debug.DrawLine(
                        new Vector3(finalPos.x - 0.2f, finalPos.y, 0f),
                        new Vector3(finalPos.x + 0.2f, finalPos.y, 0f),
                        Color.red, 2f);

                    Debug.Log($"<color=red>❌ PU midpoint blocked by collider at {finalPos} (X={midpointX:F2})</color>");
                }
            }
        }

        Debug.Log($"<color=grey>⚪ No PU spawn space found in row {rowIndex}</color>");
        return null;
    }




    // Add this anywhere inside the TargetManager class
    public float GetRowYPosition(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= numberOfRows)
            return 0f;

        return gridRowYPositions[rowIndex] + gridYOffset;
    }

    public float GetRowSpacing()
    {
        return rowSpacing;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }

    public int GetTargetCountInRow(int rowIndex)
    {
        return GetActiveTargets().Count(t =>
            t != null && targetRowLookup.ContainsKey(t) && targetRowLookup[t] == rowIndex);
    }

}
