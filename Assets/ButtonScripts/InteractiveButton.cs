using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
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

    private Vector3 originalPosition;
    private bool isPlayerInRange = false;
    private bool isAnimating = false;

    protected virtual void Awake()
    {
        originalPosition = transform.localPosition;
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Update()
    {
        if (isPlayerInRange && !isAnimating && Input.GetKeyDown(interactionKey))
        {
            StartCoroutine(AnimatePress());
        }
    }

    private IEnumerator AnimatePress()
    {
        isAnimating = true;

        Vector3 pressedPosition = originalPosition + (animationAxis.normalized * animationDistance);
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, pressedPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pressedPosition;

        OnButtonPressed();

        yield return new WaitForSeconds(0.1f);

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


    protected virtual void OnButtonPressed()
    {
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