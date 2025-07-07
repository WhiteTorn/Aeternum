using UnityEngine;

public class HintButton : InteractiveButton
{
    [Header("Hint Button Settings")]
    [Tooltip("The central puzzle manager that knows the correct sequence.")]
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
        
        puzzleManager.PlayHintSequence();
    }
}