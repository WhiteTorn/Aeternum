using System.Collections;
using System.Collections.Generic; // Required for using Lists
using UnityEngine;

public class EarthbendingController : MonoBehaviour
{
    [Header("Setup Fields")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Assign your rock prefabs here: 0=Regular, 1=Tall, 2=Wide")]
    [SerializeField] private GameObject[] rockPrefabs; // An array for our different rock types
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Bending Settings")]
    [SerializeField] private KeyCode bendKey = KeyCode.E;
    [SerializeField] private KeyCode switchTypeKey = KeyCode.Q; // Key to cycle through rock types
    [SerializeField] private int maxBentObjects = 3; // The maximum number of objects we can bend
    [SerializeField] private float bendRange = 15f;
    [SerializeField] private float pullHeight = 1f;
    [SerializeField] private float pullDuration = 0.5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem spawnVFX;
    
    // --- NEW VARIABLES ---
    private List<GameObject> bentRocks = new List<GameObject>(); // Tracks our active rocks
    private int currentRockTypeIndex = 0; // Tracks which rock type is selected

    void Update()
    {
        // Handle switching rock types
        if (Input.GetKeyDown(switchTypeKey))
        {
            SwitchRockType();
        }

        // Handle performing the bend
        if (Input.GetKeyDown(bendKey))
        {
            PerformEarthbend();
        }
    }

    private void SwitchRockType()
    {
        if (rockPrefabs.Length == 0) return; // Safety check

        // Cycle to the next index, wrapping around if we reach the end
        currentRockTypeIndex = (currentRockTypeIndex + 1) % rockPrefabs.Length;
        
        // Give the player feedback on what they selected
        Debug.Log("Switched to rock type: " + rockPrefabs[currentRockTypeIndex].name);
    }

    private void PerformEarthbend()
    {
        // --- NEW LIMIT LOGIC ---
        // 1. Clean up any destroyed rocks from our list
        bentRocks.RemoveAll(item => item == null);

        // 2. Check if we are at our limit
        if (bentRocks.Count >= maxBentObjects)
        {
            Debug.Log("Bending limit reached! Cannot spawn more rocks.");
            return; // Stop the function here
        }

        // Raycast logic remains the same
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, bendRange, groundLayerMask))
        {
            StartCoroutine(BendRockFromGround(hit.point));
        }
    }

    private IEnumerator BendRockFromGround(Vector3 spawnPosition)
    {
        if (spawnVFX != null)
        {
            Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        }

        Vector3 startPosition = spawnPosition - Vector3.up * 0.5f;
        Vector3 endPosition = spawnPosition + Vector3.up * pullHeight;
        
        // --- NEW: Select the correct prefab from our array ---
        GameObject prefabToSpawn = rockPrefabs[currentRockTypeIndex];
        GameObject newRock = Instantiate(prefabToSpawn, startPosition, Quaternion.identity);
        
        // --- NEW: Add the newly created rock to our tracking list ---
        bentRocks.Add(newRock);

        Rigidbody rockRb = newRock.GetComponent<Rigidbody>();
        Collider rockCollider = newRock.GetComponent<Collider>();

        rockRb.isKinematic = true;
        rockCollider.enabled = false;

        float elapsedTime = 0f;
        while (elapsedTime < pullDuration)
        {
            newRock.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / pullDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        newRock.transform.position = endPosition;
        rockCollider.enabled = true;
        rockRb.isKinematic = false;
    }
}