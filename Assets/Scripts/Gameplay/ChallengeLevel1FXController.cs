using UnityEngine;

public class ChallengeLevel1FXCcontroller : MonoBehaviour
{
    // Reference to the SFXManager
    public SFXManager sfxManager;

    // Function to play the stamp sound
    public void PlayStampSound()
    {
        if (sfxManager != null)
        {
            Debug.Log("FoundStampSFX");
            sfxManager.Play("FoundStampSFX");
        }
        else
        {
            Debug.LogError("SFXManager reference is missing!");
        }
    }
}