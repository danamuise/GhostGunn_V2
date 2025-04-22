using UnityEngine;
using TMPro;
using System.Collections;

public class TargetBehavior : MonoBehaviour
{
    [Header("Target Settings")]
    private int health;
    public SpriteRenderer targetSprite;
    public TextMeshProUGUI label;

    // Persistent visual variation
    private Vector2 persistentOffset = Vector2.zero;
    private float persistentZRotation = 0f;

    private void Awake()
    {
        // Auto-assign SpriteRenderer if missing
        if (targetSprite == null)
            targetSprite = GetComponentInChildren<SpriteRenderer>();

        // Auto-assign TMP label by name if missing
        if (label == null)
        {
            Transform labelTransform = transform.Find("Canvas/TargetHealth");
            if (labelTransform != null)
                label = labelTransform.GetComponent<TextMeshProUGUI>();
        }

        if (label == null)
            Debug.LogWarning($"{name} | TargetBehavior could not find TextMeshPro 'TargetHealth'");

        if (targetSprite == null)
            Debug.LogWarning($"{name} | TargetBehavior could not find SpriteRenderer");

        UpdateVisuals();
    }

    public void SetHealth(int value)
    {
        health = value;
        UpdateVisuals();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{name} | Took {amount} damage — new health: {health}");

        if (health <= 0)
        {
            StartCoroutine(DestroyAfterDelay(0.1f));  // ⏱ Delay destruction
        }
        else
        {
            UpdateVisuals();
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void UpdateVisuals()
    {
        if (label != null)
            label.text = health.ToString();

        if (targetSprite != null)
        {
            float t = Mathf.InverseLerp(0, 20, health); // 0 = green, 20 = red
            targetSprite.color = Color.Lerp(Color.green, Color.red, t);
        }
    }

    /// <summary>
    /// Call once on spawn to define the persistent visual offset/rotation.
    /// </summary>
    public void SetOffsetAndRotation(Vector2 offset, float zRotation)
    {
        persistentOffset = offset;
        persistentZRotation = zRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, persistentZRotation);
    }

    public void AnimateToPosition(Vector2 gridAlignedPosition, float duration = 0.5f, bool fromEndzone = false)
    {
        Vector2 startPosition = fromEndzone
            ? new Vector2(gridAlignedPosition.x, 5.35f)
            : (Vector2)transform.position;

        Vector2 endPosition = gridAlignedPosition + persistentOffset;

        StopAllCoroutines();
        StartCoroutine(SlideToPosition(startPosition, endPosition, duration));
    }

    private IEnumerator SlideToPosition(Vector2 startPos, Vector2 endPos, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }
}
