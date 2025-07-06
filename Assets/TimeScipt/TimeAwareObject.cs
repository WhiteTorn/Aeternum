using UnityEngine;

public class TimeAwareObject : MonoBehaviour
{
    // --- NEW ENUM ---
    public enum DisplayMode { SwapMaterial, SwapPrefab }

    private struct ObjectState { public Vector3 position; public Quaternion rotation; }
    private ObjectState pastState, presentState, futureState;

    [Header("Core Configuration")]
    [Tooltip("How this object should change its appearance across timelines.")]
    [SerializeField] private DisplayMode displayMode = DisplayMode.SwapMaterial;
    [Tooltip("The time dimensions in which this object is allowed to exist.")]
    [SerializeField] private TimeManager.TimelineMask activeInTimelines = TimeManager.TimelineMask.All;

    [Header("Mode 1: Material Swapping")]
    [Tooltip("The renderer whose material will be swapped. Only used if Display Mode is SwapMaterial.")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material pastMaterial;
    [SerializeField] private Material presentMaterial;
    [SerializeField] private Material futureMaterial;

    [Header("Mode 2: Prefab Swapping")]
    [Tooltip("The visual representation for the Past. Only used if Display Mode is SwapPrefab.")]
    [SerializeField] private GameObject pastPrefab;
    [Tooltip("The visual representation for the Present. Only used if Display Mode is SwapPrefab.")]
    [SerializeField] private GameObject presentPrefab;
    [Tooltip("The visual representation for the Future. Only used if Display Mode is SwapPrefab.")]
    [SerializeField] private GameObject futurePrefab;

    // --- Private state variables ---
    private GameObject pastInstance, presentInstance, futureInstance;
    private TimeManager.TimeDimension _birthDimension;
    private bool isDynamicallySpawned = false;

    private void Awake()
    {
        TimeManager.Instance.Register(this);
        _birthDimension = TimeManager.TimeDimension.Past;

        ObjectState initialState = new ObjectState { position = transform.position, rotation = transform.rotation };
        pastState = presentState = futureState = initialState;

        // --- Setup based on chosen mode ---
        if (displayMode == DisplayMode.SwapMaterial)
        {
            // If no renderer is assigned, try to find one on this GameObject.
            if (objectRenderer == null) { objectRenderer = GetComponent<Renderer>(); }
        }
        else if (displayMode == DisplayMode.SwapPrefab)
        {
            InstantiateVisuals();
        }
    }

    private void Start()
    {
        ApplyState(TimeManager.Instance.CurrentTime);
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null) { TimeManager.Instance.Unregister(this); }
    }

    private void InstantiateVisuals()
    {
        GameObject fallbackPrefab = presentPrefab;
        if (presentPrefab != null) presentInstance = Instantiate(presentPrefab, transform);
        
        GameObject pastVisualPrefab = (pastPrefab != null) ? pastPrefab : fallbackPrefab;
        if (pastVisualPrefab != null) pastInstance = Instantiate(pastVisualPrefab, transform);

        GameObject futureVisualPrefab = (futurePrefab != null) ? futurePrefab : fallbackPrefab;
        if (futureVisualPrefab != null) futureInstance = Instantiate(futureVisualPrefab, transform);
    }

    // --- MODIFIED: The core logic now checks the displayMode ---
    private void ApplyState(TimeManager.TimeDimension time)
    {
        // Paradox check for spawned objects
        if (isDynamicallySpawned && (int)time < (int)_birthDimension)
        {
            Destroy(gameObject);
            return;
        }

        // Visibility check
        TimeManager.TimelineMask currentMask = ConvertDimensionToMask(time);
        bool shouldExist = (activeInTimelines & currentMask) != 0;
        gameObject.SetActive(shouldExist);
        if (!shouldExist) return;

        // Apply position and rotation to the parent object
        switch (time)
        {
            case TimeManager.TimeDimension.Past: transform.position = pastState.position; transform.rotation = pastState.rotation; break;
            case TimeManager.TimeDimension.Present: transform.position = presentState.position; transform.rotation = presentState.rotation; break;
            case TimeManager.TimeDimension.Future: transform.position = futureState.position; transform.rotation = futureState.rotation; break;
        }

        // --- NEW: Choose which visual update logic to run ---
        switch (displayMode)
        {
            case DisplayMode.SwapMaterial:
                ApplyMaterial(time);
                break;
            case DisplayMode.SwapPrefab:
                ApplyPrefabVisibility(time);
                break;
        }
    }
    
    // --- The original material-swapping logic, now in its own method ---
    private void ApplyMaterial(TimeManager.TimeDimension time)
    {
        if (objectRenderer == null) return;
        switch (time)
        {
            case TimeManager.TimeDimension.Past: if (pastMaterial != null) objectRenderer.material = pastMaterial; break;
            case TimeManager.TimeDimension.Present: if (presentMaterial != null) objectRenderer.material = presentMaterial; break;
            case TimeManager.TimeDimension.Future: if (futureMaterial != null) objectRenderer.material = futureMaterial; break;
        }
    }

    // --- The prefab-swapping logic, now in its own method ---
    private void ApplyPrefabVisibility(TimeManager.TimeDimension time)
    {
        if (pastInstance != null) pastInstance.SetActive(time == TimeManager.TimeDimension.Past);
        if (presentInstance != null) presentInstance.SetActive(time == TimeManager.TimeDimension.Present);
        if (futureInstance != null) futureInstance.SetActive(time == TimeManager.TimeDimension.Future);
    }

    // (The rest of the methods like InitializeForSpawning, HandleTimeChange, etc., are unchanged)
    #region Unchanged Methods
    public void InitializeForSpawning(TimeManager.TimeDimension birthDimension, TimeManager.TimelineMask visibilityMask)
    {
        this.isDynamicallySpawned = true;
        this._birthDimension = birthDimension;
        this.activeInTimelines = visibilityMask;
        RecordState(birthDimension);
        PropagateStates(birthDimension);
        ApplyState(TimeManager.Instance.CurrentTime);
    }
    public void HandleTimeChange(TimeManager.TimeDimension oldTime, TimeManager.TimeDimension newTime)
    {
        if (gameObject.activeSelf) { RecordState(oldTime); }
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
    #endregion
}