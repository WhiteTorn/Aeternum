using System;
using System.Collections.Generic; // Required for using Lists
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static TimeManager Instance { get; private set; }
    public static event Action<TimeDimension> OnTimeDimensionChanged;

   // In TimeManager.cs

private void Awake()
{
    // This is the classic robust singleton pattern for a persistent manager
    if (Instance != null && Instance != this)
    {
        // If another TimeManager already exists, destroy this new one.
        Destroy(gameObject);
    }
    else
    {
        // If this is the first TimeManager, make it the official instance...
        Instance = this;
        // ...and tell Unity not to destroy it when a new scene loads.
        DontDestroyOnLoad(gameObject);
    }
}
    // -------------------------

    // --- NEW: A list to actively manage all time-aware objects ---
    private List<TimeAwareObject> timeAwareObjects = new List<TimeAwareObject>();

    // --- ENUMS FOR TIME SYSTEM ---
    [Flags]
    public enum TimelineMask { None = 0, Past = 1 << 0, Present = 1 << 1, Future = 1 << 2, All = ~0 }
    public enum TimeDimension { Past, Present, Future }

    // --- PROPERTIES ---
    [Header("Current State")]
    [SerializeField]
    private TimeDimension _currentTime = TimeDimension.Present;
    public TimeDimension CurrentTime => _currentTime;
    public bool CanControlTime { get; private set; } = false;

    // --- REMOVED: The static event is no longer needed ---
    // public static event Action<TimeDimension> OnTimeChanged;

    private void Update()
    {
        if (CanControlTime)
        {
            if (Input.GetKeyDown(KeyCode.T)) { SwitchToPast(); }
            else if (Input.GetKeyDown(KeyCode.R)) { SwitchToPresent(); }
            else if (Input.GetKeyDown(KeyCode.Y)) { SwitchToFuture(); }
        }
    }

    // --- NEW: Methods for objects to register/unregister themselves ---
    public void Register(TimeAwareObject obj)
    {
        if (!timeAwareObjects.Contains(obj))
        {
            timeAwareObjects.Add(obj);
        }
    }

    public void Unregister(TimeAwareObject obj)
    {
        if (timeAwareObjects.Contains(obj))
        {
            timeAwareObjects.Remove(obj);
        }
    }

    public void EnableTimeControl()
    {
        if (CanControlTime) return;
        CanControlTime = true;
        Debug.Log("Time control abilities have been acquired!");
    }

    // --- MODIFIED: Switch methods now loop through the list ---
    public void SwitchToPast()
    {
        if (_currentTime == TimeDimension.Past) return;
        TimeDimension oldTime = _currentTime;
        _currentTime = TimeDimension.Past;
        UpdateAllObjects(oldTime, _currentTime);
        OnTimeDimensionChanged?.Invoke(_currentTime); // <-- ADD THIS LINE
        Debug.Log("Switched to PAST...");
    }

    public void SwitchToPresent()
    {
        if (_currentTime == TimeDimension.Present) return;
        TimeDimension oldTime = _currentTime;
        _currentTime = TimeDimension.Present;
        UpdateAllObjects(oldTime, _currentTime);
        OnTimeDimensionChanged?.Invoke(_currentTime); // <-- ADD THIS LINE
        Debug.Log("Switched to PRESENT...");
    }

    public void SwitchToFuture()
    {
        if (_currentTime == TimeDimension.Future) return;
        TimeDimension oldTime = _currentTime;
        _currentTime = TimeDimension.Future;
        UpdateAllObjects(oldTime, _currentTime);
        OnTimeDimensionChanged?.Invoke(_currentTime); // <-- ADD THIS LINE
        Debug.Log("Switched to FUTURE.");
    }

    // --- NEW: The central update loop ---
    private void UpdateAllObjects(TimeDimension oldTime, TimeDimension newTime)
    {
        // We loop through a copy in case the list is modified during the loop (e.g., an object is destroyed)
        foreach (var obj in new List<TimeAwareObject>(timeAwareObjects))
        {
            if (obj != null) // Safety check
            {
                obj.HandleTimeChange(oldTime, newTime);
            }
        }
    }
}