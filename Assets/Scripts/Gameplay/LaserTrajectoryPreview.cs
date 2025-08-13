using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LaserTrajectoryPreview : MonoBehaviour
{
    [Header("Visual Settings")]
    public Sprite dotSprite;
    [Range(0.05f, 2f)] public float dotSize = 0.2f;
    [Range(0.01f, 2f)] public float dotSpacing = 0.2f;
    [Range(1f, 100f)] public float lineLength = 20f;
    public Color startColor = new Color(1, 1, 1, 1);
    public Color endColor = new Color(1, 1, 1, 0.2f);
    public int dotSortingOrder = 100;

    [Header("Bounce Settings")]
    [Range(1, 10)] public int maxReflections = 5;
    public LayerMask reflectionMask;

    [Header("Wiggle Effect")]
    public float wiggleIntensity = 0.1f;
    public float wiggleSpeed = 5f;
    public float phaseOffset = 0.5f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private List<GameObject> dotPool = new();
    private List<Vector3> basePositions = new();
    private List<GameObject> activeDots = new();
    private List<Coroutine> activeFades = new();

    public void DrawLaserLine(Vector2 start, Vector2 direction)
    {
        StopAllFadeCoroutines();
        ClearDots(immediate: true);

        Vector2 currentPosition = start;
        Vector2 currentDirection = direction.normalized;
        float remainingLength = lineLength;
        int reflections = 0;
        int totalDotIndex = 0;

        while (reflections < maxReflections && remainingLength > 0f)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, currentDirection, remainingLength, reflectionMask);
            Vector2 endPoint = hit.collider != null ? hit.point : currentPosition + currentDirection * remainingLength;
            float segmentLength = Vector2.Distance(currentPosition, endPoint);

            int dotCount = Mathf.FloorToInt(segmentLength / dotSpacing);
            for (int i = 0; i < dotCount; i++)
            {
                float t = (float)(i + 1) / dotCount;
                Vector2 dotPos = Vector2.Lerp(currentPosition, endPoint, t);
                SpawnDot(dotPos, t, totalDotIndex);
                totalDotIndex++;
            }

            if (hit.collider != null)
            {
                currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                currentPosition = hit.point + currentDirection * 0.001f; // Nudge to avoid re-hit
                remainingLength -= segmentLength;
                reflections++;
            }
            else break;
        }
    }

    private void SpawnDot(Vector2 position, float gradientT, int dotIndex)
    {
        GameObject dot = GetPooledDot();
        if (dot == null) return;

        Vector3 basePos = new Vector3(position.x, position.y, 0f);
        basePositions.Add(basePos);

        dot.transform.position = basePos;
        dot.transform.localScale = Vector3.one * dotSize;

        SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
        sr.color = Color.Lerp(startColor, endColor, gradientT);

        dot.SetActive(true);
        activeDots.Add(dot);
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
            Debug.LogWarning("dotSprite is not assigned.");
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

    public void ClearDots(bool immediate = false)
    {
        if (immediate)
        {
            foreach (var dot in activeDots)
            {
                dot.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < activeDots.Count; i++)
            {
                Coroutine c = StartCoroutine(FadeAndDisable(activeDots[i]));
                activeFades.Add(c);
            }
        }

        activeDots.Clear();
        basePositions.Clear();
    }

    private IEnumerator FadeAndDisable(GameObject dot)
    {
        SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            dot.SetActive(false);
            yield break;
        }

        Color originalColor = sr.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            sr.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }

        sr.color = targetColor;
        dot.SetActive(false);
    }

    private void StopAllFadeCoroutines()
    {
        foreach (var c in activeFades)
        {
            if (c != null) StopCoroutine(c);
        }
        activeFades.Clear();
    }

    private void Update()
    {
        float time = Time.time;

        for (int i = 0; i < activeDots.Count; i++)
        {
            if (i >= basePositions.Count) continue;

            GameObject dot = activeDots[i];
            Vector3 basePos = basePositions[i];

            float phase = time * wiggleSpeed + i * phaseOffset;
            float xOffset = Mathf.Sin(phase) * wiggleIntensity;

            dot.transform.position = basePos + new Vector3(xOffset, 0f, 0f);
        }
    }
}
