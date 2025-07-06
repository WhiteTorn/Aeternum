using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Time Display")]
    [Tooltip("The RectTransform of the time icon that will be rotated.")]
    [SerializeField] private RectTransform timeIconRectTransform;
    [Tooltip("How fast the icon rotates to the new position.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Earthbending Display")]
    [Tooltip("The player's EarthbendingController script.")]
    [SerializeField] private EarthbendingController playerBending;

    [Header("Selected Rock Type")]
    [Tooltip("The UI Image that shows the icon of the currently selected rock type (e.g., tall, wide).")]
    [SerializeField] private Image selectedRockTypeIcon;
    [Tooltip("A list of sprites for the rock types. MUST match the order in EarthbendingController's prefabs.")]
    [SerializeField] private List<Sprite> rockTypeSprites;

    [Header("Available Rock Count")]
    [Tooltip("The single UI Image that will display the '1', '2', or '3' sprite.")]
    [SerializeField] private Image rockCountImage;
    [Tooltip("The sprite for '0' rocks available.")]
    [SerializeField] private Sprite rockCount_0_Sprite;
    [Tooltip("The sprite for '1' rock available.")]
    [SerializeField] private Sprite rockCount_1_Sprite;
    [Tooltip("The sprite for '2' rocks available.")]
    [SerializeField] private Sprite rockCount_2_Sprite;
    [Tooltip("The sprite for '3' rocks available.")]
    [SerializeField] private Sprite rockCount_3_Sprite;


    // --- Private Variables ---
    private Quaternion targetIconRotation = Quaternion.identity;

    // --- Setup and Subscriptions ---
    private void OnEnable()
    {
        TimeManager.OnTimeDimensionChanged += HandleTimeChange;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeDimensionChanged -= HandleTimeChange;
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            HandleTimeChange(TimeManager.Instance.CurrentTime);
        }
    }

    private void Update()
    {
        // Smoothly rotate the time icon
        if (timeIconRectTransform != null)
        {
            timeIconRectTransform.rotation = Quaternion.Slerp(timeIconRectTransform.rotation, targetIconRotation, Time.deltaTime * rotationSpeed);
        }
        
        // Update the earthbending UI elements
        UpdateBendingUI();
    }

    // --- UI Update Methods ---
    private void HandleTimeChange(TimeManager.TimeDimension newTime)
    {
        float targetZRotation = 0f;
        switch (newTime)
        {
            case TimeManager.TimeDimension.Past: targetZRotation = -120f; break;
            case TimeManager.TimeDimension.Present: targetZRotation = 0f; break;
            case TimeManager.TimeDimension.Future: targetZRotation = 120f; break;
        }
        targetIconRotation = Quaternion.Euler(0, 0, targetZRotation);
    }

    private void UpdateBendingUI()
    {
        if (playerBending == null) return;

        // 1. Update the Selected Rock Type Icon
        if (selectedRockTypeIcon != null && rockTypeSprites.Count > 0)
        {
            int rockIndex = playerBending.CurrentRockIndex;
            if (rockIndex >= 0 && rockIndex < rockTypeSprites.Count)
            {
                selectedRockTypeIcon.sprite = rockTypeSprites[rockIndex];
            }
        }

        // 2. Update the Available Rock Count Image
        if (rockCountImage != null)
        {
            int availableSlots = playerBending.MaxRockCount - playerBending.CurrentRockCount;
            
            // Choose the correct sprite based on the number of available slots
            switch (availableSlots)
            {
                case 3:
                    rockCountImage.sprite = rockCount_3_Sprite;
                    break;
                case 2:
                    rockCountImage.sprite = rockCount_2_Sprite;
                    break;
                case 1:
                    rockCountImage.sprite = rockCount_1_Sprite;
                    break;
                default: // This covers 0 and any other case
                    rockCountImage.sprite = rockCount_0_Sprite;
                    break;
            }
        }
    }
}