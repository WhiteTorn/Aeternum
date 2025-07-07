using UnityEngine;

public class CheckButton : InteractiveButton
{
    [Header("Check Button Settings")]
    [Tooltip("The central puzzle manager that tracks the sequence.")]
    [SerializeField] private ButtonPuzzleManager puzzleManager;

    protected override void Awake()
    {
        base.Awake();
        if (puzzleManager == null)
        {
            Debug.LogError("Puzzle Manager not assigned on " + gameObject.name);
        }
    }

    protected override void OnButtonPressed()
    {
        base.OnButtonPressed();
        puzzleManager.CheckSequence();
    }
}