using UnityEngine;
using TMPro;
using System.Collections;
public class TargetBehavior : MonoBehaviour
{
    [Header("Target Settings")]
    public int health = 20;
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

    /*
    public void SetHealth(int value)
    {
        health = value;
        UpdateVisuals();
    }

    */
    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{name} | Took {amount} damage — new health: {health}");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            UpdateVisuals();
        }
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

    public void AnimateToPosition(Vector2 targetPosition, float duration = 0.5f)
    {
        Vector2 startPosition = new Vector2(targetPosition.x, 5.35f);
        transform.position = startPosition;
        StartCoroutine(SlideToPosition(targetPosition, duration));
    }

    private IEnumerator SlideToPosition(Vector2 endPos, float duration)
    {
        Vector2 startPos = transform.position;
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
