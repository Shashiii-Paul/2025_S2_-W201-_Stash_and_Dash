using UnityEngine;

public class PickUpFlashlight : MonoBehaviour
{
    public GameObject PickUpText; 

    void Start()
    {
        if (PickUpText != null)
            PickUpText.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Hunted")) 
        {
            if (PickUpText != null)
                PickUpText.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E)) 
            {
                
                GameObject flashLightOnPlayer = other.gameObject.transform.Find("Flashlight (1)")?.gameObject;

                if (flashLightOnPlayer != null)
                {
                    flashLightOnPlayer.SetActive(true); 
                    this.gameObject.SetActive(false); 
                    if (PickUpText != null)
                        PickUpText.SetActive(false); 
                }
                else
                {
                    Debug.LogWarning("Flashlight not found on player: " + other.gameObject.name);
                }
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