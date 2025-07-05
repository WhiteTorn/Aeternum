using System.Collections.Generic;
using UnityEngine;

// Add this line to ensure a Rigidbody component exists
[RequireComponent(typeof(Rigidbody))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Interaction")]
    [SerializeField] private Camera playerCamera; // Assign your main player camera
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    private Rigidbody rb;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        // Get the rigidbody on this.
        rb = GetComponent<Rigidbody>();
    }

    // --- NEW METHOD FOR INPUT ---
    // Check for non-physics inputs here
    void Update()
    {
        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Check for the interaction key press
        if (Input.GetKeyDown(interactionKey))
        {
            Debug.Log("Attempting Interaction");
            AttemptInteraction();
        }
    }

    // --- METHOD FOR PHYSICS ---
    // Only handle Rigidbody physics here
    void FixedUpdate()
    {
        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity = new Vector2(Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        // Apply movement.
        // Note: I changed 'rigidbody' to 'rb' to match the variable name from Awake()
        rb.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);
    }

    // Inside your FirstPersonMovement.cs script

    private void AttemptInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // We only look for the DoubleDoor component now.
            DoorScript door = hit.collider.GetComponent<DoorScript>();

            if (door != null)
            {
                // If we find it, tell it to interact.
                door.Interact();
            }
        }
    }
}