using UnityEngine;

public class WiggleDot : MonoBehaviour
{
    // Wiggle amplitude (side-to-side distance) and speed (oscillation frequency)
    [SerializeField] private float wiggleIntensity = 0.5f;
    [SerializeField] private float wiggleSpeed = 5f;

    // (Optional) Unique index for this dot in the trajectory sequence (assigned by spawner)
    public int dotIndex = 0;

    // Store the initial position to maintain original y and z (and baseline x)
    private Vector3 initialPosition;

    void Start()
    {
        // Record the initial local position of the dot
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Compute the sine wave phase for this dot, offset by its index to create a ripple effect
        float phase = Time.time * wiggleSpeed + dotIndex * 0.5f;

        // Calculate horizontal offset using sine wave
        float xOffset = Mathf.Sin(phase) * wiggleIntensity;

        // Apply the horizontal offset to the original position (preserve y and z)
        Vector3 newPosition = initialPosition;
        newPosition.x += xOffset;
        transform.localPosition = newPosition;
    }
}
