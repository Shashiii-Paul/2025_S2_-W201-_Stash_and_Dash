using Fusion;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance;
    private NetworkRunner _runner;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // stays alive between scenes
        } else {
            Destroy(gameObject);
        }
    }

    public async void StartGame(GameMode mode, string lobbyName) {
        if (_runner == null) {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        await _runner.StartGame(new StartGameArgs() {
            GameMode = mode,
            SessionName = lobbyName,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public NetworkRunner Runner => _runner;
}
