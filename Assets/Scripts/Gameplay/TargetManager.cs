using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public static bool blockAdvance = false; // 🚫 Prevents targets from advancing

    [Header("Scene References")]
    public Transform targetsParent;

    [Header("Animation Settings")]
    public float moveDuration = 0.35f;
    public float easing = 2.5f;

    [Header("Game Over Settings")]
    public float gameOverY = -3.0f;

    private void Start()
    {
        blockAdvance = false; // Reset on scene load
    }

    /// <summary>
    /// Moves all targets down by a fixed distance (typically row height).
    /// </summary>
    public IEnumerator MoveTargetsDown(float rowSpacing)
    {
        if (blockAdvance)
        {
            Debug.Log("🚫 Target advance blocked by Nuke!");
            yield break;
        }

        List<Transform> targets = new List<Transform>();
        foreach (Transform child in targetsParent)
        {
            if (child != null)
                targets.Add(child);
        }

        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();

        foreach (Transform target in targets)
        {
            Vector3 start = target.position;
            Vector3 end = new Vector3(start.x, start.y - rowSpacing, start.z);
            startPositions.Add(start);
            endPositions.Add(end);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float eased = 1f - Mathf.Pow(1f - t, easing);

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    Vector3 pos = Vector3.Lerp(startPositions[i], endPositions[i], eased);
                    if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y))
                        targets[i].position = pos;
                }
            }

            yield return null;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
                targets[i].position = endPositions[i];
        }
    }

    public void ClearAllTargets()
    {
        foreach (Transform t in targetsParent)
        {
            if (t != null)
                Destroy(t.gameObject);
        }
    }
}
