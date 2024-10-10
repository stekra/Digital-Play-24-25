using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Wind : MonoBehaviour
{
    [Tooltip("The force of the wind (maximum force when using random variation)")]
    public float windForce = 10;

    [Header("Random Variation")]
    [Tooltip("Adds random variation to the wind force")]
    public bool useRandomVariation = false;
    [Tooltip("The frequency of change")]
    public float frequency = 1;
    [Tooltip("The minimum force of the wind when using random variation. The maximum force is windForce")]
    public float minForce = 0;

    [Space]
    [Tooltip("Show gizmo visualization in the editor")]
    public bool showGizmos = true;

    float noiseValue;
    float noiseWindForce;

    // Sets the collider to be a trigger, by default
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Update the noise, if using random variation
    void Update()
    {
        if (useRandomVariation)
        {
            noiseValue = Mathf.PerlinNoise1D(Time.time * frequency);
        }
    }

    // Apply wind force to rigidbodies within the trigger
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            if (useRandomVariation)
            {
                noiseWindForce = Mathf.Lerp(minForce, windForce, noiseValue);
                rb.AddForce(transform.forward * noiseWindForce);
            }
            else
            {
                rb.AddForce(transform.forward * windForce);
            }
        }
    }

    // This is only for the gizmo visualization
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Set color and coordinates
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

        // Draw Box
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        // Draw Arrow
        float len = 0.8f;
        if (useRandomVariation && Application.isPlaying)
        {
            len = Mathf.Lerp(minForce / windForce, len, noiseValue);
        }
        Vector3 pos = new(0f, 0f, -len / 2f);
        Vector3 dir = Vector3.forward;
        Gizmos.DrawRay(pos, dir * len);

        Vector3 right = Quaternion.Euler(0, 180 + 45, 0) * Vector3.forward * len * 0.5f;
        Vector3 left = Quaternion.Euler(0, 180 - 45, 0) * Vector3.forward * len * 0.5f;
        Gizmos.DrawRay(pos + dir * len, right);
        Gizmos.DrawRay(pos + dir * len, left);

        // Reset coordinates
        Gizmos.matrix = Matrix4x4.identity;
    }
}
