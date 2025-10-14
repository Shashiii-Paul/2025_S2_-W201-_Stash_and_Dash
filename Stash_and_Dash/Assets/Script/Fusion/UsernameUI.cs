using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fusion;
using TMPro;

public class UsernameUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public NetworkRunner networkRunner;
    public Button hostButton;
    public Button joinButton;
    public TMP_InputField sessionInput;

    public void SubmitUsername()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            Debug.LogWarning("Username cannot be empty!");
            return;
        }

        // Store username locally 
        PlayerPrefs.SetString("Username", usernameInput.text);  // Save to local storage

        // Hide username input
        usernameInput.gameObject.SetActive(false);

        // Show host/join buttons
        if (hostButton != null) hostButton.gameObject.SetActive(true);
        if (joinButton != null) joinButton.gameObject.SetActive(true);
        if (sessionInput != null) sessionInput.gameObject.SetActive(true);
    }

    public void StartHost()
    {
        StartGame(GameMode.Host);
    }

    public void StartClient()
    {
        StartGame(GameMode.Client);
    }

    private async void StartGame(GameMode mode)
    {
        if (sessionInput != null && !string.IsNullOrEmpty(sessionInput.text))
        {
            sessionName = sessionInput.text;
        }

        var args = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()  
        };

        await networkRunner.StartGame(args);

        // Load lobby after starting network
        SceneManager.LoadScene("Lobby");

        // Hide buttons
        if (hostButton != null) hostButton.gameObject.SetActive(false);
        if (joinButton != null) joinButton.gameObject.SetActive(false);
        if (sessionInput != null) sessionInput.gameObject.SetActive(false);
    }

    private string sessionName = "TestRoom";  // Hardcoded session name
}