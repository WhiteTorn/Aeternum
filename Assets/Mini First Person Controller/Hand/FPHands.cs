using UnityEngine;

public class FPHands : MonoBehaviour
{
    [Header("Hand Model Settings")]
    public GameObject handModelPrefab; // Drag the hand.obj model here in Inspector
    public Vector3 handPosition = new Vector3(0.5f, -0.5f, 0.8f);
    public Vector3 handRotation = new Vector3(15f, -10f, 0f);
    public Vector3 handScale = new Vector3(1f, 1f, 1f);
    public Material handMaterial;
    
    private GameObject handInstance;
    private Camera fpCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the camera component (assuming this script is on the camera or player)
        fpCamera = Camera.main;
        if (fpCamera == null)
        {
            fpCamera = FindObjectOfType<Camera>();
        }
        
        CreateHandFromModel();
    }

    void CreateHandFromModel()
    {
        // Check if hand model prefab is assigned
        if (handModelPrefab == null)
        {
            Debug.LogError("Hand Model Prefab is not assigned! Please drag the hand.obj model to the Hand Model Prefab field in the inspector.");
            return;
        }
        
        // Instantiate the 3D hand model
        handInstance = Instantiate(handModelPrefab);
        handInstance.name = "FP_Hand_Model";
        
        // Set the hand as a child of the camera for proper positioning
        handInstance.transform.SetParent(fpCamera.transform);
        
        // Position and orient the hand for first-person view
        handInstance.transform.localPosition = handPosition;
        handInstance.transform.localRotation = Quaternion.Euler(handRotation);
        handInstance.transform.localScale = handScale;
        
        // Apply custom material if provided
        if (handMaterial != null)
        {
            ApplyMaterialToHand();
        }
        
        // Remove any colliders from the hand model (we don't want it to interfere with gameplay)
        RemoveCollidersFromHand();
        
        // Set the layer to ensure proper rendering
        SetHandLayer();
    }

    void ApplyMaterialToHand()
    {
        // Find all renderers in the hand model and apply the material
        Renderer[] renderers = handInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = handMaterial;
        }
    }

    void RemoveCollidersFromHand()
    {
        // Remove all colliders from the hand model and its children
        Collider[] colliders = handInstance.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            Destroy(collider);
        }
    }

    void SetHandLayer()
    {
        // Set the hand and all its children to a specific layer (you can create a "FPHand" layer)
        SetLayerRecursively(handInstance, LayerMask.NameToLayer("Default"));
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Keep the hand active and properly positioned
        if (handInstance != null && !handInstance.activeInHierarchy)
        {
            handInstance.SetActive(true);
        }
        
        // Optional: Add subtle hand movement/sway for more realism
        if (handInstance != null)
        {
            AddHandSway();
        }
    }

    void AddHandSway()
    {
        // Add subtle movement to make the hand feel more alive
        float swayX = Mathf.Sin(Time.time * 1.5f) * 0.005f;
        float swayY = Mathf.Sin(Time.time * 1.2f) * 0.003f;
        
        Vector3 originalPosition = handPosition;
        handInstance.transform.localPosition = originalPosition + new Vector3(swayX, swayY, 0);
    }

    // Public method to change hand pose/animation if needed
    public void SetHandPosition(Vector3 newPosition)
    {
        handPosition = newPosition;
        if (handInstance != null)
        {
            handInstance.transform.localPosition = handPosition;
        }
    }

    public void SetHandRotation(Vector3 newRotation)
    {
        handRotation = newRotation;
        if (handInstance != null)
        {
            handInstance.transform.localRotation = Quaternion.Euler(handRotation);
        }
    }
}
