using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance;

    public Text messages;      // Assign in Inspector
    public InputField input;   // Assign in Inspector
    public Button sendButton;  // Assign in Inspector

    private void Awake()
    {
        Instance = this;
        sendButton.onClick.AddListener(OnSendClicked);
    }

    private void Start()
    {
        // Optional: wait until ChatNetwork is ready
        if (ChatNetwork.Instance == null)
            Debug.LogWarning("[ChatUI] ChatNetwork not found; messages will not send.");
    }

    public void OnSendClicked()
    {
        string msg = input.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        if (ChatNetwork.Instance != null)
            ChatNetwork.Instance.SendChat(msg);
        else
            Debug.LogWarning("[ChatUI] No ChatNetwork available.");

        input.text = "";
        input.ActivateInputField(); // Keep focus
    }

    public void DisplayMessage(string username, string message)
    {
        if (messages != null)
            messages.text += $"{username}: {message}\n";
    }
}
