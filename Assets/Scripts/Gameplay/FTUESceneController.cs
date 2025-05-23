using UnityEngine;
using UnityEngine.SceneManagement;

public class FTUESceneController : MonoBehaviour
{
    private bool hasClicked = false;

    void Update()
    {
        if (!hasClicked && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            hasClicked = true;
            LoadMainScene();
        }
    }

    void LoadMainScene()
    {
        PlayerPrefs.SetInt("HasSeenFTUE", 1); // optional: one-time flag
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1Scene");
    }
}

