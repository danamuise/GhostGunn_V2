using UnityEngine;
using TMPro;
using System.Collections;
public class TargetBehavior : MonoBehaviour
{
    [Header("Target Settings")]
    private int health;
    public SpriteRenderer targetSprite;
    public TextMeshProUGUI label;

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
        //This causes the bullet to bounce off the target as it's being destroyed.
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

    public void AnimateToPosition(Vector2 targetPosition, float duration = 0.5f, bool fromEndzone = false)
    {
        Vector2 startPosition = fromEndzone
            ? new Vector2(targetPosition.x, 5.35f) // use top only for new spawns
            : (Vector2)transform.position;

        StopAllCoroutines();
        StartCoroutine(SlideToPosition(startPosition, targetPosition, duration));
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
