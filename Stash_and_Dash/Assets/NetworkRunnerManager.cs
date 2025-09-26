using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class NetworkRunnerManager : MonoBehaviour
{
    public static NetworkRunnerManager Instance;

    public NetworkRunner runner;
    public NetworkPrefabRef playerPrefab;
    public NetworkPrefabRef playerDatabasePrefab;
    public NetworkPrefabRef chatNetworkPrefab; // Add this in inspector

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (runner == null)
            runner = GetComponent<NetworkRunner>();

        // Start as Host
        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = "TestSession",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        });

        // Spawn PlayerDatabase once
        if (runner.IsServer && playerDatabasePrefab.IsValid && PlayerDatabase.Instance == null)
            runner.Spawn(playerDatabasePrefab, Vector3.zero, Quaternion.identity, null);

        // Spawn local player
        if (playerPrefab.IsValid)
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

        // Spawn ChatNetwork once
        if (runner.IsServer && chatNetworkPrefab.IsValid && ChatNetwork.Instance == null)
            runner.Spawn(chatNetworkPrefab, Vector3.zero, Quaternion.identity, null);

        Debug.Log("[NetworkRunnerManager] Setup complete.");
    }
}