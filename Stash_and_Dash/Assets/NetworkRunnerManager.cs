using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class NetworkRunnerManager : MonoBehaviour
{
    public static NetworkRunnerManager Instance;
    public NetworkRunner runnerPrefab;  // Drag a NetworkRunner prefab here

    public NetworkRunner Runner { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public async void StartRunner(GameMode mode, string sessionName)
{
    // If runner exists and running, shut it down
    if (Runner != null && Runner.IsRunning)
    {
        await Runner.Shutdown();
        Destroy(Runner.gameObject);
    }

    // Create new runner
    Runner = Instantiate(runnerPrefab);
    DontDestroyOnLoad(Runner.gameObject);

    var args = new StartGameArgs()
    {
        GameMode = mode,
        SessionName = sessionName,
        SceneManager = Runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
    };

    var result = await Runner.StartGame(args);
    if (result.Ok)
    {
        Debug.Log("Runner started successfully.");
        // Add username early for Lobby chat
        string username = PlayerPrefs.GetString("Username", "Unknown");
        PlayerDatabase.Instance.AddPlayer(Runner.LocalPlayer, username);
    }
    else
    {
        Debug.LogError("Failed to start runner. Shutdown Reason: " + result.ShutdownReason);
    }
}
}