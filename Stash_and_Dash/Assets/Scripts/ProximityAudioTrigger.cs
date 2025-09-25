using UnityEngine;

public class ProximityAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource heartbeatAudio; 
    [SerializeField] private float proximityDistance = 10f; 
    [SerializeField] private float maxVolume = 0.7f; 
    [SerializeField] private float minPitch = 0.8f; 
    [SerializeField] private float maxPitch = 1.2f; 

    private GameObject hunter; 

    void Start()
    {
        
        if (gameObject.tag != "Hunted")
        {
            enabled = false; 
            return;
        }

        
        if (heartbeatAudio == null)
        {
            heartbeatAudio = GetComponent<AudioSource>();
        }

        
        hunter = GameObject.FindWithTag("Hunter");

        if (hunter == null)
        {
            Debug.LogError("Hunter GameObject not found! Ensure it has the 'Hunter' tag.");
        }

        
        heartbeatAudio.Stop();
    }

    void Update()
    {
        if (hunter == null) return; 

        
        float distance = Vector3.Distance(transform.position, hunter.transform.position);

        
        if (distance <= proximityDistance)
        {
           
            if (!heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Play();
            }

            
            float t = 1f - (distance / proximityDistance); 
            heartbeatAudio.volume = Mathf.Lerp(0f, maxVolume, t);
            heartbeatAudio.pitch = Mathf.Lerp(minPitch, maxPitch, t);
        }
        else
        {
            
            if (heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Stop();
            }
        }
    }
}
