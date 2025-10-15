using UnityEngine;
using UnityEngine.UI;
using Fusion;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public Button addictButton;  // Drag "Play as Addict" button here in Inspector
    public Button cartelButton;  // Drag "Play as Cartel" button here
    public TextMeshProUGUI statusText;  // Optional: Drag a Text object for messages

    private NetworkRunner runner;
    private bool isCartelTaken = false;  // Track if Cartel chosen (networked later)

    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        if (runner == null) Debug.LogError("No NetworkRunner!");

        if (addictButton != null) addictButton.onClick.AddListener(ChooseAddict);
        if (cartelButton != null) cartelButton.onClick.AddListener(ChooseCartel);

        // For now, no lock—add networked check later
    }

    void ChooseAddict()
    {
        PlayerPrefs.SetString("Role", "Addict");  // Store locally
        LoadEnvironment();
    }

    void ChooseCartel()
    {
        if (isCartelTaken)
        {
            if (statusText != null) statusText.text = "Cartel locked!";
            return;
        }
        PlayerPrefs.SetString("Role", "Cartel");
        isCartelTaken = true;  // Lock (network this for multiplayer)
        LoadEnvironment();
    }

    void LoadEnvironment()
    {
        SceneManager.LoadScene("Environment");
    }
}