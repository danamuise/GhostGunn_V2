using UnityEngine;
using System.Collections;

public class BouncingBullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float maxLifetime = 5f;
    public int maxBounces = 5;
    public float invertedGravity = -9.81f;
    public LayerMask wallMask;

    [HideInInspector] public GhostShooter shooter;

    private Transform ghostTank;
    private Vector2 velocity;
    private float lifetime;
    private int bounceCount;
    private bool isActive;
    private bool hasBounced = false;
    private float floatOffset;
    private TrailRenderer trail;

    public void Fire(Vector2 startPos, Vector2 dir)
    {
        if (trail == null)
            trail = GetComponent<TrailRenderer>();

        trail.Clear();
        trail.enabled = true;

        transform.position = startPos;
        velocity = dir.normalized * speed;
        lifetime = 0f;
        bounceCount = 0;
        hasBounced = false;
        isActive = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive)
        {
            float bob = Mathf.Sin(Time.time * 4f + floatOffset) * 0.12f;
            transform.position += new Vector3(0f, bob, 0f) * Time.deltaTime;
            return;
        }

        if (hasBounced)
        {
            velocity.y += invertedGravity * Time.deltaTime;
        }

        Vector2 move = velocity * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity.normalized, move.magnitude, wallMask);

        if (hit.collider != null)
        {
            transform.position = hit.point + hit.normal * 0.01f;

            Vector2 incoming = velocity;
            Vector2 reflected = Vector2.Reflect(incoming, hit.normal);

            // Preserve vertical or horizontal velocity based on surface
            if (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.y))
            {
                // Hit wall — reflect X, preserve Y
                velocity = new Vector2(reflected.x, incoming.y);
            }
            else
            {
                // Hit floor or ceiling — reflect Y, preserve X
                velocity = new Vector2(incoming.x, reflected.y);
            }

            hasBounced = true;
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                Deactivate("Max bounces reached");
                return;
            }
        }
        else
        {
            transform.position += (Vector3)move;
        }

        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime)
        {
            Deactivate("Lifetime exceeded");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Target"))
        {
            TargetBehavior target = other.GetComponent<TargetBehavior>();
            if (target != null)
            {
                target.TakeDamage(1);
            }

            Vector2 normal = (transform.position - other.transform.position).normalized;
            velocity = Vector2.Reflect(velocity, normal);
            hasBounced = true;
        }
    }

    private void Deactivate(string reason)
    {
        if (trail != null)
            trail.enabled = false;

        isActive = false;

        if (ghostTank != null)
        {
            Vector2 returnOffset = new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
            transform.position = ghostTank.position + (Vector3)returnOffset;
        }

        floatOffset = Random.Range(0f, Mathf.PI * 2f);
        gameObject.SetActive(true); // idle float mode

        shooter?.NotifyBulletReady(this);
    }

    public void SetShooter(GhostShooter shooter)
    {
        this.shooter = shooter;
    }

    public void SetGhostTank(Transform tank)
    {
        ghostTank = tank;
    }

    public void PrepareAtHome(Vector2 homePosition)
    {
        if (trail == null)
            trail = GetComponent<TrailRenderer>();

        transform.position = homePosition;
        isActive = false;
        gameObject.SetActive(true);
        floatOffset = Random.Range(0f, Mathf.PI * 2f);

        if (trail != null)
            trail.enabled = false;
    }
}
