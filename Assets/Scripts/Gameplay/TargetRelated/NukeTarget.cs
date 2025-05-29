using UnityEngine;

public class NukeTarget : MonoBehaviour
{
    private bool isArmed = false;

    public void ArmNuke()
    {
        isArmed = true;
        Debug.Log("🧨 NukeTarget armed and ready for collision.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isArmed) return;

        if (other.CompareTag("Bullet"))
        {
            Debug.Log("💥 Nuke Launched — NukeIcon hit by bullet!");
            Destroy(gameObject); // You can replace this with effects later
        }
    }
}
