using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    [Header("Pixelation Settings")]
    public SpriteRenderer splashImage; // Assign in Inspector
    private Material splashMaterial;

    private void Start()
    {
        // 🔊 Play intro music
        SFXManager.Instance.PlayMusic("IntroMusic", 0.3f);

        // 🌟 Animate pixelation
        if (splashImage != null)
        {
            splashMaterial = splashImage.material;
            StartCoroutine(AnimatePixelDensity(0f, -67.4f, 1.75f));
        }
    }

    private System.Collections.IEnumerator AnimatePixelDensity(float startValue, float endValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float current = Mathf.Lerp(startValue, endValue, t);
            splashMaterial.SetFloat("_PixelatePixelDensity", current);
            elapsed += Time.deltaTime;
            yield return null;
        }

        splashMaterial.SetFloat("_PixelatePixelDensity", endValue); // Ensure final value is set
    }

    // Called by Start Button
    public void StartGame()
    {
        SFXManager.Instance.StopMusic();
        SceneManager.LoadScene("FTUEscene");
    }
}
