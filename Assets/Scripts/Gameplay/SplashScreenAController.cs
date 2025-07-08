using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SplashScreenAController : MonoBehaviour
{
    [Header("TextMeshPro References")]
    public TMP_Text text1;
    public TMP_Text text2;
    public TMP_Text text3;
    public TMP_Text text4;

    [Header("GameObject References")]
    public GameObject ghostParent;
    public GameObject wordBalloon;

    void Start()
    {
        // 🔊 Play intro music
        SFXManager.Instance.PlayMusic("IntroMusic", 0.3f);
        // Initial setup
        text1.gameObject.SetActive(true);
        text2.gameObject.SetActive(true);

        // Begin coroutine sequence
        StartCoroutine(PlaySplashSequence());
    }

    IEnumerator PlaySplashSequence()
    {
        yield return new WaitForSeconds(1f);
        ghostParent.SetActive(true);

        yield return new WaitForSeconds(1f);
        wordBalloon.SetActive(true);
        text3.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);
        text3.gameObject.SetActive(false);
        text4.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("SplashScreenB");
    }
}
