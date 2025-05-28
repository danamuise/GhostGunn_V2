using UnityEngine;
using System.Collections;

public class NukePU : MonoBehaviour
{
    private bool isActivated = false;
    private Vector3 consoleTargetPosition = new Vector3(-1.48f, -4.12f, 0f);
    private float travelDuration = 0.75f;
    [SerializeField] private GameObject nukeHitVFXPrefab;
    [SerializeField] private GameObject wordBalloon0;
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

                    // ✅ Show Nuke Word Balloon

                    if (wordBalloon0_ != null)
                    {
                        wordBalloon0_.EnableWordBalloon();
                        Debug.Log("💬 Enable Nuke Word Balloon");
                        // Start coroutine to disable it after 3 seconds
                        StartCoroutine(HideWordBalloonAfterDelay( 3f));
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

        //Destroy(gameObject);
    }

    private IEnumerator HideWordBalloonAfterDelay(float delay)
    {
        Debug.Log("HideWordBalloonAfterDelay");
        yield return new WaitForSeconds(delay);
        if (wordBalloon0 != null)
        {
            wordBalloon0_.HideWordBalloon();
        }
        Destroy(gameObject);
    }
}
