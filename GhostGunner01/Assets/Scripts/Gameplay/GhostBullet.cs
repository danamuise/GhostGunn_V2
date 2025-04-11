using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostBullet : MonoBehaviour
{
    public float launchForce = 5f;
    public float maxLifetime = 5f;

    private Rigidbody2D rb;
    private float lifetime;
    private bool isFired = false;
    private bool inGhostMode = false;
    public float gravityStrength = 9.81f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // idle mode: no gravity
        rb.linearVelocity = Vector2.zero;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Fire(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        inGhostMode = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * launchForce *7f;

        Debug.Log("FIRING in laser mode");
    }


    void Update()
    {
        if (!isFired) return;

        if (inGhostMode)
        {
            // Add inverted gravity manually
            rb.linearVelocity += new Vector2(0, -1f * Time.deltaTime * gravityStrength);
        }

        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            ResetBullet();
        }
    }


    private void ResetBullet()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        isFired = false;

        gameObject.SetActive(false); // object pool will handle reuse
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Ceiling"))
        {
            Debug.Log("Hit ceiling � deactivating bullet");
            ResetBullet();
            return;
        }

        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Hit target � switching to ghost mode");

            inGhostMode = true;
            rb.gravityScale = -1f;
            return;
        }

        // Reflect off other surfaces in laser mode
        if (!inGhostMode)
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflected = Vector2.Reflect(rb.linearVelocity, normal);
            rb.linearVelocity = reflected;

            Debug.Log("Reflected off: " + collision.gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger hit: " + other.name);

        if (!inGhostMode)
        {
            Vector2 normal = (transform.position - other.transform.position).normalized;
            Vector2 reflected = Vector2.Reflect(rb.velocity, normal);
            rb.velocity = reflected;
        }

        if (other.CompareTag("Target"))
        {
            inGhostMode = true;
            rb.gravityScale = -1f;
        }

        if (other.CompareTag("Ceiling"))
        {
            ResetBullet();
        }
    }

}
