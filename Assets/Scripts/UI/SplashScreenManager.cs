using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    // This method can be called by the StartButton's OnClick() event
    public void StartGame()
    {
        SceneManager.LoadScene("FTUEscene"); 
    }
}
