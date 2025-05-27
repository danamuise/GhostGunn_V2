using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class NukePU : MonoBehaviour
{

    private bool isActivated = false;
    private Vector3 consoleTargetPosition = new Vector3(-1.48f, -4.12f, 0f);
    private float travelDuration = 0.75f;
    [SerializeField] private GameObject nukeHitVFXPrefab;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;
        if (!other.CompareTag("Bullet")) return;

        isActivated = true;

        // Optional: disable collider so it can't be triggered again
        GetComponent<Collider2D>().enabled = false;

        // Optional: play a particle or flash

        StartCoroutine(AnimateToConsole(consoleTargetPosition, travelDuration));
    }

    private IEnumerator AnimateToConsole(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = Mathf.Pow(t, 3); // Slow start, fast end
            transform.position = Vector3.Lerp(start, target, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        try
        {
            GameObject nukeIcon = GameObject.Find("NukeIcon");
            if (nukeIcon != null)
            {
                SpriteRenderer sr = nukeIcon.GetComponent<SpriteRenderer>();
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
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ nukeHitVFXPrefab is not assigned in the Inspector!");
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


        // ✅ Destroy this power-up object
        Destroy(gameObject);
    }

}
