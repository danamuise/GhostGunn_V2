using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeMode1 : MonoBehaviour
{
    void Start()
    {
        PlayChallengeMusic(); // 🔥 Automatically play music when Challenge Mode starts
    }

    void Update()
    {

    }

    // 📢 Plays the challenge zone music
    public void PlayChallengeMusic()
    {
        SFXManager.Instance.PlayMusic("challengeZone1", 0.5f); // adjust volume as needed
        Debug.Log("🎵 Playing challengeZone1 music");
    }
}
