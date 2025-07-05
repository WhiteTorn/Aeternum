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
        if (other.CompareTag(interactionTag))
        {
            isPlayerInRange = false;
            Debug.Log("Player has left the range of the Sandclock.");
            // Optional: Hide the UI prompt
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CollectSandclock();
        }
    }

    private void CollectSandclock()
    {
        Debug.Log("Sandclock collected! Forcing time shift to Present...");

        // 1. Tell the TimeManager to unlock the time-switching ability.
        TimeManager.Instance.EnableTimeControl();

        // --- NEW LINE ---
        // 2. Force the time to switch to the Present dimension.
        // This will fire the OnTimeChanged event that all your TimeAwareObjects listen to.
        TimeManager.Instance.SwitchToPresent();

        // 3. Deactivate the sandclock object to make it "disappear".
        //    This must be the LAST step, otherwise the script stops and can't call SwitchToPresent.
        gameObject.SetActive(false);

        // You could also add a screen fade or a sound effect here for dramatic impact!
    }
}