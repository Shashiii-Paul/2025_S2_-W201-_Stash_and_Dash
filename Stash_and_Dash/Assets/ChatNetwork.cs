using Fusion;
using UnityEngine;

public class ChatNetwork : NetworkBehaviour
{
    public static ChatNetwork Instance { get; private set; }

    private NetworkRunner MyRunner => Runner; // Use the inherited Runner property

    public override void Spawned()
    {
        base.Spawned();

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <-- keep across scenes
        }
        else if (Instance != this)
        {
            Runner.Despawn(Object); // Remove duplicate if exists
        }
    }

    /// <summary>
    /// Called by UI when the local player sends a chat message.
    /// </summary>
    public void SendChat(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (MyRunner == null)
        {
            Debug.LogWarning("[Chat] Runner not available yet!");
            return;
        }

        PlayerRef localPlayer = MyRunner.LocalPlayer;

        string username = "Unknown";
        if (PlayerDatabase.Instance != null)
            username = PlayerDatabase.Instance.GetUsername(localPlayer);

        Debug.Log($"[Chat] Sending message from {username}: {message}");
        RPC_SendMessage(username, message);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SendMessage(string username, string message, RpcInfo info = default)
    {
        if (ChatUI.Instance != null)
            ChatUI.Instance.DisplayMessage(username, message);
        else
            Debug.LogWarning("[Chat] ChatUI.Instance missing; message not displayed!");
    }
}
