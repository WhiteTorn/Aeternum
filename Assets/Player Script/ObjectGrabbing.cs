using UnityEngine;

public class ObjectGrabbing : MonoBehaviour
{
    [Header("Setup Fields")]
    [SerializeField] private Transform objectHoldPoint;
    [SerializeField] private Camera playerCamera;
    private CharacterController playerController;

    [Header("Grabbing Settings")]
    [SerializeField] private float pickupRange = 5f;
    [Tooltip("How tightly the object follows the hold point. Higher values are more rigid.")]
    [SerializeField] private float followStiffness = 100f;

    // --- NEW SAFETY CHECK ---
    [Header("Safety Checks")]
    [Tooltip("How far below the player to check for the ground object.")]
    [SerializeField] private float groundCheckDistance = 1.1f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomDistance = 1.5f;
    [SerializeField] private float maxZoomDistance = 5f;
    private Vector3 initialHoldPointPosition;

    private Rigidbody grabbedObjectRb;
    private Collider grabbedObjectCollider;
    private float originalLinearDamping;
    private float originalAngularDamping;

    void Awake()
    {
        playerController = GetComponent<CharacterController>();
        if (objectHoldPoint != null)
        {
            initialHoldPointPosition = objectHoldPoint.localPosition;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (grabbedObjectRb == null)
            {
                TryPickupObject();
            }
            else
            {
                DropObject();
            }
        }
        
        if (grabbedObjectRb != null)
        {
            HandleZoom();
        }
    }
    
    void FixedUpdate()
    {
        if (grabbedObjectRb != null)
        {
            Vector3 positionDifference = objectHoldPoint.position - grabbedObjectRb.position;
            grabbedObjectRb.linearVelocity = positionDifference * followStiffness * Time.fixedDeltaTime;

            Quaternion rotationDifference = objectHoldPoint.rotation * Quaternion.Inverse(grabbedObjectRb.rotation);
            rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
            Vector3 angularVelocity = (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            grabbedObjectRb.angularVelocity = angularVelocity;
        }
    }

    void HandleZoom()
    {
        Vector3 currentPosition = objectHoldPoint.localPosition;
        if (Input.GetKey(KeyCode.Z)) currentPosition.z += zoomSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.X)) currentPosition.z -= zoomSpeed * Time.deltaTime;
        currentPosition.z = Mathf.Clamp(currentPosition.z, minZoomDistance, maxZoomDistance);
        objectHoldPoint.localPosition = currentPosition;
    }
    
    void TryPickupObject()
    {
        // 1. First Raycast: Check what the player is LOOKING AT.
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.CompareTag("Grabbable"))
            {
                // --- THIS IS THE NEW SAFETY CHECK LOGIC ---
                // 2. Second Raycast: Check what the player is STANDING ON.
                RaycastHit groundHit;
                if (Physics.Raycast(transform.position, Vector3.down, out groundHit, groundCheckDistance))
                {
                    // 3. Compare the object we're looking at with the object we're standing on.
                    if (hit.collider == groundHit.collider)
                    {
                        // If they are the same, do not pick it up!
                        Debug.Log("Cannot pick up an object you are standing on!");
                        return; // Exit the function immediately.
                    }
                }
                
                // --- If the check passes, proceed with the normal pickup logic ---
                grabbedObjectRb = hit.collider.GetComponent<Rigidbody>();
                if (grabbedObjectRb != null)
                {
                    grabbedObjectCollider = hit.collider;
                    
                    originalLinearDamping = grabbedObjectRb.linearDamping;
                    originalAngularDamping = grabbedObjectRb.angularDamping;
                    grabbedObjectRb.linearDamping = 10f;
                    grabbedObjectRb.angularDamping = 5f;

                    grabbedObjectRb.useGravity = false;
                    Physics.IgnoreCollision(playerController, grabbedObjectCollider, true);
                }
            }
        }
    }

    void DropObject()
    {
        if (grabbedObjectRb == null) return;

        grabbedObjectRb.useGravity = true;
        grabbedObjectRb.linearDamping = originalLinearDamping;
        grabbedObjectRb.angularDamping = originalAngularDamping;
        grabbedObjectRb.linearVelocity = Vector3.zero;

        Physics.IgnoreCollision(playerController, grabbedObjectCollider, false);
        objectHoldPoint.localPosition = initialHoldPointPosition;
        grabbedObjectRb = null;
        grabbedObjectCollider = null;
    }
}