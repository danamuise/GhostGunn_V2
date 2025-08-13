using UnityEngine;
using UnityEngine.SceneManagement;

public class FTUESceneController : MonoBehaviour
{
    private bool hasClicked = false;
    private void Start()
    {
        // 🔊 Play intro music at 30% volume (or adjust to taste)
        SFXManager.Instance.PlayMusic("mainBGmusic", 0.3f);
    }
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
        SceneManager.LoadScene("MainGameScene");
    }
}

