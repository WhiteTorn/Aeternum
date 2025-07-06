using UnityEngine;

public class HintButton : InteractiveButton
{
    [Header("Hint Button Settings")]
    [Tooltip("The central puzzle manager that knows the correct sequence.")]
    [SerializeField] private ButtonPuzzleManager puzzleManager;

    protected override void Awake()
    {
        base.Awake(); // Call the parent Awake method
        if (puzzleManager == null)
        {
            Debug.LogError("Puzzle Manager not assigned on " + gameObject.name);
        }
    }

    protected override void OnButtonPressed()
    {
        base.OnButtonPressed(); // Call the base method for logging/consistency
        
        // Tell the puzzle manager to play the hint sequence.
        puzzleManager.PlayHintSequence();
    }
}