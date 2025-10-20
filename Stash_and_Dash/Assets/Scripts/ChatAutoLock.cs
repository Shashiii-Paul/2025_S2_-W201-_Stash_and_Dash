using UnityEngine;
using UnityEngine.UI;
using Alteruna;

public class ChatAutoLock : MonoBehaviour
{
    [SerializeField] private InputField chatInput;
    private TextChatSynchronizable textChat;

    void Start()
    {
        if (chatInput == null)
        {
            chatInput = GetComponentInChildren<InputField>();
        }
        chatInput.onEndEdit.AddListener(OnChatSubmit);
        textChat = FindObjectOfType<TextChatSynchronizable>();
    }

    private void OnChatSubmit(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            var avatar = FindObjectOfType<Alteruna.Avatar>();
            if (avatar != null && avatar.IsMe)
            {
                var playerController = avatar.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // Lock cursor and enable movement
                    playerController.GetType().GetField("cursorLocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(playerController, true);
                    playerController.GetType().GetMethod("UpdateCursorState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(playerController, null);
                }
                if (textChat != null)
                {
                    textChat.SendChatMessage(message);
                }
            }
            chatInput.text = "";
            chatInput.ActivateInputField(); // Keep input active for continuous typing
        }
    }
}