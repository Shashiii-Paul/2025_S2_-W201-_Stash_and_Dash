using UnityEngine;

public class FlashlightDrop : MonoBehaviour
{
    public GameObject flashlightPickupPrefab; // Drag your pickup flashlight prefab here (ensure its Light is enabled in the prefab Inspector if you want it on by default)

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Y))
        {
            DropFlashlight();
        }
    }

    private void DropFlashlight()
    {
        if (flashlightPickupPrefab == null)
        {
            Debug.LogError("Flashlight pickup prefab not assigned!");
            return;
        }

        // Get player's transform (assuming this script is on a child of the player)
        Transform playerTransform = transform.parent;
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found!");
            return;
        }

        // Calculate drop position: In front of player, at higher height (not on floor)
        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1f; // Adjust *1f for forward distance
        dropPosition.y = playerTransform.position.y + 1f; // Spawn at player's y + 1f (e.g., waist/chest level; increase to 1.5f or 2f for higher)

        // Instantiate the pickup prefab
        GameObject droppedFlashlight = Instantiate(flashlightPickupPrefab, dropPosition, playerTransform.rotation);

        // Explicitly enable the Light on the dropped clone (fix for not enabling)
        Light droppedLight = droppedFlashlight.GetComponentInChildren<Light>();
        if (droppedLight != null)
        {
            droppedLight.enabled = true; // Turn it on if you want the dropped one lit
            // Optional: Set intensity, range, etc. if needed, e.g., droppedLight.intensity = 1f;
        }
        else
        {
            Debug.LogWarning("No Light found on dropped flashlight!");
        }

        // Ensure the whole object is active
        droppedFlashlight.SetActive(true);

        // Optional: Add a gentle forward force if the prefab has a Rigidbody (for a "toss" effect, and it will fall if gravity is on)
        Rigidbody rb = droppedFlashlight.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(playerTransform.forward * 2f, ForceMode.Impulse); // Adjust force as needed
        }

        // Deactivate the player's flashlight
        Light flashlightLight = GetComponentInChildren<Light>();
        if (flashlightLight != null)
        {
            flashlightLight.enabled = false;
        }
        gameObject.SetActive(false);

        Debug.Log("Flashlight dropped at: " + dropPosition);
    }
}