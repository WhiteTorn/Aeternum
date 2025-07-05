using Unity.VisualScripting;
using UnityEngine;

public class TimeAwareObject : MonoBehaviour
{
    // A simple struct to hold the state of our object.
    private struct ObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    // Stored states for each dimension
    private ObjectState pastState;
    private ObjectState presentState;
    private ObjectState futureState;

    [Header("Configuration")]
    [Tooltip("The time dimension this object was created in. This will be set by the script that spawns it.")]
    [SerializeField] private TimeManager.TimeDimension creationDimension = TimeManager.TimeDimension.Past; // Default to Past for objects placed in the editor

    [Header("Visuals")]
    [Tooltip("The renderer whose material will be swapped.")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material pastMaterial;
    [SerializeField] private Material presentMaterial;
    [SerializeField] private Material futureStateMaterial;

    private bool isInitialized = false;

    private void Awake()
    {
        // If no renderer is assigned, try to find one on this GameObject.
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
    }

    private void OnEnable()
    {
        TimeManager.OnTimeChanged += HandleTimeChange;
        // If the object is re-enabled, ensure its state is correct for the current time
        if (isInitialized)
        {
            ApplyState(TimeManager.Instance.CurrentTime);
        }
    }

    private void OnDisable()
    {
        TimeManager.OnTimeChanged -= HandleTimeChange;
    }

    /// <summary>
    /// This method MUST be called by the script that instantiates this object.
    /// It sets the object's creation time and saves its initial state.
    /// </summary>
    public void Initialize(TimeManager.TimeDimension birthDimension)
    {
        this.creationDimension = birthDimension;

        // Save the initial state ONLY in its birth dimension and future dimensions.
        RecordState(birthDimension);
        PropagateStates(birthDimension);

        isInitialized = true;

        // Immediately apply the correct state to ensure it's visible/invisible as needed.
        ApplyState(TimeManager.Instance.CurrentTime);
    }

    private void HandleTimeChange(TimeManager.TimeDimension newTime)
    {
        // 1. Record the object's CURRENT transform into the OLD time dimension's state
        //    (Only if the object was active in that timeline)
        if (gameObject.activeSelf)
        {
            RecordState(TimeManager.Instance.CurrentTime);
        }

        // 2. Propagate the changes according to the rules
        PropagateStates(TimeManager.Instance.CurrentTime);

        // 3. Apply the state (transform and material) for the NEW time dimension
        ApplyState(newTime);
    }

    private void RecordState(TimeManager.TimeDimension time)
    {
        switch (time)
        {
            case TimeManager.TimeDimension.Past:
                pastState.position = transform.position;
                pastState.rotation = transform.rotation;
                break;
            case TimeManager.TimeDimension.Present:
                presentState.position = transform.position;
                presentState.rotation = transform.rotation;
                break;
            case TimeManager.TimeDimension.Future:
                futureState.position = transform.position;
                futureState.rotation = transform.rotation;
                break;
        }
    }

    private void PropagateStates(TimeManager.TimeDimension changedTime)
    {
        switch (changedTime)
        {
            case TimeManager.TimeDimension.Past:
                presentState = pastState;
                futureState = pastState;
                break;
            case TimeManager.TimeDimension.Present:
                futureState = presentState;
                break;
            case TimeManager.TimeDimension.Future:
                // No propagation
                break;
        }
    }

    private void ApplyState(TimeManager.TimeDimension time)
    {
        // --- THIS IS THE KEY LOGIC FIX ---
        // An object should only exist if the current time is its creation time or later.
        // We can treat the enums as integers for comparison (Past=0, Present=1, Future=2)
        bool shouldExist = (int)time >= (int)creationDimension;

        // Activate or deactivate the object based on the check.
        gameObject.SetActive(shouldExist);


        // If it shouldn't exist, stop here.
        if (!shouldExist)
        {
            Destroy(gameObject);
            return;
        }

        // --- The rest of the method is the same ---
        switch (time)
        {
            case TimeManager.TimeDimension.Past:
                transform.position = pastState.position;
                transform.rotation = pastState.rotation;
                if (objectRenderer != null && pastMaterial != null)
                    objectRenderer.material = pastMaterial;
                break;
            case TimeManager.TimeDimension.Present:
                transform.position = presentState.position;
                transform.rotation = presentState.rotation;
                if (objectRenderer != null && presentMaterial != null)
                    objectRenderer.material = presentMaterial;
                break;
            case TimeManager.TimeDimension.Future:
                transform.position = futureState.position;
                transform.rotation = futureState.rotation;
                if (objectRenderer != null && futureStateMaterial != null)
                    objectRenderer.material = futureStateMaterial;
                break;
        }
    }
}