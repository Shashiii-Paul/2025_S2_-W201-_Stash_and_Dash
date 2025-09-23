using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float moveSpeed = 5f; 
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform cameraTransform; 
    [SerializeField] float mouseSensitivity = 100f; 

    private Vector3 _moveInput;
    private float xRotation = 0f; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(h, 0f, v).normalized;

        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); 
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        
        transform.Rotate(Vector3.up * mouseX);
    }

    private void FixedUpdate()
    {
        
        Vector3 moveDirection = (transform.forward * _moveInput.z + transform.right * _moveInput.x).normalized;
        Vector3 moveVelocity = moveDirection * moveSpeed;
        Vector3 newPosition = rb.position + moveVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        
        animator.SetFloat("MoveX", _moveInput.x);
        animator.SetFloat("MoveZ", _moveInput.z);
    }
}