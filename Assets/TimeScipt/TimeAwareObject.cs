using UnityEngine;

public class TimeAwareObject : MonoBehaviour
{
    // A simple struct to hold the state of our object.
    // You can add more variables here (e.g., bool isEnabled, float health).
    private struct ObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
        // public bool isActive; // Example of another property you could track
    }

    // Stored states for each dimension
    private ObjectState pastState;
        private ObjectState presentState;
    private ObjectState futureState;

    [Header("Visuals")]
    [Tooltip("The renderer whose material will be swapped.")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material pastMaterial;
    [SerializeField] private Material presentMaterial;
    [SerializeField] private Material futureStateMaterial;

    private void Awake()
    {
        // On start, all states are the same as the object's initial state in the scene.
        pastState.position = transform.position;
        pastState.rotation = transform.rotation;

        presentState = pastState;
        futureState = pastState;

        // If no renderer is assigned, try to find one on this GameObject.
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
    }

    private void OnEnable()
    {
        // Subscribe to the event when this object is enabled
        TimeManager.OnTimeChanged += HandleTimeChange;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors when the object is destroyed
        TimeManager.OnTimeChanged -= HandleTimeChange;
    }

    /// <summary>
    /// This is the core logic. It's called by the TimeManager's event.
    /// </summary>
    private void HandleTimeChange(TimeManager.TimeDimension newTime)
    {
        // 1. Record the object's CURRENT transform into the OLD time dimension's state
        //    This captures any changes the player made before the time switch.
        RecordState(TimeManager.Instance.CurrentTime);

        // 2. Propagate the changes according to the rules
        PropagateStates(TimeManager.Instance.CurrentTime);

        // 3. Apply the state (transform and material) for the NEW time dimension
        ApplyState(newTime);
    }

    /// <summary>
    /// Saves the current transform of the GameObject into the specified state variable.
    /// </summary>
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
    
    /// <summary>
    /// This enforces the state propagation rules you defined.
    /// It's called AFTER recording a state change.
    /// </summary>
    private void PropagateStates(TimeManager.TimeDimension changedTime)
    {
        switch (changedTime)
        {
            // If we just changed something in the PAST...
            case TimeManager.TimeDimension.Past:
                // ...it propagates to Present and Future.
                presentState = pastState;
                futureState = pastState;
                break;

            // If we just changed something in the PRESENT...
            case TimeManager.TimeDimension.Present:
                // ...it only propagates to the Future. Past is unaffected.
                futureState = presentState;
                break;

            // If we just changed something in the FUTURE...
            case TimeManager.TimeDimension.Future:
                // ...it affects nothing else.
                break;
        }
    }

    /// <summary>
    /// Sets the object's transform and material based on the state for the target time.
    /// </summary>
    private void ApplyState(TimeManager.TimeDimension time)
    {
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