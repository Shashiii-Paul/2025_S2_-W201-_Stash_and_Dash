using UnityEngine;

public class ChatPopupSpawner : MonoBehaviour
{
    public GameObject chatPrefab; // assign prefab in inspector

    private GameObject chatInstance;

    private void Awake()
    {
        if (chatInstance == null)
        {
            chatInstance = Instantiate(chatPrefab);
            DontDestroyOnLoad(chatInstance); // optional
        }
    }

    public void ToggleChat()
    {
        chatInstance.SetActive(!chatInstance.activeSelf);
    }
}
