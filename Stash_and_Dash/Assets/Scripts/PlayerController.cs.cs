using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // -------------------- CONFIGURABLE MOVEMENT SETTINGS --------------------//

    // Walking speed of the player (units per second)
    [Header("Base Setup")]
    [SerializeField] private float WalkingSpeed = 7.5f;

    // Running speed of the player (units per second)
    [SerializeField] private float RunningSpeed = 11.5f;

    // Vertical speed applied when the player jumps
    [SerializeField] private float JumpSpeed = 8.0f;

    // Downward acceleration applied when the player is in the air
    [SerializeField] private float GravityForce = 20.0f;

    // Mouse look sensitivity
    [SerializeField] private float LookSpeed = 2.0f;

    // Vertical rotation limit for the camera (in degrees)
    [SerializeField] private float LookXLimit = 45.0f;

    // Vertical offset for positioning the camera above the player
    [SerializeField] private float CameraYOffset = 0.4f;

    // -------------------- PRIVATE COMPONENT REFERENCES --------------------//

    private CharacterController _characterController;   // Handles movement and collisions
    private Camera _playerCamera;                       // Player's camera
    private Alteruna.Avatar _avatar;                    // Used to check if this player is the local one

    // -------------------- MOVEMENT VARIABLES --------------------//

    private Vector3 _moveDirection = Vector3.zero;      // Stores the current movement vector
    private float _rotationX = 0f;                      // Tracks camera pitch rotation
    [HideInInspector] public bool CanMove = true;       // Controls whether the player can move

    // Called once at the start of the game
    // Sets up all references and locks the mouse cursor
    private void Start()
    {
        _avatar = GetComponent<Alteruna.Avatar>();

        // Make sure this is the local player
        if (_avatar == null || !_avatar.IsMe)
            return;

        // Get the CharacterController component
        _characterController = GetComponent<CharacterController>();

        // If missing, log a warning and disable this script
        if (_characterController == null)
        {
            Debug.LogWarning("CharacterController component missing on PlayerController object.");
            enabled = false;
            return;
        }

        // Cache the main camera for performance
        _playerCamera = Camera.main;

        // Warn if there is no main camera in the scene
        if (_playerCamera == null)
        {
            Debug.LogWarning("Main Camera not found in scene. Player view will not function.");
            return;
        }

        // Position the camera above the player and make it follow
        _playerCamera.transform.position = new Vector3(
            transform.position.x,
            transform.position.y + CameraYOffset,
            transform.position.z
        );
        _playerCamera.transform.SetParent(transform);

        // Lock and hide the cursor so mouse movement rotates the camera
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called once every frame
    // Handles player movement and rotation
    private void Update()
    {
        // Only run for the local player
        if (_avatar == null || !_avatar.IsMe)
            return;

        // Stop if no CharacterController is attached
        if (_characterController == null)
            return;

        HandleMovement();  // Process keyboard input and move the player
        HandleRotation();  // Rotate the camera and player based on mouse movement
    }

    // Handles player walking, running, jumping, and gravity
    private void HandleMovement()
    {
        // Check if Left Shift is being held down for running
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Get forward and right directions relative to the player's current facing
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Get input from Unity's Input system (WASD or arrow keys)
        float forwardInput = Input.GetAxis("Vertical");
        float sideInput = Input.GetAxis("Horizontal");

        // Determine speeds for both directions depending on whether the player is running
        float currentForwardSpeed = CanMove ? (isRunning ? RunningSpeed : WalkingSpeed) * forwardInput : 0;
        float currentSideSpeed = CanMove ? (isRunning ? RunningSpeed : WalkingSpeed) * sideInput : 0;

        // Store the Y value before applying jump or gravity (so it carries over properly)
        float previousY = _moveDirection.y;

        // Combine forward and sideways movement
        _moveDirection = (forward * currentForwardSpeed) + (right * currentSideSpeed);

        // Jump logic — only works when grounded
        if (CanMove && _characterController.isGrounded && Input.GetButton("Jump"))
        {
            _moveDirection.y = JumpSpeed;
        }
        else
        {
            // Keep existing Y value (e.g. for falling)
            _moveDirection.y = previousY;
        }

        // Apply gravity if the player is not grounded
        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= GravityForce * Time.deltaTime;
        }

        // Move the player based on calculated direction and speed
        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    // Handles rotation of both the player and the camera
    private void HandleRotation()
    {
        // If movement is disabled or no camera found, skip rotation
        if (!CanMove || _playerCamera == null)
            return;

        // Vertical rotation (looking up and down)
        _rotationX += -Input.GetAxis("Mouse Y") * LookSpeed;

        // Clamp the rotation so the camera doesn’t flip over
        _rotationX = Mathf.Clamp(_rotationX, -LookXLimit, LookXLimit);

        // Apply vertical rotation to the camera
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        // Horizontal rotation (turning left and right)
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * LookSpeed, 0);
    }
}
