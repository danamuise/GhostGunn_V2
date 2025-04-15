using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TargetManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 5;
    public float columnSpacing = 1.5f;
    public float startX;
    [Header("Target Scale Range")]
    [Range(0.1f, 1f)] public float targetScale = 0.5f;

    public float[] gridRowYPositions = new float[]
    {
        4.5f, // Area 1
        3.5f,
        2.5f,
        1.5f,
        0.5f,
        -0.5f,
        -1.5f,
        -2.5f,
        -3.5f,
        -4.5f  // Area 10
    };

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
    }

    public void SpawnTargetsInArea1()
    {
        int spawnCount = Random.Range(minTargets, maxTargets + 1);
        List<int> availableCols = new List<int>();
        for (int i = 0; i < columns; i++) availableCols.Add(i);

        for (int i = 0; i < spawnCount && availableCols.Count > 0; i++)
        {
            int colIndex = availableCols[Random.Range(0, availableCols.Count)];
            availableCols.Remove(colIndex);

            float x = startX + colIndex * columnSpacing;
            float y = gridRowYPositions[0] + gridYOffset; // Apply grid offset here

            Vector2 spawnPos = new Vector2(x, y);
            Quaternion rot = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));

            GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
            GameObject newTarget = Instantiate(prefab, spawnPos, rot);

            // Apply random scale
            float scale = targetScale;
            newTarget.transform.localScale = Vector3.one * scale;

            activeTargets.Add(newTarget);
            targetRowLookup[newTarget] = 0;
        }
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


    private bool IsPositionFree(Vector2 pos)
    {
        foreach (var target in activeTargets)
        {
            if (Vector2.Distance(target.transform.position, pos) < targetRadius * 2f)
                return false;
        }
        return true;
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

}
