using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))] // Ensure there's a collider for the trigger
public class InteractiveButton : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The local axis the button will move along (e.g., Y for up/down).")]
    [SerializeField] private Vector3 animationAxis = Vector3.up;
    [Tooltip("How far the button moves when pressed.")]
    [SerializeField] private float animationDistance = -0.1f;
    [Tooltip("How fast the button press animation is.")]
    [SerializeField] private float animationDuration = 0.1f;

    [Header("Interaction")]
    [Tooltip("The key the player must press to use the button.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    // You could add a UI prompt here later if you want

    private Vector3 originalPosition;
    private bool isPlayerInRange = false;
    private bool isAnimating = false;

    protected virtual void Awake()
    {
        originalPosition = transform.localPosition;
        // Make sure the collider is a trigger so the player can walk into it
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Update()
    {
        // Only allow interaction if the player is in range and the button isn't already animating
        if (isPlayerInRange && !isAnimating && Input.GetKeyDown(interactionKey))
        {
            StartCoroutine(AnimatePress());
        }
    }

    private IEnumerator AnimatePress()
    {
        isAnimating = true;

        // --- Press Down ---
        Vector3 pressedPosition = originalPosition + (animationAxis.normalized * animationDistance);
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, pressedPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pressedPosition;

        // --- THIS IS WHERE THE SPECIFIC BUTTON ACTION HAPPENS ---
        OnButtonPressed();
        // ---------------------------------------------------------

        // A small delay at the bottom
        yield return new WaitForSeconds(0.1f);

        // --- Return Up ---
        elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(pressedPosition, originalPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;

        isAnimating = false;
    }

    /// <summary>
    /// This method is meant to be overridden by child classes (SoundButton, CheckButton)
    /// to define what happens when the button is physically pressed.
    /// </summary>
    protected virtual void OnButtonPressed()
    {
        // Base class does nothing, child classes will implement this.
        Debug.Log(gameObject.name + " was pressed.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}