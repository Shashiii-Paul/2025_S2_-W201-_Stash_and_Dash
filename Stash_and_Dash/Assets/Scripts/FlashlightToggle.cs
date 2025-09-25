using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    private Light flashlightLight; // Reference to the Light component on child Spotlight
    private bool isFlashlightActive = false; // Tracks if flashlight is picked up

    void Start()
    {
        // Get the Light component from child objects
        flashlightLight = GetComponentInChildren<Light>();
        if (flashlightLight == null)
        {
            Debug.LogError("No Light component found in children of FlashLightOnPlayer!");
        }

        // Ensure flashlight starts in the correct state
        if (flashlightLight != null)
        {
            isFlashlightActive = gameObject.activeSelf;
            flashlightLight.enabled = isFlashlightActive;
        }
    }

    void Update()
    {
        // Only process input if the flashlight GameObject is active
        if (gameObject.activeSelf && flashlightLight != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Toggle the light's enabled state
                isFlashlightActive = !isFlashlightActive;
                flashlightLight.enabled = isFlashlightActive;
                Debug.Log("Flashlight toggled: " + (isFlashlightActive ? "On" : "Off"));
            }
        }
    }
}