using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class MultiplayerScript : NetworkBehaviour
{
    public Text messages;   // UI text for chat
    public InputField input;

    private string queuedMessage = null;

    private NetworkRunner MyRunner => NetworkRunnerManager.Instance.Runner;
    private PlayerRef localPlayer => MyRunner.LocalPlayer;

    // Optional: queue message before object is fully initialized
    public override void Spawned()
    {
        base.Spawned();

        if (!string.IsNullOrEmpty(queuedMessage))
        {
            SendChat(queuedMessage);
            queuedMessage = null;
        }
    }

    public void CallMessageRPC()
    {
        string message = input.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        if (Runner == null || localPlayer == default)
        {
            queuedMessage = message;
            input.text = "";
            return;
        }

        SendChat(message);
        input.text = "";
    }

    private void SendChat(string message)
    {
        if (PlayerDatabase.Instance == null)
        {
            Debug.LogWarning("PlayerDatabase missing!");
            return;
        }

        string username = PlayerDatabase.Instance.GetUsername(localPlayer);
        if (string.IsNullOrEmpty(username)) username = "Unknown";

        RPC_SendMessage(username, message);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(string username, string message, RpcInfo info = default)
    {
        if (messages != null)
        {
            messages.text += $"{username}: {message}\n";
        }
    }
}