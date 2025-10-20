using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Added for InputField reference

public class PlayerController : MonoBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    private Alteruna.Avatar _avatar;

    // New: Reference to the chat InputField (we'll find it at runtime since UI is in scene)
    private InputField chatInputField;

    // New: Track cursor state
    private bool cursorLocked = true;

    void Start()
    {
        _avatar = GetComponent<Alteruna.Avatar>();

        if (!_avatar.IsMe)
            return;
        
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
        playerCamera.transform.SetParent(transform);

        // New: Find the chat InputField in the scene (assumes there's only one; adjust if needed)
        chatInputField = FindObjectOfType<InputField>();
        if (chatInputField == null)
        {
            Debug.LogError("PlayerController: Could not find InputField for chat! Make sure it's in the scene.");
        }

        // Lock cursor initially
        UpdateCursorState();
    }

    void Update()
    {
        if (!_avatar.IsMe)
            return;
        
        // New: Toggle cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            UpdateCursorState();
        }

        // New: Press Enter to unlock (if locked) and focus chat input for typing
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (cursorLocked)
            {
                cursorLocked = false;
                UpdateCursorState();
                if (chatInputField != null)
                {
                    chatInputField.Select();
                    chatInputField.ActivateInputField();
                }
            }
            // If already unlocked, do nothing—Enter will send the message if the input is focused
        }
        
        bool isRunning = false;

        isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    // New: Helper to update cursor and movement state
    private void UpdateCursorState()
    {
        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canMove = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canMove = false;
        }
    }
}