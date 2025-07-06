using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundButton : InteractiveButton
{
    [Header("Sound Button Settings")]
    [Tooltip("The central puzzle manager that tracks the sequence.")]
    [SerializeField] private ButtonPuzzleManager puzzleManager;

    [Header("Sounds per Dimension")]
    [SerializeField] private AudioClip pastSound;
    [SerializeField] private AudioClip presentSound;
    [SerializeField] private AudioClip futureSound;

    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake(); // Call the parent Awake method
        audioSource = GetComponent<AudioSource>();
        if (puzzleManager == null)
        {
            Debug.LogError("Puzzle Manager not assigned on " + gameObject.name);
        }
    }

    protected override void OnButtonPressed()
    {
        base.OnButtonPressed(); // Good practice to call the base method

        AudioClip soundToPlay = null;

        // Determine which sound to play based on the current time
        switch (TimeManager.Instance.CurrentTime)
        {
            case TimeManager.TimeDimension.Past:
                soundToPlay = pastSound;
                break;
            case TimeManager.TimeDimension.Present:
                soundToPlay = presentSound;
                break;
            case TimeManager.TimeDimension.Future:
                soundToPlay = futureSound;
                break;
        }

        if (soundToPlay != null)
        {
            // Play the sound
            audioSource.PlayOneShot(soundToPlay);
            // Tell the manager to record this sound
            puzzleManager.RecordPlayerInput(soundToPlay);
        }
    }
}