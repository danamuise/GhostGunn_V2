using UnityEngine;

public class AddBulletPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bullet")) return;
        else
        {
            Debug.Log($"🎯 AddBulletPU hit by: {other.name} at {Time.time:F2}");

            BulletPool bulletPool = FindObjectOfType<BulletPool>();
            bulletPool.EnableNextBullet();
            Destroy(gameObject);
        }
            


        
    }
}
