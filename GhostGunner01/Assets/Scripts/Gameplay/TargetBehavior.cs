using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class TargetBehavior : MonoBehaviour
{
    public int maxHealth = 20;
    private int currentHealth;

    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateColor();

        if (currentHealth == 0)
        {
            StartCoroutine(Die());
        }
    }
    IEnumerator Die()
    {
        // Optional: Play death anim, fade out, etc.
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
    void UpdateColor()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        Color color = new Color(1f - healthPercent, healthPercent, 0f); // Red to green
        sr.color = color;
    }
}
