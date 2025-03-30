using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float playerRunSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float flySpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;

    private Vector3 playerVelocity;
    private Vector3 currentMovement;
    private float currentSpeed;

    [Header("Grounded check parameters:")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayDistance = 1f;
    [field: SerializeField] public bool IsGrounded { get; private set; }

    [Header("Mobile Input")]
    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private Button jumpButton;

    [Header("Camera")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private float headBobAmplitude = 0.05f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (joystick == null)
            joystick = FindObjectOfType<FixedJoystick>();

        if (jumpButton == null)
            jumpButton = GameObject.Find("JumpButton")?.GetComponent<Button>();

        if (jumpButton != null)
            jumpButton.onClick.AddListener(() => HandleGravity(true));

        if (mainCamera == null)
            mainCamera = Camera.main.transform;

        if (playerCamera == null)
            playerCamera = mainCamera.GetComponent<PlayerCamera>();
    }

    private Vector3 GetMovementDirection()
    {
        // Get raw joystick input
        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        // Apply deadzone for better control
        if (input.magnitude < 0.1f)
            return Vector3.zero;

        // Normalize input if it exceeds 1 (for diagonal movement)
        if (input.magnitude > 1f)
            input = input.normalized;

        // Convert joystick input to world space direction relative to camera
        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;

        // Remove vertical component for horizontal movement only
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        // Normalize to ensure consistent speed in all directions
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate final movement direction
        return cameraRight * input.x + cameraForward * input.y;
    }

    public void Fly(bool ascendInput, bool descendInput)
    {
        Vector3 targetDirection = GetMovementDirection();

        // Add vertical movement for flying
        if (ascendInput)
            targetDirection += Vector3.up;
        else if (descendInput)
            targetDirection -= Vector3.up;

        // Smooth acceleration for more natural movement
        currentMovement = Vector3.Lerp(currentMovement, targetDirection * playerSpeed, Time.deltaTime * acceleration);

        // Apply movement
        controller.Move(currentMovement * Time.deltaTime);
    }

    public void Walk(bool runningInput)
    {
        Vector3 targetDirection = GetMovementDirection();
        float targetSpeed = runningInput ? playerRunSpeed : playerSpeed;

        // Calculate smooth acceleration or deceleration
        if (targetDirection.magnitude > 0.1f)
        {
            // Accelerate when moving
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
        }
        else
        {
            // Decelerate when stopping
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * deceleration);
        }

        // Apply movement
        Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
        controller.Move(movement);

        // Apply head bobbing effect if moving and on ground
        if (IsGrounded && playerCamera != null)
        {
            playerCamera.ApplyHeadBob(movement.magnitude, headBobAmplitude);
        }
    }

    public void HandleGravity(bool isJumping)
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -0.5f; // Small downward force to keep grounded
        }

        if (isJumping && IsGrounded)
            AddJumpForce();

        ApplyGravityForce();
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void AddJumpForce()
    {
        // Calculate jump force based on gravity and desired height
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
    }

    private void ApplyGravityForce()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;

        // Limit terminal velocity
        float terminalVelocity = gravityValue * 2;
        playerVelocity.y = Mathf.Max(playerVelocity.y, terminalVelocity);
    }

    private void FixedUpdate()
    {
        // Check if player is grounded
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * rayDistance);
    }
}