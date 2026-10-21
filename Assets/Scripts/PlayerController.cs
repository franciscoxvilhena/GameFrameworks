using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool faceMoveDirection;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float attackCooldown = 0.6f;

    private Animator animator;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isRunning;
    private float nextAttackTime;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        UpdateAnimator();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    public void Run(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.performed || Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        animator.SetTrigger("Attack");
    }
    void FixedUpdate()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * moveInput.y + right * moveInput.x;
        
        float speed = isRunning ? runSpeed : moveSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * moveSpeed;
        velocity.z = movement.z * moveSpeed;

        rb.linearVelocity = velocity;

        if (faceMoveDirection && movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement,Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        //Convert movement input into a speed value (0 when idle, >0 when moving)
        float currentSpeed = new Vector3(moveInput.x, 0f, moveInput.y).magnitude;

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("Run", isRunning && currentSpeed > 0f);
    }
}
