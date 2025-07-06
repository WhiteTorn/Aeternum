using UnityEngine;
using UnityEngine.SceneManagement; // <-- IMPORTANT: Add this line!

public class SceneSwitch : MonoBehaviour
{
    [Tooltip("The build index of the scene to load. Find this in File > Build Settings.")]
    [SerializeField] private int sceneNumber;

    // This function is called when another collider enters this object's trigger zone.
    private void OnTriggerEnter(Collider other)
    {
        // We check if the object that entered is tagged as "Player".
        // This prevents things like rocks or enemies from triggering the scene change.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the scene switch trigger. Loading scene: " + sceneNumber);
            TimeManager.Instance.SwitchToPresent();
            SceneManager.LoadScene(sceneNumber);
            TimeManager.Instance.EnableTimeControl();
        }
    }
}