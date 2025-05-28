using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeWordBaloons : MonoBehaviour
{
    public GameObject wordballoon0; // Prefab assigned in Inspector
    private GameObject currentBalloonInstance;

    public void EnableWordBalloon()
    {
        Debug.Log("Enabling NukeWB0");

        if (wordballoon0 != null && currentBalloonInstance == null)
        {
            Vector3 spawnPos = new Vector3(0.0f, -2.4f, 0.0f);
            currentBalloonInstance = Instantiate(wordballoon0, spawnPos, Quaternion.identity);
        }
    }

    public void HideWordBalloon()
    {
        Debug.Log("Disabling NukeWB0");

        if (currentBalloonInstance != null)
        {
            Destroy(currentBalloonInstance);
            currentBalloonInstance = null;
        }
    }
}

