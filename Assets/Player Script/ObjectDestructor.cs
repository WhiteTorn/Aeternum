using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class EarthUnbender : MonoBehaviour
{
    [Header("Setup Fields")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ParticleSystem sinkVFX; 

    [Header("Settings")]
    [SerializeField] private float unbendRange = 20f;
    [SerializeField] private float sinkDuration = 0.5f;

    private List<GameObject> sinkingObjects = new List<GameObject>();

    void Update()
    {
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
            if (hit.collider.CompareTag("Grabbable") && !sinkingObjects.Contains(hit.collider.gameObject))
            {
                StartCoroutine(UnbendRockIntoGround(hit.collider.gameObject));
            }
        }
    }

    private IEnumerator UnbendRockIntoGround(GameObject objectToUnbend)
    {
        sinkingObjects.Add(objectToUnbend);

        Rigidbody rockRb = objectToUnbend.GetComponent<Rigidbody>();
        Collider rockCollider = objectToUnbend.GetComponent<Collider>();

        rockRb.isKinematic = true;
        rockCollider.enabled = false; 

        Vector3 startPosition = objectToUnbend.transform.position;
        Vector3 endPosition = startPosition - new Vector3(0, objectToUnbend.transform.localScale.y, 0);

        if (sinkVFX != null)
        {
            Instantiate(sinkVFX, startPosition, Quaternion.identity);
        }

        float elapsedTime = 0f;
        while (elapsedTime < sinkDuration)
        {
            objectToUnbend.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / sinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        sinkingObjects.Remove(objectToUnbend);
        Destroy(objectToUnbend);
    }
}