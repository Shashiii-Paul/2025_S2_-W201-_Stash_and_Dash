using Fusion;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    public string username;
    public PlayerRef playerRef;

    public PlayerInfo(string name, PlayerRef pref)
    {
        username = name;
        playerRef = pref;
    }
}
