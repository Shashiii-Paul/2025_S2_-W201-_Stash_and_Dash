using UnityEngine;

public class PickUpFlashlight : MonoBehaviour
{
    public GameObject FlashLightOnPlayer; // Child flashlight under Player > Head
    public GameObject PickUpText; // UI Text for "Press E"

    void Start()
    {
        if (FlashLightOnPlayer != null)
            FlashLightOnPlayer.SetActive(false);
        if (PickUpText != null)
            PickUpText.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Hunted")) // Changed from "Player" to "Hunted"
        {
            if (PickUpText != null)
                PickUpText.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E)) // Changed to GetKeyDown for single press
            {
                this.gameObject.SetActive(false); // Disable world flashlight
                if (FlashLightOnPlayer != null)
                    FlashLightOnPlayer.SetActive(true); // Enable player's flashlight
                if (PickUpText != null)
                    PickUpText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hunted"))
        {
            if (PickUpText != null)
                PickUpText.SetActive(false);
        }
    }
}