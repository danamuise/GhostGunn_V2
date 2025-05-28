using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    private void Start()
    {
        // 🔊 Play intro music at 30% volume (or adjust to taste)
        SFXManager.Instance.PlayMusic("IntroMusic", 0.3f);
    }

    // This method can be called by the StartButton's OnClick() event
    public void StartGame()
    {
        SFXManager.Instance.StopMusic(); // 🔇 Stop intro music
        SceneManager.LoadScene("FTUEscene");
    }
}
