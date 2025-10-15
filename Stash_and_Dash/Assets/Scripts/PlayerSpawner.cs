using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement; // Add this line

public class PlayerSpawner : MonoBehaviour
{
    public NetworkPrefabRef addictPrefab;  // Drag Hunted prefab here
    public NetworkPrefabRef cartelPrefab;  // Drag new Cartel prefab (create if none)

    private void Start()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("PlayerSpawner: No NetworkRunner found in the scene!");
            return;
        }

        if (!runner.IsRunning)
        {
            Debug.LogWarning("PlayerSpawner: NetworkRunner is not running yet!");
            return;
        }

        if (SceneManager.GetActiveScene().name != "Environment")
        {
            Debug.LogWarning("PlayerSpawner: Not in Environment scene, skipping spawn.");
            return;
        }

        // Select prefab based on role (added)
        string role = PlayerPrefs.GetString("Role", "Addict");
        NetworkPrefabRef selectedPrefab = (role == "Cartel") ? cartelPrefab : addictPrefab;
        if (selectedPrefab == null) 
        { 
            Debug.LogError("No prefab for role: " + role); 
            return; 
        }

        Vector3 spawnPos = new Vector3(991, 0, 599);  // Warehouse coordinates
        Quaternion spawnRot = Quaternion.identity;     // Default rotation
        runner.Spawn(selectedPrefab, spawnPos, spawnRot, runner.LocalPlayer);
        Debug.Log("Spawned player at warehouse position: " + spawnPos);
    }
}