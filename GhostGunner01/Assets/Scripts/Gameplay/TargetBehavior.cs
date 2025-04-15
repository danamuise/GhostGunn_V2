using UnityEngine;
using TMPro;
using System.Collections;

public class TargetBehavior : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Visual References")]
    public SpriteRenderer sr;               // Assign TargetSprite manually or via GetComponentInChildren
    public TextMeshProUGUI healthLabel;     // Auto-found by name "TargetHealth"

    void Start()
    {
        currentHealth = maxHealth;

        // Auto-find SpriteRenderer if not assigned
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        // Auto-find TextMeshProUGUI by name
        if (healthLabel == null)
        {
            foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.name == "TargetHealth")
                {
                    healthLabel = tmp;
                    break;
                }
            }

            if (healthLabel == null)
                Debug.LogWarning($"{name} | ⚠️ Could not find TextMeshProUGUI named 'TargetHealth'");
        }


        UpdateVisuals();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} | Took damage. Current health: {currentHealth}");
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateVisuals();

        if (currentHealth == 0)
        {
            StartCoroutine(Die());
        }
    }

    private void UpdateVisuals()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        if (sr != null)
            sr.color = new Color(1f - healthPercent, healthPercent, 0f);

        if (healthLabel != null)
            healthLabel.text = currentHealth.ToString();
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}
