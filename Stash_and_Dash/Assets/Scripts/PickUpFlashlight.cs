using UnityEngine;

public class PickUpFlashlight : MonoBehaviour
{
    public GameObject FlashLightOnPlayer; 
    public GameObject PickUpText; 

    void Start()
    {
        if (FlashLightOnPlayer != null)
            FlashLightOnPlayer.SetActive(false);
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
                this.gameObject.SetActive(false); 
                if (FlashLightOnPlayer != null)
                    FlashLightOnPlayer.SetActive(true); 
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