using UnityEngine;

public class SandClockScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The tag of the object that can interact with this. Should be 'Player'.")]
    [SerializeField] private string interactionTag = "Player";

    private bool isPlayerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(interactionTag))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the Sandclock.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(interactionTag))
        {
            isPlayerInRange = false;
            Debug.Log("Player has left the range of the Sandclock.");
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

        TimeManager.Instance.EnableTimeControl();

       
        TimeManager.Instance.SwitchToPresent();

        gameObject.SetActive(false);
    }
}