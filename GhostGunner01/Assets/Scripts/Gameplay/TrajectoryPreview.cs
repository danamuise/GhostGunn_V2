using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    public int maxSteps = 30;
    public float timeStep = 0.05f;
    public float radius = 0.1f;
    public LayerMask wallMask;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    public void ShowTrajectory(Vector2 startPosition, Vector2 velocity)
    {
        Vector2 currentPosition = startPosition;
        Vector2 currentVelocity = velocity;

        lineRenderer.positionCount = maxSteps;
        lineRenderer.SetPosition(0, currentPosition);

        for (int i = 1; i < maxSteps; i++)
        {
            Vector2 nextPosition = currentPosition + currentVelocity * timeStep;

            RaycastHit2D hit = Physics2D.CircleCast(currentPosition, radius, currentVelocity.normalized, (nextPosition - currentPosition).magnitude, wallMask);
            if (hit.collider != null)
            {
                currentPosition = hit.point + hit.normal * 0.01f; // nudge off surface
                currentVelocity = Vector2.Reflect(currentVelocity, hit.normal);
            }
            else
            {
                currentPosition = nextPosition;
                currentVelocity.y += Physics2D.gravity.y * timeStep; // simulate gravity
            }

            lineRenderer.SetPosition(i, currentPosition);
        }
    }

    public void Hide()
    {
        lineRenderer.positionCount = 0;
    }
}
