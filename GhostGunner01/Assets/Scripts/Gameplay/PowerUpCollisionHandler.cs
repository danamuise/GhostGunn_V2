using UnityEngine;

public class PowerUpCollisionHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            PowerUp powerUp = GetComponent<PowerUp>();
            if (powerUp != null)
            {
                powerUp.Activate(other.gameObject);
                Destroy(gameObject);
            }
        }
    }
}
