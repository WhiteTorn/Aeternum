using UnityEngine;

public class TimeAwareObject : MonoBehaviour
{
    private struct ObjectState { public Vector3 position; public Quaternion rotation; }
    private ObjectState pastState, presentState, futureState;

    [Header("Configuration")]
    [Tooltip("The time dimensions in which this object is allowed to exist.")]
    [SerializeField] private TimeManager.TimelineMask activeInTimelines = TimeManager.TimelineMask.All;

    [Header("Visuals")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material pastMaterial, presentMaterial, futureStateMaterial;

    // --- NEW FIELDS TO TRACK CREATION ---
    private TimeManager.TimeDimension _birthDimension;
    private bool isDynamicallySpawned = false;

    private void Awake()
    {
        if (objectRenderer == null) { objectRenderer = GetComponent<Renderer>(); }
        TimeManager.Instance.Register(this);
        
        // For objects placed in the editor, we assume they were "born" in the past.
        _birthDimension = TimeManager.TimeDimension.Past;

        ObjectState initialState = new ObjectState { position = transform.position, rotation = transform.rotation };
        pastState = presentState = futureState = initialState;
    }

    private void Start()
    {
        ApplyState(TimeManager.Instance.CurrentTime);
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.Unregister(this);
        }
    }

    // This method is for your Earthbender
    public void InitializeForSpawning(TimeManager.TimeDimension birthDimension, TimeManager.TimelineMask visibilityMask)
    {
        // --- THIS IS THE KEY ---
        // We flag this object as spawned and record its true birth time.
        this.isDynamicallySpawned = true;
        this._birthDimension = birthDimension;
        
        this.activeInTimelines = visibilityMask;
        RecordState(birthDimension);
        PropagateStates(birthDimension);
        ApplyState(TimeManager.Instance.CurrentTime);
    }

    public void HandleTimeChange(TimeManager.TimeDimension oldTime, TimeManager.TimeDimension newTime)
    {
        if (gameObject.activeSelf)
        {
            RecordState(oldTime);
        }
        PropagateStates(oldTime);
        ApplyState(newTime);
    }

    private void RecordState(TimeManager.TimeDimension time)
    {
        switch (time)
        {
            case TimeManager.TimeDimension.Past: pastState.position = transform.position; pastState.rotation = transform.rotation; break;
            case TimeManager.TimeDimension.Present: presentState.position = transform.position; presentState.rotation = transform.rotation; break;
            case TimeManager.TimeDimension.Future: futureState.position = transform.position; futureState.rotation = transform.rotation; break;
        }
    }

    private void PropagateStates(TimeManager.TimeDimension changedTime)
    {
        switch (changedTime)
        {
            case TimeManager.TimeDimension.Past: presentState = pastState; futureState = pastState; break;
            case TimeManager.TimeDimension.Present: futureState = presentState; break;
        }
    }

    private TimeManager.TimelineMask ConvertDimensionToMask(TimeManager.TimeDimension dimension)
    {
        switch (dimension)
        {
            case TimeManager.TimeDimension.Past: return TimeManager.TimelineMask.Past;
            case TimeManager.TimeDimension.Present: return TimeManager.TimelineMask.Present;
            case TimeManager.TimeDimension.Future: return TimeManager.TimelineMask.Future;
            default: return TimeManager.TimelineMask.None;
        }
    }

    // --- MODIFIED: The core logic now decides between Destroy() and SetActive() ---
    private void ApplyState(TimeManager.TimeDimension time)
    {
        // --- NEW DESTRUCTION LOGIC ---
        // Check if this object was spawned AND if we've traveled to a time before its creation.
        // We can compare the enums as integers (Past=0, Present=1, Future=2).
        if (isDynamicallySpawned && (int)time < (int)_birthDimension)
        {
            // This is a paradox! Destroy the object permanently.
            // OnDestroy() will handle unregistering from the TimeManager.
            Destroy(gameObject);
            return; // Stop processing for this now-destroyed object.
        }

        // --- STANDARD VISIBILITY LOGIC ---
        // If we didn't destroy it, proceed with the normal show/hide logic.
        TimeManager.TimelineMask currentMask = ConvertDimensionToMask(time);
        bool shouldExist = (activeInTimelines & currentMask) != 0;
        gameObject.SetActive(shouldExist);

        if (!shouldExist) return;

        // Apply transform and material...
        switch (time)
        {
            case TimeManager.TimeDimension.Past:
                transform.position = pastState.position;
                transform.rotation = pastState.rotation;
                if (objectRenderer != null && pastMaterial != null) objectRenderer.material = pastMaterial;
                break;
            case TimeManager.TimeDimension.Present:
                transform.position = presentState.position;
                transform.rotation = presentState.rotation;
                if (objectRenderer != null && presentMaterial != null) objectRenderer.material = presentMaterial;
                break;
            case TimeManager.TimeDimension.Future:
                transform.position = futureState.position;
                transform.rotation = futureState.rotation;
                if (objectRenderer != null && futureStateMaterial != null) objectRenderer.material = futureStateMaterial;
                break;
        }
    }
}