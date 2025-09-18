using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float moveSpeed;
    [SerializeField] Rigidbody rb;
    Vector3 _moveInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(h, 0f, v).normalized;
    }

    private void FixedUpdate()
    {
        Vector3 moveVelocity = _moveInput * moveSpeed;
        Vector3 newPosition = rb.position + moveVelocity * Time.deltaTime;
        rb.MovePosition(newPosition);
        animator.SetFloat("MoveX", _moveInput.x);
        animator.SetFloat("MoveZ", _moveInput.z); 
    }

}
