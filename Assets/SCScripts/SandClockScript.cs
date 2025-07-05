using UnityEngine;

public class SandClockScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The tag of the object that can interact with this. Should be 'Player'.")]
    [SerializeField] private string interactionTag = "Player";

    // This flag will be true only when the player is inside the trigger zone.
    private bool isPlayerInRange = false;

    // This method is called by Unity when another collider enters this object's trigger zone.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag(interactionTag))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the Sandclock.");
            // Optional: Show a UI prompt like "[E] to Take"
        }
    }

    // This method is called by Unity when another collider exits this object's trigger zone.
    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited is the player
        if (other.CompareTag(interactionTag))
        {
            isPlayerInRange = false;
            Debug.Log("Player has left the range of the Sandclock.");
            // Optional: Hide the UI prompt
        }
    }

    private void Update()
    {
        // If the player is in range AND they press the E key...
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // ...then perform the action!
            CollectSandclock();
        }
    }

    private void CollectSandclock()
    {
        Debug.Log("Sandclock collected!");

        // 1. Tell the TimeManager to unlock the time-switching ability.
        TimeManager.Instance.EnableTimeControl();

        // 2. Deactivate the sandclock object to make it "disappear".
        //    This also stops this script from running again.
        gameObject.SetActive(false);
    }
}