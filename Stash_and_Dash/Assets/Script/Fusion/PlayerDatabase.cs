using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerDatabase : MonoBehaviour
{
    public static PlayerDatabase Instance;

    // Maps PlayerRef → username
    private Dictionary<PlayerRef, string> players = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void AddPlayer(PlayerRef playerRef, string username)
    {
        if (!players.ContainsKey(playerRef))
        {
            players.Add(playerRef, username);
            Debug.Log($"Player added: {username} [{playerRef}]");
        }
        else
        {
            Debug.LogWarning($"PlayerRef {playerRef} already has a username!");
        }
    }

    public string GetUsername(PlayerRef playerRef)
    {
        if (players.TryGetValue(playerRef, out string username))
            return username;
        return "Unknown";
    }
}
