using UnityEngine;

public class NukeTarget : MonoBehaviour
{
    private bool isArmed = false;
    public GameObject nukeSequenceObject;


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

            if (nukeSequenceObject != null)
            {
                nukeSequenceObject.SetActive(true);
                Debug.Log("💥 NukeSequence GameObject enabled!");
            }
            else
            {
                Debug.LogWarning("⚠️ NukeSequence GameObject reference is missing!");
            }

            Destroy(gameObject); 


        }
    }
}
