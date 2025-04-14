using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostBullet : MonoBehaviour
{
    public float launchForce = 10f;
    public float maxLifetime = 5f;

    [Header("Wall Drop Settings")]
    public float wallDropSpeed = 25.0f;
    public float wallDropDuration = 10.0f;

    [Header("Tank Floating")]
    public float floatMagnitude = 0.2f;
    public float floatSpeed = 2f;
    public float jitterAmount = 0.05f;

    private bool isDroppingDown = false;
    private bool isSlidingToWall = false;
    private bool isInTank = false;

    private float dropTimeElapsed = 0f;
    private float dropStartTime = -1f;

    private Vector2 wallSlideDirection;
    private Rigidbody2D rb;
    private float lifetime;
    private bool isFired = false;
    private bool inGhostMode = false;
    public float gravityStrength = 9.81f;

    private Vector3 tankBasePosition;
    private float tankTimeOffset;
    private Transform tankTransform;

    private Collider2D bulletCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        bulletCollider = GetComponent<Collider2D>();

        // Debug: Assign unique name
        gameObject.name = "GhostBullet_" + Random.Range(1000, 9999);
    }

    public void Fire(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        inGhostMode = false;
        isFired = true;
        isSlidingToWall = false;
        isDroppingDown = false;
        isInTank = false;
        dropStartTime = -1f;

        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * launchForce * 7f;

        Debug.Log(gameObject.name + " | FIRING in laser mode");
    }

    void Update()
    {
        if (!isFired && !isInTank) return;

        if (inGhostMode)
        {
            rb.linearVelocity += new Vector2(0, -1f * Time.deltaTime * gravityStrength);
        }

        if (isSlidingToWall)
        {
            rb.linearVelocity = wallSlideDirection * launchForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        if (isDroppingDown)
        {
            rb.linearVelocity = Vector2.down * wallDropSpeed;

            if (dropStartTime < 0f)
            {
                dropStartTime = Time.time;
            }

            Debug.Log(gameObject.name + " | Timer: " + (Time.time - dropStartTime) + " duration: " + wallDropDuration);

            if (Time.time - dropStartTime >= wallDropDuration)
            {
                Debug.Log(gameObject.name + " | ⏱️ Drop duration complete — entering tank");
                isDroppingDown = false;
                EnterTank();
            }
        }

        if (isInTank)
        {
            if (tankTransform == null)
            {
                GameObject tankGO = GameObject.Find("StorageTank");
                if (tankGO != null) tankTransform = tankGO.transform;
            }

            float t = Time.time * floatSpeed + tankTimeOffset;
            float sineOffset = Mathf.Sin(t) * floatMagnitude;
            float jitterX = Mathf.PerlinNoise(t, tankTimeOffset) * jitterAmount;

            transform.position = tankBasePosition + new Vector3(jitterX, sineOffset, 0f);
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
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(gameObject.name + " | Collided with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Endzone"))
        {
            Debug.Log(gameObject.name + " | 💀 Bullet hit Endzone — moving to nearest wall");

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            inGhostMode = false;
            isFired = true;

            float distToLeft = Mathf.Abs(transform.position.x - GetWallX("Wall_Left"));
            float distToRight = Mathf.Abs(transform.position.x - GetWallX("Wall_Right"));

            wallSlideDirection = distToLeft < distToRight ? Vector2.left : Vector2.right;
            isSlidingToWall = true;
            return;
        }

        if (isSlidingToWall &&
            (collision.gameObject.CompareTag("Wall_Left") || collision.gameObject.CompareTag("Wall_Right")))
        {
            Debug.Log(gameObject.name + " | ⬇️ Reached wall — beginning wall drop");
            rb.linearVelocity = Vector2.down * wallDropSpeed;
            isSlidingToWall = false;
            isDroppingDown = true;
            dropStartTime = -1f;
            return;
        }

        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log(gameObject.name + " | Hit target — switching to ghost mode");
            inGhostMode = true;
            rb.gravityScale = -1f;
            return;
        }

        if (!inGhostMode)
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflected = Vector2.Reflect(rb.linearVelocity, normal);
            rb.linearVelocity = reflected;

            Debug.Log(gameObject.name + " | Reflected off: " + collision.gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(gameObject.name + " | Trigger hit: " + other.name);

        if (!inGhostMode)
        {
            Vector2 normal = (transform.position - other.transform.position).normalized;
            Vector2 reflected = Vector2.Reflect(rb.linearVelocity, normal);
            rb.linearVelocity = reflected;
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

    float GetWallX(string tag)
    {
        GameObject wall = GameObject.FindGameObjectWithTag(tag);
        return wall != null ? wall.transform.position.x : 0f;
    }

    void EnterTank()
    {
        isFired = false;
        isInTank = true;
        inGhostMode = false;
        dropStartTime = -1f;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (tankTransform == null)
        {
            GameObject tankGO = GameObject.Find("StorageTank");
            if (tankGO != null) tankTransform = tankGO.transform;
        }

        if (tankTransform != null)
        {
            Vector3 offset = Random.insideUnitCircle * 0.3f;
            tankBasePosition = tankTransform.position + offset;
            transform.position = tankBasePosition;
            tankTimeOffset = Random.Range(0f, 100f);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " | StorageTank not found in scene.");
        }
    }
}
