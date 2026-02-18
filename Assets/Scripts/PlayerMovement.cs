using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("2D Plane Transform")]
    public Transform planeTransform;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check Settings")]
    public float groundDistance;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Transform myTransform;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myTransform = GetComponent<Transform>();
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        // copies y rotation from planeTransform
        Vector3 euler = transform.eulerAngles;
        euler.y = planeTransform.eulerAngles.y;
        transform.eulerAngles = euler;

        float movement = moveInput * moveSpeed;
        
        Vector3 moveDirection = transform.right * movement;

        rb.linearVelocity = new Vector3(
            moveDirection.x,
            rb.linearVelocity.y,
            moveDirection.z
        );
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveDirection = context.ReadValue<Vector2>();
        moveInput = moveDirection.x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        isGrounded = Physics.CheckSphere(
            myTransform.position,
            groundDistance, 
            groundLayer
        );
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

}
