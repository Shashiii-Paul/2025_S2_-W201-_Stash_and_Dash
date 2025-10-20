using UnityEngine;
using Alteruna;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject hunterPrefab; // Assign HunterPlayer prefab
    [SerializeField] private GameObject huntedPrefab; // Assign HuntedPlayer prefab
    [SerializeField] private Transform spawnPoint; // Reference to SpawnPoint GameObject
    [SerializeField] private int hunterIndex = 0; // Spawner index for HunterPlayer
    [SerializeField] private int huntedIndex = 1; // Spawner index for HuntedPlayer

    private Multiplayer multiplayer;
    private Spawner spawner;

    void Start()
    {
        multiplayer = GetComponent<Multiplayer>();
        spawner = GetComponent<Spawner>();
        if (multiplayer == null)
        {
            Debug.LogError("PlayerSpawner requires a Multiplayer component on the same GameObject!");
            return;
        }
        if (spawner == null)
        {
            Debug.LogError("PlayerSpawner requires a Spawner component on the same GameObject!");
            return;
        }

        // Register to OnRoomJoined to spawn the player
        multiplayer.OnRoomJoined.AddListener(OnRoomJoined);
    }

    private void OnRoomJoined(Multiplayer mp, Room room, User user)
    {
        if (user != multiplayer.Me) return; // Only spawn for the local player

        // Determine prefab and index based on user index
        bool isHunter = user.Index == 0;
        int spawnIndex = isHunter ? hunterIndex : huntedIndex;

        // Get spawn position and rotation from SpawnPoint
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Spawn the player using Spawner
        GameObject player = spawner.Spawn(spawnIndex, spawnPosition, spawnRotation, Vector3.one);
        Debug.Log($"Spawned {(isHunter ? "Hunter" : "Hunted")} for user {user.Name} at {spawnPosition}");

        // Ensure the Avatar component is set up correctly
        Alteruna.Avatar avatar = player.GetComponent<Alteruna.Avatar>();
        if (avatar == null)
        {
            Debug.LogError("Spawned player prefab does not have an Avatar component!");
        }
    }
}