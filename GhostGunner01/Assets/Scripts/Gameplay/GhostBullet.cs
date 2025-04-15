using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float laserSpeed = 10f;
    public float maxLaserDistance = 100f;
    public LayerMask laserCollisionMask;
    public float gravityStrength = 9.81f;

    [Header("Return to Tank Settings")]
    public float wallDropSpeed = 25f;
    public float wallDropDuration = 0.4f;

    [Header("Tank Floating Settings")]
    public float floatMagnitude = 0.2f;
    public float floatSpeed = 2f;
    public float jitterAmount = 0.05f;
    public float verticalOffset = 0f;

    private Rigidbody2D rb;
    private Vector2 laserDirection;

    private bool isInLaserMode = false;
    private bool inGhostMode = false;
    private bool isSlidingToWall = false;
    private bool isDroppingDown = false;
    private bool isInTank = false;

    private float remainingDistance;
    private float dropStartTime = -1f;
    private float lifetime;

    private Vector3 tankBasePosition;
    private float tankTimeOffset;
    private Transform tankTransform;

    private Vector2 wallSlideDirection;
    private bool isFired = false;

    public bool IsInTank => isInTank;

    public void Fire(Vector2 direction)
    {
        ExitTank();

        isInLaserMode = true;
        inGhostMode = false;
        isSlidingToWall = false;
        isDroppingDown = false;
        dropStartTime = -1f;
        isFired = true;

        laserDirection = direction.normalized;
        remainingDistance = maxLaserDistance;
        lifetime = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        if (isInLaserMode)
        {
            float stepDistance = laserSpeed * Time.fixedDeltaTime;
            Vector2 currentPosition = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, laserDirection, stepDistance, laserCollisionMask);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Target"))
                {
                    EnterGhostMode();
                    return;
                }

                if (hit.collider.CompareTag("Endzone"))
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 0f;
                    rb.linearVelocity = Vector2.zero;

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
            rb.linearVelocity += Vector2.up * gravityStrength * Time.fixedDeltaTime;
        }
        else if (isSlidingToWall)
        {
            rb.linearVelocity = new Vector2(wallSlideDirection.x * laserSpeed, 0f);
        }
        else if (isDroppingDown)
        {
            rb.linearVelocity = Vector2.down * wallDropSpeed;

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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isSlidingToWall &&
            (collision.gameObject.CompareTag("Wall_Left") || collision.gameObject.CompareTag("Wall_Right")))
        {
            isSlidingToWall = false;
            isDroppingDown = true;
            dropStartTime = -1f;
        }

        if (inGhostMode && collision.gameObject.CompareTag("Endzone"))
        {
            Debug.Log(name + " | ?? Hit Endzone during Ghost Mode � start return path");

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            inGhostMode = false;
            isInLaserMode = false;

            float distToLeft = Mathf.Abs(transform.position.x - GetWallX("Wall_Left"));
            float distToRight = Mathf.Abs(transform.position.x - GetWallX("Wall_Right"));
            wallSlideDirection = distToLeft < distToRight ? Vector2.left : Vector2.right;

            isSlidingToWall = true;
        }
    }

    private void EnterGhostMode()
    {
        isInLaserMode = false;
        inGhostMode = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = laserDirection * laserSpeed * 0.75f;
    }

    public void EnterTank()
    {
        isFired = false;
        isInTank = true;
        inGhostMode = false;
        isDroppingDown = false;
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
            Debug.LogWarning(name + " | StorageTank not found in scene.");
        }
    }

    private void ExitTank()
    {
        isInTank = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }

    private float GetWallX(string tag)
    {
        GameObject wall = GameObject.FindGameObjectWithTag(tag);
        return wall != null ? wall.transform.position.x : 0f;
    }
}
