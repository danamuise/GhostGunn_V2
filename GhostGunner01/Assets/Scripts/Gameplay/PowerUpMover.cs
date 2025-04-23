using UnityEngine;
using System.Collections;

public class PowerUpMover : MonoBehaviour
{
    public void AnimateToPosition(Vector2 targetPosition, float duration = 0.5f, bool fromEndzone = false)
    {
        Vector2 startPosition = fromEndzone
            ? new Vector2(targetPosition.x, 5.35f)
            : (Vector2)transform.position;

        StopAllCoroutines();
        StartCoroutine(SlideToPosition(startPosition, targetPosition, duration));
    }

    private IEnumerator SlideToPosition(Vector2 startPos, Vector2 endPos, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }
}
