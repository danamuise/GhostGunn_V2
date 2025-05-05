using UnityEngine;
using TMPro;
using System.Collections;

public class TargetBehavior : MonoBehaviour
{
    [Header("Target Settings")]
    private int health;
    public SpriteRenderer targetSprite;
    public TextMeshProUGUI label;

    // Animation
    private Animator zombieAnimator;

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

        // 🔄 Get Animator
        zombieAnimator = GetComponentInChildren<Animator>();
        if (zombieAnimator == null)
            Debug.LogWarning($"{name} | TargetBehavior could not find Animator");

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

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // 🔊 Play SFX 
            string[] grunts = { "Grunt0", "Grunt1", "Grunt2", "Grunt3", "Grunt4", "Grunt5" };
            SFXManager.Instance.PlayRandom(grunts, 0.5f, 0.6f, 1.3f);
            gm.AddScore(1); // ✅ Add 1 point on every hit
        }

        // Trigger damage animation
        if (zombieAnimator != null)
        {
            zombieAnimator.SetBool("zombie_damage", true);
            Invoke(nameof(ResetDamageAnimation), 0.25f); // Adjust based on your damage animation length
        }

        if (health <= 0)
        {
            StartCoroutine(DestroyAfterDelay(0.1f));
        }
        else
        {
            UpdateVisuals();
        }
    }

    private void ResetDamageAnimation()
    {
        if (zombieAnimator != null)
            zombieAnimator.SetBool("zombie_damage", false);
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

    public void SetOffsetAndRotation(Vector2 offset, float zRotation)
    {
        persistentOffset = offset;
        persistentZRotation = zRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, persistentZRotation);
    }

    public void AnimateToPosition(Vector2 gridAlignedPosition, float duration = 0.5f, bool fromEndzone = false)
    {
        int moveNumber = FindObjectOfType<GameManager>()?.GetMoveCount() ?? -1;
        Debug.Log($"🎯 TRACKING Animating target {name} on move {moveNumber}");

        Vector2 startPosition = fromEndzone
            ? new Vector2(gridAlignedPosition.x, 5.35f)
            : (Vector2)transform.position;

        Vector2 endPosition = gridAlignedPosition + persistentOffset;

        // 🧟 Enable walk animation
        if (zombieAnimator != null)
            zombieAnimator.SetBool("zombie_walk", true);

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

        // 🛑 Stop walking animation
        if (zombieAnimator != null)
            zombieAnimator.SetBool("zombie_walk", false);
    }
}
