using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerDatabase : NetworkBehaviour
{
    public static PlayerDatabase Instance;

    // Store all players
    public static Dictionary<PlayerRef, string> Players = new Dictionary<PlayerRef, string>();

    public override void Spawned()
    {
        // Only one instance on the server
        if (Runner.IsServer)
        {
            Instance = this;
            Debug.Log("PlayerDatabase is ready");
        }
    }

    // RPC called by clients, received by server
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddPlayer(string username, RpcInfo info = default)
    {
        if (!Players.ContainsKey(info.Source))
        {
            Players.Add(info.Source, username);
            Debug.Log($"Player added: {username} ({info.Source})");
        }
    }
}
