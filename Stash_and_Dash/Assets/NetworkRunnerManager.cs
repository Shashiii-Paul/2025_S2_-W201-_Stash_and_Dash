using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class NetworkRunnerManager : MonoBehaviour
{
    public NetworkRunner runner;
    public NetworkPrefabRef playerPrefab;
    public NetworkPrefabRef playerDatabasePrefab; // NEW

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

        // Spawn PlayerDatabase only once by the Host
        if (runner.IsServer && playerDatabasePrefab.IsValid)
        {
            runner.Spawn(playerDatabasePrefab, Vector3.zero, Quaternion.identity, null);
        }

        // Spawn local player
        if (playerPrefab.IsValid)
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
    }
}
