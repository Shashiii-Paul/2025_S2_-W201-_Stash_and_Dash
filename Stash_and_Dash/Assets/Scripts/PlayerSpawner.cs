using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement; // Add this line

public class PlayerSpawner : MonoBehaviour
{
    public NetworkPrefabRef playerPrefab;  // Drag PlayerPrefab here

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

        if (playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner: PlayerPrefab is not assigned in the Inspector!");
            return;
        }

        if (SceneManager.GetActiveScene().name != "Environment")
        {
            Debug.LogWarning("PlayerSpawner: Not in Environment scene, skipping spawn.");
            return;
        }

        Vector3 spawnPos = new Vector3(991, 0, 599);  // Warehouse coordinates
        Quaternion spawnRot = Quaternion.identity;     // Default rotation
        runner.Spawn(playerPrefab, spawnPos, spawnRot, runner.LocalPlayer);
        Debug.Log("Spawned player at warehouse position: " + spawnPos);
    }
}