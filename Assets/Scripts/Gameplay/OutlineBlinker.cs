using UnityEngine;
using System.Collections;

public class OutlineBlinker : MonoBehaviour
{
    private Material mat;
    public float blinkInterval = 0.5f;
    public float outlineOnWidth = 0.008f;
    public float outlineOffWidth = 0f;

    private Coroutine blinkRoutine;

    void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            mat = sr.material;
        }
    }

    void OnEnable()
    {
        if (mat != null)
            blinkRoutine = StartCoroutine(BlinkOutline());
    }

    void OnDisable()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);
    }

    IEnumerator BlinkOutline()
    {
        while (true)
        {
            mat.SetFloat("_OuterOutlineWidth", outlineOnWidth);
            yield return new WaitForSeconds(blinkInterval);
            mat.SetFloat("_OuterOutlineWidth", outlineOffWidth);
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
