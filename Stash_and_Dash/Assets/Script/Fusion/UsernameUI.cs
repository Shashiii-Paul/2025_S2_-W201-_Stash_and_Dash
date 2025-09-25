using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;

public class UsernameUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public Button submitButton;

    private void Start()
    {
        submitButton.onClick.AddListener(SubmitUsername);
    }

public void SubmitUsername()
{
    string username = usernameInput.text;

    if (string.IsNullOrEmpty(username))
    {
        Debug.LogWarning("Username cannot be empty!");
        return;
    }

    if (PlayerDatabase.Instance != null)
    {
        PlayerDatabase.Instance.RPC_AddPlayer(username);

        // Load the next scene after adding the player
        //SceneManager.LoadScene("chatFusionScene"); // Replace with your scene name
    }
    else
    {
        Debug.LogError("PlayerDatabase instance not found!");
    }
}

}
