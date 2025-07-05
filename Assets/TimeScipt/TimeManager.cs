using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    // -------------------------

    public enum TimeDimension { Past, Present, Future }

    [Header("Current State")]
    [SerializeField]
    private TimeDimension _currentTime = TimeDimension.Present;
    public TimeDimension CurrentTime => _currentTime;

    public static event Action<TimeDimension> OnTimeChanged;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) { SwitchToPast(); }
        else if (Input.GetKeyDown(KeyCode.R)) { SwitchToPresent(); }
        else if (Input.GetKeyDown(KeyCode.E)) { SwitchToFuture(); }
    }

    // =================================================================
    // =========== THE LOGIC FIX IS IN THE THREE METHODS BELOW =========
    // =================================================================
    
    public void SwitchToPast()
    {
        if (_currentTime == TimeDimension.Past) return;
        
        // IMPORTANT: We invoke the event BEFORE changing our internal state.
        // This lets all objects correctly record their state for the dimension we are LEAVING.
        OnTimeChanged?.Invoke(TimeDimension.Past);
        
        // Now we update our state.
        _currentTime = TimeDimension.Past;
        Debug.Log("Switched to PAST. Present and Future will be reset.");
    }

    public void SwitchToPresent()
    {
        if (_currentTime == TimeDimension.Present) return;
        
        OnTimeChanged?.Invoke(TimeDimension.Present);
        
        _currentTime = TimeDimension.Present;
        Debug.Log("Switched to PRESENT. Future will be reset.");
    }

    public void SwitchToFuture()
    {
        if (_currentTime == TimeDimension.Future) return;

        OnTimeChanged?.Invoke(TimeDimension.Future);

        _currentTime = TimeDimension.Future;
        Debug.Log("Switched to FUTURE.");
    }
}