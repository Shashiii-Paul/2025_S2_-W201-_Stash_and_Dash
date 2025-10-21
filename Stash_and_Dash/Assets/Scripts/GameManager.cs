using UnityEngine;
using Alteruna;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : AttributesSync
{
    [SynchronizableField] private float gameStartTimeOffset = -1f; // Host's Time.time when game starts
    [SynchronizableField] private bool gameEnded = false; // Tracks if game has ended
    [SynchronizableField] private float syncedRemainingTime = -1f; // Synced remaining time for UI

    private Multiplayer multiplayer;
    private float gameDuration = 120f; // 2 minutes in seconds
    [SerializeField] private TextMeshProUGUI timerText; // UI for timer
    [SerializeField] private GameObject endgamePopup; // UI for endgame popup
    private float localGameStartTime; // Local time when game starts (used only by host)

    void Start()
    {
        multiplayer = FindObjectOfType<Multiplayer>();
        if (multiplayer == null)
        {
            Debug.LogError("GameManager requires a Multiplayer component in the scene!");
            return;
        }

        // Hide endgame popup initially
        if (endgamePopup != null)
        {
            endgamePopup.SetActive(false);
        }
    }

    void Update()
    {
        if (multiplayer == null || !multiplayer.IsConnected) return;

        // Host starts the game with R key
        if (Input.GetKeyDown(KeyCode.R) && multiplayer.Me.Index == 0 && !gameEnded && gameStartTimeOffset < 0)
        {
            StartTheGame();
        }

        // Update timer UI if game has started
        if (gameStartTimeOffset >= 0 && !gameEnded)
        {
            UpdateTimerUI(syncedRemainingTime);

            // Host checks for endgame (backup check in case coroutine misses)
            if (multiplayer.Me.Index == 0 && syncedRemainingTime <= 0 && !gameEnded)
            {
                gameEnded = true;
                BroadcastRemoteMethod(nameof(EndGame));
            }
        }
    }

    private void StartTheGame()
    {
        if (multiplayer.Me.Index != 0) return;

        gameStartTimeOffset = Time.time;
        localGameStartTime = Time.time;
        syncedRemainingTime = gameDuration;
        Debug.Log("Hunter started the game!");
        BroadcastRemoteMethod(nameof(StartGame));
        StartCoroutine(SyncTimerCoroutine());
    }

    [SynchronizableMethod]
    private void StartGame()
    {
        // Clients start their local timer display with full duration initially
        syncedRemainingTime = gameDuration;
        Debug.Log("Game started for this client.");
    }

    [SynchronizableMethod]
    private void EndGame()
    {
        gameEnded = true;
        syncedRemainingTime = 0f;
        if (endgamePopup != null)
        {
            endgamePopup.SetActive(true);
        }
        Debug.Log("Game ended: Addicts Have Survived! Addicts Win!");

        // Start coroutine to kick players and close game
        StartCoroutine(CloseGameAfterDelay(5f));
    }

    private IEnumerator SyncTimerCoroutine()
    {
        while (!gameEnded)
        {
            float timeSinceStart = Time.time - localGameStartTime;
            syncedRemainingTime = Mathf.Max(0f, gameDuration - timeSinceStart);

            if (syncedRemainingTime <= 0 && !gameEnded)
            {
                gameEnded = true;
                BroadcastRemoteMethod(nameof(EndGame));
                yield break;
            }

            yield return new WaitForSeconds(0.5f); // Sync every 0.5 seconds to reduce network load
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