using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enforcer is a script that applies forces to a Rigidbody based on user input or trigger events.
/// </summary>

public class Enforcer : MonoBehaviour
{
    public enum ForceType { Directional, Torque }

    [System.Serializable]
    public class Force
    {
        public ForceType forceType;
        // Type of the force: Directional for linear forces, Torque for rotational forces
        public float strength = 10f; // Strength of the force
        public KeyCode inputKey; // Key to trigger the force
        public GameObject activationTrigger; // Object that triggers the force
        public bool continuous;
        // continuous force application while the input key is held down or the object is within the trigger
        // not continuous applies the force only in the frame the input key is pressed or the object enters the trigger
        public bool mustBeGrounded = false; // Whether the object must be grounded to apply the force
        public float maxSpeed = 10f; // Maximum speed at which the force can be applied
        public Vector3 direction; // Direction of the force
        public bool relativeToObject = true; // Whether the direction is relative to the object's rotation
        public Transform origin; // Point of origin for the force, if null, the object's position is used

        [HideInInspector]
        public bool isActive; // To track if the force is currently being applied
    }

    public Force[] forces; // Number of forces to apply
    public float visualizerLength = 1.0f; // Length of the visualizer lines

    private Rigidbody rb;
    private Collider col;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        // Check if the object is grounded
        isGrounded = CheckIfGrounded();

        foreach (Force force in forces)
        {
            force.isActive = false; // Reset the isActive flag

            if (force.inputKey != KeyCode.None)
            {
                if (force.continuous && Input.GetKey(force.inputKey) && (!force.mustBeGrounded || isGrounded) && rb.velocity.magnitude < force.maxSpeed)
                {
                    ApplyForce(force, ForceMode.Force);
                    force.isActive = true;
                }
                else if (!force.continuous && Input.GetKeyDown(force.inputKey) && (!force.mustBeGrounded || isGrounded) && rb.velocity.magnitude < force.maxSpeed)
                {
                    ApplyForce(force, ForceMode.Impulse);
                    force.isActive = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        foreach (Force force in forces)
        {
            if (other.gameObject == force.activationTrigger && !force.continuous && (!force.mustBeGrounded || isGrounded) && rb.velocity.magnitude < force.maxSpeed)
            {
                ApplyForce(force, ForceMode.Impulse);
                force.isActive = true;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        foreach (Force force in forces)
        {
            if (other.gameObject == force.activationTrigger && force.continuous && (!force.mustBeGrounded || isGrounded) && rb.velocity.magnitude < force.maxSpeed)
            {
                ApplyForce(force, ForceMode.Force);
                force.isActive = true;
            }
        }
    }

    void ApplyForce(Force force, ForceMode forceMode)
    {
        Vector3 forceDirection = force.relativeToObject ? transform.TransformDirection(force.direction.normalized) : force.direction.normalized;
        Vector3 forceOrigin = force.origin != null ? force.origin.position : transform.position;

        switch (force.forceType)
        {
            case ForceType.Directional:
                rb.AddForceAtPosition(forceDirection * force.strength, forceOrigin, forceMode);
                break;
            case ForceType.Torque:
                rb.AddTorque(forceDirection * force.strength, forceMode);
                break;
        }
    }

    bool CheckIfGrounded()
    {
        if (col == null) return false;

        float rayLength = col.bounds.extents.y + 0.1f; // Add a small offset to ensure the raycast starts slightly above the bottom of the collider
        return Physics.Raycast(transform.position, Vector3.down, rayLength);
    }

    void OnDrawGizmos()
    {
        if (forces == null) return;

        foreach (Force force in forces)
        {
            Vector3 forceDirection = force.relativeToObject ? transform.TransformDirection(force.direction.normalized) : force.direction.normalized;
            Vector3 forceOrigin = force.origin != null ? force.origin.position : transform.position;
            Gizmos.color = force.isActive ? Color.green : Color.red;

            if (force.forceType == ForceType.Directional)
            {
                Vector3 startPoint = forceOrigin;
                Vector3 endPoint = forceOrigin + forceDirection * force.strength * visualizerLength * 0.2f;
                Gizmos.DrawLine(startPoint, endPoint);

                // Draw arrow wings at the end of the line
                Vector3 wingDirection = (endPoint - startPoint).normalized;
                Vector3 crossAxis = Vector3.Cross(wingDirection, Vector3.up);
                if (crossAxis == Vector3.zero) // Handle the case where the force is directly up or down
                {
                    crossAxis = Vector3.Cross(wingDirection, Vector3.forward);
                }
                Vector3 wingLeft = Quaternion.AngleAxis(90, wingDirection) * Quaternion.AngleAxis(135, crossAxis) * wingDirection * 0.2f;
                Vector3 wingRight = Quaternion.AngleAxis(90, wingDirection) * Quaternion.AngleAxis(-135, crossAxis) * wingDirection * 0.2f;
                Gizmos.DrawLine(endPoint, endPoint + wingLeft);
                Gizmos.DrawLine(endPoint, endPoint + wingRight);
            }
            else if (force.forceType == ForceType.Torque)
            {
                DrawTorqueGizmo(forceDirection, force.strength * visualizerLength * 0.2f);
            }
        }
    }

    void DrawTorqueGizmo(Vector3 direction, float radius)
    {
        int segments = 20;
        float angleStep = 160f / segments; // Draw only 270 degrees
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * radius;
        if (perpendicular == Vector3.zero)
        {
            perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * radius;
        }

        Vector3 lastPoint = transform.position + Quaternion.AngleAxis(100, direction) * perpendicular;

        for (int i = 1; i <= segments; i++) // Use <= to include the last segment
        {
            float angle = 100 + i * angleStep;
            Vector3 nextPoint = transform.position + Quaternion.AngleAxis(angle, direction) * perpendicular;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        // Draw arrow wings at the end of the circle
        Vector3 wingDirection = (lastPoint - transform.position).normalized;
        Vector3 wingLeft = Quaternion.AngleAxis(-45, direction) * wingDirection * 0.2f;
        Vector3 wingRight = Quaternion.AngleAxis(-135, direction) * wingDirection * 0.2f;
        Gizmos.DrawLine(lastPoint, lastPoint + wingLeft);
        Gizmos.DrawLine(lastPoint, lastPoint + wingRight);
    }
}