using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fusion;
using TMPro;

public class UsernameUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public NetworkRunner networkRunner;

    public void SubmitUsername()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            Debug.LogWarning("Username cannot be empty!");
            return;
        }

        if (PlayerDatabase.Instance == null)
        {
            Debug.LogError("PlayerDatabase instance not found!");
            return;
        }

        // Get the local player's PlayerRef
        PlayerRef localPlayer = networkRunner.LocalPlayer;

        // Add to PlayerDatabase
        PlayerDatabase.Instance.AddPlayer(localPlayer, usernameInput.text);

        // Load Chat Scene
        SceneManager.LoadScene("chatFusionScene");
    }
}
