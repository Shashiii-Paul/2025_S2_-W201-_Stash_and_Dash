using UnityEngine;
using TMPro; // If using TextMeshPro
using Fusion;

public class LobbyPlayerName : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText; // Assign in Inspector

    private async void Start()
    {
        // Wait until PlayerDatabase exists
        while (PlayerDatabase.Instance == null)
            await System.Threading.Tasks.Task.Yield();

        // Wait until the local player is valid
        NetworkRunner runner = null;
        while (runner == null || runner.LocalPlayer == null)
        {
            runner = Object.FindFirstObjectByType<NetworkRunner>();
            await System.Threading.Tasks.Task.Yield();
        }

        var localPlayer = runner.LocalPlayer;
        string username = PlayerDatabase.Instance.GetUsername(localPlayer);

        if (usernameText != null)
            usernameText.text = "Display Name: "+username;
        else
            Debug.LogWarning("Username text component not assigned!");
    }
}
