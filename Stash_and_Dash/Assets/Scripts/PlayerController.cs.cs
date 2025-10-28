using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;      // Required for InputField
using Alteruna;           // Required for Avatar, multiplayer checks

public class PlayerController : MonoBehaviour
{

    // [Header("Base setup")]
    // Walking speed of the player (units per second)
    [Header("Base setup")]
    [SerializeField] private float walkingSpeed = 7.5f;
    // Running speed of the player (units per second)
    [SerializeField] private float runningSpeed = 11.5f;
    // Vertical speed applied when the player jumps
    [SerializeField] private float jumpSpeed = 8.0f;
    // Downward acceleration applied when the player is in the air
    [SerializeField] private float gravity = 20.0f;
    // Mouse look sensitivity
    [SerializeField] private float lookSpeed = 2.0f;
    // Vertical rotation limit for the camera (in degrees)
    [SerializeField] private float lookXLimit = 45.0f;

    // CharacterController used for movement and collision handling
    private CharacterController characterController;
    // Movement vector for this player
    private Vector3 moveDirection = Vector3.zero;
    // Track camera pitch rotation
    private float rotationX = 0f;

    // Hide-in-inspector flag controlling whether player can move
    [HideInInspector] public bool canMove = true;

    // Camera vertical offset
    [SerializeField] private float cameraYOffset = 0.4f;
    // Cached reference to the main camera
    private Camera playerCamera;

    // Avatar reference to detect local player (Alteruna)
    private Alteruna.Avatar _avatar;

    // Reference to the chat InputField (we'll find it at runtime since UI is in scene)
    private InputField chatInputField;

    // Track cursor state (true when locked and gameplay controls enabled)
    private bool cursorLocked = true;


    // Called once on object creation
    void Start()
    {
        // Get the Alteruna avatar component (used to determine local player)
        _avatar = GetComponent<Alteruna.Avatar>();

        // If this is not the local player's avatar, stop initialisation early
        if (_avatar == null || !_avatar.IsMe)
            return;

        // Cache CharacterController component (required for movement)
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            // Defensive: warn and disable script to avoid null-reference exceptions during Update.
            Debug.LogError("PlayerController: CharacterController component missing. Disabling PlayerController.");
            enabled = false;
            return;
        }

        // Cache main camera once for performance
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            // Defensive: warn and continue (movement will work but there will be no player camera)
            Debug.LogWarning("PlayerController: Main Camera not found in scene. Player view will not function correctly.");
        }
        else
        {
            // Position the camera above the player and parent it so it follows
            playerCamera.transform.position = new Vector3(
                transform.position.x,
                transform.position.y + cameraYOffset,
                transform.position.z
            );
            playerCamera.transform.SetParent(transform);
        }

        // Find the chat InputField in the scene (assumes one exists).
        chatInputField = FindObjectOfType<InputField>();
        if (chatInputField == null)
        {
            // Error message that chat won't work
            Debug.LogError("PlayerController: Could not find InputField for chat! Make sure it's in the scene.");
        }

        // Lock cursor initially
        UpdateCursorState();
    }

    // Called every frame
    void Update()
    {
        // Only run for the local player's avatar
        if (_avatar == null || !_avatar.IsMe)
            return;

        // Toggle cursor lock/unlock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            UpdateCursorState();
        }

        // Press Enter to unlock (if locked) and focus chat input for typing
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (cursorLocked)
            {
                // If currently locked, unlock and focus chat
                cursorLocked = false;
                UpdateCursorState();
                if (chatInputField != null)
                {
                    // Focus the UI InputField for typing
                    chatInputField.Select();
                    chatInputField.ActivateInputField();
                }
            }
            // If already unlocked, the InputField handles "Enter sends message" behavior as usual.
        }

        // Defensive: ensure CharacterController exists before movement calculations
        if (characterController == null)
            return;

        // Handle movement and rotation split into helper methods (readability + reuse)
        HandleMovement();   // Process keyboard input and move the player
        HandleRotation();   // Rotate the camera and player based on mouse input
    }


    // Handles player walking, running, jumping, and gravity
    private void HandleMovement()
    {
        // Determine if running (Left Shift held)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Local forward and right vectors relative to player orientation
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Get input values from Unity's input manager (WASD / arrow keys)
        float forwardAxis = Input.GetAxis("Vertical");   // forward/backwards
        float sideAxis = Input.GetAxis("Horizontal");    // left/right

        // Compute current speeds on each axis, respecting canMove
        float curSpeedForward = canMove ? ((isRunning ? runningSpeed : walkingSpeed) * forwardAxis) : 0f;
        float curSpeedSide = canMove ? ((isRunning ? runningSpeed : walkingSpeed) * sideAxis) : 0f;

        // Preserve vertical momentum before we overwrite moveDirection
        float movementDirectionY = moveDirection.y;

        // Combine directional movement from inputs
        moveDirection = (forward * curSpeedForward) + (right * curSpeedSide);

        // Jumping: only when grounded, canMove is true and Jump button pressed 
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            // Preserve previous Y (so gravity or previous velocity remains)
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity if not grounded 
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move using CharacterController (frame-rate independent)
        characterController.Move(moveDirection * Time.deltaTime);
    }

    // Handles rotation of both the player and the camera 
    private void HandleRotation()
    {
        // If movement/input is disabled or camera missing, skip rotation
        if (!canMove || playerCamera == null)
            return;

        // Vertical rotation (look up/down) — invert Y so mouse up = look up
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;

        // Clamp vertical rotation to prevent camera flipping
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // Apply vertical rotation to the camera's local rotation (so camera pitches independently)
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Horizontal rotation (yaw): rotate the player object itself
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    // Helper to update cursor lock state and movement enable flag.
    // When cursor is locked the player can move,
    // when unlocked (for UI/chat) the player cannot move and cursor is visible.
    private void UpdateCursorState()
    {
        if (cursorLocked)
        {
            // Lock and hide cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canMove = true;
        }
        else
        {
            // Unlock and show cursor for UI/chat interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canMove = false;
        }
    }

}
