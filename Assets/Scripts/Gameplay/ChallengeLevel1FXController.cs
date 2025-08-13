using UnityEngine;

public class ChallengeLevel1FXCcontroller : MonoBehaviour
{
    // Reference to the SFXManager
    public SFXManager sfxManager;

    private void Start()
    {
        // Fallback to singleton if missing
        if (sfxManager == null && SFXManager.Instance != null)
        {
            sfxManager = SFXManager.Instance;
            Debug.Log("✅ SFXManager auto-assigned from singleton in Start()");
        }
    }

    // Function to play the stamp sound
    public void PlayStampSound()
    {
        var sm = sfxManager != null ? sfxManager : SFXManager.Instance;

        if (sm != null)
        {
            Debug.Log("FoundStampSFX");
            sm.Play("FoundStampSFX");
        }
        else
        {
            Debug.LogError("SFXManager reference is missing even after fallback!");
        }
    }
}
