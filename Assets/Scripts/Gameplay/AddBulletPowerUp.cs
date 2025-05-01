using UnityEngine;

public class AddBulletPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Activate();
        }
    }

    public void Activate()
    {
        BulletPool bulletPool = FindObjectOfType<BulletPool>();
        if (bulletPool != null)
        {
            GameObject bullet = bulletPool.GetNextAvailableBullet();
            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.SetActive(true);

                GhostBullet ghost = bullet.GetComponent<GhostBullet>();
                if (ghost != null)
                {
                    PowerUpManager powerUpManager = FindObjectOfType<PowerUpManager>();
                    if (powerUpManager != null)
                    {
                        powerUpManager.PlayPickupEffects(transform.position);
                    }
                    ghost.EnterGhostMode(); // 🔮 Skip tank — go directly to GhostMode
                }

                Debug.Log("🧲 AddBulletPowerUp Activated: Spawned ghost bullet!");
            }
            else
            {
                Debug.LogWarning("⚠️ AddBulletPU: No bullet available to activate.");
            }
        }

        Destroy(gameObject); // Remove PowerUp after activation
    }
}
