using UnityEngine;

public class SWIconTarget : MonoBehaviour
{
    private bool isArmed = false;
    public SpecialWeaponType type; // Set in Inspector (e.g., Nuke, Fire)
    public GameObject triggerEffectObject; // Usually a NukeSequence or FireSequence GameObject

    private SpriteRenderer sr;
    private Color tintedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Darker tint
    private Color normalColor = Color.white;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = tintedColor; // Start dimmed
        }
        else
        {
            Debug.LogWarning($"⚠️ SWIconTarget ({type}) is missing a SpriteRenderer.");
        }
    }

    /// <summary>
    /// Called externally by SpecialWeapons.cs as charge progresses
    /// </summary>
    public void SetChargeProgress(float percent)
    {
        if (sr == null) return;

        if (percent >= 1f)
        {
            sr.color = normalColor;
            Arm();
        }
        else
        {
            sr.color = tintedColor;
        }
    }

    public void Arm()
    {
        if (!isArmed)
        {
            isArmed = true;
            Debug.Log($"🧨 {type} icon armed and ready.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isArmed || !other.CompareTag("Bullet"))
            return;

        Debug.Log($"💥 {type} triggered — icon hit by bullet!");

        if (triggerEffectObject != null)
        {
            triggerEffectObject.SetActive(true);
            Debug.Log($"🚀 {type} sequence enabled!");
            GameState.Instance.SaveState();
        }
        else
        {
            Debug.LogWarning($"⚠️ {type} trigger effect object is missing!");
        }

        Destroy(gameObject);
    }
}
