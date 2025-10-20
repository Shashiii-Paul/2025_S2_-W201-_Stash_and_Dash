using UnityEngine;
using Alteruna;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : AttributesSync
{
    [SynchronizableField] private bool[] readyStates; // Tracks ready state for each player
    [SynchronizableField] private float gameStartTime = -1f; // Server time when game starts
    [SynchronizableField] private bool gameEnded = false; // Tracks if game has ended

    private Multiplayer multiplayer;
    private float gameDuration = 120f; // 2 minutes in seconds
    [SerializeField] private TextMeshProUGUI timerText; // UI for timer
    [SerializeField] private GameObject endgamePopup; // UI for endgame popup
    private int maxPlayers;

    void Start()
    {
        multiplayer = FindObjectOfType<Multiplayer>();
        if (multiplayer == null)
        {
            Debug.LogError("GameManager requires a Multiplayer component in the scene!");
            return;
        }

        // Initialize ready states based on max players in the room
        maxPlayers = multiplayer.CurrentRoom != null ? multiplayer.CurrentRoom.MaxUsers : 8; // Default to 8 if no room
        readyStates = new bool[maxPlayers];
        for (int i = 0; i < maxPlayers; i++)
        {
            readyStates[i] = false;
        }

        // Hide endgame popup initially
        if (endgamePopup != null)
        {
            endgamePopup.SetActive(false);
        }

        // Register to room join/leave events
        multiplayer.OnRoomJoined.AddListener(OnRoomJoined);
        multiplayer.OnOtherUserLeft.AddListener(OnUserLeft);
    }

    void Update()
    {
        if (multiplayer == null || !multiplayer.IsConnected) return;

        // Ready key input
        if (Input.GetKeyDown(KeyCode.R) && !gameEnded && gameStartTime < 0)
        {
            SetReady();
        }

        // Update timer UI
        if (gameStartTime >= 0 && !gameEnded)
        {
            float timeRemaining = gameDuration - (Time.time - gameStartTime);
            if (timeRemaining <= 0)
            {
                if (multiplayer.Me.Index == 0 && !gameEnded)
                {
                    // Host triggers endgame
                    gameEnded = true;
                    BroadcastRemoteMethod(nameof(EndGame));
                }
                timeRemaining = 0;
            }
            UpdateTimerUI(timeRemaining);
        }
    }

    public void SetReady()
    {
        if (multiplayer == null || !multiplayer.IsConnected) return;

        int userIndex = multiplayer.Me.Index;
        if (userIndex >= 0 && userIndex < readyStates.Length)
        {
            readyStates[userIndex] = true;
            BroadcastRemoteMethod(nameof(SyncReadyState), userIndex, true);
            Debug.Log($"Player {multiplayer.Me.Name} is ready.");

            // Host checks if all players are ready
            if (multiplayer.Me.Index == 0)
            {
                CheckAllReady();
            }
        }
    }

    [SynchronizableMethod]
    private void SyncReadyState(int userIndex, bool isReady)
    {
        if (userIndex >= 0 && userIndex < readyStates.Length)
        {
            readyStates[userIndex] = isReady;
            Debug.Log($"Player index {userIndex} ready state updated to {isReady}");
        }
    }

    private void CheckAllReady()
    {
        if (multiplayer.Me.Index != 0) return;

        int connectedUsers = multiplayer.CurrentRoom != null ? multiplayer.CurrentRoom.GetUserCount() : 1;
        for (int i = 0; i < connectedUsers; i++)
        {
            if (!readyStates[i])
            {
                return; // Not all players are ready
            }
        }

        // All players are ready, start the game
        gameStartTime = Time.time;
        BroadcastRemoteMethod(nameof(StartGame), gameStartTime);
        Debug.Log("All players ready. Game started!");
    }

    [SynchronizableMethod]
    private void StartGame(float startTime)
    {
        gameStartTime = startTime;
        Debug.Log($"Game started at time {startTime}");
    }

    [SynchronizableMethod]
    private void EndGame()
    {
        gameEnded = true;
        if (endgamePopup != null)
        {
            endgamePopup.SetActive(true);
        }
        Debug.Log("Game ended: Addicts Have Survived! Addicts Win!");

        // Start coroutine to kick players and close game
        StartCoroutine(CloseGameAfterDelay(5f));
    }

    private void OnRoomJoined(Multiplayer mp, Room room, User user)
    {
        // Reset ready state for new user
        if (user == multiplayer.Me)
        {
            int userIndex = user.Index;
            if (userIndex >= 0 && userIndex < readyStates.Length)
            {
                readyStates[userIndex] = false;
                BroadcastRemoteMethod(nameof(SyncReadyState), userIndex, false);
            }
        }
    }

    private void OnUserLeft(Multiplayer mp, User user)
    {
        // Reset ready state for leaving user
        int userIndex = user.Index;
        if (userIndex >= 0 && userIndex < readyStates.Length)
        {
            readyStates[userIndex] = false;
            BroadcastRemoteMethod(nameof(SyncReadyState), userIndex, false);
        }
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    private IEnumerator CloseGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (multiplayer.Me.Index == 0)
        {
            // Host kicks all players by leaving the room
            if (multiplayer.CurrentRoom != null)
            {
                multiplayer.CurrentRoom.Leave();
            }
        }
        else
        {
            // Clients leave the room
            if (multiplayer.CurrentRoom != null)
            {
                multiplayer.CurrentRoom.Leave();
            }
        }

        // Close the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}