using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Movement: WASD for forward/back/left/right, Space to ascend, LeftShift to descend
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveY = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
        moveY *= moveSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveX, moveY, moveZ));

        // Rotation: Mouse look
        float rotX = Input.GetAxis("Mouse X") * lookSpeed;
        float rotY = -Input.GetAxis("Mouse Y") * lookSpeed;
        transform.Rotate(new Vector3(rotY, rotX, 0));
    }
}