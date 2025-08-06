using UnityEngine;

public class SWTarget : MonoBehaviour
{
    [Header("Type of Special Weapon this power-up represents")]
    public SpecialWeaponType weaponType;

    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated || !other.CompareTag("Bullet")) return;

        isActivated = true;

        // Disable collider to prevent repeat triggers
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Enable and play optional PickupVFX if present
        Transform vfx = transform.Find("PickupVFX");
        if (vfx != null)
        {
            vfx.gameObject.SetActive(true);
            Destroy(vfx.gameObject, 2f); // optional cleanup
        }

        // Play pickup sound
        SFXManager.Instance.Play("PUCollect");

        // Notify special weapon manager
        SpecialWeapons specialWeaponsManager = FindObjectOfType<SpecialWeapons>();
        if (specialWeaponsManager != null)
        {
            specialWeaponsManager.OnSpecialWeaponCollected(weaponType, gameObject);
        }
        else
        {
            Debug.LogWarning("⚠️ SpecialWeapons manager not found in scene.");
        }

        // ❌ Do NOT destroy here — SpecialWeapons handles object lifecycle
    }
}
