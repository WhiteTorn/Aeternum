using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }
    // -------------------------

    // ADD THIS NEW PROPERTY
    // This will be false by default, locking the time-switching ability.
    public bool CanControlTime { get; private set; } = false;

    public enum TimeDimension { Past, Present, Future }

    [Header("Current State")]
    [SerializeField]
    private TimeDimension _currentTime = TimeDimension.Present;
    public TimeDimension CurrentTime => _currentTime;

    public static event Action<TimeDimension> OnTimeChanged;

    // MODIFY THE UPDATE METHOD
    private void Update()
    {
        // IMPORTANT: Only check for input if the player has the ability!
        if (CanControlTime)
        {
            if (Input.GetKeyDown(KeyCode.T)) { SwitchToPast(); }
            else if (Input.GetKeyDown(KeyCode.R)) { SwitchToPresent(); }
            // Note: We use 'E' for the future. The Sandclock script will handle
            // the interaction 'E' press, so they won't conflict.
            else if (Input.GetKeyDown(KeyCode.Y)) { SwitchToFuture(); }
        }
    }

    // ADD THIS NEW PUBLIC METHOD
    /// <summary>
    /// This method will be called by the Sandclock to grant the time-switching power.
    /// </summary>
    public void EnableTimeControl()
    {
        if (CanControlTime) return; // Don't do anything if it's already enabled

        CanControlTime = true;
        Debug.Log("Time control abilities have been acquired!");
        // You could also trigger a UI notification or sound effect here.
    }


    // (The SwitchToPast, SwitchToPresent, and SwitchToFuture methods remain unchanged)
    public void SwitchToPast()
    {
        if (_currentTime == TimeDimension.Past) return;
        OnTimeChanged?.Invoke(TimeDimension.Past);
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