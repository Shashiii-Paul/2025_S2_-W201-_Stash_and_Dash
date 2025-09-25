using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SimpleNetworkRunnerStarter : MonoBehaviour
{
    public NetworkRunner runner;
    public NetworkPrefabRef playerPrefab;

    private async void Start()
    {
        if (runner == null)
            runner = GetComponent<NetworkRunner>();

        // Start the game as Host
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = "TestSession",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        });

        // Spawn local player
        if (playerPrefab.IsValid)
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, null);
    }
}
