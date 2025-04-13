using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LaserTrajectoryPreview : MonoBehaviour
{
    [Header("Visual Settings")]
    public Sprite dotSprite;
    [Range(0.05f, 2f)] public float dotSize = 0.2f;
    [Range(1f, 100f)] public float lineLength = 20f;
    [Range(0.01f, 2f)] public float dotSpacing = 0.2f;
    public Color startColor = new Color(1, 1, 1, 1);
    public Color endColor = new Color(1, 1, 1, 0.2f);
    public int dotSortingOrder = 100;


    [Header("Bounce Settings")]
    [Range(1, 10)] public int maxReflections = 5;
    public LayerMask reflectionMask;

    [Header("Wiggle Animation (Coming Soon)")]
    public float wiggleIntensity = 0f;

    private List<GameObject> dotPool = new();
    private List<GameObject> activeDots = new();

    public void DrawLaserLine(Vector2 start, Vector2 direction)
    {
        Debug.Log("Drawing laser line...");
        ClearDots();

        Vector2 currentPosition = start;
        Vector2 currentDirection = direction.normalized;
        float remainingLength = lineLength;
        int reflections = 0;

        while (reflections < maxReflections && remainingLength > 0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, currentDirection, remainingLength, reflectionMask);
            Vector2 endPoint = hit.collider != null ? hit.point : currentPosition + currentDirection * remainingLength;
            float segmentLength = Vector2.Distance(currentPosition, endPoint);
            Debug.DrawLine(currentPosition, endPoint, Color.cyan, 0.1f);


            int dotCount = Mathf.FloorToInt(segmentLength / dotSpacing);
            Debug.Log($"Segment length: {segmentLength}, placing {dotCount} dots.");

            for (int i = 0; i < dotCount; i++)
            {
                float t = (float)(i + 1) / dotCount;
                Vector2 dotPos = Vector2.Lerp(currentPosition, endPoint, t);
                SpawnDot(dotPos, t);
            }

            if (hit.collider != null)
            {
                Debug.Log($"Reflected off: {hit.collider.name} at {hit.point} with normal {hit.normal}");
                currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                currentPosition = hit.point + currentDirection * 0.001f; // Nudge off surface
                remainingLength -= segmentLength;
                reflections++;
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnDot(Vector2 position, float gradientT)
    {
        GameObject dot = GetPooledDot();
        if (dot == null)
        {
            Debug.LogWarning("No dot available from pool!");
            return;
        }

        dot.transform.position = new Vector3(position.x, position.y, 0f);
        dot.transform.localScale = Vector3.one * dotSize;

        SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.Lerp(startColor, endColor, gradientT);
        }
        else
        {
            Debug.LogWarning("SpriteRenderer missing on dot.");
        }

        dot.SetActive(true);
        activeDots.Add(dot);
        Debug.Log($"Spawned dot at {dot.transform.position}");
    }

    private GameObject GetPooledDot()
    {
        foreach (var dot in dotPool)
        {
            if (!dot.activeInHierarchy)
                return dot;
        }

        if (dotSprite == null)
        {
            Debug.LogError("Dot sprite not assigned!");
            return null;
        }

        GameObject newDot = new GameObject("TrajectoryDot");
        newDot.transform.parent = transform;

        SpriteRenderer sr = newDot.AddComponent<SpriteRenderer>();
        sr.sprite = dotSprite;
        sr.sortingOrder = dotSortingOrder;
        sr.color = startColor;

        dotPool.Add(newDot);
        return newDot;
    }

    public void ClearDots()
    {
        foreach (var dot in activeDots)
        {
            if (dot != null)
                dot.SetActive(false);
        }
        activeDots.Clear();
    }
}
