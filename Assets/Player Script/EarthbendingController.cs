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
    [Tooltip("The TOTAL maximum number of rocks you can have active at once.")]
    [SerializeField] private int maxBentObjects = 3;
    [SerializeField] private float bendRange = 15f;
    [SerializeField] private float pullHeight = 1f;
    [SerializeField] private float pullDuration = 0.5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem spawnVFX;

    private List<GameObject> bentRocks = new List<GameObject>();
    private int currentRockTypeIndex = 0;

    public int CurrentRockCount => bentRocks.Count;
    public int MaxRockCount => maxBentObjects;
    public int CurrentRockIndex => currentRockTypeIndex;


    void Update()
    {
        bentRocks.RemoveAll(item => item == null);

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
        if (bentRocks.Count >= maxBentObjects)
        {
            Debug.Log("Bending limit reached! Cannot spawn more rocks.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, bendRange, groundLayerMask))
        {
            StartCoroutine(BendRockFromGround(hit.point));
        }
    }

    private IEnumerator BendRockFromGround(Vector3 spawnPosition)
    {
        if (spawnVFX != null) Instantiate(spawnVFX, spawnPosition, Quaternion.identity);

        GameObject prefabToSpawn = rockPrefabs[currentRockTypeIndex];
        Vector3 startPosition = spawnPosition - Vector3.up * 0.5f;
        Vector3 endPosition = spawnPosition + Vector3.up * pullHeight;

        GameObject newRock = Instantiate(prefabToSpawn, startPosition, Quaternion.Euler(90, 0, 0));

        bentRocks.Add(newRock); 

        TimeAwareObject timeAwareComponent = newRock.GetComponent<TimeAwareObject>();
        if (timeAwareComponent != null)
        {
            TimeManager.TimeDimension creationTime = TimeManager.Instance.CurrentTime;
            TimeManager.TimelineMask visibility;
            switch (creationTime)
            {
                case TimeManager.TimeDimension.Past:
                    visibility = TimeManager.TimelineMask.Past | TimeManager.TimelineMask.Present | TimeManager.TimelineMask.Future;
                    break;
                case TimeManager.TimeDimension.Present:
                    visibility = TimeManager.TimelineMask.Present | TimeManager.TimelineMask.Future;
                    break;
                default:
                    visibility = TimeManager.TimelineMask.Future;
                    break;
            }
            timeAwareComponent.InitializeForSpawning(creationTime, visibility);
        }
        
        Rigidbody rockRb = newRock.GetComponent<Rigidbody>();
        Collider rockCollider = newRock.GetComponent<Collider>();
        if (rockRb) rockRb.isKinematic = true;
        if (rockCollider) rockCollider.enabled = false;
        
        float elapsedTime = 0f;
        while (elapsedTime < pullDuration)
        {
            if (newRock == null) yield break;
            newRock.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / pullDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (newRock == null) yield break;
        newRock.transform.position = endPosition;
        if (rockCollider) rockCollider.enabled = true;
        if (rockRb) rockRb.isKinematic = false;
    }
}