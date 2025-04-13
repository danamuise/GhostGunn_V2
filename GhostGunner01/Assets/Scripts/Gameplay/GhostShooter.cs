using UnityEngine;

public class GhostShooter : MonoBehaviour
{
    public GhostBullet bulletPrefab;
    public Transform firePoint; // where bullets spawn from
    public int poolSize = 10;
    public LaserTrajectoryPreview trajectoryPreview; // Updated reference

    private GhostBullet[] pool;
    private int currentIndex = 0;
    private bool canShoot = true;

    void Start()
    {
        // Simple object pool setup
        pool = new GhostBullet[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GhostBullet b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            b.gameObject.SetActive(false);
            pool[i] = b;
        }
    }

    void Update()
    {
        if (!canShoot) return;

#if UNITY_EDITOR
        Vector3 inputPos = Input.mousePosition;
        bool inputDown = Input.GetMouseButton(0);
        bool inputUp = Input.GetMouseButtonUp(0);
#else
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        Vector3 inputPos = touch.position;
        bool inputDown = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        bool inputUp = touch.phase == TouchPhase.Ended;
#endif

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(inputPos);
        worldPoint.z = 0f;

        Vector2 direction = (worldPoint - firePoint.position).normalized;

        if (inputDown)
        {
            if (trajectoryPreview != null)
                trajectoryPreview.DrawLaserLine(firePoint.position, direction);
        }

        if (inputUp)
        {
            if (trajectoryPreview != null)
                trajectoryPreview.ClearDots();

            Debug.DrawLine(firePoint.position, worldPoint, Color.red, 2f);
            Debug.Log("Direction: " + direction);

            FireBullet(direction);
        }
    }

    void FireBullet(Vector2 direction)
    {
        GhostBullet bullet = pool[currentIndex];
        bullet.transform.position = firePoint.position;
        bullet.gameObject.SetActive(true);
        bullet.Fire(direction);

        currentIndex = (currentIndex + 1) % pool.Length;
    }

    public void EnableGun(bool enable)
    {
        canShoot = enable;
    }

    public void DisableGun()
    {
        canShoot = false;
    }
}
