using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 300f;
    [SerializeField] private Transform playerBody;
    [SerializeField] private PlayerInput playerInput;

    [Header("Mobile Camera Settings")]
    [SerializeField] private float mobileSensitivity = 0.5f;
    [SerializeField] private float smoothing = 5f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float minVerticalAngle = -80f;

    private float verticalRotation = 0f;
    private Vector2 smoothedDelta = Vector2.zero;
    private bool isUsingTouch = false;
    private Vector2 lastTouchPosition;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Handle touch input for camera rotation
        HandleTouchInput();

        // Handle mouse input when touch is not active and on non-mobile platforms
        if (!isUsingTouch && !Application.isMobilePlatform)
        {
            float mouseX = playerInput.MousePosition.x * sensitivity * Time.deltaTime;
            float mouseY = playerInput.MousePosition.y * sensitivity * Time.deltaTime;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

            transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            // Looking is controlled by touching the right half of the screen
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // Only use touch on the right side for camera control
                if (touch.position.x > Screen.width * 0.5f)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        isUsingTouch = true;
                        lastTouchPosition = touch.position;
                    }
                    else if (touch.phase == TouchPhase.Moved && isUsingTouch)
                    {
                        Vector2 touchDelta = touch.position - lastTouchPosition;
                        smoothedDelta = Vector2.Lerp(smoothedDelta, touchDelta, Time.deltaTime * smoothing);
                        lastTouchPosition = touch.position;

                        float horizontalRotation = smoothedDelta.x * mobileSensitivity;
                        playerBody.Rotate(Vector3.up * horizontalRotation);

                        float verticalRotationChange = -smoothedDelta.y * mobileSensitivity;
                        verticalRotation = Mathf.Clamp(verticalRotation + verticalRotationChange, minVerticalAngle, maxVerticalAngle);
                        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        isUsingTouch = false;
                        smoothedDelta = Vector2.zero;
                    }
                    break; // Break after processing the first valid touch on the right side
                }
            }
        }
        else
        {
            isUsingTouch = false;
        }
    }

    public void ApplyHeadBob(float speed, float amplitude)
    {
        if (speed > 0.1f)
        {
            float bobAmount = Mathf.Sin(Time.time * 10f) * amplitude * speed;
            Vector3 localPos = transform.localPosition;
            localPos.y = bobAmount + 0.7f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPos, Time.deltaTime * 5f);
        }
        else
        {
            Vector3 localPos = transform.localPosition;
            localPos.y = 0.7f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPos, Time.deltaTime * 5f);
        }
    }
}