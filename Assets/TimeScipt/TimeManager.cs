using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public static event Action<TimeDimension> OnTimeDimensionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private List<TimeAwareObject> timeAwareObjects = new List<TimeAwareObject>();

    [Flags]
    public enum TimelineMask { None = 0, Past = 1 << 0, Present = 1 << 1, Future = 1 << 2, All = ~0 }
    public enum TimeDimension { Past, Present, Future }

    [Header("Current State")]
    [SerializeField]
    private TimeDimension _currentTime = TimeDimension.Present;
    public TimeDimension CurrentTime => _currentTime;
    public bool CanControlTime { get; private set; } = false;

    private void Update()
    {
        if (CanControlTime)
        {
            if (Input.GetKeyDown(KeyCode.T)) { SwitchToPast(); }
            else if (Input.GetKeyDown(KeyCode.R)) { SwitchToPresent(); }
            else if (Input.GetKeyDown(KeyCode.Y)) { SwitchToFuture(); }
        }
    }

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

    public void SwitchToPast()
    {
        if (_currentTime == TimeDimension.Past) return;
        TimeDimension oldTime = _currentTime;
        _currentTime = TimeDimension.Past;
        UpdateAllObjects(oldTime, _currentTime);
        OnTimeDimensionChanged?.Invoke(_currentTime);
        Debug.Log("Switched to PAST...");
    }

    public void SwitchToPresent()
    {
        if (_currentTime == TimeDimension.Present) return;
        TimeDimension oldTime = _currentTime;
        _currentTime = TimeDimension.Present;
        UpdateAllObjects(oldTime, _currentTime);
        OnTimeDimensionChanged?.Invoke(_currentTime);
        Debug.Log("Switched to PRESENT...");
    }

    public void SwitchToFuture()
    {
        if (_currentTime == TimeDimension.Future) return;
        TimeDimension oldTime = _currentTime;
        _currentTime = TimeDimension.Future;
        UpdateAllObjects(oldTime, _currentTime);
        OnTimeDimensionChanged?.Invoke(_currentTime);
        Debug.Log("Switched to FUTURE.");
    }

    private void UpdateAllObjects(TimeDimension oldTime, TimeDimension newTime)
    {
        foreach (var obj in new List<TimeAwareObject>(timeAwareObjects))
        {
            if (obj != null)
            {
                obj.HandleTimeChange(oldTime, newTime);
            }
        }
    }
}