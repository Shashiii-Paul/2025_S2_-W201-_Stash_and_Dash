using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance;

    [Header("UI References")]
    public Text messages;           // Assign in Inspector
    public InputField input;        // Assign in Inspector
    public ScrollRect scrollRect;   // Assign ScrollRect that contains the messages Text

    private bool chatFocused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Press Enter to focus the chat if not already focused
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!chatFocused)
            {
                FocusChat();
            }
            else
            {
                // If already focused, send the message
                SendMessage();
            }
        }

        // Click outside InputField to lose focus
        if (chatFocused && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject(input.gameObject))
                UnfocusChat();
        }
    }

    private void FocusChat()
    {
        input.ActivateInputField();
        chatFocused = true;
    }

    private void UnfocusChat()
    {
        EventSystem.current.SetSelectedGameObject(null);
        chatFocused = false;
    }

    private void SendMessage()
    {
        string msg = input.text.Trim();
        if (!string.IsNullOrEmpty(msg))
        {
            if (ChatNetwork.Instance != null)
                ChatNetwork.Instance.SendChat(msg);
            else
                Debug.LogWarning("[ChatUI] No ChatNetwork available.");

            input.text = "";
        }
        input.ActivateInputField(); // keep typing
    }

    public void DisplayMessage(string username, string message)
    {
        if (messages != null)
        {
            messages.text += $"{username}: {message}\n";

            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }

    // Checks if pointer is over the given UI object
    private bool IsPointerOverUIObject(GameObject uiObject)
    {
        return EventSystem.current.currentSelectedGameObject == uiObject;
    }
}
