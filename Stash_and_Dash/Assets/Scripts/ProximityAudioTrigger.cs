using UnityEngine;

public class ProximityAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource heartbeatAudio; // Assign in Inspector
    [SerializeField] private float proximityDistance = 10f; // Distance to trigger sound
    [SerializeField] private float maxVolume = 0.7f; // Max volume when hunter is close
    [SerializeField] private float minPitch = 0.8f; // Slower pitch when farther
    [SerializeField] private float maxPitch = 1.2f; // Faster pitch when closer

    private GameObject hunter; // Reference to the hunter GameObject

    void Start()
    {
        // Ensure this script only runs on the hunted player
        if (gameObject.tag != "Hunted")
        {
            enabled = false; // Disable script if not on hunted player
            return;
        }

        // Ensure AudioSource is assigned
        if (heartbeatAudio == null)
        {
            heartbeatAudio = GetComponent<AudioSource>();
        }

        // Find the hunter by tag
        hunter = GameObject.FindWithTag("Hunter");

        if (hunter == null)
        {
            Debug.LogError("Hunter GameObject not found! Ensure it has the 'Hunter' tag.");
        }

        // Stop audio initially
        heartbeatAudio.Stop();
    }
    
    void Update()
    {
        if (hunter == null) return; // Skip if hunter not found

        // Calculate distance to hunter
        float distance = Vector3.Distance(transform.position, hunter.transform.position);

        // Check if within proximity range
        if (distance <= proximityDistance)
        {
            // Start playing if not already
            if (!heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Play();
            }

            // Adjust volume and pitch based on distance
            float t = 1f - (distance / proximityDistance); // 0 (far) to 1 (close)
            heartbeatAudio.volume = Mathf.Lerp(0f, maxVolume, t);
            heartbeatAudio.pitch = Mathf.Lerp(minPitch, maxPitch, t);
        }
        else
        {
            // Stop audio if hunter is too far
            if (heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Stop();
            }
        }
    }
}