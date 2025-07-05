using System.Collections;
using UnityEngine;

public class EarthbendingController : MonoBehaviour
{
    [Header("Setup Fields")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject rockPrefab; // The cube prefab we created
    [SerializeField] private LayerMask groundLayerMask; // To ensure we only bend from the ground

    [Header("Bending Settings")]
    [SerializeField] private KeyCode bendKey = KeyCode.E; // The key to perform the bend
    [SerializeField] private float bendRange = 15f; // How far the player can bend from
    [SerializeField] private float pullHeight = 1f; // How high the rock rises from the ground
    [SerializeField] private float pullDuration = 0.5f; // How long the rising animation takes

    [Header("Effects")]
    [SerializeField] private ParticleSystem spawnVFX; // The particle effect for spawning

    void Update()
    {
        // Check if the bend key is pressed down
        if (Input.GetKeyDown(bendKey))
        {
            PerformEarthbend();
        }
    }

    private void PerformEarthbend()
    {
        // Shoot a ray from the camera to see where the player is looking
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // The raycast only checks for objects on the "Ground" layer within our bendRange
        if (Physics.Raycast(ray, out hit, bendRange, groundLayerMask))
        {
            // We hit the ground! Start the bending sequence at the hit point.
            // We use a Coroutine to handle the animation over several frames.
            StartCoroutine(BendRockFromGround(hit.point));
        }
    }

    // A Coroutine lets us perform actions over a period of time.
    private IEnumerator BendRockFromGround(Vector3 spawnPosition)
    {
        // 1. Play the visual effect at the target location.
        if (spawnVFX != null)
        {
            Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        }

        // 2. Define start and end positions for our animation.
        Vector3 startPosition = spawnPosition - Vector3.up * 0.5f; // Start slightly below ground
        Vector3 endPosition = spawnPosition + Vector3.up * pullHeight;

        // 3. Spawn the rock and get its components.
        GameObject newRock = Instantiate(rockPrefab, startPosition, Quaternion.identity);
        Rigidbody rockRb = newRock.GetComponent<Rigidbody>();
        Collider rockCollider = newRock.GetComponent<Collider>();

        // 4. Prepare the rock for animation: take control from physics.
        rockRb.isKinematic = true;
        rockCollider.enabled = false; // Disable collider to prevent snagging on the ground

        // 5. Animate the rock rising over 'pullDuration'.
        float elapsedTime = 0f;
        while (elapsedTime < pullDuration)
        {
            // Move the rock from start to end position smoothly
            newRock.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / pullDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 6. Finalize the rock's state.
        newRock.transform.position = endPosition; // Ensure it's at the final position
        rockCollider.enabled = true; // Re-enable the collider
        rockRb.isKinematic = false; // Give control back to the physics engine!
    }
}