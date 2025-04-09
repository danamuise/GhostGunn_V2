using UnityEngine;
using System.Collections;

public class BouncingBullet : MonoBehaviour
{
    public float speed = 10f;
    public float maxLifetime = 5f;
    public int maxBounces = 5;
    public LayerMask wallMask;
    [HideInInspector]
    public GhostShooter shooter;

    private Vector2 direction;
    private Vector2 startFirePosition;
    private float lifetime;
    private int bounceCount;
    private bool isActive;

    public void Fire(Vector2 startPos, Vector2 dir)
    {
        //Clear trail so it doesn't leave ghost artifacts
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();
        transform.position = startPos;
        startFirePosition = startPos;
        direction = dir.normalized;
        lifetime = 0f;
        bounceCount = 0;
        isActive = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive) return;

        float moveDistance = speed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveDistance, wallMask);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ceiling"))
            {
                StartCoroutine(ReturnHomeRoutine());
                isActive = false;
                return;
            }

            if (bounceCount < maxBounces)
            {
                transform.position = hit.point + hit.normal * 0.01f;
                direction = Vector2.Reflect(direction, hit.normal);
                bounceCount++;
            }
            else
            {
                transform.position += (Vector3)(direction * moveDistance);
            }
        }
        else
        {
            transform.position += (Vector3)(direction * moveDistance);
        }

        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime)
        {
            Deactivate("Lifetime exceeded");
        }
    }

    IEnumerator ReturnHomeRoutine()
    {
        Vector2 ceilingDir = transform.position.x < 0 ? Vector2.left : Vector2.right;

        // Step 1: Slide along ceiling until wall found
        while (true)
        {
            transform.position += (Vector3)(ceilingDir * speed * Time.deltaTime);

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, wallMask);
            if (wallHit.collider != null && wallHit.collider.CompareTag("Wall"))
                break;

            yield return null;
        }

        // Step 2: STRAIGHT drop until curve trigger point
        float curveStartY = startFirePosition.y + 1.5f;
        while (transform.position.y > curveStartY)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
            yield return null;
        }

        // Step 3: CURVED final return to firePoint
        Vector2 curveStart = transform.position;
        Vector2 curveEnd = startFirePosition;

        // Control point: halfway between start and end, slightly ABOVE to avoid bounce
        Vector2 curveControl = new Vector2(
            (curveStart.x + curveEnd.x) / 2f,
            Mathf.Min(curveStart.y, curveEnd.y) - 1.0f // slightly below the curve for downward dip
        );

        float t = 0f;
        while (t < 1f)
        {
            transform.position = BezierPoint(curveStart, curveControl, curveEnd, t);
            t += Time.deltaTime * 1.25f;
            yield return null;
        }

        transform.position = curveEnd;

        Deactivate("Returned to fire point");
    }


    Vector2 BezierPoint(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(ab, bc, t);
    }



    private void Deactivate(string reason)
    {
        Debug.Log("Bullet returned home: " + reason);
        isActive = false; // disables Update movement logic
        shooter?.NotifyBulletReady();
    }


    public void SetShooter(GhostShooter shooter)
    {
        this.shooter = shooter;
    }

    public void Stop()
    {
        isActive = false;
    }

    public void PrepareAtHome(Vector2 homePosition)
    {
        transform.position = homePosition;
        startFirePosition = homePosition;
        isActive = false;
        gameObject.SetActive(true);
    }


}
