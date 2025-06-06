using UnityEngine;
using System.Collections;

public class NukePU : MonoBehaviour
{
    private bool isActivated = false;
    private Vector3 consoleTargetPosition = new Vector3(-1.48f, -3.72f, 0f);
    private float travelDuration = 0.75f;

    [SerializeField] private GameObject nukeHitVFXPrefab;
    [SerializeField] private GameObject wordBalloon0;
    [SerializeField] private GameObject coinSheet0;

    [Header("Power-Up Data Reference")]
    [SerializeField] private PowerUpData powerUpData; // 👈 Add this field for your ScriptableObject

    private NukeWordBaloons wordBalloon0_;

    private void Start()
    {
        wordBalloon0_ = wordBalloon0.GetComponent<NukeWordBaloons>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;
        if (!other.CompareTag("Bullet")) return;

        isActivated = true;
        GetComponent<Collider2D>().enabled = false;

        // 🔊 Play the pickup sound from PowerUpData if assigned
        if (powerUpData != null && !string.IsNullOrEmpty(powerUpData.pickupSFX))
        {
            SFXManager.Instance.Play(powerUpData.pickupSFX);
        }
        else
        {
            Debug.LogWarning("⚠️ PowerUpData or pickupSFX not assigned.");
        }

        StartCoroutine(AnimateToConsole(consoleTargetPosition, travelDuration));
    }

    private IEnumerator AnimateToConsole(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = Mathf.Pow(t, 3); // Ease-in
            transform.position = Vector3.Lerp(start, target, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        GameObject nukeIcon = GameObject.Find("NukeIcon");
        SpriteRenderer sr = nukeIcon?.GetComponent<SpriteRenderer>();

        try
        {
            if (nukeIcon != null)
            {
                if (sr != null)
                {
                    sr.enabled = true;
                    Debug.Log("💥 NukeIcon SpriteRenderer enabled.");

                    if (nukeHitVFXPrefab != null)
                    {
                        GameObject vfxInstance = Instantiate(
                            nukeHitVFXPrefab,
                            nukeIcon.transform.position,
                            Quaternion.identity,
                            nukeIcon.transform
                        );
                        Debug.Log("💨 NukeHit_VFX instantiated at NukeIcon.");

                        // 🔊 Play the NukeIconLand sound
                        SFXManager.Instance.Play("NukeIconLand");
                    }

                    // ✅ Show Nuke Word Balloon
                    if (wordBalloon0_ != null)
                    {
                        wordBalloon0_.EnableWordBalloon();
                        Debug.Log("💬 Enable Nuke Word Balloon");
                        StartCoroutine(HideWordBalloonAfterDelay(3f));
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ NukeOptionWordBalloon0 not found in scene.");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ SpriteRenderer not found on NukeIcon!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ NukeIcon not found in scene!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Exception during NukeIcon SpriteRenderer activation: {e.Message}");
        }

        // 🔴 Disable any visuals left on this object
        foreach (SpriteRenderer srChild in GetComponentsInChildren<SpriteRenderer>(true))
        {
            srChild.enabled = false;
        }

        if (coinSheet0 != null)
        {
            Debug.Log("coinSheet0 found");
            coinSheet0.SetActive(false);
            Destroy(coinSheet0);
        }
        else
        {
            Debug.Log("coinSheet0 not found");
        }
    }

    private IEnumerator HideWordBalloonAfterDelay(float delay)
    {
        Debug.Log("HideWordBalloonAfterDelay");
        yield return new WaitForSeconds(delay);

        if (wordBalloon0_ != null)
        {
            wordBalloon0_.HideWordBalloon();
        }

        Destroy(gameObject);
    }
}
