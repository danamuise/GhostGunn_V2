using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CL1_buttonHandler : MonoBehaviour

{
    public ChallengeMode1 challengeManager;

    private void OnMouseDown()
    {
        // This works for mouse click and touch
        challengeManager.OnAdvanceButtonClicked();
    }
}
