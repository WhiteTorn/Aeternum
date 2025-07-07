using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Door Pivots")]
    [Tooltip("The Transform of the Left Door object that will be rotated.")]
    [SerializeField] private Transform leftDoorPivot;
    [Tooltip("The Transform of the Right Door object that will be rotated.")]
    [SerializeField] private Transform rightDoorPivot;

    [Header("Animation Settings")]
    [Tooltip("The angle for the left door to open. Usually positive.")]
    [SerializeField] private float openAngleLeft = 120f;
    [Tooltip("The angle for the right door to open. Usually negative.")]
    [SerializeField] private float openAngleRight = -120f;
    [Tooltip("How long the animation takes.")]
    [SerializeField] private float animationDuration = 2f;

    private Quaternion leftDoorClosedRotation;
    private Quaternion leftDoorOpenRotation;
    private Quaternion rightDoorClosedRotation;
    private Quaternion rightDoorOpenRotation;

    private bool isDoorOpen = false;
    private bool isAnimating = false;

    private void Start()
    {
        if (leftDoorPivot == null || rightDoorPivot == null)
        {
            Debug.LogError("One or both door pivots are not assigned in the Inspector!", this.gameObject);
            return;
        }

        leftDoorClosedRotation = leftDoorPivot.localRotation;
        rightDoorClosedRotation = rightDoorPivot.localRotation;

        leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0, openAngleLeft, 0);
        rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0, openAngleRight, 0);
    }

   
    public void Interact()
    {
        if (isAnimating || leftDoorPivot == null || rightDoorPivot == null)
        {
            return; 
        }

        isDoorOpen = !isDoorOpen;
        StartCoroutine(AnimateDoors(isDoorOpen));
    }

    private IEnumerator AnimateDoors(bool open)
    {
        isAnimating = true;

        Quaternion leftStartRot = leftDoorPivot.localRotation;
        Quaternion rightStartRot = rightDoorPivot.localRotation;

        Quaternion leftEndRot = open ? leftDoorOpenRotation : leftDoorClosedRotation;
        Quaternion rightEndRot = open ? rightDoorOpenRotation : rightDoorClosedRotation;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            leftDoorPivot.localRotation = Quaternion.Slerp(leftStartRot, leftEndRot, elapsedTime / animationDuration);
            rightDoorPivot.localRotation = Quaternion.Slerp(rightStartRot, rightEndRot, elapsedTime / animationDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        leftDoorPivot.localRotation = leftEndRot;
        rightDoorPivot.localRotation = rightEndRot;

        isAnimating = false;
    }
}