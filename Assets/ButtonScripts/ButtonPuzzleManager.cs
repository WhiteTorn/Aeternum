using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class ButtonPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Drag the audio clips here in the correct order for the puzzle solution.")]
    [SerializeField] private List<AudioClip> correctSequence;
    
    [Tooltip("The delay in seconds between each sound when playing the hint.")]
    [SerializeField] private float hintPlaybackDelay = 1.0f;

    [Header("Feedback Sounds")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failureSound;

    [Header("Events")]
    [Tooltip("This event is invoked when the puzzle is solved correctly.")]
    public UnityEvent OnPuzzleSolved;

    private List<AudioClip> playerSequence = new List<AudioClip>();
    private AudioSource audioSource;
    private bool isPlayingHint = false; 

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void RecordPlayerInput(AudioClip sound)
    {
        if (isPlayingHint) return;

        playerSequence.Add(sound);
        Debug.Log("Player sequence now has " + playerSequence.Count + " sounds.");
    }

    public void CheckSequence()
    {
        if (isPlayingHint) return;

        if (IsSequenceCorrect())
        {
            Debug.Log("SUCCESS! The sequence is correct.");
            audioSource.PlayOneShot(successSound);
            OnPuzzleSolved.Invoke();
        }
        else
        {
            Debug.Log("FAILURE! The sequence is wrong. Resetting.");
            audioSource.PlayOneShot(failureSound);
        }
        
        playerSequence.Clear();
    }

    private bool IsSequenceCorrect()
    {
        if (playerSequence.Count != correctSequence.Count) return false;
        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (playerSequence[i] != correctSequence[i]) return false;
        }
        return true;
    }

    public void PlayHintSequence()
    {
        
        if (isPlayingHint)
        {
            Debug.Log("Hint is already playing.");
            return;
        }

        playerSequence.Clear();

        StartCoroutine(HintPlaybackRoutine());
    }

    private IEnumerator HintPlaybackRoutine()
    {
        isPlayingHint = true;
        Debug.Log("Playing hint sequence...");

        foreach (AudioClip sound in correctSequence)
        {
            audioSource.PlayOneShot(sound);
            yield return new WaitForSeconds(hintPlaybackDelay);
        }

        Debug.Log("Hint sequence finished.");
        isPlayingHint = false;
    }
}