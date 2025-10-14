using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float moveSpeed = 10f; // Increased for better responsiveness
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float mouseSensitivity = 200f; // Increased for better control

    private Vector3 _moveInput;
    private float xRotation = 0f; // Pitch (up/down)
    private float yRotation = 0f; // Yaw (left/right)

    public override void Spawned()
    {
        base.Spawned();

        if (Object.HasInputAuthority)
        {
            string username = PlayerPrefs.GetString("Username", "Unknown");
            PlayerDatabase.Instance.AddPlayer(Runner.LocalPlayer, username);

            // Enable camera and lock cursor for local player
            if (cameraTransform != null)
            {
                Camera cam = cameraTransform.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = true;
                    cam.targetDisplay = 0;
                }
                AudioListener listener = cameraTransform.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Local player spawned with first-person camera enabled.");
        }
        else
        {
            if (cameraTransform != null)
                cameraTransform.gameObject.SetActive(false);
            Debug.Log("Remote player spawned (no camera).");
        }
    }

    void Update()
    {
        if (!Object.HasInputAuthority || (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null))
            return;

        // Handle mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX; // Yaw (horizontal)
        xRotation -= mouseY; // Pitch (vertical)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limit vertical look

        // Apply rotations
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f); // Rotate body horizontally
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate camera vertically

        // Handle movement input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(h, 0f, v).normalized;
    }

    private void FixedUpdate()
    {
        if (!Object.HasInputAuthority) return;

        // Move the player using Rigidbody
        Vector3 moveDirection = (transform.forward * _moveInput.z + transform.right * _moveInput.x).normalized;
        Vector3 moveVelocity = moveDirection * moveSpeed;
        Vector3 newPosition = rb.position + moveVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Update animator if present
        if (animator != null)
        {
            animator.SetFloat("MoveX", _moveInput.x);
            animator.SetFloat("MoveZ", _moveInput.z);
        }
    }
}