using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;

    [Space]
    [SerializeField]
    private float playerSpeed = 5.0f, playerRunSpeed = 8;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float flySpeed = 2;

    private Vector3 playerVelocity;

    [Header("Grounded check parameters:")]
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private float rayDistance = 1;
    [field: SerializeField]
    public bool IsGrounded { get; private set; }

    [Header("Mobile Input")]
    [SerializeField]
    private FixedJoystick joystick;
    [SerializeField]
    private Button jumpButton;

    [Header("Camera")]
    [SerializeField]
    private Transform mainCamera;
    [SerializeField]
    private float lookSpeed = 200f;

    private bool isLooking = false;
    private Vector2 lastTouchPosition;

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
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 inputDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        if (inputDirection.magnitude < 0.1f) return Vector3.zero;

        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        return cameraRight * inputDirection.x + cameraForward * inputDirection.z;
    }

    public void Fly(bool ascendInput, bool descendInput)
    {
        Vector3 movementDirection = GetMovementDirection();

        if (ascendInput)
            movementDirection += Vector3.up * flySpeed;
        else if (descendInput)
            movementDirection -= Vector3.up * flySpeed;

        controller.Move(movementDirection * playerSpeed * Time.deltaTime);
    }

    public void Walk(bool runningInput)
    {
        Vector3 movementDirection = GetMovementDirection();
        float speed = runningInput ? playerRunSpeed : playerSpeed;

        controller.Move(movementDirection * Time.deltaTime * speed);
    }

    public void HandleGravity(bool isJumping)
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        if (isJumping && IsGrounded)
            AddJumpForce();
        ApplyGravityForce();
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void AddJumpForce()
    {
        playerVelocity.y = jumpHeight;
    }

    private void ApplyGravityForce()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        playerVelocity.y = Mathf.Clamp(playerVelocity.y, gravityValue, 10);
    }

    private void FixedUpdate()
    {
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down * rayDistance);
    }

    private void Update()
    {
        HandleCameraLook();
    }

    private void HandleCameraLook()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isLooking = true;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && isLooking)
            {
                Vector2 touchDelta = touch.position - lastTouchPosition;
                lastTouchPosition = touch.position;

                // Rotate the camera and player horizontally
                float horizontalRotation = touchDelta.x * lookSpeed * Time.deltaTime;
                transform.Rotate(0, horizontalRotation, 0);

                // Rotate the camera vertically (Look Up/Down)
                float verticalRotation = -touchDelta.y * lookSpeed * Time.deltaTime;
                Vector3 currentAngles = mainCamera.localEulerAngles;
                currentAngles.x = Mathf.Clamp(currentAngles.x + verticalRotation, -80f, 80f);
                mainCamera.localEulerAngles = currentAngles;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isLooking = false;
            }
        }
    }
}
