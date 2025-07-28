using UnityEngine;
using System.Collections;

public class SpriteBlinker : MonoBehaviour
{
    public float blinkInterval = 0.5f;

    private SpriteRenderer sr;
    private Coroutine blinkRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (sr != null)
        {
            blinkRoutine = StartCoroutine(Blink());
        }
    }

    void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            sr.enabled = false; // Make sure it's off when disabled
        }
    }

    IEnumerator Blink()
    {
        while (true)
        {
            sr.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
            sr.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
