using UnityEngine;

public class AddBulletPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bullet")) return;

        Debug.Log($"🎯 AddBulletPU hit by: {other.name} at {Time.time:F2}");


        Destroy(gameObject);
    }
}
