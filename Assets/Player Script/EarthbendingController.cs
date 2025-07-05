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

    // --- NEW PUBLIC FUNCTION ---
    // This allows other scripts to tell us when a rock has been permanently placed.
    public void FreeUpBentRockSlot(GameObject rockToRemove)
    {
        if (bentRocks.Contains(rockToRemove))
        {
            bentRocks.Remove(rockToRemove);
            Debug.Log("A rock was stuck to a surface! Bending slot freed.");
        }
    }
    
    // --- The rest of your script remains unchanged ---
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