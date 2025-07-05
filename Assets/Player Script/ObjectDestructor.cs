using System.Collections;
using System.Collections.Generic; // Required for Lists
using UnityEngine;

public class EarthUnbender : MonoBehaviour
{
    [Header("Setup Fields")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ParticleSystem sinkVFX; // Optional visual effect for sinking

    [Header("Settings")]
    [SerializeField] private float unbendRange = 20f;
    [SerializeField] private float sinkDuration = 0.5f;

    // A list to track objects that are currently sinking, to prevent unbending the same object twice.
    private List<GameObject> sinkingObjects = new List<GameObject>();

    void Update()
    {
        // Check for right mouse button click
        if (Input.GetMouseButtonDown(1))
        {
            AttemptUnbend();
        }
    }

    private void AttemptUnbend()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, unbendRange))
        {
            // Check if the object is grabbable AND not already in the process of sinking
            if (hit.collider.CompareTag("Grabbable") && !sinkingObjects.Contains(hit.collider.gameObject))
            {
                // Start the sinking animation coroutine
                StartCoroutine(UnbendRockIntoGround(hit.collider.gameObject));
            }
        }
    }

    private IEnumerator UnbendRockIntoGround(GameObject objectToUnbend)
    {
        // Add the object to our list to prevent re-triggering
        sinkingObjects.Add(objectToUnbend);

        // Get the object's components
        Rigidbody rockRb = objectToUnbend.GetComponent<Rigidbody>();
        Collider rockCollider = objectToUnbend.GetComponent<Collider>();

        // Prepare the object for animation: take control from physics
        rockRb.isKinematic = true;
        rockCollider.enabled = false; // Disable collider to prevent snagging

        // Define start and end positions for our animation
        Vector3 startPosition = objectToUnbend.transform.position;
        // The end position is slightly below the object's base
        Vector3 endPosition = startPosition - new Vector3(0, objectToUnbend.transform.localScale.y, 0);

        // Play the visual effect
        if (sinkVFX != null)
        {
            Instantiate(sinkVFX, startPosition, Quaternion.identity);
        }

        // Animate the rock sinking over 'sinkDuration'
        float elapsedTime = 0f;
        while (elapsedTime < sinkDuration)
        {
            // Move the object from start to end position smoothly
            objectToUnbend.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / sinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // --- Cleanup ---
        // Remove the object from our tracking list
        sinkingObjects.Remove(objectToUnbend);
        // Finally, destroy the game object for real
        Destroy(objectToUnbend);
    }
}