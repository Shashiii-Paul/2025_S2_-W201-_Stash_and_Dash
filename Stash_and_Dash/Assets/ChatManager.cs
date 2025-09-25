using Fusion;
using Fusion.Sockets;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Header("UI References")]
    public TMP_InputField chatInput;
    public Button sendButton;
    public Text chatLog;   // Reference to your ScrollRect

    private NetworkRunner _runner;

    [System.Obsolete]
    async void Start()
    {
        sendButton.onClick.AddListener(SendChatMessage);

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = false;

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "ChatRoom",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        _runner.AddCallbacks(this);
    }

    [System.Obsolete]
    void SendChatMessage()
    {
        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            RPC_SendChat(chatInput.text.Trim());
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendChat(string message, RpcInfo info = default)
    {
        string playerName = $"Player {info.Source}";
        OnReceiveChat($"{playerName}: {message}");
    }

    public void OnReceiveChat(string message)
    {
        chatLog.text += "\n" + message;

        // Force the scroll to the bottom after Unity updates UI
        Canvas.ForceUpdateCanvases();
        Canvas.ForceUpdateCanvases();
    }

    // ===== INetworkRunnerCallbacks (empty) =====
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
