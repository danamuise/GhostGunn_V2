using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float laserSpeed = 10f;
    public float maxLaserDistance = 100f;
    public LayerMask laserCollisionMask;
    public float gravityStrength = 9.81f;
    [Range(0.001f, 1.0f)] public float bulletSize = 1f;

    [Header("Return to Tank Settings")]
    public float wallDropSpeed = 25f;
    public float wallDropDuration = 0.4f;

    [Header("Tank Floating Settings")]
    public float floatMagnitude = 0.2f;
    public float floatSpeed = 2f;
    public float jitterAmount = 0.05f;
    public float verticalOffset = 0f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 laserDirection;

    private bool isInLaserMode = false;
    private bool inGhostMode = false;
    private bool isSlidingToWall = false;
    private bool isDroppingDown = false;
    private bool isInTank = false;

    private float remainingDistance;
    private float dropStartTime = -1f;
    //private float lifetime;

    private Vector3 tankBasePosition;
    private float tankTimeOffset;
    private Transform tankTransform;

    private Vector2 wallSlideDirection;
    //private bool isFired = false;
    private GameManager gameManager;
    private float bulletLifeTimer = 0f;
    private const float maxLifeTime = 8f;
    private bool justEnteredGhostMode = false;

    public bool IsInTank => isInTank;

    public void Fire(Vector2 direction)
    {
        ExitTank();

        isInLaserMode = true;
        inGhostMode = false;
        isSlidingToWall = false;
        isDroppingDown = false;
        dropStartTime = -1f;
        //isFired = true;

        laserDirection = direction.normalized;
        remainingDistance = maxLaserDistance;
        //lifetime = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        bulletLifeTimer = 0f;
    }

    private void Awake()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();
        transform.localScale = Vector3.one * bulletSize;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        justEnteredGhostMode = false;

        if (isInLaserMode)
        {
            float stepDistance = laserSpeed * Time.fixedDeltaTime;
            Vector2 currentPosition = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, laserDirection, stepDistance, laserCollisionMask);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Target"))
                {
                    TargetBehavior tb = hit.collider.GetComponentInParent<TargetBehavior>();
                    if (tb != null && !inGhostMode)
                    {
                        tb.TakeDamage(1);
                        Debug.Log($"{name} | Laser Mode hit: {hit.collider.name} — health reduced");
                    }

                    EnterGhostMode();
                    return;
                }

                if (hit.collider.CompareTag("Endzone"))
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;

                    inGhostMode = false;
                    isInLaserMode = false;

                    float distToLeft = Mathf.Abs(transform.position.x - GetWallX("Wall_Left"));
                    float distToRight = Mathf.Abs(transform.position.x - GetWallX("Wall_Right"));
                    wallSlideDirection = distToLeft < distToRight ? Vector2.left : Vector2.right;
                    isSlidingToWall = true;
                    return;
                }

                laserDirection = Vector2.Reflect(laserDirection, hit.normal);
                transform.position = hit.point + laserDirection * 0.01f;
                return;
            }

            transform.position += (Vector3)(laserDirection * stepDistance);
            remainingDistance -= stepDistance;

            if (remainingDistance <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
        else if (inGhostMode)
        {
            rb.velocity += Vector2.up * gravityStrength * Time.fixedDeltaTime;
        }
        else if (isSlidingToWall)
        {
            rb.velocity = new Vector2(wallSlideDirection.x * laserSpeed, 0f);

            // 🔍 Raycast to detect wall contact
            RaycastHit2D hit = Physics2D.Raycast(transform.position, wallSlideDirection, 0.1f, laserCollisionMask);
            if (hit.collider != null &&
                (hit.collider.CompareTag("Wall_Left") || hit.collider.CompareTag("Wall_Right")))
            {
                isSlidingToWall = false;
                isDroppingDown = true;
                dropStartTime = -1f;
            }
        }
        else if (isDroppingDown)
        {
            rb.velocity = Vector2.down * wallDropSpeed;

            if (dropStartTime < 0f)
                dropStartTime = Time.time;

            if (Time.time - dropStartTime >= wallDropDuration)
            {
                EnterTank();
            }
        }
        else if (isInTank)
        {
            if (tankTransform == null)
            {
                GameObject tankGO = GameObject.Find("StorageTank");
                if (tankGO != null) tankTransform = tankGO.transform;
            }

            float t = Time.time * floatSpeed + tankTimeOffset;
            float sineOffset = Mathf.Sin(t) * floatMagnitude;
            float jitterX = Mathf.PerlinNoise(t, tankTimeOffset) * jitterAmount;

            transform.position = tankBasePosition + new Vector3(jitterX, sineOffset + verticalOffset, 0f);
        }

        if (!isInTank)
        {
            bulletLifeTimer += Time.deltaTime;

            if (bulletLifeTimer > maxLifeTime)
            {
                Debug.LogWarning($"{name} exceeded max lifetime — returning to tank");
                EnterTank();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (inGhostMode && !justEnteredGhostMode && collision.gameObject.CompareTag("Target"))
        {
            TargetBehavior tb = collision.gameObject.GetComponentInParent<TargetBehavior>();
            if (tb != null && !isInLaserMode)
            {
                tb.TakeDamage(1);
                Debug.Log($"{name} | Ghost Mode hit: {collision.gameObject.name} — health reduced");
            }
        }

        if (inGhostMode && collision.gameObject.CompareTag("Endzone"))
        {
            Debug.Log(name + " | ?? Hit Endzone during Ghost Mode — start return path");

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;

            inGhostMode = false;
            isInLaserMode = false;

            float distToLeft = Mathf.Abs(transform.position.x - GetWallX("Wall_Left"));
            float distToRight = Mathf.Abs(transform.position.x - GetWallX("Wall_Right"));
            wallSlideDirection = distToLeft < distToRight ? Vector2.left : Vector2.right;

            isSlidingToWall = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CornerOverride"))
        {
            if (!isInTank && !isSlidingToWall && !isDroppingDown)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;

                isInLaserMode = false;
                inGhostMode = false;

                float distToLeft = Mathf.Abs(transform.position.x - GetWallX("Wall_Left"));
                float distToRight = Mathf.Abs(transform.position.x - GetWallX("Wall_Right"));
                wallSlideDirection = distToLeft < distToRight ? Vector2.left : Vector2.right;

                isSlidingToWall = false;
                isDroppingDown = true;
                Debug.Log($"⚠️ {name} collided with CornerOverride — initiating slide to wall: " + isSlidingToWall);
            }
        }
    }

    public void EnterGhostMode()
    {
        isInLaserMode = false;
        inGhostMode = true;
        justEnteredGhostMode = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.velocity = laserDirection * laserSpeed * 0.75f;
    }

    public void EnterTank()
    {
        Debug.Log($"📤 {name} → EnterTank() called at {Time.time:F2}");

        //isFired = false;
        isInTank = true;
        inGhostMode = false;
        isDroppingDown = false;
        dropStartTime = -1f;

        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
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
            Debug.LogWarning($"{name} | 🚨 StorageTank not found in scene at {Time.time:F2}");
        }

        if (gameManager != null)
        {
            BulletPool pool = Object.FindFirstObjectByType<BulletPool>();
            if (pool != null)
            {
                Debug.Log($"📤 {name} is checking AllBulletsReturned() → {pool.AllBulletsReturned()} at {Time.time:F2}");

                if (pool.AllBulletsReturned())
                {
                    Debug.Log($"⚠️ {name} is calling OnShotComplete() from GhostBullet.cs at {Time.time:F2} BEFORE firing.");
                    gameManager.OnShotComplete();
                }
            }
            else
            {
                Debug.LogWarning($"{name} | 🚨 BulletPool not found during EnterTank() at {Time.time:F2}");
            }
        }
        else
        {
            Debug.LogWarning($"{name} | 🚨 gameManager is null in EnterTank() at {Time.time:F2}");
        }

        bulletLifeTimer = 0f;
    }

    private void ExitTank()
    {
        isInTank = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
    }

    private float GetWallX(string tag)
    {
        GameObject wall = GameObject.FindGameObjectWithTag(tag);
        return wall != null ? wall.transform.position.x : 0f;
    }
}
