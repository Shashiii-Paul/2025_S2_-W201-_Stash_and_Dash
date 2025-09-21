using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class MultiplayerScript : NetworkBehaviour
{
    public Text messages;   // The text box displaying chat
    public InputField input; // The input field for typing
    public string username = "default";

    // Queue for message if NetworkBehaviour not ready
    private string queuedMessage = null;

    // Called by UI button
    public void CallMessagedRPC()
    {
        string message = input.text;
        if (string.IsNullOrEmpty(message))
            return;

        // If NetworkBehaviour isn't ready, queue the message
        if (Runner == null || !Object.HasStateAuthority)
        {
            queuedMessage = message;
            Debug.Log("Network not ready yet, message queued.");
            input.text = "";
            return;
        }

        // Otherwise, send immediately
        RPC_SendMessage(username, message);
        input.text = "";
    }

    // This is called on all clients
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(string username, string message, RpcInfo info = default)
    {
        if (messages != null)
        {
            messages.text += $"{username}: {message}\n";
        }
        else
        {
            Debug.LogWarning("Messages Text reference is missing!");
        }
    }

    // Called automatically when this NetworkBehaviour is spawned
    public override void Spawned()
    {
        base.Spawned();

        // If there’s a queued message, send it now
        if (!string.IsNullOrEmpty(queuedMessage))
        {
            RPC_SendMessage(username, queuedMessage);
            queuedMessage = null;
        }
    }
}