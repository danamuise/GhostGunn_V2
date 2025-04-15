using UnityEngine;

public class GhostShooter : MonoBehaviour
{
    public BulletPool bulletPool; // Link to your BulletPool GameObject
    public Transform firePoint; // Where bullets spawn from
    public LaserTrajectoryPreview trajectoryPreview;

    private bool canShoot = true;

    void Update()
    {
        if (!canShoot) return;

        if (!AllBulletsAreInTank())
        {
            trajectoryPreview?.ClearDots();
            return;
        }

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
        GameObject bulletGO = bulletPool.GetBullet();
        bulletGO.transform.position = firePoint.position;
        bulletGO.SetActive(true);

        GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
        bullet.Fire(direction);
    }

    public void EnableGun(bool enable)
    {
        canShoot = enable;
    }

    public void DisableGun()
    {
        canShoot = false;
    }

    private bool AllBulletsAreInTank()
    {
        foreach (GameObject bulletGO in bulletPool.GetAllBullets())
        {
            if (!bulletGO.activeInHierarchy) continue;

            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
            if (bullet == null)
            {
                Debug.LogWarning($"Bullet GameObject '{bulletGO.name}' is missing GhostBullet component.");
                continue;
            }

            if (!bullet.IsInTank)
            {
                Debug.Log($"⛔ Bullet '{bulletGO.name}' is still active and not in tank.");
                return false;
            }
        }

        // All active bullets are in the tank
        return true;
    }


}
