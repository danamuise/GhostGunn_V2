using UnityEngine;

public class DissolveAndDisable : MonoBehaviour
{
    [Header("Fade Settings")]
    public float startValue = -2.46f;
    public float endValue = -3.22f;
    public float duration = 0.5f;

    private Material material;
    private SpriteRenderer spriteRenderer;
    private float elapsedTime;
    private bool isFading;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            material = spriteRenderer.material;
    }

    public void BeginDissolve()
    {
        Debug.Log("BeginDissolve Called");
        if (material == null || isFading)
            return;

        elapsedTime = 0f;
        isFading = true;
        spriteRenderer.enabled = true;
    }

    private void Update()
    {
        if (!isFading || material == null)
            return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);
        float currentValue = Mathf.Lerp(startValue, endValue, t);
        material.SetFloat("_DirectionalGlowFadeFade", currentValue);

        if (t >= 1f)
        {
            isFading = false;
            spriteRenderer.enabled = false; // 🧼 Reset for pooling
        }
    }
}
