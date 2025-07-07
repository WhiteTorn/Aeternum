using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneSwitch : MonoBehaviour
{
    [Tooltip("The build index of the scene to load. Find this in File > Build Settings.")]
    [SerializeField] private int sceneNumber;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the scene switch trigger. Loading scene: " + sceneNumber);
            TimeManager.Instance.SwitchToPresent();
            SceneManager.LoadScene(sceneNumber);
            TimeManager.Instance.EnableTimeControl();
        }
    }
}