using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    private Light flashlightLight; 
    private bool isFlashlightActive = false; 

    void Start()
    {
        
        flashlightLight = GetComponentInChildren<Light>();
        if (flashlightLight == null)
        {
            Debug.LogError("No Light component found in children of FlashLightOnPlayer!");
        }

       
        if (flashlightLight != null)
        {
            isFlashlightActive = gameObject.activeSelf;
            flashlightLight.enabled = isFlashlightActive;
        }
    }

    void Update()
    {
        
        if (gameObject.activeSelf && flashlightLight != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                
                isFlashlightActive = !isFlashlightActive;
                flashlightLight.enabled = isFlashlightActive;
                Debug.Log("Flashlight toggled: " + (isFlashlightActive ? "On" : "Off"));
            }
        }
    }
}