using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthbendingController : MonoBehaviour
{
    [Header("Setup Fields")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Assign your rock prefabs here: 0=Regular, 1=Tall, 2=Wide")]
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Bending Settings")]
    [SerializeField] private KeyCode bendKey = KeyCode.E;
    [SerializeField] private KeyCode switchTypeKey = KeyCode.Q;
    [SerializeField] private int maxBentObjects = 3;
    [SerializeField] private float bendRange = 15f;
    [SerializeField] private float pullHeight = 1f;
    [SerializeField] private float pullDuration = 0.5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem spawnVFX;

    private List<GameObject> bentRocks = new List<GameObject>();
    private int currentRockTypeIndex = 0;

    public void FreeUpBentRockSlot(GameObject rockToRemove)
    {
        if (bentRocks.Contains(rockToRemove))
        {
            bentRocks.Remove(rockToRemove);
            Debug.Log("A rock was stuck to a surface! Bending slot freed.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(switchTypeKey)) SwitchRockType();
        if (Input.GetKeyDown(bendKey)) PerformEarthbend();
    }

    private void SwitchRockType()
    {
        if (rockPrefabs.Length == 0) return;
        currentRockTypeIndex = (currentRockTypeIndex + 1) % rockPrefabs.Length;
        Debug.Log("Switched to rock type: " + rockPrefabs[currentRockTypeIndex].name);
    }

    private void PerformEarthbend()
    {
        bentRocks.RemoveAll(item => item == null);
        if (bentRocks.Count >= maxBentObjects)
        {
            Debug.Log("Bending limit reached! Cannot spawn more rocks.");
            return;
        }
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, bendRange, groundLayerMask))
        {
            StartCoroutine(BendRockFromGround(hit.point));
        }
    }

    private IEnumerator BendRockFromGround(Vector3 spawnPosition)
    {
        if (spawnVFX != null) Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
        Vector3 startPosition = spawnPosition - Vector3.up * 0.5f;
        Vector3 endPosition = spawnPosition + Vector3.up * pullHeight;
        GameObject prefabToSpawn = rockPrefabs[currentRockTypeIndex];
        GameObject newRock = Instantiate(prefabToSpawn, startPosition, Quaternion.identity);
        bentRocks.Add(newRock);

        // --- THIS IS THE CORRECTED BLOCK ---
        TimeAwareObject timeAwareComponent = newRock.GetComponent<TimeAwareObject>();
        if (timeAwareComponent != null)
        {
            // 1. Get the time the pillar is being created in.
            TimeManager.TimeDimension creationTime = TimeManager.Instance.CurrentTime;

            // 2. Determine which timelines it should be visible in based on causality.
            //    The | symbol is a "bitwise OR" which combines the flags.
            TimeManager.TimelineMask visibility;
            switch (creationTime)
            {
                case TimeManager.TimeDimension.Past:
                    // If created in Past, it exists in Past, Present, AND Future.
                    visibility = TimeManager.TimelineMask.Past | TimeManager.TimelineMask.Present | TimeManager.TimelineMask.Future;
                    break;
                case TimeManager.TimeDimension.Present:
                    // If created in Present, it exists in Present AND Future.
                    visibility = TimeManager.TimelineMask.Present | TimeManager.TimelineMask.Future;
                    break;
                case TimeManager.TimeDimension.Future:
                default: // Default case to be safe
                    // If created in Future, it exists ONLY in the Future.
                    visibility = TimeManager.TimelineMask.Future;
                    break;
            }

            // 3. Call the Initialize method with BOTH required arguments to fix the error.
            // This is the corrected line
            timeAwareComponent.InitializeForSpawning(creationTime, visibility);
        }
        // --- END OF CORRECTED BLOCK ---

        Rigidbody rockRb = newRock.GetComponent<Rigidbody>();
        Collider rockCollider = newRock.GetComponent<Collider>();
        if (rockRb) rockRb.isKinematic = true;
        if (rockCollider) rockCollider.enabled = false;

        float elapsedTime = 0f;
        while (elapsedTime < pullDuration)
        {
            if (newRock == null) yield break; // Safety check if rock is destroyed mid-animation
            newRock.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / pullDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (newRock == null) yield break; // Safety check

        newRock.transform.position = endPosition;
        if (rockCollider) rockCollider.enabled = true;
        if (rockRb) rockRb.isKinematic = false;
    }
}